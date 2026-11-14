using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DiceLoader;
using DiceMultiplayer.Net;
using UnityEngine;

namespace DiceMultiplayer.Sync
{
    /// <summary>
    /// Test automatique de la coop (argument « -ditd-autotest host » ou « -ditd-autotest join », avec -ditd-player).
    /// Chaque instance : crée / rejoint la partie en local, lance une run (Cinder), puis joue toute seule :
    /// ennemis tués d'office, récompenses passées, vote automatique (plus petite case). Elle journalise chaque changement
    /// d'état (« [auto] … »), la carte complète à chaque passage sur la carte (pour comparer les joueurs), détecte les
    /// blocages (état figé) et prend des captures régulières dans Mods/Multiplayer/autotest/. Fin après MaxSteps votes
    /// ou MaxMinutes, puis fermeture du jeu.
    /// </summary>
    public static class AutoTest
    {
        public static readonly string Role = Arg("-ditd-autotest");
        public static bool Enabled => Role != null;
        private const int MaxSteps = 8;
        private const float MaxMinutes = 8f;

        private static string Arg(string name)
        {
            var a = Environment.GetCommandLineArgs();
            int i = Array.FindIndex(a, x => string.Equals(x, name, StringComparison.OrdinalIgnoreCase));
            return i >= 0 && i + 1 < a.Length ? a[i + 1].ToLowerInvariant() : null;
        }

        private static float _start = -1, _nextShot, _lastChange, _actAt, _nextAct;
        private static bool _hosted, _joined, _picked, _finished;
        private static string _lastState, _dir, _lastMapDump;
        private static int _shots, _votes;

        private static void Log(string s) => Coop.AddLog("[auto] " + s);

        public static void Update()
        {
            if (!Enabled || _finished) return;
            float now = Time.unscaledTime;
            if (_start < 0)
            {
                _start = now;
                _dir = Path.Combine(Loader.ModsDirectory, "Multiplayer", "autotest");
                Directory.CreateDirectory(_dir);
                Log($"démarrage ({Role})");
            }

            // État courant et détection de blocage
            var map = Game.Map;
            string state = $"{Game.State}/{(map != null ? Game.Safe(() => map.CurrentMapState).ToString() : "-")}/{Game.TurnState}";
            if (state != _lastState)
            {
                Log($"état {state}");
                _lastState = state;
                _lastChange = now;
                _actAt = now + 2f;   // laisse le jeu se poser avant d'agir
            }
            else if (now - _lastChange > 30f)
            {
                Log($"BLOQUÉ depuis 30 s dans {state}");
                Shot("stuck");
                _lastChange = now;
            }

            if (now >= _nextShot)
            {
                _nextShot = now + 15f;
                Shot("t");
                // Arrêt demandé de l'extérieur (fichier autotest/stop.txt) : fin propre, sans tuer le processus
                if (File.Exists(Path.Combine(_dir, "stop.txt"))) { Log("arrêt demandé"); Finish(); return; }
            }
            if (now - _start > MaxMinutes * 60f || _votes >= MaxSteps) { Finish(); return; }
            if (now < _actAt || now < _nextAct) return;
            _nextAct = now + 1f;

            try { Act(map); } catch (Exception e) { Log("erreur : " + e.Message); }
        }

