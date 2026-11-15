using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace DitdLauncher
{
    /// <summary>
    /// Toute la logique d'installation et de mise à jour, sans UI.
    /// Source : dist/manifest.json sur la branche main du dépôt GitHub.
    /// </summary>
    public sealed class Updater
    {
        public const string Repo = "Reloadaxe/ditd-coop";
        public const string Branch = "main";
        public static string DistBase => $"https://raw.githubusercontent.com/{Repo}/{Branch}/dist/";
        public static string RepoUrl => $"https://github.com/{Repo}";

        public const string GameExe = "Die in the Dungeon.exe";

        public event Action<string> Log;
        public event Action<int> Progress; // 0..100, -1 = indéterminé

        private static readonly HttpClient Http;

        static Updater()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            Http = new HttpClient { Timeout = TimeSpan.FromMinutes(10) };
            Http.DefaultRequestHeaders.UserAgent.ParseAdd("DitdCoopLauncher/" + CurrentLauncherVersion);
            Http.DefaultRequestHeaders.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };
        }

        public static Version CurrentLauncherVersion => Assembly.GetExecutingAssembly().GetName().Version;

        private void L(string s) => Log?.Invoke(s);

        // ───────────── Diagnostic du dossier du jeu ─────────────

        public static bool IsValidGameDir(string dir) =>
            !string.IsNullOrWhiteSpace(dir) && File.Exists(Path.Combine(dir, GameExe)) && File.Exists(Path.Combine(dir, "GameAssembly.dll"));

        public static bool BepInExInstalled(string dir) => File.Exists(Path.Combine(dir, "BepInEx", "core", "BepInEx.Unity.IL2CPP.dll"));
        public static bool InteropGenerated(string dir) => File.Exists(Path.Combine(dir, "BepInEx", "interop", "Assembly-CSharp.dll"));
        public static bool ModsEnabled(string dir) => File.Exists(Path.Combine(dir, "winhttp.dll"));

        /// <summary>Cherche le jeu dans les emplacements Steam habituels.</summary>
        public static string AutoDetectGameDir()
        {
            var candidates = new List<string>();
            foreach (var drive in DriveInfo.GetDrives().Where(d => d.DriveType == DriveType.Fixed && d.IsReady))
            {
                candidates.Add(Path.Combine(drive.RootDirectory.FullName, @"Program Files (x86)\Steam\steamapps\common\Die in the Dungeon"));
                candidates.Add(Path.Combine(drive.RootDirectory.FullName, @"SteamLibrary\steamapps\common\Die in the Dungeon"));
                candidates.Add(Path.Combine(drive.RootDirectory.FullName, @"Steam\steamapps\common\Die in the Dungeon"));
            }
            return candidates.FirstOrDefault(IsValidGameDir);
        }

        // ───────────── Manifeste ─────────────

        /// <summary>
        /// Base des téléchargements pour cette vérification. raw.githubusercontent.com garde la branche en cache
        /// environ 5 minutes : on préfère l'URL du dernier commit (contenu immuable, jamais périmé), obtenue via l'API.
        /// </summary>
        private static string _resolvedBase;

        private async Task<string> ResolveDistBaseAsync()
        {
            try
            {
                using (var req = new HttpRequestMessage(HttpMethod.Get, $"https://api.github.com/repos/{Repo}/commits/{Branch}"))
                {
                    req.Headers.Accept.ParseAdd("application/vnd.github.sha");
                    using (var resp = await Http.SendAsync(req).ConfigureAwait(false))
                    {
                        if (resp.IsSuccessStatusCode)
                        {
                            var sha = (await resp.Content.ReadAsStringAsync().ConfigureAwait(false)).Trim();
                            if (sha.Length >= 7 && sha.All(Uri.IsHexDigit))
                                return $"https://raw.githubusercontent.com/{Repo}/{sha}/dist/";
                        }
                    }
                }
            }
            catch { }
            L("API GitHub indisponible, repli sur la branche (peut avoir quelques minutes de retard).");
            return DistBase;
        }

        public async Task<Manifest> FetchManifestAsync()
        {
            _resolvedBase = await ResolveDistBaseAsync().ConfigureAwait(false);
            var json = await Http.GetStringAsync(_resolvedBase + "manifest.json").ConfigureAwait(false);
            return Manifest.Parse(json.TrimStart('﻿'));
        }

        private static string Resolve(string url) => url.StartsWith("http", StringComparison.OrdinalIgnoreCase) ? url : (_resolvedBase ?? DistBase) + url;

        /// <summary>
        /// Remplace dest par tmp. Un fichier en cours d'utilisation (ex. CoopLink.exe lancé) ne peut pas être supprimé,
        /// mais Windows permet de le renommer : on le met de côté en « .old » (supprimé à la prochaine mise à jour) et le
        /// nouveau fichier prend sa place ; le programme en cours continue, la nouvelle version sert au prochain lancement.
        /// </summary>
        private static void ReplaceFile(string tmp, string dest)
        {
            var old = dest + ".old";
            try { if (File.Exists(old)) File.Delete(old); } catch { }
            if (File.Exists(dest))
            {
                try { File.Delete(dest); }
                catch (Exception e) when (e is IOException || e is UnauthorizedAccessException)
                {
                    try { File.Move(dest, old); }
                    catch { File.Delete(tmp); throw new IOException($"{Path.GetFileName(dest)} est utilisé par un autre programme : ferme-le puis réessaie."); }
                }
            }
            File.Move(tmp, dest);
        }

        public static string Sha256Of(string file)
        {
            if (!File.Exists(file)) return null;
            using (var sha = SHA256.Create())
            using (var fs = File.OpenRead(file))
                return BitConverter.ToString(sha.ComputeHash(fs)).Replace("-", "").ToLowerInvariant();
        }

        /// <summary>Fichiers du manifeste à télécharger : absents ou différents. Les optionnels non sélectionnés sont ignorés.</summary>
        public static List<RemoteFile> PendingFiles(Manifest m, string gameDir, ISet<string> selectedOptional)
        {
            return m.Files.Where(f => (f.Optional == null || selectedOptional.Contains(f.Optional))
                                      && !string.Equals(Sha256Of(Path.Combine(gameDir, f.Path)), f.Sha256, StringComparison.OrdinalIgnoreCase))
                          .ToList();
        }

        // ───────────── Installation ─────────────

        public async Task InstallAsync(Manifest m, string gameDir, ISet<string> selectedOptional)
        {
            if (IsGameRunning()) throw new InvalidOperationException("Ferme le jeu avant d'installer ou de mettre à jour.");

            // 1) BepInEx (couche d'injection)
            if (!BepInExInstalled(gameDir))
            {
                L($"Téléchargement de BepInEx {m.BepInEx.Version}…");
                var zip = Path.Combine(Path.GetTempPath(), "ditd-bepinex.zip");
                await DownloadAsync(m.BepInEx.Url, zip).ConfigureAwait(false);
                L("Extraction de BepInEx…");
                ExtractOverwrite(zip, gameDir);
                File.Delete(zip);
                L("BepInEx installé.");
            }
            var disabled = Path.Combine(gameDir, "winhttp.dll.disabled");
            if (File.Exists(disabled) && !File.Exists(Path.Combine(gameDir, "winhttp.dll")))
            {
                File.Move(disabled, Path.Combine(gameDir, "winhttp.dll"));
                L("Mods réactivés.");
            }
            Directory.CreateDirectory(Path.Combine(gameDir, "Mods"));

            // 2) Fichiers du loader et des mods
            var pending = PendingFiles(m, gameDir, selectedOptional);
            int i = 0;
            foreach (var f in pending)
            {
                i++;
                L($"[{i}/{pending.Count}] {f.Path}");
                var dest = Path.Combine(gameDir, f.Path);
                Directory.CreateDirectory(Path.GetDirectoryName(dest));
                var tmp = dest + ".download";
                await DownloadAsync(Resolve(f.Url), tmp).ConfigureAwait(false);
                var got = Sha256Of(tmp);
                if (!string.Equals(got, f.Sha256, StringComparison.OrdinalIgnoreCase))
                {
                    File.Delete(tmp);
                    throw new InvalidDataException($"Empreinte invalide pour {f.Path} (attendu {f.Sha256}, reçu {got}). Réessaie dans quelques minutes : GitHub met peut-être encore le fichier en cache.");
                }
                ReplaceFile(tmp, dest);
            }

            // 3) Optionnels décochés et fichiers obsolètes : suppression
            foreach (var f in m.Files.Where(f => f.Optional != null && !selectedOptional.Contains(f.Optional)))
            {
                var p = Path.Combine(gameDir, f.Path);
                if (File.Exists(p)) { File.Delete(p); L($"Retiré : {f.Path}"); }
            }
            foreach (var r in m.Remove ?? new List<string>())
            {
                var p = Path.Combine(gameDir, r);
                if (File.Exists(p)) { File.Delete(p); L($"Supprimé (obsolète) : {r}"); }
            }

            Progress?.Invoke(100);
            L(pending.Count == 0 ? "Tout est déjà à jour." : $"Installation terminée ({pending.Count} fichier(s)).");
        }

        /// <summary>Désactive l'injection sans rien supprimer : jeu vanilla.</summary>
        public void DisableMods(string gameDir)
        {
            var w = Path.Combine(gameDir, "winhttp.dll");
            var d = w + ".disabled";
            if (File.Exists(w)) { if (File.Exists(d)) File.Delete(d); File.Move(w, d); }
            L("Mods désactivés. Le jeu se lancera sans mods (Installer les réactive).");
        }

        public void Launch(string gameDir)
        {
            Process.Start(new ProcessStartInfo(Path.Combine(gameDir, GameExe)) { WorkingDirectory = gameDir, UseShellExecute = true });
            L(InteropGenerated(gameDir)
                ? "Jeu lancé. Menu des mods : F1."
                : "Jeu lancé. PREMIER LANCEMENT : BepInEx prépare le jeu (1 à 3 minutes, l'écran peut rester noir), ne ferme pas la fenêtre.");
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr after, int x, int y, int cx, int cy, uint flags);

        /// <summary>
        /// Lance plusieurs instances fenêtrées, côte à côte (test de la coop sur un seul PC).
        /// Les fenêtres se partagent la largeur de la zone de travail de l'écran principal, en 16:9.
        /// </summary>
        public async Task LaunchWindowedAsync(string gameDir, int count)
        {
            var area = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea;
            int w = area.Width / count;
            int h = Math.Min(area.Height, w * 9 / 16);
            // Taille du rendu un peu inférieure à la fenêtre (bordure et barre de titre)
            int cw = w - 16, ch = h - 39;
            var procs = new List<Process>();
            for (int i = 0; i < count; i++)
            {
                // -ditd-window : DiceLoader réimpose taille et position quand le jeu réapplique ses réglages d'affichage au menu
                var psi = new ProcessStartInfo(Path.Combine(gameDir, GameExe),
                    $"-screen-fullscreen 0 -window-mode windowed -screen-width {cw} -screen-height {ch} -ditd-window {area.Left + i * w} {area.Top} {w} {h} -ditd-player J{i + 1}")
                { WorkingDirectory = gameDir, UseShellExecute = false };
                procs.Add(Process.Start(psi));
                L($"Instance {i + 1}/{count} lancée (PID {procs[i].Id}).");
                await Task.Delay(1500).ConfigureAwait(false);
            }

            // Placement des fenêtres dès qu'elles apparaissent
            var placed = new HashSet<int>();
            for (int t = 0; t < 60 && placed.Count < count; t++)
            {
                for (int i = 0; i < procs.Count; i++)
                {
                    if (placed.Contains(i)) continue;
                    try
                    {
                        procs[i].Refresh();
                        var hwnd = procs[i].MainWindowHandle;
                        if (hwnd == IntPtr.Zero) continue;
                        SetWindowPos(hwnd, IntPtr.Zero, area.Left + i * w, area.Top, w, h, 0x0040 /* SWP_SHOWWINDOW */);
                        placed.Add(i);
                    }
                    catch { placed.Add(i); }
                }
                await Task.Delay(500).ConfigureAwait(false);
            }
            L(placed.Count == count
                ? $"{count} fenêtres placées côte à côte. Fenêtre de gauche : F2 > Créer la partie ; à droite : F2 > Rejoindre 127.0.0.1."
                : "Certaines fenêtres n'ont pas pu être placées (le jeu met peut-être du temps à démarrer).");
            if (!InteropGenerated(gameDir))
                L("PREMIER LANCEMENT : BepInEx prépare le jeu dans chaque instance, patiente 1 à 3 minutes.");
        }

        public static bool IsGameRunning() => Process.GetProcessesByName(Path.GetFileNameWithoutExtension(GameExe)).Length > 0;

        // ───────────── Auto-mise à jour du launcher ─────────────

        public static bool LauncherUpdateAvailable(Manifest m) =>
            m.Launcher != null && Version.TryParse(m.Launcher.Version, out var v) && v > CurrentLauncherVersion;

        /// <summary>Télécharge le nouveau launcher, remplace l'exe courant (renommé en .old) et le relance.</summary>
        public async Task SelfUpdateAsync(Manifest m)
        {
            var exe = Assembly.GetExecutingAssembly().Location;
            var tmp = exe + ".new";
            L($"Mise à jour du launcher vers {m.Launcher.Version}…");
            await DownloadAsync(Resolve(m.Launcher.Url), tmp).ConfigureAwait(false);
            if (!string.Equals(Sha256Of(tmp), m.Launcher.Sha256, StringComparison.OrdinalIgnoreCase))
            {
                File.Delete(tmp);
                throw new InvalidDataException("Empreinte du nouveau launcher invalide, mise à jour annulée.");
            }
            var old = exe + ".old";
            if (File.Exists(old)) File.Delete(old);
            File.Move(exe, old);       // un exe en cours d'exécution peut être renommé sous Windows
            File.Move(tmp, exe);
            Process.Start(new ProcessStartInfo(exe) { UseShellExecute = true });
        }

        public static void CleanupOldLauncher()
        {
            try
            {
                var old = Assembly.GetExecutingAssembly().Location + ".old";
                if (File.Exists(old)) File.Delete(old);
            }
            catch { }
        }

        // ───────────── Utilitaires ─────────────

        private async Task DownloadAsync(string url, string dest)
        {
            using (var resp = await Http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false))
            {
                resp.EnsureSuccessStatusCode();
                long total = resp.Content.Headers.ContentLength ?? -1;
                using (var src = await resp.Content.ReadAsStreamAsync().ConfigureAwait(false))
                using (var fs = File.Create(dest))
                {
                    var buf = new byte[81920];
                    long done = 0;
                    int n, lastPct = -2;
                    while ((n = await src.ReadAsync(buf, 0, buf.Length).ConfigureAwait(false)) > 0)
                    {
                        await fs.WriteAsync(buf, 0, n).ConfigureAwait(false);
                        done += n;
                        int pct = total > 0 ? (int)(done * 100 / total) : -1;
                        if (pct != lastPct) { lastPct = pct; Progress?.Invoke(pct); }
                    }
                }
            }
        }

        private static void ExtractOverwrite(string zip, string destDir)
        {
            using (var archive = ZipFile.OpenRead(zip))
            {
                var root = Path.GetFullPath(destDir);
                foreach (var e in archive.Entries)
                {
                    var target = Path.GetFullPath(Path.Combine(destDir, e.FullName));
                    if (!target.StartsWith(root, StringComparison.OrdinalIgnoreCase)) continue; // protection zip-slip
                    if (string.IsNullOrEmpty(e.Name)) { Directory.CreateDirectory(target); continue; }
                    Directory.CreateDirectory(Path.GetDirectoryName(target));
                    e.ExtractToFile(target, true);
                }
            }
        }
    }
}
