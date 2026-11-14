using System;
using System.Collections.Generic;
using System.Linq;
using DiceLoader;
using DiceMultiplayer.Net;
using UnityEngine;

namespace DiceMultiplayer.Sync
{
    /// <summary>
    /// Aperçu des actions des autres joueurs sur les ennemis.
    ///
    /// Chez chaque joueur, le jeu calcule l'aperçu de SON plateau (BattleSystem.BattleValues). On lit, pour chaque ennemi,
    /// les dégâts prévus (GetTargetActionAmountFromPlayer(Attacks, ennemi)) et on les envoie quand ils changent.
    /// Chez les autres, au-dessus de chaque ennemi : une ligne par joueur à sa couleur, plus le total de l'équipe.
    /// </summary>
    public static class Preview
    {
        /// <summary>id du joueur → (index de combat de l'ennemi → dégâts prévus).</summary>
        private static readonly Dictionary<int, Dictionary<int, int>> _remote = new Dictionary<int, Dictionary<int, int>>();
        private static string _lastSent;
        private static float _nextCompute;
        private static Dictionary<int, int> _local = new Dictionary<int, int>();

        public static void Reset()
        {
            _remote.Clear();
            _local.Clear();
            _lastSent = null;
        }

        // ───── Mon aperçu → réseau ─────

        public static void Update()
        {
            if (!Coop.Active || !Game.InBattle) { if (_remote.Count > 0 || _local.Count > 0) Reset(); return; }
            if (Time.unscaledTime < _nextCompute) return;
            _nextCompute = Time.unscaledTime + 0.3f;

            var st = Game.TurnState;
            bool planning = st == BattleSystem.TurnState.SelectDice || st == BattleSystem.TurnState.PerformFlashDice
                            || st == BattleSystem.TurnState.UseRunestones || st == BattleSystem.TurnState.EnemyPhaseBeforePlayer;
            _local = planning ? ComputeLocal() : new Dictionary<int, int>();

            var text = string.Join(";", _local.Where(kv => kv.Value > 0).OrderBy(kv => kv.Key).Select(kv => $"{kv.Key}:{kv.Value}"));
            if (text == _lastSent) return;
            _lastSent = text;
            Coop.Send(new Msg { t = T.Preview, text = text });
        }

        private static Dictionary<int, int> ComputeLocal()
        {
            var res = new Dictionary<int, int>();
            var battle = Game.Battle;
            var values = Game.Safe(() => battle?.BattleValues);
            if (values == null) return res;
            foreach (var e in Game.Enemies)
            {
                int idx = Game.Safe(() => battle.GetBattleIndexOfEntity(e));
                int dmg = Game.Safe(() => values.GetTargetActionAmountFromPlayer(DiceData.GeneralType.Attacks, e));
                if (dmg > 0) res[idx] = dmg;
            }
            return res;
        }

        // ───── Réseau → affichage ─────

        public static void OnRemote(Msg m)
        {
            var d = new Dictionary<int, int>();
            foreach (var part in (m.text ?? "").Split(';'))
            {
                var f = part.Split(':');
                if (f.Length == 2 && int.TryParse(f[0], out var idx) && int.TryParse(f[1], out var dmg)) d[idx] = dmg;
            }
            _remote[m.id] = d;
        }

        /// <summary>OnGUI, phase Repaint : au-dessus de chaque ennemi, l'aperçu de chaque autre joueur et le total.</summary>
        public static void Draw()
        {
            if (!Coop.Active || !Game.InBattle || _remote.Count == 0) return;
            var cam = Camera.main;
            if (cam == null) return;
            var battle = Game.Battle;
            foreach (var e in Game.Enemies)
            {
                int idx = Game.Safe(() => battle.GetBattleIndexOfEntity(e));
                var lines = new List<(int id, int dmg)>();
                foreach (var kv in _remote.OrderBy(k => k.Key))
                    if (kv.Key != Coop.LocalId && kv.Value.TryGetValue(idx, out var dmg) && dmg > 0) lines.Add((kv.Key, dmg));
                if (lines.Count == 0) continue;
                _local.TryGetValue(idx, out var mine);
                int total = mine + lines.Sum(l => l.dmg);

                float h = 2f;
                var sr = e.GetComponentInChildren<SpriteRenderer>();
                if (sr != null) h = Math.Min(4f, sr.bounds.size.y);
                var s = cam.WorldToScreenPoint(e.transform.position + new Vector3(0, h * 1.15f, 0));
                if (s.z < 0) continue;

                int rows = lines.Count + 1;
                var r = new Rect(s.x - 70, Screen.height - s.y - rows * 20 - 8, 140, rows * 20 + 6);
                GUI.Box(r, ""); GUI.Box(r, "");
                float y = r.y + 3;
                foreach (var l in lines)
                {
                    GUI.Label(new Rect(r.x + 8, y, 130, 20), $"<b><color={Coop.ColorOf(l.id)}>{Coop.NameOf(l.id)}</color></b>  ⚔ {l.dmg}");
                    y += 20;
                }
                bool lethal = total >= e.CurrentHealth + e.CurrentBlock;
                GUI.Label(new Rect(r.x + 8, y, 130, 20), $"<b>Total ⚔ {total}</b>{(lethal ? "  <color=#ff6060>☠</color>" : "")}");
            }
        }
    }
}
