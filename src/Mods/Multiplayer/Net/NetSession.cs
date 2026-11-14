using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace DiceMultiplayer.Net
{
    /// <summary>
    /// Couche réseau TCP. Topologie en étoile : l'hôte relaie tout ce qu'il reçoit.
    /// TCP garantit l'ordre sur chaque connexion et l'hôte traite les messages un par un,
    /// donc tous les clients voient les actions dans le même ordre.
    /// Les threads réseau ne font que lire et écrire. Tout le traitement se fait dans <see cref="Pump"/>,
    /// sur le thread Unity.
    /// </summary>
    public sealed class NetSession : System.IDisposable
    {
        public const string ProtocolVersion = "dim-5";
        public const int MaxPlayers = 3;
        private static readonly string[] Colors = { "#4FC3F7", "#FFB74D", "#AED581", "#F06292" };

        private sealed class Conn
        {
            public int Id;
            public TcpClient Tcp;
            public StreamWriter Writer;
            public readonly object WriteLock = new object();
            public string Name;
            public bool Greeted;
        }

        public bool IsHost { get; }
        public int LocalId { get; private set; } = -1;
        public bool Connected => IsHost || (_server?.Tcp?.Connected ?? false) && LocalId >= 0;
        public string Status { get; private set; } = "";
        public List<PlayerInfo> Players { get; private set; } = new List<PlayerInfo>();

        /// <summary>Message reçu (déjà relayé si on est l'hôte). Appelé sur le thread Unity.</summary>
        public event Action<Msg> OnMessage;
        public event Action OnPlayersChanged;
        public event Action<string> OnDisconnected;

        private readonly ConcurrentQueue<(Conn from, string line)> _inbox = new ConcurrentQueue<(Conn, string)>();
        private readonly ConcurrentQueue<string> _events = new ConcurrentQueue<string>();
        private TcpListener _listener;
        private readonly List<Conn> _clients = new List<Conn>();
        private Conn _server;
        private int _nextId = 1;
        private volatile bool _disposed;
        private readonly string _localName;

        private NetSession(bool host, string localName)
        {
            IsHost = host;
            _localName = localName;
        }

        // ───────────── Création ─────────────

        public static NetSession Host(int port, string name)
        {
            var s = new NetSession(true, name);
            s._listener = new TcpListener(IPAddress.Any, port);
            s._listener.Start();
            s.LocalId = 0;
            s.Players.Add(new PlayerInfo { Id = 0, Name = name, Color = Colors[0] });
            s.Status = $"Hôte sur le port {port}";
            new Thread(s.AcceptLoop) { IsBackground = true, Name = "DIM-Accept" }.Start();
            return s;
        }

        public static NetSession Join(string host, int port, string name)
        {
            var s = new NetSession(false, name);
            s.Status = $"Connexion à {host}:{port}…";
            new Thread(() =>
            {
                try
                {
                    var tcp = new TcpClient { NoDelay = true };
                    tcp.Connect(host, port);
                    var c = new Conn { Id = 0, Tcp = tcp, Writer = new StreamWriter(tcp.GetStream(), new UTF8Encoding(false)) { AutoFlush = true } };
                    s._server = c;
                    s.SendTo(c, new Msg { t = T.Hello, name = name, ver = ProtocolVersion });
                    s.ReadLoop(c);
                }
                catch (Exception e) { s._events.Enqueue("Connexion impossible : " + e.Message); }
            }) { IsBackground = true, Name = "DIM-Client" }.Start();
            return s;
        }

        // ───────────── Envoi ─────────────

        /// <summary>
        /// Envoie un message à tout le monde sauf soi. Côté client, il part à l'hôte, qui le relaie.
        /// Le champ id est rempli automatiquement.
        /// </summary>
        public void Broadcast(Msg m)
        {
            m.id = LocalId;
            if (IsHost) { foreach (var c in Snapshot()) SendTo(c, m); }
            else if (_server != null) SendTo(_server, m);
        }

        /// <summary>Hôte uniquement : envoie un message à tous les clients sauf <paramref name="exceptId"/>.</summary>
        private void Relay(Msg m, int exceptId)
        {
            foreach (var c in Snapshot()) if (c.Id != exceptId) SendTo(c, m);
        }

        private List<Conn> Snapshot() { lock (_clients) return _clients.Where(c => c.Greeted).ToList(); }

        private void SendTo(Conn c, Msg m)
        {
            try
            {
                var line = m.Serialize();
                lock (c.WriteLock) c.Writer.WriteLine(line);
            }
            catch (Exception e) { _events.Enqueue($"Envoi à {c.Name ?? "?"} impossible : {e.Message}"); Drop(c); }
        }

        // ───────────── Threads ─────────────

        private void AcceptLoop()
        {
            while (!_disposed)
            {
                try
                {
                    var tcp = _listener.AcceptTcpClient();
                    tcp.NoDelay = true;
                    var c = new Conn { Tcp = tcp, Writer = new StreamWriter(tcp.GetStream(), new UTF8Encoding(false)) { AutoFlush = true } };
                    lock (_clients) { c.Id = _nextId++; _clients.Add(c); }
                    new Thread(() => ReadLoop(c)) { IsBackground = true, Name = "DIM-Read-" + c.Id }.Start();
                }
                catch { if (!_disposed) Thread.Sleep(200); }
            }
        }

        private void ReadLoop(Conn c)
        {
            try
            {
                using var reader = new StreamReader(c.Tcp.GetStream(), new UTF8Encoding(false));
                string line;
                while (!_disposed && (line = reader.ReadLine()) != null)
                    if (line.Length > 0) _inbox.Enqueue((c, line));
            }
            catch { }
            if (!_disposed) _inbox.Enqueue((c, null)); // null = déconnexion
        }

        // ───────────── Traitement (thread Unity) ─────────────

        public void Pump()
        {
            while (_events.TryDequeue(out var ev))
            {
                Status = ev;
                if (!IsHost) OnDisconnected?.Invoke(ev);
            }

            int budget = 500;
            while (budget-- > 0 && _inbox.TryDequeue(out var item))
            {
                var (from, line) = item;
                if (line == null) { HandleDisconnect(from); continue; }

                Msg m;
                try { m = Msg.Parse(line); }
                catch { continue; }
                if (m?.t == null) continue;

                if (IsHost) HostHandle(from, m);
                else ClientHandle(m);
            }
        }

        private void HostHandle(Conn from, Msg m)
        {
            if (!from.Greeted)
            {
                if (m.t != T.Hello) return;
                if (m.ver != ProtocolVersion) { SendTo(from, new Msg { t = T.Reject, text = $"Version incompatible ({m.ver} ≠ {ProtocolVersion})" }); Drop(from); return; }
                if (Players.Count >= MaxPlayers) { SendTo(from, new Msg { t = T.Reject, text = "Partie pleine" }); Drop(from); return; }
                if (Game_InRun()) { SendTo(from, new Msg { t = T.Reject, text = "Une run est déjà en cours : rejoins avant le lancement." }); Drop(from); return; }

                from.Name = string.IsNullOrWhiteSpace(m.name) ? "Joueur " + from.Id : m.name.Trim();
                from.Greeted = true;
                Players.Add(new PlayerInfo { Id = from.Id, Name = from.Name, Color = Colors[Players.Count % Colors.Length] });
                SendTo(from, new Msg { t = T.Welcome, id = from.Id, players = Players });
                PublishPlayers();
                return;
            }

            m.id = from.Id; // l'hôte fait foi sur l'identité de l'expéditeur
            if (m.t == T.Ping) return;
            Relay(m, from.Id);
            OnMessage?.Invoke(m);
        }

        private void ClientHandle(Msg m)
        {
            switch (m.t)
            {
                case T.Welcome:
                    LocalId = m.id;
                    Players = m.players ?? new List<PlayerInfo>();
                    Status = "Connecté";
                    OnPlayersChanged?.Invoke();
                    return;
                case T.Reject:
                    Status = "Refusé : " + m.text;
                    OnDisconnected?.Invoke(Status);
                    return;
                case T.Players:
                    Players = m.players ?? new List<PlayerInfo>();
                    OnPlayersChanged?.Invoke();
                    return;
            }
            OnMessage?.Invoke(m);
        }

        public void PublishPlayers()
        {
            if (!IsHost) return;
            foreach (var c in Snapshot()) SendTo(c, new Msg { t = T.Players, players = Players });
            OnPlayersChanged?.Invoke();
        }

        private void HandleDisconnect(Conn c)
        {
            if (IsHost)
            {
                bool wasPlayer;
                lock (_clients) { wasPlayer = _clients.Remove(c) && c.Greeted; }
                try { c.Tcp.Close(); } catch { }
                if (wasPlayer)
                {
                    Players.RemoveAll(p => p.Id == c.Id);
                    PublishPlayers();
                    OnMessage?.Invoke(new Msg { t = T.Chat, id = -1, text = $"{c.Name} s'est déconnecté." });
                }
            }
            else
            {
                Status = "Déconnecté de l'hôte";
                OnDisconnected?.Invoke(Status);
            }
        }

        private void Drop(Conn c) { try { c.Tcp.Close(); } catch { } }

        // Un mod ne peut pas référencer le jeu depuis un thread réseau : on vérifie seulement sur le thread Unity (Pump)
        private static bool Game_InRun() => DiceLoader.Game.InRun;

        public PlayerInfo GetPlayer(int id) => Players.FirstOrDefault(p => p.Id == id);

        public void Dispose()
        {
            _disposed = true;
            try { _listener?.Stop(); } catch { }
            lock (_clients) { foreach (var c in _clients) try { c.Tcp.Close(); } catch { } _clients.Clear(); }
            try { _server?.Tcp?.Close(); } catch { }
        }
    }
}
