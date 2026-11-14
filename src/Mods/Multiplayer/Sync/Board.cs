using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using DiceLoader;
using DiceMultiplayer.Net;
using UnityEngine;
using UnityEngine.UI;

namespace DiceMultiplayer.Sync
{
    /// <summary>
    /// Plateau partagé.
    ///
    /// Chaque joueur pose SES dés sur SON plateau (logique native). À la pose, on prend un « instantané visuel » du dé :
    /// pour chaque Image de sa hiérarchie, le chemin relatif, le nom du sprite, la couleur et la visibilité ; pour chaque
    /// texte, son contenu ; plus ses types (pour la palette de couleurs SwapColor). Chez les autres, on instancie le prefab
    /// de dé du jeu (Assets.gameObjectDicePrefab) sous un parent inactif, on retire ses scripts de jeu (on garde UI, TMP
    /// et SwapColor), puis on applique l'instantané : le dé s'affiche à l'identique, sans aucune logique.
    /// Un halo (silhouette du dé, teintée à la couleur du joueur) est placé derrière. La case est bloquée pour les autres.
    /// </summary>
    public static class Board
    {
        public sealed class Ghost
        {
            public int PlayerId, Uid, X, Y, Value, Face;
            public string Name, Vis, Data;
            public GameObject Go;
            public bool Failed;
            // Vrai dé du jeu posé dans la case (copie du dé distant) : participe aux interactions entre dés
            public GameObjectDice Real;
            public BoardSlot Slot;
            public bool RealFailed, Stale;
        }

        private static readonly List<Ghost> _ghosts = new List<Ghost>();
        private static Dictionary<string, Sprite> _sprites;
        private const char F = '\u001f';
        private static int _ghostUid = -100000;
        private static float _nextFix;
        public static bool PlacingGhost { get; private set; }

        public static void Reset()
        {
            foreach (var g in _ghosts) Destroy(g);
            _ghosts.Clear();
        }

        public static bool IsTakenByOther(int x, int y) => _ghosts.Any(g => g.X == x && g.Y == y && !g.Stale);

        public static bool IsGhostSlot(BoardSlot s) => s != null && _ghosts.Any(g => g.Real != null && g.Slot != null && g.Slot.Pointer == s.Pointer);
        public static bool IsGhostDice(GameObjectDice d) => d != null && _ghosts.Any(g => g.Real != null && g.Real.Pointer == d.Pointer);

        private static bool Planning
        {
            get
            {
                var st = Game.TurnState;
                return st == BattleSystem.TurnState.SelectDice || st == BattleSystem.TurnState.UseRunestones;
            }
        }

        // ───────────── Local → réseau ─────────────

        /// <summary>Préfixe de BattleBoard.PlaceDiceOnBoard (contexte Player). Renvoie false si la case est prise par un autre joueur.</summary>
        public static bool OnLocalPlace(BoardSlot slot, GameObjectDice dice)
        {
            if (!Coop.Active || slot == null || dice == null) return true;
            var c = slot.CurrentCoord;
            var taken = _ghosts.FirstOrDefault(g => g.X == c.x && g.Y == c.y && !g.Stale);
            if (taken != null) { Coop.Toast($"Case déjà prise par {Coop.NameOf(taken.PlayerId)}"); return false; }
            var ad = Game.Safe(() => dice.CurrentActiveDice);
            if (ad == null) return true;
            // Envoyé APRÈS la pose : le dé a alors son apparence de plateau (en main, il n'a pas le même rendu)
            int key = Key(dice), x = c.x, y = c.y;
            Game.Schedule(() =>
            {
                if (dice == null) return;
                Coop.Send(new Msg
                {
                    t = T.BoardPlace, uid = key, sx = x, sy = y,
                    text = Game.Safe(() => ad.diceDataName) ?? "?", btn = Game.Safe(() => ad.CurrentFaceValue),
                    path = Game.Safe(() => ad.ToCompressedStringForRunHistory()),
                    h = Game.Safe(() => ad.faces.CurrentFaceIndex),
                    vis = Game.Safe(() => Snapshot(dice)),
                });
            }, 0.35f);
            return true;
        }

