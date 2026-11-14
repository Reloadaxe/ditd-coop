using System;
using System.Linq;
using BepInEx.Configuration;
using DiceLoader;
using DiceMultiplayer.Net;
using DiceMultiplayer.Sync;
using UnityEngine;

namespace DiceMultiplayer
{
    /// <summary>
    /// Mod multijoueur coop (2 ou 3 joueurs) en TCP direct.
    /// Tout le monde joue la même run, synchronisée en lockstep sur les entrées. Voir Sync/Coop.cs.
    /// </summary>
    [DiceModInfo("dicemodding.multiplayer", "Multijoueur Coop", "0.1.0",
        Author = "DiceModding", Description = "Coop à 2 ou 3 joueurs : même run, dés répartis, curseurs partagés (TCP).")]
    public class MultiplayerMod : DiceMod
    {
        public static MultiplayerMod Instance;

        private ConfigEntry<string> _name, _host, _localOnly;
        private ConfigEntry<int> _port;
        private ConfigEntry<bool> _rotateLeader, _showCursors, _showOwnership;
        private string _chat = "";
        private Vector2 _mouse;
        private float _nextCursor;
        private Vector2 _logScroll;

        public override void OnLoad()
        {
            Instance = this;
            _name = Config.Bind("Réseau", "Pseudo", Environment.UserName, "Ton nom affiché aux autres joueurs");
            _host = Config.Bind("Réseau", "DernierHote", "127.0.0.1", "Adresse IP de l'hôte (LAN, Radmin VPN, ZeroTier, Tailscale…)");
            _port = Config.Bind("Réseau", "Port", 27015, "Port TCP");
            _rotateLeader = Config.Bind("Règles", "MeneurTournant", false, "Le meneur (carte, récompenses, boutique) change à chaque étage");
            _showCursors = Config.Bind("Affichage", "Curseurs", true, "Afficher les curseurs des autres joueurs");
            _showOwnership = Config.Bind("Affichage", "ProprietaireDes", true, "Afficher le nom du propriétaire sous chaque dé de la main");
            _localOnly = Config.Bind("Règles", "UILocale", "Settings,Pause,Backpack,Codex,Feedback,Tooltip,Glossary,Options",
                "Mots-clés de chemins d'UI dont les clics restent locaux (non synchronisés), séparés par des virgules");

            Coop.RotateLeader = () => _rotateLeader.Value;
            Coop.LocalOnlyKeywords = () => _localOnly.Value.Split(',');
        }

        public override void OnEnable() => Patches.Apply(Harmony);

        public override void OnDisable() => Coop.Detach();

        // ───────────── Boucle ─────────────

        public override void OnUpdate()
        {
            var net = Coop.Net;
            if (net == null) return;
            net.Pump();

            if (Coop.Active && Time.unscaledTime >= _nextCursor)
            {
                _nextCursor = Time.unscaledTime + 1f / 15f;
                Coop.Send(new Msg
                {
                    t = T.Cursor,
                    x = _mouse.x / Math.Max(1, Screen.width),
                    y = _mouse.y / Math.Max(1, Screen.height),
                    scene = Game.State.ToString(),
                });
            }
        }

        public override void OnFloorStart(FloorSystem floor) => Coop.OnFloorStarted();

        public override void OnTurnStart(BattleSystem battle, int turn)
        {
            if (!Coop.Active) return;
            // Empreinte prise un peu après le début du tour, une fois la pioche terminée
            Game.Schedule(() =>
            {
                var (h, detail) = StateHash.Compute();
                Coop.SubmitHash(turn + 1000 * FloorKey(), h, detail);
            }, 2.5f);
        }

        private static int _floorKey;
        public override void OnFloorEnd(FloorSystem floor, bool success) => _floorKey++;
        private static int FloorKey() => _floorKey;

        public override void OnRunStart(CurrentRunData run, bool loaded) { _floorKey = 0; Coop.DesyncWarning = null; }

        // ───────────── Overlay ─────────────

        public override void OnGUI()
        {
            var ev = Event.current;
            if (ev != null) _mouse = new Vector2(ev.mousePosition.x, Screen.height - ev.mousePosition.y);

            if (!Coop.Active) return;
            if (ev == null || ev.type != EventType.Repaint) return;

            // Liste des joueurs (en haut à droite)
            float w = 240, x = Screen.width - w - 10, y = 10;
            var players = Coop.Net.Players.OrderBy(p => p.Id).ToList();
            GUI.Box(new Rect(x, y, w, 26 + players.Count * 20 + (Game.InBattle ? 20 : 0)), "");
            GUI.Label(new Rect(x + 8, y + 4, w, 20), $"<b>Coop</b> - meneur : <color={Coop.ColorOf(Coop.LeaderId)}>{Coop.NameOf(Coop.LeaderId)}</color>");
            y += 24;
            foreach (var p in players)
            {
                string ready = Game.InBattle ? (Coop.ReadyPlayers.Contains(p.Id) ? "  ✔ prêt" : "  … joue") : "";
                string me = p.Id == Coop.LocalId ? " (toi)" : "";
                GUI.Label(new Rect(x + 8, y, w, 20), $"<color={p.Color}>●</color> {p.Name}{me}{ready}");
                y += 20;
            }
            if (Game.InBattle) GUI.Label(new Rect(x + 8, y, w, 20), "<i>Fin de tour : tout le monde doit valider</i>");

            // Curseurs des autres joueurs
            if (_showCursors.Value)
            {
                string scene = Game.State.ToString();
                foreach (var kv in Coop.Cursors)
                {
                    if (kv.Key == Coop.LocalId || kv.Value.scene != scene || Time.unscaledTime - kv.Value.time > 3f) continue;
                    float cx = kv.Value.x * Screen.width, cy = Screen.height - kv.Value.y * Screen.height;
                    GUI.Label(new Rect(cx - 6, cy - 10, 30, 24), $"<size=22><color={Coop.ColorOf(kv.Key)}>➤</color></size>");
                    GUI.Label(new Rect(cx + 14, cy + 4, 160, 20), $"<color={Coop.ColorOf(kv.Key)}>{Coop.NameOf(kv.Key)}</color>");
                }
            }

            // Propriétaire de chaque dé en main
            if (_showOwnership.Value && Game.InBattle) DrawDiceOwners();

            // Toast et alerte de désynchronisation
            if (Coop.ToastText != null && Time.unscaledTime < Coop.ToastUntil)
                GUI.Box(new Rect(Screen.width / 2f - 200, 60, 400, 28), Coop.ToastText);
            if (Coop.DesyncWarning != null)
                GUI.Label(new Rect(10, 10, 700, 40), $"<size=16><color=#ff5050><b>⚠ {Coop.DesyncWarning}</b></color></size>");
        }

