# Die in the Dungeon 5.1.0 : guide de rétro-ingénierie

Build analysé : Unity **2022.3.62f** · backend **IL2CPP** (x64) · métadonnées **v31**, non chiffrées.
Le code du jeu se trouve dans `GameAssembly.dll` et les noms dans `Die in the Dungeon_Data/il2cpp_data/Metadata/global-metadata.dat`.

## Contenu du SDK

| Fichier | Contenu |
|---|---|
| `api/Assembly-CSharp.cs` | **Toutes** les classes du jeu (3 615 types) : champs avec offsets, propriétés, méthodes avec RVA. C'est le fichier de référence principal. |
| `api/Classes.md` | Index des 1 898 types « réels » (hors types générés par le compilateur), classés en 20 catégories, avec numéro de ligne. |
| `api/Enums.md` | Les 323 énumérations avec leurs valeurs : reliques (195), ennemis (60), types de dés (37), modificateurs (74), potions (58)… |
| `dump/dump.cs` | Dump brut Il2CppDumper, toutes assemblies confondues (Unity, Rewired, DOTween, Odin, Steamworks…). |
| `dump/DummyDll/` | DLL factices (signatures sans corps), à ouvrir dans dnSpy ou ILSpy pour naviguer. |
| `dump/il2cpp.h` | Structures C de tous les types, à importer dans IDA ou Ghidra. |
| `dump/script.json` | Adresses de toutes les méthodes, chaînes et métadonnées, pour les scripts `ida_py3.py` / `ghidra.py` d'Il2CppDumper. |
| `dump/stringliteral.json` | Toutes les chaînes littérales du code. |

> Pour le modding C#, tu n'as pas besoin des RVA : BepInEx génère au premier lancement les assemblies d'interop (`BepInEx/interop/*.dll`), qui exposent ces mêmes classes en C#. Les RVA servent au hooking natif et à l'analyse dans IDA ou Ghidra. Adresse réelle = base de `GameAssembly.dll` + RVA.

## Architecture du jeu

### 1. GameManager : la machine à états globale
`GameManager : Singleton<GameManager>`, accessible par `Singleton<GameManager>.Instance`.
Il pilote une `EnumStateMachine<GameManager.GameState, GameManagerBaseState>` avec un objet état par valeur :

```
NONE → GameBoot → TitleScreen → RunStart → Map ⇄ FloorInit → BattleFloor | EventFloor | ShopFloor | LootFloor
                                              ↘ DungeonFloorInit → DungeonFloorProgress
     FloorEndSuccess / FloorEndFail → Map … → MapAreaFinish → RunFinish → GameWin | GameOver
```

- Méthodes `ChangeStateXxx()` : transitions. `OnEnterXxx()` : callbacks d'entrée dans un état.
- `StartRun(RunInfo.NewRunData, ActiveCustomRun, bool)` RVA `0x3C67E0`, `LoadRun(SaveData)` RVA `0x3C4580`.
- `LoadNewBattle / LoadNewEvent / LoadNewShop / LoadNewLoot / LoadNewDungeon` : chargement des salles.
- `DevModeActive` : **mode développeur caché**, normalement activé par une séquence de touches dans `DebugInput()`. Il active `DebugInputDevMode()` et `DebugExtraKeys()`.
- Événement statique : `GameManager.GameStateChangeEvent` (`SafeEvent<GameManagerStateChangeArgs>`).

### 2. Cycle de vie : `IExecutionEvents`
La plupart des systèmes implémentent `IExecutionEvents` :
`GameBoot()`, `RunStart()`, `FloorLoad()`, `FloorStart()`, `FloorEnd(bool success)`, `RunEnd()`.
Implémenteurs : `BattleSystem`, `FloorSystem`, `DiceManager`, `Relics`, `Runestones`, `CurrencySystem`, `Background`, `SoundManager`, `CameraController`, `CanvasManager`, `SettingsUI`…

