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
    /// Tout le monde joue la mÃªme run, synchronisÃ©e en lockstep sur les entrÃ©es. Voir Sync/Coop.cs.
    /// </summary>
    [DiceModInfo("dicemodding.multiplayer", "Multijoueur Coop", "0.7.1",
        Author = "DiceModding", Description = "Coop Ã  2 ou 3 : chacun sa run et son hÃ©ros, ennemis et plateau partagÃ©s (TCP).")]
    public class MultiplayerMod : DiceMod
    {
        public static MultiplayerMod Instance;

        private ConfigEntry<string> _name, _host, _localOnly;
        private string _nameOverride;

        /// <summary>Pseudo de cette instance : celui de -ditd-player s'il est fourni, sinon celui de la config.</summary>
        private string LocalName
        {
            get => _nameOverride ?? _name.Value;
            set { if (_nameOverride != null) _nameOverride = value; else _name.Value = value; }
        }
        private ConfigEntry<int> _port;
        private ConfigEntry<bool> _rotateLeader, _showCursors;
        private string _chat = "";
        private Vector2 _mouse;
        private float _nextCursor;
        private Vector2 _logScroll;

        public override void OnLoad()
        {
            Instance = this;
            _name = Config.Bind("RÃ©seau", "Pseudo", Environment.UserName, "Ton nom affichÃ© aux autres joueurs");
            // Argument -ditd-player (test Ã  plusieurs fenÃªtres) : pseudo propre Ã  cette instance, gardÃ© en mÃ©moire seulement
            var args = Environment.GetCommandLineArgs();
            int ai = Array.FindIndex(args, a => string.Equals(a, "-ditd-player", StringComparison.OrdinalIgnoreCase));
            if (ai >= 0 && ai + 1 < args.Length) _nameOverride = args[ai + 1];
            _host = Config.Bind("RÃ©seau", "DernierHote", "127.0.0.1", "Adresse IP de l'hÃ´te (LAN, Radmin VPN, ZeroTier, Tailscaleâ€¦)");
            _port = Config.Bind("RÃ©seau", "Port", 27015, "Port TCP");
            _rotateLeader = Config.Bind("RÃ¨gles", "MeneurTournant", false, "Le meneur (carte, rÃ©compenses, boutique) change Ã  chaque Ã©tage");
            _showCursors = Config.Bind("Affichage", "Curseurs", true, "Afficher les curseurs des autres joueurs");
            _localOnly = Config.Bind("RÃ¨gles", "UILocale", "Settings,Pause,Backpack,Codex,Feedback,Tooltip,Glossary,Options",
                "Mots-clÃ©s de chemins d'UI dont les clics restent locaux (non synchronisÃ©s), sÃ©parÃ©s par des virgules");

            Coop.RotateLeader = () => _rotateLeader.Value;
            Coop.LocalOnlyKeywords = () => _localOnly.Value.Split(',');
        }

        public override void OnEnable() => Patches.Apply(Harmony);

        public override void OnDisable() => Coop.Detach();

        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ Boucle â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

        public override void OnUpdate()
        {
            var net = Coop.Net;
            if (net == null) return;
            net.Pump();
            try { Heroes.Update(); } catch (Exception e) { Logger.LogDebug("Heroes : " + e.Message); }
            try { Board.Update(); } catch (Exception e) { Logger.LogDebug("Board : " + e.Message); }
            try { Preview.Update(); } catch (Exception e) { Logger.LogDebug("Preview : " + e.Message); }
            try { Coop.UpdateReadyVisual(); } catch { }


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

        public override void OnBattleStart(BattleSystem battle) => Board.Reset();

        public override void OnTurnStateChanged(BattleSystem battle, BattleSystem.TurnState state)
        {
            if (!Coop.Active) return;
            // Fin de mon tour : mes dÃ©s quittent le plateau chez les autres
            if (state == BattleSystem.TurnState.EndTurn) Board.OnLocalTurnEnd();
            // DÃ©but de tour : l'hÃ´te envoie les PV de rÃ©fÃ©rence des ennemis (une fois les coups du tour prÃ©cÃ©dent arrivÃ©s)
            if (state == BattleSystem.TurnState.StartTurn && Coop.IsHost) Game.Schedule(Combat.HostBroadcastEnemies, 0.8f);
        }

        public override void OnBattleEnd(BattleSystem battle, BattleSystem.EndState result)
        {
            Coop.ClearReady();
            Board.Reset();
        }

        private static bool _attackWarm;

        /// <summary>
        /// Le constructeur statique d'Attack crÃ©e Attack.ForceKill, qui lit RunInfo.CurrentRun : s'il s'exÃ©cute Ã  un moment
        /// oÃ¹ la run est absente, la classe reste cassÃ©e pour tout le processus (TypeInitializationException Ã  chaque coup).
        /// On le dÃ©clenche donc ici, sur la carte, quand la run existe forcÃ©ment.
        /// ignoreEndlessIncrease = true : notre instance Ã  nous ne lit pas la run.
        /// </summary>
        private void PrewarmAttack()
        {
            if (_attackWarm) return;
            bool hasRun = RunInfo.CurrentRun != null;
            try
            {
                var a = new Attack(0, 1, false, null, false, true);
                _attackWarm = true;
                Logger.LogInfo($"[diag] Attack initialisÃ©e (run {(hasRun ? "prÃ©sente" : "ABSENTE")}, ForceKill {(Attack.ForceKill != null ? "ok" : "null")}).");
            }
            catch (Exception e)
            {
                Coop.AddLog($"[diag] Attack dÃ©jÃ  cassÃ©e avant la carte (run {(hasRun ? "prÃ©sente" : "ABSENTE")}) : {e.GetType().Name} {e.Message}");
            }
        }

        public override void OnGameStateChanged(GameManager.GameState previous, GameManager.GameState current)
        {
            if (current == GameManager.GameState.Map) PrewarmAttack();
            if (current == GameManager.GameState.TitleScreen) Team.Picks.Clear();

            // De retour sur la carte = mon Ã©tage est fini (rÃ©compenses, Ã©vÃ©nement, boutique) : le meneur peut avancer
            if (Coop.Active && Game.InRun)
            {
                if (current == GameManager.GameState.Map) Coop.SetOnMap(true);
                else if (previous == GameManager.GameState.Map) Coop.SetOnMap(false);
            }
        }

        public override void OnTurnStart(BattleSystem battle, int turn)
        {
            if (!Coop.Active) return;
            // Empreinte prise un peu aprÃ¨s le dÃ©but du tour, une fois la pioche terminÃ©e
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

        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ Overlay â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

        public override void OnGUI()
        {
            var ev = Event.current;
            if (ev != null) _mouse = new Vector2(ev.mousePosition.x, Screen.height - ev.mousePosition.y);

            // Ã‰cran d'attente du client (avant que l'hÃ´te n'ouvre la sÃ©lection)
            if (Lobby.ClientWaiting && !_panelOpen)
            {
                Lobby.DrawWaitingScreen(() =>
                {
                    UI.BeginRow();
                    if (UI.Button("Quitter la partie", 160)) { Coop.Detach(); Coop.AddLog("Session fermÃ©e."); }
                    if (UI.Button("Panneau coop (F2)", 160)) _panelOpen = true;
                    UI.EndRow();
                });
                if (ev != null && ev.type == EventType.KeyDown && ev.keyCode == KeyCode.F2) { _panelOpen = true; ev.Use(); }
                return;
            }

            // Panneau coop : touche F2 ou bouton en haut Ã  gauche
            if (ev != null && ev.type == EventType.KeyDown && ev.keyCode == KeyCode.F2) { _panelOpen = !_panelOpen; ev.Use(); }
            if (_panelOpen) DrawPanel();
            else if (!Game.InRun)
            {
                GUILayout.BeginArea(new Rect(10, 10, 220, 34));
                if (UI.Button(Coop.Active ? $"Coop : {Coop.Net.Players.Count} joueur(s) (F2)" : "Multijoueur coop (F2)")) _panelOpen = true;
                GUILayout.EndArea();
            }

            if (!Coop.Active) return;
            if (ev == null || ev.type != EventType.Repaint) return;

            // Liste des joueurs (en haut Ã  droite)
            float w = 240, x = Screen.width - w - 10, y = 10;
            var players = Coop.Net.Players.OrderBy(p => p.Id).ToList();
            GUI.Box(new Rect(x, y, w, 26 + players.Count * 20 + (Game.InBattle ? 20 : 0)), "");
            GUI.Label(new Rect(x + 8, y + 4, w, 20), $"<b>Coop (F2)</b> - chemin choisi au vote");
            y += 24;
            foreach (var p in players)
            {
                string me = p.Id == Coop.LocalId ? " (toi)" : "";
                string extra;
                if (!Game.InRun)
                    extra = Team.Picks.TryGetValue(p.Id, out var pick) ? $"  âœ” {Team.Describe(pick)}" : "  â€¦ choisit";
                else
                {
                    var rp = Team.RunPicks.FirstOrDefault(r => r.Id == p.Id);
                    extra = rp != null ? $" ({(Player.CharacterName)rp.Run.Character})" : "";
                    if (Game.InBattle) extra += Coop.ReadyPlayers.Contains(p.Id) ? "  âœ” prÃªt" : "  â€¦ joue";
                }
                GUI.Label(new Rect(x + 8, y, w, 20), $"<color={p.Color}>â—</color> {p.Name}{me}{extra}");
                y += 20;
            }
            if (Game.InBattle) GUI.Label(new Rect(x + 8, y, w, 20), "<i>Fin de tour : tout le monde doit valider</i>");

            // Noms au-dessus des personnages
            try { Heroes.DrawLabels(); } catch { }

            // Curseurs des autres joueurs
            if (_showCursors.Value)
            {
                string scene = Game.State.ToString();
                foreach (var kv in Coop.Cursors)
                {
                    if (kv.Key == Coop.LocalId || kv.Value.scene != scene || Time.unscaledTime - kv.Value.time > 3f) continue;
                    float cx = kv.Value.x * Screen.width, cy = Screen.height - kv.Value.y * Screen.height;
                    GUI.Label(new Rect(cx - 6, cy - 10, 30, 24), $"<size=22><color={Coop.ColorOf(kv.Key)}>âž¤</color></size>");
                    GUI.Label(new Rect(cx + 14, cy + 4, 160, 20), $"<color={Coop.ColorOf(kv.Key)}>{Coop.NameOf(kv.Key)}</color>");
                }
            }

            // DÃ©s posÃ©s par les autres joueurs sur le plateau
            try { Board.DrawGhosts(); } catch { }
            try { Coop.DrawVotes(); } catch { }
            try { Coop.DrawReadyBanner(); } catch { }
            try { Preview.Draw(); } catch { }

            // Toast et alerte de dÃ©synchronisation
            if (Coop.ToastText != null && Time.unscaledTime < Coop.ToastUntil)
                GUI.Box(new Rect(Screen.width / 2f - 200, 60, 400, 28), Coop.ToastText);
            if (Coop.DesyncWarning != null)
                GUI.Label(new Rect(10, 10, 700, 40), $"<size=16><color=#ff5050><b>âš  {Coop.DesyncWarning}</b></color></size>");
        }

        private bool _panelOpen;
        private Vector2 _panelScroll;

        private void DrawPanel()
        {
            var r = new Rect(10, 10, 460, Math.Min(620f, Screen.height - 20f));
            GUI.Box(r, "");
            GUILayout.BeginArea(new Rect(r.x + 10, r.y + 8, r.width - 20, r.height - 16));
            UI.BeginRow();
            UI.Header("Multijoueur coop");
            GUILayout.FlexibleSpace();
            if (UI.Button("Fermer (F2)", 110)) _panelOpen = false;
            UI.EndRow();
            _panelScroll = GUILayout.BeginScrollView(_panelScroll, UI.None);
            try { OnSettingsGUI(); }
            catch (Exception e) { UI.Label("Erreur : " + e.Message); Logger.LogError(e); }
            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }


        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ Page du menu F1 â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

        public override void OnSettingsGUI()
        {
            var net = Coop.Net;
            UI.BeginRow(); UI.Label("Pseudo :"); LocalName = UI.Text(LocalName); UI.EndRow();

            if (net == null)
            {
                _port.Value = UI.IntStepper("Port", _port.Value, 1, 1024, 65535);
                UI.Space();
                UI.Header("HÃ©berger");
                UI.Label("Partage ton IP (locale, ou celle de Radmin VPN, ZeroTier ou Tailscale) et ouvre le port TCP dans le pare-feu.");
                if (UI.Button("CrÃ©er la partie"))
                {
                    try { Coop.Attach(NetSession.Host(_port.Value, LocalName)); Coop.AddLog($"Partie crÃ©Ã©e sur le port {_port.Value}"); }
                    catch (Exception e) { Coop.AddLog("Impossible d'hÃ©berger : " + e.Message); }
                }
                UI.Space();
                UI.Header("Rejoindre");
                UI.BeginRow(); UI.Label("IP de l'hÃ´te :"); _host.Value = UI.Text(_host.Value); UI.EndRow();
                if (UI.Button("Rejoindre")) Coop.Attach(NetSession.Join(_host.Value.Trim(), _port.Value, LocalName));
            }
            else
            {
                UI.Label($"{(net.IsHost ? "HÃ”TE" : "CLIENT")} - {net.Status}");
                UI.Label("Joueurs : " + string.Join(", ", net.Players.Select(p => $"{p.Name}{(p.Id == net.LocalId ? " (toi)" : "")}")));
                UI.Label("Chacun choisit son personnage (Nouvelle run) puis clique sur Commencer. La run dÃ©marre quand tout le monde est prÃªt.");

                if (UI.Button("Quitter la partie")) { Coop.Detach(); Coop.AddLog("Session fermÃ©e."); }

                UI.Space();
                UI.BeginRow();
                _chat = UI.Text(_chat);
                if (UI.Button("Envoyer", 80) && !string.IsNullOrWhiteSpace(_chat))
                {
                    Coop.Send(new Msg { t = T.Chat, text = _chat.Trim() });
                    Coop.AddLog($"{LocalName} : {_chat.Trim()}");
                    _chat = "";
                }
                UI.EndRow();
            }

            _showCursors.Value = UI.Toggle(_showCursors.Value, "Afficher les curseurs des autres");

            UI.Space();
            UI.Header("Journal");
            _logScroll = GUILayout.BeginScrollView(_logScroll, UI.Opt(GUILayout.Height(180)));
            foreach (var l in Coop.Log) UI.Label(l);
            GUILayout.EndScrollView();
        }
    }
}
