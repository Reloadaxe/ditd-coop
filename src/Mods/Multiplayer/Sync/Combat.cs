using System;
using System.Collections.Generic;
using System.Linq;
using DiceLoader;
using DiceMultiplayer.Net;

namespace DiceMultiplayer.Sync
{
    /// <summary>
    /// Ennemis partagés.
    ///  - Les mêmes ennemis existent chez tous (même seed, même combat).
    ///  - Quand TON héros inflige des dégâts à un ennemi, on l'envoie (index de combat de l'ennemi, dégâts, ignore-bloc).
    ///    Chez les autres, le même coup est appliqué à leur copie de l'ennemi.
    ///  - Au début de chaque tour, l'hôte envoie les PV et le bloc de référence de chaque ennemi, et chacun corrige
    ///    sa copie (filet de sécurité si un coup a été raté ou appliqué deux fois).
    ///  - Les attaques ennemies : chaque jeu fait attaquer son propre héros. Les ennemis frappent donc tous les joueurs.
    /// </summary>
    public static class Combat
    {
        /// <summary>Vrai pendant qu'on applique un coup venu du réseau : il n'est pas renvoyé.</summary>
        public static bool ApplyingRemote;
        private static int _logged;

        // ───── Local → réseau ─────

        /// <summary>Postfix de Enemy._Damage : un coup de MON héros sur un ennemi est partagé.</summary>
        public static void OnEnemyDamaged(Enemy enemy, Hit hit)
        {
            if (!Coop.Active || ApplyingRemote || enemy == null || hit == null) return;
            var attacker = Game.Safe(() => hit.Attacker);
            var local = Game.Player;
            // On ne partage que les coups de mon héros (les dés de mon plateau, mes reliques comptent comme lui)
            if (attacker == null || local == null || attacker.Pointer != local.Pointer) return;
            int idx = Game.Safe(() => Game.Battle.GetBattleIndexOfEntity(enemy));
            Coop.Send(new Msg
            {
                t = T.EnemyDamage, uid = idx, btn = Game.Safe(() => hit.BaseDamage),
                flag = Game.Safe(() => hit.IgnoreBlock), x = Game.Safe(() => hit.IsForceKill) ? 1 : 0,
            });
        }

        // ───── Réseau → mes ennemis ─────

        public static void OnRemoteDamage(Msg m)
        {
            if (!Game.InBattle) return;
            var e = Game.Safe(() => Game.Battle.GetEntityWithBattleIndex(m.uid, Entity.Side.Enemy));
            if (e == null || e.IsDyingOrDead) return;
            ApplyingRemote = true;
            try
            {
                // Sans attaquant : mes propres bonus (reliques, modificateurs) ne s'appliquent pas aux dégâts d'un autre joueur
                Hit hit;
                try { hit = new Hit(m.btn, m.x > 0, null, false, m.flag); }
                catch { hit = new Hit(m.btn, m.x > 0, Game.Player, false, m.flag); }
                MEC.Timing.RunCoroutine(e._Damage(hit));
                if (_logged++ < 5) Coop.AddLog($"[sync] {Coop.NameOf(m.id)} inflige {m.btn} à l'ennemi {m.uid}.");
            }
            catch (Exception ex) { MultiplayerMod.Instance.Logger.LogWarning("Dégâts distants : " + ex.Message); }
            finally { ApplyingRemote = false; }
        }

        // ───── Fin d'exécution commune ─────
        //
        // Chaque joueur exécute SES dés chez lui ; ses coups arrivent chez les autres par le réseau, un peu plus tard.
        // Sans attente, un jeu dont les dés n'avaient pas tué les ennemis passait directement à la phase ennemie : les
        // ennemis encore en vie attaquaient, puis mouraient sous les coups des autres arrivés juste après.
        // On retient donc la phase ennemie (EnemyPhaseAfterPlayer) jusqu'à ce que tous les joueurs aient fini d'exécuter
        // leurs dés (message ExecDone, envoyé après leurs coups), plus un court délai pour appliquer les derniers coups.

        private static readonly Dictionary<string, HashSet<int>> _execDone = new Dictionary<string, HashSet<int>>();
        private static readonly Dictionary<string, float> _execLast = new Dictionary<string, float>();
        private static BattleSystem _pendingBattle;
        private static string _pendingKey;
        private static float _pendingSince;
        private static bool _releasing;
        private const float ExecGrace = 0.6f, ExecTimeout = 8f;

        private static string ExecKey(int floor, int turn) => $"{floor}:{turn}";

        private static void MarkDone(string key, int id)
        {
            if (!_execDone.TryGetValue(key, out var set)) _execDone[key] = set = new HashSet<int>();
            set.Add(id);
            _execLast[key] = UnityEngine.Time.unscaledTime;
        }

        private static bool AllDone(string key) =>
            _execDone.TryGetValue(key, out var set) && Coop.Net.Players.All(p => set.Contains(p.Id));

