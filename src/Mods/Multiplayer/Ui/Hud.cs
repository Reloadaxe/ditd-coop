using System;
using System.Collections.Generic;
using System.Linq;
using DiceLoader;
using DiceMultiplayer.Net;
using DiceMultiplayer.Sync;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static DiceMultiplayer.Ui.GameUi;
using Text = DiceMultiplayer.Ui.GameUi.Text;
using Image = UnityEngine.UI.Image;

namespace DiceMultiplayer.Ui
{
    /// <summary>
    /// Toute l'interface du mod, construite avec les ressources du jeu (voir GameUi) et mise à jour chaque frame :
    /// bouton « Multijoueur » de l'écran titre, panneau coop (F2), écran d'attente du client, liste des joueurs,
    /// bandeaux d'état (vote, fin de tour), messages, votes sur la carte, noms des héros, aperçus des dégâts des
    /// autres joueurs et curseurs.
    /// </summary>
    public static class Hud
    {
        private static MultiplayerMod M => MultiplayerMod.Instance;
        private static bool _built;

        // Écran titre
        private static Btn _menuBtn;

        // Panneau coop
        private static RectTransform _panel, _offline, _online, _playersRows;
        private static TMP_InputField _fName, _fPort, _fHost, _fChat;
        private static Text _status;   // (journal retiré du panneau : il cachait le reste ; il reste dans le fichier de log)
        private static Btn _cursorsBtn, _difficultyBtn;
        private static readonly float[] DifficultySteps = { 0.5f, 0.75f, 1f, 1.25f, 1.5f, 2f };

        private static void CycleDifficulty()
        {
            var cur = M.HpPerExtraPlayer;
            int i = Array.FindIndex(DifficultySteps, v => v > cur + 0.01f);
            M.HpPerExtraPlayer = i < 0 ? DifficultySteps[0] : DifficultySteps[i];
        }
        private static string _playersSig;

        // Écran d'attente
        private static RectTransform _waiting;
        private static Text _waitText, _waitCount;

        // Liste des joueurs, bandeau, message, alerte
        private static RectTransform _players, _playersList, _banner, _toast, _desync;
        private static Text _bannerText, _toastText, _desyncText;
        private static CanvasGroup _toastGroup;
        private static string _listSig;

        // Éléments posés sur la scène
        private static readonly List<RectTransform> _votePool = new List<RectTransform>();
        private static readonly List<Text> _heroPool = new List<Text>();
        private static readonly List<(RectTransform box, Text text)> _previewPool = new List<(RectTransform, Text)>();
        private static readonly List<(RectTransform root, Image arrow, Text name)> _cursorPool = new List<(RectTransform, Image, Text)>();

        public static void Update()
        {
            if (!_built)
            {
                if (!Ensure()) return;
                try { Build(); _built = true; }
                catch (Exception e) { M.Logger.LogError("Interface du mod : " + e); _built = true; return; }
            }
            if (Root == null) return;
            try { AttachToCursorLayer(); } catch (Exception e) { Debug("calque", e); }

            try { UpdateMenuButton(); } catch (Exception e) { Debug("menu", e); }
            try { UpdatePanel(); } catch (Exception e) { Debug("panneau", e); }
            try { UpdateWaiting(); } catch (Exception e) { Debug("attente", e); }
            try { UpdatePlayers(); } catch (Exception e) { Debug("joueurs", e); }
            try { UpdateBanner(); } catch (Exception e) { Debug("bandeau", e); }
            try { UpdateToast(); } catch (Exception e) { Debug("message", e); }
            try { UpdateVotes(); } catch (Exception e) { Debug("votes", e); }
            try { UpdateHeroes(); } catch (Exception e) { Debug("héros", e); }
            try { UpdatePreviews(); } catch (Exception e) { Debug("aperçus", e); }
            try { UpdateCursors(); } catch (Exception e) { Debug("curseurs", e); }
        }

