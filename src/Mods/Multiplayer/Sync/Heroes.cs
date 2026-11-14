using System;
using System.Collections.Generic;
using System.Linq;
using DiceLoader;
using DiceMultiplayer.Net;
using UnityEngine;

namespace DiceMultiplayer.Sync
{
    /// <summary>
    /// Un vrai héros par joueur.
    ///
    /// Le héros de l'hôte est celui du jeu : FloorSystem.CurrentPlayer et BattleSystem.Player le désignent.
    /// Pour chaque autre joueur, on fait comme FloorSystem.SpawnPlayer :
    ///   GameData.GetPlayerData(personnage).prefab → Instantiate → GetComponent&lt;Player&gt; → LoadSkin → LoadEntityData.
    /// Ses données de vie (ActiveEntity : PV, PV max, bloc) appartiennent au mod et persistent d'un étage à l'autre.
    /// Tout est déterministe, donc identique chez tous les joueurs.
    ///
    /// Étape 1 : chaque héros est un vrai Player, propriétaire des dés de son joueur (bloc, soins et attaques lui reviennent).
    /// Étape 2 : chaque coup d'ennemi porté au héros de l'hôte est rejoué sur chaque autre héros debout.
    /// Étape 4 : à 0 PV, un héros supplémentaire est K.O. jusqu'à la fin du combat, puis revient à 1 PV en cas de victoire.
    /// </summary>
    public static class Heroes
    {
        public sealed class Hero
        {
            public int PlayerId;
            public PlayerPick Pick;
            public ActiveEntity Data;
            public Player Entity;
            public bool KO;
            public Vector3 Offset;
        }

        private static readonly Dictionary<int, Hero> _heroes = new Dictionary<int, Hero>();
        private static Player _host;
        private static float _nextCheck;

        public static IEnumerable<Hero> All => _heroes.Values;
        public static bool Enabled => Coop.Active && Team.RunPicks.Count > 1;

        // ───────────── Création des données (début de run) ─────────────

        /// <summary>Appelé au lancement de la run d'équipe, chez tous et dans le même ordre.</summary>
        public static void OnTeamRunStarted()
        {
            DespawnAll();
            _heroes.Clear();
        }

        private static Hero GetOrCreate(PlayerPick pick)
        {
            if (_heroes.TryGetValue(pick.Id, out var h)) return h;
            var pd = PlayerDataOf(pick);
            h = new Hero { PlayerId = pick.Id, Pick = pick, Data = new ActiveEntity(pd.data) };
            _heroes[pick.Id] = h;
            Coop.AddLog($"Héros de {Coop.NameOf(pick.Id)} créé : {(Player.CharacterName)pick.Run.Character}, {h.Data.CurrentHealth}/{h.Data.MaxHealth} PV.");
            return h;
        }

        private static GameData.PlayerData PlayerDataOf(PlayerPick pick) =>
            Game.Manager.ActiveGameData.GetPlayerData((Player.CharacterName)pick.Run.Character);

        // ───────────── Apparition en combat et en événement ─────────────

        public static void Update()
        {
            if (!Enabled || !Game.InRun) { DespawnAll(); return; }
            var host = Game.Player;
            bool scene = host != null && Game.Safe(() => host.gameObject.activeInHierarchy)
                         && (Game.State == GameManager.GameState.BattleFloor || Game.State == GameManager.GameState.EventFloor
                             || Game.State == GameManager.GameState.ShopFloor || Game.State == GameManager.GameState.LootFloor);
            if (!scene) { DespawnAll(); return; }

            // Chaque frame : chaque héros garde son décalage par rapport au héros de l'hôte (Enter() le ramènerait sur lui)
            foreach (var hh in _heroes.Values)
                if (hh.Entity != null) Game.Safe(() => { hh.Entity.transform.position = host.transform.position + hh.Offset; return 0; });

            if (Time.unscaledTime < _nextCheck) return;
            _nextCheck = Time.unscaledTime + 0.25f;

            if (_host == null || _host.Pointer != host.Pointer) { DespawnAll(); _host = host; }

            int k = 1;
            foreach (var pick in Team.RunPicks.Skip(1))
            {
                var h = GetOrCreate(pick);
                if (h.Entity == null) Spawn(h, host, k);
                else
                {
                    // Le Start() du prefab peut recharger les données de la run (celles de l'hôte) : on réimpose les siennes
                    var data = Game.Safe(() => h.Entity.ActiveData);
                    if (data == null || data.Pointer != h.Data.Pointer) Game.Safe(() => { h.Entity.LoadEntityData(h.Data); return 0; });
                }
                k++;
            }
        }

