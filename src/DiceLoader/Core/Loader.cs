using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

namespace DiceLoader
{
    /// <summary>Un mod découvert dans le dossier Mods/.</summary>
    public sealed class LoadedMod
    {
        public DiceMod Instance { get; internal set; }
        public DiceModInfoAttribute Info { get; internal set; }
        public string FilePath { get; internal set; }
        public ConfigEntry<bool> EnabledEntry { get; internal set; }
        /// <summary>Erreur de chargement ou d'exécution ; null si tout va bien.</summary>
        public string Error { get; internal set; }
        public bool IsEnabled => Instance != null && Instance.Enabled;
    }

    /// <summary>Découverte, chargement, ordre des dépendances et activation/désactivation des mods.</summary>
    public static class Loader
    {
        public const string ModsFolderName = "Mods";

        internal static ManualLogSource Log;
        private static ConfigFile _loaderConfig;
        private static readonly List<LoadedMod> _mods = new List<LoadedMod>();
        private static readonly HashSet<string> _searchDirs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public static IReadOnlyList<LoadedMod> Mods => _mods;
        public static string ModsDirectory { get; private set; }

        /// <summary>Retrouve un mod chargé par son id (pour l'interopérabilité entre mods).</summary>
        public static T Get<T>() where T : DiceMod => _mods.Select(m => m.Instance).OfType<T>().FirstOrDefault();
        public static LoadedMod Find(string id) => _mods.FirstOrDefault(m => string.Equals(m.Info?.Id, id, StringComparison.OrdinalIgnoreCase));

        internal static void Init(ManualLogSource log, ConfigFile loaderConfig)
        {
            Log = log;
            _loaderConfig = loaderConfig;
            ModsDirectory = Path.Combine(Paths.GameRootPath, ModsFolderName);
            Directory.CreateDirectory(ModsDirectory);
            Directory.CreateDirectory(Path.Combine(Paths.ConfigPath, "mods"));
            AppDomain.CurrentDomain.AssemblyResolve += ResolveFromModDirs;
        }

        internal static void LoadAll()
        {
            var files = Directory.GetFiles(ModsDirectory, "*.dll", SearchOption.AllDirectories)
                                 .OrderBy(f => f, StringComparer.OrdinalIgnoreCase).ToList();
            foreach (var f in files) _searchDirs.Add(Path.GetDirectoryName(f));

            Log.LogInfo($"Recherche de mods dans {ModsDirectory} : {files.Count} DLL");

            var found = new List<(Type type, DiceModInfoAttribute info, string file)>();
            foreach (var file in files)
            {
                Assembly asm;
                try { asm = Assembly.LoadFrom(file); }
                catch (Exception e) { Log.LogError($"Impossible de charger {Path.GetFileName(file)} : {e.Message}"); continue; }

                Type[] types;
                try { types = asm.GetTypes(); }
                catch (ReflectionTypeLoadException e)
                {
                    types = e.Types.Where(t => t != null).ToArray();
                    Log.LogWarning($"{Path.GetFileName(file)} : certains types n'ont pas pu être chargés ({e.LoaderExceptions.FirstOrDefault()?.Message})");
                }

                foreach (var t in types.Where(t => typeof(DiceMod).IsAssignableFrom(t) && !t.IsAbstract))
                {
                    var info = t.GetCustomAttribute<DiceModInfoAttribute>();
                    if (info == null)
                    {
                        Log.LogWarning($"{t.FullName} hérite de DiceMod mais n'a pas d'attribut [DiceModInfo], ignoré.");
                        continue;
                    }
                    if (found.Any(x => string.Equals(x.info.Id, info.Id, StringComparison.OrdinalIgnoreCase)))
                    {
                        Log.LogError($"Id de mod en double : {info.Id} ({Path.GetFileName(file)}), ignoré.");
                        continue;
                    }
                    found.Add((t, info, file));
                }
            }

            foreach (var entry in SortByDependencies(found))
                Instantiate(entry.type, entry.info, entry.file);

            Log.LogInfo($"{_mods.Count(m => m.Instance != null)} mod(s) chargé(s), {_mods.Count(m => m.IsEnabled)} actif(s).");
        }