        public static void OnRemoteExecDone(Msg m) => MarkDone(ExecKey(m.uid, m.turn), m.id);

        /// <summary>Préfixe de BattleSystem.ChangeState : retient l'entrée en phase ennemie après l'exécution des dés.</summary>
        public static bool OnBattleChangeState(BattleSystem battle, BattleSystem.TurnState next)
        {
            if (!Coop.Active || _releasing || next != BattleSystem.TurnState.EnemyPhaseAfterPlayer) return true;
            if (Coop.Net.Players.Count < 2) return true;
            var key = ExecKey(MultiplayerMod.CurrentFloorKey, Game.Turn);
            MarkDone(key, Coop.LocalId);
            Coop.Send(new Msg { t = T.ExecDone, uid = MultiplayerMod.CurrentFloorKey, turn = Game.Turn });
            _pendingBattle = battle;
            _pendingKey = key;
            _pendingSince = UnityEngine.Time.unscaledTime;
            return false;   // reprise dans UpdateExecWait
        }

        /// <summary>Chaque frame : relance la phase ennemie quand tout le monde a fini (ou après le délai maximum).</summary>
        public static void UpdateExecWait()
        {
            if (_pendingBattle == null) return;
            float now = UnityEngine.Time.unscaledTime;
            bool ready = AllDone(_pendingKey) && now - _execLast[_pendingKey] >= ExecGrace;
            bool timeout = now - _pendingSince > ExecTimeout;
            if (!ready && !timeout) return;
            var battle = _pendingBattle;
            _pendingBattle = null;
            if (timeout && !ready) Coop.AddLog("[sync] Fin d'exécution des autres joueurs non reçue : phase ennemie lancée quand même.");
            // Le combat a pu se terminer entre-temps (derniers ennemis tués par les autres) : rien à relancer
            var st = Game.Safe(() => battle.CurrentTurnState);
            if (st != BattleSystem.TurnState.ExecuteDice) return;
            _releasing = true;
            try { battle.ChangeState(BattleSystem.TurnState.EnemyPhaseAfterPlayer); }
            catch (Exception e) { MultiplayerMod.Instance.Logger.LogWarning("Reprise de la phase ennemie : " + e.Message); }
            finally { _releasing = false; }
        }

        // ───── Statuts des ennemis (poison, faiblesse…) ─────

        /// <summary>Phases où ce sont MES dés (ou mes runes) qui agissent : un statut posé alors vient de moi.</summary>
        private static bool PlayerPhase()
        {
            var st = Game.TurnState;
            return st == BattleSystem.TurnState.SelectDice || st == BattleSystem.TurnState.PerformFlashDice
                || st == BattleSystem.TurnState.UseRunestones || st == BattleSystem.TurnState.PreExecuteDice
                || st == BattleSystem.TurnState.ExecuteDice;
        }

        /// <summary>
        /// Préfixe de Entity.AddModifier. Un statut posé sur un ennemi pendant mes phases de dés est partagé. Les statuts que
        /// les ennemis se donnent eux-mêmes arrivent pendant leurs phases et existent déjà chez tous (même seed).
        /// Les effets périodiques (dégâts du poison, baisse des tours) tournent ensuite nativement chez chacun ; les dégâts
        /// du poison n'ont pas d'attaquant et ne sont donc pas renvoyés. L'hôte corrige les écarts à chaque tour.
        /// </summary>
        public static void OnModifierAdded(Entity target, ModifierData md)
        {
            if (!Coop.Active || ApplyingRemote || target == null || md == null || !Game.InBattle) return;
            var enemy = target.TryCast<Enemy>();
            if (enemy == null || enemy.isDummy || md.isPreview || !PlayerPhase()) return;
            int idx; try { idx = Game.Battle.GetBattleIndexOfEntity(enemy); } catch { return; }
            if (idx < 0) return;
            Coop.Send(new Msg { t = T.EnemyMod, uid = idx, btn = (int)md.type, sx = md.value, sy = md.multiplier, flag = md.haveTurnLimit });
        }

        public static void OnRemoteModifier(Msg m)
        {
            if (!Game.InBattle) return;
            var e = Game.Safe(() => Game.Battle.GetEntityWithBattleIndex(m.uid, Entity.Side.Enemy));
            if (e == null || e.IsDyingOrDead) return;
            ApplyingRemote = true;
            try
            {
                var md = ModifierExt.GetModifierData((ModifierData.Modifier)m.btn, e, m.sx);
                if (md == null) return;
                if (m.sy != 0) md.multiplier = m.sy;
                md.haveTurnLimit = m.flag;
                e.AddModifier(md);
                e.RefreshModifierVisuals();
                if (_logged++ < 10) Coop.AddLog($"[sync] {Coop.NameOf(m.id)} pose {(ModifierData.Modifier)m.btn} {m.sx} sur l'ennemi {m.uid}.");
            }
            catch (Exception ex) { MultiplayerMod.Instance.Logger.LogWarning("Statut distant : " + ex.Message); }
            finally { ApplyingRemote = false; }
        }

