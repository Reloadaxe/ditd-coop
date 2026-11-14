using System;

namespace DiceLoader
{
    /// <summary>
    /// Événements globaux, une alternative aux méthodes virtuelles de <see cref="DiceMod"/>.
    /// Exemple : <c>DiceEvents.TurnStart += (battle, turn) => Game.Energy += 1;</c>
    /// Pense à te désabonner dans OnDisable.
    /// </summary>
    public static class DiceEvents
    {
        public static event Action<GameManager.GameState, GameManager.GameState> GameStateChanged;
        public static event Action<CurrentRunData, bool> RunStart;
        public static event Action<CurrentRunData, bool> RunEnd;
        public static event Action<FloorSystem> FloorStart;
        public static event Action<FloorSystem, bool> FloorEnd;
        public static event Action<BattleSystem> BattleStart;
        public static event Action<BattleSystem, int> TurnStart;
        public static event Action<BattleSystem, BattleSystem.TurnState> TurnStateChanged;
        public static event Action<BattleSystem, BattleSystem.EndState> BattleEnd;
        public static event Action<Enemy> EnemyDeath;
        public static event Action<Player> PlayerDeath;
        public static event Action<ActiveRelic> RelicAdded;
        public static event Action<ActiveRelic> RelicRemoved;

        // ───── dispatch interne : événements C# puis mods actifs, chaque appel isolé ─────

        internal static void RaiseGameStateChanged(GameManager.GameState a, GameManager.GameState b)
        { Invoke(GameStateChanged, h => h(a, b)); Loader.ForEachEnabled(m => m.OnGameStateChanged(a, b), nameof(DiceMod.OnGameStateChanged)); }

        internal static void RaiseRunStart(CurrentRunData run, bool loaded)
        { Invoke(RunStart, h => h(run, loaded)); Loader.ForEachEnabled(m => m.OnRunStart(run, loaded), nameof(DiceMod.OnRunStart)); }

        internal static void RaiseRunEnd(CurrentRunData run, bool won)
        { Invoke(RunEnd, h => h(run, won)); Loader.ForEachEnabled(m => m.OnRunEnd(run, won), nameof(DiceMod.OnRunEnd)); }

        internal static void RaiseFloorStart(FloorSystem f)
        { Invoke(FloorStart, h => h(f)); Loader.ForEachEnabled(m => m.OnFloorStart(f), nameof(DiceMod.OnFloorStart)); }

        internal static void RaiseFloorEnd(FloorSystem f, bool success)
        { Invoke(FloorEnd, h => h(f, success)); Loader.ForEachEnabled(m => m.OnFloorEnd(f, success), nameof(DiceMod.OnFloorEnd)); }

        internal static void RaiseBattleStart(BattleSystem b)
        { Invoke(BattleStart, h => h(b)); Loader.ForEachEnabled(m => m.OnBattleStart(b), nameof(DiceMod.OnBattleStart)); }

        internal static void RaiseTurnStart(BattleSystem b, int turn)
        { Invoke(TurnStart, h => h(b, turn)); Loader.ForEachEnabled(m => m.OnTurnStart(b, turn), nameof(DiceMod.OnTurnStart)); }

        internal static void RaiseTurnStateChanged(BattleSystem b, BattleSystem.TurnState s)
        { Invoke(TurnStateChanged, h => h(b, s)); Loader.ForEachEnabled(m => m.OnTurnStateChanged(b, s), nameof(DiceMod.OnTurnStateChanged)); }

        internal static void RaiseBattleEnd(BattleSystem b, BattleSystem.EndState s)
        { Invoke(BattleEnd, h => h(b, s)); Loader.ForEachEnabled(m => m.OnBattleEnd(b, s), nameof(DiceMod.OnBattleEnd)); }

        internal static void RaiseEnemyDeath(Enemy e)
        { Invoke(EnemyDeath, h => h(e)); Loader.ForEachEnabled(m => m.OnEnemyDeath(e), nameof(DiceMod.OnEnemyDeath)); }

        internal static void RaisePlayerDeath(Player p)
        { Invoke(PlayerDeath, h => h(p)); Loader.ForEachEnabled(m => m.OnPlayerDeath(p), nameof(DiceMod.OnPlayerDeath)); }

        internal static void RaiseRelicAdded(ActiveRelic r)
        { Invoke(RelicAdded, h => h(r)); Loader.ForEachEnabled(m => m.OnRelicAdded(r), nameof(DiceMod.OnRelicAdded)); }

        internal static void RaiseRelicRemoved(ActiveRelic r)
        { Invoke(RelicRemoved, h => h(r)); Loader.ForEachEnabled(m => m.OnRelicRemoved(r), nameof(DiceMod.OnRelicRemoved)); }

        private static void Invoke<T>(T del, Action<T> call) where T : Delegate
        {
            if (del == null) return;
            foreach (var d in del.GetInvocationList())
            {
                try { call((T)d); }
                catch (Exception e) { Loader.Log.LogError($"Handler d'événement ({d.Method.DeclaringType?.FullName}.{d.Method.Name}) : {e}"); }
            }
        }
    }
}