        /// <summary>Vrai si ce point de l'écran est sur une fenêtre du mod : le jeu ne doit pas recevoir le clic.</summary>
        public static bool BlocksPointer(Vector2 screen)
        {
            if (Root == null) return false;
            return Contains(_waiting, screen) || Contains(_panel, screen) || (_menuBtn != null && Contains(_menuBtn.Rect, screen));
        }

        private static float _nextDebug;
        private static void Debug(string what, Exception e)
        {
            if (Time.unscaledTime < _nextDebug) return;
            _nextDebug = Time.unscaledTime + 5f;
            M.Logger.LogWarning($"Interface ({what}) : {e.Message}");
        }

        private static void SetActive(Component c, bool on)
        {
            if (c != null && c.gameObject.activeSelf != on) c.gameObject.SetActive(on);
        }

        // ───────────── Construction ─────────────

        private static void Build()
        {
            // Bouton de l'écran titre (en haut à gauche)
            _menuBtn = Button(Root, "Multijoueur", () => M.PanelOpen = true, 84, 18);
            Place(_menuBtn.Rect, new Vector2(0, 1), new Vector2(0, 1), new Vector2(6, -6), new Vector2(84, 18));

            BuildPlayers();
            BuildBanner();
            BuildToast();
            BuildWaiting();
            BuildPanel();   // en dernier : au-dessus du reste
        }

        private static void BuildPanel()
        {
            var win = Box(Root, SprWindow, 12, 10, 4, false);
            win.GetComponent<VerticalLayoutGroup>().childForceExpandWidth = true;
            var fit = win.gameObject.AddComponent<ContentSizeFitter>();
            fit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            Place(win, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(10, 0), new Vector2(214, 200));
            win.GetComponent<Image>().raycastTarget = true;   // les clics sur le panneau ne traversent pas
            _panel = win;

            var head = Row(win, 4);
            var title = Label(head, "Multijoueur coop", 9, Gold, TextAlignmentOptions.MidlineLeft, false);
            Size(title.Rect.GetComponent<TextMeshProUGUI>(), -1, 12, 1);
            var close = IconButton(head, SprClose, () => M.PanelOpen = false, 10);
            Size(close.GetComponent<Image>(), 10, 10);

            // Hors ligne
            _offline = Group(win);
            var r1 = Row(_offline); Caption(r1, "Pseudo"); _fName = Field(r1, M.PlayerName);
            var r2 = Row(_offline); Caption(r2, "Port"); _fPort = Field(r2, M.Port.ToString(), 50, 17, true); Size(_fPort.GetComponent<Image>(), 50, 17, 0);
            Section(_offline, "Héberger");
            Hint(_offline, "Partage ton adresse IP (locale, ou celle de Radmin VPN, ZeroTier, Tailscale) et ouvre le port TCP dans le pare-feu.");
            var r3 = Row(_offline); Button(r3, "Créer la partie", () => M.HostGame(), 96, 18);
            Section(_offline, "Rejoindre");
            var r4 = Row(_offline); Caption(r4, "IP de l'hôte"); _fHost = Field(r4, M.HostIp);
            var r5 = Row(_offline); Button(r5, "Rejoindre", () => M.JoinGame(), 96, 18);

            // En ligne
            _online = Group(win);
            _status = Label(_online, "", 7, Paper);
            _playersRows = Group(_online);
            _playersRows.GetComponent<VerticalLayoutGroup>().spacing = 2;
            Hint(_online, "Nouvelle run : chacun choisit son personnage, puis la partie démarre quand tout le monde est prêt.");
            var r6 = Row(_online); Button(r6, "Quitter la partie", () => M.LeaveGame(), 96, 18);
            var rd = Row(_online); _difficultyBtn = Button(rd, "", CycleDifficulty, 130, 16);
            Hint(_online, "PV des ennemis selon le nombre de joueurs (choisi par l'hôte, figé au lancement de la run).");
            var r7 = Row(_online); _fChat = Field(r7, ""); Button(r7, "Envoyer", SendChat, 48, 16);

            // Commun
            var r8 = Row(win);
            _cursorsBtn = Button(r8, "", () => M.ShowCursors = !M.ShowCursors, 120, 16);
            _panel.gameObject.SetActive(false);
        }

