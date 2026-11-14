using System;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

namespace DiceLoader
{
    /// <summary>
    /// Métadonnées obligatoires d'un mod. À placer sur la classe qui hérite de <see cref="DiceMod"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class DiceModInfoAttribute : Attribute
    {
        /// <summary>Identifiant unique, ex. "com.moi.monmod".</summary>
        public string Id { get; }
        public string Name { get; }
        public string Version { get; }
        public string Author { get; set; } = "Inconnu";
        public string Description { get; set; } = "";

        public DiceModInfoAttribute(string id, string name, string version)
        {
            Id = id;
            Name = name;
            Version = version;
        }
    }

    /// <summary>
    /// Déclare qu'un mod dépend d'un autre mod (chargé avant lui).
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class DiceModDependencyAttribute : Attribute
    {
        public string Id { get; }
        public bool Optional { get; set; }
        public DiceModDependencyAttribute(string id) { Id = id; }
    }

    /// <summary>
    /// Classe de base de tous les mods DiceLoader.
    /// Surcharge seulement les méthodes dont tu as besoin.
    /// Toutes les méthodes sont appelées sur le thread principal de Unity,
    /// et chaque appel est protégé : une exception dans un mod ne fait pas planter le jeu ni les autres mods.
    /// </summary>
    public abstract class DiceMod
    {
        public DiceModInfoAttribute Info { get; internal set; }
        /// <summary>Logger dédié (BepInEx/LogOutput.log + console).</summary>
        public ManualLogSource Logger { get; internal set; }
        /// <summary>Fichier de config dédié : BepInEx/config/mods/&lt;id&gt;.cfg</summary>
        public ConfigFile Config { get; internal set; }
        /// <summary>Instance Harmony dédiée (id = Info.Id). Défaite automatiquement quand le mod est désactivé.</summary>
        public Harmony Harmony { get; internal set; }
        /// <summary>Dossier contenant la DLL du mod (pour charger tes propres fichiers).</summary>
        public string Directory { get; internal set; }
        public bool Enabled { get; internal set; }

        // ───────────── Cycle de vie du mod ─────────────

        /// <summary>Appelé une seule fois après le chargement de la DLL (même si le mod est désactivé). Déclare tes ConfigEntry ici.</summary>
        public virtual void OnLoad() { }
        /// <summary>Appelé quand le mod est activé (au démarrage s'il est actif, ou via le menu F1). Applique tes patchs Harmony ici.</summary>
        public virtual void OnEnable() { }
        /// <summary>Appelé quand le mod est désactivé via le menu. Les patchs de <see cref="Harmony"/> sont retirés automatiquement après.</summary>
        public virtual void OnDisable() { }

        /// <summary>Chaque frame (MonoBehaviour.Update).</summary>
        public virtual void OnUpdate() { }
        /// <summary>Chaque appel IMGUI (MonoBehaviour.OnGUI). Pour dessiner ton propre overlay.</summary>
        public virtual void OnGUI() { }
        /// <summary>Contenu de l'onglet du mod dans le menu DiceLoader (F1). Utilise <see cref="UI"/>.</summary>
        public virtual void OnSettingsGUI() { }

        // ───────────── Événements du jeu ─────────────

        /// <summary>Changement d'état global de GameManager (TitleScreen, Map, BattleFloor, ShopFloor...).</summary>
        public virtual void OnGameStateChanged(GameManager.GameState previous, GameManager.GameState current) { }
        /// <summary>Début d'une run (nouvelle run : loaded = false ; reprise d'une sauvegarde : loaded = true).</summary>
        public virtual void OnRunStart(CurrentRunData run, bool loaded) { }
        /// <summary>Fin de run (victoire ou défaite).</summary>
        public virtual void OnRunEnd(CurrentRunData run, bool won) { }
        /// <summary>Début d'un « floor » (combat, événement, boutique, loot...).</summary>
        public virtual void OnFloorStart(FloorSystem floor) { }
        /// <summary>Fin d'un floor.</summary>
        public virtual void OnFloorEnd(FloorSystem floor, bool success) { }
        /// <summary>Le combat commence (état Intro de BattleSystem).</summary>
        public virtual void OnBattleStart(BattleSystem battle) { }
        /// <summary>Début d'un tour du joueur.</summary>
        public virtual void OnTurnStart(BattleSystem battle, int turn) { }
        /// <summary>Chaque changement d'état de la machine de tour de BattleSystem.</summary>
        public virtual void OnTurnStateChanged(BattleSystem battle, BattleSystem.TurnState state) { }
        /// <summary>Fin de combat (Win / Lose / ForceFinish).</summary>
        public virtual void OnBattleEnd(BattleSystem battle, BattleSystem.EndState result) { }
        /// <summary>Un ennemi meurt.</summary>
        public virtual void OnEnemyDeath(Enemy enemy) { }
        /// <summary>Le joueur meurt.</summary>
        public virtual void OnPlayerDeath(Player player) { }
        /// <summary>Une relique (ou malédiction) est ajoutée.</summary>
        public virtual void OnRelicAdded(ActiveRelic relic) { }
        /// <summary>Une relique est retirée.</summary>
        public virtual void OnRelicRemoved(ActiveRelic relic) { }
    }
}
