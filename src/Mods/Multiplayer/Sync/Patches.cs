using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Reflection;
using DiceLoader;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes;
using UnityEngine.EventSystems;

namespace DiceMultiplayer.Sync
{
    /// <summary>Pose tous les patchs Harmony du multijoueur, chacun isolé.</summary>
    public static class Patches
    {
        private static int _ok, _total;

        public static void Apply(Harmony h)
        {
            _ok = _total = 0;

            // 1) Tous les OnPointerClick(PointerEventData) du jeu et de l'UI Unity → réplication générique des clics
            var asms = new[] { typeof(GameManager).Assembly, typeof(UnityEngine.UI.Button).Assembly };
            var pre = new HarmonyMethod(typeof(Patches), nameof(ClickPrefix));
            var post = new HarmonyMethod(typeof(Patches), nameof(ClickPostfix));
            int clicks = 0, folded = 0;
            // Le compilateur IL2CPP fusionne les fonctions natives identiques : plusieurs OnPointerClick peuvent
            // partager la même adresse. Patcher deux fois la même adresse fait planter le jeu, d'où le dédoublonnage.
            // Une adresse partagée par plusieurs types n'est jamais patchée : l'interop envelopperait
            // l'objet dans le mauvais type et le trampoline lèverait une NullReferenceException.
            var byAddress = new Dictionary<IntPtr, List<(Type t, MethodInfo m)>>();
            var trace = Path.Combine(MultiplayerMod.Instance.Directory, "patch-trace.log");
            using var tw = new StreamWriter(trace, false) { AutoFlush = true };
            foreach (var asm in asms)
            {
                Type[] types;
                try { types = asm.GetTypes(); } catch (ReflectionTypeLoadException e) { types = e.Types.Where(t => t != null).ToArray(); }
                foreach (var t in types)
                {
                    if (t.IsInterface || t.IsAbstract || t.IsGenericTypeDefinition || t.ContainsGenericParameters || t.Name.Contains("<")) continue;
                    // Abstraits : l'interop ne sait pas construire leur wrapper (NullReferenceException dans le trampoline).
                    // Interfaces IL2CPP : générées comme des classes, on les reconnaît via Il2CppType.
                    if (Game_IsIl2CppInterface(t) || ExcludedTypes.Contains(t.FullName) || (t.Namespace ?? "").StartsWith("Rewired")) continue;
                    MethodInfo m;
                    try { m = t.GetMethod("OnPointerClick", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, new[] { typeof(PointerEventData) }, null); }
                    catch { continue; }
                    if (m == null || m.IsAbstract) continue;

                    var native = NativePointer(m);
                    if (native == IntPtr.Zero) { tw.WriteLine($"SKIP (pas de pointeur) {t.FullName}"); continue; }
                    if (!byAddress.TryGetValue(native, out var list)) byAddress[native] = list = new List<(Type, MethodInfo)>();
                    list.Add((t, m));
                }
            }
            foreach (var kv in byAddress)
            {
                if (kv.Value.Count > 1)
                {
                    folded += kv.Value.Count;
                    tw.WriteLine($"FOLD @ {kv.Key.ToInt64():X} : {string.Join(", ", kv.Value.Select(x => x.t.FullName))}");
                    continue;
                }
                var (t, m) = kv.Value[0];
                tw.WriteLine($"PATCH {t.FullName} @ {kv.Key.ToInt64():X}");
                try { h.Patch(m, prefix: pre, postfix: post); clicks++; }
                catch (Exception e) { tw.WriteLine($"  ERREUR {e.Message}"); }
            }
            tw.WriteLine("FIN");
            MultiplayerMod.Instance.Logger.LogInfo($"Réplication des clics : {clicks} méthodes interceptées ({folded} fusionnées ignorées)");


            // 2) Fin de tour collective
            P(h, typeof(FinishTurnUI), nameof(FinishTurnUI.OnClickFinishTurnButton), nameof(FinishTurnPrefix));

            // 3) Plateau partagé : mes poses et retraits partent aux autres, les cases des autres me sont bloquées
            P(h, typeof(BattleBoard), nameof(BattleBoard.PlaceDiceOnBoard), nameof(PlaceDicePrefix));
            P(h, typeof(ReturnDiceToHand), nameof(ReturnDiceToHand.ExecuteReturnDiceToHand), nameof(ReturnDicePrefix));
            // Dés des autres posés en vrai dans mes cases : leurs actions ne comptent pas chez moi, et ils ne vont pas dans ma défausse
            P(h, typeof(BoardSlot), nameof(BoardSlot.GetBattleActions), nameof(GhostActionsPostfix), postfix: true);
            // Case occupée par le dé d'un autre joueur : refusée en amont (sinon le dé lâché restait à moitié posé)
            P(h, typeof(BoardSlot), "CanPlaceDice", nameof(CanPlaceDicePostfix), postfix: true);
            P(h, typeof(BattleBoard), nameof(BattleBoard.DiscardAllDice), nameof(DiscardAllPrefix));
            P(h, typeof(HandUI), nameof(HandUI.FloorEnd), nameof(HandFloorEndPrefix));
            // Les dés des autres sont verrouillés : ni clic, ni sélection, ni déplacement
            foreach (var mn in new[] { "OnDicePointerDown", "OnDicePointerClick", "OnCustomPointerClick", "OnLeftClick", "OnRightClick" })
                P(h, typeof(GameObjectDice), mn, nameof(GhostDiceInputPrefix));
            foreach (var m in typeof(BattleBoard).GetMethods().Where(x => x.Name == nameof(BattleBoard.SelectDiceFromBoard)))
            {
                _total++;
                try { h.Patch(m, prefix: new HarmonyMethod(typeof(Patches), nameof(GhostSelectPrefix))); _ok++; }
                catch (Exception e) { MultiplayerMod.Instance.Logger.LogWarning("Patch SelectDiceFromBoard : " + e.Message); }
            }

            // 4) Ennemis partagés : mes dégâts sont envoyés aux autres
            P(h, typeof(Enemy), "_Damage", nameof(EnemyDamagePostfix), postfix: true);
            // Mort en coop : K.O. (spectateur) puis retour à 1 PV, tant qu'un autre joueur est debout
            P(h, typeof(Player), nameof(Player.CanCompleteDeath), nameof(CanCompleteDeathPrefix));
            // Phase ennemie retenue tant que les autres joueurs n'ont pas fini d'exécuter leurs dés
            P(h, typeof(BattleSystem), nameof(BattleSystem.ChangeState), nameof(BattleChangeStatePrefix));
            // Sorts d'ennemis sur le plateau (points faibles du boss scorpion) : l'hôte fait foi
            P(h, typeof(BoardSlot), nameof(BoardSlot.CastWeakSpotSpell), nameof(CastWeakSpotPrefix));
            // Statuts posés sur les ennemis (poison, faiblesse…) : partagés
            P(h, typeof(Entity), nameof(Entity.AddModifier), nameof(AddModifierPrefix));

            // Carte : le choix de case est un vote (toutes les entrées passent par Map.ClickOnMapTile)
            P(h, typeof(Map), nameof(Map.ClickOnMapTile), nameof(MapTileClickPrefix));
            P(h, typeof(Map), nameof(Map.ChangeState), nameof(MapChangeStatePrefix));
            // Point de passage de TOUT départ (certains chemins du jeu ne passent pas par ChangeState)
            P(h, typeof(Map), "EnterMovePlayerState", nameof(EnterMovePlayerPrefix));
            // Un seul pas par départ en coop : sinon un effet propre à un joueur (pas en plus) le fait avancer seul
            P(h, typeof(Map), "_MoveForward", nameof(MoveForwardPrefix));
            // Entrée clavier/manette (Rewired) qui demande de sélectionner une case nulle : NullReferenceException en boucle
            P(h, typeof(Map), nameof(Map.ForceTileSelectableSelection), nameof(NullTileGuard));
            // Embranchement pendant un déplacement : la flèche choisit la direction sans repasser par ChangeState
            P(h, typeof(MapTileArrow), nameof(MapTileArrow.OnPointerClick), nameof(ArrowClickPrefix));

            // Aperçu des dégâts : la flèche de cible au-dessus de l'ennemi affiche aussi les dégâts des autres joueurs.
            P(h, typeof(TargetEnemyArrow), nameof(TargetEnemyArrow.ChangeToAttack), nameof(Preview.ArrowAttackPrefix), patchType: typeof(Preview));
            P(h, typeof(TargetEnemyArrow), nameof(TargetEnemyArrow.ChangeToTargetOnly), nameof(Preview.ArrowTargetOnlyPrefix), patchType: typeof(Preview));

            // Récompenses, reliques, potions, main et dés (tirage de la pioche, lancers) propres à chaque joueur ;
            // ennemis, combat et carte restent communs
            P(h, typeof(SeedSystem.Rng), nameof(SeedSystem.Rng.GetRandomRng), nameof(PersonalRngPostfix), postfix: true);
            // Pioche : tirée par SeedSystem.DiceRNG (commun). On garde l'appel (séquence commune alignée) mais le dé tiré
            // dans MA pioche est choisi par un générateur propre à chaque joueur
            P(h, typeof(DiceManager.DiceList), "get_RandomDice", nameof(PersonalDrawPostfix), postfix: true);

            // 5) Lancement de run et seed commune
            P(h, typeof(GameManager), nameof(GameManager.StartRun), nameof(StartRunPrefix));
            P(h, typeof(GameManager), nameof(GameManager.LoadRun), nameof(LoadRunPrefix));
            P(h, typeof(SeedSystem), nameof(SeedSystem.StartRun), nameof(SeedStartRunPostfix), postfix: true);

            // Plusieurs fenêtres sur le même PC (-ditd-player autre que J1) : fichier de sauvegarde propre à l'instance,
            // sinon les deux jeux écrivent le même fichier en même temps (échec de sauvegarde, chargement de salle bloqué)
            if (LocalInstanceSuffix != null)
            {
                P(h, typeof(SaveDataManager), "GetSaveFileName", nameof(SaveFilePostfix), postfix: true);
                P(h, typeof(SaveDataManager), "GetSavePath", nameof(SaveFilePostfix), postfix: true);
            }

            // Pas de tutoriel en coop (il bloquerait la partie d'un seul joueur)
            // (get_AreTutorialsActive est exclu : sa fonction native est partagée avec 7 autres getters du jeu)
            foreach (var g in new[] { "get_ShouldShowIntroFirstBattleTutorial", "get_ShouldShowMapTutorial", "get_ShouldShowEnemyModifierTutorial",
                                      "get_ShouldShowChangeTargetTutorial", "get_ShouldShowCurrencyTutorial", "get_ShouldShowRelicsTutorial" })
                P(h, typeof(RunInfo.Tutorials), g, nameof(NoTutorialPostfix), postfix: true);

            // 6) Lobby : sélection de personnage pilotée par l'hôte
            var L = typeof(Lobby);
            P(h, typeof(CanvasTitleScreen), nameof(CanvasTitleScreen.OnClickNewRunButtonPreCheckPopup), nameof(Lobby.NewRunPrefix), patchType: L);
            P(h, typeof(CanvasTitleScreen), nameof(CanvasTitleScreen.OnClickNewRunButton), nameof(Lobby.NewRunPrefix), patchType: L);
            P(h, typeof(CanvasTitleScreen), nameof(CanvasTitleScreen.OnClickNewRunButton), nameof(Lobby.NewRunPostfix), postfix: true, patchType: L);
            P(h, typeof(CanvasTitleScreen), nameof(CanvasTitleScreen.OnClickReturnTitleButton), nameof(Lobby.ReturnPrefix), patchType: L);
            P(h, typeof(CanvasTitleScreen), nameof(CanvasTitleScreen.OnClickReturnTitleButton), nameof(Lobby.ReturnPostfix), postfix: true, patchType: L);
            P(h, typeof(CanvasTitleScreen), nameof(CanvasTitleScreen.OnClickContinueButton), nameof(Lobby.BlockedInCoopPrefix), patchType: L);
            P(h, typeof(CanvasTitleScreen), nameof(CanvasTitleScreen.OnClickEndlessModeButton), nameof(Lobby.BlockedInCoopPrefix), patchType: L);
            P(h, typeof(CanvasTitleScreen), nameof(CanvasTitleScreen.OnClickNewEndlessRunButton), nameof(Lobby.BlockedInCoopPrefix), patchType: L);

            MultiplayerMod.Instance.Logger.LogInfo($"Patchs multijoueur : {_ok}/{_total}");
        }