        public static void OnLocalRemove(GameObjectDice dice)
        {
            if (!Coop.Active || dice == null) return;
            Coop.Send(new Msg { t = T.BoardRemove, uid = Key(dice) });
        }

        public static void OnLocalTurnEnd()
        {
            if (!Coop.Active) return;
            Coop.Send(new Msg { t = T.BoardClear });
        }

        /// <summary>
        /// Préfixe de BattleBoard.DiscardAllDice (fin de tour, après l'exécution) : on retire les copies des dés des autres
        /// AVANT, sinon elles finiraient dans ma défausse.
        /// </summary>
        public static void BeforeDiscardAll()
        {
            if (!Coop.Active) return;
            foreach (var g in _ghosts) RemoveReal(g);
            foreach (var g in _ghosts.Where(g => g.Stale)) Destroy(g);
            _ghosts.RemoveAll(g => g.Stale);
        }

        private static int Key(GameObjectDice d) => unchecked((int)(d.Pointer.ToInt64() >> 3));

        /// <summary>Instantané visuel : lignes I (image), T (texte), S (types pour SwapColor), H (image principale pour le halo).</summary>
        private static string Snapshot(GameObjectDice d)
        {
            var root = d.transform;
            var sb = new StringBuilder();
            foreach (var img in d.GetComponentsInChildren<Image>(true))
            {
                var path = RelPath(root, img.transform);
                var c = img.color;
                sb.Append('I').Append(F).Append(path).Append(F).Append(img.sprite != null ? img.sprite.name : "").Append(F)
                  .Append(Fl(c.r)).Append(',').Append(Fl(c.g)).Append(',').Append(Fl(c.b)).Append(',').Append(Fl(c.a)).Append(F)
                  .Append(img.enabled ? '1' : '0').Append(img.gameObject.activeSelf ? '1' : '0').Append('\n');
            }
            foreach (var txt in d.GetComponentsInChildren<TMPro.TMP_Text>(true))
                sb.Append('T').Append(F).Append(RelPath(root, txt.transform)).Append(F).Append((txt.text ?? "").Replace('\n', ' ')).Append(F)
                  .Append(txt.gameObject.activeSelf ? '1' : '0').Append('\n');
            var ad = d.CurrentActiveDice;
            if (ad?.types != null)
            {
                var types = new List<string>();
                foreach (var t in ad.types) types.Add(((int)t).ToString());
                sb.Append('S').Append(F).Append(string.Join(",", types)).Append('\n');
            }
            var main = Game.Safe(() => d.diceImage);
            if (main != null) sb.Append('H').Append(F).Append(RelPath(root, main.transform)).Append('\n');
            return sb.ToString();
        }

        private static string Fl(float f) => f.ToString("0.###", CultureInfo.InvariantCulture);

        private static string RelPath(Transform root, Transform t)
        {
            var parts = new List<string>();
            while (t != null && t.Pointer != root.Pointer) { parts.Add(t.name); t = t.parent; }
            parts.Reverse();
            return string.Join("/", parts);
        }

        // ───────────── Réseau → plateau ─────────────

        public static void OnRemotePlace(Msg m)
        {
            foreach (var old in _ghosts.Where(g => g.PlayerId == m.id && g.Uid == m.uid)) Destroy(old);
            _ghosts.RemoveAll(g => g.PlayerId == m.id && g.Uid == m.uid);
            _ghosts.Add(new Ghost { PlayerId = m.id, Uid = m.uid, X = m.sx, Y = m.sy, Name = m.text, Value = m.btn, Vis = m.vis, Data = m.path, Face = (int)m.h });
        }

        public static void OnRemoteRemove(Msg m)
        {
            foreach (var g in _ghosts.Where(g => g.PlayerId == m.id && g.Uid == m.uid)) Destroy(g);
            _ghosts.RemoveAll(g => g.PlayerId == m.id && g.Uid == m.uid);
        }

