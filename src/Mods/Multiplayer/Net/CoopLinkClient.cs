using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace DiceMultiplayer.Net
{
    /// <summary>
    /// Client de l'API locale de CoopLink (réseaux d'amis sécurisés, sans serveur ; voir github.com/Reloadaxe/coop-link).
    /// Une connexion persistante à 127.0.0.1:47811, authentifiée par le jeton de %APPDATA%\CoopLink\api.token.
    /// Toutes les requêtes passent par un fil d'arrière-plan : l'interface lit l'état mis en cache (Networks, Sessions).
    /// Fermer le client retire les parties publiées et les ports ouverts (côté CoopLink).
    /// </summary>
    public sealed class CoopLinkClient : System.IDisposable
    {
        public const int ApiPort = 47811;
        public const string GameId = "die-in-the-dungeon";

        public sealed class Member { public string name { get; set; } public string friendCode { get; set; } public bool online { get; set; } }
        public sealed class Network { public string id { get; set; } public string name { get; set; } public List<Member> members { get; set; } }
        public sealed class Session { public string id { get; set; } public string game { get; set; } public string title { get; set; } public string network { get; set; } public string networkName { get; set; } public string hostName { get; set; } public bool mine { get; set; } }

        private sealed class Msg
        {
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string cmd { get; set; }
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string token { get; set; }
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string game { get; set; }
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string title { get; set; }
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string network { get; set; }
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)] public int port { get; set; }
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string session { get; set; }
            public bool ok { get; set; }
            public string error { get; set; }
            public string name { get; set; }
            public List<Network> networks { get; set; }
            public List<Session> sessions { get; set; }
        }

        public volatile bool Connected;
        public volatile string Error;
        public volatile string PlayerName;
        public volatile List<Network> Networks = new List<Network>();
        public volatile List<Session> Sessions = new List<Session>();

        private TcpClient _tcp;
        private StreamReader _r;
        private StreamWriter _w;
        private readonly SemaphoreSlim _io = new SemaphoreSlim(1, 1);
        private volatile bool _disposed;
        private DateTime _nextRefresh;

        public static string TokenPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CoopLink", "api.token");

        /// <summary>Connexion (en arrière-plan) puis rafraîchissement régulier de l'état et des parties.</summary>
        public void Start() => Task.Run(Loop);

        private async Task Loop()
        {
            while (!_disposed)
            {
                try
                {
                    if (!Connected) await ConnectAsync();
                    if (Connected && DateTime.UtcNow >= _nextRefresh)
                    {
                        _nextRefresh = DateTime.UtcNow.AddSeconds(2);
                        var st = await CallAsync(new Msg { cmd = "status" });
                        if (st?.ok == true) { Networks = st.networks ?? new List<Network>(); PlayerName = st.name; }
                        var ss = await CallAsync(new Msg { cmd = "sessions", game = GameId });
                        if (ss?.ok == true) Sessions = ss.sessions ?? new List<Session>();
                    }
                }
                catch (Exception e) { Drop(e.Message); }
                await Task.Delay(Connected ? 500 : 3000);
            }
        }

        private async Task ConnectAsync()
        {
            if (!File.Exists(TokenPath)) { Error = "CoopLink n'est pas installé ou jamais lancé"; return; }
            var tcp = new TcpClient();
            try
            {
                var connect = tcp.ConnectAsync("127.0.0.1", ApiPort);
                if (await Task.WhenAny(connect, Task.Delay(1500)) != connect || !tcp.Connected) { tcp.Close(); Error = "CoopLink n'est pas lancé"; return; }
            }
            catch { tcp.Close(); Error = "CoopLink n'est pas lancé"; return; }
            _tcp = tcp;
            var s = tcp.GetStream();
            _r = new StreamReader(s, new UTF8Encoding(false));
            _w = new StreamWriter(s, new UTF8Encoding(false)) { AutoFlush = true, NewLine = "\n" };
            var auth = await CallAsync(new Msg { cmd = "auth", token = File.ReadAllText(TokenPath).Trim() });
            if (auth?.ok != true) { Drop("CoopLink a refusé la connexion"); return; }
            PlayerName = auth.name;
            Connected = true;
            Error = null;
            _nextRefresh = DateTime.MinValue;
        }

        private async Task<Msg> CallAsync(Msg req)
        {
            await _io.WaitAsync();
            try
            {
                if (_w == null) return null;
                await _w.WriteLineAsync(JsonSerializer.Serialize(req));
                var line = await _r.ReadLineAsync();
                if (line == null) throw new IOException("CoopLink fermé");
                return JsonSerializer.Deserialize<Msg>(line);
            }
            finally { _io.Release(); }
        }

        private void Drop(string why)
        {
            Connected = false;
            Error = why;
            try { _tcp?.Close(); } catch { }
            _tcp = null; _w = null; _r = null;
            Networks = new List<Network>();
            Sessions = new List<Session>();
        }

        /// <summary>Publie ma partie (port local du jeu) sur un réseau. Rappel avec l'id de la partie ou une erreur.</summary>
        public void Publish(string networkId, string title, int port, Action<string, string> done)
        {
            Task.Run(async () =>
            {
                try
                {
                    var r = await CallAsync(new Msg { cmd = "publish", game = GameId, title = title, network = networkId, port = port });
                    done(r?.ok == true ? r.session : null, r?.ok == true ? null : r?.error ?? "CoopLink injoignable");
                    _nextRefresh = DateTime.MinValue;
                }
                catch (Exception e) { done(null, e.Message); }
            });
        }

        public void Unpublish()
        {
            Task.Run(async () => { try { await CallAsync(new Msg { cmd = "unpublish" }); } catch { } });
        }

        /// <summary>Demande un port local relié à la partie d'un ami. Rappel avec le port (0 si échec) et l'erreur éventuelle.</summary>
        public void Connect(string sessionId, Action<int, string> done)
        {
            Task.Run(async () =>
            {
                try
                {
                    var r = await CallAsync(new Msg { cmd = "connect", session = sessionId });
                    done(r?.ok == true ? r.port : 0, r?.ok == true ? null : r?.error ?? "CoopLink injoignable");
                }
                catch (Exception e) { done(0, e.Message); }
            });
        }

        /// <summary>Lance CoopLink (installé avec le mod dans le dossier du jeu) s'il n'est pas déjà lancé.</summary>
        public static bool Launch(string gameDir)
        {
            try
            {
                var exe = Path.Combine(gameDir, "CoopLink", "CoopLink.exe");
                if (!File.Exists(exe)) return false;
                Process.Start(new ProcessStartInfo(exe, "--tray") { UseShellExecute = true, WorkingDirectory = Path.GetDirectoryName(exe) });
                return true;
            }
            catch { return false; }
        }

        public void Dispose()
        {
            _disposed = true;
            Drop("fermé");
        }
    }
}
