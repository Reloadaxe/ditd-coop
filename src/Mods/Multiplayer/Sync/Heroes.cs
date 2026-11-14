using System;
using System.Collections.Generic;
using System.Linq;
using DiceLoader;
using DiceMultiplayer.Net;
using UnityEngine;

namespace DiceMultiplayer.Sync
{
    /// <summary>
    /// Héros des autres joueurs, affichés à côté du tien.
    ///
    /// Chaque joueur joue son héros dans SA run. Chez toi, les héros des autres sont des visuels :
    /// le prefab de leur personnage (GameData.GetPlayerData), instancié sous un parent inactif pour
    /// qu'aucun script ne démarre, puis débarrassé de ses scripts, canvas et colliders. Il ne reste que
    /// les sprites et l'Animator, avec le skin choisi.
    /// Leurs PV et leur bloc viennent du réseau : chaque joueur envoie l'état de SON héros (message HeroState).
    /// Le visuel reprend l'animation de ton héros, pour suivre le rythme du combat.
    /// </summary>
    public static class Heroes
    {
        public sealed class Remote
        {
            public int PlayerId;
            public GameObject Go;
            public Animator Anim;
            public int LastState;
            public Vector3 Offset;
            public int Hp, MaxHp, Block;
            public bool KO;
            public float LastUpdate;
        }

        private static readonly Dictionary<int, Remote> _remotes = new Dictionary<int, Remote>();
        private static Player _local;
        private static float _nextSend, _nextCheck;
        private static string _lastSent;

        public static IEnumerable<Remote> All => _remotes.Values;

        private static bool InScene(Player p) => p != null && Game.Safe(() => p.gameObject.activeInHierarchy)
            && (Game.State == GameManager.GameState.BattleFloor || Game.State == GameManager.GameState.EventFloor
                || Game.State == GameManager.GameState.ShopFloor || Game.State == GameManager.GameState.LootFloor);

        public static void Reset()
        {
            DespawnAll();
            _remotes.Clear();
            _lastSent = null;
        }

        // ───────────── État de mon héros → réseau ─────────────

        private static void SendLocalState()
        {
            if (Time.unscaledTime < _nextSend) return;
            _nextSend = Time.unscaledTime + 0.25f;
            var p = Game.Player;
            if (p == null) return;
            int hp = Game.Safe(() => p.CurrentHealth), max = Game.Safe(() => p.MaxHealth), blk = Game.Safe(() => p.CurrentBlock);
            bool ko = Game.Safe(() => p.IsDyingOrDead);
            string key = $"{hp}/{max}/{blk}/{ko}";
            if (key == _lastSent && Time.unscaledTime % 2f > 0.3f) return;   // pas de doublon, sauf un rappel régulier
            _lastSent = key;
            Coop.Send(new Msg { t = T.HeroState, h = hp, sx = max, sy = blk, flag = ko });
        }

        public static void OnRemoteState(Msg m)
        {
            if (!_remotes.TryGetValue(m.id, out var r)) _remotes[m.id] = r = new Remote { PlayerId = m.id };
            r.Hp = (int)m.h; r.MaxHp = m.sx; r.Block = m.sy; r.KO = m.flag; r.LastUpdate = Time.unscaledTime;
        }

        // ───────────── Visuels ─────────────

        public static void Update()
        {
            if (!Coop.Active || !Game.InRun) { DespawnAll(); return; }
            SendLocalState();

            var local = Game.Player;
            if (!InScene(local)) { DespawnAll(); return; }
            if (_local == null || _local.Pointer != local.Pointer) { DespawnAll(); _local = local; }

            // Suivi de position et miroir d'animation, à chaque frame
            int state = 0; float norm = 0;
            var anim = Game.Safe(() => local.GetComponentInChildren<Animator>());
            if (anim != null) { var info = anim.GetCurrentAnimatorStateInfo(0); state = info.fullPathHash; norm = info.normalizedTime % 1f; }
            foreach (var r in _remotes.Values)
            {
                if (r.Go == null) continue;
                r.Go.transform.position = local.transform.position + r.Offset;
                if (r.Go.activeSelf == r.KO) r.Go.SetActive(!r.KO);
                if (r.Anim != null && state != 0 && state != r.LastState) { r.LastState = state; try { r.Anim.Play(state, 0, norm); } catch { } }
            }

            if (Time.unscaledTime < _nextCheck) return;
            _nextCheck = Time.unscaledTime + 0.5f;
            int k = 1;
            foreach (var pick in Team.RunPicks.Where(p => p.Id != Coop.LocalId))
            {
                if (!_remotes.TryGetValue(pick.Id, out var r)) _remotes[pick.Id] = r = new Remote { PlayerId = pick.Id };
                if (r.Go == null) Spawn(r, pick, local, k);
                k++;
            }
        }

