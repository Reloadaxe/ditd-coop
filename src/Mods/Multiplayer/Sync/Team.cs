using System;
using System.Collections.Generic;
using System.Linq;
using DiceLoader;
using DiceMultiplayer.Net;

namespace DiceMultiplayer.Sync
{
    /// <summary>
    /// Lobby et composition de l'équipe.
    ///
    /// 1. Chaque joueur choisit librement son personnage, son skin, son compagnon et ses reliques sur son propre écran,
    ///    puis clique sur « Commencer » : son choix part au réseau, il est « prêt ».
    /// 2. Quand tout le monde est prêt, l'hôte lance la run de l'ÉQUIPE, identique chez tous :
    ///    héros porteur = personnage de l'hôte (le jeu ne gère qu'un héros logique), difficulté et compagnon de l'hôte,
    ///    reliques de départ = réunion des reliques de tous.
    /// 3. À l'arrivée sur la carte, le deck de départ du personnage de chaque autre joueur est ajouté au deck commun,
    ///    dans le même ordre chez tous. Ces dés appartiennent au joueur concerné.
    /// </summary>
    public static class Team
    {
        /// <summary>Choix de chaque joueur (id → choix).</summary>
        public static readonly Dictionary<int, RunData> Picks = new Dictionary<int, RunData>();
        /// <summary>Choix figés au lancement de la run (même liste et même ordre chez tous).</summary>
        public static List<PlayerPick> RunPicks = new List<PlayerPick>();
        /// <summary>uid du dé → id du joueur propriétaire.</summary>
        public static readonly Dictionary<int, int> DiceOwners = new Dictionary<int, int>();

        private static bool _decksMerged;

        public static void Reset()
        {
            Picks.Clear();
            RunPicks = new List<PlayerPick>();
            DiceOwners.Clear();
            _decksMerged = false;
        }

        public static RunData FromNewRunData(RunInfo.NewRunData d) => new RunData
        {
            Character = (int)d.playerName,
            Skin = (int)d.playerSkin,
            Companion = (int)d.companionName,
            Difficulty = (int)d.chosenDifficulty,
            Challenges = d.challengesEnabled,
            Relics = d.initialRelics == null ? Array.Empty<int>() : d.initialRelics.Select(r => (int)r).ToArray(),
        };

        public static RunInfo.NewRunData ToNewRunData(RunData r)
        {
            var relics = new Il2CppSystem.Collections.Generic.List<RelicData.Title>();
            foreach (var x in r.Relics ?? Array.Empty<int>()) relics.Add((RelicData.Title)x);
            var data = new RunInfo.NewRunData(
                (Player.CharacterName)r.Character, (PlayerSkinData.Name)r.Skin, (Companion.Name)r.Companion,
                relics, (Difficulty.Level)r.Difficulty, default(RunInfo.NewRunData.TutorialMode));
            data.challengesEnabled = r.Challenges;
            data.tutorialsActive = false;
            return data;
        }

        public static string Describe(RunData r) =>
            r == null ? "…choisit" : $"{(Player.CharacterName)r.Character}";

        // ───────────── Lobby ─────────────

        /// <summary>
        /// Clic local sur « Commencer » (préfixe de GameManager.StartRun). En coop, on ne laisse jamais passer
        /// l'appel d'origine : l'hôte relance lui-même la run d'équipe quand tout le monde est prêt.
        /// </summary>
        /// <summary>ActiveCustomRun passé par l'écran de sélection. Le jeu en a besoin : avec null, Attack plante (mode Endless).</summary>
        public static ActiveCustomRun LocalCustomRun;

        public static bool OnLocalStartRun(RunInfo.NewRunData data, ActiveCustomRun customRun)
        {
            if (!Coop.Active || Coop.Replaying) return true;
            LocalCustomRun = customRun;
            var pick = FromNewRunData(data);
            Picks[Coop.LocalId] = pick;
            Coop.Send(new Msg { t = T.Pick, run = pick, flag = true });
            Coop.AddLog($"Tu es prêt avec {Describe(pick)} (customRun {(customRun == null ? "null" : "ok")}).");
            if (Coop.IsHost) TryStartTeamRun();
            else Coop.Toast("Prêt ! En attente des autres joueurs et de l'hôte…");
            return false;
        }

        public static void OnRemotePick(Msg m)
        {
            if (m.run == null) return;
            Picks[m.id] = m.run;
            Coop.AddLog($"{Coop.NameOf(m.id)} est prêt avec {Describe(m.run)}.");
            if (Coop.IsHost) TryStartTeamRun();
        }

