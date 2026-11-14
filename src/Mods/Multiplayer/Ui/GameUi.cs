using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace DiceMultiplayer.Ui
{
    /// <summary>
    /// Petit kit d'interface aux couleurs du jeu : on réutilise ses sprites (parchemins, boutons, bulles), ses polices
    /// pixel « Froggy » (texte clair + contour sombre + ombre, comme les CustomText du jeu) et son échelle
    /// (480×270 « pixels jeu », 1 pixel de sprite = 1 unité).
    /// </summary>
    public static class GameUi
    {
        public const float RefW = 480f, RefH = 270f;
        public static float FieldFontSize = 12f;

        // Couleurs relevées dans l'interface du jeu
        public static readonly Color Ink = new Color(0.192f, 0.133f, 0.220f, 1f);      // contour des textes
        public static readonly Color Paper = new Color(0.965f, 0.906f, 0.906f, 1f);    // texte clair
        public static readonly Color Gold = new Color(0.988f, 0.835f, 0.400f, 1f);     // titres
        public static readonly Color Muted = new Color(0.914f, 0.812f, 0.682f, 1f);    // texte secondaire (clair : lisible avec le contour sombre)
        public static readonly Color Green = new Color(0.763f, 1.000f, 0.590f, 1f);
        public static readonly Color Red = new Color(1.000f, 0.561f, 0.502f, 1f);

        // Sprites du jeu
        public const string SprWindow = "ui event body";                        // grande fenêtre (événements)
        public const string SprBubble = "text template body with padding_0";   // bulle claire (infobulles)
        public const string SprBubbleDark = "text template body dark";
        public const string SprButton = "ui character select menu start button idle";
        public const string SprButtonHover = "ui character select menu start button hover";
        public const string SprButtonPress = "ui character select menu start button press";
        public const string SprField = "ui event option";
        public const string SprClose = "close_button_on";
        public const string SprCursor = "cursor_open";

        public static RectTransform Root { get; private set; }
        private static Dictionary<string, Sprite> _sprites = new Dictionary<string, Sprite>();
        private static TMP_FontAsset _chars, _outline;
        private static Material _charsMat, _outlineMat;
        private static readonly Regex ColorTags = new Regex("<color=[^>]*>|</color>|<#[0-9a-fA-F]{3,8}>", RegexOptions.Compiled);

        /// <summary>Crée le canvas du mod si les ressources du jeu sont chargées. Faux tant que ce n'est pas possible.</summary>
        public static bool Ensure()
        {
            if (Root != null) return true;
            _chars ??= FindFont("Froggy Font Characters v4");
            _outline ??= FindFont("Froggy Font Outline v4");
            // Matériaux propres au jeu (le matériau par défaut de ces polices pixel donne des pâtés illisibles)
            _charsMat ??= FindMat("Froggy Font Characters v4 Material");
            _outlineMat ??= FindMat("Froggy Font Outline v4 Material");
            if (_chars == null || Sprite(SprWindow) == null) return false;

            var go = new GameObject("DIM_Hud");
            UnityEngine.Object.DontDestroyOnLoad(go);
            _canvas = go.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 600;
            _canvas.pixelPerfect = true;
            var cs = go.AddComponent<CanvasScaler>();
            cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            cs.referenceResolution = new Vector2(RefW, RefH);
            cs.matchWidthOrHeight = 0f;
            cs.referencePixelsPerUnit = 27f;
            go.AddComponent<GraphicRaycaster>();
            Root = go.GetComponent<RectTransform>();
            AttachToCursorLayer();
            return true;
        }

        private static Canvas _canvas;

        /// <summary>Caméra du canvas du mod (null en superposition).</summary>
        public static Camera Cam => _canvas != null && _canvas.renderMode == RenderMode.ScreenSpaceCamera ? _canvas.worldCamera : null;

        /// <summary>
        /// Le pointeur du jeu est une image dans son propre canvas (« Cursor Visual [Canvas 900] », caméra). Un canvas en
        /// superposition passerait par-dessus et le cacherait : on se met dans le même calque, juste en dessous de lui.
        /// Appelé chaque frame (la caméra peut changer d'une scène à l'autre).
        /// </summary>
        public static void AttachToCursorLayer()
        {
            if (_canvas == null) return;
            if (_canvas.renderMode == RenderMode.ScreenSpaceCamera && _canvas.worldCamera != null) return;
            Canvas cursor = null;
            foreach (var c in UnityEngine.Object.FindObjectsOfType<Canvas>())
                if (c != null && c.name.StartsWith("Cursor Visual") && c.renderMode == RenderMode.ScreenSpaceCamera && c.worldCamera != null) { cursor = c; break; }
            if (cursor == null)
            {
                if (_canvas.renderMode != RenderMode.ScreenSpaceOverlay) { _canvas.renderMode = RenderMode.ScreenSpaceOverlay; _canvas.sortingOrder = 600; }
                return;
            }
            _canvas.renderMode = RenderMode.ScreenSpaceCamera;
            _canvas.worldCamera = cursor.worldCamera;
            _canvas.planeDistance = Math.Max(0.5f, cursor.planeDistance * 0.9f);
            _canvas.sortingLayerID = cursor.sortingLayerID;
            _canvas.sortingOrder = cursor.sortingOrder - 10;
        }

        /// <summary>Vrai si ce point de l'écran est sur l'un de ces éléments actifs du mod.</summary>
        public static bool Contains(RectTransform rt, Vector2 screen) =>
            rt != null && rt.gameObject.activeInHierarchy && RectTransformUtility.RectangleContainsScreenPoint(rt, screen, Cam);

        public static bool Owns(Component c)
        {
            if (Root == null || c == null) return false;
            var t = c.transform;
            while (t != null) { if (t.Pointer == Root.Pointer) return true; t = t.parent; }
            return false;
        }

        private static Material FindMat(string name) =>
            Resources.FindObjectsOfTypeAll<Material>().FirstOrDefault(m => m != null && m.name == name);

        private static TMP_FontAsset FindFont(string name) =>
            Resources.FindObjectsOfTypeAll<TMP_FontAsset>().FirstOrDefault(f => f != null && f.name == name);

        public static Sprite Sprite(string name)
        {
            if (_sprites.TryGetValue(name, out var s) && s != null) return s;
            s = Resources.FindObjectsOfTypeAll<Sprite>().FirstOrDefault(x => x != null && x.name == name);
            if (s != null) _sprites[name] = s;
            return s;
        }

        // ───────────── Construction ─────────────

        public static RectTransform Node(string name, Transform parent)
        {
            var go = new GameObject(name);
            go.layer = 5;   // UI
            var rt = go.AddComponent<RectTransform>();
            if (parent != null) rt.SetParent(parent, false);
            return rt;
        }

        public static void Place(RectTransform rt, Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size)
        {
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = pivot;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
        }

        public static void Stretch(RectTransform rt, float left = 0, float bottom = 0, float right = 0, float top = 0)
        {
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(left, bottom);
            rt.offsetMax = new Vector2(-right, -top);
        }

        /// <summary>Image d'un sprite du jeu, en 9-slice si le sprite a des bordures, 1 pixel de sprite = 1 unité.</summary>
        public static Image Image(Transform parent, string sprite, Color? tint = null, string name = null)
        {
            var rt = Node(name ?? sprite ?? "Image", parent);
            var img = rt.gameObject.AddComponent<Image>();
            var s = sprite != null ? Sprite(sprite) : null;
            if (s != null)
            {
                img.sprite = s;
                img.type = s.border.sqrMagnitude > 0 ? UnityEngine.UI.Image.Type.Sliced : UnityEngine.UI.Image.Type.Simple;
                img.pixelsPerUnitMultiplier = 27f / Math.Max(1f, s.pixelsPerUnit);
            }
            img.color = tint ?? Color.white;
            img.raycastTarget = false;
            return img;
        }

        /// <summary>Boîte verticale qui s'ajuste à son contenu (fond : sprite du jeu).</summary>
        public static RectTransform Box(Transform parent, string sprite, int padX, int padY, float spacing, bool fit = true, TextAnchor align = TextAnchor.UpperLeft)
        {
            var img = Image(parent, sprite, null, "Box");
            var rt = img.rectTransform;
            var v = rt.gameObject.AddComponent<VerticalLayoutGroup>();
            v.padding = new RectOffset(padX, padX, padY, padY);
            v.spacing = spacing;
            v.childAlignment = align;
            v.childControlWidth = true; v.childControlHeight = true;
            v.childForceExpandWidth = false; v.childForceExpandHeight = false;
            if (fit)
            {
                var f = rt.gameObject.AddComponent<ContentSizeFitter>();
                f.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                f.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            }
            return rt;
        }

        public static RectTransform Row(Transform parent, float spacing = 4, TextAnchor align = TextAnchor.MiddleLeft)
        {
            var rt = Node("Row", parent);
            var h = rt.gameObject.AddComponent<HorizontalLayoutGroup>();
            h.spacing = spacing;
            h.childAlignment = align;
            h.childControlWidth = true; h.childControlHeight = true;
            h.childForceExpandWidth = false; h.childForceExpandHeight = false;
            return rt;
        }

        public static LayoutElement Size(Component c, float w = -1, float h = -1, float flexW = -1)
        {
            var le = c.gameObject.GetComponent<LayoutElement>() ?? c.gameObject.AddComponent<LayoutElement>();
            if (w >= 0) { le.preferredWidth = w; le.minWidth = w; }
            if (h >= 0) { le.preferredHeight = h; le.minHeight = h; }
            if (flexW >= 0) le.flexibleWidth = flexW;
            return le;
        }

        /// <summary>Carré de couleur à contour sombre (pastille d'un joueur), en pixels jeu.</summary>
        public static RectTransform Chip(Transform parent, Color color, float size = 5)
        {
            var border = Image(parent, null, Ink, "Chip");
            Size(border, size + 2, size + 2);
            var inner = Image(border.transform, null, color, "Fill");
            Stretch(inner.rectTransform, 1, 1, 1, 1);
            return border.rectTransform;
        }

        // ───────────── Texte ─────────────

        /// <summary>
        /// Texte façon jeu : ombre (police contour, encre à 50 %, 1 px plus bas), contour (police contour, encre) et
        /// caractères (police caractères, couleur). La couche racine porte la mise en page.
        /// </summary>
        public sealed class Text
        {
            public RectTransform Rect;
            public TextMeshProUGUI Shadow, Outline, Chars;
            public CustomText Custom;   // copie d'un vrai texte du jeu (préféré)
            public string Tint;         // couleur du texte (balise ajoutée autour de la valeur)
            private string _value;

            public string Value
            {
                get => _value;
                set
                {
                    value ??= "";
                    if (value == _value) return;
                    _value = value;
                    if (Custom != null)
                    {
                        try
                        {
                            if (value.Length == 0) Custom.ClearText();
                            else Custom.SetTextNoParse(Tint != null ? $"<color={Tint}>{value}</color>" : value);
                            return;
                        }
                        catch (Exception e) { MultiplayerMod.Instance.Logger.LogDebug("CustomText : " + e.Message); }
                    }
                    var plain = ColorTags.Replace(value, "");
                    if (Shadow != null) Shadow.text = plain;
                    if (Outline != null) Outline.text = plain;
                    if (Chars != null) Chars.text = value;
                }
            }

            public Color Color { set { if (Chars != null) Chars.color = value; } }
            public bool Active { set { if (Rect.gameObject.activeSelf != value) Rect.gameObject.SetActive(value); } }
        }

        private static GameObject _templates;
        private static CustomText _tplText;

        /// <summary>
        /// Modèle de texte : copie d'un CustomText du jeu en police « Froggy Font Characters v4 » (de préférence le texte
        /// des infobulles), gardée inactive. Ses couches (réglages, contour, caractères, ombre) sont gérées par le jeu.
        /// </summary>
        private static CustomText TextTemplate()
        {
            if (_tplText != null) return _tplText;
            try
            {
                var src = Resources.FindObjectsOfTypeAll<CustomText>()
                    .Where(c => c != null && c.charactersText != null && c.charactersText.font != null
                                && c.charactersText.font.name == "Froggy Font Characters v4")
                    .OrderBy(c => c.name == "Tooltip Text" ? 0 : c.name.Contains("Text") ? 1 : 2)
                    .FirstOrDefault();
                if (src == null) return null;
                if (_templates == null)
                {
                    _templates = new GameObject("DIM_Templates");
                    _templates.SetActive(false);
                    UnityEngine.Object.DontDestroyOnLoad(_templates);
                }
                var go = UnityEngine.Object.Instantiate(src.gameObject, _templates.transform);
                go.name = "DIM_TextTemplate";
                foreach (var f in go.GetComponents<ContentSizeFitter>()) UnityEngine.Object.DestroyImmediate(f);
                foreach (var l in go.GetComponents<LayoutElement>()) UnityEngine.Object.DestroyImmediate(l);
                _tplText = go.GetComponent<CustomText>();
                MultiplayerMod.Instance.Logger.LogInfo($"Modèle de texte : {src.name}");
            }
            catch (Exception e) { MultiplayerMod.Instance.Logger.LogWarning("Modèle de texte : " + e.Message); }
            return _tplText;
        }

        public static Text Label(Transform parent, string value, float size = 7, Color? color = null,
                                 TextAlignmentOptions align = TextAlignmentOptions.TopLeft, bool wrap = true)
        {
            var tpl = TextTemplate();
            if (tpl != null)
            {
                try
                {
                    var go = UnityEngine.Object.Instantiate(tpl.gameObject, parent);
                    go.name = "Text";
                    var rt = go.GetComponent<RectTransform>();
                    rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
                    rt.pivot = new Vector2(0.5f, 0.5f);
                    rt.localScale = Vector3.one;
                    rt.anchoredPosition = Vector2.zero;
                    go.SetActive(true);
                    var ct = go.GetComponent<CustomText>();
                    foreach (var tmp in go.GetComponentsInChildren<TMP_Text>(true))
                    {
                        tmp.alignment = align;
                        tmp.enableWordWrapping = wrap;
                        tmp.raycastTarget = false;
                        tmp.fontSize = size;
                    }
                    try { ct.ChangeFontSize(size); } catch { }
                    var t = new Text { Rect = rt, Custom = ct, Chars = ct.charactersText?.TryCast<TextMeshProUGUI>() };
                    if (color != null && color.Value != Paper) t.Tint = "#" + ColorUtility.ToHtmlStringRGB(color.Value);
                    t.Value = value;
                    return t;
                }
                catch (Exception e) { MultiplayerMod.Instance.Logger.LogWarning("Texte du jeu : " + e.Message); }
            }
            return LayeredLabel(parent, value, size, color, align, wrap);
        }

        private static Text LayeredLabel(Transform parent, string value, float size, Color? color, TextAlignmentOptions align, bool wrap)
        {
            var root = Node("Text", parent);
            var t = new Text { Rect = root };
            t.Shadow = MakeTmp(root.gameObject, _outline ?? _chars, size, new Color(Ink.r, Ink.g, Ink.b, 0.5f), align, wrap);
            if (_outline != null)
            {
                var o = Node("Outline", root); Stretch(o, 0, 1, 0, -1);
                t.Outline = MakeTmp(o.gameObject, _outline, size, Ink, align, wrap);
            }
            var c = Node("Characters", root); Stretch(c, 0, 1, 0, -1);
            t.Chars = MakeTmp(c.gameObject, _chars, size, color ?? Paper, align, wrap);
            t.Value = value;
            return t;
        }

        private static TextMeshProUGUI MakeTmp(GameObject go, TMP_FontAsset font, float size, Color color, TextAlignmentOptions align, bool wrap)
        {
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.font = font;
            var mat = font == _chars ? _charsMat : font == _outline ? _outlineMat : null;
            if (mat != null) tmp.fontSharedMaterial = mat;
            tmp.fontSize = size;
            tmp.color = color;
            tmp.alignment = align;
            tmp.enableWordWrapping = wrap;
            tmp.richText = true;
            tmp.raycastTarget = false;
            tmp.overflowMode = TextOverflowModes.Overflow;
            tmp.margin = Vector4.zero;
            return tmp;
        }

        // ───────────── Bouton ─────────────

        public sealed class Btn
        {
            public Button Button;
            public Text Label;
            public RectTransform Rect => (RectTransform)Button.transform;
            public bool Active { set { if (Button.gameObject.activeSelf != value) Button.gameObject.SetActive(value); } }
        }

        /// <summary>Bouton du jeu (celui de « Lancer » à la sélection de personnage) : survol et appui natifs.</summary>
        public static Btn Button(Transform parent, string text, Action onClick, float w = 70, float h = 18, float textSize = 7)
        {
            var img = Image(parent, SprButton, null, "Button");
            img.raycastTarget = true;
            Size(img, w, h);
            img.rectTransform.sizeDelta = new Vector2(w, h);
            var b = img.gameObject.AddComponent<Button>();
            b.targetGraphic = img;
            var hover = Sprite(SprButtonHover);
            var press = Sprite(SprButtonPress);
            if (hover != null && press != null)
            {
                b.transition = UnityEngine.UI.Selectable.Transition.SpriteSwap;
                var st = new SpriteState { highlightedSprite = hover, pressedSprite = press, selectedSprite = Sprite(SprButton) };
                b.spriteState = st;
            }
            var nav = new Navigation { mode = Navigation.Mode.None };
            b.navigation = nav;
            b.onClick.AddListener((UnityAction)(() => { try { onClick?.Invoke(); } catch (Exception e) { MultiplayerMod.Instance.Logger.LogWarning("Bouton : " + e.Message); } }));
            var label = Label(img.transform, text, textSize, Paper, TextAlignmentOptions.Center, false);
            Stretch(label.Rect, 2, 2, 2, 1);
            return new Btn { Button = b, Label = label };
        }

        /// <summary>Petit bouton image (croix de fermeture du jeu).</summary>
        public static Button IconButton(Transform parent, string sprite, Action onClick, float size)
        {
            var img = Image(parent, sprite, null, "IconButton");
            img.raycastTarget = true;
            img.preserveAspect = true;
            img.rectTransform.sizeDelta = new Vector2(size, size);
            var b = img.gameObject.AddComponent<Button>();
            b.targetGraphic = img;
            b.navigation = new Navigation { mode = Navigation.Mode.None };
            b.onClick.AddListener((UnityAction)(() => { try { onClick?.Invoke(); } catch { } }));
            return b;
        }

        // ───────────── Champ de saisie ─────────────

        public static TMP_InputField Field(Transform parent, string value, float w = 90, float h = 15, bool numeric = false)
        {
            var bg = Image(parent, SprField, null, "Field");
            bg.raycastTarget = true;
            Size(bg, w, h, 1);
            bg.gameObject.SetActive(false);   // pas d'OnEnable avant que tout soit relié

            var area = Node("Text Area", bg.transform);
            Stretch(area, 5, 2, 5, 2);
            area.gameObject.AddComponent<RectMask2D>();
            var txt = Node("Text", area);
            Stretch(txt);
            var ff = FindFont("m3x6 SDF") ?? _chars;
            var tmp = MakeTmp(txt.gameObject, ff, FieldFontSize, Ink, TextAlignmentOptions.MidlineLeft, false);

            var field = bg.gameObject.AddComponent<TMP_InputField>();
            field.textViewport = area;
            field.textComponent = tmp;
            field.fontAsset = ff;
            field.pointSize = FieldFontSize;
            field.caretColor = Ink;
            field.customCaretColor = true;
            field.selectionColor = new Color(Gold.r, Gold.g, Gold.b, 0.6f);
            field.contentType = numeric ? TMP_InputField.ContentType.IntegerNumber : TMP_InputField.ContentType.Standard;
            field.lineType = TMP_InputField.LineType.SingleLine;
            field.targetGraphic = bg;
            field.navigation = new Navigation { mode = Navigation.Mode.None };
            bg.gameObject.SetActive(true);
            field.text = value ?? "";
            return field;
        }

        // ───────────── Positionnement sur un point de l'écran ─────────────

        /// <summary>Position écran (pixels, origine en bas à gauche) → position locale dans le canvas du mod.</summary>
        public static Vector2 ToLocal(Vector2 screen)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(Root, screen, Cam, out var local);
            return local;
        }

        public static void AtScreen(RectTransform rt, Vector2 screen, Vector2 offset)
        {
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            var p = ToLocal(screen) + offset;
            rt.anchoredPosition = new Vector2(Mathf.Round(p.x), Mathf.Round(p.y));
        }

        public static Color Hex(string hex)
        {
            if (!string.IsNullOrEmpty(hex) && ColorUtility.TryParseHtmlString(hex, out var c)) return c;
            return Color.white;
        }
    }
}