        /// <summary>
        /// Fin de tour d'un autre joueur. Pendant ma propre exécution, on garde ses dés (leurs bonus comptent encore pour mes
        /// dés) : ils sont marqués périmés et retirés juste avant ma défausse.
        /// </summary>
        public static void OnRemoteClear(Msg m)
        {
            bool now = Planning;
            foreach (var g in _ghosts.Where(g => g.PlayerId == m.id))
            {
                if (now || g.Real == null) Destroy(g);
                else g.Stale = true;
            }
            _ghosts.RemoveAll(g => g.PlayerId == m.id && (now || g.Real == null) );
        }

        private static void Destroy(Ghost g)
        {
            RemoveReal(g);
            if (g.Go != null) try { UnityEngine.Object.Destroy(g.Go); } catch { }
            g.Go = null;
        }

        /// <summary>Chaque frame en combat : pose les vrais dés manquants (ou, à défaut, les visuels).</summary>
        public static void Update()
        {
            if (_ghosts.Count == 0 || !Game.InBattle) return;
            bool planning = Planning;
            if (planning && _ghosts.Any(g => g.Stale))
            {
                foreach (var g in _ghosts.Where(g => g.Stale)) Destroy(g);
                _ghosts.RemoveAll(g => g.Stale);
            }

            BoardSlot[] slots = null;
            foreach (var g in _ghosts)
            {
                if (!ReferenceEquals(g.Real, null) && g.Real == null) { g.Real = null; g.Slot = null; }   // détruit par le jeu
                if (g.Real != null || g.Go != null) continue;
                slots ??= UnityEngine.Object.FindObjectsOfType<BoardSlot>().ToArray();
                var slot = slots.FirstOrDefault(s => { var c = s.CurrentCoord; return c.x == g.X && c.y == g.Y && s.gameObject.activeInHierarchy; });
                if (slot == null) continue;

                if (!g.RealFailed && !string.IsNullOrEmpty(g.Data))
                {
                    if (!planning) continue;   // pose seulement pendant la phase de préparation
                    try { PlaceReal(g, slot); }
                    catch (Exception e) { MultiplayerMod.Instance.Logger.LogWarning("Dé distant (réel) : " + e); }
                    if (g.Real != null) continue;
                    g.RealFailed = true;
                }
                if (g.Failed) continue;
                try { g.Go = BuildVisual(g, slot); }
                catch (Exception e) { g.Failed = true; MultiplayerMod.Instance.Logger.LogWarning("Visuel de dé distant : " + e.Message); }
                if (g.Go == null) g.Failed = true;
            }

            // Le jeu peut réactiver les clics ou repeindre le dé : on réapplique régulièrement
            if (Time.unscaledTime >= _nextFix)
            {
                _nextFix = Time.unscaledTime + 0.5f;
                foreach (var g in _ghosts.Where(g => g.Real != null)) Decorate(g);
            }
        }

        // ───────────── Vrais dés distants ─────────────

        /// <summary>
        /// Pose une copie réelle du dé distant dans la case, par le chemin du jeu (BattleBoard.PlaceDiceOnSlot, contexte
        /// System, comme les dés posés par les ennemis). Les interactions entre dés (bonus, prisme, vide, miroir…) sont
        /// alors calculées nativement, dans les deux sens. Effets de bord annulés : énergie restaurée, dé retiré des listes
        /// du DiceManager, et ses propres actions exclues (GetBattleActions) : c'est son propriétaire qui les exécute.
        /// </summary>
        private static void PlaceReal(Ghost g, BoardSlot slot)
        {
            if (Game.Safe(() => slot.HasPlacedDice)) return;   // case occupée chez moi : visuel seulement
            var board = Game.Safe(() => slot.board) ?? UnityEngine.Object.FindObjectOfType<BattleBoard>();
            var dm = Game.Dice;
            var player = Game.Player;
            if (board == null || dm == null || player == null) return;

            var ad = new ActiveDice(g.Data);
            ad.uid = _ghostUid--;
            try { ad.faces.SetFace(g.Face); } catch { }

            int energy = dm.CurrentEnergy;
            var lastPlaced = dm.lastActiveDicePlacedOnBoard;
            GameObjectDice go;
            PlacingGhost = true;
            try { go = board.PlaceDiceOnSlot(ad, player, slot, BattleBoard.PlaceDiceContext.System, false, null); }
            finally { PlacingGhost = false; }
            if (go == null) return;

            g.Real = go;
            g.Slot = slot;
            try { ad.faces.SetFace(g.Face); go.RefreshVisuals(); } catch { }
            if (dm.CurrentEnergy != energy) dm.CurrentEnergy = energy;
            dm.lastActiveDicePlacedOnBoard = lastPlaced;
            Purge(dm, ad);
            Decorate(g);
            try { board.RecalculateAllBattleValues(); } catch { }
            MultiplayerMod.Instance.Logger.LogInfo($"Dé de {Coop.NameOf(g.PlayerId)} posé en ({g.X},{g.Y}) : {g.Name} {g.Value}");
        }

