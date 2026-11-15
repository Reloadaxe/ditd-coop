# Multijoueur Coop (v0.4, expérimental)

Coop à 2 joueurs en TCP direct. **Chaque joueur joue sa propre run** : son personnage, son deck, sa main, son énergie, ses PV, ses reliques, son or et ses récompenses.
Les runs avancent ensemble et ne partagent que la carte, les ennemis, le plateau et le rythme des tours.

## Jouer
1. Hôte : **F2** > *Créer la partie* (port TCP 27015, à autoriser dans le pare-feu).
2. Autres joueurs : **F2** > IP de l'hôte > *Rejoindre*. Un écran d'attente s'affiche. À distance, utilisez Radmin VPN, ZeroTier ou Tailscale.
3. L'hôte clique sur **Nouvelle run** : la sélection s'ouvre chez tout le monde. Chacun choisit son personnage, puis clique sur **Commencer**.
4. Quand tout le monde est prêt, chacun démarre SA run sur la même seed.
5. Pour tester seul : bouton **Lancer 2 fenêtres** du launcher.

## Ce qui est partagé
| | |
|---|---|
| **Carte** | Même carte (même seed). Seul le meneur choisit la case, et il ne peut avancer que quand tout le monde est revenu sur la carte. |
| **Ennemis** | Mêmes combats. Les dégâts de chaque joueur sont appliqués aux ennemis de tous. L'hôte corrige les PV des ennemis à chaque tour. |
| **Attaques ennemies** | Chaque jeu fait attaquer son propre héros : les ennemis frappent tous les joueurs en même temps. |
| **Plateau** | Les dés posés par les autres apparaissent sur ton plateau, à leur couleur, et leurs cases te sont bloquées. |
| **Héros** | Les héros des autres sont affichés à côté du tien, avec leurs PV et leur bloc en direct. |
| **Fin de tour** | Chacun clique sur « Fin de tour » pour être prêt. Le tour passe quand tout le monde l'est. |

Tout le reste (récompenses, événements, boutique, reliques, or) est personnel.

## Fonctionnement technique
- `Sync/Team.cs` : lobby, choix de chacun, seed commune, lancement de chaque run.
- `Sync/Coop.cs` : messages, carte pilotée par le meneur (clics rejoués uniquement sur la carte), fin de tour collective.
- `Sync/Combat.cs` : `Enemy._Damage` en postfix (mes coups partent aux autres), application des coups distants, correction des PV par l'hôte.
- `Sync/Board.cs` : poses et retraits de dés envoyés, fantômes des dés des autres, cases bloquées.
- `Sync/Heroes.cs` : visuels des héros des autres (prefab du personnage sans scripts), PV et bloc reçus du réseau.
- `Sync/Lobby.cs` : écran d'attente, sélection ouverte par l'hôte, retour bloqué pour les clients.

## Limites connues (v0.4)
- Les effets **combinés** entre les dés de joueurs différents (adjacence, bonus de case) ne sont pas calculés : chacun ne profite que de ses dés.
- Les **statuts** posés sur les ennemis (poison, etc.) ne sont pas encore partagés. Seuls les dégâts le sont.
- Si un joueur meurt, il perd sa run normalement : pas encore de K.O. ni de retour.
- Les ennemis doivent agir pareil chez tous (même seed). L'hôte corrige leurs PV, mais pas encore leurs intentions.
