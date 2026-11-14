using System;
using System.Collections.Generic;
using UnityEngine;

namespace DiceLoader
{
    /// <summary>File d'actions différées, exécutée dans Update sur le thread Unity.</summary>
    internal static class Scheduler
    {
        private struct Job { public float At; public Action Action; }
        private static readonly List<Job> Jobs = new List<Job>();
        private static readonly object Lock = new object();

        public static void Add(Action a, float delay)
        {
            if (a == null) return;
            lock (Lock) Jobs.Add(new Job { At = Time.unscaledTime + Math.Max(0f, delay), Action = a });
        }

        public static void Tick()
        {
            List<Action> due = null;
            float now = Time.unscaledTime;
            lock (Lock)
            {
                for (int i = Jobs.Count - 1; i >= 0; i--)
                {
                    if (Jobs[i].At <= now)
                    {
                        (due ??= new List<Action>()).Add(Jobs[i].Action);
                        Jobs.RemoveAt(i);
                    }
                }
            }
            if (due == null) return;
            for (int i = due.Count - 1; i >= 0; i--)
            {
                try { due[i](); }
                catch (Exception e) { Loader.Log.LogError($"Action planifiée : {e}"); }
            }
        }
    }
}
