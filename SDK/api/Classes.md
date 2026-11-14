# Index des types — Assembly-CSharp (Die in the Dungeon 5.1.0)

1898 types. La colonne **Ligne** renvoie à [Assembly-CSharp.cs](Assembly-CSharp.cs), qui contient les champs (avec offsets), les propriétés et les méthodes (avec RVA).


## Cœur / états du jeu

| Type | Genre | Hérite / implémente | Ligne |
|---|---|---|---|
| `Assets` | class | `MonoBehaviour` | 228 |
| `CoroutineManager` | class | `MonoBehaviour` | 66877 |
| `GameData` | class | `SerializedScriptableObject, IDisposable` | 36744 |
| `GameData.AreaInMap` | class | `ICustomRNG, ISeedable` | 36390 |
| `GameData.CompanionData` | struct |  | 36369 |
| `GameData.DicePropertyAvailability` | struct |  | 36643 |
| `GameData.DiceRewardMode` | enum |  | 36676 |
| `GameData.DiceShapeAvailability` | struct |  | 36660 |
| `GameData.DiceShapeAvailability.ShapePerCharacter` | struct |  | 36653 |
| `GameData.DiceTypeAvailability` | struct |  | 36633 |
| `GameData.EnemyData` | struct |  | 36354 |
| `GameData.EventDataForProgression` | class |  | 36561 |
| `GameData.FamilyData` | struct |  | 36377 |
| `GameData.PlayerData` | struct |  | 36344 |
| `GameData.ProgressionMapData` | class | `ICustomRNG, ISeedable` | 36513 |
| `GameData.ProgressionMapData.EliteMobData` | class |  | 36404 |
| `GameData.RarityChance` | struct |  | 36570 |
| `GameData.RelicChanceMode` | enum |  | 36668 |
| `GameManager.BuildMode` | enum |  | 23653 |
| `GameManager.GameState` | enum |  | 23628 |
| `GameManagerBattleState` | class | `GameManagerBaseState` | 24236 |
| `GameManagerDungeonInitState` | class | `GameManagerBaseState` | 24272 |
| `GameManagerDungeonProgressState` | class | `GameManagerBaseState` | 24281 |
| `GameManagerEventState` | class | `GameManagerBaseState` | 24245 |
| `GameManagerFloorEndFailState` | class | `GameManagerBaseState` | 24299 |
| `GameManagerFloorEndSuccessState` | class | `GameManagerBaseState` | 24290 |
| `GameManagerFloorInitState` | class | `GameManagerBaseState` | 24227 |
| `GameManagerGameBootState` | class | `GameManagerBaseState` | 24185 |
| `GameManagerGameOverState` | class | `GameManagerBaseState` | 24308 |
| `GameManagerGameWinState` | class | `GameManagerBaseState` | 24324 |
| `GameManagerIndex` | class | `ScriptableObject` | 36816 |
| `GameManagerLootState` | class | `GameManagerBaseState` | 24254 |
| `GameManagerMapAreaFinishState` | class | `GameManagerBaseState` | 24219 |
| `GameManagerMapDeathState` | class | `GameManagerBaseState` | 24210 |
| `GameManagerMapState` | class | `GameManagerBaseState` | 24201 |
| `GameManagerNoneState` | class | `GameManagerBaseState` | 24178 |
| `GameManagerRunFinishState` | class | `GameManagerBaseState` | 24316 |
| `GameManagerShopState` | class | `GameManagerBaseState` | 24263 |
| `GameManagerTitleScreenState` | class | `GameManagerBaseState` | 24193 |
| `GameplayContext` | class | `ISettingsUIResponsive, IBackpackUIResponsive, IEventsUIResponsive, ...` | 24400 |
| `GameplayContext.Context` | enum |  | 24339 |
| `IExecutionEvents` | interface |  | 20375 |
| `IGameplayContextResponsive` | interface |  | 24470 |
| `IResourceManager_Bundles` | interface |  | 67024 |
| `ISceneLoadedResponsive` | interface |  | 21671 |
| `ResourceManager` | class | `MonoBehaviour` | 67030 |
| `SceneController.AsyncSceneLoader` | class | `CustomYieldInstruction` | 55718 |

## Run / progression

| Type | Genre | Hérite / implémente | Ligne |
|---|---|---|---|
| `ActiveCustomRun` | class | `ICloneable` | 30152 |
| `CatalogData.DifficultyInfo` | class |  | 32194 |
| `CharacterSelectorInfoController.DifficultyButtonValues` | class |  | 53590 |
| `CurrentRunData` | class | `IRunFlow` | 30216 |
| `CustomRunData` | class | `ScriptableObject` | 33049 |
| `CustomRunData.CharacterFilter` | enum |  | 32834 |
| `CustomRunData.CustomArea` | struct |  | 32809 |
| `CustomRunData.CustomArea.BasePerLoop` | class |  | 32752 |
| `CustomRunData.CustomArea.EliteMobsPerLoop` | class | `CustomRunData.CustomArea.BasePerLoop` | 32775 |
| `CustomRunData.CustomArea.MobsPerLoop` | class | `CustomRunData.CustomArea.BasePerLoop` | 32765 |
| `CustomRunData.CustomDiceReward` | class |  | 33015 |
| `CustomRunData.CustomDiceReward.AreaMode` | enum |  | 33007 |
| `CustomRunData.FloorReplaceData` | class |  | 32998 |
| `CustomRunData.LengthMode` | enum |  | 32850 |
| `CustomRunData.PhaseRules` | class |  | 32966 |
| `CustomRunData.PhaseRules.Changes` | struct |  | 32890 |
| `CustomRunData.PhaseRules.Changes.GrowthData` | struct |  | 32881 |
| `CustomRunData.PhaseRules.Changes.ValueIncrease` | struct |  | 32866 |
| `CustomRunData.PhaseRules.Changes.ValueIncrease.Mode` | enum |  | 32858 |
| `CustomRunData.PhaseRules.FloorFrequency` | enum |  | 32936 |
| `CustomRunData.PhaseRules.FloorMode` | enum |  | 32927 |
| `CustomRunData.PhaseRules.ModifyReward` | struct |  | 32911 |
| `CustomRunData.PhaseRules.ModifyReward.TargetTile` | enum |  | 32903 |
| `CustomRunData.PhaseRules.ModifyTile` | struct |  | 32897 |
| `CustomRunData.PhaseRules.TargetRewardRow` | enum |  | 32945 |
| `CustomRunData.StartingRelicMode` | enum |  | 32842 |
| `CustomRunData.Title` | enum |  | 32826 |
| `DiceFaceCatalog.FacesRNGMode` | enum |  | 33886 |
| `Difficulty` | class |  | 30458 |
| `Difficulty.Level` | enum |  | 30414 |
| `Difficulty.VisualGroup` | enum |  | 30441 |
| `EditorSeedMode` | enum |  | 45622 |
| `EnemyBehavior.BehaviorByDifficulty` | struct |  | 15399 |
| `Floor` | class |  | 20075 |
| `Floor.RoomBase` | class |  | 20010 |
| `Floor.RoomBattle` | class | `Floor.RoomBase` | 20019 |
| `Floor.RoomEvent` | class | `Floor.RoomBase` | 20045 |
| `Floor.RoomLoot` | class | `Floor.RoomBase` | 20056 |
| `Floor.RoomShop` | class | `Floor.RoomBase` | 20065 |
| `FloorEffects` | class | `MonoBehaviour` | 20142 |
| `FloorExtra` | class |  | 20106 |
| `FloorSystem.TestTileMode` | enum |  | 20154 |
| `FloorVisualizer` | class | `SerializedMonoBehaviour` | 20218 |
| `ICustomRNG` | interface | `ISeedable` | 31057 |
| `IFloorChangeResponsive` | interface |  | 20227 |
| `IQuitRunButtonResponsive` | interface |  | 21065 |
| `IRestartRunInputResponsive` | interface |  | 21077 |
| `IRngUpdateResponsive` | interface |  | 30475 |
| `IRunFlow` | interface |  | 30405 |
| `ISeedable` | interface |  | 31064 |
| `IStarRunButtonResponsive` | interface |  | 21095 |
| `IStarRunResponsive` | interface |  | 24476 |
| `RNGExtensions` | class |  | 30946 |
| `RunDifficultyUI` | class | `MonoBehaviour, IInputTypeResponsive, IIdentifier, IGameplayContextR...` | 41843 |
| `RunHistoryDifficultyController` | class | `MonoBehaviour, IIdentifier, IToggleOnInputResponsive` | 29813 |
| `RunHistoryEntry` | class | `MonoBehaviour` | 29840 |
| `RunHistoryUI` | class | `MonoBehaviour, ILocalizable, IToggleOnInputResponsive, IIdentifier` | 30007 |
| `RunHistoryUI.DetailsNavMode` | enum |  | 29888 |
| `RunInfo` | class |  | 30881 |
| `RunInfo.MapAreas` | class | `IRunFlow` | 30776 |
| `RunInfo.MapAreas.AreaPositionInfo` | class |  | 30652 |
| `RunInfo.MapAreas.SectionNameInfo` | class |  | 30695 |
| `RunInfo.MapAreas.SectionNameInfo.EliteEnemyGroup` | class |  | 30668 |
| `RunInfo.NewRunData` | class |  | 30540 |
| `RunInfo.NewRunData.TutorialMode` | enum |  | 30524 |
| `RunInfo.Stats` | class | `IRunFlow` | 30572 |
| `RunInfo.Stats.Grade` | enum |  | 30559 |
| `RunInfo.Timer` | class | `IRunFlow` | 30624 |
| `RunInfo.Tutorials` | class | `IRunFlow` | 30845 |
| `RunInfoUI` | class | `MonoBehaviour, ILocalizable` | 41872 |
| `SaveData.ActiveCustomRunSaveData` | class |  | 3925 |
| `SaveData.ActiveRunData` | class |  | 3872 |
| `SaveData.ActiveSeedSaveData` | class |  | 3940 |
| `SaveData.RunHistoryData` | class |  | 4098 |
| `SaveData.RunHistoryData.StatsHistory` | class |  | 4064 |
| `SeedExtensions` | class |  | 30934 |
| `SeedHelper` | class |  | 30923 |
| `SeedHelper.MapTileRNGData` | struct | `ICustomRNG, ISeedable` | 30910 |
| `SeedSystem` | class | `IRunFlow` | 31022 |
| `SeedSystem.Rng` | class |  | 30980 |
| `SeedSystem.Rng.Context` | enum |  | 30959 |

## Sauvegarde / progression méta

| Type | Genre | Hérite / implémente | Ligne |
|---|---|---|---|
| `ActiveCharacterProgression` | class |  | 21945 |
| `ActiveSaveSlot` | class |  | 51696 |
| `CharacterProgressionUI` | class | `MonoBehaviour, IInputTypeResponsive, IIdentifier, IButtonResponsive` | 21771 |
| `CharacterProgressionUIHelper` | class | `MonoBehaviour` | 21855 |
| `Codex.ItemEnemy.UnlockPageMode` | enum |  | 44079 |
| `IDataLoader` | interface |  | 3724 |
| `IDataSaver` | interface |  | 3731 |
| `LevelProgression` | class |  | 21897 |
| `PlayerPreferences` | class |  | 3737 |
| `SaveData` | class |  | 4144 |
| `SaveData.ActiveDeckSaveData` | class |  | 3972 |
| `SaveData.ActiveDiceSaveData` | class |  | 3981 |
| `SaveData.ActiveFaceSaveData` | class |  | 4014 |
| `SaveData.ActivePotionSaveData` | class |  | 4036 |
| `SaveData.ActiveRelicSaveData` | class |  | 4024 |
| `SaveData.ActiveRerollSaveData` | class |  | 3949 |
| `SaveData.DefeatedEnemiesSaveData` | class |  | 4046 |
| `SaveData.DiceFacesSaveData` | class |  | 4002 |
| `SaveData.MapMovementSaveData` | class |  | 3962 |
| `SaveData.MinionDragonflyFleedSaveData` | class |  | 4054 |
| `SaveData.UserData` | class |  | 3860 |
| `SaveDataManager` | class |  | 4195 |
| `SaveSlots` | class |  | 51674 |
| `SaveSlotsController` | class | `MonoBehaviour, IInputUsedResponsive, IIdentifier, IInputTypeResponsive` | 51730 |
| `UnlockableContentPopup` | class | `MonoBehaviour, ILocalizable, IHighPriorityUI, IAttachedTooltip, IIn...` | 54242 |
| `Unlockables` | class |  | 45152 |
| `Unlockables.AchievementData` | class |  | 45019 |
| `Unlockables.DebugForceMode` | enum |  | 45009 |
| `UnlockeableButtonController` | class | `CustomButtonController, ISelectableResponsive` | 42775 |
| `UnlockeableButtonController.UnlockeableButtonType` | enum |  | 42766 |
| `UserDataStore_KeyValue.ActionMappingSaveMode` | enum |  | 59131 |
| `UserDataStore_KeyValue.ControllerAssignmentSaveInfo` | class |  | 59072 |
| `UserDataStore_KeyValue.ControllerAssignmentSaveInfo.JoystickInfo` | class |  | 59062 |
| `UserDataStore_KeyValue.ControllerAssignmentSaveInfo.PlayerInfo` | class |  | 59046 |
| `UserDataStore_PlayerPrefs.ActionMappingSaveMode` | enum |  | 59390 |
| `UserDataStore_PlayerPrefs.ControllerAssignmentSaveInfo` | class |  | 59341 |
| `UserDataStore_PlayerPrefs.ControllerAssignmentSaveInfo.JoystickInfo` | class |  | 59331 |
| `UserDataStore_PlayerPrefs.ControllerAssignmentSaveInfo.PlayerInfo` | class |  | 59315 |
| `VisualSaveSlot` | class | `MonoBehaviour, ILocalizable` | 51765 |

## Combat