        private static void RemoveReal(Ghost g)
        {
            var dice = g.Real;
            var slot = g.Slot;
            g.Real = null;
            g.Slot = null;
            if (dice == null) return;
            var dm = Game.Dice;
            int energy = dm != null ? dm.CurrentEnergy : 0;
            var board = slot != null ? Game.Safe(() => slot.board) : null;
            try
            {
                var ad = Game.Safe(() => dice.CurrentActiveDice);
                if (board != null && slot != null) try { board.RemoveDiceFromSlot(slot, dice); } catch { }
                if (slot != null) try { slot.RemoveDice(); } catch { }
                var hand = UnityEngine.Object.FindObjectOfType<HandUI>();
                if (hand != null) try { hand.RemoveDice(dice); } catch { }
                if (dm != null && ad != null) Purge(dm, ad);
                dice.gameObject.SetActive(false);
                UnityEngine.Object.Destroy(dice.gameObject);
            }
            catch (Exception e) { MultiplayerMod.Instance.Logger.LogDebug("Retrait dé distant : " + e.Message); }
            if (dm != null && dm.CurrentEnergy != energy) dm.CurrentEnergy = energy;
            if (board != null)
            {
                try { board.RefreshActiveBoardSlots(); } catch { }
                try { board.RecalculateAllBattleValues(); } catch { }
            }
        }

        /// <summary>Retire le dé copié de toutes les listes du DiceManager (il ne m'appartient pas).</summary>
        private static void Purge(DiceManager dm, ActiveDice ad)
        {
            foreach (var list in new DiceManager.DiceList[] { dm.diceInHand, dm.diceOnBoard, dm.diceInDrawPile, dm.diceInDiscardPile, dm.diceInExilePile })
            {
                if (list == null) continue;
                try
                {
                    var all = list.AllDice;
                    bool found = false;
                    for (int i = 0; i < all.Count; i++) if (all[i] != null && all[i].Pointer == ad.Pointer) { found = true; break; }
                    if (found) list.Remove(ad);
                }
                catch { }
            }
        }

        /// <summary>Non cliquable, contour à la couleur du joueur.</summary>
        private static void Decorate(Ghost g)
        {
            var go = g.Real;
            if (go == null) return;
            try
            {
                foreach (var gr in go.GetComponentsInChildren<Graphic>(true)) gr.raycastTarget = false;
                foreach (var cg in go.GetComponentsInChildren<CanvasGroup>(true)) cg.blocksRaycasts = false;
                var col = ParseHex(Coop.ColorOf(g.PlayerId));
                foreach (var sc in go.GetComponentsInChildren<SwapColor>(true)) SetBorderColor(sc, col);
            }
            catch { }
        }

