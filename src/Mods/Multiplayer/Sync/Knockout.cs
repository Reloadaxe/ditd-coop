using System;
using System.Linq;
using DiceLoader;
using DiceMultiplayer.Net;
using UnityEngine;

namespace DiceMultiplayer.Sync
{
    /// <summary>
    /// Mort en coop : K.O. puis retour à 1 PV.
    ///  - Mon héros tombe à 0 PV alors qu'un autre joueur est encore debout : la mort n'est pas validée
    ///    (Player.CanCompleteDeath, le point d'entrée de la relique « patte de singe »). Je suis K.O. : je ne pose plus
    ///    de dés, ma fin de tour est validée automatiquement, je regarde les autres finir le combat.
    ///  - Fin du combat gagnée : je reviens avec 1 PV.
    ///  - Tout le monde K.O. : la mort est validée, c'est la défaite (les joueurs déjà K.O. meurent aussi).
    /// </summary>
    public static class Knockout
    {
        public static bool LocalKO { get; private set; }
        private static bool _allowDeath;

        public static void Reset() { LocalKO = false; _allowDeath = false; }

        /// <summary>Préfixe de Player.CanCompleteDeath. Renvoie false (et __result = false) pour garder le héros K.O.</summary>
        public static bool OnCanCompleteDeath(Player p, ref bool result)
        {
            if (!Coop.Active || _allowDeath || p == null || Game.Player == null || p.Pointer != Game.Player.Pointer) return true;
            if (!Heroes.AnyRemoteAlive) return true;   // plus personne debout : défaite normale
            result = false;
            if (!LocalKO)
            {
                LocalKO = true;
                Coop.AddLog("Tu es K.O. : tu regardes la fin du combat et tu reviendras avec 1 PV.");
                Coop.Toast("K.O. ! Tu reviendras avec 1 PV après le combat");
                AutoReady();
            }
            return false;
        }

        /// <summary>Début de la phase de choix des dés : un joueur K.O. valide sa fin de tour tout seul.</summary>
        public static void AutoReady()
        {
            if (!LocalKO || !Game.InBattle) return;
            Game.Schedule(() =>
            {
                if (!LocalKO || !Game.InBattle || Game.TurnState != BattleSystem.TurnState.SelectDice) return;
                if (Coop.ReadyPlayers.Contains(Coop.LocalId)) return;
                var ui = UnityEngine.Object.FindObjectOfType<FinishTurnUI>();
                if (ui != null) ui.OnClickFinishTurnButton();
            }, 1.2f);
        }

        /// <summary>Un joueur K.O. ne pose pas de dés.</summary>
        public static bool BlocksPlacement()
        {
            if (!LocalKO) return false;
            Coop.Toast("Tu es K.O. : tu ne peux pas jouer de dés ce combat");
            return true;
        }

        /// <summary>Fin du combat : retour à 1 PV si le combat est gagné.</summary>
        public static void OnBattleEnd(BattleSystem.EndState result)
        {
            if (!LocalKO) return;
            LocalKO = false;
            var p = Game.Player;
            if (p == null) return;
            try
            {
                if (p.CurrentHealth < 1) p.ChangeCurrentHealth(1);
                p.ForceHealthRefresh();
                Coop.AddLog("De retour avec 1 PV.");
                Coop.Toast("De retour avec 1 PV !");
            }
            catch (Exception e) { MultiplayerMod.Instance.Logger.LogWarning("Retour à 1 PV : " + e.Message); }
        }

        /// <summary>Chaque frame en combat : si tout le monde est K.O., la défaite est validée chez moi aussi.</summary>
        public static void Update()
        {
            if (!LocalKO || !Coop.Active || !Game.InBattle || Heroes.AnyRemoteAlive || _allowDeath) return;
            var p = Game.Player;
            if (p == null) return;
            _allowDeath = true;
            Coop.AddLog("Toute l'équipe est K.O. : défaite.");
            try { MEC.Timing.RunCoroutine(p._Damage(new Hit(9999, true, null, false, true), null, true, true, false)); }
            catch (Exception e) { MultiplayerMod.Instance.Logger.LogWarning("Défaite de l'équipe : " + e.Message); }
        }
    }
}