        /// <summary>Entrée (depuis OnGUI) : envoie le message du chat s'il est en cours de saisie.</summary>
        public static void OnReturn()
        {
            if (_fChat != null && (_fChat.isFocused || !string.IsNullOrWhiteSpace(_fChat.text))) SendChat();
        }

        private static void SendChat()
        {
            var t = _fChat.text?.Trim();
            if (string.IsNullOrEmpty(t)) return;
            M.SendChat(t);
            _fChat.text = "";
        }

        private static RectTransform Group(Transform parent)
        {
            var g = Node("Group", parent);
            var v = g.gameObject.AddComponent<VerticalLayoutGroup>();
            v.spacing = 4;
            v.childControlWidth = true; v.childControlHeight = true;
            v.childForceExpandWidth = true; v.childForceExpandHeight = false;
            return g;
        }

        private static void Caption(Transform row, string text)
        {
            var l = Label(row, text, 7, Paper, TextAlignmentOptions.MidlineLeft, false);
            Size(l.Rect.GetComponent<TextMeshProUGUI>(), 62, 17);
        }

        private static void Section(Transform parent, string text) => Label(parent, text, 7, Gold);

        private static void Hint(Transform parent, string text) => Label(parent, text, 7, Muted);

        private static void BuildWaiting()
        {
            var dim = Image(Root, null, new Color(Ink.r, Ink.g, Ink.b, 0.78f), "Waiting");
            dim.raycastTarget = true;   // bloque les clics sur l'écran titre
            Stretch(dim.rectTransform);
            _waiting = dim.rectTransform;

            var win = Box(_waiting, SprWindow, 14, 12, 5, false, TextAnchor.UpperCenter);
            win.GetComponent<VerticalLayoutGroup>().childForceExpandWidth = true;
            win.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            Place(win, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(230, 100));
            Label(win, "Connecté à la partie", 9, Gold, TextAlignmentOptions.Top);
            _waitText = Label(win, "", 7, Paper, TextAlignmentOptions.Top);
            Label(win, "La sélection des personnages s'ouvrira automatiquement.", 7, Muted, TextAlignmentOptions.Top);
            _waitCount = Label(win, "", 7, Paper, TextAlignmentOptions.Top);
            var row = Row(win, 6, TextAnchor.MiddleCenter);
            Button(row, "Quitter", () => M.LeaveGame(), 70, 18);
            Button(row, "Panneau coop", () => M.PanelOpen = true, 80, 18);
            _waiting.gameObject.SetActive(false);
        }

        private static void BuildPlayers()
        {
            _players = Box(Root, SprBubble, 6, 5, 2);
            Place(_players, new Vector2(1, 1), new Vector2(1, 1), new Vector2(-6, -40), Vector2.zero);
            Label(_players, "Coop", 7, Gold, TextAlignmentOptions.TopLeft, false);
            _playersList = Group(_players);
            _playersList.GetComponent<VerticalLayoutGroup>().spacing = 2;
            _playersList.GetComponent<VerticalLayoutGroup>().childForceExpandWidth = false;
            _players.gameObject.SetActive(false);
        }

        private static void BuildBanner()
        {
            _banner = Box(Root, SprBubble, 8, 5, 0, true, TextAnchor.MiddleCenter);
            Place(_banner, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 24), Vector2.zero);
            _bannerText = Label(_banner, "", 7, Paper, TextAlignmentOptions.Center, false);
            _banner.gameObject.SetActive(false);
        }

        private static void BuildToast()
        {
            _toast = Box(Root, SprBubble, 8, 5, 0, true, TextAnchor.MiddleCenter);
            Place(_toast, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -34), Vector2.zero);
            _toastText = Label(_toast, "", 7, Paper, TextAlignmentOptions.Center, false);
            _toastGroup = _toast.gameObject.AddComponent<CanvasGroup>();
            _toastGroup.blocksRaycasts = false;
            _toast.gameObject.SetActive(false);

