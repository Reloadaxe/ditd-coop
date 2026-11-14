using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DiceLoader;
using DiceMultiplayer.Net;
using Il2CppInterop.Runtime.InteropTypes;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DiceMultiplayer.Sync
{
    /// <summary>
    /// Contrôleur de la coop : reçoit les messages, rejoue les actions et gère qui a le droit de faire quoi.
    ///
    /// Modèle « lockstep sur les entrées » :
    ///  - tout le monde joue la même run (même personnage, même seed) ;
    ///  - une action locale s'exécute tout de suite et part sur le réseau, puis les autres la rejouent ;
    ///  - pour éviter les conflits, hors combat seul le *meneur* agit (carte, récompenses, événements, boutique) ;
    ///    en combat, chacun ne manipule que ses propres dés ;
    ///  - la fin de tour exige que tous les joueurs soient prêts ;
    ///  - une empreinte de l'état est comparée à chaque tour pour détecter les désynchronisations.
    /// </summary>
    public static class Coop
    {
        public static NetSession Net;
        public static bool Active => Net != null && Net.Connected && Net.Players.Count > 0;
        public static bool IsHost => Net?.IsHost ?? false;
        public static int LocalId => Net?.LocalId ?? -1;

        /// <summary>Vrai pendant qu'on rejoue une action venue du réseau : les patchs la laissent passer sans la renvoyer.</summary>
        public static bool Replaying;
        /// <summary>Profondeur d'appels OnPointerClick imbriqués (override puis base), pour n'émettre qu'une seule fois.</summary>
        public static int ClickDepth;

        public static int? PendingSeed;
        public static string DesyncWarning;
        public static readonly List<string> Log = new List<string>();
        public static readonly Dictionary<int, (float x, float y, string scene, float time)> Cursors = new Dictionary<int, (float, float, string, float)>();

        private static readonly HashSet<int> _ready = new HashSet<int>();
        private static readonly Dictionary<int, long> _hostHashes = new Dictionary<int, long>();
        private static readonly Dictionary<int, Dictionary<int, long>> _clientHashes = new Dictionary<int, Dictionary<int, long>>();

        // Config, remplie par MultiplayerMod
        public static Func<bool> RotateLeader = () => false;
        public static Func<string[]> LocalOnlyKeywords = () => Array.Empty<string>();

        // ───────────── Rôles ─────────────

        /// <summary>Id du meneur : l'hôte, ou un joueur différent à chaque étage si la rotation est activée.</summary>
        public static int LeaderId
        {
            get
            {
                if (!Active || Net.Players.Count == 0) return 0;
                if (!RotateLeader()) return 0;
                int floor = _floorCounter;
                var ordered = Net.Players.OrderBy(p => p.Id).ToList();
                return ordered[Math.Abs(floor) % ordered.Count].Id;
            }
        }

        private static int _floorCounter;
        private static int RunFloorIndex() => _floorCounter;
        public static void OnFloorStarted() => _floorCounter++;

        public static bool IsLeader => !Active || LeaderId == LocalId;

        /// <summary>Propriétaire d'un dé : celui dont le personnage l'a apporté (Team), sinon répartition stable par uid.</summary>
        public static int OwnerOfDice(int uid)
        {
            if (!Active) return LocalId;
            var known = Team.OwnerOf(uid);
            if (known != null && GetPlayer(known.Value) != null) return known.Value;
            var ordered = Net.Players.OrderBy(p => p.Id).ToList();
            return ordered[((uid % ordered.Count) + ordered.Count) % ordered.Count].Id;
        }

        public static bool OwnsDice(GameObjectDice d)
        {
            if (!Active || d == null) return true;
            var ad = Game.Safe(() => d.CurrentActiveDice);
            if (ad == null) return true;
            return OwnerOfDice(ad.uid) == LocalId;
        }

        public static PlayerInfo GetPlayer(int id) => Net?.GetPlayer(id);
        public static string NameOf(int id) => id < 0 ? "Système" : GetPlayer(id)?.Name ?? ("#" + id);
        public static string ColorOf(int id) => GetPlayer(id)?.Color ?? "#FFFFFF";

        // ───────────── Session ─────────────

        public static void Attach(NetSession s)
        {
            Detach();
            Net = s;
            s.OnMessage += Handle;
            s.OnPlayersChanged += () => { AddLog("Joueurs : " + string.Join(", ", s.Players.Select(p => p.Name))); Lobby.OnPlayersChanged(); };
            s.OnDisconnected += reason => AddLog(reason);
        }

        public static void Detach()
        {
            Net?.Dispose();
            Net = null;
            Cursors.Clear();
            _ready.Clear();
            DesyncWarning = null;
            PendingSeed = null;
            _floorCounter = 0;
            Team.Reset();
            Lobby.Reset();
        }

        public static void AddLog(string s)
        {
            Log.Add($"[{DateTime.Now:HH:mm:ss}] {s}");
            if (Log.Count > 60) Log.RemoveAt(0);
            MultiplayerMod.Instance?.Logger.LogInfo(s);
        }

        public static void Send(Msg m) { if (Active) Net.Broadcast(m); }

        // ───────────── Réception ─────────────

        private static void Handle(Msg m)
        {
            try
            {
                switch (m.t)
                {
                    case T.Cursor: Cursors[m.id] = (m.x, m.y, m.scene, Time.unscaledTime); break;
                    case T.Chat: AddLog($"{NameOf(m.id)} : {m.text}"); break;
                    case T.StartRun: ApplyStartRun(m); break;
                    case T.Pick: Team.OnRemotePick(m); break;
                    case T.OpenSel: Lobby.OnOpenSelection(); break;
                    case T.CloseSel: Lobby.OnCloseSelection(); break;
                    case T.Click: ReplayClick(m); break;
                    case T.PlaceDice: ReplayPlaceDice(m); break;
                    case T.ReturnDice: ReplayReturnDice(m); break;
                    case T.Ready: OnReady(m.id, m.turn, m.flag); break;
                    case T.EndTurn: ExecuteEndTurn(); break;
                    case T.Hash: OnRemoteHash(m); break;
                    case T.Desync: DesyncWarning = m.text; AddLog("<color=#ff6060>" + m.text + "</color>"); break;
                }
            }
            catch (Exception e)
            {
                AddLog($"Erreur en rejouant « {m.t} » de {NameOf(m.id)} : {e.Message}");
                MultiplayerMod.Instance?.Logger.LogError(e);
            }
        }

        public static void Replay(Action a) => WithReplay(a);

        private static void WithReplay(Action a)
        {
            bool prev = Replaying;
            Replaying = true;
            try { a(); }
            finally { Replaying = prev; }
        }

        // ───────────── Lancement de run ─────────────

        /// <summary>Hôte : appelé après SeedSystem.StartRun, quand la seed est connue.</summary>
        public static void HostBroadcastRun(RunData team, int seed)
        {
            Send(new Msg { t = T.StartRun, run = team, seed = seed, picks = Team.RunPicks });
            _floorCounter = 0;
            Team.OnHostTeamRunStarted();
            Heroes.OnTeamRunStarted();
            Lobby.OnRunStarted();
            AddLog($"Run d'équipe lancée pour tous (seed {seed}) : " +
                   string.Join(", ", Team.RunPicks.Select(p => $"{NameOf(p.Id)} = {(Player.CharacterName)p.Run.Character}")));
        }

        /// <summary>Lance GameManager.StartRun sans que nos patchs l'interceptent.</summary>
        public static void StartRunReplaying(RunInfo.NewRunData data)
        {
            var gm = Game.Manager;
            if (gm == null) { AddLog("GameManager introuvable : impossible de lancer la run."); return; }
            var custom = Team.LocalCustomRun;
            if (custom == null) AddLog("Attention : aucun ActiveCustomRun local (as-tu cliqué sur Commencer ?)");
            WithReplay(() => gm.StartRun(data, custom, true));
        }

        private static void ApplyStartRun(Msg m)
        {
            if (m.run == null) return;
            Team.OnTeamRunReceived(m);
            Heroes.OnTeamRunStarted();
            Lobby.OnRunStarted();
            PendingSeed = m.seed;
            _floorCounter = 0;
            AddLog($"Run d'équipe lancée par {NameOf(m.id)} (seed {m.seed}) : " +
                   string.Join(", ", Team.RunPicks.Select(p => $"{NameOf(p.Id)} = {(Player.CharacterName)p.Run.Character}")));
            var data = Team.ToNewRunData(m.run);
            Game.Schedule(() => StartRunReplaying(data), 0.1f);
        }

        /// <summary>Client : force la seed de l'hôte juste après la génération de la sienne.</summary>
        public static void ApplyPendingSeed(SeedSystem seedSystem)
        {
            if (PendingSeed == null || seedSystem == null) return;
            int s = PendingSeed.Value;
            PendingSeed = null;
            var m = typeof(SeedSystem).GetMethod(nameof(SeedSystem.ForceSeed));
            m.Invoke(seedSystem, new object[] { s });

            // ForceSeed n'écrit que le champ seed. SeedSystem.StartRun a déjà créé ses 4 générateurs statiques à partir
            // de SA seed : on les recrée comme le jeu le fait (Random(seed + étape)), sinon dés, reliques, récompenses
            // et potions divergeraient entre les joueurs.
            try
            {
                SeedSystem.DiceRNG = SeedSystem.Rng.GetRandomRng(SeedSystem.Rng.Context.Dice);
                SeedSystem.RelicRNG = SeedSystem.Rng.GetRandomRng(SeedSystem.Rng.Context.Relic);
                SeedSystem.RewardRNG = SeedSystem.Rng.GetRandomRng(SeedSystem.Rng.Context.Reward);
                SeedSystem.PotionRNG = SeedSystem.Rng.GetRandomRng(SeedSystem.Rng.Context.Potion);
            }
            catch (Exception e) { AddLog("Générateurs aléatoires non resynchronisés : " + e.Message); }
            AddLog($"Seed synchronisée : {s} (lue après : {ReadSeed(seedSystem)})");
        }

        public static int ReadSeed(SeedSystem ss) => Game.Safe(() => ss.seed);

        // ───────────── Clics génériques ─────────────

        /// <summary>Patch préfixe posé sur toutes les méthodes OnPointerClick du jeu et de l'UI Unity. Renvoie false pour bloquer.</summary>
        public static bool OnLocalClick(Il2CppObjectBase instance, PointerEventData data, MethodBase original)
        {
            if (!Active || Replaying) return true;
            if (!Game.InRun)                            // lobby et sélection de personnage : chacun clique chez soi
            {
                if (Lobby.ClientWaiting) { Toast("En attente de l'hôte…"); return false; }
                return true;
            }
            if (ClickDepth++ > 0) return true;          // appel imbriqué (base.OnPointerClick) : déjà traité

            var comp = instance.TryCast<Component>();
            if (comp == null) return true;
            var go = comp.gameObject;
            string path = GoPath.Of(go);

            if (GoPath.PathContains(path, LocalOnlyKeywords())) return true;     // UI locale : options, sac, codex…
            if (IsFinishTurnButton(go)) return true;                              // fin de tour : gérée par le système « prêt »

            if (!CanAct(path, comp, out var why))
            {
                ClickDepth = 0;
                Toast(why);
                return false;
            }

            var pos = data != null ? data.position : Vector2.zero;
            Send(new Msg
            {
                t = T.Click,
                path = path,
                // Vrai type IL2CPP de l'objet : la méthode patchée peut être une fonction native fusionnée avec celle d'un autre type
                comp = Game.Safe(() => comp.GetIl2CppType().FullName) ?? original.DeclaringType?.FullName,
                method = "OnPointerClick",
                btn = data != null ? (int)data.button : 0,
                x = pos.x / Math.Max(1, Screen.width),
                y = pos.y / Math.Max(1, Screen.height),
            });
            return true;
        }

        public static void EndLocalClick() { if (ClickDepth > 0) ClickDepth--; }

        private static bool CanAct(string path, Component comp, out string why)
        {
            why = null;
            bool inSelectPhase = Game.InBattle && Game.TurnState == BattleSystem.TurnState.SelectDice;
            if (inSelectPhase)
            {
                var dice = comp.GetComponentInParent<GameObjectDice>();
                if (dice != null && !OwnsDice(dice)) { why = $"Ce dé appartient à {NameOf(OwnerOfDice(dice.CurrentActiveDice.uid))}"; return false; }
                return true;
            }
            if (!IsLeader) { why = $"C'est {NameOf(LeaderId)} qui décide ici"; return false; }
            return true;
        }

        private static readonly Dictionary<string, Type> _typeCache = new Dictionary<string, Type>();

        /// <summary>Type d'interop C# correspondant à un nom de type IL2CPP (ex. "VisualMapTile", "UnityEngine.UI.Button").</summary>
        private static Type ResolveManagedType(string il2cppName)
        {
            if (string.IsNullOrEmpty(il2cppName)) return null;
            if (_typeCache.TryGetValue(il2cppName, out var t)) return t;
            var candidates = new[] { typeof(GameManager).Assembly, typeof(UnityEngine.UI.Button).Assembly, typeof(Component).Assembly };
            t = candidates.Select(a => a.GetType(il2cppName, false)).FirstOrDefault(x => x != null)
                ?? AppDomain.CurrentDomain.GetAssemblies().Select(a => { try { return a.GetType(il2cppName, false); } catch { return null; } }).FirstOrDefault(x => x != null && typeof(Il2CppObjectBase).IsAssignableFrom(x));
            _typeCache[il2cppName] = t;
            return t;
        }

        private static void ReplayClick(Msg m)
        {
            var go = GoPath.Find(m.path);
            if (go == null) { AddLog($"Clic de {NameOf(m.id)} introuvable : {m.path}"); return; }

            var type = ResolveManagedType(m.comp);
            if (type == null) { AddLog($"Type inconnu : {m.comp}"); return; }

            var il2Type = Il2CppInterop.Runtime.Il2CppType.From(type);
            var comp = go.GetComponent(il2Type);
            if (comp == null) { AddLog($"Composant {type.Name} absent sur {go.name}"); return; }

            var wrapper = typeof(Il2CppObjectBase).GetMethod(nameof(Il2CppObjectBase.TryCast)).MakeGenericMethod(type).Invoke(comp, null);
            var method = type.GetMethod(m.method, new[] { typeof(PointerEventData) });
            if (wrapper == null || method == null) return;

            var ped = new PointerEventData(EventSystem.current)
            {
                button = (PointerEventData.InputButton)m.btn,
                position = new Vector2(m.x * Screen.width, m.y * Screen.height),
                pointerPress = go,
                rawPointerPress = go,
                pointerEnter = go,
                clickCount = 1,
            };
            WithReplay(() => method.Invoke(wrapper, new object[] { ped }));
        }

        // ───────────── Dés (glisser-déposer) ─────────────

        public static void OnLocalPlaceDice(BoardSlot slot, GameObjectDice dice)
        {
            if (!Active || Replaying || ClickDepth > 0) return; // un clic l'a déjà transmis
            var ad = dice?.CurrentActiveDice;
            if (ad == null || slot == null) return;
            var c = slot.CurrentCoord;
            Send(new Msg { t = T.PlaceDice, uid = ad.uid, sx = c.x, sy = c.y });
        }

        public static void OnLocalReturnDice(GameObjectDice dice)
        {
            if (!Active || Replaying || ClickDepth > 0) return;
            var ad = dice?.CurrentActiveDice;
            if (ad == null || !Game.InBattle || Game.TurnState != BattleSystem.TurnState.SelectDice) return;
            Send(new Msg { t = T.ReturnDice, uid = ad.uid });
        }

        private static GameObjectDice FindDice(int uid)
        {
            foreach (var d in UnityEngine.Object.FindObjectsOfType<GameObjectDice>())
            {
                var ad = Game.Safe(() => d.CurrentActiveDice);
                if (ad != null && ad.uid == uid) return d;
            }
            return null;
        }

        private static BoardSlot FindSlot(int x, int y)
        {
            foreach (var s in UnityEngine.Object.FindObjectsOfType<BoardSlot>())
            {
                var c = s.CurrentCoord;
                if (c.x == x && c.y == y && s.gameObject.activeInHierarchy) return s;
            }
            return null;
        }

        private static void ReplayPlaceDice(Msg m)
        {
            var dice = FindDice(m.uid);
            var slot = FindSlot(m.sx, m.sy);
            var board = UnityEngine.Object.FindObjectOfType<BattleBoard>();
            if (dice == null || slot == null || board == null) { AddLog($"Pose de dé impossible (uid {m.uid} → {m.sx},{m.sy})"); return; }
            WithReplay(() => board.PlaceDiceOnBoard(slot, dice, Game.Player, BattleBoard.PlaceDiceContext.Player));
        }

        private static void ReplayReturnDice(Msg m)
        {
            var dice = FindDice(m.uid);
            var ret = UnityEngine.Object.FindObjectOfType<ReturnDiceToHand>();
            if (dice == null || ret == null) return;
            WithReplay(() => ret.ExecuteReturnDiceToHand(dice, true));
        }

        // ───────────── Fin de tour collective ─────────────

        public static IReadOnlyCollection<int> ReadyPlayers => _ready;

        /// <summary>Vrai uniquement pendant la vraie fin de tour décidée par l'hôte (tout le monde prêt).</summary>
        private static bool _executingEndTurn;

        /// <summary>
        /// Préfixe de FinishTurnUI.OnClickFinishTurnButton. Renvoie false pour bloquer l'action du jeu.
        /// Seul ExecuteEndTurn la laisse passer : un clic rejoué ou un appel venu d'ailleurs ne termine jamais le tour d'un seul joueur.
        /// </summary>
        public static bool OnLocalFinishTurn()
        {
            if (!Active) return true;
            if (_executingEndTurn) return true;
            if (Replaying) return false;
            bool nowReady = !_ready.Contains(LocalId);
            Send(new Msg { t = T.Ready, turn = Game.Turn, flag = nowReady });
            OnReady(LocalId, Game.Turn, nowReady);
            return false;
        }

        /// <summary>Nouveau tour ou nouveau combat : plus personne n'est prêt.</summary>
        public static void ClearReady() => _ready.Clear();

        private static void OnReady(int id, int turn, bool ready)
        {
            // Pas de remise à zéro sur un numéro de tour différent : un léger décalage entre clients ne doit pas annuler le « prêt » des autres.
            if (ready) _ready.Add(id); else _ready.Remove(id);
            AddLog($"{NameOf(id)} {(ready ? "est prêt" : "n'est plus prêt")} ({_ready.Count}/{Net.Players.Count})");

            if (IsHost && Net.Players.All(p => _ready.Contains(p.Id)))
            {
                Send(new Msg { t = T.EndTurn, turn = turn });
                ExecuteEndTurn();
            }
        }

        private static void ExecuteEndTurn()
        {
            _ready.Clear();
            var ui = UnityEngine.Object.FindObjectOfType<FinishTurnUI>();
            if (ui == null) { AddLog("Bouton de fin de tour introuvable"); return; }
            _executingEndTurn = true;
            try { WithReplay(() => ui.OnClickFinishTurnButton()); }
            finally { _executingEndTurn = false; }
        }

        /// <summary>Vrai si le GameObject cliqué est le bouton de fin de tour, ou l'un de ses enfants.</summary>
        private static bool IsFinishTurnButton(GameObject go)
        {
            if (go.GetComponentInParent<FinishTurnUI>() != null) return true;
            var ft = UnityEngine.Object.FindObjectOfType<FinishTurnUI>();
            var btn = ft != null ? Game.Safe(() => ft.finishTurnButton) : null;
            return btn != null && (go == btn.gameObject || go.transform.IsChildOf(btn.transform));
        }

        // ───────────── Détection de désynchronisation ─────────────

        public static void SubmitHash(int key, long hash, string detail)
        {
            if (!Active) return;
            if (IsHost)
            {
                _hostHashes[key] = hash;
                if (_clientHashes.TryGetValue(key, out var others))
                    foreach (var kv in others) Compare(key, kv.Key, kv.Value);
            }
            else Send(new Msg { t = T.Hash, turn = key, h = hash, text = detail });
        }

        private static void OnRemoteHash(Msg m)
        {
            if (!IsHost) return;
            if (!_clientHashes.TryGetValue(m.turn, out var d)) _clientHashes[m.turn] = d = new Dictionary<int, long>();
            d[m.id] = m.h;
            Compare(m.turn, m.id, m.h);
            if (_clientHashes.Count > 50) _clientHashes.Remove(_clientHashes.Keys.Min());
        }

        private static void Compare(int key, int playerId, long hash)
        {
            if (!_hostHashes.TryGetValue(key, out var mine) || mine == hash) return;
            var text = $"Désynchronisation détectée avec {NameOf(playerId)} (point {key}). Les parties ont divergé.";
            DesyncWarning = text;
            Send(new Msg { t = T.Desync, text = text });
            AddLog("<color=#ff6060>" + text + "</color>");
        }

        // ───────────── Divers ─────────────

        public static string ToastText;
        public static float ToastUntil;
        public static void Toast(string s) { ToastText = s; ToastUntil = Time.unscaledTime + 2.5f; }
    }
}