### 3. Run : `RunInfo` (statique) et `CurrentRunData`
- `RunInfo.CurrentRun` : `CurrentRunData` de la run en cours.
- `RunInfo.CurrentFloor` : `Floor` courant. `RunInfo.CurrentFloorChangeEvent` est déclenché à chaque changement de salle.
- `CurrentRunData` : `Seed` (`SeedSystem`), `Stats`, `Timer`, `Areas` (carte), `PlayerName` (`Player.CharacterName` : Cinder, Lisver, Nera, Mango), `CompanionName`, `CurrencyAmount`, `DifficultyLevel`, `AllSelectedRelics`, `SelectedChallenges`, `IsEndlessMode`, `futureBattleChanges / futureEventChanges / futureShopChanges / futureMapChanges`…
  - `StartRun` `0x4348D0`, `EndRun(bool won)` `0x432D80`, `ChangeCurrencyAmount(int)` `0x4326D0`.

### 4. Floors : `FloorSystem : Singleton<FloorSystem>`
Il contient `battle` (`BattleSystem`), `eventController`, `lootController`, `dungeonController`, `ShopController`, `CurrentPlayer`.
`FloorStart()` `0x389400`, `FloorEnd(bool)` `0x389140`.

### 5. Combat : `BattleSystem`
Machine de tour `BattleSystem.TurnState` :
`Intro → StartTurn → DrawDice → PreBoardActions → ShowBoard → TurnBeginningActions → EnemySelectDice → SelectDice → (PerformFlashDice | UseRunestones) → EnemyPhaseBeforePlayer → PreExecuteDice → ExecuteDice → EnemyPhaseAfterPlayer → EndTurn → …`
et en fin de combat `DefeatedAllEnemies → WinBattle | LoseBattle → SelectReward → ContinueAfterWin`, plus `CompleteArea`, `CompleteRun` et `EndlessProposal`.

- `ChangeState(TurnState)` `0x3718C0` : point de hook idéal pour suivre le tour.
- `BattleEnd(EndState)` `0x36FE20` (`Win`, `Lose`, `ForceFinish`).
- Événements statiques : `IntroEvent`, `StartTurnEvent`, `EndTurnEvent`, `EnterPreExecuteDiceEvent`, `EnterEnemyPhaseAfterPlayerEvent`…
- `BattleValues` (`BattleSystem.Values`) : totaux attaque, bloc, soin… calculés depuis le plateau.
- Commandes de debug intégrées : `DebugKillAllEnemies()`, `DebugForceFinish()`, `DebugApplyMaxHeal()`, `DebugAllActionsMax()`, `SkipToWinScreen()`, `ToggleEnemiesAttack()`.
- Le plateau : `BattleBoard` (abstrait), avec des `BoardSlot` où l'on pose des `GameObjectDice`.
- Les coroutines utilisent **MEC** (`IEnumerator<float>`, fonctions `_Xxx`). Évite de les patcher : préfère les méthodes synchrones qui les lancent.

### 6. Entités : `Entity` → `Player` / `Enemy`
- `Entity` : `CurrentHealth`, `MaxHealth`, `CurrentBlock`, `modifiers` (`List<ModifierData>`), `ChangeCurrentHealth` `0x784F60`, `ChangeMaxHealth`, `ChangeBlockValue`, `AddModifier` `0x783D30`, `RemoveModifier`, `_Damage(Hit…)`, `_Heal(HealActiveData)`, `Death(DeathInfo, Action[])`.
  - Événements : `HitMadeEvent`, `DamageBlockedEvent`, `DamageReceivedEvent`, `EntityDeathEvent`.
  - Les données persistantes sont dans `ActiveEntity` (`ActiveData`).
- `Player` (abstrait) : `CinderPlayer`, `LisverPlayer`, `NeraPlayer`, `MangoPlayer`. `Death` `0x78C120`.
- `Enemy` (abstrait) : une sous-classe par ennemi (`AntEnemy`, `BeeQueenEnemy`, `CaterpillarBossEnemy`, `MosquitoDukeEnemy`…). `EnemyName` (`Enemy.Name`), `InitEnemy` `0x75A4A0`, `Death` `0x758150`, `EnemyDeathEvent`.
- Compagnons : `Companion` (abstrait), par exemple `BabyFlyCompanion` et `PotionMimicCompanion`.