        private static void Spawn(Hero h, Player host, int index)
        {
            try
            {
                var pd = PlayerDataOf(h.Pick);
                if (pd.prefab == null) { Coop.AddLog($"Prefab introuvable pour {(Player.CharacterName)h.Pick.Run.Character}"); return; }

                // Décalage derrière le héros de l'hôte, réduit tant qu'il sortirait du champ de la caméra
                // (la largeur des sprites n'est pas fiable : un sprite d'effet élargit fortement les bounds)
                float face = Game.Safe(() => host.FaceDirection);
                if (Math.Abs(face) < 0.01f) face = 1f;
                float step = 2.2f;
                var cam = Camera.main;
                for (int tries = 0; tries < 8 && cam != null; tries++)
                {
                    var vp = cam.WorldToViewportPoint(host.transform.position + new Vector3(-face * step * index, 0, 0));
                    if (vp.x > 0.06f && vp.x < 0.94f) break;
                    step *= 0.75f;
                }
                h.Offset = new Vector3(-face * step * index, 0.15f * index, 0.01f * index);
                var pos = host.transform.position + h.Offset;

                var go = UnityEngine.Object.Instantiate(pd.prefab, pos, Quaternion.identity);
                go.name = $"DIM_Hero_{h.PlayerId}";
                go.transform.SetParent(host.transform.parent, true);
                go.transform.localScale = host.transform.localScale;

                var p = go.GetComponent<Player>();
                if (p == null) { Coop.AddLog("Le prefab du héros n'a pas de composant Player"); UnityEngine.Object.Destroy(go); return; }
                h.Entity = p;
                try { p.LoadSkin((PlayerSkinData.Name)h.Pick.Run.Skin); } catch (Exception e) { Coop.AddLog("Skin : " + e.Message); }
                p.LoadEntityData(h.Data);
                // Comme pour le héros du jeu : initialisation puis animation d'entrée (sans elle le héros reste invisible)
                try { p.Init(); } catch (Exception e) { Coop.AddLog("Init du héros : " + e.Message); }
                try { p.Enter(); } catch (Exception e) { Coop.AddLog("Enter du héros : " + e.Message); }
                p.LoadEntityData(h.Data);

                var rends = go.GetComponentsInChildren<SpriteRenderer>(true);
                MultiplayerMod.Instance.Logger.LogInfo($"[diag] Héros {h.PlayerId} : actif={go.activeInHierarchy} pos={go.transform.position} hôte={host.transform.position} " +
                    $"échelle={go.transform.lossyScale} sprites={rends.Length} visibles={rends.Count(r => r.enabled && r.sprite != null)} parent={go.transform.parent?.name}");
                Coop.AddLog($"Héros de {Coop.NameOf(h.PlayerId)} en jeu ({h.Data.CurrentHealth}/{h.Data.MaxHealth} PV{(h.KO ? ", K.O." : "")}).");
            }
            catch (Exception e)
            {
                Coop.AddLog($"Apparition du héros de {Coop.NameOf(h.PlayerId)} impossible : {e.Message}");
                MultiplayerMod.Instance.Logger.LogError(e);
            }
        }

        public static void DespawnAll()
        {
            foreach (var h in _heroes.Values)
            {
                if (h.Entity != null) { try { UnityEngine.Object.Destroy(h.Entity.gameObject); } catch { } }
                h.Entity = null;
            }
            _host = null;
        }

        // ───────────── Requêtes ─────────────

        public static Hero ByEntity(Entity e)
        {
            if (e == null) return null;
            foreach (var h in _heroes.Values) if (h.Entity != null && h.Entity.Pointer == e.Pointer) return h;
            return null;
        }

        public static bool IsExtraHero(Entity e) => ByEntity(e) != null;
        public static bool IsHostHero(Entity e) => e != null && Game.Player != null && e.Pointer == Game.Player.Pointer;