| Type | Genre | Hérite / implémente | Ligne |
|---|---|---|---|
| `AlphaHitTestMinimumThreshold` | class | `MonoBehaviour` | 40017 |
| `Attack` | class |  | 17787 |
| `AttackBattleAction` | class | `BasicBattleAction` | 10341 |
| `BabyTickSlotManipulator` | class | `SlotManipulator` | 2656 |
| `BaseBoardSlot` | class | `MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerE...` | 952 |
| `BaseBoardSlot.Manipulators` | struct | `IAssertableDisposable, IDisposable, IDisposable, IAssertive` | 888 |
| `BaseBoardSlot.Special` | enum |  | 855 |
| `BattleAction` | class |  | 10249 |
| `BattleAction.EnemyModifiers` | struct |  | 10216 |
| `BattleAction.EnemyModifiers.Target` | enum |  | 10208 |
| `BattleBoard` | class | `MonoBehaviour, ISceneLoadedResponsive, IGameObjectDiceHoverResponsi...` | 1417 |
| `BattleBoard.BackgroundVisuals` | struct |  | 1038 |
| `BattleBoard.BattleData` | class |  | 1053 |
| `BattleBoard.DiscardMode` | enum |  | 1080 |
| `BattleBoard.ExtraGraphics` | enum |  | 1030 |
| `BattleBoard.PlaceDiceContext` | enum |  | 1071 |
| `BattleBoard.SlotDistanceStyle` | enum |  | 1045 |
| `BattleBoard.TurnData` | class |  | 1062 |
| `BattleChallenges` | class | `MonoBehaviour` | 18064 |
| `BattleChallenges.ActiveChallenge` | class |  | 17960 |
| `BattleChallenges.ActiveChallenge.Status` | enum |  | 17951 |
| `BattleChallenges.Challenge` | enum |  | 17845 |
| `BattleChallenges.Context` | enum |  | 17943 |
| `BattleChallenges.DebugForceMode` | enum |  | 17926 |
| `BattleChallenges.RewardType` | enum |  | 17896 |
| `BattleChallenges.TestChallenge` | struct |  | 17936 |
| `BattleDiceDescriptionController` | class | `DiceDescriptionController` | 5643 |
| `BattleEffectConfiguration` | class |  | 16834 |
| `BattleFX` | class | `MonoBehaviour` | 824 |
| `BattleFX.Context` | enum |  | 792 |
| `BattleFX.Data` | struct |  | 800 |
| `BattleFX.Type` | enum |  | 750 |
| `BattleInfo` | class | `IEnemyDeathResponsive, IIdentifier` | 18346 |
| `BattleInfo.TurnData` | class |  | 18202 |
| `BattleSystem` | class | `MonoBehaviour, IExecutionEvents, IInputTypeResponsive, IIdentifier,...` | 19504 |
| `BattleSystem.BattleAction` | class |  | 18705 |
| `BattleSystem.BattleActionModifiers` | class | `BattleSystem.BattleAction` | 18727 |
| `BattleSystem.CurrentPlaylist` | class |  | 18522 |
| `BattleSystem.DiceToBoard` | class |  | 18739 |
| `BattleSystem.EndState` | enum |  | 18752 |
| `BattleSystem.TurnState` | enum |  | 18540 |
| `BattleSystem.Values` | class |  | 18660 |
| `BlockBattleAction` | class | `BasicBattleAction` | 10420 |
| `BoardSlot` | class | `BaseBoardSlot, IDiceBoxResponsive, IBattleBoardResponsive, IEventsU...` | 2057 |
| `BoardSlot.BoardDirection` | enum |  | 1715 |
| `BoardSlot.FlashActionMode` | enum |  | 1725 |
| `BoardSlot.PathfindingData` | struct |  | 1692 |
| `BoardSlot.ShapeResources` | struct |  | 1676 |
| `BoardSlot.ShapeResources.Position` | enum |  | 1661 |
| `BoardSlot.TurnData` | class |  | 1705 |
| `CatalogData.BoardExtraGraphicInfo` | class |  | 32243 |
| `CatalogData.BoardExtraGraphicInfo.SpriteData` | struct |  | 32229 |
| `CatalogData.BoardExtraGraphicInfo.SpriteData.SizeMode` | enum |  | 32221 |
| `CatalogData.BoardSlotSpecialInfo` | struct |  | 31941 |
| `CatalogData.BoardSlotSpecialInfo.Order` | enum |  | 31931 |
| `CatalogData.BoardSlotSpellInfo` | struct |  | 31983 |
| `CatalogData.ChallengeInfo` | struct |  | 32271 |
| `CatalogData.ChallengeInfo.DescriptionMode` | enum |  | 32254 |
| `CatalogData.ChallengeInfo.ExtraInfo` | enum |  | 32263 |
| `CatchSlotManipulator` | class | `SlotManipulator` | 2685 |
| `ChallengeGlobal` | class |  | 18196 |
| `ChallengeRelic` | class | `MonoBehaviour` | 2701 |
| `ChallengesUI` | class | `MonoBehaviour, IInputTypeResponsive, IIdentifier, IGameplayContextR...` | 43650 |
| `CinderBoard` | class | `BattleBoard` | 2367 |
| `CodeUISlotsNavigation` | class |  | 43802 |
| `CodexSlot` | class | `MonoBehaviour, ISelectableResponsive, IIdentifier` | 44319 |
| `CodexSlot.Size` | enum |  | 44309 |
| `CodexSlotCharacter` | class | `CodexSlot` | 44411 |
| `CodexSlotCharacter.Page` | enum |  | 44370 |
| `CodexSlotDefault` | class | `CodexSlot` | 44527 |
| `CodexSlotEnemy` | class | `CodexSlot, ILocalizable` | 44644 |
| `CodexSlotEnemy.Page` | enum |  | 44597 |
| `CodexSlotEnemy.StatScore` | enum |  | 44585 |
| `CodexSlotPotion` | class | `CodexSlot` | 44738 |
| `CursorDetectionsBlockerController` | class | `MonoBehaviour` | 45186 |
| `DamagePrediction` | class |  | 19755 |
| `Enemy.TurnActions` | class |  | 12601 |
| `Enemy.TurnActions.TurnActionsConditions` | enum |  | 12592 |
| `Enemy.TurnData` | class |  | 12583 |
| `Entity.BlockReductionMode` | enum |  | 15433 |
| `EntityArgs.BlockModificationArgs` | class | `EventArgs` | 11548 |
| `EntityHealth` | class | `MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerE...` | 11075 |
| `EventOptionSlotUI` | class | `MonoBehaviour, ISettingsUIResponsive` | 46140 |
| `EventOptionSlotsData` | class | `EventOptionData` | 36236 |
| `EventOptionSlotsData.ActiveEventOptionSlots` | class | `EventOptionData.ActiveEventOption` | 36216 |
| `EventOptionSlotsData.Slot` | class |  | 36146 |
| `EventOptionSlotsData.Slot.AutoSelectMode` | enum |  | 36112 |
| `EventOptionSlotsData.Slot.PropertyRequirement` | enum |  | 36120 |
| `EventOptionSlotsData.Slot.Result` | enum |  | 36129 |
| `EventOptionSlotsData.Slot.TierRequirement` | enum |  | 36103 |
| `EventOptionSlotsData.Slot.TypeRequirement` | enum |  | 36094 |
| `EventOptionUI.SlotsData` | class |  | 46285 |
| `EventOptionUI.SlotsData.Outcome` | enum |  | 46225 |
| `ExposedSlotManipulator` | class | `SlotManipulatorMultiplicator` | 2540 |
| `FinishMapTurnButtonController` | class | `CustomButtonController, IVisuals, ILocalizable` | 42461 |
| `FinishTurnUI` | class | `MonoBehaviour, IHoverable, IEventsUIResponsive, ISettingsUIResponsi...` | 40626 |
| `Future.BattleChanges.TurnChanges` | class |  | 20282 |
| `GiveDiceBattleAction.Target` | enum |  | 10588 |
| `GrantBlockBattleAction` | class | `BasicBattleAction` | 10454 |
| `GravityPropertyManager.SlotHeightValue` | class |  | 7456 |
| `HasteSlotManipulator` | class | `SlotManipulator` | 2624 |
| `HealActiveData` | class |  | 16297 |
| `HealActiveData.Context` | enum |  | 16280 |
| `HealBattleAction` | class | `BasicBattleAction` | 10491 |
| `HealCounter` | class | `MonoBehaviour` | 11121 |
| `HealRewardData` | class | `RewardData` | 29392 |
| `Hit` | class |  | 17812 |
| `IBattleBoardResponsive` | interface |  | 47793 |
| `IBattleChangeTurnStateResponsive` | interface |  | 19820 |
| `IBattleEndTurnResponsive` | interface |  | 19826 |
| `IBattleTurnBeginningActionsResponsive` | interface |  | 19844 |
| `IBattleTurnStartResponsive` | interface |  | 19850 |
| `IEnemySpecialTarget` | interface |  | 12486 |
| `IEntityDamageBlockedResponsive` | interface |  | 16804 |
| `IEntityDamageReceivedResponsive` | interface |  | 16810 |
| `IEntityHitResponsive` | interface |  | 16822 |
| `IGameObjectDicePlacedOnBoardResponsive` | interface |  | 6885 |
| `IGameObjectDiceRemovedFromBoardResponsive` | interface |  | 7507 |
| `ILocalizeTarget` | class | `ScriptableObject` | 66537 |
| `ILocalizeTargetDescriptor` | class |  | 66560 |
| `ISlotBattleValueUpdatedResponsive` | interface |  | 2473 |
| `InfestSlotAction` | class | `BattleAction` | 10704 |
| `InfestedSlotManipulator` | class | `SlotManipulator` | 2532 |
| `InputTypeBlockerController` | class | `BlockerController` | 27697 |
| `InternalBoostSlotManipulator` | class | `SlotManipulator` | 2513 |
| `LeaderBoardData` | class |  | 39216 |
| `LeakBigSlotManipulator` | class | `SlotManipulator` | 2648 |
| `LeakMediumSlotManipulator` | class | `SlotManipulator` | 2640 |
| `LeakSmallSlotManipulator` | class | `SlotManipulator` | 2632 |
| `LisverBoard` | class | `BattleBoard` | 2400 |
| `MangoBoard` | class | `BattleBoard` | 2413 |
| `MangoBoardAlt` | class | `BattleBoard` | 2429 |
| `MirrorBoardSlot` | class | `BaseBoardSlot` | 2441 |
| `ModifierBasedAttackBattleAction` | class | `BasicBattleAction` | 10779 |
| `MonitoredSlotManipulator` | class | `SlotManipulator` | 2616 |
| `MonitoredTangledSlotManipulator` | class | `SlotManipulator` | 2677 |
| `MultiImageTargetGraphics` | class | `MonoBehaviour` | 48449 |
| `NeraBoard` | class | `BattleBoard` | 2461 |
| `NodeChangeDiceShapeData.TargetMode` | enum |  | 34266 |
| `NodeModifyHealthData` | class | `NodeData` | 35178 |
| `NodeModifyHealthData.ActiveModifyHealthNode` | class | `NodeData.ActiveNode` | 35157 |
| `NodeModifyHealthData.Amount` | enum |  | 35130 |
| `NodeModifyHealthData.Mode` | enum |  | 35120 |
| `NodeSkipToSpecificTile.TargetTile` | enum |  | 35622 |
| `NullifierSlotManipulator` | class | `SlotManipulator` | 2693 |
| `PachinkoSlotManipulator` | class | `SlotManipulator` | 2582 |
| `PriorityHealBattleAction` | class | `HealBattleAction` | 10535 |
| `PropertyTarget` | struct |  | 34849 |
| `ReapedAttackSlotManipulator` | class | `SlotManipulator` | 2600 |
| `ReapedBlockSlotManipulator` | class | `SlotManipulator` | 2608 |
| `ReaperSlotManipulator` | class | `SlotManipulator` | 2592 |
| `RedHoodMonitorEye.BoardPosition` | struct |  | 28430 |
| `RelicSlotManipulator` | class | `SlotManipulator` | 2563 |
| `ShieldAttackBattleAction` | class | `BasicBattleAction` | 10382 |
| `ShiningFireflySlotManipulator` | class | `SlotManipulator` | 2665 |
| `SimpleCombinedKeyboardMouseRemapping.TargetMapping` | struct |  | 64934 |
| `SlotManipulator` | class |  | 2479 |
| `SlotManipulatorMultiplicator` | class | `SlotManipulator` | 2496 |
| `SpecialBasedAttackBattleAction` | class | `BasicBattleAction` | 10733 |
| `SpitedSlotManipulator` | class | `SlotManipulatorMultiplicator` | 2549 |
| `TangledSlotManipulator` | class | `SlotManipulator` | 2524 |
| `TargetEnemyArrow` | class | `MonoBehaviour` | 41908 |
| `TargetEnemySystem` | class | `MonoBehaviour, IExecutionEvents` | 52624 |
| `TerrainSlotManipulator` | class | `SlotManipulator` | 2503 |
| `ThemeSettings.CustomColorBlock` | struct |  | 63788 |
| `TitleScreenChallengeStatusController` | class | `MonoBehaviour, IInputTypeResponsive, IIdentifier` | 53770 |
| `TremblingChairSlotManipulator` | class | `RelicSlotManipulator` | 2575 |
| `TurnBasedModifierData` | class | `ModifierData` | 8080 |
| `Tutorial.Turn` | struct |  | 39656 |
| `VisualChallenge` | class | `MonoBehaviour` | 54357 |
| `VisualEntity.SpecialAttackType` | enum |  | 17470 |

## Dés / deck

| Type | Genre | Hérite / implémente | Ligne |
|---|---|---|---|
| `ActiveDeck` | class |  | 4441 |
| `ActiveDice` | class | `ICloneable` | 5260 |
| `ActiveDice.DiceSuffixMode` | enum |  | 5174 |
| `ActiveDice.FontSizeMode` | enum |  | 5184 |
| `ActiveFace` | class | `ICloneable` | 5461 |
| `ActiveModifyDiceFace` | class |  | 27245 |
| `BackpackUI.DrawDiceMode` | enum |  | 40962 |
| `CatalogData.DiceImageInfo` | struct |  | 31771 |
| `CatalogData.FaceInfo` | struct |  | 31780 |
| `Codex.ItemDiceType` | class | `Codex.Item` | 43938 |
| `Codex.ItemDiceType.Stats` | class | `Codex.Item.BaseStats` | 43919 |
| `DeckData` | class | `ScriptableObject` | 33494 |
| `DeckData.Content` | enum |  | 33474 |
| `DeckData.StartingDice` | struct |  | 33485 |
| `DeckSelectCard` | class | `MonoBehaviour` | 4505 |
| `DecksSelector` | class | `MonoBehaviour` | 4546 |
| `DiceAvatar` | class | `MonoBehaviour` | 5767 |
| `DiceBox` | class | `MonoBehaviour, IExecutionEvents, IPointerClickHandler, IEventSystem...` | 40169 |
| `DiceBox.BoxType` | enum |  | 40120 |
| `DiceBoxDice` | class | `MonoBehaviour, IHoverable, ISettingsUIResponsive, IIdentifier, ILoc...` | 5901 |
| `DiceBoxDice.Context` | enum |  | 5778 |
| `DiceBoxDice.OnClick` | class | `MulticastDelegate` | 5790 |
| `DiceCustomSpriteHolder` | class |  | 5574 |
| `DiceData` | class | `SerializedScriptableObject, ISerializeReferenceByAssetGuid, IEditor...` | 33760 |
| `DiceData.Face` | enum |  | 33662 |
| `DiceData.GeneralType` | enum |  | 33650 |
| `DiceData.Property` | enum |  | 33707 |
| `DiceData.Rarity` | enum |  | 33695 |
| `DiceData.Shape` | enum |  | 33751 |
| `DiceData.Surname` | enum |  | 33736 |
| `DiceData.Tier` | enum |  | 33686 |
| `DiceData.Type` | enum |  | 33607 |
| `DiceDescriptionController` | class | `MonoBehaviour, IUIDisplay, IVisuals, ILocalizable, IIdentifier` | 5681 |
| `DiceDescriptionFace` | class | `MonoBehaviour` | 40271 |
| `DiceDescriptionFace.FaceMode` | enum |  | 40263 |
| `DiceFaceCatalog` | class | `SerializedScriptableObject` | 33940 |
| `DiceFaceCatalog.DiceFacesBySurname` | struct |  | 33921 |
| `DiceFaceCatalog.DiceFacesBySurname.DiceFacesByShape` | struct |  | 33905 |
| `DiceFaceCatalog.DiceFacesByTier` | struct |  | 33894 |
| `DiceFaceGlobal` | class |  | 5494 |
| `DiceFaceHighlighter` | class | `MonoBehaviour` | 40296 |
| `DiceFaces` | class | `ICloneable` | 6054 |
| `DiceInfo` | class |  | 33841 |
| `DiceInfo.BloodDiceBehavior` | enum |  | 33814 |
| `DiceManager.DiceFaceMode` | enum |  | 6215 |
| `DiceManager.DiceList` | class | `ICloneable` | 6168 |
| `DiceManager.DiceListBox` | class | `DiceManager.DiceList` | 6199 |
| `DiceManager.StarterDeckType` | enum |  | 6113 |
| `DiceSelector` | class | `MonoBehaviour, ISelectableResponsive, IExecutionEvents, IIdentifier...` | 40453 |
| `DiceSelector.Context` | enum |  | 40308 |
| `DiceSelector.SkipMode` | enum |  | 40316 |
| `DiceTooltipHolder` | class |  | 53015 |
| `DiceTooltipHolder.DiceTooltipTexts` | class |  | 52999 |
| `DiceTooltipHolder.DiceTooltipValues` | struct |  | 52971 |
| `DiceTypeCount` | class |  | 6799 |
| `DicesCount` | class |  | 6793 |
| `EndScreenDice` | class | `MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerE...` | 6805 |
| `GameObjectDice` | class | `MonoBehaviour, IHoverable, ISettingsUIResponsive, IIdentifier, IRer...` | 7152 |
| `GameObjectDice.CustomAnimation` | enum |  | 6900 |
| `GameObjectDice.CustomAnimationData` | struct |  | 6910 |
| `GameObjectDice.EnergyCostContext` | enum |  | 6917 |
| `GameObjectDiceCustomAnimator` | class | `MonoBehaviour` | 7447 |
| `GiveDiceBattleAction` | class | `BattleAction` | 10616 |
| `HandSymbol` | class | `MonoBehaviour` | 46898 |
| `HandUI` | class | `MonoBehaviour, IExecutionEvents, IDiceBoxResponsive, ISettingsUIRes...` | 40737 |
| `IBattlePreExecuteDiceResponsive` | interface |  | 19838 |
| `IDiceBoxDiceInputResponsive` | interface |  | 20939 |
| `IDiceBoxResponsive` | interface |  | 47805 |
| `IDiceSelectedResponsive` | interface |  | 7495 |
| `IDiceSelectorResponsive` | interface |  | 47850 |
| `IDiceSerialSelectionResponsive` | interface |  | 20945 |
| `IDrawDiceToHandResponsive` | interface |  | 7501 |
| `IGameObjectDiceHoverResponsive` | interface |  | 6879 |
| `IGameObjectDicePlacementResponsive` | interface | `IGameObjectDiceRemovedFromBoardResponsive, IGameObjectDicePlacedOnB...` | 6891 |
| `IGameObjectDiceSelectionResponsive` | interface |  | 6894 |
| `IHandUIContentResponsive` | interface | `IHandUIGameObjectDiceAddedResponsive, IHandUIGameObjectDiceRemovedR...` | 40669 |
| `IHandUIGameObjectDiceAddedResponsive` | interface |  | 40672 |
| `IHandUIGameObjectDiceRemovedResponsive` | interface |  | 40678 |
| `IModifyDiceFacesUIResponsive` | interface |  | 27285 |
| `IObtainableDiceSelectedResponsive` | interface |  | 50114 |
| `MirroringDice` | class | `MonoBehaviour, ISlotBattleValueUpdatedResponsive, IIdentifier` | 7513 |
| `ModifiableDiceFace` | class | `MonoBehaviour, IUpButtonResponsive, IDownButtonResponsive, ISelecta...` | 27312 |
| `ModifyDiceFaceData` | class |  | 27420 |
| `ModifyDiceFaceData.ModifyMode` | enum |  | 27389 |
| `ModifyDiceFaceData.PointsMode` | enum |  | 27405 |
| `ModifyDiceFacesUI` | class | `MonoBehaviour, IDiceSelectedResponsive, IIdentifier, ILocalizable, ...` | 27498 |
| `ModifyDiceFacesUI.Skin` | struct |  | 27475 |
| `ModifyFacesUICustomButton` | class | `CustomButtonController` | 42499 |
| `NodeChangeDiceShapeData` | class | `NodeData` | 34322 |
| `NodeChangeDiceShapeData.ActiveChangeDiceShapeNode` | class | `NodeData.ActiveNode` | 34305 |
| `NodeDiceAmountConditional` | class | `EventOptionData` | 35921 |
| `NodeDiceAmountConditional.ActiveNodeDiceAmountConditional` | class | `EventOptionData.ActiveEventOption, IEventConditional` | 35905 |
| `NodeDiscardDiceData` | class | `NodeData` | 34500 |
| `NodeDiscardDiceData.ActiveDiscardDiceNode` | class | `NodeData.ActiveNode` | 34481 |
| `NodeDiscardDiceData.Mode` | enum |  | 34429 |
| `NodeModifyDiceFaces` | class | `NodeData` | 35108 |
| `NodeModifyDiceFaces.ActiveModifyDiceFacesNode` | class | `NodeData.ActiveNode, IEventConditional` | 35086 |
| `NodeSelectDiceData` | class | `NodeData` | 35401 |
| `NodeSelectDiceData.ActiveSelectDiceNode` | class | `NodeData.ActiveNode` | 35385 |
| `NodeSelectDiceData.Mode` | enum |  | 35357 |
| `NodeTransformDiceData` | class | `NodeData` | 35726 |
| `NodeTransformDiceData.ActiveTransformDiceNode` | class | `NodeData.ActiveNode` | 35712 |
| `NodeTransformDiceData.Mode` | enum |  | 35693 |
| `NodeUpgradeDiceData` | class | `NodeData` | 35807 |
| `NodeUpgradeDiceData.ActiveUpgradeDiceNode` | class | `NodeData.ActiveNode` | 35792 |
| `NodeUpgradeDiceData.Mode` | enum |  | 35744 |
| `ObtainableDice` | class | `MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerE...` | 50170 |
| `ObtainableDiceConfig` | class | `SerializedScriptableObject` | 50302 |
| `ObtainableDiceSelectableCollection` | struct |  | 50324 |
| `ObtainableDiceShadowsConfig` | class | `SerializedScriptableObject` | 50340 |
| `OuterSpriteOutlineHandler` | class | `MonoBehaviour` | 49892 |
| `ReturnDiceToHand` | class | `MonoBehaviour, IPointerClickHandler, IEventSystemHandler` | 41833 |
| `RunestoneDice` | class |  | 38489 |
| `RunestoneDice.Shape` | enum |  | 38474 |
| `RunestoneDiceInfo` | class | `MonoBehaviour` | 31271 |
| `SelectableDiceKnightSkin` | class | `SelectableSkin` | 3035 |
| `SoundHolders.DiceSounds` | class |  | 337 |
| `Tutorial.Dice` | struct |  | 39662 |
| `UnityUIPlayerControllerElementGlyphBase.Result2DSelectionHandler` | class | `MulticastDelegate` | 60727 |
| `UnityUIPlayerControllerElementGlyphBase.ResultSelectionHandler` | class | `MulticastDelegate` | 60718 |
| `UnityUITextMeshProGlyphHelper.IsRewiredElementAllowedHandler` | class | `MulticastDelegate` | 61022 |
| `UnityUITextMeshProGlyphHelper.ParseTagAttributesHandler` | class | `MulticastDelegate` | 60811 |
| `UnityUITextMeshProGlyphHelper.RewiredElementResult2DSelectionHandler` | class | `MulticastDelegate` | 61040 |
| `UnityUITextMeshProGlyphHelper.RewiredElementResultSelectionHandler` | class | `MulticastDelegate` | 61031 |
| `UserDataStore_File.IDataHandler` | interface |  | 58992 |
| `UserDataStore_File.LocalFileDataHandler` | class | `UserDataStore_File.IDataHandler` | 58954 |
| `VisualDice` | class | `MonoBehaviour, IAssertableDisposable, IDisposable, IDisposable, IAs...` | 54685 |
| `VisualDice.BodyContext` | enum |  | 54624 |
| `VisualDice.ShadowVisuals` | struct |  | 54633 |
| `VisualRunestoneDice` | class | `MonoBehaviour` | 31637 |