        private static string ModsOf(Enemy e)
        {
            var parts = new List<string>();
            try
            {
                foreach (var md in e.modifiers)
                    if (md != null && !md.isPreview) parts.Add($"{(int)md.type}={md.value}");
            }
            catch { }
            return string.Join(",", parts);
        }

        /// <summary>Aligne les statuts de mon ennemi sur ceux de l'hôte (ajout, valeur, retrait).</summary>
        private static int FixModifiers(Enemy e, string hostMods)
        {
            var want = new Dictionary<int, int>();
            foreach (var p in (hostMods ?? "").Split(','))
            {
                var kv = p.Split('=');
                if (kv.Length == 2 && int.TryParse(kv[0], out var t) && int.TryParse(kv[1], out var v)) want[t] = v;
            }
            int fixes = 0;
            ApplyingRemote = true;
            try
            {
                var have = new Dictionary<int, ModifierData>();
                foreach (var md in e.modifiers) if (md != null && !md.isPreview && !have.ContainsKey((int)md.type)) have[(int)md.type] = md;
                foreach (var kv in want)
                {
                    if (have.TryGetValue(kv.Key, out var md))
                    {
                        if (md.value != kv.Value) { md.value = kv.Value; fixes++; }
                    }
                    else
                    {
                        var nmd = ModifierExt.GetModifierData((ModifierData.Modifier)kv.Key, e, kv.Value);
                        if (nmd != null) { e.AddModifier(nmd); fixes++; }
                    }
                }
                foreach (var kv in have)
                    if (!want.ContainsKey(kv.Key)) { e.RemoveModifier(kv.Value); fixes++; }
                if (fixes > 0) e.RefreshModifierVisuals();
            }
            catch (Exception ex) { MultiplayerMod.Instance.Logger.LogDebug("Correction statuts : " + ex.Message); }
            finally { ApplyingRemote = false; }
            return fixes;
        }

        // ───── Correction par l'hôte ─────

        private static string _lastSyncKey;

        /// <summary>Une fois par tour, en phase de choix des dés (effets de début de tour terminés).</summary>
        public static void HostBroadcastOncePerTurn()
        {
            var key = $"{Game.Battle?.Pointer}:{Game.Turn}";
            if (key == _lastSyncKey) return;
            _lastSyncKey = key;
            HostBroadcastEnemies();
        }

        public static void HostBroadcastEnemies()
        {
            if (!Coop.Active || !Coop.IsHost || !Game.InBattle) return;
            var parts = new List<string>();
            foreach (var e in AllEnemies())
            {
                int idx = Game.Safe(() => Game.Battle.GetBattleIndexOfEntity(e));
                parts.Add($"{idx}:{(e.IsDyingOrDead ? -1 : e.CurrentHealth)}:{e.CurrentBlock}:{ModsOf(e)}");
            }
            Coop.Send(new Msg { t = T.EnemySync, text = string.Join(";", parts), turn = Game.Turn });
        }

        public static void OnEnemySync(Msg m)
        {
            if (Coop.IsHost || !Game.InBattle || string.IsNullOrEmpty(m.text)) return;
            int fixedCount = 0;
            foreach (var part in m.text.Split(';'))
            {
                var f = part.Split(':');
                if (f.Length < 3 || !int.TryParse(f[0], out var idx) || !int.TryParse(f[1], out var hp) || !int.TryParse(f[2], out var blk)) continue;
                var e = Game.Safe(() => Game.Battle.GetEntityWithBattleIndex(idx, Entity.Side.Enemy));
                if (e == null || e.IsDyingOrDead) continue;
                try
                {
                    if (hp < 0)
                    {
                        ApplyingRemote = true;
                        try { MEC.Timing.RunCoroutine(e._DamageForceKill()); } finally { ApplyingRemote = false; }
                        fixedCount++;
                        continue;
                    }
                    if (e.CurrentHealth != hp) { e.ChangeCurrentHealth(hp); e.ForceHealthRefresh(); fixedCount++; }
                    if (e.CurrentBlock != blk) { e.ChangeBlockValue(blk); fixedCount++; }
                    if (f.Length >= 4) fixedCount += FixModifiers(e.Cast<Enemy>(), f[3]);
                }
                catch { }
            }
            if (fixedCount > 0) Coop.AddLog($"[sync] {fixedCount} correction(s) d'ennemis reçues de l'hôte.");
        }

        private static IEnumerable<Enemy> AllEnemies()
        {
            foreach (var e in UnityEngine.Object.FindObjectsOfType<Enemy>())
                if (e != null && !e.isDummy) yield return e;
        }

        public static string HashPart() =>
            string.Join(";", AllEnemies().Select(e => (Game.Safe(() => Game.Battle.GetBattleIndexOfEntity(e)), e.IsDyingOrDead ? -1 : e.CurrentHealth))
                                       .OrderBy(t => t.Item1).Select(t => $"{t.Item1}:{t.Item2}"));
    }
}
