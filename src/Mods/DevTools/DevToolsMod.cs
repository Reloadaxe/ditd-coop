using System;
using System.Linq;
using BepInEx.Configuration;
using DiceLoader;

namespace DevTools
{
    /// <summary>
    /// Panneau de triche et de debug : PV, or, énergie, reliques, ennemis, mode dev caché du jeu.
    /// Il sert d'exemple pour l'API <see cref="Game"/> et pour OnSettingsGUI.
    /// </summary>
    [DiceModInfo("dicemodding.devtools", "Dev Tools", "1.0.0",
        Author = "DiceModding", Description = "Triches et outils de debug (menu F1 > Options).")]
    public class DevToolsMod : DiceMod
    {
        private ConfigEntry<bool> _godMode;
        private ConfigEntry<bool> _infiniteEnergy;
        private string _relicSearch = "";
        private RelicData.Title[] _allRelics;

        public override void OnLoad()
        {
            _godMode = Config.Bind("Cheats", "GodMode", false, "PV remis au max à chaque tour et après chaque coup");
            _infiniteEnergy = Config.Bind("Cheats", "InfiniteEnergy", false, "Énergie remise à 99 à chaque tour");
            _allRelics = Enum.GetValues(typeof(RelicData.Title)).Cast<RelicData.Title>()
                             .Where(t => t != RelicData.Title.Empty).ToArray();
        }

        public override void OnTurnStart(BattleSystem battle, int turn)
        {
            if (_godMode.Value) Game.HealFull();
            if (_infiniteEnergy.Value) Game.Energy = 99;
        }

        public override void OnUpdate()
        {
            // God mode : remet les PV au max dès qu'ils baissent pendant un combat
            if (_godMode.Value && Game.InBattle && Game.Health > 0 && Game.Health < Game.MaxHealth)
                Game.HealFull();
        }

        public override void OnSettingsGUI()
        {
            UI.Header("Bascules");
            _godMode.Value = UI.Toggle(_godMode.Value, "God mode");
            _infiniteEnergy.Value = UI.Toggle(_infiniteEnergy.Value, "Énergie infinie");
            Game.DevMode = UI.Toggle(Game.DevMode, "Mode développeur caché du jeu (GameManager.DevModeActive)");
            UI.Space();

            if (!Game.InRun) { UI.Label("Lance une run pour accéder aux autres outils."); return; }

            UI.Header("Joueur");
            UI.Label($"PV {Game.Health}/{Game.MaxHealth}   Bloc {Game.Block}   Or {Game.Gold}   Énergie {Game.Energy}");
            UI.BeginRow();
            if (UI.Button("Soin complet")) Game.HealFull();
            if (UI.Button("PV max +10")) Game.SetMaxHealth(Game.MaxHealth + 10);
            if (UI.Button("Bloc +20")) Game.SetBlock(Game.Block + 20);
            UI.EndRow();
            UI.BeginRow();
            if (UI.Button("Or +100")) Game.AddGold(100);
            if (UI.Button("Or +1000")) Game.AddGold(1000);
            if (UI.Button("Énergie +3")) Game.Energy += 3;
            UI.EndRow();
            UI.Space();

            if (Game.InBattle)
            {
                UI.Header("Combat");
                var enemies = Game.Enemies;
                foreach (var e in enemies)
                    UI.Label($" - {Game.Safe(() => e.EnemyName.ToString())} : {e.CurrentHealth}/{e.MaxHealth} PV, bloc {e.CurrentBlock}");
                UI.BeginRow();
                if (UI.Button("Tuer tous les ennemis")) Game.KillAllEnemies();
                if (UI.Button("Forcer la fin du combat")) Game.ForceFinishBattle();
                UI.EndRow();
                UI.Space();
            }

            UI.Header($"Reliques ({Game.RelicTitles.Count})");
            UI.Label(string.Join(", ", Game.RelicTitles));
            UI.BeginRow();
            UI.Label("Recherche :");
            _relicSearch = UI.Text(_relicSearch);
            UI.EndRow();
            if (!string.IsNullOrWhiteSpace(_relicSearch))
            {
                foreach (var t in _allRelics.Where(r => r.ToString().IndexOf(_relicSearch.Trim(), StringComparison.OrdinalIgnoreCase) >= 0).Take(12))
                {
                    UI.BeginRow();
                    UI.Label(t.ToString());
                    if (Game.HasRelic(t)) { if (UI.Button("Retirer", 80)) Game.RemoveRelic(t); }
                    else if (UI.Button("Ajouter", 80)) Game.AddRelic(t);
                    UI.EndRow();
                }
            }
            UI.Space();

            UI.Header($"Deck ({Game.Deck.Count} dés)");
            foreach (var d in Game.Deck.Take(24))
            {
                var types = new System.Collections.Generic.List<string>();
                foreach (var t in d.types) types.Add(t.ToString());
                UI.Label($" - {d.diceDataName}  [{string.Join("/", types)}]  tier {d.tier}, moy. {d.FormattedAverageValue}");
            }

            UI.Space();
            if (UI.Button("Sauvegarder la run maintenant")) Game.SaveRun();
        }
    }
}
