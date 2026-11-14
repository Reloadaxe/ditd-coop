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
        /// <summary>Rang du joueur local parmi les joueurs triés par id (0 = hôte).</summary>
        public static int LocalRank => Net == null ? 0 : Net.Players.OrderBy(p => p.Id).ToList().FindIndex(p => p.Id == LocalId);

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
                    case T.EnemyMod: Combat.OnRemoteModifier(m); break;
                    case T.Preview: Preview.OnRemote(m); break;
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

        /// <summary>
        /// Patch préfixe posé sur les OnPointerClick du jeu et de l'UI Unity. En coop, il ne sert plus qu'au lobby
        /// (écran d'attente du client). En run, chacun clique chez soi : le chemin passe par le vote (Map.ClickOnMapTile).
        /// </summary>
        /// <summary>Diagnostic : type et nom du dernier objet cliqué (pour savoir par où passe le choix du chemin).</summary>
        private static string _lastClickInfo = "—";

        public static bool OnLocalClick(Il2CppObjectBase instance, PointerEventData data, MethodBase original)
        {
            // Interface du mod : ses propres boutons passent ; un clic du jeu sous une fenêtre du mod est bloqué
            try
            {
                var hc = instance.TryCast<Component>();
                if (hc != null && Ui.GameUi.Owns(hc)) return true;
                if (data != null && Ui.Hud.BlocksPointer(data.position)) return false;
            }
            catch { }
            if (!Active || Replaying) return true;
            try
            {
                var c = instance.TryCast<Component>();
                if (c != null)
                {
                    var typeName = Game.Safe(() => c.GetIl2CppType().Name) ?? "";
                    _lastClickInfo = $"{typeName} « {c.gameObject.name} »";
                    if (MapClickTypes.Contains(typeName) && Game.InRun)
                    {
                        _lastMapClickTime = Time.unscaledTime;
                        // Départ bloqué en attente d'un choix : la case survolée au moment de ce clic devient le vote
                        if (VotingOpen)
                        {
                            var h = HoveredCoord(Game.Map);
                            if (h != null && IsOption(h.Value)) CastVote(h.Value);
                        }
                    }
                }
            }
            catch { }
            if (!Game.InRun && Lobby.ClientWaiting) { Toast("En attente de l'hôte..."); return false; }
            return true;
        }

        // ───────────── Vote du chemin ─────────────

        /// <summary>Vote de chaque joueur (id → coordonnées de la case, dans sx/sy).</summary>
        public static readonly Dictionary<int, Msg> Votes = new Dictionary<int, Msg>();

        /// <summary>Dernière case cliquée localement (le clic reste local : infos de la case, sélection du chemin).</summary>
        private static Vector2Int? _lastClicked;
        /// <summary>Déplacement autorisé (vote résolu) jusqu'à cet instant.</summary>
        private static float _allowMoveUntil;

        /// <summary>Préfixe de Map.ClickOnMapTile : on laisse le jeu faire (infos, sélection), on retient juste la case.</summary>
        public static bool OnMapTileClick(VisualMapTile tile)
        {
            if (!Active || tile == null) return true;
            if (Replaying) return true;
            var coord = Game.Safe(() => tile.ActiveTile.CoordVector);
            _lastClicked = coord;
            _lastMapClickTime = Time.unscaledTime;
            // Départ déjà bloqué faute de choix : ce clic est le vote
            if (VotingOpen && IsOption(coord)) { CastVote(coord); return false; }   // départ bloqué ou vote en cours : ce clic est (ou change) mon vote
            return true;
        }

        /// <summary>
        /// Préfixe de Map.ChangeState : le départ (MovePlayer) est le vrai moment du choix. Sans vote résolu, on le bloque
        /// et on vote pour la case retenue. Renvoie false pour bloquer.
        /// </summary>
        /// <summary>
        /// Préfixe de Map.EnterMovePlayerState : certains départs du jeu (sélection automatique, suite d'un déplacement…)
        /// ne passent pas par ChangeState. Même contrôle ici ; si l'on bloque à ce stade, la carte est déjà en état
        /// « déplacement » : le départ voté relancera directement EnterMovePlayerState (voir ExecuteMapGo).
        /// </summary>
        public static bool OnEnterMovePlayer()
        {
            if (!Active || Time.unscaledTime < _allowMoveUntil) return true;
            bool ok = OnMapChangeState(Map.MapState.MovePlayer);
            if (!ok) { _moveBlockedInside = true; AddLog("[vote] Départ direct du jeu bloqué : en attente du vote."); }
            return ok;
        }

        /// <summary>
        /// Préfixe de Map._MoveForward : un déplacement peut compter plusieurs pas (relique, effet de case…) et traverse
        /// alors les cases sans s'arrêter. Ces effets étant propres à chaque joueur, l'un partait plus loin que les autres.
        /// En coop, chaque départ fait exactement un pas : la case suivante repasse par le vote chez tout le monde.
        /// </summary>
        public static void ClampMove(Map.MoveData move)
        {
            if (!Active || move == null) return;
            int spent = move.movesSpent, total = move.totalAmountOfMoves;
            if (total <= spent + 1) return;
            move.totalAmountOfMoves = spent + 1;
            AddLog($"[vote] Déplacement de {total - spent} pas ramené à 1 (le suivant se vote).");
        }

        /// <summary>Un départ a été bloqué dans EnterMovePlayerState (la carte attend en état « déplacement »).</summary>
        private static bool _moveBlockedInside;

        public static bool OnMapChangeState(Map.MapState next)
        {
            if (!Active) return true;
            // Arrivée en sélection de case : on propose le choix tout de suite (vote auto si un seul chemin)
            if (next == Map.MapState.SelectTile) { _moveBlockedInside = false; Game.Schedule(StartChoice, 0.6f); return true; }
            if (next != Map.MapState.MovePlayer) return true;
            if (Time.unscaledTime < _allowMoveUntil) return true;
            // Destination choisie : dernière case cliquée, sinon le choix enregistré par le jeu (flèches…),
            // sinon la case survolée au moment du clic (sélection par le système Selectable du jeu) — uniquement au début
            // d'un déplacement : en cours de route (pas déjà faits), c'est une suite automatique.
            var map = Game.Map;
            var options = ClickableOptions(map, out var optDetail);
            var clicked = _lastClicked; var desired = ReadDesired(map); var hovered = HoveredCoord(map);
            bool recentMapClick = Time.unscaledTime - _lastMapClickTime < 3f;
            AddLog($"[vote] Départ demandé : chemins possibles={options.Count} ({optDetail}) clic={Fmt(clicked)} destination={Fmt(desired)} " +
                   $"survol={Fmt(hovered)} clic carte récent={recentMapClick} dernier clic={_lastClickInfo}");

            // Aucun chemin connu : on laisse le jeu faire. Un seul chemin : c'est aussi un vote (le clic « continuer » vaut
            // vote pour ce chemin), pour que personne ne parte seul
            if (options.Count == 0) { AddLog("[vote] Aucun chemin connu : départ laissé au jeu."); return true; }
            if (options.Count == 1) { _currentOptions = options; CastVote(options[0], auto: true); return false; }

            // Plusieurs chemins : on bloque, chez tout le monde, jusqu'au vote. La case survolée ne compte que si l'on vient
            // de cliquer sur la carte (sinon c'est un survol ancien, pas un choix).
            _currentOptions = options;
            // Seule une case réellement accessible compte : un survol périmé (ex. clic sur « Continuer » en fin de salle)
            // ne doit pas devenir un vote — sinon « partir » vers la case actuelle relance la même salle
            Vector2Int? target = null;
            foreach (var cand in new[] { clicked, desired, recentMapClick ? hovered : null })
                if (cand != null && IsOption(cand.Value)) { target = cand; break; }
            if (target == null)
            {
                _awaitingChoice = true;
                Toast("Plusieurs chemins : clique sur la case de ton choix pour voter");
                return false;
            }
            CastVote(target.Value);
            return false;
        }

        /// <summary>Attente d'un choix : le départ est bloqué et le prochain clic sur une case devient le vote.</summary>
        private static bool _awaitingChoice;
        private static float _lastMapClickTime = -10f;
        private static readonly HashSet<string> MapClickTypes = new HashSet<string> { "VisualMapTile", "MapTileArrow", "HoverEvents" };

        /// <summary>Cases accessibles au moment où le départ a été bloqué (le vote ne peut porter que sur elles).</summary>
        private static List<Vector2Int> _currentOptions = new List<Vector2Int>();

        private static bool IsOption(Vector2Int c) => _currentOptions.Count == 0 || _currentOptions.Contains(c);

        /// <summary>J'ai voté et le vote n'est pas encore résolu : mes clics sur la carte changent mon vote.</summary>
        private static bool _myVotePending;

        /// <summary>Vrai tant qu'un vote local peut être émis ou changé (départ bloqué en attente, ou vote déjà émis).</summary>
        private static bool VotingOpen => _awaitingChoice || _myVotePending;

        private static void CastVote(Vector2Int c, bool auto = false)
        {
            bool change = _myVotePending;
            _awaitingChoice = false;
            _myVotePending = true;
            var vote = new Msg { t = T.Vote, sx = c.x, sy = c.y };
            Send(vote);
            vote.id = LocalId;
            OnVote(vote);
            Toast(auto ? "Vote enregistré : départ dès que tout le monde a voté" : change ? "Vote changé" : "Vote enregistré : en attente des autres joueurs (re-clique pour changer)");
        }

        /// <summary>Cases accessibles au prochain pas (Map.NextPlayerTiles, privée dans le jeu).</summary>
        /// <summary>
        /// Cases réellement sélectionnables : celles que le jeu marque comme cliquables (VisualMapTile.isClickable,
        /// celles qui « respirent » à l'écran). Repli sur le calcul des cases suivantes si le jeu n'en marque aucune.
        /// </summary>
        private static List<Vector2Int> ClickableOptions(Map map, out string detail)
        {
            // Le marquage « cliquable » du jeu devient périmé quand on bloque puis relance le départ (au 2e choix, les cases
            // du choix précédent rebondissaient encore). On s'appuie donc sur la position réelle du joueur (cases suivantes
            // de sa case = niveau suivant), puis on remet nous-mêmes les visuels en accord (SetClickables).
            var clickable = new List<Vector2Int>();
            foreach (var t in UnityEngine.Object.FindObjectsOfType<VisualMapTile>())
            {
                try { if (t != null && t.isClickable && t.gameObject.activeInHierarchy) clickable.Add(t.ActiveTile.CoordVector); }
                catch { }
            }
            var options = NextOptions(map, out var nextDetail);
            detail = $"jeu (cliquables)=[{string.Join(" ", clickable.Select(c => $"{c.x},{c.y}"))}] {nextDetail}";
            return options.Count > 0 ? options : clickable.Distinct().ToList();
        }

        /// <summary>
        /// Aligne l'animation « case cliquable » (respiration) sur les choix du vote : les cases possibles rebondissent,
        /// avec les raccourcis Option1..3 ; toutes les autres s'arrêtent. Liste vide = plus aucune case cliquable.
        /// </summary>
        private static void SetClickables(List<Vector2Int> options)
        {
            foreach (var t in UnityEngine.Object.FindObjectsOfType<VisualMapTile>())
            {
                try
                {
                    if (t == null) continue;
                    var c = t.ActiveTile.CoordVector;
                    int idx = options.IndexOf(c);
                    bool should = idx >= 0;
                    if (t.isClickable == should && !should) continue;
                    var shortcut = idx == 0 ? TileShortcutOption.Option1 : idx == 1 ? TileShortcutOption.Option2 : idx == 2 ? TileShortcutOption.Option3 : TileShortcutOption.NONE;
                    t.ToggleClickableVisuals(should, should ? shortcut : TileShortcutOption.NONE);
                }
                catch { }
            }
        }

        /// <summary>
        /// Arrivée en sélection de case (Map passe en SelectTile) : un seul chemin => voté automatiquement (tout le monde
        /// part ensemble, sans clic) ; plusieurs chemins => le vote s'ouvre tout de suite chez tous.
        /// </summary>
        private static void StartChoice()
        {
            if (!Active || !Game.InRun) return;
            var map = Game.Map;
            if (map == null || Game.Safe(() => map.CurrentMapState) != Map.MapState.SelectTile) return;
            if (Time.unscaledTime < _allowMoveUntil) return;
            var options = ClickableOptions(map, out var detail);
            _currentOptions = options;
            AddLog($"[vote] Choix du chemin : {options.Count} case(s) possible(s) ({detail})");
            SetClickables(options);
            if (options.Count == 0) return;
            if (options.Count == 1)
            {
                if (!_myVotePending) { _awaitingChoice = true; Toast("Un seul chemin : clique pour voter, vous partirez ensemble"); }
                return;
            }
            if (!_myVotePending) { _awaitingChoice = true; Toast("Plusieurs chemins : clique sur la case de ton choix pour voter"); }
        }

        /// <summary>
        /// Cases où l'on peut aller au prochain pas : union de Map.NextPlayerTiles(1) et des cases suivantes de la case
        /// où se trouve réellement le joueur (CurrentPlayerTile.ActiveTile.NextTilesCoords). Après une salle, la première
        /// source pouvait n'en voir qu'une alors qu'il y avait un vrai choix.
        /// </summary>
        private static List<Vector2Int> NextOptions(Map map, out string detail)
        {
            var fromNext = new List<Vector2Int>();
            var fromTile = new List<Vector2Int>();
            Vector2Int? here = null;
            try
            {
                var tiles = map?.NextPlayerTiles(1);
                if (tiles != null) foreach (var t in tiles) if (t != null) fromNext.Add(t.ActiveTile.CoordVector);
            }
            catch { }
            try
            {
                var cur = map?.CurrentPlayerTile;
                if (cur != null)
                {
                    here = cur.ActiveTile.CoordVector;
                    var next = cur.ActiveTile.NextTilesCoords;
                    if (next != null) foreach (var c in next) fromTile.Add(c);
                }
            }
            catch { }
            detail = $"ici={Fmt(here)} prochain pas=[{string.Join(" ", fromNext.Select(c => $"{c.x},{c.y}"))}] suivantes de la case=[{string.Join(" ", fromTile.Select(c => $"{c.x},{c.y}"))}]";
            return fromNext.Concat(fromTile).Distinct().ToList();
        }

        /// <summary>Clé d'un vote : chemin de la flèche choisie, sinon coordonnées de la case.</summary>
        private static readonly System.Random _voteRng = new System.Random();

        private static string Key(Msg v) => !string.IsNullOrEmpty(v.path) ? "flèche:" + v.path : $"{v.sx},{v.sy}";

        private static void OnVote(Msg m)
        {
            // Annonce du compte à rebours par l'hôte (pas un vote)
            if (m.sx == int.MinValue) { ResolveAt = Time.unscaledTime + m.turn / 1000f; return; }
            Votes[m.id] = m;
            AddLog($"{NameOf(m.id)} vote pour la case {Key(m)} ({Votes.Count}/{Net.Players.Count}).");
            if (IsHost) TryResolveVote();
        }

        /// <summary>Appelé aussi quand un joueur revient sur la carte : le vote se résout dès que tout le monde est là et a voté.</summary>
        /// <summary>Quand tout le monde a voté, le départ attend ce délai : on peut encore changer d'avis (chaque changement le relance).</summary>
        private const float ResolveDelay = 2f;
        private static int _voteVersion;
        public static float ResolveAt = -1f;

        public static void TryResolveVote()
        {
            if (!Active || !IsHost || Votes.Count == 0) { ResolveAt = -1f; return; }
            // On vote là où le jeu propose le choix (carte, ou juste après un combat) : seul compte que tout le monde ait voté
            if (Net.Players.Any(p => !Votes.ContainsKey(p.Id))) { ResolveAt = -1f; return; }
            int version = ++_voteVersion;
            ResolveAt = Time.unscaledTime + ResolveDelay;
            Send(new Msg { t = T.Vote, id = LocalId, sx = int.MinValue, turn = (int)(ResolveDelay * 1000) });   // annonce du compte à rebours
            Game.Schedule(() => { if (version == _voteVersion) ResolveNow(); }, ResolveDelay);
        }

        private static void ResolveNow()
        {
            ResolveAt = -1f;
            if (!Active || !IsHost || Votes.Count == 0 || Net.Players.Any(p => !Votes.ContainsKey(p.Id))) return;
            // Votes pour une case non accessible (survol périmé chez un joueur) : écartés, on attend un vrai choix
            var invalid = Votes.Values.Where(v => string.IsNullOrEmpty(v.path) && !IsOption(new Vector2Int(v.sx, v.sy))).Select(v => v.id).ToList();
            if (invalid.Count > 0)
            {
                foreach (var id in invalid) Votes.Remove(id);
                AddLog($"[vote] Vote(s) pour une case non accessible écarté(s) : {string.Join(", ", invalid.Select(NameOf))}.");
                return;
            }

            // Majorité ; en cas d'égalité, le choix de l'hôte s'il en fait partie, sinon celui du plus petit id
            var groups = Votes.Values.GroupBy(Key).ToList();
            int best = groups.Max(g => g.Count());
            var top = groups.Where(g => g.Count() == best).ToList();
            // Égalité : tirage au sort par l'hôte, puis résultat envoyé à tous (tout le monde part vers la même case)
            var winner = top.Count == 1 ? top[0] : top[_voteRng.Next(top.Count)];
            var choice = winner.First();
            AddLog(top.Count == 1
                ? $"Chemin choisi : {Key(choice)} ({best} voix sur {Votes.Count})."
                : $"Égalité ({best} voix chacune) : tirage au sort entre {top.Count} chemins → {Key(choice)}.");
            var go = new Msg { t = T.MapGo, sx = choice.sx, sy = choice.sy, path = choice.path };
            Send(go);
            ExecuteMapGo(go);
        }

        private static void ExecuteMapGo(Msg m)
        {
            Votes.Clear();
            _awaitingChoice = false;
            _myVotePending = false;
            ResolveAt = -1f;
            // Vote pour une flèche : on rejoue le clic du joueur sur cette flèche, chez tous
            if (!string.IsNullOrEmpty(m.path))
            {
                var arrowGo = GoPath.Find(m.path);
                var arrow = arrowGo != null ? arrowGo.GetComponent<MapTileArrow>() : null;
                if (arrow == null) { AddLog($"[vote] Flèche introuvable chez toi : {m.path}"); return; }
                AddLog("[vote] Départ par la flèche choisie.");
                _allowMoveUntil = Time.unscaledTime + 3f;
                _lastClicked = null;
                var ped = new PointerEventData(EventSystem.current) { button = PointerEventData.InputButton.Left, pointerPress = arrowGo, rawPointerPress = arrowGo, pointerEnter = arrowGo, clickCount = 1 };
                WithReplay(() => arrow.OnPointerClick(ped));
                return;
            }
            // Jamais de « départ » vers la case où l'on est déjà : cela relancerait la même salle
            var here = Game.Safe(() => Game.Map?.CurrentPlayerTile != null ? Game.Map.CurrentPlayerTile.ActiveTile.CoordVector : new Vector2Int(-999, -999));
            if (here.x == m.sx && here.y == m.sy) { AddLog($"[vote] Case {m.sx},{m.sy} = case actuelle : départ ignoré."); _awaitingChoice = true; return; }
            _currentOptions = new List<Vector2Int>();
            var tile = FindTile(m.sx, m.sy);
            var map = Game.Map;
            if (tile == null || map == null) { AddLog($"[vote] Case {m.sx},{m.sy} introuvable sur ta carte."); return; }
            AddLog($"[vote] Départ vers la case {m.sx},{m.sy}.");
            Game.Schedule(() => SetClickables(new List<Vector2Int>()), 0.6f);
            _allowMoveUntil = Time.unscaledTime + 3f;
            _lastClicked = null;
            // La case gagnante devient le choix du jeu partout où il peut le lire (survol, curseur, destination),
            // comme si le joueur l'avait choisie, puis départ si le jeu ne l'a pas déjà lancé
            var coord = new Vector2Int(m.sx, m.sy);
            Game.Safe(() => { map.lastHoveredTile = tile; return 0; });
            Game.Safe(() => { map.navigationCurrentCoord = coord; return 0; });
            WriteDesired(map, coord);
            if (_moveBlockedInside)
            {
                // La carte attend déjà en état « déplacement » (départ direct bloqué) : on relance le déplacement lui-même
                _moveBlockedInside = false;
                AddLog("[vote] Reprise du déplacement bloqué vers la case choisie.");
                WithReplay(() => map.ClickOnMapTile(tile));
                WriteDesired(map, coord);
                WithReplay(() => map.EnterMovePlayerState());
                return;
            }
            WithReplay(() => map.ClickOnMapTile(tile));
            WriteDesired(map, coord);
            Game.Schedule(() =>
            {
                var st = Game.Safe(() => map.CurrentMapState);
                if (st == Map.MapState.SelectTile) Game.Safe(() => { map.ChangeState(Map.MapState.MovePlayer); return 0; });
            }, 0.3f);
            // Filet de sécurité : si le déplacement n'a pas démarré (carte restée en sélection, départ du jeu à moitié
            // engagé…), on le relance une fois, avec une trace de l'état pour le diagnostic
            Game.Schedule(() => EnsureMoveStarted(map, tile, coord, here), 1.5f);
        }

        private static void EnsureMoveStarted(Map map, VisualMapTile tile, Vector2Int target, Vector2Int start)
        {
            if (!Active || map == null || Game.State != GameManager.GameState.Map) return;
            var st = Game.Safe(() => map.CurrentMapState);
            var at = Game.Safe(() => map.CurrentPlayerTile != null ? map.CurrentPlayerTile.ActiveTile.CoordVector : new Vector2Int(-999, -999));
            bool moving = st == Map.MapState.MovePlayer || st == Map.MapState.TileAction || st == Map.MapState.AfterTileFinish || at != start;
            AddLog($"[vote] Contrôle du départ : carte={st} case={at.x},{at.y} (départ {start.x},{start.y}, cible {target.x},{target.y}){(moving ? "" : " : déplacement relancé")}");
            if (moving) return;
            _allowMoveUntil = Time.unscaledTime + 3f;
            Game.Safe(() => { map.lastHoveredTile = tile; return 0; });
            WriteDesired(map, target);
            WithReplay(() => map.ClickOnMapTile(tile));
            WriteDesired(map, target);
            Game.Safe(() => { map.ChangeState(Map.MapState.MovePlayer); return 0; });
        }

        /// <summary>
        /// Préfixe de MapTileArrow.OnPointerClick : l'action de la flèche lance le départ directement, sans écrire de destination
        /// lisible. On bloque donc le clic et on vote pour CETTE flèche (repérée par son chemin dans la scène, identique chez tous).
        /// À la résolution, le clic sur la flèche gagnante est rejoué chez tout le monde. Renvoie false pour bloquer.
        /// </summary>
        public static bool OnArrowClick(MapTileArrow arrow)
        {
            if (!Active || Replaying || arrow == null || Time.unscaledTime < _allowMoveUntil) return true;
            var path = GoPath.Of(arrow.gameObject);
            var parentTile = Game.Safe(() => arrow.GetComponentInParent<VisualMapTile>());
            var coord = parentTile != null ? Game.Safe(() => parentTile.ActiveTile.CoordVector) : new Vector2Int(-999, -999);
            _awaitingChoice = false;
            _myVotePending = true;
            var vote = new Msg { t = T.Vote, path = path, sx = coord.x, sy = coord.y };
            Send(vote);
            vote.id = LocalId;
            OnVote(vote);
            Toast("Vote enregistré : en attente des autres joueurs");
            return false;
        }

        private static string Fmt(Vector2Int? v) => v == null ? "—" : $"{v.Value.x},{v.Value.y}";

        /// <summary>Pas déjà faits dans le déplacement en cours (0 = on n'est pas encore parti).</summary>
        private static int MovesSpent(Map map) => Game.Safe(() => map.currentMoveData != null ? map.currentMoveData.MovesSpent : 0);

        /// <summary>Case survolée en dernier (champ privé Map.lastHoveredTile).</summary>
        private static Vector2Int? HoveredCoord(Map map)
        {
            var t = Game.Safe(() => map?.lastHoveredTile);
            if (t == null) return null;
            try { return t.ActiveTile.CoordVector; } catch { return null; }
        }

        private static void ClearDesired(Map map)
        {
            try { map.desiredNextPlayerCoord = new Il2CppSystem.Nullable<Vector2Int>(); }
            catch (Exception e) { AddLog("[vote] Effacement de la destination impossible : " + e.Message); }
        }

        /// <summary>Champ privé Map.desiredNextPlayerCoord (Nullable&lt;Vector2Int&gt;) : la destination choisie par le joueur.</summary>
        private static Vector2Int? ReadDesired(Map map)
        {
            if (map == null) return null;
            try
            {
                var n = map.desiredNextPlayerCoord;
                return n.HasValue ? n.Value : (Vector2Int?)null;
            }
            catch { return null; }
        }

        private static void WriteDesired(Map map, Vector2Int coord)
        {
            if (map == null) return;
            try { map.desiredNextPlayerCoord = new Il2CppSystem.Nullable<Vector2Int>(coord); }
            catch (Exception e) { AddLog("[vote] Écriture de la destination impossible : " + e.Message); }
        }

        private static VisualMapTile FindTile(int x, int y)
        {
            foreach (var t in UnityEngine.Object.FindObjectsOfType<VisualMapTile>())
            {
                var c = Game.Safe(() => t.ActiveTile.CoordVector);
                if (c.x == x && c.y == y) return t;
            }
            return null;
        }

        /// <summary>Votes à afficher : position écran de la case (ou flèche) votée et les votants (ids triés).</summary>
        public static List<(Vector2 screen, List<int> voters)> VoteMarkers()
        {
            var res = new List<(Vector2, List<int>)>();
            if (!Active || !Game.InRun || Votes.Count == 0) return res;
            foreach (var grp in Votes.Values.GroupBy(Key))
            {
                var first = grp.First();
                Transform anchor = null;
                if (!string.IsNullOrEmpty(first.path)) anchor = GoPath.Find(first.path)?.transform;   // vote pour une flèche
                if (anchor == null) anchor = FindTile(first.sx, first.sy)?.transform;                // vote pour une case
                if (anchor == null) continue;
                var sp = Board.ScreenPos(anchor);
                if (sp == null) continue;
                res.Add((sp.Value, grp.Select(v => v.id).OrderBy(i => i).ToList()));
            }
            return res;
        }

        /// <summary>Texte d'état du vote (null s'il n'y a rien à afficher).</summary>
        public static string VoteStatus()
        {
            if (!Active || !Game.InRun || (Votes.Count == 0 && !_awaitingChoice)) return null;
            var missing = Net.Players.Where(p => !Votes.ContainsKey(p.Id)).Select(p => $"<color={p.Color}>{p.Name}</color>").ToList();
            float left = ResolveAt - Time.unscaledTime;
            if (left > 0) return $"Tout le monde a voté : départ dans {Math.Ceiling(left):0} s\n<color=#e9cfae>Clique sur une autre case pour changer ton vote</color>";
            if (Votes.Count == 0)
                return _currentOptions.Count == 1 ? "Un seul chemin : clique dessus pour voter, vous partirez ensemble"
                                                  : "Choisissez le chemin : clique sur une case pour voter";
            return missing.Count > 0 ? "En attente du vote de " + string.Join(", ", missing) : null;
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
            Toast(nowReady ? "Prêt ! En attente des autres joueurs" : "Tu n'es plus prêt");
            return false;
        }

        public static void ClearReady() => _ready.Clear();

        // ───── Retour visuel sur le bouton de fin de tour ─────

        private static string _btnOriginalText;
        private static Color? _btnOriginalColor;
        private static bool _btnShowingReady;

        /// <summary>
        /// Chaque frame en combat : tant que je suis prêt, le bouton « Fin de tour » est à ma couleur et affiche
        /// « Prêt ✔ (x/n) » ; sinon il retrouve son texte et sa couleur d'origine.
        /// </summary>
        public static void UpdateReadyVisual()
        {
            if (!Active || !Game.InBattle) { _btnShowingReady = false; _btnOriginalText = null; _btnOriginalColor = null; return; }
            var ui = UnityEngine.Object.FindObjectOfType<FinishTurnUI>();
            if (ui == null) return;
            var texts = Game.Safe(() => ui.finishTurnText)?.GetComponentsInChildren<TMPro.TMP_Text>(true);
            var img = Game.Safe(() => ui.finishTurnImage);
            bool ready = _ready.Contains(LocalId);
            try
            {
                if (ready)
                {
                    if (!_btnShowingReady)
                    {
                        if (texts != null && texts.Length > 0) _btnOriginalText = texts[0].text;
                        if (img != null) _btnOriginalColor = img.color;
                        _btnShowingReady = true;
                    }
                    var label = $"Prêt {_ready.Count}/{Net.Players.Count}";
                    if (texts != null) foreach (var t in texts) if (t.text != label) t.text = label;
                    if (img != null)
                    {
                        var c = Board.ParseHex(ColorOf(LocalId));
                        img.color = Color.Lerp(Color.white, c, 0.75f);
                    }
                }
                else if (_btnShowingReady)
                {
                    if (texts != null && _btnOriginalText != null) foreach (var t in texts) t.text = _btnOriginalText;
                    if (img != null && _btnOriginalColor != null) img.color = _btnOriginalColor.Value;
                    _btnShowingReady = false;
                }
            }
            catch { }
        }

        /// <summary>Texte du bandeau « prêt / en attente » (null s'il n'y a rien à afficher).</summary>
        public static string ReadyStatus()
        {
            if (!Active || !Game.InBattle || _ready.Count == 0) return null;
            var waiting = Net.Players.Where(p => !_ready.Contains(p.Id)).Select(p => $"<color={p.Color}>{p.Name}</color>").ToList();
            return _ready.Contains(LocalId)
                ? (waiting.Count > 0 ? $"Tu es prêt : en attente de {string.Join(", ", waiting)}\n<color=#e9cfae>Clique à nouveau sur Fin de tour pour annuler</color>" : "Tout le monde est prêt !")
                : $"{string.Join(", ", _ready.Select(id => $"<color={ColorOf(id)}>{NameOf(id)}</color>"))} {(_ready.Count > 1 ? "sont prêts" : "est prêt")} : à toi de finir ton tour";
        }

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
