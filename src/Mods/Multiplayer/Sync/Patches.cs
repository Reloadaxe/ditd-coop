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

            // 4) Ennemis partagés : mes dégâts sont envoyés aux autres
            P(h, typeof(Enemy), "_Damage", nameof(EnemyDamagePostfix), postfix: true);

            // 5) Lancement de run et seed commune
            P(h, typeof(GameManager), nameof(GameManager.StartRun), nameof(StartRunPrefix));
            P(h, typeof(GameManager), nameof(GameManager.LoadRun), nameof(LoadRunPrefix));
            P(h, typeof(SeedSystem), nameof(SeedSystem.StartRun), nameof(SeedStartRunPostfix), postfix: true);

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

        // ───── Plateau ─────
        private static bool PlaceDicePrefix(BoardSlot __0, GameObjectDice __1, BattleBoard.PlaceDiceContext __3)
        {
            if (__3 != BattleBoard.PlaceDiceContext.Player) return true;
            try { return Board.OnLocalPlace(__0, __1); }
            catch (Exception e) { MultiplayerMod.Instance.Logger.LogWarning("PlaceDicePrefix : " + e.Message); return true; }
        }

        private static void ReturnDicePrefix(GameObjectDice __0)
        {
            try { Board.OnLocalRemove(__0); } catch { }
        }

        // ───── Ennemis ─────
        private static void EnemyDamagePostfix(Enemy __instance, Hit __0)
        {
            try { Combat.OnEnemyDamaged(__instance, __0); }
            catch (Exception e) { MultiplayerMod.Instance.Logger.LogWarning("EnemyDamagePostfix : " + e.Message); }
        }

        // ───── Run ─────
        private static bool StartRunPrefix(RunInfo.NewRunData __0, ActiveCustomRun __1) => Team.OnLocalStartRun(__0, __1);

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