### 7. Dés : `DiceManager : Singleton<DiceManager>`
- Piles : `diceInDrawPile`, `diceInHand`, `diceInDiscardPile`, `diceOnBoard`, `diceInExilePile`.
- `CurrentRunDeck` (`ActiveDeck`, de 3 à 24 dés), `CurrentEnergy` (setter `0x710880`), `diceDrawPerTurn`, `MaxDiceInHand()` `0x70A4E0`, `LoadDeck(DeckData)` `0x7090C0`.
- `ActiveDice` : `types` (`List<DiceData.Type>`), `faces` (`DiceFaces`), `tier`, `rarity`, `CurrentFaceValue`, propriétés temporaires et permanentes (`DiceData.Property`), `Shape`.
- `DiceData` : ScriptableObject (Odin `SerializedScriptableObject`) qui définit un dé.

### 8. Reliques, potions, runes
- `Relics : Singleton<Relics>` : `CurrentRelics` (`List<ActiveRelic>`), `AddRelic(Title, Side, int)` `0x41B920`, `RemoveRelic`, `HaveThisRelic`. `RelicData` : `title`, `rarity`, `allergies`, et des listes statiques (`StarterRelics`, `ShopRelics`, `CorruptedRelics`…).
- `PotionData` / `ActivePotion` : potions. `Runestones : Singleton<Runestones>` : runes (`RunestoneDice`).
- `ModifierData` et ses ~70 sous-classes `XxxModifierData` (Poison, Thorns, Rage, Barrier…) : statuts appliqués aux entités.

### 9. Économie, boutique, récompenses
- `CurrencySystem : Singleton<CurrencySystem>` : `AddCurrencyValue` `0x473590`, `RemoveCurrencyValue`, bornes 0 à 9999, `CurrencyAddedEvent` / `CurrencyRemovedEvent`.
- `ShopController`, `ShopItems`, `IShopItem` ; `RewardData` et `RewardsUI` ; `GameData.ItemChanceProgression<T>` pour les tables de drop.

### 10. Carte et événements
- `Map : Singleton<Map>`, `ActiveMapChunk`, `ActiveMapTile`, `MapXxxTileData` (Battle, Elite, Event, Shop, Rest, Forge, Treasure, Dungeon…).
- Événements narratifs : `EventData`, `EventController`, et les nœuds `NodeXxxData` (ModifyHealth, ModifyCurrency, SelectRelic, UpgradeDice, TransformDice…).

### 11. Sauvegarde : `SaveDataManager` (statique)
- `SavedData` (`SaveData`), `SaveGameData()`, `LoadGameData()` `0x47D4D0`, `SaveActiveRun()` `0x47E100`, `DeleteActiveRun()`, `ChangeActiveSlot(int)`, `GetSavePath(int slot)`.
- Sérialisation : Odin (`Sirenix.Serialization`).

### 12. Divers
- Entrées : **Rewired** (`RewiredInputManager`) et Unity InputSystem. L'ancien `UnityEngine.Input` n'est pas fiable : dans un mod, lis le clavier avec `Event.current` dans `OnGUI`.
- Localisation : **I2 Localization** (`I2.Loc.LocalizationManager`).
- Tweens : DOTween. Steam : `SteamManager`, `SteamStats`, `Achievements`.
- IMGUI : beaucoup de méthodes ont été strippées. Restent `GUILayout.Label / Button / Toggle / TextArea / Space / Begin|End(Horizontal|Vertical|Area|ScrollView)`, `GUI.Label(Rect,string)` et `GUI.Box(Rect,string)`.

## Recettes

**Hook Harmony sur une méthode du jeu** (dans un mod DiceLoader) :
```csharp
[HarmonyPatch(typeof(Entity), nameof(Entity.AddModifier))]
static class NoPoison {
    static bool Prefix(Entity __instance, ModifierData __0) =>
        !(__instance.GetSide() == Entity.Side.Player && __0.type == ModifierData.Modifier.Poison); // false = annule l'appel
}
```
Vérifie le nom exact des champs dans `api/Assembly-CSharp.cs` : ici `ModifierData.type` (offset 0x10).
Les scripts IDA et Ghidra sont dans `SDK/tools/`.

**Trouver une méthode** : cherche dans `api/Classes.md`, ouvre `api/Assembly-CSharp.cs` à la ligne indiquée, ou fais un `grep` sur le nom.

**Analyse native (IDA ou Ghidra)** : charge `GameAssembly.dll`, puis lance `ida_with_struct_py3.py` ou `ghidra_with_struct.py`, fournis par Il2CppDumper, avec `dump/script.json` et `dump/il2cpp.h`. Toutes les fonctions sont alors nommées et typées.