## Ennemis

| Type | Genre | Hérite / implémente | Ligne |
|---|---|---|---|
| `ActiveMob` | class |  | 11523 |
| `AntEnemy` | class | `Enemy` | 11676 |
| `AudioCaterpillarBoss` | class | `AudioEnemy` | 10074 |
| `AudioEnemy` | class | `AudioEntity` | 10088 |
| `AxeEnemy` | class | `Enemy` | 11776 |
| `AxeEnemy.AxeEnemyActions` | class | `Enemy.BaseActions` | 11695 |
| `BackstabberEnemy` | class | `Enemy` | 11806 |
| `BaseEnemyData` | class | `BaseEntityData` | 33971 |
| `BeeQueenEnemy` | class | `Enemy` | 11892 |
| `BeeQueenEnemy.QueenBeeEnemyActions` | class | `Enemy.BaseActions` | 11815 |
| `BeeWorkerEnemy` | class | `Enemy` | 12078 |
| `BeetballEnemy` | class | `Enemy` | 11998 |
| `BeetballEnemy.BeetballAction` | enum |  | 11951 |
| `BeetballEnemy.BeetballEnemyActions` | class | `Enemy.BaseActions` | 11934 |
| `BeetleEnemy` | class | `Enemy` | 12047 |
| `CatalogData.EnemyInfo` | class |  | 32159 |
| `CatalogData.MapBossTile` | struct |  | 32101 |
| `CaterpillarBossEnemy` | class | `Enemy` | 12118 |
| `CaterpillarBossEnemy.CaterpillarBossEnemyActions` | class | `Enemy.BaseActions` | 12105 |
| `CaterpillarEnemy` | class | `Enemy` | 12138 |
| `CicadaEnemy` | class | `Enemy` | 12176 |
| `CicadaEnemy.CicadaEnemyActions` | class | `Enemy.BaseActions` | 12149 |
| `CicandelaEnemy` | class | `Enemy` | 12308 |
| `CicandelaEnemy.CicandelaEnemyActions` | class | `Enemy.BaseActions` | 12271 |
| `CicandelaEnemy.CreativeBattleAction` | class | `BasicBattleAction` | 12263 |
| `CicandelaEnemy.CrossBattleAction` | class | `CicandelaEnemy.CreativeBattleAction` | 12225 |
| `CicandelaEnemy.PassBattleAction` | class | `CicandelaEnemy.CreativeBattleAction` | 12251 |
| `CicandelaEnemy.SpinBattleAction` | class | `CicandelaEnemy.CreativeBattleAction` | 12237 |
| `CicandelaEnemy.SweepBattleAction` | class | `CicandelaEnemy.CreativeBattleAction` | 12215 |
| `CockroachEnemy` | class | `Enemy` | 12407 |
| `Codex.ItemEnemy` | class | `Codex.Item` | 44093 |
| `Codex.ItemEnemy.Stats` | class | `Codex.Item.BaseStats` | 44046 |
| `CodexEnemyButton` | class | `CustomButtonController, ISkipButtonResponsive` | 42192 |
| `CodexEnemyCreditsButton` | class | `MonoBehaviour, IOutlineable` | 44231 |
| `CodexEnemyCreditsButton.CreditsMode` | enum |  | 44223 |
| `CodexEnemyMarkerPage` | class | `MonoBehaviour, IOutlineable, IInputTypeResponsive, IIdentifier` | 44259 |
| `DragonflyEliteEnemy` | class | `Enemy` | 12433 |
| `DragonflyEliteEnemy.DragonflyEliteEnemyActions` | class | `Enemy.BaseActions` | 12419 |
| `DragonflyMinionEnemy` | class | `Enemy` | 12466 |
| `DragonflyMinionEnemy.DragonflyMinionEnemyActions` | class | `Enemy.BaseActions` | 12452 |
| `Enemy` | class | `Entity` | 12891 |
| `Enemy.ActionOrder` | enum |  | 12575 |
| `Enemy.BaseActions` | class |  | 12558 |
| `Enemy.Name` | enum |  | 12492 |
| `EnemyBehavior` | class | `SerializedMonoBehaviour` | 15407 |
| `EnemyPointer` | class | `MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerE...` | 10810 |
| `EnemyPointer.Mode` | enum |  | 10802 |
| `EnemyVisualsSync` | class | `MonoBehaviour` | 16866 |
| `FireflyEnemy` | class | `Enemy` | 13460 |
| `FlyEnemy` | class | `Enemy` | 13475 |
| `IEnemyButtonResponsive` | interface |  | 20981 |
| `IEnemyDeathResponsive` | interface |  | 14885 |
| `IrascibleEnemy` | class | `Enemy, IEntityDamageReceivedResponsive` | 13492 |
| `JinxipedeEnemy` | class | `Enemy` | 13501 |
| `MageEnemy` | class | `Enemy` | 13591 |
| `MantisEnemyTransformationPhase` | class | `MonoBehaviour` | 13870 |
| `MobData` | class | `ScriptableObject` | 38131 |
| `MobData.SpecialRule` | enum |  | 38123 |
| `MobDataExtensions` | class |  | 38152 |
| `MobsPlaylistData` | class | `ScriptableObject` | 38178 |
| `MobsPlaylistData.Element` | struct |  | 38159 |
| `MobsPlaylistData.StarterRelicData` | struct |  | 38167 |
| `MosquitoDukeEnemy` | class | `Enemy` | 14102 |
| `MosquitoDukeEnemy.Level` | enum |  | 13965 |
| `MothEnemy` | class | `Enemy` | 14179 |
| `NodeChangeBossMapData` | class | `NodeData` | 34256 |
| `NodeChangeBossMapData.ActiveChangeBossMapNode` | class | `NodeData.ActiveNode` | 34245 |
| `NonBossBattleConditional` | class | `EventOptionData` | 35941 |
| `NonBossBattleConditional.ActiveNonBossBattleConditional` | class | `EventOptionData.ActiveEventOption` | 35931 |
| `NunEnemy` | class | `Enemy` | 14185 |
| `PlagueEnemy` | class | `Enemy` | 14302 |
| `PlagueEnemy.PlagueEnemyActions` | class | `Enemy.BaseActions` | 14194 |
| `PropellerEnemy` | class | `Enemy` | 14333 |
| `RainEnemy` | class | `Enemy` | 14696 |
| `RainEnemy.Level` | enum |  | 14349 |
| `RainEnemy.Phase` | enum |  | 14341 |
| `RedHoodEnemy` | class | `Enemy` | 14858 |
| `RedHoodEnemy.RedHoodEnemyActions` | class | `Enemy.BaseActions` | 14811 |
| `RevengerEnemy` | class | `Enemy` | 14891 |
| `ScorpionMountBoss` | class | `Enemy` | 14969 |
| `ScorpionMountBoss.ScorpionMountBossEnemyActions` | class | `Enemy.BaseActions` | 14900 |
| `ScorpionRiderBoss` | class | `Enemy, IEnemySpecialTarget` | 15069 |
| `ScytheMantisEnemy` | class | `MantisEnemy` | 13899 |
| `SentientJewelBeetleEnemy` | class | `Enemy` | 15137 |
| `SickleMantisEnemy` | class | `MantisEnemy` | 13944 |
| `SneakyEnemy` | class | `Enemy, IEnemySpecialTarget` | 15194 |
| `SpiderEnemy` | class | `Enemy, IBattleEndTurnResponsive, IEntityDamageReceivedResponsive, I...` | 15280 |
| `SpitefulEnemy` | class | `Enemy` | 15290 |
| `SpitterEnemy` | class | `Enemy` | 15361 |
| `SporeEnemy` | class | `Enemy` | 15376 |
| `TickEnemy` | class | `Enemy` | 15385 |
| `VisualBossScorpion` | class | `VisualEnemy` | 16978 |
| `VisualCaterpillarBoss` | class | `VisualEnemy` | 17110 |
| `VisualEnemy` | class | `VisualEntity` | 17235 |
| `VisualEnemyBeetball` | class | `VisualEnemy` | 17334 |
| `VisualEnemyMage` | class | `VisualEnemy` | 17367 |
| `VisualEnemyRain` | class | `VisualEnemy` | 17378 |
| `VisualEnemyRedHood` | class | `VisualEnemy` | 17385 |
| `VisualEnemySneaky` | class | `VisualEnemy` | 17430 |
| `VisualScorpionBossPart` | class | `MonoBehaviour` | 17057 |

## Joueur / personnages / compagnons

| Type | Genre | Hérite / implémente | Ligne |
|---|---|---|---|
| `ActiveEntity` | class | `ICloneable` | 11422 |
| `ActiveEntity.PermanentModifier` | class |  | 11403 |
| `AudioEntity` | class | `MonoBehaviour` | 10105 |
| `BabyFlyCompanion` | class | `Companion` | 11568 |
| `BaseCompanionData` | class | `ScriptableObject` | 33959 |
| `BaseEntityData` | class | `ScriptableObject` | 33979 |
| `BaseTooltipBox.SkinData` | struct |  | 52810 |
| `CharacterLevel` | class |  | 21972 |
| `CharacterLevel.Level` | enum |  | 21954 |
| `CharacterNavLegendUI` | class | `MonoBehaviour` | 2737 |
| `CharacterSelectorInfoController` | class | `MonoBehaviour` | 53634 |
| `CharacterSelectorInfoController.LoadMode` | enum |  | 53619 |
| `CharacterSelectorInfoUIManager` | class | `MonoBehaviour, IVisuals, ISelectedCharacterResponsive, IUIDisplay, ...` | 43723 |
| `CharacterSelectorStartButtonController` | class | `CustomButtonController, ILocalizable, ISelectedCharacterResponsive,...` | 42168 |
| `CinderPlayer` | class | `Player` | 16308 |
| `CinderPlushCompanion` | class | `Companion` | 11587 |
| `Codex.ItemCharacter` | class | `Codex.Item` | 43895 |
| `Codex.ItemCharacter.Stats` | class | `Codex.Item.BaseStats` | 43885 |
| `CodexCharacterMarkerPage` | class | `MonoBehaviour, IOutlineable, IInputTypeResponsive, IIdentifier` | 44173 |
| `Companion` | class | `MonoBehaviour` | 11620 |
| `Companion.Name` | enum |  | 11610 |
| `CompanionGlobal` | class |  | 11632 |
| `Entity` | class | `SerializedMonoBehaviour, IIdentifier, IAssertive, IEntityHitRespons...` | 16062 |
| `Entity.Side` | enum |  | 15425 |
| `EntityArgs` | class |  | 11562 |
| `EntityCanvas` | class | `MonoBehaviour, IDiceBoxResponsive, IPrepareForEndlessModeResponsive...` | 10912 |
| `EntityComposedMarker` | class | `EntityMarker` | 11147 |
| `EntityComposedMarker.InfoMode` | enum |  | 11139 |
| `EntityCurrentBoost` | class | `MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerE...` | 11054 |
| `EntityMarker` | class | `MonoBehaviour, IVisuals` | 11235 |
| `EntityMarker.Info` | struct |  | 11208 |
| `EntityMarker.MarkerType` | enum |  | 11223 |
| `EntityMarker.PositionMode` | enum |  | 11215 |
| `ICharacterCompanionButtonResponsive` | interface |  | 20909 |
| `ICharacterSelectionUIResponsive` | interface |  | 2897 |
| `ICharacterSkinButtonResponsive` | interface |  | 20915 |
| `IEntityDeathResponsive` | interface |  | 16816 |
| `ISelectedCharacterResponsive` | interface |  | 2903 |
| `LisverPlayer` | class | `Player` | 16364 |
| `LoadedCharacterInfo` | class |  | 2909 |
| `MangoPlayer` | class | `Player` | 16409 |
| `NeraPlayer` | class | `Player` | 16451 |
| `Player` | class | `Entity, IPointerClickHandler, IEventSystemHandler, IPlayer` | 16735 |
| `Player.CharacterName` | enum |  | 16465 |
| `PlayerFogOfWarDetectionController` | class | `MonoBehaviour` | 26977 |
| `PlayerPointerEventData` | class | `PointerEventData` | 59602 |
| `PlayerSkinData` | class | `ScriptableObject` | 34049 |
| `PlayerSkinData.Name` | enum |  | 34030 |
| `PotionMimicCompanion` | class | `Companion` | 11639 |
| `RelicCharacterSelectController` | class | `MonoBehaviour` | 28683 |
| `SelectableCharacter` | class | `MonoBehaviour` | 2957 |
| `SelectableCharacter.Data` | struct |  | 2933 |
| `SelectableCompanion` | class | `ExtraSelectable` | 3020 |
| `SelectableGoldenWeapons.GoldenWeaponByCharacter` | struct |  | 3050 |
| `SelectableSkin` | class | `ExtraSelectable` | 3081 |
| `SkinGlobal` | class |  | 34059 |
| `SmallBugFromSelectableMango` | class | `MonoBehaviour` | 3099 |
| `Tooltip.Skin` | enum |  | 53089 |
| `TutorialEntity` | class | `Player` | 16828 |
| `UnityUITextMeshProGlyphHelper.TMProSprite_AssetV1_1_0.TMPro_SpriteCharacter` | class |  | 61192 |
| `User.CharacterStats` | class |  | 54521 |
| `VisualEntity` | class | `MonoBehaviour, IIdentifier` | 17589 |

## Modificateurs / statuts

| Type | Genre | Hérite / implémente | Ligne |
|---|---|---|---|
| `AdamantModifierData` | class | `ModifierData` | 9212 |
| `BackstabModifierData` | class | `ModifierData` | 7914 |
| `BarrierModifier` | class | `TurnBasedModifierData` | 8499 |
| `BaseStanceModifierData` | class | `ModifierData` | 8941 |
| `BaseStanceModifierData.Level` | enum |  | 8932 |
| `BonusModifierData` | class | `ModifierData` | 8567 |
| `BoostedModifierData` | class | `ModifierData` | 9097 |
| `BoosterModifier` | class | `ModifierData` | 7900 |
| `BreatheModifierBase` | class |  | 55599 |
| `BreatheModifierPosition` | class | `BreatheModifierBase` | 55629 |
| `BreatheModifierRotation` | class | `BreatheModifierBase` | 55637 |
| `BreatheModifierScale` | class | `BreatheModifierBase` | 55645 |
| `BrewmasterModifierData` | class | `ModifierData` | 9119 |
| `BulwarkModifierData` | class | `ModifierData` | 8925 |
| `CatalogData.ModifierInfo` | struct |  | 31889 |
| `CatalogData.ModifierInfo.DescriptionMode` | enum |  | 31873 |
| `CatalogData.ModifierInfo.NameColorMode` | enum |  | 31864 |
| `CatalogData.PropertyInfo` | struct |  | 31839 |
| `ChargeModifierData` | class | `ModifierData` | 9290 |
| `ChargeModifierData.Mode` | enum |  | 9224 |
| `CorruptionModifierData` | class | `ModifierData` | 9566 |
| `CowardModifierData` | class | `ModifierData` | 9345 |
| `CreativeModifier` | class | `ModifierData` | 9590 |
| `CreativeModifier.CreativeActions` | enum |  | 9579 |
| `DisruptibleModifier` | class | `ModifierData` | 8049 |
| `DropdownTemplateModifier` | class | `MonoBehaviour` | 45829 |
| `EnergizerModifierData` | class | `TurnBasedModifierData` | 8608 |
| `ExtraModifierData` | class | `ModifierData` | 9107 |
| `FortifyModifierData` | class | `ModifierData` | 7907 |
| `GenerousModifier` | class | `ModifierData` | 8643 |
| `GravityPropertyManager` | class |  | 7479 |
| `GreedyModifierData` | class | `ModifierData` | 9376 |
| `HastyModifierData` | class | `ModifierData` | 8773 |
| `HighTempoModifier` | class | `ModifierData` | 9597 |
| `InputBehaviorWindow.PropertyType` | enum |  | 63190 |
| `LanguageData.ModifierKeys` | class |  | 63306 |
| `LazyModifierData` | class | `ModifierData` | 8918 |
| `LuckyModifierData` | class | `ModifierData` | 8578 |
| `MachinationModifierData` | class | `ModifierData` | 9164 |
| `MartyrModifierData` | class | `ModifierData` | 8378 |
| `MightModifierData` | class | `ModifierData` | 8908 |
| `ModifierData` | class |  | 7720 |
| `ModifierData.Modifier` | enum |  | 7582 |
| `ModifierData.Preview` | struct |  | 7572 |
| `ModifierData.ReduceMode` | enum |  | 7560 |
| `ModifierExt` | class |  | 9727 |
| `MonitorModifierData` | class | `ModifierData, IMantisBehaviour` | 7872 |
| `MonitorModifierData.Phase` | enum |  | 7846 |
| `MonitorTangledModifierData` | class | `ModifierData` | 9176 |
| `NarcissistModifierData` | class | `ModifierData` | 8263 |
| `NeutralizerModifierData` | class | `ModifierData` | 8511 |
| `NodeAddPermanentModifier` | class | `NodeData` | 34153 |
| `NodeAddPermanentModifier.ActiveAddPermanentModifierNode` | class | `NodeData.ActiveNode` | 34136 |
| `NodeAddPermanentModifier.Mode` | enum |  | 34102 |
| `NodeAddPermanentModifier.PermanentModifierInfo` | struct |  | 34109 |
| `NodeGrantPropertyData` | class | `NodeData` | 34991 |
| `NodeGrantPropertyData.ActiveGrantPropertyNode` | class | `NodeData.ActiveNode` | 34972 |
| `OpulentModifierData` | class | `ModifierData` | 9405 |
| `ParryModifierData` | class | `ModifierData` | 9461 |
| `PerceptionModifier` | class | `ModifierData` | 9689 |
| `PerceptionModifier.State` | enum |  | 9606 |
| `PestilenceModifierData` | class | `ModifierData` | 7960 |
| `PettyModifierData` | class | `ModifierData` | 8195 |
| `PickpocketModifierData` | class | `ModifierData` | 9308 |
| `PoisonModifierData` | class | `TurnBasedModifierData` | 8143 |
| `PoisonousModifierData` | class | `ModifierData` | 8893 |
| `PresenceModifierData` | class | `ModifierData, IDisposable, IDisposable` | 7812 |
| `PropertyIconConfig` | class | `SerializedScriptableObject` | 38529 |
| `PropertySpriteInfo` | class |  | 5526 |
| `PropertyStyle` | enum |  | 34841 |
| `PureModifierData` | class | `ModifierData` | 9148 |
| `QuickCastModifier` | class | `TurnBasedModifierData` | 8791 |
| `RageModifierData` | class | `ModifierData` | 8217 |
| `ReapedModifierData` | class | `ModifierData, IMantisBehaviour` | 8421 |
| `ReapedModifierData.Phase` | enum |  | 8386 |
| `RegenerationModifier` | class | `TurnBasedModifierData` | 8469 |
| `ResilientModifierData` | class | `ModifierData` | 7794 |
| `RetaliateModifierData` | class | `ModifierData` | 8625 |
| `SedentaryModifier` | class | `ModifierData` | 8759 |
| `SpawnerModifier` | class | `ModifierData` | 8719 |
| `SpiteModifierData` | class | `ModifierData` | 8303 |
| `StanceParryModifierData` | class | `BaseStanceModifierData` | 9069 |
| `StanceSweepModifierData` | class | `BaseStanceModifierData` | 9016 |
| `StanceThrustModifierData` | class | `BaseStanceModifierData` | 8973 |
| `SuperTastyModifierData` | class | `ModifierData, IIdentifier` | 9536 |
| `SuperTastyModifierData.Mode` | enum |  | 9477 |
| `TanglerModifierData` | class | `ModifierData` | 8550 |
| `TastyModifierData` | class | `ModifierData, IIdentifier` | 8006 |
| `TastyModifierData.Mode` | enum |  | 7972 |
| `ThornsModifierData` | class | `TurnBasedModifierData` | 8339 |
| `TimidModifier` | class | `ModifierData` | 8798 |
| `TombstoneModifierData` | class | `ModifierData` | 7804 |
| `VileModifierData` | class | `ModifierData` | 7836 |
| `VisualModifier` | class | `MonoBehaviour, IEntityDeathResponsive, IIdentifier, IDisposable, ID...` | 9733 |
| `VoltModifierManager` | class | `IGameObjectDicePlacementResponsive, IGameObjectDiceRemovedFromBoard...` | 9978 |
| `VoltageModifier` | class | `ModifierData` | 8830 |
| `WeakModifierData` | class | `ModifierData` | 9087 |

