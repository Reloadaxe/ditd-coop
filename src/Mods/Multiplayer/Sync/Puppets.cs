using System;
using System.Collections.Generic;
using System.Linq;
using DiceLoader;
using DiceMultiplayer.Net;
using UnityEngine;

namespace DiceMultiplayer.Sync
{
    /// <summary>
    /// Affiche les personnages des autres joueurs à côté du héros, en combat et en événement.
    ///
    /// Le jeu ne gère qu'un héros logique. Les autres sont donc des « marionnettes » visuelles :
    ///  - on clone le GameObject du héros sous un parent inactif, pour qu'aucun Awake/OnEnable ne s'exécute ;
    ///  - on retire tous les scripts (MonoBehaviour), les canvas (barre de vie) et les colliders : il ne reste que
    ///    les sprites et l'Animator, et le clone n'a aucun effet sur la logique du jeu ;
    ///  - on applique l'AnimatorOverrideController du personnage et du skin choisis par le joueur ;
    ///  - à chaque frame, la marionnette rejoue l'état d'animation courant du héros (attaque, coup, bloc…).
    ///    Les skins sont des override controllers d'une même machine d'états, donc les états correspondent.
    /// </summary>
    public static class Puppets
    {
        private sealed class Puppet
        {
            public int PlayerId;
            public GameObject Go;
            public Animator Anim;
            public int LastState;
            public Vector3 Offset;
            public float Height;
        }

        private static readonly List<Puppet> _puppets = new List<Puppet>();
        private static Player _source;
        private static float _nextCheck;

        public static void Update()
        {
            if (!Coop.Active || Team.RunPicks.Count < 2 || !Game.InRun) { Clear(); return; }

            var player = Game.Player;
            bool visible = player != null && Game.Safe(() => player.gameObject.activeInHierarchy)
                           && (Game.State == GameManager.GameState.BattleFloor || Game.State == GameManager.GameState.EventFloor
                               || Game.State == GameManager.GameState.ShopFloor || Game.State == GameManager.GameState.LootFloor);
            if (!visible) { Clear(); return; }

            // (Re)création si le héros a changé ou si un clone a été détruit avec la scène
            if (Time.unscaledTime >= _nextCheck)
            {
                _nextCheck = Time.unscaledTime + 0.5f;
                if (_source == null || _source.Pointer != player.Pointer || _puppets.Any(p => p.Go == null) || _puppets.Count != Team.RunPicks.Count - 1)
                {
                    Clear();
                    _source = player;
                    int k = 1;
                    foreach (var pick in Team.RunPicks.Skip(1))
                    {
                        try { var p = Create(player, pick, k++); if (p != null) _puppets.Add(p); }
                        catch (Exception e) { MultiplayerMod.Instance.Logger.LogWarning($"Marionnette de {Coop.NameOf(pick.Id)} impossible : {e.Message}"); }
                    }
                }
            }

            // Suivi de position et miroir d'animation
            var hostAnim = Game.Safe(() => player.GetComponentInChildren<Animator>());
            int state = 0; float norm = 0;
            if (hostAnim != null)
            {
                var info = hostAnim.GetCurrentAnimatorStateInfo(0);
                state = info.fullPathHash;
                norm = info.normalizedTime % 1f;
            }
            foreach (var p in _puppets)
            {
                if (p.Go == null) continue;
                p.Go.transform.position = player.transform.position + p.Offset;
                if (p.Anim != null && state != 0 && state != p.LastState)
                {
                    p.LastState = state;
                    try { p.Anim.Play(state, 0, norm); } catch { }
                }
            }
        }

