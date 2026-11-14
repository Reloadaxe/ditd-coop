using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using UnityEngine;

namespace DiceLoader
{
    /// <summary>
    /// Point d'entrée : plugin BepInEx 6 (IL2CPP). BepInEx s'occupe de l'injection (Doorstop + CoreCLR)
    /// et de l'interop. DiceLoader ajoute par-dessus le système de mods, les événements du jeu,
    /// l'API <see cref="Game"/> et le menu en jeu.
    /// </summary>
    [BepInPlugin(Guid, "DiceLoader", Version)]
    public class Plugin : BasePlugin
    {
        public const string Guid = "dicemodding.diceloader";
        public const string Version = "1.0.0";

        internal static ConfigEntry<KeyCode> MenuKey;
        internal static ConfigEntry<bool> ShowWatermark;

        public override void Load()
        {
            MenuKey = Config.Bind("General", "MenuKey", KeyCode.F1, "Touche d'ouverture du menu DiceLoader");
            ShowWatermark = Config.Bind("General", "ShowWatermark", true, "Afficher « DiceLoader » en bas de l'écran");

            Loader.Init(Log, Config);
            Hooks.Apply(new Harmony(Guid));
            AddComponent<LoaderBehaviour>();   // enregistre le type dans IL2CPP et l'attache à un GameObject persistant
            Loader.LoadAll();

            Log.LogInfo($"DiceLoader {Version} prêt. Mods : {Loader.ModsDirectory}");
        }
    }
}
