using System;
using System.Collections.Generic;
using System.Linq;
using DiceLoader;
using DiceMultiplayer.Net;

namespace DiceMultiplayer.Sync
{
    /// <summary>
    /// Lobby et lancement des runs.
    ///
    /// Modèle : CHAQUE joueur joue SA run, normalement : son personnage, son deck, sa main, son énergie,
    /// ses PV, ses reliques, son or, ses récompenses. Le jeu gère tout ça nativement chez lui.
    /// Ce qui est partagé : la seed (donc la carte et les combats), le chemin choisi par le meneur,
    /// les ennemis (dégâts et PV), le plateau (affichage des dés des autres) et la fin de tour.
    ///
    /// Lancement : chacun choisit son personnage et clique sur « Commencer » (choix envoyé, joueur prêt).
    /// Quand tout le monde est prêt, l'hôte tire la seed et envoie StartRun. Chacun lance alors SA run,
    /// avec SON choix et la seed commune.
    /// </summary>
    public static class Team
    {
        /// <summary>Choix de chaque joueur (id → choix).</summary>
        public static readonly Dictionary<int, RunData> Picks = new Dictionary<int, RunData>();
        /// <summary>Choix figés au lancement (même liste et même ordre chez tous).</summary>
        public static List<PlayerPick> RunPicks = new List<PlayerPick>();

        /// <summary>Choix local (personnage, skin, compagnon, reliques, difficulté) et paramètre custom du jeu.</summary>
        private static RunInfo.NewRunData _localData;
        private static ActiveCustomRun _localCustom;

        public static void Reset()
        {
            Picks.Clear();
            RunPicks = new List<PlayerPick>();
            _localData = null;
            _localCustom = null;
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
                relics, (Difficulty.Level)r.Difficulty, RunInfo.NewRunData.TutorialMode.ForceTutorialInactive);
            data.challengesEnabled = r.Challenges;
            data.tutorialsActive = false;
            return data;
        }

        public static string Describe(RunData r) => r == null ? "choisit..." : $"{(Player.CharacterName)r.Character}";

        public static RunData PickOf(int playerId) => RunPicks.FirstOrDefault(p => p.Id == playerId)?.Run;

        // ───────────── Lobby ─────────────

        /// <summary>Préfixe de GameManager.StartRun (clic « Commencer »). En coop, l'appel d'origine est toujours bloqué.</summary>
        public static bool OnLocalStartRun(RunInfo.NewRunData data, ActiveCustomRun customRun)
        {
            if (!Coop.Active || Coop.Replaying) return true;
            _localData = data;
            _localCustom = customRun;
            var pick = FromNewRunData(data);
            Picks[Coop.LocalId] = pick;
            Coop.Send(new Msg { t = T.Pick, run = pick, flag = true });
            Coop.AddLog($"Tu es prêt avec {Describe(pick)}.");
            if (Coop.IsHost) TryStart();
            else Coop.Toast("Prêt ! En attente des autres joueurs");
            return false;
        }

        public static void OnRemotePick(Msg m)
        {
            if (m.run == null) return;
            Picks[m.id] = m.run;
            Coop.AddLog($"{Coop.NameOf(m.id)} est prêt avec {Describe(m.run)}.");
            if (Coop.IsHost) TryStart();
        }

        public static IEnumerable<int> Waiting => Coop.Net?.Players.Where(p => !Picks.ContainsKey(p.Id)).Select(p => p.Id) ?? Enumerable.Empty<int>();

        private static void TryStart()
        {
            var waiting = Waiting.ToList();
            if (waiting.Count > 0) { Coop.Toast("En attente de " + string.Join(", ", waiting.Select(Coop.NameOf))); return; }
            if (!Picks.ContainsKey(Coop.LocalId)) { Coop.Toast("Choisis ton personnage puis clique sur Commencer"); return; }

            RunPicks = Coop.Net.Players.OrderBy(p => p.Id).Select(p => new PlayerPick { Id = p.Id, Run = Picks[p.Id] }).ToList();
            int seed = new Random().Next(1000, int.MaxValue);
            Coop.Send(new Msg { t = T.StartRun, seed = seed, picks = RunPicks });
            Coop.AddLog($"Tout le monde est prêt : lancement (seed {seed}).");
            StartOwnRun(seed);
        }

        /// <summary>Client : l'hôte a lancé la partie.</summary>
        public static void OnStartRun(Msg m)
        {
            RunPicks = m.picks ?? new List<PlayerPick>();
            Coop.AddLog($"Lancement par {Coop.NameOf(m.id)} (seed {m.seed}) : " +
                        string.Join(", ", RunPicks.Select(p => $"{Coop.NameOf(p.Id)} = {(Player.CharacterName)p.Run.Character}")));
            StartOwnRun(m.seed);
        }

        /// <summary>Chacun lance SA run, avec SON choix, sur la seed commune. Le lancement est différé d'une frame pour sortir du clic.</summary>
        private static void StartOwnRun(int seed)
        {
            var mine = PickOf(Coop.LocalId);
            // Données reconstruites (et non celles du clic) : le tutoriel est forcé inactif, sinon une première run le lance
            var data = mine != null ? ToNewRunData(mine) : _localData;
            if (data != null) data.tutorialsActive = false;
            if (data == null) { Coop.AddLog("Aucun personnage choisi localement : impossible de lancer la run."); return; }
            Coop.PendingSeed = seed;
            Lobby.OnRunStarted();
            Coop.OnRunStarted();
            var custom = _localCustom;
            Game.Schedule(() => Coop.StartRunReplaying(data, custom), 0.1f);
        }
    }
}