        private void DrawDiceOwners()
        {
            var dm = Game.Dice;
            var hand = Game.Safe(() => dm?.GetAllGODicesOnHand());
            if (hand == null) return;
            foreach (var d in hand)
            {
                try
                {
                    var ad = d.CurrentActiveDice;
                    if (ad == null) continue;
                    var sp = ScreenPos(d.transform);
                    if (sp == null) continue;
                    int owner = Coop.OwnerOfDice(ad.uid);
                    GUI.Label(new Rect(sp.Value.x - 40, Screen.height - sp.Value.y + 34, 80, 20),
                        $"<color={Coop.ColorOf(owner)}><b>{Coop.NameOf(owner)}</b></color>");
                }
                catch { }
            }
        }

        private static Vector2? ScreenPos(Transform t)
        {
            var canvas = t.GetComponentInParent<Canvas>();
            if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay) return t.position;
            var cam = canvas != null && canvas.worldCamera != null ? canvas.worldCamera : Camera.main;
            if (cam == null) return null;
            var p = cam.WorldToScreenPoint(t.position);
            return p.z < 0 ? (Vector2?)null : new Vector2(p.x, p.y);
        }

        // ───────────── Page du menu F1 ─────────────

        public override void OnSettingsGUI()
        {
            var net = Coop.Net;
            UI.BeginRow(); UI.Label("Pseudo :"); _name.Value = UI.Text(_name.Value); UI.EndRow();

            if (net == null)
            {
                _port.Value = UI.IntStepper("Port", _port.Value, 1, 1024, 65535);
                UI.Space();
                UI.Header("Héberger");
                UI.Label("Partage ton IP (locale, ou celle de Radmin VPN, ZeroTier ou Tailscale) et ouvre le port TCP dans le pare-feu.");
                if (UI.Button("Créer la partie"))
                {
                    try { Coop.Attach(NetSession.Host(_port.Value, _name.Value)); Coop.AddLog($"Partie créée sur le port {_port.Value}"); }
                    catch (Exception e) { Coop.AddLog("Impossible d'héberger : " + e.Message); }
                }
                UI.Space();
                UI.Header("Rejoindre");
                UI.BeginRow(); UI.Label("IP de l'hôte :"); _host.Value = UI.Text(_host.Value); UI.EndRow();
                if (UI.Button("Rejoindre")) Coop.Attach(NetSession.Join(_host.Value.Trim(), _port.Value, _name.Value));
            }
            else
            {
                UI.Label($"{(net.IsHost ? "HÔTE" : "CLIENT")} - {net.Status}");
                UI.Label("Joueurs : " + string.Join(", ", net.Players.Select(p => $"{p.Name}{(p.Id == net.LocalId ? " (toi)" : "")}")));
                if (net.IsHost) UI.Label("Quand tout le monde est connecté, lance une nouvelle run : elle démarre chez tous les joueurs.");
                else UI.Label("Attends que l'hôte lance la run.");
                _rotateLeader.Value = UI.Toggle(_rotateLeader.Value, "Meneur tournant à chaque étage (à régler pareil chez tous)");
                if (UI.Button("Quitter la partie")) { Coop.Detach(); Coop.AddLog("Session fermée."); }

                UI.Space();
                UI.BeginRow();
                _chat = UI.Text(_chat);
                if (UI.Button("Envoyer", 80) && !string.IsNullOrWhiteSpace(_chat))
                {
                    Coop.Send(new Msg { t = T.Chat, text = _chat.Trim() });
                    Coop.AddLog($"{_name.Value} : {_chat.Trim()}");
                    _chat = "";
                }
                UI.EndRow();
            }

            _showCursors.Value = UI.Toggle(_showCursors.Value, "Afficher les curseurs des autres");
            _showOwnership.Value = UI.Toggle(_showOwnership.Value, "Afficher le propriétaire des dés");

            UI.Space();
            UI.Header("Journal");
            _logScroll = GUILayout.BeginScrollView(_logScroll, UI.Opt(GUILayout.Height(180)));
            foreach (var l in Coop.Log) UI.Label(l);
            GUILayout.EndScrollView();
        }
    }
}