        private static GameObject BuildVisual(Ghost g, BoardSlot slot)
        {
            var prefab = Game.Safe(() => Game.Manager.assets.gameObjectDicePrefab);
            if (prefab == null || string.IsNullOrEmpty(g.Vis)) return null;

            var holder = new GameObject("DIM_GhostHolder");
            holder.SetActive(false);   // aucun Awake/OnEnable sur le clone
            var go = UnityEngine.Object.Instantiate(prefab, holder.transform);
            go.name = $"DIM_Ghost_{g.PlayerId}_{g.X}_{g.Y}";

            // Retirer la logique du jeu : on garde l'UI Unity, TextMeshPro et SwapColor (palette de couleurs du dé)
            foreach (var mb in go.GetComponentsInChildren<MonoBehaviour>(true).ToArray().Reverse())
            {
                var ns = Game.Safe(() => mb.GetIl2CppType().Namespace) ?? "";
                var name = Game.Safe(() => mb.GetIl2CppType().Name) ?? "";
                if (ns.StartsWith("UnityEngine") || ns.StartsWith("TMPro") || name == "SwapColor") continue;
                try { UnityEngine.Object.DestroyImmediate(mb); } catch { }
            }
            foreach (var a in go.GetComponentsInChildren<Animator>(true)) a.enabled = false;
            foreach (var gr in go.GetComponentsInChildren<Graphic>(true)) gr.raycastTarget = false;
            var cg = go.GetComponent<CanvasGroup>();
            if (cg != null) { cg.alpha = 1f; cg.blocksRaycasts = false; }

            // Appliquer l'instantané
            string haloPath = null;
            int[] types = null;
            foreach (var line in g.Vis.Split('\n'))
            {
                var f = line.Split(F);
                if (f.Length < 2) continue;
                var t = f[1].Length == 0 ? go.transform : go.transform.Find(f[1]);
                switch (f[0])
                {
                    case "I" when f.Length >= 5 && t != null:
                        var img = t.GetComponent<Image>();
                        if (img == null) break;
                        var sp = SpriteByName(f[2]);
                        if (sp != null) img.sprite = sp;
                        var c = f[3].Split(',');
                        if (c.Length == 4) img.color = new Color(P(c[0]), P(c[1]), P(c[2]), P(c[3]));
                        img.enabled = f[4].Length > 0 && f[4][0] == '1';
                        t.gameObject.SetActive(f[4].Length > 1 && f[4][1] == '1');
                        break;
                    case "T" when f.Length >= 4 && t != null:
                        var txt = t.GetComponent<TMPro.TMP_Text>();
                        if (txt != null) txt.text = f[2];
                        t.gameObject.SetActive(f[3] == "1");
                        break;
                    case "S":
                        types = f[1].Split(',').Select(s => int.TryParse(s, out var v) ? v : -1).Where(v => v >= 0).ToArray();
                        break;
                    case "H":
                        haloPath = f[1];
                        break;
                }
            }

            // Placement : à côté de la case (même parent), pas en enfant, pour ne pas décaler les index d'enfants utilisés par le jeu
            // Ignoré par un éventuel LayoutGroup du plateau : sinon la grille des cases serait décalée
            var le = go.GetComponent<LayoutElement>() ?? go.AddComponent<LayoutElement>();
            le.ignoreLayout = true;
            go.transform.SetParent(slot.transform.parent, false);
            go.transform.position = slot.transform.position;
            go.transform.localScale = slot.transform.localScale;
            go.transform.SetAsLastSibling();
            UnityEngine.Object.Destroy(holder);
            go.SetActive(true);

            // Palette de couleurs selon le type du dé, comme le jeu, puis contour (BorderColor) à la couleur du joueur
            var playerColor = ParseHex(Coop.ColorOf(g.PlayerId));
            foreach (var sc in go.GetComponentsInChildren<SwapColor>(true))
            {
                try
                {
                    if (types != null && types.Length >= 2) sc.Swap((DiceData.Type)types[0], (DiceData.Type)types[1], false);
                    else if (types != null && types.Length == 1) sc.Swap((DiceData.Type)types[0], false);
                }
                catch { }
                SetBorderColor(sc, playerColor);
            }
            return go;
        }

        /// <summary>
        /// Contour du dé à la couleur du joueur : entrée BorderColor (49) de la palette SwapColor.
        /// ExecuteSwapColor est privée dans le jeu, d'où l'appel par réflexion, suivi d'un Apply de la texture de palette.
        /// </summary>
        private static void SetBorderColor(SwapColor sc, Color color)
        {
            try
            {
                var m = typeof(SwapColor).GetMethods(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic)
                    .FirstOrDefault(x => x.Name == "ExecuteSwapColor" && x.GetParameters().Length == 2 && x.GetParameters()[0].ParameterType == typeof(SwapColor.SwapIndex));
                if (m == null) return;
                m.Invoke(sc, new object[] { SwapColor.SwapIndex.BorderColor, color });
                var tex = Game.Safe(() => sc.colorSwapTexture);
                if (tex != null) tex.Apply();
            }
            catch (Exception e) { MultiplayerMod.Instance.Logger.LogDebug("Contour : " + e.Message); }
        }