        private static void Spawn(Remote r, PlayerPick pick, Player local, int index)
        {
            try
            {
                var pd = Game.Manager.ActiveGameData.GetPlayerData((Player.CharacterName)pick.Run.Character);
                if (pd.prefab == null) return;

                var holder = new GameObject("DIM_RemoteHolder");
                holder.SetActive(false);   // parent inactif : aucun Awake/OnEnable ne s'exécute sur le clone
                var go = UnityEngine.Object.Instantiate(pd.prefab, holder.transform);
                go.name = $"DIM_Remote_{pick.Id}";
                foreach (var mb in go.GetComponentsInChildren<MonoBehaviour>(true).ToArray().Reverse()) try { UnityEngine.Object.DestroyImmediate(mb); } catch { }
                foreach (var c in go.GetComponentsInChildren<Canvas>(true).ToArray()) try { if (c != null && c.gameObject != go) UnityEngine.Object.DestroyImmediate(c.gameObject); } catch { }
                foreach (var col in go.GetComponentsInChildren<Collider2D>(true).ToArray()) try { UnityEngine.Object.DestroyImmediate(col); } catch { }

                r.Anim = go.GetComponentInChildren<Animator>(true);
                try
                {
                    var skin = pd.data.GetSkinData((PlayerSkinData.Name)pick.Run.Skin) ?? pd.data.GetSkinData(pd.data.defaultSkinName);
                    if (r.Anim != null && skin != null && skin.animatorController != null) r.Anim.runtimeAnimatorController = skin.animatorController;
                }
                catch { }

                // Derrière ton héros, décalage réduit tant qu'il sortirait du champ de la caméra
                float face = Game.Safe(() => local.FaceDirection);
                if (Math.Abs(face) < 0.01f) face = 1f;
                float step = 2.2f;
                var cam = Camera.main;
                for (int t = 0; t < 8 && cam != null; t++)
                {
                    var vp = cam.WorldToViewportPoint(local.transform.position + new Vector3(-face * step * index, 0, 0));
                    if (vp.x > 0.06f && vp.x < 0.94f) break;
                    step *= 0.75f;
                }
                r.Offset = new Vector3(-face * step * index, 0.15f * index, 0.01f * index);

                go.transform.SetParent(local.transform.parent, true);
                go.transform.position = local.transform.position + r.Offset;
                go.transform.localScale = local.transform.localScale;
                UnityEngine.Object.Destroy(holder);
                go.SetActive(true);
                r.Go = go;
                Coop.AddLog($"Héros de {Coop.NameOf(pick.Id)} affiché ({(Player.CharacterName)pick.Run.Character}).");
            }
            catch (Exception e) { Coop.AddLog($"Affichage du héros de {Coop.NameOf(pick.Id)} impossible : {e.Message}"); }
        }

        public static void DespawnAll()
        {
            foreach (var r in _remotes.Values) { if (r.Go != null) try { UnityEngine.Object.Destroy(r.Go); } catch { } r.Go = null; }
            _local = null;
        }

        // ───────────── Affichage (OnGUI, phase Repaint) ─────────────

        public static void DrawLabels()
        {
            var cam = Camera.main;
            if (cam == null || !Coop.Active || _local == null) return;
            Label(cam, _local.transform, $"<b><color={Coop.ColorOf(Coop.LocalId)}>{Coop.NameOf(Coop.LocalId)}</color></b>");
            foreach (var r in _remotes.Values)
            {
                if (r.Go == null && !r.KO) continue;
                var t = r.Go != null ? r.Go.transform : _local.transform;
                string stats = r.KO ? "<color=#ff6060>K.O.</color>" : $"♥ {r.Hp}/{r.MaxHp}" + (r.Block > 0 ? $"  ⛨ {r.Block}" : "");
                Label(cam, t, $"<b><color={Coop.ColorOf(r.PlayerId)}>{Coop.NameOf(r.PlayerId)}</color></b>  {stats}", r.Go == null ? -30 : 0);
            }
        }

        private static void Label(Camera cam, Transform t, string text, float dy = 0)
        {
            float hgt = 2f;
            var sr = t.GetComponentInChildren<SpriteRenderer>();
            if (sr != null) hgt = Math.Min(4f, sr.bounds.size.y);
            var s = cam.WorldToScreenPoint(t.position + new Vector3(0, hgt * 1.05f, 0));
            if (s.z < 0) return;
            GUI.Label(new Rect(s.x - 80, Screen.height - s.y - 10 + dy, 220, 22), text);
        }

        public static bool AnyRemoteAlive => _remotes.Values.Any(r => !r.KO && Time.unscaledTime - r.LastUpdate < 10f);
    }
}
