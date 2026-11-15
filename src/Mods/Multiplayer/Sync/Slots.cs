using System;
using System.Collections.Generic;
using System.Linq;
using DiceLoader;
using DiceMultiplayer.Net;
using UnityEngine;

namespace DiceMultiplayer.Sync
{
    /// <summary>
    /// Cases supplémentaires mises en commun. Chaque joueur peut gagner des cases en plus (relique qui active une case du
    /// bord à chaque tour, modificateur, boss…), tirées au hasard chez lui. Chacun envoie la liste de SES cases actives ;
    /// chez les autres, celles qui manquent sont activées (BattleBoard.ManageBoardSlotState), puis désactivées quand leur
    /// propriétaire ne les a plus (sauf si un dé y est encore posé). Les deux plateaux gardent ainsi la même forme.
    /// Une case absente de mon plateau (autre personnage, autre forme) est ignorée.
    /// </summary>
    public static class Slots
    {
        private static readonly Dictionary<int, HashSet<(int x, int y)>> _remote = new Dictionary<int, HashSet<(int, int)>>();
        /// <summary>Cases que j'ai activées pour les autres (elles ne font pas partie des miennes).</summary>
        private static readonly HashSet<(int x, int y)> _added = new HashSet<(int, int)>();
        private static string _lastSent;
        private static float _next;
        private static IntPtr _board;

        public static void Update()
        {
            if (!Coop.Active || !Game.InBattle)
            {
                _added.Clear();
                _remote.Clear();
                _lastSent = null;
                _board = IntPtr.Zero;
                return;
            }
            if (Time.unscaledTime < _next) return;
            _next = Time.unscaledTime + 0.25f;

            var board = UnityEngine.Object.FindObjectOfType<BattleBoard>();
            if (board == null) return;
            if (board.Pointer != _board) { _board = board.Pointer; _added.Clear(); }

            // Mes cases actives (sans celles ajoutées pour les autres) → réseau
            var active = new HashSet<(int, int)>();
            var mine = new List<string>();
            foreach (var s in board.ActiveBoardSlots)
            {
                if (s == null) continue;
                var c = s.CurrentCoord;
                active.Add((c.x, c.y));
                if (!_added.Contains((c.x, c.y))) mine.Add($"{c.x},{c.y}");
            }
            mine.Sort(StringComparer.Ordinal);
            var text = string.Join(";", mine);
            if (text != _lastSent)
            {
                _lastSent = text;
                Coop.Send(new Msg { t = T.BoardSlots, text = text });
            }

            // Cases des autres → mon plateau
            var wanted = new HashSet<(int, int)>();
            foreach (var kv in _remote)
                if (kv.Key != Coop.LocalId) wanted.UnionWith(kv.Value);

            bool changed = false;
            foreach (var c in wanted)
            {
                if (active.Contains(c)) continue;
                var slot = Game.Safe(() => board.GetSlotWithCoord(c.Item1, c.Item2));
                if (slot == null) continue;
                try
                {
                    bool on = true; var sl = slot;
                    board.ManageBoardSlotState(ref on, ref sl);
                    _added.Add(c);
                    changed = true;
                }
                catch (Exception e) { MultiplayerMod.Instance.Logger.LogDebug("Case ajoutée : " + e.Message); }
            }
            foreach (var c in _added.ToList())
            {
                if (wanted.Contains(c)) continue;
                var slot = Game.Safe(() => board.GetSlotWithCoord(c.Item1, c.Item2));
                if (slot == null) { _added.Remove(c); continue; }
                if (Game.Safe(() => slot.HasPlacedDice)) continue;   // un dé y est encore : on attend qu'il parte
                try { bool off = false; var sl = slot; board.ManageBoardSlotState(ref off, ref sl); } catch { }
                _added.Remove(c);
                changed = true;
            }
            if (changed) try { board.RefreshActiveBoardSlots(); } catch { }
        }

        public static void OnRemote(Msg m)
        {
            var set = new HashSet<(int, int)>();
            foreach (var part in (m.text ?? "").Split(';'))
            {
                var xy = part.Split(',');
                if (xy.Length == 2 && int.TryParse(xy[0], out var x) && int.TryParse(xy[1], out var y)) set.Add((x, y));
            }
            _remote[m.id] = set;
            _next = 0;   // appliqué tout de suite
        }

        public static void RemovePlayer(int id)
        {
            if (_remote.Remove(id)) _next = 0;
        }
    }
}
