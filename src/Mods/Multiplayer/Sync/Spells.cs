using System;
using System.Collections.Generic;
using System.Linq;
using DiceLoader;
using DiceMultiplayer.Net;
using UnityEngine;

namespace DiceMultiplayer.Sync
{
    /// <summary>
    /// Sorts d'ennemis posés sur le plateau (ex. points faibles « ×2 » du boss scorpion : WeakSpotSpellData).
    /// Le sort tire sa case avec le générateur des ennemis (commun), mais dans la liste des cases disponibles, qui dépend
    /// du plateau de chacun : la case différait d'un joueur à l'autre. L'hôte fait donc foi : il lance le sort normalement
    /// et envoie chaque case touchée (coordonnées, rang, direction) ; chez les autres, le lancer local est retenu et les
    /// cases de l'hôte sont appliquées à la place (et deviennent les cases du sort).
    /// </summary>
    public static class Spells
    {
        private static SpellData _localSpell;
        private static readonly List<Msg> _queue = new List<Msg>();
        private static bool _applying;

        public static void Reset()
        {
            _localSpell = null;
            _queue.Clear();
        }

        /// <summary>Préfixe de BoardSlot.CastWeakSpotSpell. Renvoie false pour bloquer (clients).</summary>
        public static bool OnCast(BoardSlot slot, SpellData spell, int index, BoardSlot.BoardDirection dir)
        {
            if (!Coop.Active || _applying || slot == null) return true;
            if (Coop.IsHost)
            {
                var c = slot.CurrentCoord;
                Coop.Send(new Msg { t = T.Spell, sx = c.x, sy = c.y, btn = index, uid = (int)dir, turn = Game.Turn });
                Coop.AddLog($"[sync] Sort sur la case {c.x},{c.y} (rang {index}) envoyé.");
                return true;
            }
            // Client : on garde le sort (pour l'appliquer aux cases de l'hôte), sans le lancer sur la case tirée ici
            if (_localSpell == null || _localSpell.Pointer != spell.Pointer || index == 0)
            {
                _localSpell = spell;
                ClearSlots(spell);
            }
            Flush();
            return false;
        }

        /// <summary>Cases du dernier lancer de l'hôte (les seules qui comptent pour le sort chez moi).</summary>
        private static readonly HashSet<(int, int)> _hostSlots = new HashSet<(int, int)>();

        public static void OnRemote(Msg m)
        {
            if (Coop.IsHost) return;
            if (m.btn == 0) _hostSlots.Clear();   // nouveau lancer
            _hostSlots.Add((m.sx, m.sy));
            _queue.Add(m);
            Flush();
        }

        /// <summary>Chaque frame en combat (clients) : le sort ne garde que les cases de l'hôte.</summary>
        public static void Update()
        {
            if (!Coop.Active || Coop.IsHost || _localSpell == null || !Game.InBattle) return;
            try
            {
                var ws = _localSpell.TryCast<WeakSpotSpellData>();
                var list = ws?.currentSlots;
                if (list == null) return;
                for (int i = list.Count - 1; i >= 0; i--)
                {
                    var s = list[i];
                    if (s == null) { list.RemoveAt(i); continue; }
                    var c = s.CurrentCoord;
                    if (!_hostSlots.Contains((c.x, c.y))) list.RemoveAt(i);
                }
            }
            catch { }
        }

        private static void Flush()
        {
            if (_localSpell == null || _queue.Count == 0 || !Game.InBattle) return;
            foreach (var m in _queue.ToList())
            {
                _queue.Remove(m);
                var slot = UnityEngine.Object.FindObjectsOfType<BoardSlot>()
                    .FirstOrDefault(s => { var c = s.CurrentCoord; return c.x == m.sx && c.y == m.sy && s.gameObject.activeInHierarchy; });
                if (slot == null) { Coop.AddLog($"[sync] Case du sort {m.sx},{m.sy} introuvable."); continue; }
                _applying = true;
                try
                {
                    slot.CastWeakSpotSpell(_localSpell, m.btn, (BoardSlot.BoardDirection)m.uid);
                    var ws = _localSpell.TryCast<WeakSpotSpellData>();
                    if (ws?.currentSlots != null && !ws.currentSlots.Contains(slot)) ws.currentSlots.Add(slot);
                    Coop.AddLog($"[sync] Sort de l'hôte appliqué sur la case {m.sx},{m.sy}.");
                }
                catch (Exception e) { MultiplayerMod.Instance.Logger.LogWarning("Sort distant : " + e.Message); }
                finally { _applying = false; }
            }
        }

        /// <summary>Retire les cases tirées localement de la liste du sort (elles seront remplacées par celles de l'hôte).</summary>
        private static void ClearSlots(SpellData spell)
        {
            try
            {
                var ws = spell.TryCast<WeakSpotSpellData>();
                ws?.currentSlots?.Clear();
            }
            catch { }
        }
    }
}
