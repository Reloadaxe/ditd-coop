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

            // 2) Actions sémantiques
            P(h, typeof(BattleBoard), nameof(BattleBoard.PlaceDiceOnBoard), nameof(PlaceDicePrefix));
            P(h, typeof(ReturnDiceToHand), nameof(ReturnDiceToHand.ExecuteReturnDiceToHand), nameof(ReturnDicePrefix));
            P(h, typeof(FinishTurnUI), nameof(FinishTurnUI.OnClickFinishTurnButton), nameof(FinishTurnPrefix));

            // 3) Propriété des dés : on ne peut pas prendre le dé d'un autre joueur
            P(h, typeof(GameObjectDice), nameof(GameObjectDice.OnDicePointerDown), nameof(DiceOwnershipPrefix));
            P(h, typeof(GameObjectDice), nameof(GameObjectDice.OnDicePointerClick), nameof(DiceOwnershipPrefix));
            P(h, typeof(GameObjectDice), nameof(GameObjectDice.OnCustomPointerClick), nameof(DiceOwnershipPrefix));

            // 4) Lancement de run et seed
            P(h, typeof(GameManager), nameof(GameManager.StartRun), nameof(StartRunPrefix));
            P(h, typeof(GameManager), nameof(GameManager.LoadRun), nameof(LoadRunPrefix));
            P(h, typeof(SeedSystem), nameof(SeedSystem.StartRun), nameof(SeedStartRunPostfix), postfix: true);
            P(h, typeof(ActiveDeck), nameof(ActiveDeck.AddDice), nameof(DeckAddDicePostfix), postfix: true);
            P(h, typeof(ActiveDeck), nameof(ActiveDeck.RemoveDice), nameof(DeckRemoveDicePrefix));

            // 6) Héros multiples : propriétaire des dés, coups ennemis rejoués sur chaque héros, K.O.
            P(h, typeof(GameObjectDice), nameof(GameObjectDice.Init), nameof(DiceInitPrefix));
            P(h, typeof(Player), "_Damage", nameof(PlayerDamagePostfix), postfix: true);
            P(h, typeof(Player), nameof(Player.Death), nameof(PlayerDeathPrefix));
            P(h, typeof(DiceManager.DiceList), "get_RandomDice", nameof(RandomDicePostfix), postfix: true);
            P(h, typeof(DiceManager), nameof(DiceManager.InitialEnergyPerTurn), nameof(InitialEnergyPostfix), postfix: true);
            P(h, typeof(DiceManager), nameof(DiceManager.MaxDiceInHand), nameof(MaxHandPostfix), postfix: true);
            // Un GameObjectDice sans données (objet auxiliaire venu du prefab d'un héros supplémentaire) faisait planter le calcul d'énergie
            P(h, typeof(GameObjectDice), "GetCurrentEnergyCost", nameof(EnergyCostGuard));
            P(h, typeof(GameObjectDice), "CanSelectFromHand", nameof(CanSelectGuard));

            // Diagnostic : qui remet la run à zéro ?
            foreach (var m in new[] { nameof(RunInfo.Reset), nameof(RunInfo.ExitRun), nameof(RunInfo.EndUnfinishedRun), nameof(RunInfo.EndRun) })
                P(h, typeof(RunInfo), m, nameof(RunResetTrace));

            // 5) Lobby : sélection de personnage pilotée par l'hôte
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

        // ───── Dés ─────
        private static void PlaceDicePrefix(BoardSlot __0, GameObjectDice __1, ref Entity __2, BattleBoard.PlaceDiceContext __3)
        {
            if (__3 != BattleBoard.PlaceDiceContext.Player) return;
            Coop.OnLocalPlaceDice(__0, __1);
            // Le héros qui pose le dé est celui de son propriétaire
            try
            {
                if (Heroes.Enabled && __1 != null && Heroes.IsHostHero(__2))
                {
                    var owner = Heroes.EntityOf(Coop.OwnerOfDice(__1.CurrentActiveDice.uid));
                    if (owner != null) __2 = owner;
                }
            }
            catch { }
        }

        private static void ReturnDicePrefix(GameObjectDice __0) => Coop.OnLocalReturnDice(__0);

        private static bool FinishTurnPrefix() => Coop.OnLocalFinishTurn();

        private static bool DiceOwnershipPrefix(GameObjectDice __instance)
        {
            if (!Coop.Active || Coop.Replaying) return true;
            if (!DiceLoader.Game.InBattle) return true;
            if (Coop.OwnsDice(__instance)) return true;
            Coop.Toast($"Ce dé appartient à {Coop.NameOf(Coop.OwnerOfDice(__instance.CurrentActiveDice.uid))}");
            return false;
        }

        // ───── Run ─────
        /// <summary>Hôte : run d'équipe en cours de lancement, envoyée avec la seed dans SeedSystem.StartRun.</summary>
        public static Net.RunData HostPendingRun;

        private static bool StartRunPrefix(RunInfo.NewRunData __0, ActiveCustomRun __1) => Team.OnLocalStartRun(__0, __1);

        private static void DeckAddDicePostfix(ActiveDice __0) => Team.OnDiceAddedToDeck(__0);

        private static void DeckRemoveDicePrefix(ActiveDeck __instance, ActiveDice __0) => Team.OnDiceRemovedFromDeck(__instance, __0);

        /// <summary>Un dé tiré pour le héros de l'hôte mais appartenant à un autre joueur reçoit le héros de ce joueur comme propriétaire.</summary>
        private static void DiceInitPrefix(ActiveDice __0, ref Entity __2)
        {
            try
            {
                if (!Heroes.Enabled || __0 == null || !Heroes.IsHostHero(__2)) return;
                var owner = Heroes.EntityOf(Coop.OwnerOfDice(__0.uid));
                if (owner != null && !Heroes.IsHostHero(owner)) __2 = owner;
            }
            catch (Exception e) { MultiplayerMod.Instance.Logger.LogWarning("DiceInitPrefix : " + e.Message); }
        }

        private static void PlayerDamagePostfix(Player __instance, Hit __0)
        {
            try { Heroes.OnHostDamaged(__instance, __0); }
            catch (Exception e) { MultiplayerMod.Instance.Logger.LogWarning("PlayerDamagePostfix : " + e.Message); }
        }

        private static bool PlayerDeathPrefix(Player __instance) => Heroes.OnPlayerDeath(__instance);

        private static void RandomDicePostfix(DiceManager.DiceList __instance, ref ActiveDice __result)
        {
            try { Draw.OnRandomDice(__instance, ref __result); }
            catch (Exception e) { MultiplayerMod.Instance.Logger.LogWarning("RandomDicePostfix : " + e.Message); }
        }

        private static void InitialEnergyPostfix(ref int __result) => Draw.OnInitialEnergy(ref __result);

        private static void MaxHandPostfix(ref int __result) => Draw.OnMaxHand(ref __result);

        private static bool EnergyCostGuard(GameObjectDice __instance, ref int __result)
        {
            if (Game.Safe(() => __instance.CurrentActiveDice) != null) return true;
            __result = 0;
            return false;
        }

        private static bool CanSelectGuard(GameObjectDice __instance, ref bool __result)
        {
            if (Game.Safe(() => __instance.CurrentActiveDice) != null) return true;
            __result = false;
            return false;
        }

        private static void RunResetTrace(System.Reflection.MethodBase __originalMethod)
        {
            if (!Coop.Active) return;
            Coop.AddLog($"[diag] RunInfo.{__originalMethod?.Name} appelé (état {DiceLoader.Game.State}, run {(RunInfo.CurrentRun != null ? "présente" : "absente")})");
        }

        private static bool LoadRunPrefix()
        {
            if (!Coop.Active) return true;
            Coop.Toast("Reprendre une sauvegarde n'est pas encore géré en multijoueur");
            return false;
        }

        private static void SeedStartRunPostfix(SeedSystem __instance)
        {
            if (!Coop.Active) return;
            if (Coop.IsHost && HostPendingRun != null)
            {
                // La seed n'est pas encore générée ici (valeur par défaut 999) : l'hôte en tire une et la force chez tous, lui compris.
                int seed = new Random().Next(1000, int.MaxValue);
                Coop.PendingSeed = seed;
                Coop.ApplyPendingSeed(__instance);
                Coop.HostBroadcastRun(HostPendingRun, seed);
                HostPendingRun = null;
            }
            else if (!Coop.IsHost) Coop.ApplyPendingSeed(__instance);
        }
    }
}