## Reliques / malédictions

| Type | Genre | Hérite / implémente | Ligne |
|---|---|---|---|
| `ActiveRelic` | class |  | 28507 |
| `Codex.ItemRelic` | class | `Codex.Item` | 43979 |
| `Codex.ItemRelic.Stats` | class | `Codex.Item.BaseStats` | 43962 |
| `IRelicSelectorResponsive` | interface |  | 47856 |
| `NodeDiscardRelicOrCurseData` | class | `NodeData` | 34697 |
| `NodeDiscardRelicOrCurseData.ActiveDiscardRelicOrCurseNode` | class | `NodeData.ActiveNode` | 34678 |
| `NodeDiscardRelicOrCurseData.Mode` | enum |  | 34618 |
| `NodeDiscardRelicOrCurseData.Type` | enum |  | 34626 |
| `NodeDuplicateRelicNode` | class | `NodeData` | 34744 |
| `NodeDuplicateRelicNode.ActiveDuplicateRelicNode` | class | `NodeData.ActiveNode` | 34727 |
| `NodeDuplicateRelicNode.Mode` | enum |  | 34719 |
| `NodeSelectRelicData` | class | `NodeData` | 35573 |
| `NodeSelectRelicData.ActiveSelectRelicNode` | class | `NodeData.ActiveNode` | 35551 |
| `NodeSelectRelicData.Mode` | enum |  | 35513 |
| `ObtainableRelic` | class | `MonoBehaviour, IOutlineable, IHoverable, IShopItem` | 28611 |
| `ObtainableRelic.Context` | enum |  | 28573 |
| `ObtainableRelic.RarityInfo` | struct |  | 28563 |
| `RelicData` | class | `SerializedScriptableObject` | 38848 |
| `RelicData.Category` | enum |  | 38831 |
| `RelicData.ConditionNumber` | enum |  | 38822 |
| `RelicData.Rarity` | enum |  | 38800 |
| `RelicData.Style` | enum |  | 38813 |
| `RelicData.Title` | enum |  | 38591 |
| `RelicData.Type` | enum |  | 38792 |
| `RelicData.UseMode` | enum |  | 38583 |
| `RelicData.VisualEffectData` | struct |  | 38560 |
| `RelicExtensions` | class |  | 38938 |
| `RelicExtensions.ConditionInfo` | struct |  | 38917 |
| `RelicMarker` | class | `MonoBehaviour` | 50076 |
| `RelicMarker.EffectInfo` | struct |  | 50068 |
| `RelicMarker.EffectInfo.Type` | enum |  | 50055 |
| `RelicSelector` | class | `MonoBehaviour, IExecutionEvents, IGameplayContextResponsive, IIdent...` | 41557 |
| `RelicSelector.SkipMode` | enum |  | 41454 |
| `Relics.ContextFrom` | enum |  | 28724 |
| `RelicsOrCursesAmountConditional` | class | `EventOptionData` | 35959 |
| `RelicsOrCursesAmountConditional.ActiveRelicsOrCursesAmountConditional` | class | `EventOptionData.ActiveEventOption` | 35948 |
| `RelicsUI` | class | `MonoBehaviour, IExecutionEvents, IGameManagerStateResponsive, IIden...` | 41686 |
| `RelicsUI.Context` | enum |  | 41640 |
| `VisualRelic` | class | `MonoBehaviour, IEventsUIResponsive, ISettingsUIResponsive, IPrepare...` | 29053 |

## Potions / runes

| Type | Genre | Hérite / implémente | Ligne |
|---|---|---|---|
| `ActivePotion` | class | `ICloneable` | 31070 |
| `Codex.ItemPotion` | class | `Codex.Item` | 44021 |
| `Codex.ItemPotion.Stats` | class | `Codex.Item.BaseStats` | 44004 |
| `IPotionsUIResponsive` | interface |  | 47832 |
| `NodeDiscardPotionData` | class | `NodeData` | 34603 |
| `NodeDiscardPotionData.ActiveDiscardPotionNode` | class | `NodeData.ActiveNode` | 34588 |
| `NodeDiscardPotionData.Mode` | enum |  | 34521 |
| `NodeReplacePotionData` | class | `NodeData` | 35341 |
| `NodeReplacePotionData.ActiveReplacePotionNode` | class | `NodeData.ActiveNode` | 35326 |
| `NodeReplacePotionData.Mode` | enum |  | 35258 |
| `NodeSelectPotionData` | class | `NodeData` | 35487 |
| `NodeSelectPotionData.ActiveSelectPotionNode` | class | `NodeData.ActiveNode` | 35473 |
| `NodeSelectPotionData.Mode` | enum |  | 35425 |
| `ObtainableRunestone` | class | `MonoBehaviour, IOutlineable, IHoverable, IShopItem` | 31143 |
| `ObtainableRunestone.Context` | enum |  | 31108 |
| `ObtainableRunestone.RarityInfo` | struct |  | 31098 |
| `PotionData` | class | `ScriptableObject` | 38432 |
| `PotionData.ActionCondition` | class |  | 38344 |
| `PotionData.ActionCondition.Condition` | enum |  | 38333 |
| `PotionData.ActionResult` | class |  | 38407 |
| `PotionData.ActionResult.Effect` | enum |  | 38355 |
| `PotionData.ActionResult.ValueMode` | enum |  | 38399 |
| `PotionData.Rarity` | enum |  | 38323 |
| `PotionData.Title` | enum |  | 38259 |
| `RunestoneGlobal` | class |  | 38505 |
| `RunestoneSelector` | class | `MonoBehaviour, IExecutionEvents, IAttachedTooltip, IGameplayContext...` | 31488 |
| `Runestones.ContextFrom` | enum |  | 31306 |
| `RunestonesUI` | class | `MonoBehaviour, IToggleOnInputResponsive` | 51600 |
| `RunestonesUI.RollData` | struct |  | 51541 |
| `SpellData` | class |  | 9838 |
| `SpellData.Name` | enum |  | 9830 |
| `VisualRunestone` | class | `MonoBehaviour, IToggleOnInputResponsive` | 31584 |
| `WeakSpotSpellData` | class | `SpellData` | 9884 |

## Boutique / monnaie / récompenses

| Type | Genre | Hérite / implémente | Ligne |
|---|---|---|---|
| `AvailableRewards` | class | `ResourceObject, IAssertive` | 29349 |
| `CurrencyData` | class | `ScriptableObject, ICurrencyData` | 3292 |
| `CurrencyRewardData` | class | `RewardData` | 29371 |
| `CurrencyUI` | class | `MonoBehaviour, IVisuals, IIdentifier, ILocalizable, IExecutionEvents` | 3493 |
| `CurrencyVariationArgs` | class | `EventArgs` | 3568 |
| `Future.ShopChanges` | class |  | 20334 |
| `ICurrencyData` | interface |  | 3589 |
| `ICurrencyData.VariationModeExecution` | enum |  | 3581 |
| `ICurrencyRemovedResponsive` | interface |  | 3616 |
| `ICurrencyUIRefreshResponsive` | interface |  | 3622 |
| `IEndsShopResponsive` | interface |  | 51854 |
| `IRewardData` | interface |  | 29398 |
| `IRewardPressedChangeResponsive` | interface |  | 29682 |
| `IRewardsResponsive` | interface |  | 47838 |
| `IShopCycleResponsive` | interface | `IStartShopResponsive, IEndsShopResponsive` | 51860 |
| `IShopItem` | interface |  | 51863 |
| `IShopItemBoughtResponsive` | interface |  | 51876 |
| `IShopUIResponsive` | interface |  | 51882 |
| `IStartShopResponsive` | interface |  | 51888 |
| `LootController` | class | `MonoBehaviour` | 20270 |
| `MapBaseTileData.BaseTileInfo.RewardMode` | enum |  | 37377 |
| `MapGenericShopTileData` | class | `MapBaseTileData` | 37863 |
| `MapGenericShopTileData.GenericShopTileInfo` | class | `ShopTileInfo` | 37851 |
| `MapTreasureTileData` | class | `MapBaseTileData` | 37946 |
| `MapTreasureTileData.TreasureTileInfo` | class | `MapBaseTileData.BaseTileInfo` | 37925 |
| `NodeBattleRewardsData` | class | `NodeData` | 34235 |
| `NodeBattleRewardsData.ActiveBattleRewardsNode` | class | `NodeData.ActiveNode` | 34223 |
| `NodeCurrencyAmountConditional` | class | `EventOptionData` | 35886 |
| `NodeCurrencyAmountConditional.ActiveNodeCurrencyAmountConditional` | class | `EventOptionData.ActiveEventOption` | 35876 |
| `NodeCurrencyAmountConditional.Mode` | enum |  | 35867 |
| `NodeModifyCurrencyData` | class | `NodeData` | 35054 |
| `NodeModifyCurrencyData.ActiveModifyCurrencyNode` | class | `NodeData.ActiveNode` | 35037 |
| `NodeModifyCurrencyData.Mode` | enum |  | 35011 |
| `RewardData` | class | `SerializedScriptableObject, IRewardData` | 29554 |
| `RewardData.RerollMode` | enum |  | 29545 |
| `RewardData.SpriteContext` | enum |  | 29463 |
| `RewardData.Type` | enum |  | 29471 |
| `RewardValidator` | class |  | 29688 |
| `RewardsRow` | struct |  | 29666 |
| `RewardsUI` | class | `MonoBehaviour, IExecutionEvents, IGameplayContextResponsive, IIdent...` | 51298 |
| `RewardsUI.Source` | enum |  | 50369 |
| `ShopController` | class | `MonoBehaviour, ICurrencyRemovedResponsive, IIdentifier, IExecutionE...` | 52003 |
| `ShopItemVariationArgs` | class | `EventArgs` | 52234 |
| `ShopItems` | class | `ResourceObject, IIdentifier, ICloneable` | 52174 |
| `ShopTileInfo` | class | `MapBaseTileData.BaseTileInfo` | 37987 |
| `ShopTileInfo.Mode` | enum |  | 37956 |
| `ShopUI` | class | `MonoBehaviour, IShopCycleResponsive, IStartShopResponsive, IEndsSho...` | 52406 |
| `ShopUIConfig` | class |  | 52534 |
| `ShopUIConfig.ShopConfigType` | enum |  | 52508 |
| `VisualCurrency` | class | `MonoBehaviour, IVisuals` | 3708 |
| `VisualReward.Context` | enum |  | 29697 |

## Carte / donjons

| Type | Genre | Hérite / implémente | Ligne |
|---|---|---|---|
| `ActiveMapChunk` | class | `ResourceObject, IMapPlayerLocation, IMapLocationObserver, IIdentifier` | 25254 |
| `ActiveMapChunk.Data` | class | `ICustomRNG, ISeedable` | 25213 |
| `ActiveMapChunk.Data.Path` | class | `ICustomRNG, ISeedable` | 25192 |
| `ActiveMapChunk.Data.PathData` | struct |  | 25165 |
| `ActiveMapChunk.Params` | class | `ICustomRNG, ISeedable` | 25139 |
| `ActiveMapChunk.Params.PathData` | struct |  | 25112 |
| `ActiveMapChunk.Params.PathMode` | enum |  | 25122 |
| `ActiveMapChunk.Params.ProceduralTileData` | struct | `ISeedable` | 25130 |
| `ActiveMapTile` | class | `ICustomRNG, ISeedable` | 25310 |
| `ArabicMapping` | class |  | 67154 |
| `AreaTitleUI` | class | `MonoBehaviour` | 40092 |
| `AreaTitleUI.Data` | struct |  | 40028 |
| `BackgroundVariation.ValuesPerArea` | class |  | 615 |
| `CanvasMap` | class | `MonoBehaviour, IIdentifier, ILocalizable, IMapUIResponsive, IDragRe...` | 43066 |
| `CatalogData.DungeonInfo` | struct |  | 32109 |
| `CatalogData.MapAreaInfo` | struct | `ICustomRNG, ISeedable` | 32027 |
| `CatalogData.MapTileInfo` | struct |  | 32060 |
| `CatalogData.MapTileInfo.BridgeSprite` | struct |  | 32044 |
| `ControllerElementGlyphSelectorOptions.ControllerMapSelector` | struct |  | 60095 |
| `DungeonController` | class | `MonoBehaviour` | 19940 |
| `DungeonController.ActiveDungeon` | class |  | 19901 |
| `DungeonPathIcon` | class | `MonoBehaviour` | 45839 |
| `DungeonPathUI` | class | `MonoBehaviour, ILocalizable` | 45931 |
| `ForceTileTrigger` | class |  | 25395 |
| `ForceTileTrigger.PositionData` | class |  | 25386 |
| `Future.MapChanges` | class |  | 20348 |
| `GlyphTools.DefaultControllerMapCache` | class |  | 60434 |
| `GlyphTools.DefaultControllerMapCache.Entry` | class |  | 60422 |
| `GlyphTools.DefaultControllerMapCache.Selector` | struct |  | 60411 |
| `Grid.TileBlueprint` | class |  | 25469 |
| `Grid.TileToExpand` | class |  | 25534 |
| `Grid.TileToReplace` | class |  | 25544 |
| `IExecuteTileActionResponsive` | interface |  | 26808 |
| `IFinishTileActionResponsive` | interface |  | 26814 |
| `IMapButtonResponsive` | interface |  | 21017 |
| `IMapLocationObserver` | interface |  | 37218 |
| `IMapPathSelectionResponsive` | interface |  | 25823 |
| `IMapPlayerLocation` | interface |  | 37225 |
| `IMapSectionData` | interface |  | 37237 |
| `IMapStateChangeResponsive` | interface |  | 25829 |
| `IMapUIResponsive` | interface |  | 41335 |
| `ITileButtonResponsive` | interface |  | 21107 |
| `Map.MapState` | enum |  | 25849 |
| `Map.MoveData` | class |  | 25865 |
| `Map.Part` | enum |  | 25887 |
| `Map.TilesRecord` | class |  | 25835 |
| `MapAlchemyTileData` | class | `MapBaseTileData` | 37365 |
| `MapAlchemyTileData.AlchemyTileInfo` | class | `MapBaseTileData.BaseTileInfo` | 37333 |
| `MapAlchemyTileData.AlchemyTileInfo.Mode` | enum |  | 37324 |
| `MapAreaData` | class | `SerializedScriptableObject` | 36956 |
| `MapAreaData.GenerationMode` | enum |  | 36867 |
| `MapAreaData.OverrideColumn` | struct |  | 36902 |
| `MapAreaData.Position` | enum |  | 36875 |
| `MapAreaData.SectionLength` | struct |  | 36895 |
| `MapAreaData.Title` | enum |  | 36885 |
| `MapAreaGlobal` | class |  | 36999 |
| `MapBaseTileData` | class | `SerializedScriptableObject, ISeedable` | 37457 |
| `MapBaseTileData.BaseTileInfo` | class | `ICloneable, ISeedable` | 37395 |
| `MapBaseTileData.BaseTileInfo.UnknownMode` | enum |  | 37386 |
| `MapBaseTileData.Type` | enum |  | 37431 |
| `MapBattleTileData` | class | `MapBaseTileData` | 37544 |
| `MapBattleTileData.BattleTileInfo` | class | `MapBaseTileData.BaseTileInfo` | 37505 |
| `MapBattleTileData.BattleTileInfo.Mode` | enum |  | 37480 |
| `MapBridgeTileData` | class | `MapBaseTileData` | 37588 |
| `MapBridgeTileData.BridgeTileInfo` | class | `MapBaseTileData.BaseTileInfo` | 37569 |
| `MapBridgeTileData.Title` | enum |  | 37554 |
| `MapButtonUI` | class | `MonoBehaviour, IHoverable, IStartShopResponsive, IEndsShopResponsiv...` | 41348 |
| `MapChunkData` | class | `SerializedScriptableObject` | 37087 |
| `MapChunkData.Path` | class |  | 37070 |
| `MapChunkData.PathFinder` | class |  | 37052 |
| `MapChunkData.TileInChunk` | class |  | 37011 |
| `MapDecorationChunks` | class |  | 26384 |
| `MapDecorationSurface` | class | `MonoBehaviour` | 26404 |
| `MapDiscardTileData` | class | `MapBaseTileData` | 37613 |
| `MapDiscardTileData.DiscardTileInfo` | class | `MapBaseTileData.BaseTileInfo` | 37600 |
| `MapDrag` | class | `MonoBehaviour, IToggleInputResponsive, IToggleOnInputResponsive, IT...` | 26462 |
| `MapDragPointer` | class | `MonoBehaviour, IPointerDownHandler, IEventSystemHandler, IPointerUp...` | 26519 |
| `MapDungeonTileData` | class | `MapBaseTileData` | 37688 |
| `MapDungeonTileData.DungeonTileInfo` | class | `MapBaseTileData.BaseTileInfo` | 37649 |
| `MapDungeonTileData.DungeonTileInfo.BuildMode` | enum |  | 37641 |
| `MapDungeonTileData.DungeonTileInfo.DungeonMode` | enum |  | 37623 |
| `MapDungeonTileData.DungeonTileInfo.Title` | enum |  | 37631 |
| `MapEliteBattleTileData` | class | `MapBaseTileData` | 37748 |
| `MapEliteBattleTileData.EliteBattleTileInfo` | class | `MapBaseTileData.BaseTileInfo` | 37717 |
| `MapEliteBattleTileData.EliteBattleTileInfo.Mode` | enum |  | 37701 |
| `MapEventTileData` | class | `MapBaseTileData` | 37819 |
| `MapEventTileData.EventTileInfo` | class | `MapBaseTileData.BaseTileInfo` | 37786 |
| `MapEventTileData.EventTileInfo.Mode` | enum |  | 37758 |
| `MapFogWar` | class | `IIdentifier` | 37104 |
| `MapForgeTileData` | class | `MapBaseTileData` | 37841 |
| `MapForgeTileData.ForgeTileInfo` | class | `ShopTileInfo` | 37829 |
| `MapImproveTileData` | class | `MapBaseTileData` | 37886 |
| `MapImproveTileData.ImproveTileInfo` | class | `MapBaseTileData.BaseTileInfo` | 37873 |
| `MapMaskOffset` | class |  | 26530 |
| `MapMoveTileData` | class | `MapBaseTileData` | 38092 |
| `MapMoveTileData.MoveTileInfo` | class | `MapBaseTileData.BaseTileInfo` | 38065 |
| `MapMoveTileData.MoveTileInfo.Mode` | enum |  | 38050 |
| `MapPlayerToken` | class | `MapToken, IPlayer` | 26662 |
| `MapRestTileData` | class | `MapBaseTileData` | 37909 |
| `MapRestTileData.RestTileInfo` | class | `MapBaseTileData.BaseTileInfo` | 37896 |
| `MapSectionData` | class | `SerializedScriptableObject, IMapSectionData, ICustomRNG, ISeedable` | 37290 |
| `MapSectionData.ChunkMode` | enum |  | 37260 |
| `MapSectionData.Name` | enum |  | 37269 |
| `MapTileArrow` | class | `MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerE...` | 26688 |
| `MapTileExt` | class |  | 37919 |
| `MapTileGlobal` | class |  | 37471 |
| `MapTileLine` | class | `MonoBehaviour` | 26724 |
| `MapTileLine.LineDirection` | enum |  | 26705 |
| `MapToken` | class | `MonoBehaviour` | 26937 |
| `MapToken.MovementType` | enum |  | 26909 |
| `MapUnknownTileData` | class | `MapBaseTileData` | 38113 |
| `MapUnknownTileData.UnknownTileInfo` | class | `MapBaseTileData.BaseTileInfo` | 38102 |
| `MapVisuals` | class | `MonoBehaviour, IIdentifier` | 26757 |
| `MultipleNodeData.NodeByArea` | struct |  | 34075 |
| `MyPlatformHardwareJoystickMapPlatformMap` | class | `HardwareJoystickMapCustomPlatformMapSO` | 65352 |
| `MyPlatformHardwareJoystickMapPlatformMap.MatchingCriteria` | class | `HardwareJoystickMapCustomPlatformMap.MatchingCriteria` | 65340 |
| `MyPlatformHardwareJoystickMapPlatformMap.PlatformMap` | class | `MyPlatformHardwareJoystickMapPlatformMap.PlatformMapBase` | 65330 |
| `NodeCompletePromiseBridgeMapData` | class | `NodeData` | 34349 |
| `NodeCompletePromiseBridgeMapData.ActiveCompletePromiseBridgeMapNode` | class | `NodeData.ActiveNode` | 34338 |
| `NodeData.RollData.RollByArea` | struct |  | 34385 |
| `NodeSkipMapTiles` | class | `NodeData` | 35611 |
| `NodeSkipMapTiles.ActiveSkipMapTilesNode` | class | `NodeData.ActiveNode` | 35600 |
| `NodeSkipToSpecificTile` | class | `NodeData` | 35640 |
| `NodeSkipToSpecificTile.ActiveSkipToSpecificTileNode` | class | `NodeData.ActiveNode` | 35629 |
| `NodeVerticalMapTiles` | class | `NodeData` | 35844 |
| `NodeVerticalMapTiles.ActiveVerticalMapTilesNode` | class | `NodeData.ActiveNode` | 35833 |
| `PathArea` | class | `MonoBehaviour` | 675 |
| `PropPath` | struct |  | 744 |
| `SoundHolders.MapOverworldSounds` | class |  | 376 |
| `SpawnArea` | class | `MonoBehaviour` | 19856 |
| `TileHiderController` | class | `MonoBehaviour, IIdentifier` | 26864 |
| `TileShortcutOption` | enum |  | 26899 |
| `UserDataStore_KeyValue.ControllerElementByRoleMap` | class |  | 59118 |
| `UserDataStore_KeyValue.ControllerElementByRoleMap.Entry` | struct |  | 59105 |
| `UserDataStore_PlayerPrefs.ControllerElementByRoleMap` | class |  | 59377 |
| `UserDataStore_PlayerPrefs.ControllerElementByRoleMap.Entry` | struct |  | 59364 |
| `VisualMapTile` | class | `MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerE...` | 27115 |