        private static void Act(Map map)
        {
            var st = Game.State;
            var M = MultiplayerMod.Instance;

            // Lobby
            if (st == GameManager.GameState.TitleScreen)
            {
                if (Role == "host" && !_hosted) { _hosted = true; M.HostGame(); Log("partie créée"); return; }
                if (Role == "join" && !_joined && Time.unscaledTime - _start > 8f) { _joined = true; M.JoinGame(); Log("connexion"); return; }
                if (Coop.Active && Coop.Net.Players.Count >= 2 && !_picked)
                {
                    _picked = true;
                    var pick = new RunData { Character = 0, Skin = 10, Companion = 0, Difficulty = 0, Relics = Array.Empty<int>() };
                    Team.OnLocalStartRun(Team.ToNewRunData(pick), null);
                    Log("personnage choisi (Cinder)");
                }
                return;
            }

            // Carte (elle s'affiche aussi pendant la fin d'étage) : vidage de la carte, puis vote automatique
            if (map != null && Game.Safe(() => map.CurrentMapState) == Map.MapState.SelectTile)
            {
                DumpMap(map);
                var v = Coop.AutoVote();
                if (v != null) { _votes++; Log($"vote {_votes} : {v}"); }
                if (st == GameManager.GameState.Map || v != null) return;
            }

            // Combat : ennemis tués, puis récompenses passées
            if (Game.InBattle)
            {
                var ts = Game.TurnState;
                if (ts == BattleSystem.TurnState.SelectDice)
                {
                    bool killed = false;
                    foreach (var e in Game.Enemies.ToList())
                        if (e != null && !e.IsDyingOrDead) { MEC.Timing.RunCoroutine(e._DamageForceKill()); killed = true; Log($"ennemi tué ({e.name})"); }
                    // Puis fin de tour (« prêt ») : le combat se termine quand tout le monde a validé
                    if (!killed && !Coop.ReadyPlayers.Contains(Coop.LocalId) && Time.unscaledTime >= _nextFinish)
                    {
                        _nextFinish = Time.unscaledTime + 6f;
                        var ui = UnityEngine.Object.FindObjectOfType<FinishTurnUI>();
                        if (ui != null) { ui.OnClickFinishTurnButton(); Log("fin de tour"); }
                    }
                    return;
                }
                if (ts == BattleSystem.TurnState.SelectReward) SkipRewards();
                return;
            }
            SkipRewards();
        }

        private static float _nextFinish;

        private static void SkipRewards()
        {
            var rewards = UnityEngine.Object.FindObjectOfType<RewardsUI>();
            if (rewards == null || !rewards.isActiveAndEnabled) return;
            bool any = Game.Safe(() => rewards.CanSelectReward());
            if (!any && Game.TurnState != BattleSystem.TurnState.SelectReward) return;
            // Le vrai bouton « Passer » du jeu (EditorOnPressSkip n'agit pas toujours)
            try
            {
                var sel = rewards.SkipRewardSelectable;
                var go = sel != null ? sel.gameObject : null;
                var btn = go != null ? go.GetComponentInChildren<UnityEngine.UI.Button>(true) ?? go.GetComponentInParent<UnityEngine.UI.Button>() : null;
                if (btn != null && btn.gameObject.activeInHierarchy && btn.interactable) { btn.onClick.Invoke(); Log("récompenses passées (bouton)"); return; }
            }
            catch (Exception e) { Log("bouton Passer : " + e.Message); }
            try { rewards.EditorOnPressSkip(); } catch { }
        }

        private static void DumpMap(Map map)
        {
            var tiles = new List<string>();
            foreach (var t in UnityEngine.Object.FindObjectsOfType<VisualMapTile>())
            {
                try
                {
                    var a = t.ActiveTile;
                    if (a == null) continue;
                    var c = a.CoordVector;
                    tiles.Add($"{c.x},{c.y}={a.TileType}");
                }
                catch { }
            }
            tiles.Sort(StringComparer.Ordinal);
            var dump = string.Join(" ", tiles);
            if (dump == _lastMapDump) return;
            _lastMapDump = dump;
            var here = Game.Safe(() => map.CurrentPlayerTile != null ? map.CurrentPlayerTile.ActiveTile.CoordVector.ToString() : "?");
            Log($"carte ({tiles.Count} cases, joueur en {here}, empreinte {dump.GetHashCode():X8}) : {dump}");
        }

        private static void Shot(string tag)
        {
            try
            {
                var name = Coop.Net != null ? Coop.NameOf(Coop.LocalId) : Role;
                ScreenCapture.CaptureScreenshot(Path.Combine(_dir, $"{name}_{tag}_{_shots++ % 40:00}.png"));
            }
            catch { }
        }

        private static void Finish()
        {
            _finished = true;
            Log($"fin du test ({_votes} votes, {(Time.unscaledTime - _start):0} s)");
            Shot("fin");
            File.WriteAllText(Path.Combine(_dir, $"{Role}_done.txt"), "ok");
            Game.Schedule(() => Application.Quit(), 2f);
        }
    }
}
