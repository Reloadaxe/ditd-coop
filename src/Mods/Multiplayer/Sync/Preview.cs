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
    ///
    /// Chez les autres, les dégâts reçus sont AJOUTÉS au nombre du jeu : la flèche de cible (TargetEnemyArrow) au-dessus
    /// de l'ennemi. On intercepte ChangeToAttack (valeur + dégâts des autres) et ChangeToTargetOnly (si les autres
    /// attaquent cet ennemi, on passe quand même en mode attaque). Quand un aperçu distant change, on rejoue la
    /// dernière valeur locale pour rafraîchir le nombre.
    /// Ennemi visé par un autre mais pas par moi (pas de flèche) : petite étiquette « ⚔ X » à la couleur du joueur.
    /// </summary>
    public static class Preview
    {
        /// <summary>id du joueur → (index de combat de l'ennemi → dégâts prévus).</summary>
        private static readonly Dictionary<int, Dictionary<int, int>> _remote = new Dictionary<int, Dictionary<int, int>>();
        /// <summary>Flèche (pointeur natif) → dernière valeur locale demandée par le jeu (-1 = mode cible seule).</summary>
        private static readonly Dictionary<IntPtr, int> _arrowLocal = new Dictionary<IntPtr, int>();
        private static string _lastSent;
        private static float _nextCompute;
        private static bool _dirty;
        private static bool _inRefresh;
        private static TargetEnemySystem _system;

        public static void Reset()
        {
            bool had = _remote.Count > 0;
            _remote.Clear();
            _lastSent = null;
            if (had) _dirty = true;
        }

        // ───── Mon aperçu → réseau ─────

        public static void Update()
        {
            if (!Coop.Active || !Game.InBattle)
            {
                if (_remote.Count > 0) Reset();
                _arrowLocal.Clear();
                _system = null;
                return;
            }
            if (_dirty) { _dirty = false; RefreshArrows(); }
            if (Time.unscaledTime < _nextCompute) return;
            _nextCompute = Time.unscaledTime + 0.3f;

            var st = Game.TurnState;
            bool planning = st == BattleSystem.TurnState.SelectDice || st == BattleSystem.TurnState.PerformFlashDice
                            || st == BattleSystem.TurnState.UseRunestones || st == BattleSystem.TurnState.EnemyPhaseBeforePlayer;
            var local = planning ? ComputeLocal() : new Dictionary<int, int>();

            var text = string.Join(";", local.Where(kv => kv.Value > 0).OrderBy(kv => kv.Key).Select(kv => $"{kv.Key}:{kv.Value}"));
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
                int dmg = ArrowValue(values, e);
                if (dmg > 0) res[idx] = dmg;
            }
            return res;
        }

        // ───── Réseau → nombre du jeu ─────

        public static void OnRemote(Msg m)
        {
            var d = new Dictionary<int, int>();
            foreach (var part in (m.text ?? "").Split(';'))
            {
                var f = part.Split(':');
                if (f.Length == 2 && int.TryParse(f[0], out var idx) && int.TryParse(f[1], out var dmg)) d[idx] = dmg;
            }
            _remote[m.id] = d;
            _dirty = true;
        }

        private static int SafeIdx(Enemy e)
        {
            try { return Game.Battle.GetBattleIndexOfEntity(e); } catch { return -1; }
        }

        private static readonly DiceData.Type[] ArrowTypes =
            { DiceData.Type.Attack, DiceData.Type.AreaAttack, DiceData.Type.PunctureAttack, DiceData.Type.LethalAttack };

        /// <summary>
        /// Nombre de la flèche de cible, calculé comme le jeu (BattleSystem.ChangeBattleValues) : attaque, zone, perforante
        /// et létale visant cet ennemi, plus la part d'éclaboussure (GetSplashAttackValue : seulement la part reçue par une
        /// cible secondaire), passés dans un Hit de mon héros (modificateurs appliqués). Mes dés seuls : les copies des dés
        /// des autres sont exclues des valeurs. -1 si inconnu.
        /// </summary>
        private static int ArrowValue(BattleSystem.Values values, Enemy e)
        {
            if (values == null || e == null) return -1;
            try
            {
                int sum = 0;
                foreach (var t in ArrowTypes) sum += values.GetTargetActionAmountFromOwner(null, t, e, DiceData.Property.None);
                sum += values.GetSplashAttackValue(e, DiceData.Property.None);
                if (sum <= 0) return 0;
                var hit = new Hit(sum, false, Game.Player, false, false);
                return hit.DamageAmount;
            }
            catch { return -1; }
        }

        /// <summary>Somme des dégâts prévus par les AUTRES joueurs sur cet ennemi.</summary>
        private static int RemoteFor(Enemy e)
        {
            if (e == null || _remote.Count == 0) return 0;
            int idx = SafeIdx(e);
            if (idx < 0) return 0;
            int sum = 0;
            foreach (var kv in _remote)
                if (kv.Key != Coop.LocalId && kv.Value.TryGetValue(idx, out var dmg)) sum += dmg;
            return sum;
        }

        public static bool ArrowAttackPrefix(TargetEnemyArrow __instance, ref int __0)
        {
            try
            {
                if (!Coop.Active || __instance == null) return true;
                if (!_inRefresh) _arrowLocal[__instance.Pointer] = __0;
                var enemy = __instance.CurrentTargetEnemy;
                // Avec des copies de dés des autres sur mon plateau, le nombre du jeu peut déjà compter une partie de
                // leurs dégâts (ex. dégâts de zone sur les cibles secondaires), qui sont aussi dans l'aperçu reçu :
                // on repart alors de MES actions seules (les copies en sont exclues), plus l'aperçu des autres.
                if (Board.HasRealGhosts && enemy != null)
                {
                    int mine = ArrowValue(Game.Battle?.BattleValues, enemy);
                    if (mine >= 0) __0 = mine;
                }
                __0 += RemoteFor(enemy);
                if (__0 <= 0 && Board.HasRealGhosts)
                {
                    // Rien à attaquer pour l'équipe : flèche en mode « cible seule » (sans compter la copie)
                    bool was = _inRefresh;
                    _inRefresh = true;
                    try { __instance.ChangeToTargetOnly(); } finally { _inRefresh = was; }
                    return false;
                }
            }
            catch { }
            return true;
        }

        public static bool ArrowTargetOnlyPrefix(TargetEnemyArrow __instance)
        {
            try
            {
                if (!Coop.Active || __instance == null) return true;
                if (!_inRefresh) _arrowLocal[__instance.Pointer] = -1;
                if (RemoteFor(__instance.CurrentTargetEnemy) > 0)
                {
                    bool was = _inRefresh;
                    _inRefresh = true;
                    try { __instance.ChangeToAttack(0); } finally { _inRefresh = was; }
                    return false;
                }
            }
            catch { }
            return true;
        }

        private static IEnumerable<TargetEnemyArrow> Arrows()
        {
            if (_system == null) _system = UnityEngine.Object.FindObjectOfType<TargetEnemySystem>();
            if (_system == null) yield break;
            foreach (var a in new[] { _system.mainTargetArrow, _system.secondaryTargetArrow1, _system.secondaryTargetArrow2, _system.secondaryTargetArrow3 })
                if (a != null) yield return a;
        }

        /// <summary>Un aperçu distant a changé : on rejoue la dernière valeur locale de chaque flèche visible.</summary>
        private static void RefreshArrows()
        {
            foreach (var a in Arrows())
            {
                try
                {
                    if (!a.gameObject.activeInHierarchy || a.CurrentTargetEnemy == null) continue;
                    if (!_arrowLocal.TryGetValue(a.Pointer, out var local)) local = -1;
                    _inRefresh = true;
                    if (local >= 0) a.ChangeToAttack(local);
                    else a.ChangeToTargetOnly();
                }
                catch (Exception e) { MultiplayerMod.Instance.Logger.LogDebug("Preview arrow : " + e.Message); }
                finally { _inRefresh = false; }
            }
        }

        /// <summary>
        /// Ennemis qu'aucune de mes flèches ne vise mais que d'autres attaquent : position écran, joueurs et total
        /// (affiché par Ui/Hud.cs).
        /// </summary>
        public static List<(Vector2 screen, List<int> ids, int total)> RemoteMarkers()
        {
            var res = new List<(Vector2, List<int>, int)>();
            if (!Coop.Active || !Game.InBattle || _remote.Count == 0) return res;
            var cam = Camera.main;
            if (cam == null) return res;

            var targeted = new HashSet<IntPtr>();
            foreach (var a in Arrows())
                try { if (a.gameObject.activeInHierarchy && a.CurrentTargetEnemy != null) targeted.Add(a.CurrentTargetEnemy.Pointer); } catch { }

            var battle = Game.Battle;
            foreach (var e in Game.Enemies)
            {
                if (targeted.Contains(e.Pointer)) continue;
                int idx = SafeIdx(e);
                var ids = new List<int>();
                int total = 0;
                foreach (var kv in _remote.OrderBy(k => k.Key))
                    if (kv.Key != Coop.LocalId && kv.Value.TryGetValue(idx, out var dmg) && dmg > 0)
                    {
                        ids.Add(kv.Key);
                        total += dmg;
                    }
                if (total == 0) continue;

                float h = 2f;
                var sr = e.GetComponentInChildren<SpriteRenderer>();
                if (sr != null) h = Math.Min(4f, sr.bounds.size.y);
                var s = cam.WorldToScreenPoint(e.transform.position + new Vector3(0, h * 1.15f, 0));
                if (s.z < 0) continue;
                res.Add((new Vector2(s.x, s.y), ids, total));
            }
            return res;
        }
    }
}
