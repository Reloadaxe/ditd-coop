using System;
using System.Collections.Generic;
using System.Linq;
using DiceLoader;
using UnityEngine;

namespace DiceMultiplayer.Sync
{
    /// <summary>
    /// Pioche et main par joueur.
    ///  - En combat, DiceManager.diceDrawPerTurn est multiplié par le nombre de joueurs.
    ///  - Le jeu tire chaque dé via diceInDrawPile.RandomDice : on remplace le résultat par un dé du joueur suivant
    ///    (J1, J2, J1, J2…), choisi de façon déterministe (seed, tour, rang du tirage), donc identique chez tous.
    ///  - Côté affichage, chaque joueur ne voit dans sa main que ses propres dés ; ceux des autres sont masqués
    ///    (ils existent toujours dans la logique, à l'identique chez tous). Sur le plateau, tout est visible.
    ///  - L'énergie par tour est multipliée par le nombre de joueurs : réserve commune en attendant l'énergie séparée.
    /// </summary>
    public static class Draw
    {
        private static int? _baseDraw;
        private static int _drawIndex;
        private static readonly Dictionary<IntPtr, bool> _hidden = new Dictionary<IntPtr, bool>();

        private static int Players => Coop.Net?.Players.Count ?? 1;
        private static bool On => Heroes.Enabled && Game.InBattle && Players > 1;

        // ───── Nombre de dés piochés ─────

        public static void OnBattleStart()
        {
            if (!Heroes.Enabled) return;
            var dm = Game.Dice;
            if (dm == null) return;
            if (_baseDraw == null) _baseDraw = dm.diceDrawPerTurn;
            dm.diceDrawPerTurn = _baseDraw.Value * Players;
            _drawIndex = 0;
            Coop.AddLog($"Pioche : {_baseDraw.Value} dés par joueur et par tour ({dm.diceDrawPerTurn} au total).");
        }

        public static void OnBattleEnd()
        {
            var dm = Game.Dice;
            if (dm != null && _baseDraw != null) dm.diceDrawPerTurn = _baseDraw.Value;
            _baseDraw = null;
            _hidden.Clear();
        }

        public static void OnTurnState(BattleSystem.TurnState s)
        {
            if (s == BattleSystem.TurnState.DrawDice || s == BattleSystem.TurnState.StartTurn) _drawIndex = 0;
        }

        // ───── Choix du dé tiré ─────

        /// <summary>Postfix de DiceList.get_RandomDice.</summary>
        public static void OnRandomDice(DiceManager.DiceList list, ref ActiveDice result)
        {
            if (!On || list == null) return;
            var dm = Game.Dice;
            var pile = Game.Safe(() => dm.diceInDrawPile);
            if (pile == null || pile.Pointer != list.Pointer) return;

            var order = Coop.Net.Players.OrderBy(p => p.Id).Select(p => p.Id).ToList();
            int owner = order[_drawIndex % order.Count];
            int rank = _drawIndex++;

            var candidates = new List<ActiveDice>();
            foreach (var d in list.AllDice) if (d != null && Coop.OwnerOfDice(d.uid) == owner) candidates.Add(d);
            if (candidates.Count == 0) return;      // ce joueur n'a plus de dé dans la pioche : on garde le tirage du jeu
            candidates.Sort((a, b) => a.uid.CompareTo(b.uid));

            int seed = Game.Safe(() => RunInfo.CurrentRun.Seed.seed);
            unchecked
            {
                int h = seed * 31 + Game.Turn * 1009 + rank * 7919;
                result = candidates[(int)((uint)h % (uint)candidates.Count)];
            }
        }

        /// <summary>Postfix de DiceManager.InitialEnergyPerTurn : réserve commune à la taille de l'équipe.</summary>
        public static void OnInitialEnergy(ref int result)
        {
            if (Heroes.Enabled && Players > 1) result *= Players;
        }

        // ───── Main : chacun ne voit que ses dés ─────

        public static void UpdateHandVisibility()
        {
            if (!On) { if (_hidden.Count > 0) RestoreAll(); return; }
            foreach (var d in UnityEngine.Object.FindObjectsOfType<GameObjectDice>())
            {
                if (d == null) continue;
                bool hide = Game.Safe(() => d.IsOnHand) && !Coop.OwnsDice(d);
                var key = d.Pointer;
                if (_hidden.TryGetValue(key, out var was) && was == hide) continue;
                SetVisible(d.gameObject, !hide);
                _hidden[key] = hide;
            }
        }

        private static void RestoreAll()
        {
            foreach (var d in UnityEngine.Object.FindObjectsOfType<GameObjectDice>())
                if (d != null && _hidden.TryGetValue(d.Pointer, out var h) && h) SetVisible(d.gameObject, true);
            _hidden.Clear();
        }

        private static void SetVisible(GameObject go, bool visible)
        {
            foreach (var r in go.GetComponentsInChildren<Renderer>(true)) r.enabled = visible;
            foreach (var g in go.GetComponentsInChildren<UnityEngine.UI.Graphic>(true)) g.enabled = visible;
        }
    }
}
