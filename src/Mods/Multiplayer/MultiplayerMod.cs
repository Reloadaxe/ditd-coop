using System;
using System.Linq;
using BepInEx.Configuration;
using DiceLoader;
using DiceMultiplayer.Net;
using DiceMultiplayer.Sync;
using DiceMultiplayer.Ui;
using UnityEngine;

namespace DiceMultiplayer
{
    /// <summary>
    /// Mod multijoueur coop (2 ou 3 joueurs) en TCP direct.
    /// Tout le monde joue la même run, synchronisée en lockstep sur les entrées. Voir Sync/Coop.cs.
    /// </summary>
    [DiceModInfo("dicemodding.multiplayer", "Multijoueur Coop", "0.13.1",
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
            _hpPerPlayer = Config.Bind("Règles", "PVEnnemisParJoueur", 1.0f, "Multiplicateur de PV des ennemis par joueur en plus (1 = x2 à deux joueurs, x3 à trois)");
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
            try { UiDump.Update(); } catch { }
            try { AutoTest.Update(); } catch (Exception e) { Logger.LogWarning("AutoTest : " + e.Message); }
            try { Hud.Update(); } catch (Exception e) { Logger.LogDebug("Hud : " + e.Message); }
            var net = Coop.Net;
            if (net == null) return;
            net.Pump();
            try { Heroes.Update(); } catch (Exception e) { Logger.LogDebug("Heroes : " + e.Message); }
            try { Board.Update(); } catch (Exception e) { Logger.LogDebug("Board : " + e.Message); }
            try { Preview.Update(); } catch (Exception e) { Logger.LogDebug("Preview : " + e.Message); }
            try { Coop.UpdateReadyVisual(); } catch { }
            try { Combat.UpdateExecWait(); } catch (Exception e) { Logger.LogDebug("ExecWait : " + e.Message); }
            try { Spells.Update(); } catch { }
            try { Knockout.Update(); } catch { }
            try { Scaling.Update(); } catch { }
            try { Board.UpdateSlotValues(); } catch (Exception e) { Logger.LogDebug("SlotValues : " + e.Message); }


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

        public override void OnBattleStart(BattleSystem battle) { Board.Reset(); Spells.Reset(); Scaling.Reset(); Combat.ResetExec(); }

        public override void OnTurnStateChanged(BattleSystem battle, BattleSystem.TurnState state)
        {
            if (!Coop.Active) return;
            // Fin de mon tour : mes dés quittent le plateau chez les autres
            if (state == BattleSystem.TurnState.EndTurn) Board.OnLocalTurnEnd();
            // Début de tour : l'hôte envoie les PV de référence des ennemis (une fois les coups du tour précédent arrivés)
            if (state == BattleSystem.TurnState.SelectDice && Coop.IsHost) Game.Schedule(Combat.HostBroadcastOncePerTurn, 0.5f);
            // Dés restés sur mon plateau (lourds…) : renvoyés aux autres à chaque nouveau tour
            if (state == BattleSystem.TurnState.SelectDice) Game.Schedule(Board.ResendOwnDice, 0.8f);
            if (state == BattleSystem.TurnState.SelectDice) Knockout.AutoReady();
        }

        public override void OnBattleEnd(BattleSystem battle, BattleSystem.EndState result)
        {
            Coop.ClearReady();
            Knockout.OnBattleEnd(result);
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
        public static int CurrentFloorKey => _floorKey;

        public override void OnRunStart(CurrentRunData run, bool loaded) { _floorKey = 0; Coop.DesyncWarning = null; Knockout.Reset(); }

        // ───────────── Interface (voir Ui/Hud.cs) ─────────────

        /// <summary>Panneau coop ouvert (touche F2, bouton « Multijoueur » de l'écran titre).</summary>
        public bool PanelOpen { get; set; }
        public string PlayerName { get => LocalName; set => LocalName = value; }
        public int Port { get => _port.Value; set => _port.Value = value; }
        public string HostIp { get => _host.Value; set => _host.Value = value; }
        public bool ShowCursors { get => _showCursors.Value; set => _showCursors.Value = value; }
        private ConfigEntry<float> _hpPerPlayer;
        /// <summary>PV des ennemis : multiplicateur par joueur en plus (choisi par l'hôte, figé au lancement de la run).</summary>
        public float HpPerExtraPlayer { get => _hpPerPlayer.Value; set => _hpPerPlayer.Value = value; }

        public void HostGame()
        {
            try { Coop.Attach(NetSession.Host(_port.Value, LocalName)); Coop.AddLog($"Partie créée sur le port {_port.Value}"); }
            catch (Exception e) { Coop.AddLog("Impossible d'héberger : " + e.Message); Coop.Toast("Impossible d'héberger : " + e.Message); }
        }

        public void JoinGame()
        {
            try { Coop.Attach(NetSession.Join(_host.Value.Trim(), _port.Value, LocalName)); }
            catch (Exception e) { Coop.AddLog("Connexion impossible : " + e.Message); Coop.Toast("Connexion impossible : " + e.Message); }
        }

        public void LeaveGame()
        {
            Coop.EndSession(Coop.Active && Coop.IsHost ? "Tu as fermé la partie." : "Tu as quitté la partie.");
        }

        public void SendChat(string text)
        {
            if (string.IsNullOrWhiteSpace(text) || Coop.Net == null) return;
            Coop.Send(new Msg { t = T.Chat, text = text.Trim() });
            Coop.AddLog($"{LocalName} : {text.Trim()}");
        }

        /// <summary>IMGUI : seulement le raccourci F2 et la position de la souris (envoyée aux autres pour les curseurs).</summary>
        public override void OnGUI()
        {
            var ev = Event.current;
            if (ev == null) return;
            _mouse = new Vector2(ev.mousePosition.x, Screen.height - ev.mousePosition.y);
            if (ev.type == EventType.KeyDown)
            {
                if (ev.keyCode == KeyCode.F2) { PanelOpen = !PanelOpen; ev.Use(); }
                else if (ev.keyCode == KeyCode.Escape && PanelOpen) { PanelOpen = false; ev.Use(); }
                else if ((ev.keyCode == KeyCode.Return || ev.keyCode == KeyCode.KeypadEnter) && PanelOpen) Hud.OnReturn();
            }
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
