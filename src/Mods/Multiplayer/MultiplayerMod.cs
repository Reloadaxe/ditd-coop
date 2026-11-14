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
    [DiceModInfo("dicemodding.multiplayer", "Multijoueur Coop", "0.5.15",
        Author = "DiceModding", Description = "Coop à 2 ou 3 : chacun sa run et son héros, ennemis et plateau partagés (TCP).")]
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
            _name = Config.Bind("Réseau", "Pseudo", Environment.UserName, "Ton nom affiché aux autres joueurs");
            // Argument -ditd-player (test à plusieurs fenêtres) : pseudo propre à cette instance, gardé en mémoire seulement
            var args = Environment.GetCommandLineArgs();
            int ai = Array.FindIndex(args, a => string.Equals(a, "-ditd-player", StringComparison.OrdinalIgnoreCase));
            if (ai >= 0 && ai + 1 < args.Length) _nameOverride = args[ai + 1];
            _host = Config.Bind("Réseau", "DernierHote", "127.0.0.1", "Adresse IP de l'hôte (LAN, Radmin VPN, ZeroTier, Tailscale…)");
            _port = Config.Bind("Réseau", "Port", 27015, "Port TCP");
            _rotateLeader = Config.Bind("Règles", "MeneurTournant", false, "Le meneur (carte, récompenses, boutique) change à chaque étage");
            _showCursors = Config.Bind("Affichage", "Curseurs", true, "Afficher les curseurs des autres joueurs");
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
            try { Heroes.Update(); } catch (Exception e) { Logger.LogDebug("Heroes : " + e.Message); }
            try { Board.Update(); } catch (Exception e) { Logger.LogDebug("Board : " + e.Message); }
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
            // Fin de mon tour : mes dés quittent le plateau chez les autres
            if (state == BattleSystem.TurnState.EndTurn) Board.OnLocalTurnEnd();
            // Début de tour : l'hôte envoie les PV de référence des ennemis (une fois les coups du tour précédent arrivés)
            if (state == BattleSystem.TurnState.StartTurn && Coop.IsHost) Game.Schedule(Combat.HostBroadcastEnemies, 0.8f);
        }

        public override void OnBattleEnd(BattleSystem battle, BattleSystem.EndState result)
        {
            Coop.ClearReady();
            Board.Reset();
        }

        private static bool _attackWarm;

        /// <summary>
        /// Le constructeur statique d'Attack crée Attack.ForceKill, qui lit RunInfo.CurrentRun : s'il s'exécute à un moment
        /// où la run est absente, la classe reste cassée pour tout le processus (TypeInitializationException à chaque coup).
        /// On le déclenche donc ici, sur la carte, quand la run existe forcément.
        /// ignoreEndlessIncrease = true : notre instance à nous ne lit pas la run.
        /// </summary>
        private void PrewarmAttack()
        {
            if (_attackWarm) return;
            bool hasRun = RunInfo.CurrentRun != null;
            try
            {
                var a = new Attack(0, 1, false, null, false, true);
                _attackWarm = true;
                Logger.LogInfo($"[diag] Attack initialisée (run {(hasRun ? "présente" : "ABSENTE")}, ForceKill {(Attack.ForceKill != null ? "ok" : "null")}).");
            }
            catch (Exception e)
            {
                Coop.AddLog($"[diag] Attack déjà cassée avant la carte (run {(hasRun ? "présente" : "ABSENTE")}) : {e.GetType().Name} {e.Message}");
            }
        }

        public override void OnGameStateChanged(GameManager.GameState previous, GameManager.GameState current)
        {
            if (current == GameManager.GameState.Map) PrewarmAttack();
            if (current == GameManager.GameState.TitleScreen) Team.Picks.Clear();

            // De retour sur la carte = mon étage est fini (récompenses, événement, boutique) : le meneur peut avancer
            if (Coop.Active && Game.InRun)
            {
                if (current == GameManager.GameState.Map) Coop.SetOnMap(true);
                else if (previous == GameManager.GameState.Map) Coop.SetOnMap(false);
            }
        }

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

            // Écran d'attente du client (avant que l'hôte n'ouvre la sélection)
            if (Lobby.ClientWaiting && !_panelOpen)
            {
                Lobby.DrawWaitingScreen(() =>
                {
                    UI.BeginRow();
                    if (UI.Button("Quitter la partie", 160)) { Coop.Detach(); Coop.AddLog("Session fermée."); }
                    if (UI.Button("Panneau coop (F2)", 160)) _panelOpen = true;
                    UI.EndRow();
                });
                if (ev != null && ev.type == EventType.KeyDown && ev.keyCode == KeyCode.F2) { _panelOpen = true; ev.Use(); }
                return;
            }

            // Panneau coop : touche F2 ou bouton en haut à gauche
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

            // Liste des joueurs (en haut à droite)
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
                    extra = Team.Picks.TryGetValue(p.Id, out var pick) ? $"  ✔ {Team.Describe(pick)}" : "  … choisit";
                else
                {
                    var rp = Team.RunPicks.FirstOrDefault(r => r.Id == p.Id);
                    extra = rp != null ? $" ({(Player.CharacterName)rp.Run.Character})" : "";
                    if (Game.InBattle) extra += Coop.ReadyPlayers.Contains(p.Id) ? "  ✔ prêt" : "  … joue";
                }
                GUI.Label(new Rect(x + 8, y, w, 20), $"<color={p.Color}>●</color> {p.Name}{me}{extra}");
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
                    GUI.Label(new Rect(cx - 6, cy - 10, 30, 24), $"<size=22><color={Coop.ColorOf(kv.Key)}>➤</color></size>");
                    GUI.Label(new Rect(cx + 14, cy + 4, 160, 20), $"<color={Coop.ColorOf(kv.Key)}>{Coop.NameOf(kv.Key)}</color>");
                }
            }

            // Dés posés par les autres joueurs sur le plateau
            try { Board.DrawGhosts(); } catch { }
            try { Coop.DrawVotes(); } catch { }
            try { Coop.DrawReadyBanner(); } catch { }

            // Toast et alerte de désynchronisation
            if (Coop.ToastText != null && Time.unscaledTime < Coop.ToastUntil)
                GUI.Box(new Rect(Screen.width / 2f - 200, 60, 400, 28), Coop.ToastText);
            if (Coop.DesyncWarning != null)
                GUI.Label(new Rect(10, 10, 700, 40), $"<size=16><color=#ff5050><b>⚠ {Coop.DesyncWarning}</b></color></size>");
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


        // ───────────── Page du menu F1 ─────────────

        public override void OnSettingsGUI()
        {
            var net = Coop.Net;
            UI.BeginRow(); UI.Label("Pseudo :"); LocalName = UI.Text(LocalName); UI.EndRow();

            if (net == null)
            {
                _port.Value = UI.IntStepper("Port", _port.Value, 1, 1024, 65535);
                UI.Space();
                UI.Header("Héberger");
                UI.Label("Partage ton IP (locale, ou celle de Radmin VPN, ZeroTier ou Tailscale) et ouvre le port TCP dans le pare-feu.");
                if (UI.Button("Créer la partie"))
                {
                    try { Coop.Attach(NetSession.Host(_port.Value, LocalName)); Coop.AddLog($"Partie créée sur le port {_port.Value}"); }
                    catch (Exception e) { Coop.AddLog("Impossible d'héberger : " + e.Message); }
                }
                UI.Space();
                UI.Header("Rejoindre");
                UI.BeginRow(); UI.Label("IP de l'hôte :"); _host.Value = UI.Text(_host.Value); UI.EndRow();
                if (UI.Button("Rejoindre")) Coop.Attach(NetSession.Join(_host.Value.Trim(), _port.Value, LocalName));
            }
            else
            {
                UI.Label($"{(net.IsHost ? "HÔTE" : "CLIENT")} - {net.Status}");
                UI.Label("Joueurs : " + string.Join(", ", net.Players.Select(p => $"{p.Name}{(p.Id == net.LocalId ? " (toi)" : "")}")));
                UI.Label("Chacun choisit son personnage (Nouvelle run) puis clique sur Commencer. La run démarre quand tout le monde est prêt.");

                if (UI.Button("Quitter la partie")) { Coop.Detach(); Coop.AddLog("Session fermée."); }

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
