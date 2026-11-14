using System;
using DiceLoader;
using DiceMultiplayer.Net;
using UnityEngine;

namespace DiceMultiplayer.Sync
{
    /// <summary>
    /// Écran titre en coop :
    ///  - client connecté : écran d'attente, tous les clics du jeu sont bloqués ;
    ///  - hôte : « Nouvelle run » ouvre la sélection de personnage chez tous les clients ;
    ///  - clients : ils ne peuvent ni revenir en arrière, ni lancer ou continuer une run eux-mêmes ;
    ///  - hôte : un retour à l'écran titre ferme la sélection chez tous.
    /// </summary>
    public static class Lobby
    {
        /// <summary>Hôte : la sélection de personnage est ouverte.</summary>
        public static bool HostInSelection;
        /// <summary>Client : l'hôte a ouvert la sélection.</summary>
        public static bool SelectionOpen;

        public static bool IsClient => Coop.Active && !Coop.IsHost;

        /// <summary>Le client attend que l'hôte ouvre la sélection : écran d'attente et clics bloqués.</summary>
        public static bool ClientWaiting => IsClient && Game.State == GameManager.GameState.TitleScreen && !Game.InRun && !SelectionOpen;

        public static void Reset()
        {
            HostInSelection = false;
            SelectionOpen = false;
        }

        private static CanvasTitleScreen Title => UnityEngine.Object.FindObjectOfType<CanvasTitleScreen>();

        // ───────────── Préfixes et postfixes (CanvasTitleScreen) ─────────────

        /// <summary>« Nouvelle run » (et sa pop-up de vérification). Bloqué chez le client sauf s'il est rejoué.</summary>
        public static bool NewRunPrefix()
        {
            if (!IsClient || Coop.Replaying) return true;
            Coop.Toast("C'est l'hôte qui ouvre la sélection des personnages");
            return false;
        }

        /// <summary>Hôte : la sélection vient de s'ouvrir, on l'ouvre chez tout le monde.</summary>
        public static void NewRunPostfix()
        {
            if (!Coop.Active || !Coop.IsHost || Coop.Replaying) return;
            HostInSelection = true;
            Team.Picks.Clear();
            Coop.Send(new Msg { t = T.OpenSel });
            Coop.AddLog("Sélection des personnages ouverte pour tous.");
        }

        /// <summary>Bouton retour de la sélection. Interdit aux clients, propagé à tous quand c'est l'hôte.</summary>
        public static bool ReturnPrefix()
        {
            if (!Coop.Active || Coop.Replaying) return true;
            if (IsClient) { Coop.Toast("Seul l'hôte peut revenir à l'écran titre"); return false; }
            return true;
        }

        public static void ReturnPostfix()
        {
            if (!Coop.Active || !Coop.IsHost || Coop.Replaying) return;
            HostInSelection = false;
            Team.Picks.Clear();
            Coop.Send(new Msg { t = T.CloseSel });
            Coop.AddLog("Retour à l'écran titre pour tous.");
        }

        /// <summary>« Continuer », Endless et autres modes : pas encore gérés en coop.</summary>
        public static bool BlockedInCoopPrefix()
        {
            if (!Coop.Active || Coop.Replaying) return true;
            Coop.Toast("Pas encore disponible en coop : utilise « Nouvelle run »");
            return false;
        }

        // ───────────── Messages ─────────────

        public static void OnOpenSelection()
        {
            if (Coop.IsHost) return;
            Team.Picks.Clear();
            if (SelectionOpen && !Game.InRun) return;
            var t = Title;
            if (t == null) { Coop.AddLog("Écran titre introuvable : impossible d'ouvrir la sélection."); return; }
            SelectionOpen = true;
            Coop.AddLog("L'hôte a ouvert la sélection : choisis ton personnage puis clique sur Commencer.");
            Coop.Replay(() => t.OnClickNewRunButton());
        }

        public static void OnCloseSelection()
        {
            if (Coop.IsHost) return;
            Team.Picks.Clear();
            if (!SelectionOpen) return;
            SelectionOpen = false;
            var t = Title;
            if (t != null) Coop.Replay(() => t.OnClickReturnTitleButton());
            Coop.AddLog("L'hôte est revenu à l'écran titre.");
        }

        /// <summary>Hôte : un joueur vient d'arriver. S'il y a une sélection en cours, on la lui ouvre aussi.</summary>
        public static void OnPlayersChanged()
        {
            if (Coop.Active && Coop.IsHost && HostInSelection && !Game.InRun)
                Coop.Send(new Msg { t = T.OpenSel });
        }

        public static void OnRunStarted()
        {
            HostInSelection = false;
            SelectionOpen = false;
        }

        // ───────────── Écran d'attente ─────────────

        public static void DrawWaitingScreen(Action drawLeaveButton)
        {
            if (!ClientWaiting) return;
            var full = new Rect(0, 0, Screen.width, Screen.height);
            GUI.Box(full, "");
            GUI.Box(full, "");   // deux couches pour assombrir davantage
            float w = 520, h = 200;
            var r = new Rect((Screen.width - w) / 2, (Screen.height - h) / 2, w, h);
            GUI.Box(r, "");
            GUILayout.BeginArea(new Rect(r.x + 20, r.y + 16, w - 40, h - 32));
            UI.Label("<size=22><b>Connecté à la partie</b></size>");
            UI.Space(6);
            var host = Coop.NameOf(0);
            int dots = (int)(Time.unscaledTime * 2) % 4;
            UI.Label($"<size=15>En attente que <b>{host}</b> lance une nouvelle run{new string('.', dots)}</size>");
            UI.Label("La sélection des personnages s'ouvrira automatiquement.");
            UI.Label($"Joueurs : {Coop.Net.Players.Count}/{NetSession.MaxPlayers}");
            UI.Space(10);
            drawLeaveButton?.Invoke();
            GUILayout.EndArea();
        }
    }
}
