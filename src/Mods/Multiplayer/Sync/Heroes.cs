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
            public int LastState, WantedState;
            public bool GreyShown;
            public RectTransform HealthAnchor;
            public float WantedTime;
            public Vector3 Offset;
            public int Hp, MaxHp, Block;
            public bool KO;
            public float LastUpdate;
            public TMPro.TMP_Text HpText, BlockText;
            /// <summary>Tous les textes pilotés par le CustomText (chiffre, ombre, contour…) : on les met tous à jour.</summary>
            public List<TMPro.TMP_Text> HpTexts = new List<TMPro.TMP_Text>(), BlockTexts = new List<TMPro.TMP_Text>();
            public GameObject BlockParent;
            public bool HpWithMax;
            public string ShownHp, ShownBlock;
        }

        private static readonly Dictionary<int, Remote> _remotes = new Dictionary<int, Remote>();
        private static Player _local;
        private static float _nextSend, _nextCheck;
        private static string _lastSent;
        private static int _diagCount;

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
            bool ko = Knockout.LocalKO || Game.Safe(() => p.IsDyingOrDead);
            string key = $"{hp}/{max}/{blk}/{ko}";
            if (key == _lastSent && Time.unscaledTime % 2f > 0.3f) return;   // pas de doublon, sauf un rappel régulier
            _lastSent = key;
            Coop.Send(new Msg { t = T.HeroState, h = hp, sx = max, sy = blk, flag = ko });
        }

        private static int _lastLocalAnim;
        private static bool _localGrey;

        /// <summary>Animation du héros d'un autre joueur : sa copie chez moi la jouera (même contrôleur d'animation).</summary>
        public static void OnRemoteAnim(Msg m)
        {
            if (!_remotes.TryGetValue(m.id, out var r)) _remotes[m.id] = r = new Remote { PlayerId = m.id };
            r.WantedState = (int)m.h;
            r.WantedTime = m.x;
            r.LastState = 0;   // rejouer même si c'est le même état (attaque répétée)
        }

        public static void OnRemoteState(Msg m)
        {
            if (!_remotes.TryGetValue(m.id, out var r)) _remotes[m.id] = r = new Remote { PlayerId = m.id };
            if (r.LastUpdate == 0 || (r.Hp != (int)m.h && _diagCount < 6))
            {
                _diagCount++;
                Coop.AddLog($"[diag] État reçu de {Coop.NameOf(m.id)} : {m.h}/{m.sx} PV, bloc {m.sy}{(m.flag ? ", K.O." : "")}");
            }
            r.Hp = (int)m.h; r.MaxHp = m.sx; r.Block = m.sy; r.KO = m.flag; r.LastUpdate = Time.unscaledTime;
        }

        // ───────────── Visuels ─────────────

        public static void Update()
        {
            if (!Coop.Active || !Game.InRun) { DespawnAll(); return; }
            SendLocalState();

            var local = Game.Player;
            if (!InScene(local)) { DespawnAll(); return; }
            if (_local == null || _local.Pointer != local.Pointer) { DespawnAll(); _local = local; _localGrey = false; }
            if (_localGrey != Knockout.LocalKO) { Knockout.SetGrey(local.gameObject, Knockout.LocalKO); _localGrey = Knockout.LocalKO; }

            // Animation de MON héros → réseau : chaque copie distante joue l'animation de son propre joueur
            // (avant, elle rejouait la mienne : quand j'attaquais seul, la copie de l'autre attaquait aussi chez moi)
            var anim = Game.Safe(() => local.GetComponentInChildren<Animator>());
            if (anim != null)
            {
                var info = anim.GetCurrentAnimatorStateInfo(0);
                if (info.fullPathHash != 0 && info.fullPathHash != _lastLocalAnim)
                {
                    _lastLocalAnim = info.fullPathHash;
                    Coop.Send(new Msg { t = T.HeroAnim, h = info.fullPathHash, x = info.normalizedTime % 1f });
                }
            }
            foreach (var r in _remotes.Values)
            {
                if (r.Go == null) continue;
                r.Go.transform.position = local.transform.position + r.Offset;
                // K.O. : la copie reste visible, grisée
                if (!r.Go.activeSelf) r.Go.SetActive(true);
                if (r.GreyShown != r.KO) { Knockout.SetGrey(r.Go, r.KO); r.GreyShown = r.KO; }
                if (r.Anim != null && r.WantedState != 0 && r.WantedState != r.LastState)
                {
                    r.LastState = r.WantedState;
                    try { r.Anim.Play(r.WantedState, 0, r.WantedTime); } catch { }
                }
                // Interface de vie du jeu, remplie avec l'état reçu du réseau
                try
                {
                    if (r.HpText != null)
                    {
                        var s = r.HpWithMax ? $"{r.Hp}/{r.MaxHp}" : r.Hp.ToString();
                        if (s != r.ShownHp || (r.HpTexts.Count > 0 && r.HpTexts[0].text != s)) { SetAll(r.HpTexts, s); r.ShownHp = s; }
                    }
                    if (r.BlockParent != null && r.BlockParent.activeSelf != (r.Block > 0)) r.BlockParent.SetActive(r.Block > 0);
                    if (r.BlockText != null)
                    {
                        var b = r.Block.ToString();
                        if (b != r.ShownBlock || (r.BlockTexts.Count > 0 && r.BlockTexts[0].text != b)) { SetAll(r.BlockTexts, b); r.ShownBlock = b; }
                    }
                }
                catch { }
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
                // Interface de vie du prefab (EntityCanvas) : on la garde pour afficher les PV avec la même UI que le héros local.
                // On note ses textes avant de retirer les scripts, et on masque les marqueurs (intentions, modificateurs).
                var ec = go.GetComponentInChildren<EntityCanvas>(true);
                if (ec != null)
                {
                    r.HpText = Tmp(Game.Safe(() => ec.healthText));
                    r.HealthAnchor = Game.Safe(() => ec.currentHealthDice);
                    r.HpTexts = AllTmp(Game.Safe(() => ec.healthText));
                    r.BlockText = Tmp(Game.Safe(() => ec.blockText));
                    r.BlockTexts = AllTmp(Game.Safe(() => ec.blockText));
                    r.BlockParent = Game.Safe(() => ec.parentBlock)?.gameObject;
                    foreach (var mk in new[] { Game.Safe(() => ec.attackMarker), Game.Safe(() => ec.blockMarker), Game.Safe(() => ec.healMarker),
                                               Game.Safe(() => ec.boostMarker), Game.Safe(() => ec.currentBoostMarker) })
                        if (mk != null) try { mk.gameObject.SetActive(false); } catch { }
                    var mods = Game.Safe(() => ec.modifiersParentRT);
                    if (mods != null) for (int i = 0; i < mods.childCount; i++) mods.GetChild(i).gameObject.SetActive(false);
                    var parry = Game.Safe(() => ec.parentParryRT);
                    if (parry != null) parry.gameObject.SetActive(false);
                }
                // Retirer la logique : on garde l'UI Unity et TextMeshPro, tout le reste (scripts du jeu) part
                foreach (var mb in go.GetComponentsInChildren<MonoBehaviour>(true).ToArray().Reverse())
                {
                    var ns = Game.Safe(() => mb.GetIl2CppType().Namespace) ?? "";
                    if (ns.StartsWith("UnityEngine") || ns.StartsWith("TMPro")) continue;
                    try { UnityEngine.Object.DestroyImmediate(mb); } catch { }
                }
                foreach (var col in go.GetComponentsInChildren<Collider2D>(true).ToArray()) try { UnityEngine.Object.DestroyImmediate(col); } catch { }
                foreach (var gr in go.GetComponentsInChildren<UnityEngine.UI.Graphic>(true)) gr.raycastTarget = false;
                foreach (var cv in go.GetComponentsInChildren<Canvas>(true))
                    if (cv.renderMode != RenderMode.ScreenSpaceOverlay && cv.worldCamera == null) cv.worldCamera = Camera.main;
                // Format du texte de vie local (« 20 » ou « 20/20 ») pour l'imiter
                var localEc = Game.Safe(() => local.GetComponentInChildren<EntityCanvas>(true));
                var localHp = Tmp(Game.Safe(() => localEc.healthText));
                r.HpWithMax = localHp != null && (localHp.text ?? "").Contains("/");

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
                Coop.AddLog($"Héros de {Coop.NameOf(pick.Id)} affiché ({(Player.CharacterName)pick.Run.Character}). " +
                            $"[diag] UI de vie : {r.HpTexts.Count} texte(s) PV ({string.Join(", ", r.HpTexts.Select(t => $"{t.name}=«{Game.Safe(() => t.text)}»"))}), " +
                            $"bloc {(r.BlockText != null ? "trouvé" : "introuvable")}, format local {(r.HpWithMax ? "pv/max" : "pv")}");
            }
            catch (Exception e) { Coop.AddLog($"Affichage du héros de {Coop.NameOf(pick.Id)} impossible : {e.Message}"); }
        }

        /// <summary>Tous les textes TextMeshPro sous un CustomText (le jeu peut en piloter plusieurs : chiffre, ombre, contour).</summary>
        private static List<TMPro.TMP_Text> AllTmp(CustomText ct)
        {
            var list = new List<TMPro.TMP_Text>();
            if (ct == null) return list;
            var arr = Game.Safe(() => ct.GetComponentsInChildren<TMPro.TMP_Text>(true));
            if (arr != null) foreach (var t in arr) if (t != null) list.Add(t);
            return list;
        }

        private static void SetAll(List<TMPro.TMP_Text> texts, string value)
        {
            foreach (var t in texts)
            {
                try
                {
                    t.text = value;
                    t.SetAllDirty();
                    t.ForceMeshUpdate(true, true);
                }
                catch { }
            }
        }

        /// <summary>Texte TextMeshPro piloté par un CustomText du jeu (sur le même objet ou un enfant).</summary>
        private static TMPro.TMP_Text Tmp(CustomText ct)
        {
            if (ct == null) return null;
            return Game.Safe(() => ct.GetComponent<TMPro.TMP_Text>()) ?? Game.Safe(() => ct.GetComponentInChildren<TMPro.TMP_Text>(true));
        }

        public static void DespawnAll()
        {
            foreach (var r in _remotes.Values) { if (r.Go != null) try { UnityEngine.Object.Destroy(r.Go); } catch { } r.Go = null; }
            _local = null;
        }

        // ───────────── Données d'affichage (voir Ui/Hud.cs) ─────────────

        /// <summary>Noms au-dessus des héros : joueur, position écran, texte d'état éventuel (K.O., PV si pas d'interface).</summary>
        public static List<(int id, Vector2 screen, string extra)> HeroLabels()
        {
            var res = new List<(int, Vector2, string)>();
            var cam = Camera.main;
            if (cam == null || !Coop.Active || _local == null) return res;
            var localHealth = Game.Safe(() => _local.MyEntityCanvas != null ? _local.MyEntityCanvas.currentHealthDice : null);
            var p = TopOf(localHealth) ?? Above(cam, _local.transform);
            if (p != null) res.Add((Coop.LocalId, p.Value, Knockout.LocalKO ? "<color=#ff7a6b>K.O.</color>" : null));
            foreach (var r in _remotes.Values)
            {
                if (r.Go == null && !r.KO) continue;
                var t = r.Go != null ? r.Go.transform : _local.transform;
                string extra = r.KO ? "<color=#ff7a6b>K.O.</color>"
                             : r.HpText != null ? null : $"PV {r.Hp}/{r.MaxHp}" + (r.Block > 0 ? $"  Bloc {r.Block}" : "");
                var s = (r.Go != null && r.Go.activeInHierarchy ? TopOf(r.HealthAnchor) : null) ?? Above(cam, t);
                if (s != null) res.Add((r.PlayerId, s.Value + (r.Go == null ? new Vector2(0, 40) : Vector2.zero), extra));
            }
            return res;
        }

        /// <summary>Haut (centre) d'un élément d'interface du héros, en coordonnées écran : le nom se pose juste au-dessus de la vie.</summary>
        private static Vector2? TopOf(RectTransform rt)
        {
            if (rt == null || !rt.gameObject.activeInHierarchy) return null;
            try
            {
                var corners = new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStructArray<Vector3>(4);
                rt.GetWorldCorners(corners);
                var top = (corners[1] + corners[2]) * 0.5f;
                var canvas = rt.GetComponentInParent<Canvas>();
                if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay) return new Vector2(top.x, top.y);
                var cam = canvas != null && canvas.worldCamera != null ? canvas.worldCamera : Camera.main;
                if (cam == null) return null;
                var sp = cam.WorldToScreenPoint(top);
                return sp.z < 0 ? (Vector2?)null : new Vector2(sp.x, sp.y);
            }
            catch { return null; }
        }

        private static Vector2? Above(Camera cam, Transform t)
        {
            float hgt = 2f;
            var sr = t.GetComponentInChildren<SpriteRenderer>();
            if (sr != null) hgt = Math.Min(4f, sr.bounds.size.y);
            var s = cam.WorldToScreenPoint(t.position + new Vector3(0, hgt * 1.05f, 0));
            return s.z < 0 ? (Vector2?)null : new Vector2(s.x, s.y);
        }

        public static bool AnyRemoteAlive => _remotes.Values.Any(r => !r.KO && Time.unscaledTime - r.LastUpdate < 10f);
    }
}
