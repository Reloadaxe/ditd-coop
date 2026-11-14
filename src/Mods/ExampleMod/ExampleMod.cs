using BepInEx.Configuration;
using DiceLoader;
using HarmonyLib;

namespace ExampleMod
{
    /// <summary>
    /// Mod d'exemple commenté, à copier pour démarrer (voir new-mod.ps1). Il montre :
    ///  1. la config (ConfigEntry, éditable dans le menu F1 et dans BepInEx/config/mods/*.cfg) ;
    ///  2. les événements du jeu (OnRunStart, OnTurnStart, OnEnemyDeath...) ;
    ///  3. un patch Harmony sur une méthode du jeu (taille max de la main) ;
    ///  4. une page d'options dans le menu DiceLoader.
    /// </summary>
    [DiceModInfo("dicemodding.example", "Example Mod", "1.0.0",
        Author = "DiceModding", Description = "Or de départ, énergie bonus, main plus grande, or par ennemi tué.")]
    public class ExampleMod : DiceMod
    {
        internal static ExampleMod Instance;

        public ConfigEntry<int> StartingGold;
        public ConfigEntry<int> BonusEnergyPerTurn;
        public ConfigEntry<int> ExtraHandSize;
        public ConfigEntry<int> GoldPerKill;

        public override void OnLoad()
        {
            Instance = this;
            StartingGold       = Config.Bind("Run",    "StartingGold",       50, "Or ajouté au début de chaque nouvelle run");
            BonusEnergyPerTurn = Config.Bind("Combat", "BonusEnergyPerTurn",  1, "Énergie supplémentaire à chaque début de tour");
            ExtraHandSize      = Config.Bind("Combat", "ExtraHandSize",       2, "Emplacements de main supplémentaires");
            GoldPerKill        = Config.Bind("Combat", "GoldPerKill",         5, "Or gagné à chaque ennemi tué");
        }

        public override void OnEnable()
        {
            // Applique tous les [HarmonyPatch] de cette DLL. Ils sont retirés automatiquement dans OnDisable.
            Harmony.PatchAll(typeof(ExampleMod).Assembly);
            Logger.LogInfo("Example Mod activé !");
        }

        public override void OnDisable() => Logger.LogInfo("Example Mod désactivé.");

        public override void OnGameStateChanged(GameManager.GameState previous, GameManager.GameState current)
            => Logger.LogDebug($"État : {previous} -> {current}");

        public override void OnRunStart(CurrentRunData run, bool loaded)
        {
            Logger.LogInfo($"Run {(loaded ? "reprise" : "lancée")} avec {run.PlayerName}, difficulté {run.DifficultyLevel}");
            if (!loaded && StartingGold.Value != 0)
                Game.Schedule(() => Game.AddGold(StartingGold.Value), 1f); // laisse le temps à l'UI de se créer
        }

        public override void OnTurnStart(BattleSystem battle, int turn)
        {
            if (BonusEnergyPerTurn.Value != 0)
                Game.Schedule(() => Game.Energy += BonusEnergyPerTurn.Value); // après le reset d'énergie du jeu
        }

        public override void OnEnemyDeath(Enemy enemy)
        {
            if (GoldPerKill.Value > 0) Game.AddGold(GoldPerKill.Value);
        }

        public override void OnBattleEnd(BattleSystem battle, BattleSystem.EndState result)
            => Logger.LogInfo($"Combat terminé : {result} en {battle.CurrentTurnCount} tour(s)");

        public override void OnSettingsGUI()
        {
            StartingGold.Value       = UI.IntStepper("Or de départ", StartingGold.Value, 25, 0, 9999);
            BonusEnergyPerTurn.Value = UI.IntStepper("Énergie bonus / tour", BonusEnergyPerTurn.Value, 1, 0, 10);
            ExtraHandSize.Value      = UI.IntStepper("Taille de main en plus", ExtraHandSize.Value, 1, 0, 10);
            GoldPerKill.Value        = UI.IntStepper("Or par ennemi tué", GoldPerKill.Value, 1, 0, 100);
        }
    }

    /// <summary>
    /// Patch Harmony classique : DiceManager.MaxDiceInHand() renvoie la taille max de la main.
    /// Le postfix modifie la valeur de retour (__result).
    /// </summary>
    [HarmonyPatch(typeof(DiceManager), nameof(DiceManager.MaxDiceInHand))]
    internal static class MaxHandPatch
    {
        private static void Postfix(ref int __result)
        {
            if (ExampleMod.Instance != null) __result += ExampleMod.Instance.ExtraHandSize.Value;
        }
    }
}
