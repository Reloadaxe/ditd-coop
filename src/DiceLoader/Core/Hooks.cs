using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;

namespace DiceLoader
{
    /// <summary>
    /// Hooks Harmony posés par le loader sur les méthodes clés du jeu. Ils alimentent <see cref="DiceEvents"/>.
    /// Chaque hook est appliqué séparément : si une méthode change dans une future version du jeu,
    /// seul l'événement correspondant cesse de fonctionner.
    /// Les paramètres utilisent __0, __1... (index) plutôt que leur nom, pour mieux résister aux mises à jour.
    /// </summary>
    internal static class Hooks
    {
        private static Harmony _h;

        public static void Apply(Harmony harmony)
        {
            _h = harmony;
            int ok = 0, total = 0;
            void P(Type t, string method, string patch, Type[] args = null)
            {
                total++;
                try
                {
                    var target = FindMethod(t, method, args);
                    if (target == null) { Loader.Log.LogWarning($"Hook introuvable : {t.Name}.{method}"); return; }
                    var hm = new HarmonyMethod(typeof(Hooks), patch);
                    if (patch.StartsWith("Pre_")) _h.Patch(target, prefix: hm);
                    else _h.Patch(target, postfix: hm);
                    ok++;
                }
                catch (Exception e) { Loader.Log.LogWarning($"Hook {t.Name}.{method} impossible : {e.Message}"); }
            }

            // Run (CurrentRunData)
            P(typeof(CurrentRunData), nameof(CurrentRunData.StartRun), nameof(Post_RunStart));
            P(typeof(CurrentRunData), nameof(CurrentRunData.LoadRun), nameof(Post_RunLoad));
            P(typeof(CurrentRunData), nameof(CurrentRunData.EndRun), nameof(Post_RunEnd));
            // Floor (FloorSystem implémente IExecutionEvents)
            P(typeof(FloorSystem), nameof(FloorSystem.FloorStart), nameof(Post_FloorStart));
            P(typeof(FloorSystem), nameof(FloorSystem.FloorEnd), nameof(Post_FloorEnd));
            // Combat
            P(typeof(BattleSystem), nameof(BattleSystem.ChangeState), nameof(Post_ChangeTurnState));
            P(typeof(BattleSystem), nameof(BattleSystem.BattleEnd), nameof(Post_BattleEnd));
            // Morts : Enemy et Player surchargent Entity.Death
            P(typeof(Enemy), nameof(Enemy.Death), nameof(Post_EnemyDeath));
            P(typeof(Player), nameof(Player.Death), nameof(Post_PlayerDeath));
            // Reliques : toutes les surcharges de AddRelic finissent par AddRelic(ActiveRelic, int)
            P(typeof(Relics), nameof(Relics.AddRelic), nameof(Post_RelicAdded), new[] { typeof(ActiveRelic), typeof(int) });
            P(typeof(Relics), nameof(Relics.RemoveRelic), nameof(Pre_RelicRemoved), new[] { typeof(ActiveRelic) });

            Loader.Log.LogInfo($"Hooks du loader : {ok}/{total} appliqués.");
        }

        private static MethodBase FindMethod(Type t, string name, Type[] args)
        {
            const BindingFlags F = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            var candidates = t.GetMethods(F).Where(m => m.Name == name).ToList();
            if (args != null)
                return candidates.FirstOrDefault(m => m.GetParameters().Select(p => p.ParameterType).SequenceEqual(args));
            return candidates.OrderBy(m => m.GetParameters().Length).FirstOrDefault();
        }

        // ───── Postfixes ─────
        // Note : RemoveRelic est patché en *préfixe* pour avoir la relique avant qu'elle ne disparaisse.

        private static void Post_RunStart(CurrentRunData __instance) => DiceEvents.RaiseRunStart(__instance, false);
        private static void Post_RunLoad(CurrentRunData __instance) => DiceEvents.RaiseRunStart(__instance, true);
        private static void Post_RunEnd(CurrentRunData __instance, bool __0) => DiceEvents.RaiseRunEnd(__instance, __0);

        private static void Post_FloorStart(FloorSystem __instance) => DiceEvents.RaiseFloorStart(__instance);
        private static void Post_FloorEnd(FloorSystem __instance, bool __0) => DiceEvents.RaiseFloorEnd(__instance, __0);

        private static void Post_ChangeTurnState(BattleSystem __instance, BattleSystem.TurnState __0)
        {
            DiceEvents.RaiseTurnStateChanged(__instance, __0);
            if (__0 == BattleSystem.TurnState.Intro) DiceEvents.RaiseBattleStart(__instance);
            else if (__0 == BattleSystem.TurnState.StartTurn) DiceEvents.RaiseTurnStart(__instance, Game.Safe(() => __instance.currentTurnCount));
        }

        private static void Post_BattleEnd(BattleSystem __instance, BattleSystem.EndState __0) => DiceEvents.RaiseBattleEnd(__instance, __0);

        private static void Post_EnemyDeath(Enemy __instance) => DiceEvents.RaiseEnemyDeath(__instance);
        private static void Post_PlayerDeath(Player __instance) => DiceEvents.RaisePlayerDeath(__instance);

        private static void Post_RelicAdded(ActiveRelic __0) => DiceEvents.RaiseRelicAdded(__0);

        private static void Pre_RelicRemoved(ActiveRelic __0) => DiceEvents.RaiseRelicRemoved(__0);
    }
}