        public static IEnumerable<int> Waiting => Coop.Net?.Players.Where(p => !Picks.ContainsKey(p.Id)).Select(p => p.Id) ?? Enumerable.Empty<int>();

        private static void TryStartTeamRun()
        {
            var waiting = Waiting.ToList();
            if (waiting.Count > 0)
            {
                Coop.Toast("En attente de " + string.Join(", ", waiting.Select(Coop.NameOf)));
                return;
            }
            if (!Picks.ContainsKey(Coop.LocalId)) { Coop.Toast("Choisis ton personnage puis clique sur Commencer"); return; }

            RunPicks = Coop.Net.Players.OrderBy(p => p.Id).Select(p => new PlayerPick { Id = p.Id, Run = Picks[p.Id] }).ToList();
            var team = BuildTeamRun(RunPicks);
            Coop.AddLog("Tout le monde est prêt : lancement de la run d'équipe !");
            Patches.HostPendingRun = team;        // envoyé avec la seed dans SeedSystem.StartRun
            // Frame suivante : on peut être dans le préfixe du clic « Commencer », et la suite du code du jeu
            // (abandon de la sauvegarde en cours…) remettrait RunInfo.CurrentRun à null après notre lancement.
            Game.Schedule(() => Coop.StartRunReplaying(ToNewRunData(team)), 0.1f);
        }

        /// <summary>Run d'équipe : base de l'hôte (premier de la liste) + réunion des reliques de départ.</summary>
        public static RunData BuildTeamRun(List<PlayerPick> picks)
        {
            var host = picks[0].Run;
            return new RunData
            {
                Character = host.Character,
                Skin = host.Skin,
                Companion = host.Companion,
                Difficulty = host.Difficulty,
                Challenges = host.Challenges,
                Relics = picks.SelectMany(p => p.Run.Relics ?? Array.Empty<int>()).Distinct().ToArray(),
            };
        }

        /// <summary>Client : réception de la run d'équipe.</summary>
        public static void OnTeamRunReceived(Msg m)
        {
            RunPicks = m.picks ?? new List<PlayerPick>();
            DiceOwners.Clear();
            _decksMerged = false;
        }

        public static void OnHostTeamRunStarted()
        {
            DiceOwners.Clear();
            _decksMerged = false;
        }

        // ───────────── Decks ─────────────

        /// <summary>
        /// Appelé à la première arrivée sur la carte. Le deck de l'hôte est déjà chargé : on note ses propriétaires,
        /// puis on ajoute dans l'ordre les decks de départ des autres personnages. C'est déterministe chez tous.
        /// </summary>
        public static void MergeDecksIfNeeded()
        {
            if (_decksMerged || !Coop.Active || RunPicks.Count == 0) return;
            var dm = Game.Dice;
            var run = Game.Run;
            if (dm == null || run == null || !dm.IsCurrentRunDeckLoaded) return;
            _decksMerged = true;

            int hostId = RunPicks[0].Id;
            foreach (var d in Game.Deck) DiceOwners[d.uid] = hostId;

            var difficulty = run.DifficultyLevel;
            foreach (var p in RunPicks.Skip(1))
            {
                try
                {
                    var pd = ToNewRunData(p.Run).PlayerData;
                    var deck = pd?.PlayerDeck(difficulty);
                    var starting = deck?.GetStartingDiceBasedOnContent();
                    if (starting == null) continue;
                    int added = 0;
                    foreach (var sd in starting)
                    {
                        var ad = ActiveDice.FromStarting(sd);
                        if (ad == null) continue;
                        ad.uid = run.ProvideNewDiceUID();
                        dm.CurrentRunDeck.AddDice(ad);
                        DiceOwners[ad.uid] = p.Id;
                        added++;
                    }
                    Coop.AddLog($"Deck de {Coop.NameOf(p.Id)} ({(Player.CharacterName)p.Run.Character}) ajouté : {added} dés.");
                }
                catch (Exception e) { Coop.AddLog($"Ajout du deck de {Coop.NameOf(p.Id)} impossible : {e.Message}"); }
            }
        }

        /// <summary>Dé ajouté au deck en cours de run (récompense, événement…) : il va au meneur du moment.</summary>
        public static void OnDiceAddedToDeck(ActiveDice d)
        {
            if (!Coop.Active || d == null || !_decksMerged) return;
            if (!DiceOwners.ContainsKey(d.uid)) DiceOwners[d.uid] = Coop.LeaderId;
        }

        public static int? OwnerOf(int uid) => DiceOwners.TryGetValue(uid, out var o) ? o : (int?)null;
    }
}
