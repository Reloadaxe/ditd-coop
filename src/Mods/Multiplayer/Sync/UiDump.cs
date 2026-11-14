using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DiceLoader;
using UnityEngine;
using UnityEngine.UI;

namespace DiceMultiplayer.Sync
{
    /// <summary>
    /// Outil de développement (argument -ditd-uidump) : inventaire des ressources d'interface du jeu pour intégrer
    /// l'interface du mod au style du jeu. Sur l'écran titre : liste des sprites (taille, bordures 9-slice), polices
    /// TMP et images visibles, capture de l'écran, puis planches d'aperçu des sprites candidats, puis fermeture du jeu.
    /// Fichiers écrits dans Mods/Multiplayer/uidump/.
    /// </summary>
    public static class UiDump
    {
        public static bool Enabled => Environment.GetCommandLineArgs().Any(a => a == "-ditd-uidump");

        private static int _step = -1;
        private static float _next;
        private static string _dir;
        private static List<Sprite> _cands;
        private static int _page;
        private static GameObject _canvas;
        private const int PerPage = 48;

        public static bool TestEnabled => Environment.GetCommandLineArgs().Any(a => a == "-ditd-uitest");
        private static int _t = -1;

        /// <summary>
        /// Test visuel de l'interface du mod (argument -ditd-uitest) : captures de l'écran titre, du panneau hors ligne,
        /// du panneau en ligne (partie hébergée en local) avec un message, puis fermeture du jeu.
        /// </summary>
        private static void UpdateTest()
        {
            var M = MultiplayerMod.Instance;
            if (_t == -1)
            {
                if (Game.State != GameManager.GameState.TitleScreen) return;
                _t = 0; _next = Time.unscaledTime + 6f;
                _dir = System.IO.Path.Combine(DiceLoader.Loader.ModsDirectory, "Multiplayer", "uidump");
                Directory.CreateDirectory(_dir);
                return;
            }
            if (Time.unscaledTime < _next) return;
            _next = Time.unscaledTime + 1.5f;
            switch (_t++)
            {
                case 0: ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(_dir, "t1_title.png")); break;
                case 1: M.PanelOpen = true; break;
                case 2: ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(_dir, "t2_panel.png")); break;
                case 3: M.HostGame(); Coop.Toast("Message de test : vote enregistré"); break;
                case 4: ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(_dir, "t3_online.png")); break;
                case 5: M.PanelOpen = false; Coop.Toast("Prêt ! En attente des autres joueurs"); break;
                case 6: ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(_dir, "t4_hud.png")); break;
                case 7: M.LeaveGame(); File.WriteAllText(System.IO.Path.Combine(_dir, "test_done.txt"), "ok"); Application.Quit(); break;
            }
        }

        public static void Update()
        {
            if (TestEnabled) { try { UpdateTest(); } catch (Exception e) { MultiplayerMod.Instance.Logger.LogWarning("UiTest : " + e); } return; }
            if (!Enabled) return;
            if (_step == -1)
            {
                if (Game.State != GameManager.GameState.TitleScreen) return;
                _step = 0; _next = Time.unscaledTime + 6f;
                _dir = System.IO.Path.Combine(DiceLoader.Loader.ModsDirectory, "Multiplayer", "uidump");
                Directory.CreateDirectory(_dir);
                return;
            }
            if (Time.unscaledTime < _next) return;
            _next = Time.unscaledTime + 1.5f;
            try { Step(); } catch (Exception e) { MultiplayerMod.Instance.Logger.LogWarning("UiDump : " + e); _step = 99; }
        }

        private static void Step()
        {
            switch (_step)
            {
                case 0:
                    ScreenCapture.CaptureScreenshot(Path.Combine(_dir, "title.png"));
                    WriteReport();
                    _step = 1; _page = 0;
                    break;
                case 1:
                    if (_canvas != null) UnityEngine.Object.Destroy(_canvas);
                    if (_page * PerPage >= _cands.Count) { _step = 99; break; }
                    _canvas = BuildPage(_page);
                    _step = 2;
                    break;
                case 2:
                    ScreenCapture.CaptureScreenshot(Path.Combine(_dir, $"sprites_{_page:00}.png"));
                    _page++; _step = 1;
                    break;
                case 99:
                    if (_canvas != null) UnityEngine.Object.Destroy(_canvas);
                    File.WriteAllText(Path.Combine(_dir, "done.txt"), "ok");
                    _step = 100;
                    Application.Quit();
                    break;
            }
        }

        private static void WriteReport()
        {
            var sb = new StringBuilder();
            var all = Resources.FindObjectsOfTypeAll<Sprite>().Where(s => s != null).ToList();
            sb.AppendLine($"=== Sprites chargés : {all.Count}");
            foreach (var s in all.OrderBy(s => s.name))
            {
                var b = s.border;
                sb.AppendLine($"{s.name}\t{(int)s.rect.width}x{(int)s.rect.height}\tborder={b.x},{b.y},{b.z},{b.w}\ttex={s.texture?.name}");
            }

            sb.AppendLine("=== Polices TMP");
            foreach (var f in Resources.FindObjectsOfTypeAll<TMPro.TMP_FontAsset>())
                sb.AppendLine($"{f.name}");
            sb.AppendLine("=== Matériaux de police");
            foreach (var t in UnityEngine.Object.FindObjectsOfType<TMPro.TextMeshProUGUI>())
                sb.AppendLine($"{TPath(t.transform)}\tfont={t.font?.name}\tmat={t.fontSharedMaterial?.name}\tsize={t.fontSize}\tcol={t.color}\ttext={(t.text ?? "").Replace('\n', ' ')}");

            sb.AppendLine("=== Images visibles");
            foreach (var img in UnityEngine.Object.FindObjectsOfType<Image>())
                if (img.sprite != null && img.isActiveAndEnabled)
                    sb.AppendLine($"{TPath(img.transform)}\tsprite={img.sprite.name}\ttype={img.type}\tcol={img.color}\tsize={img.rectTransform.rect.width}x{img.rectTransform.rect.height}");

            sb.AppendLine("=== Canvas");
            foreach (var c in UnityEngine.Object.FindObjectsOfType<Canvas>())
            {
                var cs = c.GetComponent<CanvasScaler>();
                sb.AppendLine($"{TPath(c.transform)}\tmode={c.renderMode}\torder={c.sortingOrder}\tscaler={(cs != null ? $"{cs.uiScaleMode} ref={cs.referenceResolution} match={cs.matchWidthOrHeight} ppu={cs.referencePixelsPerUnit}" : "-")}");
            }

            // Candidats : sprites d'interface (9-slice, ou nom évocateur), pas trop petits
            var keys = new[] { "panel", "frame", "button", "btn", "box", "tooltip", "window", "banner", "bg", "background", "border", "popup", "slot", "label", "tab", "bar", "card", "plate", "ribbon", "scroll", "parchment", "board" };
            _cands = all.Where(s => s.rect.width >= 12 && s.rect.height >= 12 &&
                                    (s.border.sqrMagnitude > 0 || keys.Any(k => s.name.ToLowerInvariant().Contains(k))))
                        .GroupBy(s => s.name).Select(g => g.First()).OrderBy(s => s.name).ToList();
            sb.AppendLine($"=== Candidats ({_cands.Count}) : index → nom (page = index / {PerPage})");
            for (int i = 0; i < _cands.Count; i++)
                sb.AppendLine($"{i}\t{_cands[i].name}\t{(int)_cands[i].rect.width}x{(int)_cands[i].rect.height}\tborder={_cands[i].border}");
            File.WriteAllText(System.IO.Path.Combine(_dir, "report.txt"), sb.ToString());
        }

        private static GameObject BuildPage(int page)
        {
            var go = new GameObject("DIM_UiDump");
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 32000;
            var bg = new GameObject("bg").AddComponent<Image>();
            bg.transform.SetParent(go.transform, false);
            bg.color = new Color(0.35f, 0.35f, 0.38f, 1f);
            var brt = bg.rectTransform; brt.anchorMin = Vector2.zero; brt.anchorMax = Vector2.one; brt.offsetMin = brt.offsetMax = Vector2.zero;

            var font = Resources.FindObjectsOfTypeAll<TMPro.TMP_FontAsset>().FirstOrDefault();
            int cols = 8, rows = PerPage / cols;
            float cw = Screen.width / (float)cols, ch = Screen.height / (float)rows;
            for (int k = 0; k < PerPage; k++)
            {
                int i = page * PerPage + k;
                if (i >= _cands.Count) break;
                var s = _cands[i];
                int cx = k % cols, cy = k / cols;
                var img = new GameObject("s" + i).AddComponent<Image>();
                img.transform.SetParent(go.transform, false);
                img.sprite = s;
                img.type = s.border.sqrMagnitude > 0 ? Image.Type.Sliced : Image.Type.Simple;
                if (img.type == Image.Type.Simple) img.preserveAspect = true;
                var rt = img.rectTransform;
                rt.anchorMin = rt.anchorMax = new Vector2(0, 1);
                rt.pivot = new Vector2(0, 1);
                rt.anchoredPosition = new Vector2(cx * cw + 6, -(cy * ch + 6));
                rt.sizeDelta = new Vector2(cw - 12, ch - 30);

                var t = new GameObject("t" + i).AddComponent<TMPro.TextMeshProUGUI>();
                t.transform.SetParent(go.transform, false);
                if (font != null) t.font = font;
                t.fontSize = 14; t.color = Color.yellow;
                t.text = $"{i}";
                var trt = t.rectTransform;
                trt.anchorMin = trt.anchorMax = new Vector2(0, 1);
                trt.pivot = new Vector2(0, 1);
                trt.anchoredPosition = new Vector2(cx * cw + 6, -(cy * ch + ch - 24));
                trt.sizeDelta = new Vector2(cw - 12, 22);
            }
            return go;
        }

        private static string TPath(Transform t)
        {
            var parts = new List<string>();
            while (t != null) { parts.Add(t.name); t = t.parent; }
            parts.Reverse();
            return string.Join("/", parts);
        }
    }
}