        private static Puppet Create(Player player, PlayerPick pick, int index)
        {
            var src = player.gameObject;
            var holder = new GameObject("DIM_PuppetHolder");
            holder.SetActive(false);
            var clone = UnityEngine.Object.Instantiate(src, holder.transform);
            clone.name = $"DIM_Puppet_{pick.Id}";

            // 1) Retirer toute logique : scripts, canvas (barre de vie, textes), colliders (clics)
            foreach (var mb in clone.GetComponentsInChildren<MonoBehaviour>(true).ToArray().Reverse())
                try { UnityEngine.Object.DestroyImmediate(mb); } catch { }
            foreach (var c in clone.GetComponentsInChildren<Canvas>(true).ToArray())
                try { if (c != null && c.gameObject != clone) UnityEngine.Object.DestroyImmediate(c.gameObject); } catch { }
            foreach (var col in clone.GetComponentsInChildren<Collider2D>(true).ToArray())
                try { UnityEngine.Object.DestroyImmediate(col); } catch { }

            // 2) Apparence du personnage choisi
            var anim = clone.GetComponentInChildren<Animator>(true);
            try
            {
                var pd = Team.ToNewRunData(pick.Run).PlayerData;
                var skin = pd.GetSkinData((PlayerSkinData.Name)pick.Run.Skin) ?? pd.GetSkinData(pd.defaultSkinName);
                if (anim != null && skin != null && skin.animatorController != null)
                    anim.runtimeAnimatorController = skin.animatorController;
            }
            catch (Exception e) { MultiplayerMod.Instance.Logger.LogWarning($"Skin de {Coop.NameOf(pick.Id)} : {e.Message}"); }

            // 3) Placement derrière le héros, un peu en retrait
            float width = 1.5f, height = 2f;
            var sr = src.GetComponentInChildren<SpriteRenderer>();
            if (sr != null) { width = Math.Max(0.5f, sr.bounds.size.x); height = Math.Max(0.5f, sr.bounds.size.y); }
            float face = Game.Safe(() => player.FaceDirection);
            if (Math.Abs(face) < 0.01f) face = 1f;
            var offset = new Vector3(-face * width * 0.85f * index, 0.05f * index, 0.01f * index);

            foreach (var r in clone.GetComponentsInChildren<SpriteRenderer>(true))
                r.sortingOrder -= 2 * index;

            clone.transform.SetParent(src.transform.parent, true);
            clone.transform.position = src.transform.position + offset;
            clone.transform.localScale = src.transform.localScale;
            UnityEngine.Object.Destroy(holder);
            clone.SetActive(true);

            MultiplayerMod.Instance.Logger.LogInfo($"Personnage de {Coop.NameOf(pick.Id)} affiché ({(Player.CharacterName)pick.Run.Character}).");
            return new Puppet { PlayerId = pick.Id, Go = clone, Anim = anim, Offset = offset, Height = height };
        }

        public static void Clear()
        {
            foreach (var p in _puppets) try { if (p.Go != null) UnityEngine.Object.Destroy(p.Go); } catch { }
            _puppets.Clear();
            _source = null;
        }

        /// <summary>Noms au-dessus du héros et des marionnettes (appelé dans OnGUI, phase Repaint).</summary>
        public static void DrawLabels()
        {
            if (_puppets.Count == 0 || _source == null || Team.RunPicks.Count == 0) return;
            var cam = Camera.main;
            if (cam == null) return;
            float h = _puppets.FirstOrDefault()?.Height ?? 2f;
            Label(cam, _source.transform.position + new Vector3(0, h * 1.05f, 0), Team.RunPicks[0].Id);
            foreach (var p in _puppets)
                if (p.Go != null) Label(cam, p.Go.transform.position + new Vector3(0, h * 1.05f, 0), p.PlayerId);
        }

        private static void Label(Camera cam, Vector3 world, int playerId)
        {
            var s = cam.WorldToScreenPoint(world);
            if (s.z < 0) return;
            GUI.Label(new Rect(s.x - 60, Screen.height - s.y - 10, 120, 22),
                $"<b><color={Coop.ColorOf(playerId)}>{Coop.NameOf(playerId)}</color></b>");
        }
    }
}