## Événements / dialogues

| Type | Genre | Hérite / implémente | Ligne |
|---|---|---|---|
| `ActiveEvent` | class |  | 17768 |
| `BaseDialog` | class | `ScriptableObject` | 33532 |
| `BaseDialog.ExtraData` | class |  | 33520 |
| `BaseLine` | class | `ScriptableObject` | 33573 |
| `BaseLine.NextLineConditions` | enum |  | 33564 |
| `CatalogData.EventInfo` | struct |  | 32120 |
| `CinematicData` | class | `SerializedScriptableObject` | 32733 |
| `CinematicData.Title` | enum |  | 32704 |
| `CinematicData.Type` | enum |  | 32723 |
| `CinematicGlobal` | class |  | 32745 |
| `CinematicPlayer.FailReason` | enum |  | 3109 |
| `CinematicRequest` | class |  | 3203 |
| `ControlRemappingDemo1.DialogHelper` | class |  | 64270 |
| `ControlRemappingDemo1.DialogHelper.DialogType` | enum |  | 64258 |
| `DialogFX` | class |  | 4598 |
| `DialogFX.BaseData` | class |  | 4592 |
| `DialogSystem` | class | `MonoBehaviour, IToggleOnInputResponsive, IIdentifier, IInputTypeRes...` | 4998 |
| `DialogSystem.TextSpeed` | enum |  | 4765 |
| `EventCallback` | class |  | 65638 |
| `EventConditionalOptionData` | class | `EventOptionData` | 35991 |
| `EventConditionalOptionData.ActiveEventConditionalOption` | class | `EventOptionData.ActiveEventOption` | 35975 |
| `EventController` | class | `MonoBehaviour, IIdentifier` | 19982 |
| `EventController.BattleFromEvent` | class |  | 19967 |
| `EventController.SystemMode` | enum |  | 19959 |
| `EventData` | class | `ScriptableObject` | 36313 |
| `EventData.Group` | enum |  | 36253 |
| `EventData.Mood` | enum |  | 36260 |
| `EventData.Title` | enum |  | 36268 |
| `EventGlobal` | class |  | 36326 |
| `EventNodesData` | class | `ScriptableObject` | 36333 |
| `EventOptionData` | class | `ScriptableObject` | 36066 |
| `EventOptionData.ActiveEventOption` | class |  | 36048 |
| `EventOptionData.OutcomeExplanation` | class |  | 36012 |
| `EventOptionData.OutcomeExplanation.Mode` | enum |  | 36002 |
| `EventOptionData.SimpleCondition` | enum |  | 36036 |
| `EventOptionGlobal` | class |  | 36088 |
| `EventOptionUI` | class | `MonoBehaviour, ISettingsUIResponsive, IOptionClickResponsive, IIden...` | 46396 |
| `EventOutcomeTooltipBox` | class | `BaseTooltipBox` | 53042 |
| `EventsUI` | class | `MonoBehaviour, IExecutionEvents, IIdentifier, IInputTypeResponsive` | 46565 |
| `Future.EventChanges` | class |  | 20320 |
| `HoverEvents` | class | `MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerE...` | 47180 |
| `ICinematicResponsive` | interface |  | 3215 |
| `IDialogueFocusModResponsive` | interface |  | 20933 |
| `IEventConditional` | interface |  | 35854 |
| `IEventsUIResponsive` | interface |  | 47823 |
| `JSONNode` | class |  | 67292 |
| `LineRendererExt` | class |  | 55251 |
| `MultipleNodeData` | class | `NodeData` | 34082 |
| `MultipleNodeData.Mode` | enum |  | 34067 |
| `NewsData.Update.LineChange` | class |  | 38195 |
| `NewsLineChange` | class | `MonoBehaviour` | 49676 |
| `NodeBattleData` | class | `NodeData` | 34199 |
| `NodeBattleData.ActiveBattleNode` | class | `NodeData.ActiveNode` | 34186 |
| `NodeBattleData.Mode` | enum |  | 34168 |
| `NodeBattleData.SectionMode` | enum |  | 34177 |
| `NodeData` | class | `ScriptableObject` | 34413 |
| `NodeData.ActiveNode` | class |  | 34359 |
| `NodeData.RollData` | struct |  | 34392 |
| `NodeData.RollData.Mode` | enum |  | 34375 |
| `NodeEliteBattleData` | class | `NodeData` | 34791 |
| `NodeEliteBattleData.ActiveEliteBattleNode` | class | `NodeData.ActiveNode` | 34778 |
| `NodeEliteBattleData.Mode` | enum |  | 34761 |
| `NodeEliteBattleData.SectionMode` | enum |  | 34769 |
| `NodeFinishData` | class | `NodeData` | 34825 |
| `NodeFinishData.ActiveFinishNode` | class | `NodeData.ActiveNode` | 34815 |
| `NodeRemovePropertiesData` | class | `NodeData` | 35238 |
| `NodeRemovePropertiesData.ActiveRemovePropertiesNode` | class | `NodeData.ActiveNode` | 35225 |
| `NodeRemovePropertiesData.Mode` | enum |  | 35198 |
| `NodeTextData` | class | `NodeData` | 35666 |
| `NodeTextData.ActiveTextNode` | class | `NodeData.ActiveNode` | 35651 |
| `PointerEventType` | enum |  | 59777 |
| `Tooltip.Line` | struct |  | 53106 |

## Succès / Steam / DLC

| Type | Genre | Hérite / implémente | Ligne |
|---|---|---|---|
| `AchievementTracker` | class |  | 39108 |
| `Achievements` | class |  | 39091 |
| `Achievements.Title` | enum |  | 39025 |
| `CatalogData.AchievementInfo` | class |  | 32212 |
| `Codex.Item.BaseStats` | class |  | 43848 |
| `DLCManager` | class |  | 10017 |
| `DLCManager.TitleDLC` | enum |  | 10008 |
| `IEndStatsButtonResponsive` | interface |  | 20975 |
| `SteamLeaderboard` | class |  | 39277 |
| `SteamManager` | class | `MonoBehaviour` | 39410 |
| `SteamScript` | class | `MonoBehaviour` | 39432 |
| `SteamStats` | class |  | 39399 |
| `User.ItemsStats` | class |  | 54542 |
| `User.UserStats` | class | `IRunFlow` | 54424 |

## Audio

| Type | Genre | Hérite / implémente | Ligne |
|---|---|---|---|
| `AudioChannel` | class |  | 265 |
| `AudioPlayer` | class | `AudioEntity` | 10137 |
| `AudioQueenBee` | class | `AudioEnemy` | 10145 |
| `ButtonAudio` | class | `MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerC...` | 42035 |
| `ClipData` | class |  | 285 |
| `SoundHolders` | class |  | 393 |
| `SoundHolders.UISounds` | class |  | 358 |
| `SoundManager.Channel` | enum |  | 24580 |
| `SoundManager.PitchData` | class |  | 24568 |
| `SoundManager.Theme` | enum |  | 24504 |
| `SoundManager.ThemeData` | class |  | 24531 |
| `SoundUI` | class | `MonoBehaviour, ILocalizable, IDataLoader, IConfiguration` | 48267 |

## Entrées / navigation

| Type | Genre | Hérite / implémente | Ligne |
|---|---|---|---|
| `AxisInput.AxisInputRange` | class | `IIdentifier` | 20403 |
| `ButtonControllerCallbacks.ButtonActionPointerTempo` | enum |  | 42094 |
| `ControlRemappingDemo1.ControllerSelection` | class |  | 64232 |
| `ControllerElementGlyphSelectorOptions` | class |  | 60124 |
| `ControllerElementGlyphSelectorOptions.ControllerSelector` | struct |  | 60069 |
| `ControllerElementGlyphSelectorOptionsSO` | class | `ControllerElementGlyphSelectorOptionsSOBase` | 60168 |
| `ControllerElementGlyphSelectorOptionsSOBase` | class | `ScriptableObject` | 60179 |
| `CursorHelper` | class | `MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerE...` | 45199 |
| `CursorVisual` | class | `MonoBehaviour, IIdentifier` | 45340 |
| `CursorVisual.State` | enum |  | 45224 |
| `CursorVisual.UpdateSync` | enum |  | 45234 |
| `CustomOnPointerUp` | class | `MonoBehaviour` | 45429 |
| `CustomSelector` | class | `MonoBehaviour, IConfiguration, IIdentifier` | 47862 |
| `CustomSelectorOption` | class |  | 47908 |
| `CustomSelectorOptionExt` | class |  | 47942 |
| `ExtraSelectable` | class | `MonoBehaviour` | 2850 |
| `ExtraSelectable.InteractionContext` | enum |  | 2842 |
| `GroupSelectionsExt` | class |  | 48634 |
| `GroupsSelections` | class | `IGameplayContextResponsive, IIdentifier` | 48565 |
| `GroupsSelections.Group` | enum |  | 48460 |
| `HoverOutline` | class | `MonoBehaviour` | 47271 |
| `IAxisInputResponsive` | interface |  | 20861 |
| `ICustomSelectable` | interface | `ICancelHandler, IEventSystemHandler` | 63132 |
| `IGoBackInputResponsive` | interface |  | 20987 |
| `IHoverable` | interface |  | 45215 |
| `IInputReadyResponsive` | interface |  | 20630 |
| `IInputSelectResponsive` | interface |  | 20993 |
| `IInputTextID` | interface |  | 22590 |
| `IInputTypeResponsive` | interface |  | 20999 |
| `IInputUsedResponsive` | interface |  | 21005 |
| `INavigationGroupsChangeResponsive` | interface |  | 48778 |
| `INavigationMovementEndResponsive` | interface |  | 48784 |
| `INavigationMovementResponsive` | interface | `INavigationMovementEndResponsive, INavigationMovementStartResponsive` | 48790 |
| `INavigationMovementStartResponsive` | interface |  | 48793 |
| `INonNavigationGroupsChangeResponsive` | interface |  | 48805 |
| `ISelectableResponsive` | interface |  | 48640 |
| `ISelectionGroupsResponsive` | interface | `INavigationGroupsChangeResponsive, INonNavigationGroupsChangeRespon...` | 48811 |
| `ISelectionResponsive` | interface |  | 48814 |
| `IStartNavigationSortingRefreshResponsive` | interface |  | 48799 |
| `ISupportedInputResponsive` | interface |  | 21101 |
| `IToggleInputResponsive` | interface | `IToggleOnInputResponsive, IToggleOffInputResponsive` | 20755 |
| `IToggleOffInputResponsive` | interface |  | 20758 |
| `IToggleOnInputResponsive` | interface |  | 20764 |
| `InputActionType` | enum |  | 20665 |
| `InputActionTypeExt` | class |  | 20722 |
| `InputBehaviorWindow` | class | `Window` | 63210 |
| `InputBehaviorWindow.ButtonIdentifier` | enum |  | 63181 |
| `InputBehaviorWindow.InputBehaviorInfo` | class |  | 63161 |
| `InputFieldInfo` | class | `UIElementInfo` | 63246 |
| `InputImage` | class |  | 47345 |
| `InputManager` | class | `InputManager_Base` | 58876 |
| `InputRow` | class | `MonoBehaviour` | 63278 |
| `InputSlider` | class | `MonoBehaviour, IIdentifier` | 20636 |
| `InputTextID` | class |  | 22610 |
| `InputTypeChangeDetectionController` | class | `MonoBehaviour, IInputTypeResponsive, IIdentifier` | 20540 |
| `InputTypeChangeDetectionController.InputTypeChangeResponse` | struct |  | 20527 |
| `InputTypeTextID` | struct | `IInputTextID` | 22621 |
| `JoystickInput` | class | `IAxisInputResponsive, IIdentifier, IToggleOnInputResponsive` | 20556 |
| `MyPlatformInputSource` | class | `CustomPlatformInputSource` | 65381 |
| `MyPlatformInputSource.Joystick` | class | `CustomPlatformInputSource.Joystick, IControllerVibrator` | 65361 |
| `NavigationContextGroupsConfiguration` | class | `SerializedScriptableObject` | 48646 |
| `PreferenceNavigationZone` | class | `MonoBehaviour, IGameplayContextResponsive, IIdentifier` | 48666 |
| `ScrollRectSelectableChild` | class | `MonoBehaviour, ISelectHandler, IEventSystemHandler` | 63635 |
| `SelectableExt` | class |  | 48963 |
| `SelectableGoldenWeapons` | class | `SelectableSkin` | 3057 |
| `SelectableRiggedNavigationExtensions` | class |  | 48857 |
| `SelectionContentManagement` | class |  | 49018 |
| `SelectionManager.NavigationMode` | enum |  | 49089 |
| `SelectionManager.RelativePosition` | enum |  | 49074 |
| `SelectionNavigationMovement` | class | `IIdentifier, IGameplayContextResponsive` | 49295 |
| `SpecialSelections` | class | `IControllerTypeChangedResponsive, IBattleSubmenuButtonResponsive, I...` | 49586 |
| `SupportedInputTextID` | struct | `IInputTextID` | 22638 |
| `ThemeSettings.SelectableSettings` | class | `ThemeSettings.SelectableSettings_Base` | 63702 |
| `ThemeSettings.SelectableSettings_Base` | class |  | 63681 |
| `UIPointer` | class | `UIBehaviour` | 64814 |
| `UISelectionUtility` | class |  | 64069 |
| `UnityInputJoystickSource` | class |  | 65453 |
| `UnityInputJoystickSource.Joystick` | class | `IControllerVibrator` | 65421 |