        private static IEnumerable<(Type type, DiceModInfoAttribute info, string file)> SortByDependencies(
            List<(Type type, DiceModInfoAttribute info, string file)> mods)
        {
            var byId = mods.ToDictionary(m => m.info.Id, StringComparer.OrdinalIgnoreCase);
            var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var visiting = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var result = new List<(Type, DiceModInfoAttribute, string)>();

            bool Visit((Type type, DiceModInfoAttribute info, string file) m)
            {
                if (visited.Contains(m.info.Id)) return true;
                if (!visiting.Add(m.info.Id)) { Log.LogError($"Dépendance circulaire impliquant {m.info.Id}"); return false; }
                foreach (var dep in m.type.GetCustomAttributes<DiceModDependencyAttribute>())
                {
                    bool ok = byId.TryGetValue(dep.Id, out var d) && Visit(d) && result.Any(r => r.Item2.Id == d.info.Id);
                    if (!ok && !dep.Optional)
                    {
                        Log.LogError($"{m.info.Name} requiert le mod « {dep.Id} » (absent ou en erreur). Mod non chargé.");
                        _mods.Add(new LoadedMod { Info = m.info, FilePath = m.file, Error = $"Dépendance manquante : {dep.Id}" });
                        visiting.Remove(m.info.Id);
                        visited.Add(m.info.Id);
                        return false;
                    }
                }
                visiting.Remove(m.info.Id);
                visited.Add(m.info.Id);
                result.Add(m);
                return true;
            }

            foreach (var m in mods) Visit(m);
            return result;
        }

        private static void Instantiate(Type type, DiceModInfoAttribute info, string file)
        {
            var lm = new LoadedMod { Info = info, FilePath = file };
            _mods.Add(lm);
            lm.EnabledEntry = _loaderConfig.Bind("Mods", info.Id, true, $"{info.Name} v{info.Version} par {info.Author}");
            try
            {
                var mod = (DiceMod)Activator.CreateInstance(type);
                mod.Info = info;
                mod.Logger = Logger.CreateLogSource(info.Name);
                mod.Config = new ConfigFile(Path.Combine(Paths.ConfigPath, "mods", info.Id + ".cfg"), true);
                mod.Harmony = new Harmony(info.Id);
                mod.Directory = Path.GetDirectoryName(file);
                lm.Instance = mod;

                mod.OnLoad();
                Log.LogInfo($"Chargé : {info.Name} v{info.Version} ({info.Id})");
                if (lm.EnabledEntry.Value) SetEnabled(lm, true);
            }
            catch (Exception e)
            {
                lm.Error = e.Message;
                Log.LogError($"Erreur au chargement de {info.Name} : {e}");
            }
        }

        /// <summary>Active ou désactive un mod à chaud.</summary>
        public static void SetEnabled(LoadedMod lm, bool enabled)
        {
            if (lm?.Instance == null || lm.Instance.Enabled == enabled) return;
            var mod = lm.Instance;
            try
            {
                if (enabled)
                {
                    mod.Enabled = true;
                    mod.OnEnable();
                }
                else
                {
                    mod.OnDisable();
                    mod.Harmony.UnpatchSelf();
                    mod.Enabled = false;
                }
                lm.EnabledEntry.Value = enabled;
                lm.Error = null;
            }
            catch (Exception e)
            {
                lm.Error = e.Message;
                Log.LogError($"{(enabled ? "Activation" : "Désactivation")} de {lm.Info.Name} : {e}");
                if (enabled) { try { mod.Harmony.UnpatchSelf(); } catch { } mod.Enabled = false; }
            }
        }

        /// <summary>Appelle <paramref name="call"/> sur chaque mod actif, en isolant les exceptions.</summary>
        internal static void ForEachEnabled(Action<DiceMod> call, string what)
        {
            for (int i = 0; i < _mods.Count; i++)
            {
                var lm = _mods[i];
                if (!lm.IsEnabled) continue;
                try { call(lm.Instance); }
                catch (Exception e)
                {
                    lm.Error = $"{what} : {e.Message}";
                    Log.LogError($"[{lm.Info.Name}] {what} : {e}");
                }
            }
        }

        private static Assembly ResolveFromModDirs(object sender, ResolveEventArgs args)
        {
            var name = new AssemblyName(args.Name).Name + ".dll";
            foreach (var dir in _searchDirs)
            {
                var p = Path.Combine(dir, name);
                if (File.Exists(p)) return Assembly.LoadFrom(p);
            }
            return null;
        }
    }
}