        private static readonly HashSet<string> ExcludedTypes = new HashSet<string>
        {
            "ButtonAudio",                                 // simple son de clic
            "UnityEngine.UIElements.PanelEventHandler",    // UI Toolkit, non utilisée pour le gameplay
        };

        /// <summary>
        /// Interface IL2CPP générée par l'interop : une classe qui hérite directement d'Il2CppObjectBase
        /// (les vraies classes héritent d'Il2CppSystem.Object).
        /// SURTOUT ne pas utiliser Il2CppType.From ici : il force l'initialisation statique native de la classe.
        /// Appelé sur tous les types au chargement (sur l'écran titre, sans run), il cassait par exemple Attack,
        /// dont le constructeur statique lit RunInfo.CurrentRun, et ce pour toute la session.
        /// </summary>
        private static bool Game_IsIl2CppInterface(Type t) => t.BaseType == typeof(Il2CppObjectBase);

        /// <summary>Adresse de la fonction native derrière une méthode d'interop (premier champ de Il2CppMethodInfo).</summary>
        private static IntPtr NativePointer(MethodBase m)
        {
            try
            {
                var f = Il2CppInterop.Common.Il2CppInteropUtils.GetIl2CppMethodInfoPointerFieldForGeneratedMethod(m);
                if (f == null) return IntPtr.Zero;
                var info = (IntPtr)f.GetValue(null);
                return info == IntPtr.Zero ? IntPtr.Zero : Marshal.ReadIntPtr(info);
            }
            catch { return IntPtr.Zero; }
        }

