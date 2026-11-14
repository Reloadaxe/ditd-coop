using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DiceLoader
{
    /// <summary>
    /// Façade haut niveau sur les systèmes du jeu (rétro-ingénierie de Die in the Dungeon 5.1.0).
    /// Chaque accès est sûr : il renvoie null ou une valeur par défaut si le système n'existe pas encore,
    /// par exemple sur l'écran titre.
    /// Pour aller plus loin, utilise directement les classes du jeu (voir SDK/api/Classes.md).
    /// </summary>
    public static class Game
    {
        // ───────────── Singletons et systèmes ─────────────

        /// <summary>Gestionnaire global : machine à états, chargement de scènes, runs.</summary>
        public static GameManager Manager => Safe(() => Singleton<GameManager>.IsInitialized ? Singleton<GameManager>.Instance : null);
        /// <summary>Système de floor : contient BattleSystem, EventController, LootController, ShopController et le joueur.</summary>
        public static FloorSystem Floor => Safe(() => Singleton<FloorSystem>.IsInitialized ? Singleton<FloorSystem>.Instance : null);
        /// <summary>Dés : pioche, main, défausse, exil, énergie, deck de la run.</summary>
        public static DiceManager Dice => Safe(() => Singleton<DiceManager>.IsInitialized ? Singleton<DiceManager>.Instance : null);
        /// <summary>Reliques et malédictions du joueur.</summary>
        public static Relics Relics => Safe(() => Singleton<Relics>.IsInitialized ? Singleton<Relics>.Instance : null);
        /// <summary>Runes (runestones).</summary>
        public static Runestones Runestones => Safe(() => Singleton<Runestones>.IsInitialized ? Singleton<Runestones>.Instance : null);
        /// <summary>Monnaie (affichage et animations).</summary>
        public static CurrencySystem Currency => Safe(() => Singleton<CurrencySystem>.IsInitialized ? Singleton<CurrencySystem>.Instance : null);
        /// <summary>Carte de la run.</summary>
        public static Map Map => Safe(() => Singleton<Map>.IsInitialized ? Singleton<Map>.Instance : null);
        /// <summary>Système de combat courant.</summary>
        public static BattleSystem Battle => Safe(() => Floor?.battle);
        /// <summary>Joueur courant (Cinder, Lisver, Nera, Mango).</summary>
        public static Player Player => Safe(() => Floor?.CurrentPlayer);
        /// <summary>Données de la run en cours : seed, stats, reliques choisies, or, difficulté...</summary>
        public static CurrentRunData Run => Safe(() => RunInfo.CurrentRun);

        // ───────────── État ─────────────

        public static GameManager.GameState State => Safe(() => Manager != null ? Manager.GetCurrentGameState() : GameManager.GameState.NONE);
        public static bool InRun => Run != null && State > GameManager.GameState.TitleScreen;
        public static bool InBattle => State == GameManager.GameState.BattleFloor && Battle != null;
        public static BattleSystem.TurnState TurnState => Safe(() => Battle != null ? Battle.CurrentTurnState : BattleSystem.TurnState.None);
        public static int Turn => Safe(() => Battle != null ? Battle.CurrentTurnCount : 0);
        /// <summary>Version du jeu, ex. "5.1.0".</summary>
        public static string Version => Safe(() => GameManager.CompleteUpdateNumber) ?? "?";

        /// <summary>Mode développeur caché du jeu : raccourcis de debug intégrés au GameManager.</summary>
        public static bool DevMode
        {
            get => Safe(() => Manager != null && Manager.DevModeActive);
            set { var m = Manager; if (m != null) m.DevModeActive = value; }
        }

        // ───────────── Joueur ─────────────

        public static int Health => Safe(() => Player != null ? Player.CurrentHealth : 0);
        public static int MaxHealth => Safe(() => Player != null ? Player.MaxHealth : 0);
        public static int Block => Safe(() => Player != null ? Player.CurrentBlock : 0);

        public static void SetHealth(int hp)
        {
            var p = Player;
            if (p == null) return;
            p.ChangeCurrentHealth(Math.Clamp(hp, 0, p.MaxHealth));
            p.ForceHealthRefresh();
        }

        public static void Heal(int amount) => SetHealth(Health + amount);
        public static void HealFull() => SetHealth(MaxHealth);

        public static void SetMaxHealth(int max)
        {
            var p = Player;
            if (p == null) return;
            p.ChangeMaxHealth(Math.Max(1, max));
            p.ForceHealthRefresh();
        }

        public static void SetBlock(int block) => Player?.ChangeBlockValue(Math.Max(0, block));

        // ───────────── Or ─────────────

        public static int Gold => Safe(() => Run != null ? Run.CurrencyAmount : 0);

        /// <summary>
        /// Ajoute ou retire de l'or. Passe par CurrencySystem (avec l'animation de l'UI) quand c'est possible,
        /// sinon modifie directement CurrentRunData.
        /// </summary>
        public static void AddGold(int amount)
        {
            if (amount == 0 || Run == null) return;
            var cs = Currency;
            if (cs != null)
            {
                try
                {
                    // Signatures du jeu : AddCurrencyValue(in int, DOTweenPunchData, in object, ActiveCurrency)
                    //                     RemoveCurrencyValue(in int, in DOTweenPunchData, in object)
                    // Elles sont appelées par réflexion pour ne pas dépendre de la façon dont l'interop expose « in ».
                    var name = amount > 0 ? "AddCurrencyValue" : "RemoveCurrencyValue";
                    var m = typeof(CurrencySystem).GetMethods(BindingFlags.Public | BindingFlags.Instance)
                        .FirstOrDefault(x => x.Name == name);
                    if (m != null)
                    {
                        var ps = m.GetParameters();
                        var args = new object[ps.Length];
                        args[0] = Math.Abs(amount);
                        if (ps.Length > 1) args[1] = cs.defaultPunchData;
                        if (ps.Length > 2) args[2] = cs;
                        m.Invoke(cs, args);
                        return;
                    }
                }
                catch (Exception e) { Loader.Log.LogDebug($"AddGold via CurrencySystem impossible, repli : {e.Message}"); }
            }
            Run.ChangeCurrencyAmount(Math.Clamp(Gold + amount, CurrencySystem.MinCurrency, CurrencySystem.MaxCurrency));
        }

        // ───────────── Énergie / dés ─────────────

        public static int Energy
        {
            get => Safe(() => Dice != null ? Dice.CurrentEnergy : 0);
            set { var d = Dice; if (d != null) d.CurrentEnergy = value; }
        }

        /// <summary>Nombre de dés piochés par tour (valeur de base du DiceManager).</summary>
        public static int DiceDrawPerTurn
        {
            get => Safe(() => Dice != null ? Dice.diceDrawPerTurn : 0);
            set { var d = Dice; if (d != null) d.diceDrawPerTurn = value; }
        }

        /// <summary>Copie de la liste des dés du deck de la run.</summary>
        public static List<ActiveDice> Deck
        {
            get
            {
                var res = new List<ActiveDice>();
                var deck = Safe(() => Dice != null && Dice.IsCurrentRunDeckLoaded ? Dice.CurrentRunDeck : null);
                if (deck?.dice == null) return res;
                foreach (var d in deck.dice) res.Add(d);
                return res;
            }
        }

        // ───────────── Reliques ─────────────

        public static bool HasRelic(RelicData.Title title) => Safe(() => Relics != null && Relics.HaveThisRelic(title, false));

        public static List<RelicData.Title> RelicTitles
        {
            get
            {
                var res = new List<RelicData.Title>();
                var list = Safe(() => Relics?.CurrentRelics);
                if (list == null) return res;
                foreach (var relic in list)
                {
                    var data = relic?.RelicData;
                    if (data != null) res.Add(data.title);
                }
                return res;
            }
        }

        public static void AddRelic(RelicData.Title title)
        {
            var r = Relics;
            if (r == null) { Loader.Log.LogWarning("AddRelic : aucune run active."); return; }
            r.AddRelic(title, Entity.Side.Player, -1);
        }

        public static void RemoveRelic(RelicData.Title title) => Relics?.RemoveRelic(title);

        // ───────────── Ennemis ─────────────

        /// <summary>Ennemis vivants dans la scène.</summary>
        public static List<Enemy> Enemies
        {
            get
            {
                var res = new List<Enemy>();
                if (!InBattle) return res;
                var arr = Safe(() => UnityEngine.Object.FindObjectsOfType<Enemy>());
                if (arr == null) return res;
                foreach (var e in arr)
                    if (e != null && !e.IsDyingOrDead && !e.isDummy) res.Add(e);
                return res;
            }
        }

        /// <summary>Tue tous les ennemis (commande de debug intégrée au jeu).</summary>
        public static void KillAllEnemies() => Battle?.DebugKillAllEnemies();
        /// <summary>Termine le combat immédiatement (commande de debug intégrée au jeu).</summary>
        public static void ForceFinishBattle() => Battle?.DebugForceFinish();

        // ───────────── Sauvegarde ─────────────

        public static void SaveRun() => SaveDataManager.SaveActiveRun();
        public static void SaveGame() => SaveDataManager.SaveGameData();
        public static string SavePath => Safe(() => SaveDataManager.GetSavedDataFilePath);

        // ───────────── Utilitaires ─────────────

        /// <summary>Exécute une fonction en avalant les exceptions IL2CPP, par exemple quand un objet a été détruit.</summary>
        public static T Safe<T>(Func<T> f)
        {
            try { return f(); }
            catch { return default; }
        }

        /// <summary>Exécute une action à la frame suivante, ou après un délai en secondes, sur le thread Unity.</summary>
        public static void Schedule(Action action, float delaySeconds = 0f) => Scheduler.Add(action, delaySeconds);
    }
}
