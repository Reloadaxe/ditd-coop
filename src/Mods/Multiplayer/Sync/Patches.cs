using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Reflection;
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

            MultiplayerMod.Instance.Logger.LogInfo($"Patchs multijoueur : {_ok}/{_total}");
        }

        private static readonly HashSet<string> ExcludedTypes = new HashSet<string>
        {
            "ButtonAudio",                                 // simple son de clic
            "UnityEngine.UIElements.PanelEventHandler",    // UI Toolkit, non utilisée pour le gameplay
        };

        private static bool Game_IsIl2CppInterface(Type t)
        {
            try { return Il2CppInterop.Runtime.Il2CppType.From(t, false)?.IsInterface ?? false; }
            catch { return false; }
        }

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

        private static void P(Harmony h, Type t, string method, string patch, bool postfix = false)
        {
            _total++;
            try
            {
                var m = t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
                         .Where(x => x.Name == method).OrderByDescending(x => x.GetParameters().Length).FirstOrDefault();
                if (m == null) { MultiplayerMod.Instance.Logger.LogWarning($"Introuvable : {t.Name}.{method}"); return; }
                var hm = new HarmonyMethod(typeof(Patches), patch);
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
        private static void PlaceDicePrefix(BoardSlot __0, GameObjectDice __1, BattleBoard.PlaceDiceContext __3)
        {
            if (__3 == BattleBoard.PlaceDiceContext.Player) Coop.OnLocalPlaceDice(__0, __1);
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
        private static RunInfo.NewRunData _hostPendingRun;

        private static bool StartRunPrefix(RunInfo.NewRunData __0)
        {
            if (!Coop.Active || Coop.Replaying) return true;
            if (!Coop.IsHost) { Coop.Toast("Seul l'hôte peut lancer la run"); return false; }
            _hostPendingRun = __0;   // envoyé quand la seed sera connue (SeedSystem.StartRun)
            return true;
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
            if (Coop.IsHost && _hostPendingRun != null)
            {
                Coop.HostBroadcastRun(_hostPendingRun, Coop.ReadSeed(__instance));
                _hostPendingRun = null;
            }
            else if (!Coop.IsHost) Coop.ApplyPendingSeed(__instance);
        }
    }
}