        private static void P(Harmony h, Type t, string method, string patch, bool postfix = false, Type patchType = null)
        {
            _total++;
            try
            {
                var m = t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
                         .Where(x => x.Name == method).OrderByDescending(x => x.GetParameters().Length).FirstOrDefault();
                if (m == null) { MultiplayerMod.Instance.Logger.LogWarning($"Introuvable : {t.Name}.{method}"); return; }
                var hm = new HarmonyMethod(patchType ?? typeof(Patches), patch);
                if (postfix) h.Patch(m, postfix: hm); else h.Patch(m, prefix: hm);
                _ok++;
            }
            catch (Exception e) { MultiplayerMod.Instance.Logger.LogWarning($"Patch {t.Name}.{method} : {e.Message}"); }
        }

        // ───── Clics ─────
        private static bool ClickPrefix(Il2CppObjectBase __instance, PointerEventData __0, MethodBase __originalMethod)
        {
            try { return Coop.OnLocalClick(__instance, __0, __originalMethod); }
            catch (Exception e) { MultiplayerMod.Instance.Logger.LogError($"ClickPrefix : {e}"); return true; }
        }

        private static void ClickPostfix() => Coop.EndLocalClick();

        // ───── Fin de tour ─────
        private static bool FinishTurnPrefix() => Coop.OnLocalFinishTurn();