        /// <summary>Halo léger : copie de l'image principale, un peu plus grande, teintée à la couleur du joueur, derrière le dé.</summary>
        private static void AddHalo(GameObject go, string mainPath, int playerId)
        {
            var main = !string.IsNullOrEmpty(mainPath) ? go.transform.Find(mainPath)?.GetComponent<Image>() : null;
            if (main == null) main = go.GetComponentsInChildren<Image>(true).Where(i => i.enabled && i.sprite != null)
                                       .OrderByDescending(i => i.rectTransform.rect.width * i.rectTransform.rect.height).FirstOrDefault();
            if (main == null) return;

            var halo = new GameObject("DIM_Halo");
            var rt = halo.AddComponent<RectTransform>();
            rt.SetParent(main.transform.parent, false);
            rt.SetSiblingIndex(main.transform.GetSiblingIndex());   // juste derrière l'image principale
            var mrt = main.rectTransform;
            rt.anchorMin = mrt.anchorMin; rt.anchorMax = mrt.anchorMax; rt.pivot = mrt.pivot;
            rt.anchoredPosition = mrt.anchoredPosition; rt.sizeDelta = mrt.sizeDelta;
            rt.localScale = mrt.localScale * 1.16f;
            var img = halo.AddComponent<Image>();
            img.sprite = main.sprite;
            img.preserveAspect = main.preserveAspect;
            img.raycastTarget = false;
            var col = ParseHex(Coop.ColorOf(playerId));
            img.color = new Color(col.r, col.g, col.b, 0.55f);
        }

        // ───────────── Utilitaires ─────────────

        private static Sprite SpriteByName(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;
            if (_sprites == null || !_sprites.ContainsKey(name))
            {
                _sprites = new Dictionary<string, Sprite>();
                foreach (var s in Resources.FindObjectsOfTypeAll<Sprite>())
                    if (s != null && !_sprites.ContainsKey(s.name)) _sprites[s.name] = s;
            }
            return _sprites.TryGetValue(name, out var sp) ? sp : null;
        }

        private static float P(string s) => float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var v) ? v : 1f;

        public static Color ParseHex(string hex)
        {
            if (!string.IsNullOrEmpty(hex) && hex[0] == '#' && hex.Length >= 7
                && int.TryParse(hex.Substring(1, 6), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var v))
                return new Color(((v >> 16) & 255) / 255f, ((v >> 8) & 255) / 255f, (v & 255) / 255f, 1f);
            return Color.white;
        }

        /// <summary>Repli si le visuel n'a pas pu être construit : encadré coloré dessiné en IMGUI.</summary>
        public static void DrawGhosts()
        {
            if (_ghosts.Count == 0 || !Game.InBattle) return;
            var slots = UnityEngine.Object.FindObjectsOfType<BoardSlot>();
            foreach (var g in _ghosts.Where(x => x.Go == null))
            {
                BoardSlot slot = null;
                foreach (var s in slots) { var c = s.CurrentCoord; if (c.x == g.X && c.y == g.Y && s.gameObject.activeInHierarchy) { slot = s; break; } }
                if (slot == null) continue;
                var sp = ScreenPos(slot.transform);
                if (sp == null) continue;
                var r = new Rect(sp.Value.x - 34, Screen.height - sp.Value.y - 34, 68, 68);
                GUI.Box(r, "");
                GUI.Label(new Rect(r.x + 4, r.y + 4, 64, 22), $"<b><color={Coop.ColorOf(g.PlayerId)}>{Coop.NameOf(g.PlayerId)}</color></b>");
                GUI.Label(new Rect(r.x + 4, r.y + 22, 64, 26), $"<size=18><b>{g.Value}</b></size>");
            }
        }

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
