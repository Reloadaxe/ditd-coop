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
    /// Contrôleur de la coop.
    ///
    /// Modèle « runs parallèles » : chaque joueur joue SA run (personnage, deck, main, énergie, PV, reliques,
    /// récompenses), avec la même seed. On ne partage que :
    ///  - la carte : seul le meneur choisit le chemin, ses clics sur la carte sont rejoués chez les autres,
    ///    et il ne peut avancer que quand tout le monde est revenu sur la carte ;
    ///  - les ennemis : dégâts partagés et correction des PV par l'hôte (Combat) ;
    ///  - le plateau : affichage des dés des autres et cases bloquées (Board) ;
    ///  - les héros : PV et bloc de chacun affichés chez les autres (Heroes) ;
    ///  - la fin de tour : elle a lieu quand tout le monde est prêt.
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
        private static readonly HashSet<int> _onMap = new HashSet<int>();
        private static readonly Dictionary<int, long> _hostHashes = new Dictionary<int, long>();
        private static readonly Dictionary<int, Dictionary<int, long>> _clientHashes = new Dictionary<int, Dictionary<int, long>>();

        // Config, remplie par MultiplayerMod
        public static Func<bool> RotateLeader = () => false;
        public static Func<string[]> LocalOnlyKeywords = () => Array.Empty<string>();

        // ───────────── Rôles ─────────────

        private static int _floorCounter;
        public static void OnFloorStarted() => _floorCounter++;

        /// <summary>Id du meneur (celui qui choisit le chemin) : l'hôte, ou à tour de rôle à chaque étage.</summary>
        public static int LeaderId
        {
            get
            {
                if (!Active || Net.Players.Count == 0) return 0;
                if (!RotateLeader()) return 0;
                var ordered = Net.Players.OrderBy(p => p.Id).ToList();
                return ordered[Math.Abs(_floorCounter) % ordered.Count].Id;
            }
        }

        public static bool IsLeader => !Active || LeaderId == LocalId;

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
            _onMap.Clear();
            Votes.Clear();
            DesyncWarning = null;
            PendingSeed = null;
            _floorCounter = 0;
            Team.Reset();
            Lobby.Reset();
            Heroes.Reset();
            Board.Reset();
        }

        public static void OnRunStarted()
        {
            _floorCounter = 0;
            _ready.Clear();
            _onMap.Clear();
            Votes.Clear();
            DesyncWarning = null;
            Heroes.Reset();
            Board.Reset();
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
                    case T.StartRun: Team.OnStartRun(m); break;
                    case T.Pick: Team.OnRemotePick(m); break;
                    case T.OpenSel: Lobby.OnOpenSelection(); break;
                    case T.CloseSel: Lobby.OnCloseSelection(); break;
                    case T.Click: ReplayClick(m); break;
                    case T.Ready: OnReady(m.id, m.turn, m.flag); break;
                    case T.EndTurn: ExecuteEndTurn(); break;
                    case T.Hash: OnRemoteHash(m); break;
                    case T.Desync: DesyncWarning = m.text; AddLog("<color=#ff6060>" + m.text + "</color>"); break;
                    case T.HeroState: Heroes.OnRemoteState(m); break;
                    case T.BoardPlace: Board.OnRemotePlace(m); break;
                    case T.BoardRemove: Board.OnRemoteRemove(m); break;
                    case T.BoardClear: Board.OnRemoteClear(m); break;
                    case T.EnemyDamage: Combat.OnRemoteDamage(m); break;
                    case T.EnemySync: Combat.OnEnemySync(m); break;
                    case T.OnMap: if (m.flag) _onMap.Add(m.id); else _onMap.Remove(m.id); TryResolveVote(); break;
                    case T.Vote: OnVote(m); break;
                    case T.MapGo: ExecuteMapGo(m); break;
                }
            }
            catch (Exception e)
            {
                AddLog($"Erreur en traitant « {m.t} » de {NameOf(m.id)} : {e.Message}");
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

        /// <summary>Lance GameManager.StartRun sans que nos patchs l'interceptent.</summary>
        public static void StartRunReplaying(RunInfo.NewRunData data, ActiveCustomRun custom)
        {
            var gm = Game.Manager;
            if (gm == null) { AddLog("GameManager introuvable : impossible de lancer la run."); return; }
            WithReplay(() => gm.StartRun(data, custom, true));
        }

        /// <summary>Appelé après SeedSystem.StartRun : force la seed commune, chez tous (hôte compris).</summary>
        public static void ApplyPendingSeed(SeedSystem seedSystem)
        {
            if (PendingSeed == null || seedSystem == null) return;
            int s = PendingSeed.Value;
            PendingSeed = null;
            var m = typeof(SeedSystem).GetMethod(nameof(SeedSystem.ForceSeed));
            m.Invoke(seedSystem, new object[] { s });

            // ForceSeed n'écrit que le champ seed. SeedSystem.StartRun a déjà créé ses 4 générateurs statiques à partir
            // de SA seed : on les recrée comme le jeu le fait (Random(seed + étape)).
            try
            {
                SeedSystem.DiceRNG = SeedSystem.Rng.GetRandomRng(SeedSystem.Rng.Context.Dice);
                SeedSystem.RelicRNG = SeedSystem.Rng.GetRandomRng(SeedSystem.Rng.Context.Relic);
                SeedSystem.RewardRNG = SeedSystem.Rng.GetRandomRng(SeedSystem.Rng.Context.Reward);
                SeedSystem.PotionRNG = SeedSystem.Rng.GetRandomRng(SeedSystem.Rng.Context.Potion);
            }
            catch (Exception e) { AddLog("Générateurs aléatoires non resynchronisés : " + e.Message); }
            AddLog($"Seed commune appliquée : {s}");
        }

        // ───────────── Carte : chemin choisi par le meneur ─────────────

        /// <summary>Signale aux autres que je suis sur la carte (fin de mon étage : récompenses, événement, boutique terminés).</summary>
        public static void SetOnMap(bool onMap)
        {
            if (onMap) _onMap.Add(LocalId); else _onMap.Remove(LocalId);
            if (!onMap) Votes.Clear();
            Send(new Msg { t = T.OnMap, flag = onMap });
            TryResolveVote();
        }

        public static IEnumerable<int> NotOnMap => Net?.Players.Where(p => !_onMap.Contains(p.Id)).Select(p => p.Id) ?? Enumerable.Empty<int>();

        /// <summary>Patch préfixe posé sur les OnPointerClick du jeu et de l'UI Unity. Renvoie false pour bloquer.</summary>
        public static bool OnLocalClick(Il2CppObjectBase instance, PointerEventData data, MethodBase original)
        {
            if (!Active || Replaying) return true;
            if (!Game.InRun)                            // lobby et sélection de personnage : chacun clique chez soi
            {
                if (Lobby.ClientWaiting) { Toast("En attente de l'hôte…"); return false; }
                return true;
            }
            // Hors de la carte, chacun joue SA run : rien n'est répliqué
            if (Game.State != GameManager.GameState.Map) return true;
            if (ClickDepth++ > 0) return true;          // appel imbriqué (base.OnPointerClick) : déjà traité

            var comp = instance.TryCast<Component>();
            if (comp == null) return true;
            // Seule la navigation (cases et flèches de chemin) est partagée : récompenses, sac, etc. restent locaux
            var typeName = Game.Safe(() => comp.GetIl2CppType().Name) ?? "";
            if (!MapNavigationTypes.Contains(typeName)) return true;
            var go = comp.gameObject;
            string path = GoPath.Of(go);

            // Vote : le clic ne déplace personne, il enregistre le choix du joueur
            var pos = data != null ? data.position : Vector2.zero;
            var vote = new Msg
            {
                t = T.Vote,
                path = path,
                comp = Game.Safe(() => comp.GetIl2CppType().FullName) ?? original.DeclaringType?.FullName,
                method = "OnPointerClick",
                btn = data != null ? (int)data.button : 0,
                x = pos.x / Math.Max(1, Screen.width),
                y = pos.y / Math.Max(1, Screen.height),
            };
            ClickDepth = 0;
            Send(vote);
            vote.id = LocalId;
            OnVote(vote);
            return false;
        }

        private static readonly HashSet<string> MapNavigationTypes = new HashSet<string> { "VisualMapTile", "MapTileArrow" };

        // ───────────── Vote du chemin ─────────────

        /// <summary>Vote de chaque joueur (id → clic sur une case).</summary>
        public static readonly Dictionary<int, Msg> Votes = new Dictionary<int, Msg>();

        private static void OnVote(Msg m)
        {
            Votes[m.id] = m;
            AddLog($"{NameOf(m.id)} vote pour une case ({Votes.Count}/{Net.Players.Count}).");
            if (IsHost) TryResolveVote();
        }

        /// <summary>Appelé aussi quand un joueur revient sur la carte : le vote se résout dès que tout le monde est là et a voté.</summary>
        public static void TryResolveVote()
        {
            if (!Active || !IsHost || Votes.Count == 0) return;
            if (NotOnMap.Any() || Net.Players.Any(p => !Votes.ContainsKey(p.Id))) return;

            // Majorité ; en cas d'égalité, le choix de l'hôte s'il en fait partie, sinon le premier dans l'ordre des joueurs
            var groups = Votes.Values.GroupBy(v => v.path).ToList();
            int best = groups.Max(g => g.Count());
            var top = groups.Where(g => g.Count() == best).ToList();
            var winner = top.FirstOrDefault(g => g.Any(v => v.id == LocalId))
                         ?? top.OrderBy(g => g.Min(v => v.id)).First();
            var choice = winner.First();
            AddLog($"Chemin choisi ({best} voix sur {Votes.Count}).");
            var go = new Msg { t = T.MapGo, path = choice.path, comp = choice.comp, method = choice.method, btn = choice.btn, x = choice.x, y = choice.y };
            Send(go);
            ExecuteMapGo(go);
        }

        private static void ExecuteMapGo(Msg m)
        {
            Votes.Clear();
            ReplayClick(m);
        }

        /// <summary>Affiche les votes au-dessus des cases (OnGUI, phase Repaint).</summary>
        public static void DrawVotes()
        {
            if (!Active || Votes.Count == 0 || Game.State != GameManager.GameState.Map) return;
            foreach (var grp in Votes.Values.GroupBy(v => v.path))
            {
                var go = GoPath.Find(grp.Key);
                if (go == null) continue;
                var sp = Board.ScreenPos(go.transform);
                if (sp == null) continue;
                var names = string.Join(" ", grp.Select(v => $"<color={ColorOf(v.id)}>●{NameOf(v.id)}</color>"));
                GUI.Box(new Rect(sp.Value.x - 60, Screen.height - sp.Value.y - 52, 120, 24), "");
                GUI.Label(new Rect(sp.Value.x - 56, Screen.height - sp.Value.y - 50, 116, 22), $"<b>{names}</b>");
            }
            var missing = Net.Players.Where(p => !Votes.ContainsKey(p.Id)).Select(p => p.Name).ToList();
            var away = NotOnMap.Select(NameOf).ToList();
            string status = away.Count > 0 ? "En attente de " + string.Join(", ", away) + " (pas encore sur la carte)"
                          : missing.Count > 0 ? "Votes manquants : " + string.Join(", ", missing) : "";
            if (status != "") GUI.Box(new Rect(Screen.width / 2f - 220, Screen.height - 46, 440, 28), status);
        }

        public static void EndLocalClick() { if (ClickDepth > 0) ClickDepth--; }

        private static readonly Dictionary<string, Type> _typeCache = new Dictionary<string, Type>();

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
            if (Game.State != GameManager.GameState.Map) { AddLog($"Clic de carte de {NameOf(m.id)} ignoré : tu n'es pas sur la carte."); return; }
            var go = GoPath.Find(m.path);
            if (go == null) { AddLog($"Clic de {NameOf(m.id)} introuvable : {m.path}"); return; }
            var type = ResolveManagedType(m.comp);
            if (type == null) { AddLog($"Type inconnu : {m.comp}"); return; }
            var comp = go.GetComponent(Il2CppInterop.Runtime.Il2CppType.From(type));
            if (comp == null) { AddLog($"Composant {type.Name} absent sur {go.name}"); return; }
            var wrapper = typeof(Il2CppObjectBase).GetMethod(nameof(Il2CppObjectBase.TryCast)).MakeGenericMethod(type).Invoke(comp, null);
            var method = type.GetMethod(m.method, new[] { typeof(PointerEventData) });
            if (wrapper == null || method == null) return;
            var ped = new PointerEventData(EventSystem.current)
            {
                button = (PointerEventData.InputButton)m.btn,
                position = new Vector2(m.x * Screen.width, m.y * Screen.height),
                pointerPress = go, rawPointerPress = go, pointerEnter = go, clickCount = 1,
            };
            WithReplay(() => method.Invoke(wrapper, new object[] { ped }));
        }

        // ───────────── Fin de tour collective ─────────────

        public static IReadOnlyCollection<int> ReadyPlayers => _ready;
        private static bool _executingEndTurn;

        /// <summary>Préfixe de FinishTurnUI.OnClickFinishTurnButton : déclare « prêt ». Le tour ne finit que quand tout le monde l'est.</summary>
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

        public static void ClearReady() => _ready.Clear();

        private static void OnReady(int id, int turn, bool ready)
        {
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

        // ───────────── Détection de désynchronisation (ennemis uniquement) ─────────────

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
            var text = $"Ennemis différents chez {NameOf(playerId)} (tour {key % 1000}) : correction envoyée.";
            AddLog("<color=#ffb060>" + text + "</color>");
            Combat.HostBroadcastEnemies();
        }

        // ───────────── Divers ─────────────

        public static string ToastText;
        public static float ToastUntil;
        public static void Toast(string s) { ToastText = s; ToastUntil = Time.unscaledTime + 2.5f; }
    }
}