        private static bool NullTileGuard(VisualMapTile __0) => __0 != null;

        private static bool ArrowClickPrefix(MapTileArrow __instance)
        {
            try { return Coop.OnArrowClick(__instance); }
            catch (Exception e) { MultiplayerMod.Instance.Logger.LogWarning("ArrowClickPrefix : " + e.Message); return true; }
        }

        /// <summary>Suffixe de sauvegarde de cette instance (-ditd-player J2, J3…), null pour J1 ou une partie normale.</summary>
        private static readonly string LocalInstanceSuffix = InstanceSuffix();

        private static string InstanceSuffix()
        {
            var a = Environment.GetCommandLineArgs();
            int i = Array.FindIndex(a, x => string.Equals(x, "-ditd-player", StringComparison.OrdinalIgnoreCase));
            if (i < 0 || i + 1 >= a.Length) return null;
            var name = new string(a[i + 1].Where(char.IsLetterOrDigit).ToArray());
            return string.IsNullOrEmpty(name) || name.Equals("J1", StringComparison.OrdinalIgnoreCase) ? null : name;
        }

        /// <summary>Fichier de sauvegarde propre à l'instance, créé à partir de celui du joueur s'il n'existe pas encore.</summary>
        private static void SaveFilePostfix(ref string __result)
        {
            try
            {
                if (string.IsNullOrEmpty(__result) || LocalInstanceSuffix == null) return;
                // Nom seul (GetSaveFileName) ou chemin complet (GetSavePath) ; jamais deux fois le suffixe
                var original = __result;
                var name = Path.GetFileNameWithoutExtension(original);
                if (name.EndsWith("_" + LocalInstanceSuffix)) return;
                var folder = Path.GetDirectoryName(original);
                var mineName = name + "_" + LocalInstanceSuffix + Path.GetExtension(original);
                var dir = string.IsNullOrEmpty(folder) ? UnityEngine.Application.persistentDataPath : folder;
                var src = Path.Combine(dir, Path.GetFileName(original));
                var dst = Path.Combine(dir, mineName);
                if (!File.Exists(dst) && File.Exists(src)) File.Copy(src, dst);
                __result = string.IsNullOrEmpty(folder) ? mineName : dst;
            }
            catch { }
        }