## Localisation / texte

| Type | Genre | Hérite / implémente | Ligne |
|---|---|---|---|
| `ControllerElementGlyphBase.GlyphOrTextObject` | class |  | 59995 |
| `CreditsTextTooltip` | class | `MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerE...` | 3221 |
| `CustomLocalizeCallback` | class | `MonoBehaviour` | 66890 |
| `CustomText` | class | `MonoBehaviour, IVisuals, IAssertableDisposable, IDisposable, IDispo...` | 53919 |
| `CustomText.CustomLanguageData` | class |  | 53837 |
| `CustomText.CustomTMPValues` | class |  | 53846 |
| `CustomText.LanguageOverrides` | class |  | 53828 |
| `CustomText.StringType` | enum |  | 53802 |
| `CustomText.TextMode` | enum |  | 53810 |
| `CustomTextBuffersHolder` | class | `BuffersHolder` | 54058 |
| `CustomTextData` | class | `ScriptableObject` | 33359 |
| `CustomTextData.ColorGroup` | class |  | 33323 |
| `CustomTextData.ColorGroup.Gradient` | class |  | 33314 |
| `CustomTextData.Gradient` | class |  | 33305 |
| `CustomTextData.Gradient.Type` | enum |  | 33080 |
| `CustomTextEffects` | class | `MonoBehaviour` | 54110 |
| `CustomTextEffects.FX` | enum |  | 54090 |
| `CustomTextEffects.FXStart` | enum |  | 54101 |
| `CustomTextExt` | class |  | 55190 |
| `CustomTextFieldController` | class | `FocusInteractiveCustomUIController` | 45465 |
| `CustomTextSetter` | class | `MonoBehaviour, ILocalizable` | 54159 |
| `CustomTextSnip` | class | `MonoBehaviour, IAssertableDisposable, IDisposable, IDisposable, IAs...` | 54181 |
| `CustomTextSnip.Data` | struct |  | 54171 |
| `DebugLocalization` | class | `MonoBehaviour` | 4245 |
| `EnableByLocalization` | class | `MonoBehaviour, ILocalizable` | 22113 |
| `GlyphOrTextBase` | class | `MonoBehaviour` | 60219 |
| `GlyphOrTextBase.TypeFlags` | enum |  | 60209 |
| `GoogleLanguages` | class |  | 65670 |
| `GoogleLanguages.LanguageCodeDef` | struct |  | 65661 |
| `ILanguageSource` | interface |  | 65975 |
| `ILocalizable` | interface |  | 22135 |
| `ILocalizationParamsManager` | interface |  | 66962 |
| `ITextRelations` | interface |  | 22734 |
| `LanguageData` | class |  | 65898 |
| `LanguageData` | class | `LanguageDataBase` | 63318 |
| `LanguageData.CustomEntry` | class |  | 63295 |
| `LanguageDataBase` | class | `ScriptableObject` | 63505 |
| `LanguageImageController.ImageLanguageConfiguration` | class |  | 47365 |
| `LanguageRectController.RectLanguageConfiguration` | class |  | 47383 |
| `LanguageSource` | class | `MonoBehaviour, ISerializationCallbackReceiver, ILanguageSource` | 65924 |
| `LanguageSource.fnOnSourceUpdated` | class | `MulticastDelegate` | 65915 |
| `LanguageSourceAsset` | class | `ScriptableObject, ILanguageSource` | 65963 |
| `LanguageSourceData` | class |  | 66067 |
| `LanguageSourceData.MissingTranslationAction` | enum |  | 65984 |
| `LanguageSourceData.eAllowUnloadLanguages` | enum |  | 65994 |
| `LanguageSourceData.eGoogleUpdateFrequency` | enum |  | 66003 |
| `LanguageSourceData.eGoogleUpdateSynchronization` | enum |  | 66016 |
| `LanguageValues` | class |  | 33448 |
| `Localization` | class |  | 22466 |
| `Localization.CustomString` | class |  | 22329 |
| `Localization.CustomString.Mode` | enum |  | 22310 |
| `Localization.GenericKeyword` | enum |  | 22222 |
| `Localization.GenericString` | enum |  | 22203 |
| `Localization.Language` | enum |  | 22186 |
| `LocalizationManager` | class |  | 66438 |
| `LocalizationManager.FnCustomApplyLocalizationParams` | class | `MulticastDelegate` | 66316 |
| `LocalizationManager.OnLocalizeCallback` | class | `MulticastDelegate` | 66334 |
| `LocalizationManager._GetParam` | class | `MulticastDelegate` | 66325 |
| `LocalizationParamsManager` | class | `MonoBehaviour, ILocalizationParamsManager` | 66975 |
| `LocalizationParamsManager.ParamValue` | struct |  | 66968 |
| `LocalizationReader` | class |  | 66191 |
| `LocalizationSettings` | class | `SerializedScriptableObject` | 36851 |
| `LocalizationSettings.LanguageData` | class |  | 36836 |
| `LocalizationSettings.LanguageData.ColorOverride` | struct |  | 36829 |
| `Localize` | class | `MonoBehaviour` | 66235 |
| `Localize.TermModification` | enum |  | 66207 |
| `LocalizeDropdown` | class | `MonoBehaviour` | 66300 |
| `LocalizedString` | struct |  | 66990 |
| `ModifyWithLocalization` | class | `MonoBehaviour` | 22543 |
| `ModifyWithLocalization.LanguageModification` | struct |  | 22517 |
| `ModifyWithLocalization.Modifications` | struct |  | 22524 |
| `RuntimeTextRelations` | class | `ITextRelations` | 22800 |
| `SetLanguage` | class | `MonoBehaviour` | 67202 |
| `SetLanguageDropdown` | class | `MonoBehaviour` | 67212 |
| `SettingsTextIconsToggle` | class | `SettingsToggle` | 48053 |
| `TextCountersUI` | class | `MonoBehaviour, IExecutionEvents` | 41949 |
| `TextFX` | struct |  | 54137 |
| `TextID` | class |  | 23055 |
| `TextModeExtra` | class |  | 33439 |
| `TextModeExtra.Title` | enum |  | 33428 |
| `TextModeInfo` | class |  | 33417 |
| `TextModeInfo.OverrideParams` | class |  | 33383 |
| `TextModeInfo.Params` | class |  | 33392 |
| `TextRelationsCatalog` | class | `SerializedScriptableObject` | 22943 |
| `TextRelationsCatalogInfo` | class |  | 22993 |
| `ThemeSettings.TextSettings` | class |  | 63889 |
| `ToggleLanguage` | class | `MonoBehaviour` | 65553 |
| `UnityUIDefaultGlyphOrTextTMProPrefabProvider` | class |  | 60652 |
| `UnityUITextMeshProGlyphHelper` | class | `MonoBehaviour` | 61343 |
| `UnityUITextMeshProGlyphHelper.ActionTag` | class | `UnityUITextMeshProGlyphHelper.Tag` | 60884 |
| `UnityUITextMeshProGlyphHelper.Asset` | class |  | 60931 |
| `UnityUITextMeshProGlyphHelper.ControllerElementTag` | class | `UnityUITextMeshProGlyphHelper.Tag` | 60862 |
| `UnityUITextMeshProGlyphHelper.DisplayType` | enum |  | 61315 |
| `UnityUITextMeshProGlyphHelper.ITMProSprite` | interface |  | 61049 |
| `UnityUITextMeshProGlyphHelper.ITMProSpriteAsset` | interface |  | 61094 |
| `UnityUITextMeshProGlyphHelper.PlayerTag` | class | `UnityUITextMeshProGlyphHelper.Tag` | 60901 |
| `UnityUITextMeshProGlyphHelper.RewiredElementHelper` | class |  | 60952 |
| `UnityUITextMeshProGlyphHelper.SpriteMaterialProperties` | struct |  | 61009 |
| `UnityUITextMeshProGlyphHelper.TMProAssetVersionHelper` | class |  | 61112 |
| `UnityUITextMeshProGlyphHelper.TMProSprite_AssetV1_0_0` | class | `UnityUITextMeshProGlyphHelper.ITMProSprite` | 61144 |
| `UnityUITextMeshProGlyphHelper.TMProSprite_AssetV1_0_0.TMPro_SpriteAsset` | class | `UnityUITextMeshProGlyphHelper.ITMProSpriteAsset` | 61122 |
| `UnityUITextMeshProGlyphHelper.TMProSprite_AssetV1_1_0` | class | `UnityUITextMeshProGlyphHelper.ITMProSprite` | 61259 |
| `UnityUITextMeshProGlyphHelper.TMProSprite_AssetV1_1_0.TMPro_SpriteAsset` | class | `UnityUITextMeshProGlyphHelper.ITMProSpriteAsset` | 61233 |
| `UnityUITextMeshProGlyphHelper.TMProSprite_AssetV1_1_0.TMPro_SpriteGlyph` | class |  | 61218 |
| `UnityUITextMeshProGlyphHelper.Tag` | class |  | 60846 |
| `UnityUITextMeshProGlyphHelper.Tag.Pool` | class |  | 60829 |
| `UnityUITextMeshProGlyphHelper.Tag.TagType` | enum |  | 60820 |
| `UserDataStore_File.UTF8Text` | class | `UserDataStore_File.Codec` | 58974 |
| `WordGenericTextID.GenericWord` | enum |  | 23071 |
| `eLanguageDataFlags` | enum |  | 65889 |

## UI / visuels

| Type | Genre | Hérite / implémente | Ligne |
|---|---|---|---|
| `AnimExt` | class |  | 55043 |
| `AnimatorBattleEffectConfiguration` | class | `BattleEffectConfiguration` | 16846 |
| `AnimatorBattleEffectConfigurationHolder` | class |  | 16855 |
| `ArrowButtonController` | class | `CustomButtonController, ILocalizable` | 42072 |
| `AttachedTooltip` | class | `SerializedMonoBehaviour, IVisuals, IIdentifier, IGameplayContextRes...` | 52712 |
| `BackgroundProp` | class | `ScriptableObject` | 571 |
| `BackgroundVariation` | class | `ScriptableObject` | 635 |
| `BackgroundVariation.Type` | enum |  | 605 |
| `BackpackUI` | class | `MonoBehaviour, IExecutionEvents, IHoverable, ISettingsUIResponsive,...` | 41183 |
| `BackpackUI.OpenData` | class |  | 40909 |
| `BackpackUI.OpenData.State` | enum |  | 40869 |
| `BackpackUI.Requirement` | enum |  | 40861 |
| `BaseTooltipBox` | class | `MonoBehaviour` | 52847 |
| `BaseTooltipBox.Placement` | enum |  | 52801 |
| `BaseTooltipBoxBuffersHolder` | class | `BuffersHolder` | 52902 |
| `ButtonController` | class | `MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerE...` | 47448 |
| `ButtonControllerCallbacks` | class | `IDisposable, IDisposable` | 42115 |
| `ButtonControllerCallbacks.ButtonActionAnimationTempo` | enum |  | 42106 |
| `ButtonHold` | class | `MonoBehaviour, IPointerDownHandler, IEventSystemHandler, IPointerUp...` | 42043 |
| `ButtonInfo` | class | `UIElementInfo` | 61471 |
| `ButtonStateVisualFeedback` | struct |  | 42141 |
| `ButtonTitleScreen` | class | `MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerE...` | 42793 |
| `CalibrationWindow.ButtonIdentifier` | enum |  | 61477 |
| `CameraController.Mode` | enum |  | 23100 |
| `CancelButtonController` | class | `ArrowButtonController, IGoBackInputResponsive` | 42156 |
| `CanvasGameOver` | class | `CanvasEndScreen` | 43039 |
| `CanvasGroupExt` | class |  | 55149 |
| `CanvasManager.RenderTransform` | class |  | 23400 |
| `CanvasManager.RenderTransform.Conditions` | enum |  | 23390 |
| `CanvasScalerExt` | class | `CanvasScaler` | 61585 |
| `CanvasScalerFitter` | class | `MonoBehaviour` | 61602 |
| `CanvasScalerFitter.BreakPoint` | class |  | 61592 |
| `CanvasTitleScreen` | class | `MonoBehaviour, IHoverable, IIdentifier, IDisposable, IDisposable` | 43310 |
| `CanvasUtil` | class |  | 56310 |
| `CanvasValues` | class |  | 43565 |
| `CanvasWin` | class | `CanvasEndScreen` | 43550 |
| `CardFX` | class | `MonoBehaviour` | 43585 |
| `CenterCameraButtonController` | class | `MonoBehaviour` | 43603 |
| `CenterCameraButtonController.Type` | enum |  | 43595 |
| `CodexUI` | class | `MonoBehaviour, INextCategoryResponsive, IPreviousCategoryResponsive...` | 44893 |
| `CodexUI.CategoryData` | struct |  | 44785 |
| `CodexUI.PageOrientation` | enum |  | 44777 |
| `CodexUI.VisualContent` | class |  | 44798 |
| `ColorFX` | class | `DialogFX` | 4575 |
| `ColorFX.ColorData` | class | `DialogFX.BaseData` | 4563 |
| `ConfirmButtonController` | class | `ModifyFacesUICustomButton, IConfirmButtonResponsive` | 42202 |
| `ContinueButtonController` | class | `ArrowButtonController, IContinueButtonResponsive` | 42217 |
| `ControllerUIEffect` | class | `MonoBehaviour` | 65122 |
| `ControllerUIElement` | class | `MonoBehaviour` | 65139 |
| `CreditsUI` | class | `MonoBehaviour, ILocalizable, IIdentifier, IInputTypeResponsive` | 47549 |
| `CustomButton` | class | `Button, ICustomSelectable, ICancelHandler, IEventSystemHandler` | 62957 |
| `CustomButtonController` | class | `MonoBehaviour, IOutlineable, IIdentifier` | 42354 |
| `CustomButtonController.DelayType` | enum |  | 42234 |
| `DOTweenMoveData` | struct |  | 157 |
| `DOTweenPunchData` | class |  | 57339 |
| `DOTweenScaleData` | class |  | 57363 |
| `DOTweenShakeData` | class |  | 57350 |
| `DOTweenUtil` | class |  | 57328 |
| `DebugLogUI` | class | `MonoBehaviour` | 45572 |
| `DevMenu` | class | `MonoBehaviour, IExecutionEvents` | 45648 |
| `DiscordButtonController` | class | `CustomButtonController, IDiscordButtonResponsive, IInputReadyRespon...` | 42448 |
| `DropFX` | class | `StartFX` | 4670 |
| `DropFX.DropData` | class | `DialogFX.BaseData` | 4658 |
| `EndlessModeMessageUI` | class | `MonoBehaviour, ILocalizable, IHighPriorityUI` | 46017 |
| `EndlessModeMessageUI.Option` | enum |  | 45979 |
| `EnergyHolder.EnergyAnimationParameter` | enum |  | 46068 |
| `ExtraValueUI` | class | `MonoBehaviour` | 6849 |
| `FXType` | enum |  | 17713 |
| `Fade` | class | `TweenCustom` | 55534 |
| `FadeUI` | class | `MonoBehaviour, IIdentifier` | 40588 |
| `FocusInteractiveCustomUIController` | class | `InteractiveCustomUIController, IGoBackInputResponsive` | 45528 |
| `GameplayUI` | class | `MonoBehaviour, ILocalizable, IDataLoader, IConfiguration` | 47595 |
| `GlobalCanvas` | class | `MonoBehaviour, ILocalizable` | 24482 |
| `GraphicsUI` | class | `MonoBehaviour, ILocalizable, IDataLoader, IConfiguration` | 47711 |
| `HeaderUI` | class | `MonoBehaviour, IExecutionEvents` | 46955 |
| `HeaderUI.Position` | enum |  | 46933 |
| `HighlighterUI` | class | `MonoBehaviour, IExecutionEvents` | 47124 |
| `HighlighterUI.ActiveHighlight` | struct |  | 47029 |
| `HighlighterUI.FadeData` | struct |  | 47059 |
| `HighlighterUI.OutlineData` | struct |  | 47088 |
| `HighlighterUI.ShapeData` | struct |  | 47079 |
| `HighlighterUI.ShapeData.Type` | enum |  | 47070 |
| `HighlighterUI.ShapeTypeInfo` | class |  | 47096 |
| `HighlighterUI.SizeData` | struct |  | 47047 |
| `IAltShortcutButtonResponsive` | interface |  | 20894 |
| `IAttachedTooltip` | interface |  | 53052 |
| `IBackpackUIResponsive` | interface |  | 47787 |
| `IBattleSubmenuButtonResponsive` | interface |  | 20900 |
| `IButtonResponsive` | interface |  | 20728 |
| `IConfirmButtonResponsive` | interface |  | 20921 |
| `IContinueButtonResponsive` | interface |  | 20927 |
| `ICreditsUIResponsive` | interface |  | 47799 |
| `IDiscordButtonResponsive` | interface |  | 20951 |
| `IDisplayCanvasEndScreenResponsive` | interface |  | 43559 |
| `IDisplayFeedbackUIResponsive` | interface |  | 46876 |
| `IDownButtonResponsive` | interface |  | 20867 |
| `IDrawButtonResponsive` | interface |  | 20957 |
| `IEndBackpackButtonResponsive` | interface |  | 20969 |
| `IHighPriorityUI` | interface |  | 53712 |
| `IHighPriorityUIResponsive` | interface |  | 53720 |
| `IHorizontalButtonResponsive` | interface | `IRightButtonResponsive, ILeftButtonResponsive` | 20873 |
| `ILeftButtonResponsive` | interface |  | 20876 |
| `ILoadingScreenStartHideResponsive` | interface |  | 22009 |
| `ILoadingScreenStartShownResponsive` | interface |  | 22015 |
| `IMinusOneButtonResponsive` | interface |  | 21023 |
| `IPlayerButtonResponsive` | interface |  | 21041 |
| `IPlusOneButtonResponsive` | interface |  | 21047 |
| `IRerollButtonDescription` | interface |  | 42475 |
| `IResetButtonResponsive` | interface |  | 21071 |
| `IRightButtonResponsive` | interface |  | 20882 |
| `IScreenResponsive` | interface |  | 51806 |
| `ISendFeedbackButtonResponsive` | interface |  | 21083 |
| `ISettingsUIResponsive` | interface |  | 47844 |
| `ISkipButtonResponsive` | interface |  | 21089 |
| `ITooltipShowable` | interface |  | 52795 |
| `ITutorialButtonResponsive` | interface |  | 21119 |
| `ITwitterButtonResponsive` | interface |  | 21125 |
| `IUIDisplay` | interface |  | 53726 |
| `IUIDisplayResponsive` | interface | `IUIHideResponsive, IUIShownResponsive` | 53736 |
| `IUIHideResponsive` | interface |  | 53739 |
| `IUIShownResponsive` | interface |  | 53745 |
| `IUpButtonResponsive` | interface |  | 20888 |
| `IVisuals` | interface |  | 47359 |
| `InteractiveCustomUIController` | class | `MonoBehaviour, IInputTypeResponsive, IIdentifier, ISelectableRespon...` | 45548 |
| `LayoutUIController` | class | `MonoBehaviour, IBackpackUIResponsive, ISettingsUIResponsive` | 47413 |
| `LayoutUIController.Type` | enum |  | 47406 |
| `MinusOneButtonController` | class | `ModifyFacesUICustomButton, IMinusOneButtonResponsive` | 42491 |
| `MultiImageButton` | class | `Button` | 48438 |
| `NewsButtonController` | class | `CustomButtonController` | 42512 |
| `NextCategoryButtonController` | class | `CustomButtonController, INextCategoryResponsive` | 42518 |
| `NextOptionButtonController` | class | `CustomButtonController, INextOptionResponsive` | 42527 |
| `NoneFX` | class | `StartFX` | 4681 |
| `OriginsWishlistButton` | class | `MonoBehaviour, ILocalizable` | 49790 |
| `OriginsWishlistPopupUI` | class | `MonoBehaviour, ILocalizable` | 49840 |
| `OriginsWishlistPopupUI.Location` | enum |  | 49811 |
| `PlaylistUI` | class | `MonoBehaviour` | 41432 |
| `PlusOneButtonController` | class | `ModifyFacesUICustomButton, IPlusOneButtonResponsive` | 42536 |
| `PopUpsUI` | class | `MonoBehaviour, IUIDisplay, IIdentifier` | 49986 |
| `PreviousCategoryButtonController` | class | `CustomButtonController, IPreviousCategoryResponsive` | 42544 |
| `PreviousOptionButtonController` | class | `CustomButtonController, IPreviousOptionResponsive` | 42553 |
| `QueenBeeVisual` | class | `VisualEnemy` | 16922 |
| `QuitRestartButtons` | class | `MonoBehaviour` | 50019 |
| `RerollButtonController` | class | `CustomButtonController, ILocalizable` | 42591 |
| `RerollButtonController.Type` | enum |  | 42562 |
| `RerollButtonDescriptionController` | class | `MonoBehaviour` | 42673 |
| `ResetButtonController` | class | `ModifyFacesUICustomButton, IResetButtonResponsive` | 42698 |
| `ReturnButtonController` | class | `CustomButtonController, IGoBackInputResponsive` | 42712 |
| `ScreenManager.AnchorPosition` | enum |  | 51812 |
| `Screenshots` | class | `MonoBehaviour` | 55890 |
| `Screenshots.CaptureMode` | enum |  | 55837 |
| `SendFeedbackCustomButtonController` | class | `CustomButtonController, ISendFeedbackButtonResponsive` | 46888 |
| `SettingsBattleButton` | class | `MonoBehaviour` | 47959 |
| `SettingsCameraMoveToggle` | class | `SettingsToggle` | 47975 |
| `SettingsFinishButtonToggle` | class | `SettingsToggle` | 48037 |
| `ShakeFX` | class | `VertexFX` | 4622 |
| `ShakeFX.ShakeData` | class | `DialogFX.BaseData` | 4613 |
| `SizeFX` | class | `VertexFX` | 4644 |
| `SizeFX.SizeData` | class | `DialogFX.BaseData` | 4633 |
| `SkipButtonController` | class | `ArrowButtonController, ISkipButtonResponsive` | 42730 |
| `SkipTutorialButtonController` | class | `CustomButtonController, ITutorialButtonResponsive` | 42743 |
| `StampFX` | class | `StartFX` | 4700 |
| `StampFX.StampData` | class | `DialogFX.BaseData` | 4688 |
| `StartFX` | class | `VertexFX` | 4711 |
| `SyncAnimationsController` | class | `MonoBehaviour` | 160 |
| `ThemeSettings.CustomAnimationTriggers` | class |  | 63856 |
| `TokenAnimationValues` | struct |  | 26986 |
| `TokenCanvas` | class | `MonoBehaviour` | 27032 |
| `Tooltip` | class | `MonoBehaviour` | 53346 |
| `Tooltip.Message` | struct |  | 53097 |
| `Tooltip.Mode` | enum |  | 53059 |
| `Tooltip.Orientation` | enum |  | 53078 |
| `TooltipBox` | class | `BaseTooltipBox` | 53468 |
| `TooltipManager` | class | `MonoBehaviour` | 53495 |
| `TransitionUI` | class | `MonoBehaviour` | 41997 |
| `TutorialUI` | class | `MonoBehaviour, ISettingsUIResponsive, IDiceBoxResponsive, IInputTyp...` | 53529 |
| `TweenCustom` | class |  | 56125 |
| `TweenMoveCommand` | class | `TweenerCommand` | 211 |
| `TweenerCommand` | class | `ICommand, IDisposable, IDisposable` | 185 |
| `TwitterButtonController` | class | `CustomButtonController, ITwitterButtonResponsive, IInputReadyRespon...` | 42753 |
| `UIButtonHolder` | struct |  | 54211 |
| `UIControl` | class | `MonoBehaviour` | 63969 |
| `UIControlSet` | class | `MonoBehaviour` | 64001 |
| `UIElement` | enum |  | 23012 |
| `UIElementInfo` | class | `MonoBehaviour, ISelectHandler, IEventSystemHandler` | 64016 |
| `UIGroup` | class | `MonoBehaviour` | 64030 |
| `UIImageHelper` | class | `MonoBehaviour` | 64055 |
| `UIImageHelper.State` | class |  | 64046 |
| `UIPriorityController` | class |  | 54218 |
| `UISliderControl` | class | `UIControl` | 64088 |
| `UITools` | class |  | 39596 |
| `UITools` | class |  | 64107 |
| `UnityUIControllerElementGlyph` | class | `ControllerElementGlyph` | 60620 |
| `UnityUIControllerElementGlyphBase` | class | `ControllerElementGlyphBase` | 60627 |
| `UnityUIDefaultControllerElementGlyphSettings` | class | `DefaultControllerElementGlyphSettingsBase` | 60645 |
| `UnityUIPlayerControllerElementGlyph` | class | `UnityUIPlayerControllerElementGlyphBase` | 60686 |
| `UnityUIPlayerControllerElementGlyphBase` | class | `UnityUIControllerElementGlyphBase` | 60736 |
| `UnscaledAutoKillTweenerCommand` | class | `TweenerCommand` | 221 |
| `VertexFX` | class | `DialogFX` | 4727 |
| `VisualDragonflyElite` | class | `VisualEnemy, ISelectableResponsive, IInputTypeResponsive` | 17126 |
| `VisualFX` | class | `MonoBehaviour` | 17720 |
| `VisualPlayer` | class | `VisualEntity` | 17741 |
| `WaveFX` | class | `VertexFX` | 4751 |
| `WaveFX.WaveData` | class | `DialogFX.BaseData` | 4740 |
| `WishlistButtonUI` | class | `MonoBehaviour, ISettingsUIResponsive, ICreditsUIResponsive` | 54402 |
| `WishlistPopupUI` | class | `MonoBehaviour, IExecutionEvents` | 49731 |