        /// <summary>Entité du héros d'un joueur : celui de l'hôte (héros du jeu) ou un héros supplémentaire.</summary>
        public static Player EntityOf(int playerId)
        {
            if (Team.RunPicks.Count > 0 && Team.RunPicks[0].Id == playerId) return Game.Player;
            return _heroes.TryGetValue(playerId, out var h) && !h.KO ? h.Entity : null;
        }

        // ───────────── Étape 2 : les ennemis frappent tous les héros ─────────────

        /// <summary>Postfix de Player._Damage : un coup d'ennemi au héros de l'hôte est rejoué sur chaque autre héros debout.</summary>
        public static void OnHostDamaged(Player victim, Hit hit)
        {
            if (!Enabled || hit == null || !IsHostHero(victim)) return;
            var attacker = Game.Safe(() => hit.Attacker);
            if (attacker == null || attacker.TryCast<Enemy>() == null) return;   // seuls les coups d'ennemis sont partagés

            foreach (var h in _heroes.Values)
            {
                if (h.KO || h.Entity == null || h.Entity.IsDyingOrDead) continue;
                try
                {
                    var copy = new Hit(hit.BaseDamage, hit.IsForceKill, attacker, hit.IsRelicBasedDamage, hit.IgnoreBlock);
                    MEC.Timing.RunCoroutine(h.Entity._Damage(copy));
                }
                catch (Exception e) { MultiplayerMod.Instance.Logger.LogWarning($"Coup sur le héros de {Coop.NameOf(h.PlayerId)} : {e.Message}"); }
            }
        }

        // ───────────── Étape 4 : K.O. ─────────────

        /// <summary>Préfixe de Player.Death. Renvoie false pour un héros supplémentaire : il est mis K.O. au lieu de faire perdre la run.</summary>
        public static bool OnPlayerDeath(Player p)
        {
            var h = ByEntity(p);
            if (h == null) return true;
            if (!h.KO)
            {
                h.KO = true;
                Coop.AddLog($"<color=#ff8080>Le héros de {Coop.NameOf(h.PlayerId)} est K.O. !</color>");
                try { h.Entity.gameObject.SetActive(false); } catch { }
            }
            return false;
        }

        /// <summary>Fin de combat gagné : les héros K.O. reviennent à 1 PV.</summary>
        public static void OnBattleWon()
        {
            foreach (var h in _heroes.Values.Where(x => x.KO))
            {
                h.KO = false;
                h.Data.ChangeCurrentHealth(1);
                if (h.Entity != null) { try { h.Entity.gameObject.SetActive(true); h.Entity.ForceHealthRefresh(); } catch { } }
                Coop.AddLog($"Le héros de {Coop.NameOf(h.PlayerId)} se relève avec 1 PV.");
            }
        }

        /// <summary>Tous les héros sont tombés ? (le héros de l'hôte compris)</summary>
        public static bool AllExtraHeroesKO => _heroes.Count > 0 && _heroes.Values.All(h => h.KO);

        // ───────────── Affichage ─────────────

        public static void DrawLabels()
        {
            var cam = Camera.main;
            if (cam == null || !Enabled || Team.RunPicks.Count == 0) return;
            var host = Game.Player;
            if (host != null) Label(cam, host.transform, Team.RunPicks[0].Id, false);
            foreach (var h in _heroes.Values)
                if (h.Entity != null) Label(cam, h.Entity.transform, h.PlayerId, h.KO);
        }

        private static void Label(Camera cam, Transform t, int playerId, bool ko)
        {
            float hgt = 2f;
            var sr = t.GetComponentInChildren<SpriteRenderer>();
            if (sr != null) hgt = sr.bounds.size.y;
            var s = cam.WorldToScreenPoint(t.position + new Vector3(0, hgt * 1.05f, 0));
            if (s.z < 0) return;
            GUI.Label(new Rect(s.x - 60, Screen.height - s.y - 10, 140, 22),
                $"<b><color={Coop.ColorOf(playerId)}>{Coop.NameOf(playerId)}</color></b>{(ko ? " <color=#ff6060>K.O.</color>" : "")}");
        }

        public static string HashPart() =>
            string.Join(";", _heroes.Values.OrderBy(h => h.PlayerId).Select(h => $"h{h.PlayerId}={h.Data.CurrentHealth}/{h.Data.MaxHealth}/{h.Data.CurrentBlock}/{(h.KO ? 1 : 0)}"));
    }
}
