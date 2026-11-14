using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DitdLauncher
{
    public sealed class MainForm : Form
    {
        private readonly Settings _settings = Settings.Load();
        private readonly Updater _updater = new Updater();
        private Manifest _manifest;
        private bool _busy;

        private readonly TextBox _gameDir = new TextBox { Dock = DockStyle.Fill };
        private readonly Button _browse = new Button { Text = "Parcourir…", AutoSize = true };
        private readonly Label _status = new Label { AutoSize = true, Padding = new Padding(0, 6, 0, 6) };
        private readonly FlowLayoutPanel _options = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, WrapContents = true };
        private readonly CheckBox _autoUpdate = new CheckBox { Text = "Mettre à jour automatiquement avant de jouer", AutoSize = true };
        private readonly Button _play = new Button { Text = "▶  JOUER", Height = 44, Dock = DockStyle.Fill, Font = new Font("Segoe UI", 12f, FontStyle.Bold) };
        private readonly Button _install = new Button { Text = "Installer / Mettre à jour", AutoSize = true };
        private readonly Button _check = new Button { Text = "Vérifier", AutoSize = true };
        private readonly Button _disable = new Button { Text = "Désactiver les mods", AutoSize = true };
        private readonly Button _openDir = new Button { Text = "Dossier du jeu", AutoSize = true };
        private readonly Button _openLogs = new Button { Text = "Logs", AutoSize = true };
        private readonly LinkLabel _repo = new LinkLabel { Text = Updater.RepoUrl, AutoSize = true };
        private readonly ProgressBar _progress = new ProgressBar { Dock = DockStyle.Fill, Height = 14 };
        private readonly TextBox _log = new TextBox { Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical, Dock = DockStyle.Fill, BackColor = Color.FromArgb(24, 24, 28), ForeColor = Color.Gainsboro, Font = new Font("Consolas", 9f) };

        public MainForm()
        {
            Text = $"Die in the Dungeon - Coop Launcher v{Updater.CurrentLauncherVersion.ToString(3)}";
            Width = 720; Height = 560; MinimumSize = new Size(600, 460);
            StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("Segoe UI", 9.5f);
            try { Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath); } catch { }

            var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, Padding = new Padding(12) };
            Controls.Add(root);

            root.Controls.Add(new Label { Text = "Dossier du jeu (celui qui contient « Die in the Dungeon.exe ») :", AutoSize = true });
            var dirRow = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 2, AutoSize = true };
            dirRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            dirRow.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            dirRow.Controls.Add(_gameDir, 0, 0);
            dirRow.Controls.Add(_browse, 1, 0);
            root.Controls.Add(dirRow);

            root.Controls.Add(_status);
            root.Controls.Add(new Label { Text = "Composants optionnels :", AutoSize = true });
            root.Controls.Add(_options);
            root.Controls.Add(_autoUpdate);
            root.Controls.Add(_play);

            var buttons = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true };
            buttons.Controls.AddRange(new Control[] { _install, _check, _disable, _openDir, _openLogs });
            root.Controls.Add(buttons);
            root.Controls.Add(_progress);
            root.RowStyles.Clear();
            for (int i = 0; i < 9; i++) root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.Controls.Add(_log);
            root.Controls.Add(_repo);

            _gameDir.Text = _settings.GameDir;
            if (!Updater.IsValidGameDir(_gameDir.Text))
            {
                var auto = Updater.AutoDetectGameDir();
                if (auto != null) _gameDir.Text = auto;
            }
            _autoUpdate.Checked = _settings.AutoUpdate;

            _gameDir.TextChanged += (s, e) => { SaveSettings(); RefreshStatus(); };
            _browse.Click += (s, e) => Browse();
            _autoUpdate.CheckedChanged += (s, e) => SaveSettings();
            _play.Click += async (s, e) => await Run(PlayAsync);
            _install.Click += async (s, e) => await Run(InstallAsync);
            _check.Click += async (s, e) => await Run(CheckAsync);
            _disable.Click += (s, e) => { if (ValidDir()) { _updater.DisableMods(GameDir); RefreshStatus(); } };
            _openDir.Click += (s, e) => { if (ValidDir()) Process.Start("explorer.exe", GameDir); };
            _openLogs.Click += (s, e) =>
            {
                var p = Path.Combine(GameDir, "BepInEx", "LogOutput.log");
                if (File.Exists(p)) Process.Start("notepad.exe", p); else Log("Pas encore de log : lance le jeu une fois.");
            };
            _repo.LinkClicked += (s, e) => Process.Start(Updater.RepoUrl);

            _updater.Log += Log;
            _updater.Progress += p => UI(() =>
            {
                if (p < 0) _progress.Style = ProgressBarStyle.Marquee;
                else { _progress.Style = ProgressBarStyle.Blocks; _progress.Value = Math.Max(0, Math.Min(100, p)); }
            });

            Shown += async (s, e) => { RefreshStatus(); await Run(CheckAsync); };
        }

        private string GameDir => _gameDir.Text.Trim().Trim('"');

        // ───────────── Actions ─────────────

        private async Task CheckAsync()
        {
            Log("Recherche de mises à jour…");
            _manifest = await _updater.FetchManifestAsync();
            Log($"Version disponible : {_manifest.Version}" + (string.IsNullOrWhiteSpace(_manifest.Changelog) ? "" : $" - {_manifest.Changelog}"));
            UI(BuildOptions);

            if (Updater.LauncherUpdateAvailable(_manifest))
            {
                var r = MessageBox.Show(this, $"Une nouvelle version du launcher est disponible ({_manifest.Launcher.Version}).\nLa télécharger maintenant ?",
                    "Mise à jour du launcher", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                if (r == DialogResult.Yes)
                {
                    await _updater.SelfUpdateAsync(_manifest);
                    UI(Close);
                    return;
                }
            }

            if (ValidDir(false))
            {
                var pending = Updater.PendingFiles(_manifest, GameDir, _settings.Optional);
                Log(pending.Count == 0 && Updater.BepInExInstalled(GameDir)
                    ? "Les mods sont à jour."
                    : $"{pending.Count} fichier(s) à installer ou mettre à jour" + (Updater.BepInExInstalled(GameDir) ? "." : ", plus BepInEx."));
            }
            UI(RefreshStatus);
        }

        private async Task InstallAsync()
        {
            if (!ValidDir()) return;
            if (_manifest == null) await CheckAsync();
            await _updater.InstallAsync(_manifest, GameDir, _settings.Optional);
            UI(RefreshStatus);
        }

        private async Task PlayAsync()
        {
            if (!ValidDir()) return;
            if (Updater.IsGameRunning()) { Log("Le jeu est déjà lancé."); return; }
            if (_settings.AutoUpdate || !Updater.BepInExInstalled(GameDir))
            {
                try
                {
                    if (_manifest == null) await CheckAsync();
                    await _updater.InstallAsync(_manifest, GameDir, _settings.Optional);
                }
                catch (Exception e) when (Updater.BepInExInstalled(GameDir))
                {
                    Log("Mise à jour impossible (" + e.Message + "), lancement avec la version installée.");
                }
            }
            _updater.Launch(GameDir);
            UI(RefreshStatus);
        }

        private async Task Run(Func<Task> action)
        {
            if (_busy) return;
            _busy = true;
            SetEnabled(false);
            try { await action(); }
            catch (Exception e) { Log("ERREUR : " + e.Message); }
            finally
            {
                _busy = false;
                UI(() => { SetEnabled(true); _progress.Style = ProgressBarStyle.Blocks; });
            }
        }

        // ───────────── UI ─────────────

        private void BuildOptions()
        {
            _options.Controls.Clear();
            var opts = _manifest?.Files.Where(f => f.Optional != null).Select(f => f.Optional).Distinct().ToList();
            if (opts == null || opts.Count == 0) { _options.Controls.Add(new Label { Text = "(aucun)", AutoSize = true }); return; }
            foreach (var o in opts)
            {
                var cb = new CheckBox { Text = o, AutoSize = true, Checked = _settings.Optional.Contains(o), Margin = new Padding(0, 0, 16, 0) };
                cb.CheckedChanged += (s, e) =>
                {
                    if (cb.Checked) _settings.Optional.Add(o); else _settings.Optional.Remove(o);
                    SaveSettings();
                    Log($"{o} : {(cb.Checked ? "sera installé" : "sera retiré")} au prochain « Installer » ou « Jouer ».");
                };
                _options.Controls.Add(cb);
            }
        }

        private void RefreshStatus()
        {
            var d = GameDir;
            if (!Updater.IsValidGameDir(d))
            {
                _status.ForeColor = Color.Firebrick;
                _status.Text = "✖ Dossier invalide : « Die in the Dungeon.exe » et « GameAssembly.dll » introuvables.";
                return;
            }
            bool bep = Updater.BepInExInstalled(d), interop = Updater.InteropGenerated(d), enabled = Updater.ModsEnabled(d);
            bool loader = File.Exists(Path.Combine(d, "BepInEx", "plugins", "DiceLoader", "DiceLoader.dll"));
            int pending = _manifest == null ? -1 : Updater.PendingFiles(_manifest, d, _settings.Optional).Count;

            _status.ForeColor = bep && loader && enabled && pending == 0 ? Color.SeaGreen : Color.DarkOrange;
            _status.Text =
                $"BepInEx : {(bep ? "✔" : "✖ absent")}    DiceLoader : {(loader ? "✔" : "✖ absent")}    " +
                $"Injection : {(enabled ? "✔ active" : "✖ désactivée")}    Préparation 1er lancement : {(interop ? "✔" : "à faire")}\n" +
                (pending < 0 ? "Mises à jour : non vérifiées" : pending == 0 ? $"Mods à jour (v{_manifest.Version})" : $"{pending} mise(s) à jour disponible(s) - v{_manifest.Version}");
        }

        private void Browse()
        {
            using (var dlg = new FolderBrowserDialog { Description = "Sélectionne le dossier qui contient « Die in the Dungeon.exe »", SelectedPath = GameDir })
                if (dlg.ShowDialog(this) == DialogResult.OK) _gameDir.Text = dlg.SelectedPath;
        }

        private bool ValidDir(bool warn = true)
        {
            if (Updater.IsValidGameDir(GameDir)) return true;
            if (warn) Log("Choisis d'abord le dossier du jeu (bouton Parcourir…).");
            return false;
        }

        private void SaveSettings()
        {
            _settings.GameDir = GameDir;
            _settings.AutoUpdate = _autoUpdate.Checked;
            try { _settings.Save(); } catch { }
        }

        private void SetEnabled(bool on)
        {
            foreach (var c in new Control[] { _play, _install, _check, _disable, _browse, _gameDir }) c.Enabled = on;
        }

        private void Log(string s) => UI(() => _log.AppendText($"[{DateTime.Now:HH:mm:ss}] {s}{Environment.NewLine}"));

        private void UI(Action a)
        {
            if (IsDisposed) return;
            if (InvokeRequired) BeginInvoke(a); else a();
        }
    }
}