## Interfaces (callbacks *Responsive)

| Type | Genre | Hérite / implémente | Ligne |
|---|---|---|---|
| `IAssertableDisposable` | interface | `IDisposable, IDisposable, IAssertive` | 27922 |
| `IAssertive` | interface |  | 29169 |
| `IBattleIntroResponsive` | interface |  | 19832 |
| `ICategoryResponsive` | interface | `INextCategoryResponsive, IPreviousCategoryResponsive` | 20906 |
| `ICodexDisplayResponsive` | interface |  | 45003 |
| `ICommand` | interface | `IDisposable, IDisposable` | 27859 |
| `IConfiguration` | interface |  | 47781 |
| `IControllerTypeChangedResponsive` | interface |  | 20734 |
| `IDisposable` | interface | `IDisposable` | 27931 |
| `IDragResponsive` | interface |  | 25816 |
| `IEditorDevRuntimeValue` | interface |  | 10046 |
| `IEditorObject` | interface | `IAssertive` | 10052 |
| `IEndlessModeClickedResponsive` | interface |  | 47811 |
| `IFiniteEffect` | interface |  | 2394 |
| `IFlightPedalsTemplate` | interface | `IControllerTemplate` | 57812 |
| `IFlightYokeTemplate` | interface | `IControllerTemplate` | 57708 |
| `IGameManagerStateResponsive` | interface |  | 24464 |
| `IGamepadTemplate` | interface | `IControllerTemplate` | 57372 |
| `IHOTASTemplate` | interface | `IControllerTemplate` | 57526 |
| `IHidedContentHolder` | interface |  | 37209 |
| `IIdentifier` | interface |  | 27939 |
| `IInteractionTutorialResponsive` | interface |  | 39628 |
| `IJoystickDynamicValueChangeResponsive` | interface |  | 20749 |
| `IJoystickResponsive` | interface |  | 21011 |
| `ILifeCycleListener` | interface |  | 27966 |
| `ILogReceiver` | interface |  | 47339 |
| `IMantisBehaviour` | interface |  | 7553 |
| `INextCategoryResponsive` | interface |  | 21029 |
| `INextOptionResponsive` | interface |  | 21035 |
| `INonInteractionTutorialResponsive` | interface |  | 39634 |
| `IOnDisableResponsive` | interface |  | 27975 |
| `IOnEnableResponsive` | interface |  | 27981 |
| `IOptionClickResponsive` | interface |  | 46700 |
| `IOutlineable` | interface |  | 47295 |
| `IPlayer` | interface |  | 11466 |
| `IPrepareForEndlessModeResponsive` | interface |  | 47817 |
| `IPreviousCategoryResponsive` | interface |  | 21053 |
| `IPreviousOptionResponsive` | interface |  | 21059 |
| `IRacingWheelTemplate` | interface | `IControllerTemplate` | 57436 |
| `IRefresh` | interface |  | 27947 |
| `IRerollData` | interface |  | 3667 |
| `IReset` | interface |  | 27953 |
| `ISerializeReferenceByAssetGuid` | interface |  | 38998 |
| `ISerializeReferenceByCustomGuid` | interface |  | 39001 |
| `ISixDofControllerTemplate` | interface | `IControllerTemplate` | 57824 |
| `ISubscriber` | interface |  | 27959 |
| `ISwitchEnableResponsive` | interface | `IOnEnableResponsive, IOnDisableResponsive` | 27987 |
| `IToggleOptionResponsive` | interface |  | 21113 |
| `IToggleable` | interface |  | 48410 |
| `IUnselectResponsive` | interface |  | 21131 |
| `IWindowDynamicValue` | interface |  | 10055 |

## Divers