        private static void MoveForwardPrefix(Map.MoveData __0)
        {
            try { Coop.ClampMove(__0); }
            catch (Exception e) { MultiplayerMod.Instance.Logger.LogWarning("MoveForwardPrefix : " + e.Message); }
        }

        private static bool EnterMovePlayerPrefix()
        {
            try { return Coop.OnEnterMovePlayer(); }
            catch (Exception e) { MultiplayerMod.Instance.Logger.LogWarning("EnterMovePlayerPrefix : " + e.Message); return true; }
        }

        private static bool MapChangeStatePrefix(Map.MapState __0)
        {
            try { return Coop.OnMapChangeState(__0); }
            catch (Exception e) { MultiplayerMod.Instance.Logger.LogWarning("MapChangeStatePrefix : " + e.Message); return true; }
        }

        private static bool MapTileClickPrefix(VisualMapTile __0)
        {
            try { return Coop.OnMapTileClick(__0); }
            catch (Exception e) { MultiplayerMod.Instance.Logger.LogWarning("MapTileClickPrefix : " + e.Message); return true; }
        }

        /// <summary>
        /// Postfix de SeedSystem.Rng.GetRandomRng : pour les contextes personnels, le générateur est dérivé de celui du jeu
        /// et du rang du joueur. Déterministe pour chaque joueur, mais différent de l'un à l'autre. Rang 0 (hôte) : inchangé.
        /// </summary>
        private static System.Random _drawRng;
        private static int _drawRngSeed;

        private static void PersonalDrawPostfix(DiceManager.DiceList __instance, ref ActiveDice __result)
        {
            try
            {
                if (!Coop.Active || __instance == null) return;
                var pile = Game.Safe(() => Game.Dice.diceInDrawPile);
                if (pile == null || pile.Pointer != __instance.Pointer) return;
                var all = __instance.AllDice;
                int n = all?.Count ?? 0;
                if (n == 0) return;
                int seed = Game.Safe(() => RunInfo.CurrentRun.Seed.seed);
                int key = unchecked(seed ^ (Coop.LocalId + 1) * 0x2545F491);
                if (_drawRng == null || _drawRngSeed != key) { _drawRng = new System.Random(key); _drawRngSeed = key; }
                __result = all[_drawRng.Next(n)];
            }
            catch (Exception e) { MultiplayerMod.Instance.Logger.LogDebug("PersonalDrawPostfix : " + e.Message); }
        }

        private static void PersonalRngPostfix(SeedSystem.Rng.Context __0, ref Il2CppSystem.Random __result)
        {
            if (!Coop.Active || __result == null) return;
            if (__0 != SeedSystem.Rng.Context.Reward && __0 != SeedSystem.Rng.Context.Relic
                && __0 != SeedSystem.Rng.Context.Potion && __0 != SeedSystem.Rng.Context.Hand && __0 != SeedSystem.Rng.Context.Dice) return;
            int rank = Coop.LocalRank;
            if (rank <= 0) return;
            unchecked { __result = new Il2CppSystem.Random(__result.Next() ^ (rank * 0x5bd1e995) ^ ((int)__0 << 20)); }
        }

        // ───── Plateau ─────
        private static bool PlaceDicePrefix(BoardSlot __0, GameObjectDice __1, BattleBoard.PlaceDiceContext __3)
        {
            try
            {
                // Un dé d'un autre joueur ne peut être posé que par le mod lui-même
                if (!Board.PlacingGhost && Board.IsGhostDice(__1)) return false;
                if (__3 != BattleBoard.PlaceDiceContext.Player) return true;
                return Board.OnLocalPlace(__0, __1);
            }
            catch (Exception e) { MultiplayerMod.Instance.Logger.LogWarning("PlaceDicePrefix : " + e.Message); return true; }
        }

