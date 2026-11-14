using System;
using System.Collections.Generic;
using System.Linq;
using DiceLoader;
using DiceMultiplayer.Net;
using UnityEngine;

namespace DiceMultiplayer.Sync
{
    /// <summary>
    /// Plateau partagé.
    ///
    /// Chaque joueur pose SES dés sur SON plateau (logique native du jeu). La pose est envoyée aux autres,
    /// qui affichent un « fantôme » du dé sur la même case, à la couleur du joueur. Une case occupée par
    /// un autre joueur est bloquée : on ne peut pas y poser de dé.
    /// Les effets d'un dé (dégâts aux ennemis) sont appliqués par son propriétaire, puis partagés via Combat.
    /// </summary>
    public static class Board
    {
        public sealed class Ghost
        {
            public int PlayerId, Uid, X, Y, Value;
            public string Name;
        }

        private static readonly List<Ghost> _ghosts = new List<Ghost>();

        public static void Reset() => _ghosts.Clear();

        public static bool IsTakenByOther(int x, int y) => _ghosts.Any(g => g.X == x && g.Y == y);

        // ───── Local → réseau ─────

        /// <summary>Préfixe de BattleBoard.PlaceDiceOnBoard (contexte Player). Renvoie false si la case est déjà prise par un autre joueur.</summary>
        public static bool OnLocalPlace(BoardSlot slot, GameObjectDice dice)
        {
            if (!Coop.Active || slot == null || dice == null) return true;
            var c = slot.CurrentCoord;
            var taken = _ghosts.FirstOrDefault(g => g.X == c.x && g.Y == c.y);
            if (taken != null) { Coop.Toast($"Case déjà prise par {Coop.NameOf(taken.PlayerId)}"); return false; }
            var ad = Game.Safe(() => dice.CurrentActiveDice);
            if (ad == null) return true;
            Coop.Send(new Msg
            {
                t = T.BoardPlace, uid = Key(dice), sx = c.x, sy = c.y,
                text = Game.Safe(() => ad.diceDataName) ?? "?", btn = Game.Safe(() => ad.CurrentFaceValue),
            });
            return true;
        }

        public static void OnLocalRemove(GameObjectDice dice)
        {
            if (!Coop.Active || dice == null) return;
            Coop.Send(new Msg { t = T.BoardRemove, uid = Key(dice) });
        }

        /// <summary>Fin du tour : mes dés quittent le plateau chez les autres.</summary>
        public static void OnLocalTurnEnd()
        {
            if (!Coop.Active) return;
            Coop.Send(new Msg { t = T.BoardClear });
        }

        /// <summary>Identifiant d'un dé pour cette pose : le pointeur natif tronqué suffit (unique pendant le tour).</summary>
        private static int Key(GameObjectDice d) => unchecked((int)(d.Pointer.ToInt64() >> 3));

        // ───── Réseau → affichage ─────

        public static void OnRemotePlace(Msg m)
        {
            _ghosts.RemoveAll(g => g.PlayerId == m.id && g.Uid == m.uid);
            _ghosts.Add(new Ghost { PlayerId = m.id, Uid = m.uid, X = m.sx, Y = m.sy, Name = m.text, Value = m.btn });
        }

        public static void OnRemoteRemove(Msg m) => _ghosts.RemoveAll(g => g.PlayerId == m.id && g.Uid == m.uid);
        public static void OnRemoteClear(Msg m) => _ghosts.RemoveAll(g => g.PlayerId == m.id);

        public static void DrawGhosts()
        {
            if (_ghosts.Count == 0 || !Game.InBattle) return;
            var slots = UnityEngine.Object.FindObjectsOfType<BoardSlot>();
            foreach (var g in _ghosts)
            {
                BoardSlot slot = null;
                foreach (var s in slots) { var c = s.CurrentCoord; if (c.x == g.X && c.y == g.Y && s.gameObject.activeInHierarchy) { slot = s; break; } }
                if (slot == null) continue;
                var sp = ScreenPos(slot.transform);
                if (sp == null) continue;
                var r = new Rect(sp.Value.x - 34, Screen.height - sp.Value.y - 34, 68, 68);
                GUI.Box(r, "");
                GUI.Box(r, "");
                GUI.Label(new Rect(r.x + 4, r.y + 4, 64, 22), $"<b><color={Coop.ColorOf(g.PlayerId)}>{Coop.NameOf(g.PlayerId)}</color></b>");
                GUI.Label(new Rect(r.x + 4, r.y + 22, 64, 26), $"<size=18><b>{g.Value}</b></size>");
                GUI.Label(new Rect(r.x + 4, r.y + 46, 64, 22), $"<size=10>{Short(g.Name)}</size>");
            }
        }

        private static string Short(string s) => string.IsNullOrEmpty(s) ? "" : (s.Length > 10 ? s.Substring(0, 10) : s);

        public static Vector2? ScreenPos(Transform t)
        {
            var canvas = t.GetComponentInParent<Canvas>();
            if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay) return t.position;
            var cam = canvas != null && canvas.worldCamera != null ? canvas.worldCamera : Camera.main;
            if (cam == null) return null;
            var p = cam.WorldToScreenPoint(t.position);
            return p.z < 0 ? (Vector2?)null : new Vector2(p.x, p.y);
        }
    }
}