            _desync = Box(Root, SprBubbleDark, 8, 5, 0, true, TextAnchor.MiddleCenter);
            Place(_desync, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -58), Vector2.zero);
            _desyncText = Label(_desync, "", 7, Red, TextAlignmentOptions.Center, false);
            _desync.gameObject.SetActive(false);
        }

        // ───────────── Mise à jour ─────────────

        private static void UpdateMenuButton()
        {
            bool show = !Game.InRun && !M.PanelOpen && !Lobby.ClientWaiting && Game.State == GameManager.GameState.TitleScreen;
            _menuBtn.Active = show;
            if (show)
                _menuBtn.Label.Value = Coop.Active ? $"Coop : {Coop.Net.Players.Count} joueur{(Coop.Net.Players.Count > 1 ? "s" : "")}" : "Multijoueur";
        }

        private static void UpdatePanel()
        {
            bool open = M.PanelOpen;
            SetActive(_panel, open);
            if (!open) return;
            var net = Coop.Net;
            SetActive(_offline, net == null);
            SetActive(_online, net != null);

            if (net == null)
            {
                if (!_fName.isFocused && _fName.text != M.PlayerName) M.PlayerName = _fName.text;
                if (int.TryParse(_fPort.text, out var port) && port >= 1024 && port <= 65535) M.Port = port;
                if (_fHost.text != M.HostIp) M.HostIp = _fHost.text;
            }
            else
            {
                _status.Value = $"{(net.IsHost ? "Tu héberges la partie" : "Connecté à la partie")}\n<color=#e9cfae>{net.Status}</color>";
                var sig = string.Join("|", net.Players.Select(p => $"{p.Id}:{p.Name}:{p.Color}"));
                if (sig != _playersSig)
                {
                    _playersSig = sig;
                    Clear(_playersRows);
                    foreach (var p in net.Players.OrderBy(p => p.Id))
                    {
                        var row = Row(_playersRows, 4);
                        Chip(row, Hex(p.Color));
                        Label(row, $"{p.Name}{(p.Id == net.LocalId ? " <color=#e9cfae>(toi)</color>" : "")}{(p.Id == 0 ? " <color=#e9cfae>- hôte</color>" : "")}", 7, Paper, TextAlignmentOptions.MidlineLeft, false);
                    }
                }

            }
            _cursorsBtn.Label.Value = M.ShowCursors ? "Curseurs des autres : oui" : "Curseurs des autres : non";
            if (net != null)
            {
                bool host = net.IsHost && !Game.InRun;
                _difficultyBtn.Active = host;
                if (host) _difficultyBtn.Label.Value = $"PV des ennemis : x{Scaling.FactorFor(Math.Max(2, net.Players.Count), M.HpPerExtraPlayer):0.##}";
            }
        }

        private static void Clear(Transform t)
        {
            for (int i = t.childCount - 1; i >= 0; i--) UnityEngine.Object.Destroy(t.GetChild(i).gameObject);
        }

        private static void UpdateWaiting()
        {
            bool show = Lobby.ClientWaiting && !M.PanelOpen;
            SetActive(_waiting, show);
            if (!show) return;
            int dots = (int)(Time.unscaledTime * 2) % 4;
            _waitText.Value = $"En attente que <color={Coop.ColorOf(0)}>{Coop.NameOf(0)}</color> lance une nouvelle run{new string('.', dots)}";
            _waitCount.Value = $"Joueurs : {Coop.Net.Players.Count}/{NetSession.MaxPlayers}";
        }

        private static void UpdatePlayers()
        {
            bool show = Coop.Active && !M.PanelOpen && !Lobby.ClientWaiting && !Game.InBattle;
            SetActive(_players, show);
            if (!show) return;
            var lines = new List<(string color, string text)>();
            foreach (var p in Coop.Net.Players.OrderBy(p => p.Id))
            {
                string state;
                if (!Game.InRun) state = Team.Picks.TryGetValue(p.Id, out var pick) ? $"<color=#c3ff96>{Team.Describe(pick)}</color>" : "<color=#e9cfae>choisit...</color>";
                else
                {
                    var rp = Team.RunPicks.FirstOrDefault(r => r.Id == p.Id);
                    state = rp != null ? $"<color=#e9cfae>{(Player.CharacterName)rp.Run.Character}</color>" : "";
                }
                lines.Add((p.Color, $"{p.Name}{(p.Id == Coop.LocalId ? " (toi)" : "")}  {state}"));
            }
            var sig = string.Join("|", lines.Select(l => l.color + l.text));
            if (sig == _listSig) return;
            _listSig = sig;
            Clear(_playersList);
            foreach (var (color, text) in lines)
            {
                var row = Row(_playersList, 3);
                Chip(row, Hex(color), 4);
                Label(row, text, 7, Paper, TextAlignmentOptions.MidlineLeft, false);
            }
        }

        private static void UpdateBanner()
        {
            string text = Coop.ReadyStatus();
            float y = 24;
            if (text == null) { text = Coop.VoteStatus(); y = 8; }
            bool show = text != null && Coop.Active && !M.PanelOpen;
            SetActive(_banner, show);
            if (!show) return;
            _bannerText.Value = text;
            _banner.anchoredPosition = new Vector2(0, y);
        }

        private static void UpdateToast()
        {
            float left = Coop.ToastUntil - Time.unscaledTime;
            bool show = Coop.ToastText != null && left > 0;
            SetActive(_toast, show);
            if (show)
            {
                _toastText.Value = Coop.ToastText;
                _toastGroup.alpha = Mathf.Clamp01(left / 0.4f);
            }
            bool desync = Coop.Active && Coop.DesyncWarning != null;
            SetActive(_desync, desync);
            if (desync) _desyncText.Value = Coop.DesyncWarning;
        }

        // ───────────── Éléments posés sur la scène ─────────────

        private static void UpdateVotes()
        {
            var marks = Coop.VoteMarkers();
            for (int i = 0; i < marks.Count; i++)
            {
                if (i >= _votePool.Count)
                {
                    var box = Image(Root, SprBubble, null, "Vote").rectTransform;
                    var h = box.gameObject.AddComponent<HorizontalLayoutGroup>();
                    h.padding = new RectOffset(4, 4, 4, 4); h.spacing = 2;
                    h.childControlWidth = true; h.childControlHeight = true;
                    h.childForceExpandWidth = false; h.childForceExpandHeight = false;
                    var f = box.gameObject.AddComponent<ContentSizeFitter>();
                    f.horizontalFit = f.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                    box.pivot = new Vector2(0.5f, 0);
                    _votePool.Add(box);
                }
                var b = _votePool[i];
                SetActive(b, true);
                var key = string.Join(",", marks[i].voters);
                if (b.name != key)
                {
                    b.name = key;
                    Clear(b);
                    foreach (var id in marks[i].voters) Chip(b, Hex(Coop.ColorOf(id)), 6);
                }
                AtScreen(b, marks[i].screen, new Vector2(0, 12));
            }
            for (int i = marks.Count; i < _votePool.Count; i++) SetActive(_votePool[i], false);
        }

        private static void UpdateHeroes()
        {
            var labels = Game.InBattle ? Heroes.HeroLabels() : new List<(int id, Vector2 screen, string extra)>();
            for (int i = 0; i < labels.Count; i++)
            {
                if (i >= _heroPool.Count)
                {
                    var t = Label(Root, "", 7, Paper, TextAlignmentOptions.Bottom, false);
                    t.Rect.sizeDelta = new Vector2(120, 10);
                    t.Rect.pivot = new Vector2(0.5f, 0);
                    _heroPool.Add(t);
                }
                var (id, screen, extra) = labels[i];
                var lbl = _heroPool[i];
                lbl.Active = true;
                string ready = Coop.ReadyPlayers.Contains(id) ? "  <color=#c3ff96>prêt</color>" : "";
                lbl.Value = $"<color={Coop.ColorOf(id)}>{Coop.NameOf(id)}</color>{ready}{(extra != null ? "  " + extra : "")}";
                AtScreen(lbl.Rect, screen, new Vector2(0, 1));
            }
            for (int i = labels.Count; i < _heroPool.Count; i++) _heroPool[i].Active = false;
        }

        private static void UpdatePreviews()
        {
            var marks = Preview.RemoteMarkers();
            for (int i = 0; i < marks.Count; i++)
            {
                if (i >= _previewPool.Count)
                {
                    var box = Image(Root, SprBubble, null, "Preview").rectTransform;
                    var h = box.gameObject.AddComponent<HorizontalLayoutGroup>();
                    h.padding = new RectOffset(4, 5, 3, 3); h.spacing = 2;
                    h.childAlignment = TextAnchor.MiddleLeft;
                    h.childControlWidth = true; h.childControlHeight = true;
                    h.childForceExpandWidth = false; h.childForceExpandHeight = false;
                    var f = box.gameObject.AddComponent<ContentSizeFitter>();
                    f.horizontalFit = f.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                    box.pivot = new Vector2(0.5f, 0);
                    var chips = Node("Chips", box);
                    var ch = chips.gameObject.AddComponent<HorizontalLayoutGroup>();
                    ch.spacing = 1; ch.childControlWidth = true; ch.childControlHeight = true;
                    ch.childForceExpandWidth = false; ch.childForceExpandHeight = false;
                    var txt = Label(box, "", 7, Paper, TextAlignmentOptions.MidlineLeft, false);
                    _previewPool.Add((box, txt));
                }
                var (b, text) = _previewPool[i];
                SetActive(b, true);
                var chipsRt = (RectTransform)b.GetChild(0);
                var key = string.Join(",", marks[i].ids);
                if (chipsRt.name != key)
                {
                    chipsRt.name = key;
                    Clear(chipsRt);
                    foreach (var id in marks[i].ids) Chip(chipsRt, Hex(Coop.ColorOf(id)), 4);
                }
                text.Value = $"+{marks[i].total}";
                AtScreen(b, marks[i].screen, new Vector2(0, 2));
            }
            for (int i = marks.Count; i < _previewPool.Count; i++) SetActive(_previewPool[i].box, false);
        }

        private static void UpdateCursors()
        {
            var list = new List<(int id, Vector2 screen)>();
            if (Coop.Active && M.ShowCursors)
            {
                string scene = Game.State.ToString();
                foreach (var kv in Coop.Cursors)
                {
                    if (kv.Key == Coop.LocalId || kv.Value.scene != scene || Time.unscaledTime - kv.Value.time > 3f) continue;
                    list.Add((kv.Key, new Vector2(kv.Value.x * Screen.width, kv.Value.y * Screen.height)));
                }
            }
            for (int i = 0; i < list.Count; i++)
            {
                if (i >= _cursorPool.Count)
                {
                    var root = Node("Cursor", Root);
                    root.sizeDelta = new Vector2(12, 12);
                    root.pivot = new Vector2(0, 1);
                    var arrow = Image(root, SprCursor);
                    arrow.preserveAspect = true;
                    Place(arrow.rectTransform, new Vector2(0, 1), new Vector2(0, 1), Vector2.zero, new Vector2(12, 12));
                    var name = Label(root, "", 7, Paper, TextAlignmentOptions.TopLeft, false);
                    Place(name.Rect, new Vector2(0, 1), new Vector2(0, 1), new Vector2(11, -9), new Vector2(80, 8));
                    _cursorPool.Add((root, arrow, name));
                }
                var (r, a, n) = _cursorPool[i];
                SetActive(r, true);
                var col = Hex(Coop.ColorOf(list[i].id));
                a.color = Color.Lerp(Color.white, col, 0.6f);
                n.Value = $"<color={Coop.ColorOf(list[i].id)}>{Coop.NameOf(list[i].id)}</color>";
                AtScreen(r, list[i].screen, Vector2.zero);
                r.SetAsLastSibling();
            }
            for (int i = list.Count; i < _cursorPool.Count; i++) SetActive(_cursorPool[i].root, false);
        }
    }
}
