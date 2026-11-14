using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DitdLauncher
{
    /// <summary>Réglages : %APPDATA%\DitdCoopLauncher\settings.ini (format clé=valeur).</summary>
    public sealed class Settings
    {
        public string GameDir = "";
        public HashSet<string> Optional = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        public bool AutoUpdate = true;

        private static string FilePath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DitdCoopLauncher", "settings.ini");

        public static Settings Load()
        {
            var s = new Settings();
            try
            {
                if (!File.Exists(FilePath)) return s;
                foreach (var line in File.ReadAllLines(FilePath))
                {
                    int eq = line.IndexOf('=');
                    if (eq <= 0) continue;
                    var k = line.Substring(0, eq).Trim();
                    var v = line.Substring(eq + 1).Trim();
                    switch (k)
                    {
                        case "GameDir": s.GameDir = v; break;
                        case "Optional": s.Optional = new HashSet<string>(v.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()), StringComparer.OrdinalIgnoreCase); break;
                        case "AutoUpdate": s.AutoUpdate = v != "false"; break;
                    }
                }
            }
            catch { }
            return s;
        }

        public void Save()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
            File.WriteAllLines(FilePath, new[]
            {
                "GameDir=" + GameDir,
                "Optional=" + string.Join(",", Optional),
                "AutoUpdate=" + (AutoUpdate ? "true" : "false"),
            });
        }
    }
}
