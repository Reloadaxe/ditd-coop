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
        private static bool _pileTagged;
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
            _pileTagged = false;
            Coop.AddLog($"Pioche : {_baseDraw.Value} dés par joueur et par tour ({dm.diceDrawPerTurn} au total).");
            Game.Schedule(() =>
            {
                var pile = Game.Safe(() => Game.Dice.diceInDrawPile.AllDice);
                if (pile == null) return;
                var parts = new List<string>();
                foreach (var d in pile) parts.Add($"{d.uid}>{Coop.NameOf(Coop.OwnerOfDice(d.uid))}");
                Coop.AddLog($"[diag] Pioche du combat ({parts.Count}) : {string.Join(" ", parts)}");
            }, 0.2f);
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
            if (!_pileTagged && (s == BattleSystem.TurnState.Intro || s == BattleSystem.TurnState.StartTurn) && Heroes.Enabled)
            {
                Team.TagBattlePile();
                _pileTagged = Game.Safe(() => Game.Dice.diceInDrawPile.DiceAmount) > 0;
            }
            if (s == BattleSystem.TurnState.SelectDice && Heroes.Enabled)
                Game.Schedule(() =>
                {
                    var hand = Game.Safe(() => Game.Dice.GetAllGODicesOnHand());
                    if (hand == null) return;
                    int mine = 0, total = 0, hidden = 0;
                    foreach (var d in hand)
                    {
                        total++;
                        if (Coop.OwnsDice(d)) mine++;
                        if (_hidden.TryGetValue(d.Pointer, out var hh) && hh) hidden++;
                    }
                    Coop.AddLog($"[diag] Main : {total} dés, {mine} à moi, {hidden} masqués (énergie {Game.Energy}).");
                }, 0.5f);
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

        /// <summary>Postfix de DiceManager.MaxDiceInHand : la main peut contenir les dés de tous les joueurs.</summary>
        public static void OnMaxHand(ref int result)
        {
            if (Heroes.Enabled && Players > 1) result *= Players;
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
                bool changed = !_hidden.TryGetValue(key, out var was) || was != hide;
                // Le jeu anime lui-même la transparence du dé : on la réimpose à chaque frame tant qu'il est masqué
                if (hide || changed) SetVisible(d, !hide);
                _hidden[key] = hide;
            }
        }

        private static void RestoreAll()
        {
            foreach (var d in UnityEngine.Object.FindObjectsOfType<GameObjectDice>())
                if (d != null && _hidden.TryGetValue(d.Pointer, out var h) && h) SetVisible(d, true);
            _hidden.Clear();
        }

        private static void SetVisible(GameObjectDice d, bool visible)
        {
            // 1) Transparence et clics
            var cg = Game.Safe(() => d.canvasGroup);
            if (cg != null) { cg.alpha = visible ? 1f : 0f; cg.blocksRaycasts = visible; cg.interactable = visible; }
            var vis = Game.Safe(() => d.visuals);
            if (vis != null)
                foreach (var r in vis.GetComponentsInChildren<Renderer>(true)) r.enabled = visible;

            // 2) Taille nulle : rien ne dépasse, même si le jeu réanime la transparence
            var t = d.transform;
            if (!visible) { if (t.localScale != Vector3.zero) { _scales[d.Pointer] = t.localScale; t.localScale = Vector3.zero; } }
            else if (_scales.TryGetValue(d.Pointer, out var s)) { t.localScale = s; _scales.Remove(d.Pointer); }

            // 3) Hors de la mise en page de la main : pas de trou laissé par les dés des autres
            var le = d.GetComponent<UnityEngine.UI.LayoutElement>();
            if (le == null && !visible) le = d.gameObject.AddComponent<UnityEngine.UI.LayoutElement>();
            if (le != null) le.ignoreLayout = !visible;
        }

        private static readonly Dictionary<IntPtr, Vector3> _scales = new Dictionary<IntPtr, Vector3>();
    }
}
