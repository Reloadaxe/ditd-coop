using System;
using System.Collections.Generic;
using DiceLoader;
using UnityEngine;

namespace DiceMultiplayer.Sync
{
    /// <summary>
    /// Difficulté en coop : les ennemis encaissent les dés de tous les joueurs mais attaquent chacun à pleine puissance.
    /// Leurs PV (max et actuels) sont donc multipliés par 1 + Taux × (joueurs − 1). Le multiplicateur est choisi par
    /// l'hôte et figé au lancement de la run (StartRun), identique chez tous : les PV de référence (EnemySync) restent
    /// cohérents. Chaque ennemi est mis à l'échelle une seule fois, invocations en cours de combat comprises.
    /// </summary>
    public static class Scaling
    {
        /// <summary>Multiplicateur de la run en cours (1 = solo).</summary>
        public static float RunFactor = 1f;

        private static readonly Dictionary<IntPtr, int> _scaled = new Dictionary<IntPtr, int>();   // ennemi → PV max après mise à l'échelle
        private static float _nextCheck;

        public static float FactorFor(int players, float perExtra) => 1f + Math.Max(0f, perExtra) * Math.Max(0, players - 1);

        public static void Reset() => _scaled.Clear();

        public static void Update()
        {
            if (!Coop.Active || !Game.InBattle || RunFactor <= 1.001f) return;
            if (Time.unscaledTime < _nextCheck) return;
            _nextCheck = Time.unscaledTime + 0.2f;
            foreach (var e in UnityEngine.Object.FindObjectsOfType<Enemy>())
            {
                if (e == null || e.isDummy || e.IsDyingOrDead) continue;
                try
                {
                    int max = e.MaxHealth;
                    if (max <= 0) continue;
                    if (_scaled.TryGetValue(e.Pointer, out var done) && done == max) continue;   // déjà fait
                    if (_scaled.ContainsKey(e.Pointer) && max > done) { _scaled[e.Pointer] = max; continue; }   // PV max augmentés par le jeu ensuite
                    int newMax = Mathf.Max(1, Mathf.RoundToInt(max * RunFactor));
                    int cur = e.CurrentHealth;
                    int newCur = Mathf.Clamp(Mathf.RoundToInt(cur * RunFactor), 1, newMax);
                    e.ChangeMaxHealth(newMax);
                    e.ChangeCurrentHealth(newCur);
                    e.ForceHealthRefresh();
                    _scaled[e.Pointer] = newMax;
                    MultiplayerMod.Instance.Logger.LogInfo($"PV de {e.name} : {cur}/{max} → {newCur}/{newMax} (×{RunFactor:0.##})");
                }
                catch (Exception ex) { MultiplayerMod.Instance.Logger.LogDebug("Mise à l'échelle : " + ex.Message); }
            }
        }
    }
}
