# Multijoueur Coop (v0.1, expérimental)

Coop de 2 à 3 joueurs en TCP direct. Tout le monde joue **la même run**, synchronisée en *lockstep sur les entrées*.

## Jouer
1. Tous les joueurs ont le jeu avec DiceLoader et `Mods/Multiplayer/Multiplayer.dll`.
2. **Hôte** : menu F1 > Multijoueur Coop > Options > *Créer la partie* (port 27015 par défaut, TCP à autoriser dans le pare-feu).
3. **Autres joueurs** : même page, saisir l'IP de l'hôte > *Rejoindre*.
   En dehors d'un réseau local, utilise Radmin VPN, ZeroTier ou Tailscale et l'IP qu'ils fournissent.
4. Quand tout le monde est connecté, l'hôte lance une **nouvelle run**. Elle démarre automatiquement chez tous les joueurs, avec la même seed.
5. Test en solo : lance deux fois le jeu sur le même PC. La première instance crée la partie, la seconde rejoint `127.0.0.1`.

## Règles
- **Carte, récompenses, événements, boutique** : seul le *meneur* clique (l'hôte, ou un joueur différent à chaque étage si « Meneur tournant » est activé chez tous). Les autres voient ses choix se répercuter.
- **Combat** : les dés de la main sont répartis entre les joueurs. Le nom du propriétaire s'affiche sous chaque dé. Chacun pose ses propres dés, et un dé posé chez l'un est posé chez tous.
- **Fin de tour** : chaque joueur clique « Fin de tour » pour se déclarer prêt. Le tour se termine quand tout le monde l'est.
- **Curseurs** : on voit le curseur et le nom des autres joueurs sur la carte et sur le plateau.
- **Chat** : depuis la page Options.
- **Désynchronisation** : une empreinte de l'état (PV, or, ennemis, main…) est comparée à chaque tour. Une alerte rouge s'affiche si les parties divergent.

## Fonctionnement technique
- `Net/` : TCP en étoile, JSON ligne par ligne, l'hôte relaie et fait foi sur l'ordre des actions.
- `Sync/Patches.cs` :
  - patch générique de chaque `OnPointerClick` du jeu et de l'UI. Les fonctions natives fusionnées par le compilateur sont ignorées ;
  - patchs dédiés sur `BattleBoard.PlaceDiceOnBoard` (contexte Player), `ReturnDiceToHand`, `FinishTurnUI`, `GameManager.StartRun` et `SeedSystem.StartRun` (application de la seed de l'hôte).
- `Sync/Coop.cs` : rejeu des actions, droits (meneur, propriété des dés), système « prêt », comparaison des empreintes.

## Limites connues (v0.1)
- **Un seul héros dans la logique du jeu** : PV, bloc et énergie sont partagés. Les dés sont répartis, mais il n'y a pas encore plusieurs personnages distincts. Ce sera la phase suivante, qui demande de réécrire une partie de `BattleSystem`.
- Aucune resynchronisation automatique après une divergence pour l'instant : il faut relancer la run.
- La reprise d'une sauvegarde (« Continuer ») est bloquée pendant une session.
- Clavier et manette ne sont pas répliqués : joue à la souris.
- Les clics se repèrent par leur chemin dans la hiérarchie d'objets Unity. Si un clic est introuvable chez un joueur, le journal l'indique.