| Type | Genre | Hérite / implémente | Ligne |
|---|---|---|---|
| `ActionsPack` | class |  | 11367 |
| `ActiveChargedBattleAction` | struct |  | 10577 |
| `ActiveTutorial` | class |  | 40000 |
| `ArabicTable` | class |  | 67163 |
| `ArrayExt` | class |  | 55074 |
| `ArraysReferencesTools` | class |  | 39495 |
| `AutoChangeCultureInfo` | class | `MonoBehaviour` | 66870 |
| `AxisActionNaming` | class |  | 20386 |
| `BackGroundMode` | enum |  | 560 |
| `BackpackGlobal` | class |  | 41329 |
| `BaseDirection` | class | `MonoBehaviour` | 54972 |
| `BaseDirection.ChangeMode` | enum |  | 54963 |
| `BasePlayerData` | class | `BaseEntityData` | 33998 |
| `BaseSpecializationManager` | class |  | 65611 |
| `BasicBattleAction` | class | `BattleAction` | 10303 |
| `Bezier` | class | `MonoBehaviour` | 54851 |
| `BezierUtil` | class |  | 56205 |
| `Blit` | class | `MonoBehaviour` | 54867 |
| `BodySpriteInfo` | class |  | 5510 |
| `BodySpriteInfo.SpriteInfoType` | enum |  | 5501 |
| `BoostAction` | class | `BattleAction` | 10653 |
| `BreatheBase` | class | `MonoBehaviour` | 54883 |
| `BreathePosition` | class | `BreatheBase` | 54927 |
| `BreatheRotation` | class | `BreatheBase` | 54939 |
| `BreatheScale` | class | `BreatheBase` | 54951 |
| `BreatheUtil` | class |  | 56297 |
| `BreatheUtil.SinWaveMode` | enum |  | 56216 |
| `Bullet` | class | `MonoBehaviour` | 64911 |
| `CalibrationWindow` | class | `Window` | 61498 |
| `CallbackNotification` | class | `MonoBehaviour` | 65490 |
| `CatalogData` | class | `SerializedScriptableObject` | 32300 |
| `CatalogData.BridgeInfo` | struct |  | 32131 |
| `CatalogData.GenericKeywordInfo` | struct |  | 32140 |
| `CatalogData.GradeScore` | struct |  | 32204 |
| `CatalogData.NameContext` | enum |  | 32291 |
| `CatalogData.RarityInfo` | struct |  | 31832 |
| `CatalogData.ShapeInfo` | struct |  | 31789 |
| `CatalogData.SurnameInfo` | struct |  | 31823 |
| `CatalogData.TermInfo` | struct |  | 32010 |
| `CatalogData.TermInfo.Term` | enum |  | 32000 |
| `CatalogData.TierInfo` | struct |  | 31799 |
| `CatalogData.TypeInfo` | struct |  | 31735 |
| `CatalogInfo` | class |  | 32639 |
| `ChargedBattleAction` | class | `BattleAction` | 10564 |
| `Codex` | class |  | 44128 |
| `Codex.Category` | class |  | 43831 |
| `Codex.Category.Type` | enum |  | 43819 |
| `Codex.Item` | class |  | 43861 |
| `Codex.Item.State` | enum |  | 43839 |
| `CodexCategoryMarker` | class | `MonoBehaviour` | 44149 |
| `ColorExt` | class |  | 55159 |
| `ColorTools` | class |  | 39446 |
| `ColorUtil` | class |  | 56317 |
| `ControlRemappingDemo1` | class | `MonoBehaviour` | 64479 |
| `ControlRemappingDemo1.Calibration` | class | `ControlRemappingDemo1.QueueEntry` | 64411 |
| `ControlRemappingDemo1.ElementAssignmentChange` | class | `ControlRemappingDemo1.QueueEntry` | 64378 |
| `ControlRemappingDemo1.ElementAssignmentChangeType` | enum |  | 64458 |
| `ControlRemappingDemo1.FallbackJoystickIdentification` | class | `ControlRemappingDemo1.QueueEntry` | 64395 |
| `ControlRemappingDemo1.JoystickAssignmentChange` | class | `ControlRemappingDemo1.QueueEntry` | 64358 |
| `ControlRemappingDemo1.QueueActionType` | enum |  | 64447 |
| `ControlRemappingDemo1.QueueEntry` | class |  | 64329 |
| `ControlRemappingDemo1.QueueEntry.State` | enum |  | 64320 |
| `ControlRemappingDemo1.UserResponse` | enum |  | 64469 |
| `ControlRemappingDemo1.WindowProperties` | struct |  | 64437 |
| `ControllerElementGlyph` | class | `ControllerElementGlyphBase` | 59974 |
| `ControllerElementGlyphBase` | class | `MonoBehaviour` | 60026 |
| `ControllerElementGlyphBase.AllowedTypes` | enum |  | 60017 |
| `ControllerTemplateFactory` | class |  | 59587 |
| `CreditGroup` | class | `ScriptableObject` | 47480 |
| `CreditGroup.Type` | enum |  | 47471 |
| `CustomControllerDemo` | class | `MonoBehaviour` | 64583 |
| `CustomControllerDemo_Player` | class | `MonoBehaviour` | 64611 |
| `CustomControllersTiltDemo` | class | `MonoBehaviour` | 64569 |
| `CustomDropdownController` | class | `FocusInteractiveCustomUIController, IToggleOnInputResponsive` | 45447 |
| `CustomSlider` | class | `Slider, ICustomSelectable, ICancelHandler, IEventSystemHandler` | 63018 |
| `CustomSpriteHolder` | class |  | 5544 |
| `CustomSpriteHolder.ValueMode` | enum |  | 5536 |
| `CustomToggle` | class | `Toggle, ICustomSelectable, ICancelHandler, IEventSystemHandler` | 63075 |
| `CustomToggleController` | class | `InteractiveCustomUIController` | 45479 |
| `DOJumpData` | class |  | 105 |
| `DOMoveData` | class |  | 118 |
| `DeathInfo` | class |  | 19793 |
| `DeathInfo.Extra` | struct |  | 19784 |
| `DebugExtra` | class |  | 56006 |
| `DefaultControllerElementGlyphSettingsBase` | class | `MonoBehaviour` | 60188 |
| `DictionariesReferencesTools` | class |  | 39505 |
| `Directions` | class |  | 55023 |
| `Directions.Direction` | enum |  | 55008 |
| `DropdownHelper` | class | `MonoBehaviour` | 45820 |
| `ECardinalPosition` | enum |  | 2379 |
| `EarlyAccessDisclaimer` | class | `MonoBehaviour, ILocalizable` | 10028 |
| `EditorBattleShortcuts` | class | `IIdentifier` | 45599 |
| `EmbeddedAttribute` | class | `Attribute` | 66 |
| `EndlessMode` | class |  | 45971 |
| `EnergyHolder` | class | `MonoBehaviour` | 46076 |
| `EntitiesTools` | class |  | 39452 |
| `EnumTools` | class |  | 39458 |
| `Enums` | class |  | 56012 |
| `EvilFrog` | class | `Enemy` | 13376 |
| `EvilFrog.Phase` | enum |  | 13083 |
| `EvilFrog.Variant` | enum |  | 13091 |
| `ExternalTools` | class | `IExternalTools` | 58893 |
| `ExtraMoveCounter` | class | `MonoBehaviour` | 11103 |
| `FallbackJoystickIdentificationDemo` | class | `MonoBehaviour` | 64744 |
| `FlightPedalsTemplate` | class | `ControllerTemplate, IFlightPedalsTemplate, IControllerTemplate` | 58698 |
| `FlightYokeTemplate` | class | `ControllerTemplate, IFlightYokeTemplate, IControllerTemplate` | 58513 |
| `ForceCap` | class | `MonoBehaviour` | 55547 |
| `Future` | class |  | 20369 |
| `Future.BattleChanges` | class |  | 20300 |
| `GameObjectExt` | class |  | 55224 |
| `GeneralArabicLetters` | enum |  | 67107 |
| `GlyphProvider` | class | `MonoBehaviour, IGlyphProvider` | 60256 |
| `GlyphSet` | class | `ScriptableObject` | 60309 |
| `GlyphSet.EntryBase` | class |  | 60283 |
| `GlyphSetCollection` | class | `ScriptableObject` | 60352 |
| `GlyphTools` | class |  | 60512 |
| `GlyphTools.Action2DHelper` | class |  | 60379 |
| `GlyphTools.Action2DHelper.Result` | enum |  | 60371 |
| `GlyphTools.ControllerInfo` | struct |  | 60482 |
| `GoogleTranslation` | class |  | 65740 |
| `GoogleTranslation.fnOnTranslated` | class | `MulticastDelegate` | 65693 |
| `GoogleTranslation.fnOnTranslationReady` | class | `MulticastDelegate` | 65702 |
| `Grid` | class |  | 25717 |
| `Grid.Coord` | class |  | 25404 |
| `Grid.Directions` | class |  | 25446 |
| `Grid.Directions.Direction` | enum |  | 25431 |
| `Grid.EndPointData` | class |  | 25458 |
| `Grid.OverrideColumnData` | class |  | 25518 |
| `Grid.OverrideColumnData.OverrideMode` | enum |  | 25510 |
| `HOTASTemplate` | class | `ControllerTemplate, IHOTASTemplate, IControllerTemplate` | 58159 |
| `HSBColor` | struct |  | 55558 |
| `HSLController` | class | `MonoBehaviour` | 47311 |
| `HSLController.State` | enum |  | 47301 |
| `HeaderPositionConfiguration` | struct |  | 46911 |
| `Highlight` | class | `MonoBehaviour` | 47016 |
| `HindiFixer` | class |  | 66912 |
| `I2BasePersistentStorage` | class |  | 65588 |
| `I2CustomPersistentStorage` | class | `I2BasePersistentStorage` | 65605 |
| `I2Utils` | class |  | 66937 |
| `ImageExt` | class |  | 55240 |
| `IsolatedArabicLetters` | enum |  | 67060 |
| `JSON` | class |  | 67568 |
| `JSONArray` | class | `JSONNode, IEnumerable` | 67402 |
| `JSONBinaryTag` | enum |  | 67234 |
| `JSONClass` | class | `JSONNode, IEnumerable` | 67480 |
| `JSONData` | class | `JSONNode` | 67507 |
| `JSONLazyCreator` | class | `JSONNode` | 67526 |
| `Layers` | class |  | 56015 |
| `LerpUtil` | class |  | 56915 |
| `LifeCycleArgs` | class | `EventArgs` | 28016 |
| `LifeCycleRuntimeSet` | class | `IDisposable, IDisposable, ILifeCycleListener` | 28061 |
| `ListExt` | class |  | 55272 |
| `ListsReferencesTools` | class |  | 39512 |
| `LogTools` | class | `MonoBehaviour` | 39474 |
| `LogTools.MessagePreSet` | enum |  | 39465 |
| `MathExt` | class |  | 55381 |
| `MathExt.OperationType` | enum |  | 55298 |
| `MaxWidthFitter` | class | `MonoBehaviour` | 27225 |
| `MinMaxValue` | struct |  | 55577 |
| `Mode` | enum |  | 34833 |
| `MonoBehaviourExt` | class |  | 55406 |
| `MouseSimulator` | class | `StandaloneInputModule, IIdentifier` | 20840 |
| `MyPlatformControllerExtension` | class | `CustomControllerExtension, IControllerVibrator` | 65299 |
| `MyPlatformControllerExtension.Source` | class | `IControllerExtensionSource` | 65291 |
| `MyPlatformControllerIdentifier` | struct |  | 65316 |
| `MyPlatformUnifiedKeyboardSource` | class | `CustomPlatformUnifiedKeyboardSource` | 65400 |
| `MyPlatformUnifiedMouseSource` | class | `CustomPlatformUnifiedMouseSource` | 65411 |
| `NewBreathe` | class | `SerializedMonoBehaviour` | 55588 |
| `NewsData` | class | `SerializedScriptableObject` | 38251 |
| `NewsData.Update` | class |  | 38220 |
| `NewsData.Update.UpdateNameMode` | enum |  | 38211 |
| `NewsData.Update.UpdateType` | enum |  | 38203 |
| `NewsPopupElement` | class | `PopUpElement` | 49686 |
| `NoBattleAction` | class | `BattleAction` | 10295 |
| `NullableAttribute` | class | `Attribute` | 72 |
| `OSTools` | class |  | 39486 |
| `ObjectPool` | class | `MonoBehaviour` | 55663 |
| `ObjectPool.ParentType` | enum |  | 55653 |
| `ObtainableEnergyShadowsConfig` | class | `SerializedScriptableObject` | 50358 |
| `OrbController` | class | `MonoBehaviour` | 13687 |
| `OrbsController` | class | `MonoBehaviour` | 13753 |
| `Particle` | class | `MonoBehaviour` | 27633 |
| `ParticleExt` | class |  | 55412 |
| `ParticleSystemCallbackHelper` | class | `MonoBehaviour` | 27648 |
| `PersistentStorage` | class |  | 65571 |
| `PersistentStorage.eFileType` | enum |  | 65561 |
| `PixelGraphics` | class |  | 28423 |
| `PrompCatalog` | class | `SerializedScriptableObject` | 48710 |
| `PrompCatalogInfo` | class |  | 48725 |
| `PrompConfig` | class |  | 48734 |
| `PrompConfigExt` | class |  | 48746 |
| `PrompController` | class | `MonoBehaviour, IVisuals, ISupportedInputResponsive, IIdentifier` | 48752 |
| `PropMovement` | class | `MonoBehaviour` | 723 |
| `PropertiesPool` | class | `IAssertableDisposable, IDisposable, IDisposable, IAssertive` | 54791 |
| `PropertiesPool.Side` | enum |  | 54783 |
| `QuaternionUtil` | class |  | 56984 |
| `RTLFixer` | class |  | 67051 |
| `RTLFixerTool` | class |  | 67185 |
| `RacingWheelTemplate` | class | `ControllerTemplate, IRacingWheelTemplate, IControllerTemplate` | 58019 |
| `RandomExt` | class |  | 55418 |
| `RealTimeTranslation` | class | `MonoBehaviour` | 65525 |
| `RectTransformExt` | class |  | 55432 |
| `RedHoodMonitorEye` | class | `MonoBehaviour` | 28468 |
| `ReferencesTools` | class |  | 39552 |
| `RegisterBundlesManager` | class | `MonoBehaviour, IResourceManager_Bundles` | 65544 |
| `RegisterCallback_AllowSyncFromGoogle` | class | `MonoBehaviour` | 67005 |
| `RegisterGlobalParameters` | class | `MonoBehaviour, ILocalizationParamsManager` | 67015 |
| `RerollData` | class | `IRerollData` | 3683 |
| `ResourceManagmentTools` | class |  | 39563 |
| `ResourceObject` | class | `IDisposable, IDisposable, IAssertive` | 29175 |
| `ResourceTools` | class |  | 39569 |
| `RigidbodyUtil` | class |  | 56991 |
| `RotateCustom` | class | `MonoBehaviour` | 55692 |
| `SOCacheEntry` | class |  | 38984 |
| `SceneController` | class |  | 55799 |
| `SceneController.SceneName` | enum |  | 55704 |
| `ScriptableObjectReferenceCache` | class | `ScriptableObject, IExternalStringReferenceResolver` | 38962 |
| `ScrollRectExt` | class |  | 55446 |
| `ScrollbarVisibilityHelper` | class | `MonoBehaviour` | 63627 |
| `SerializableDate` | class |  | 55921 |
| `SessionLog` | class |  | 89 |
| `SettingsDefaultControllerOption` | class | `MonoBehaviour` | 48024 |
| `SettingsForceTutorialToggle` | class | `SettingsToggle` | 48045 |
| `SettingsTimerToggle` | class | `SettingsToggle` | 48061 |
| `SettingsToggle` | class | `SerializedMonoBehaviour, ILocalizable, IToggleable, IConfiguration,...` | 48069 |
| `Shake` | class |  | 56999 |
| `ShakeInstance` | class |  | 57132 |
| `ShakeUtil` | class |  | 57245 |
| `SimpleCombinedKeyboardMouseRemapping` | class | `MonoBehaviour` | 64974 |
| `SimpleCombinedKeyboardMouseRemapping.Row` | class |  | 64923 |
| `SimpleControlRemapping` | class | `MonoBehaviour` | 65049 |
| `SimpleControlRemapping.Row` | class |  | 65007 |
| `SixDofControllerTemplate` | class | `ControllerTemplate, ISixDofControllerTemplate, IControllerTemplate` | 58717 |
| `Socials` | class |  | 39009 |
| `SpawnPositions` | class | `MonoBehaviour` | 19889 |
| `SpawnPositions.Type` | enum |  | 19872 |
| `SpecializationManager` | class | `BaseSpecializationManager` | 65624 |
| `SpringUtil` | class |  | 57260 |
| `Springing` | class |  | 57266 |
| `SpriteGlyphSet` | class | `GlyphSet` | 60605 |
| `SpriteMaskExt` | class |  | 55452 |
| `SpriteRenderExt` | class |  | 55461 |
| `StringExt` | class |  | 55480 |
| `StringObfucator` | class |  | 67220 |
| `SwapColor` | class | `MonoBehaviour` | 56104 |
| `SwapColor.Info` | struct |  | 56051 |
| `SwapColor.SwapIndex` | enum |  | 56089 |
| `Tags` | class |  | 56042 |
| `TashkeelLocation` | class |  | 67176 |
| `TermData` | class |  | 66836 |
| `TermsPopup` | class | `PropertyAttribute` | 66858 |
| `ThemeSettings` | class | `ScriptableObject` | 63929 |
| `ThemeSettings.CustomSpriteState` | struct |  | 63828 |
| `ThemeSettings.FontStyleOverride` | enum |  | 63918 |
| `ThemeSettings.ImageSettings` | class |  | 63749 |
| `ThemeSettings.ScrollbarSettings` | class | `ThemeSettings.SelectableSettings_Base` | 63733 |
| `ThemeSettings.SliderSettings` | class | `ThemeSettings.SelectableSettings_Base` | 63714 |
| `ThemedElement` | class | `MonoBehaviour` | 63669 |
| `ThemedElement.ElementInfo` | class |  | 63655 |
| `TimeScaleController` | class | `MonoBehaviour, IDataLoader` | 4315 |
| `TimeScaleController.Context` | enum |  | 4270 |
| `TimeScaleController.OnTimeScaleChange` | class | `MulticastDelegate` | 4261 |
| `ToggleGroup` | class | `MonoBehaviour` | 48421 |
| `ToggleInfo` | class | `InputFieldInfo` | 63963 |
| `TransformController` | class | `MonoBehaviour` | 39606 |
| `TransformExt` | class |  | 55505 |
| `TransformTools` | class |  | 39577 |
| `TransitionController` | class |  | 25089 |
| `TranslationFlag` | enum |  | 66828 |
| `TranslationJob` | class | `IDisposable` | 65796 |
| `TranslationJob.eJobState` | enum |  | 65787 |
| `TranslationJob_GET` | class | `TranslationJob_WWW` | 65815 |
| `TranslationJob_Main` | class | `TranslationJob` | 65829 |
| `TranslationJob_POST` | class | `TranslationJob_WWW` | 65844 |
| `TranslationJob_WEB` | class | `TranslationJob_WWW` | 65870 |
| `TranslationJob_WWW` | class | `TranslationJob` | 65806 |
| `TranslationQuery` | struct |  | 65776 |
| `Tutorial.Mode` | enum |  | 39640 |
| `TutorialsInfo` | class |  | 39982 |
| `TypeTools` | class |  | 39583 |
| `UnitySourceGeneratedAssemblyMonoScriptTypes_v1` | class |  | 57321 |
| `UnitySourceGeneratedAssemblyMonoScriptTypes_v1.MonoScriptData` | struct |  | 57311 |
| `User` | class |  | 54590 |
| `User.UserInfo` | class | `IRunFlow` | 54494 |
| `UserDataStore_File` | class | `UserDataStore_KeyValue` | 59008 |
| `UserDataStore_File.CLZF2` | class | `UserDataStore_File.Codec` | 58982 |
| `UserDataStore_File.Codec` | class |  | 58966 |
| `UserDataStore_File.DataFormat` | enum |  | 59000 |
| `UserDataStore_File.DataStore` | class | `UserDataStore_KeyValue.IDataStore` | 58939 |
| `UserDataStore_KeyValue` | class | `UserDataStore` | 59184 |
| `UserDataStore_KeyValue.IDataStore` | interface |  | 59095 |
| `UserDataStore_KeyValue.JoystickAssignmentHistoryInfo` | class |  | 59086 |
| `UserDataStore_PlayerPrefs` | class | `UserDataStore` | 59443 |
| `UserDataStore_PlayerPrefs.JoystickAssignmentHistoryInfo` | class |  | 59355 |
| `ValueBoostCounter` | class | `MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerE...` | 11322 |
| `ValueCounter` | class | `MonoBehaviour` | 11339 |
| `VectorExt` | class |  | 55514 |
| `VectorUtil` | class |  | 57299 |
| `VectorUtil.AxisType` | enum |  | 57277 |
| `VectorUtil.Random` | enum |  | 57291 |
| `VibrationTest` | class | `MonoBehaviour` | 65471 |
| `WaveData` | class |  | 54148 |
| `Window` | class | `MonoBehaviour` | 64156 |
| `Window.Timer` | class |  | 64120 |
| `WishlistFeature` | class | `MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerE...` | 49713 |
| `ePluralType` | enum |  | 65649 |
| `eSpreadsheetUpdateMode` | enum |  | 66181 |
| `eTermType` | enum |  | 66810 |

## Tiers (Rewired / I2 Localization / exemples)

| Type | Genre | Hérite / implémente | Ligne |
|---|---|---|---|
| `ControlMapper` | class | `MonoBehaviour` | 62464 |
| `ControlMapper.AxisCalibrator` | class |  | 62086 |
| `ControlMapper.GUIButton` | class | `ControlMapper.GUIElement` | 61656 |
| `ControlMapper.GUIElement` | class |  | 61617 |
| `ControlMapper.GUIInputField` | class | `ControlMapper.GUIElement` | 61680 |
| `ControlMapper.GUILabel` | class |  | 61738 |
| `ControlMapper.GUIToggle` | class | `ControlMapper.GUIElement` | 61720 |
| `ControlMapper.InputActionSet` | class |  | 62025 |
| `ControlMapper.InputBehaviorSettings` | class |  | 61802 |
| `ControlMapper.InputGrid` | class |  | 62157 |
| `ControlMapper.InputGridEntryList` | class |  | 62285 |
| `ControlMapper.InputGridEntryList.ActionCategoryEntry` | class |  | 62248 |
| `ControlMapper.InputGridEntryList.ActionEntry` | class |  | 62214 |
| `ControlMapper.InputGridEntryList.FieldSet` | class |  | 62239 |
| `ControlMapper.InputGridEntryList.MapCategoryEntry` | class |  | 62189 |
| `ControlMapper.InputMapping` | class |  | 62039 |
| `ControlMapper.LayoutElementSizeType` | enum |  | 62132 |
| `ControlMapper.MappingSet` | class |  | 61775 |
| `ControlMapper.MappingSet.ActionListMode` | enum |  | 61767 |
| `ControlMapper.Prefabs` | class |  | 61848 |
| `ControlMapper.References` | class |  | 61899 |
| `ControlMapper.WindowManager` | class |  | 62319 |
| `ControlMapper.WindowType` | enum |  | 62140 |
| `ControlMapperDemoMessage` | class | `MonoBehaviour` | 65107 |
| `CustomPlatformHardwareJoystickMapPlatformDataSet` | class | `ScriptableObject` | 65247 |
| `CustomPlatformHardwareJoystickMapProvider` | class | `IHardwareJoystickMapCustomPlatformMapProvider` | 65264 |
| `CustomPlatformHardwareJoystickMapProvider.PlatformDataSet` | class |  | 65255 |
| `CustomPlatformManager` | class | `MonoBehaviour, ICustomPlatformInitializer` | 65275 |
| `CustomPlatformType` | enum |  | 65284 |
| `DeckExampleDice` | class | `MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerE...` | 4479 |
| `DualShock4SpecialFeaturesExample` | class | `MonoBehaviour` | 64694 |
| `DualShock4SpecialFeaturesExample.Touch` | class |  | 64676 |
| `EightPlayersExample_Player` | class | `MonoBehaviour` | 64723 |
| `Example_ChangeLanguage` | class | `MonoBehaviour` | 65497 |
| `Example_LocalizedString` | class | `MonoBehaviour` | 65507 |
| `GamepadTemplate` | class | `ControllerTemplate, IGamepadTemplate, IControllerTemplate` | 57912 |
| `GamepadTemplateUI` | class | `MonoBehaviour` | 65194 |
| `GamepadTemplateUI.Stick` | class |  | 65167 |
| `GamepadTemplateUI.UIElement` | class |  | 65185 |
| `GlobalParametersExample` | class | `RegisterGlobalParameters` | 65518 |
| `PlayerMouseSpriteExample` | class | `MonoBehaviour` | 64766 |
| `PlayerPointerEventHandlerExample` | class | `MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerE...` | 64792 |
| `PressAnyButtonToJoinExample_Assigner` | class | `MonoBehaviour` | 64834 |
| `PressAnyButtonToJoinExample_GamePlayer` | class | `MonoBehaviour` | 64844 |
| `PressStartToJoinExample_Assigner` | class | `MonoBehaviour` | 64874 |
| `PressStartToJoinExample_Assigner.PlayerMap` | class |  | 64865 |
| `PressStartToJoinExample_GamePlayer` | class | `MonoBehaviour` | 64890 |
| `RewiredEventSystem` | class | `EventSystem` | 59635 |
| `RewiredInputManager.ControllerIcons` | enum |  | 21137 |
| `RewiredInputManager.InputType` | enum |  | 21160 |
| `RewiredInputManager.SupportedInput` | enum |  | 21147 |
| `RewiredPointer.State` | enum |  | 20770 |
| `RewiredPointerInputModule` | class | `BaseInputModule` | 59723 |
| `RewiredPointerInputModule.ButtonState` | class |  | 59671 |
| `RewiredPointerInputModule.MouseButtonEventData` | class |  | 59660 |
| `RewiredPointerInputModule.MouseState` | class |  | 59648 |
| `RewiredPointerInputModule.UnityInputSource` | class | `IMouseInputSource, ITouchInputSource` | 59687 |
| `RewiredStandaloneInputModule` | class | `RewiredPointerInputModule` | 59796 |
| `RewiredStandaloneInputModule.PlayerSetting` | class |  | 59785 |
| `TouchButtonExample` | class | `MonoBehaviour, IPointerDownHandler, IEventSystemHandler, IPointerUp...` | 64629 |
| `TouchJoystickExample` | class | `MonoBehaviour, IPointerDownHandler, IEventSystemHandler, IPointerUp...` | 64647 |