        private static bool ReturnDicePrefix(GameObjectDice __0)
        {
            try
            {
                if (Board.IsGhostDice(__0)) return false;   // dé d'un autre joueur : on ne le reprend pas en main
                Board.OnLocalRemove(__0);
            }
            catch { }
            return true;
        }

        private static void CanPlaceDicePostfix(BoardSlot __instance, ref bool __result)
        {
            try
            {
                if (!__result || !Coop.Active || Board.PlacingGhost || __instance == null) return;
                if (Knockout.LocalKO) { __result = false; return; }   // K.O. : spectateur
                var c = __instance.CurrentCoord;
                if (Board.IsTakenByOther(c.x, c.y) || Board.IsGhostSlot(__instance)) __result = false;
            }
            catch { }
        }

        private static bool GhostDiceInputPrefix(GameObjectDice __instance)
        {
            try { return !(Coop.Active && Board.IsGhostDice(__instance)); } catch { return true; }
        }

        private static bool GhostSelectPrefix(GameObjectDice __0)
        {
            try { return !(Coop.Active && Board.IsGhostDice(__0)); } catch { return true; }
        }

        private static void GhostActionsPostfix(BoardSlot __instance, ref Il2CppSystem.Collections.Generic.List<BattleSystem.BattleAction> __result)
        {
            try
            {
                if (Coop.Active && Board.IsGhostSlot(__instance))
                    __result = new Il2CppSystem.Collections.Generic.List<BattleSystem.BattleAction>();
            }
            catch { }
        }

        private static void HandFloorEndPrefix(HandUI __instance)
        {
            try { Board.BeforeHandFloorEnd(__instance); } catch { }
        }

        private static void DiscardAllPrefix()
        {
            try { Board.BeforeDiscardAll(); }
            catch (Exception e) { MultiplayerMod.Instance.Logger.LogWarning("DiscardAllPrefix : " + e.Message); }
        }

        // ───── Ennemis ─────
        private static void EnemyDamagePostfix(Enemy __instance, Hit __0)
        {
            try { Combat.OnEnemyDamaged(__instance, __0); }
            catch (Exception e) { MultiplayerMod.Instance.Logger.LogWarning("EnemyDamagePostfix : " + e.Message); }
        }

        private static bool CanCompleteDeathPrefix(Player __instance, ref bool __result)
        {
            try { return Knockout.OnCanCompleteDeath(__instance, ref __result); }
            catch (Exception e) { MultiplayerMod.Instance.Logger.LogWarning("CanCompleteDeathPrefix : " + e.Message); return true; }
        }

        private static bool CastWeakSpotPrefix(BoardSlot __instance, SpellData __0, int __1, BoardSlot.BoardDirection __2)
        {
            try { return Spells.OnCast(__instance, __0, __1, __2); }
            catch (Exception e) { MultiplayerMod.Instance.Logger.LogWarning("CastWeakSpotPrefix : " + e.Message); return true; }
        }

        private static bool BattleChangeStatePrefix(BattleSystem __instance, BattleSystem.TurnState __0)
        {
            try { return Combat.OnBattleChangeState(__instance, __0); }
            catch (Exception e) { MultiplayerMod.Instance.Logger.LogWarning("BattleChangeStatePrefix : " + e.Message); return true; }
        }

        private static void AddModifierPrefix(Entity __instance, ModifierData __0)
        {
            try { Combat.OnModifierAdded(__instance, __0); }
            catch (Exception e) { MultiplayerMod.Instance.Logger.LogDebug("AddModifierPrefix : " + e.Message); }
        }

        // ───── Run ─────
        private static bool StartRunPrefix(RunInfo.NewRunData __0, ActiveCustomRun __1) => Team.OnLocalStartRun(__0, __1);

        private static void NoTutorialPostfix(ref bool __result) { if (Coop.Active) __result = false; }

        private static bool LoadRunPrefix()
        {
            if (!Coop.Active) return true;
            Coop.Toast("Reprendre une sauvegarde n'est pas encore géré en multijoueur");
            return false;
        }

        /// <summary>Après SeedSystem.StartRun : la seed commune (tirée par l'hôte) remplace celle du jeu, chez tous.</summary>
        private static void SeedStartRunPostfix(SeedSystem __instance)
        {
            if (Coop.Active) Coop.ApplyPendingSeed(__instance);
        }
    }
}
