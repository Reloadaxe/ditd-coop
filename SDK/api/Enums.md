# Énumérations — Die in the Dungeon 5.1.0

Généré depuis le dump IL2CPP. Utilisables directement en C# via les assemblies d'interop (ex. `RelicData.Title.XXX`).

### BackGroundMode (5)
`GenericRandom = 0`, `RandomBattleMode = 1`, `CustomBattleMode = 2`, `RandomEventMode = 3`, `CustomEventMode = 4`

### BackgroundVariation.Type (4)
`Far = 0`, `Back = 1`, `Mid = 2`, `Front = 3`

### BattleFX.Type (36)
`None = 0`, `HorizontalNormalSlash = 1`, `SmallHit = 2`, `IncreaseBlock = 3`, `Boost = 4`, `Heal = 5`, `HorizontalFastSlash = 6`, `DecreaseBlock = 7`, `BreakBlock = 8`, `BigHit = 9`, `SpearArrowHit = 10`, `AxeHit = 11`, `FlashDice = 12`, `TerrainSlot = 13`, `PoisonHit = 14`, `ImprovementGlassDice = 15`, `ImprovementGrowDice = 16`, `ImprovementHeavyDice = 17`, `ImprovementHollowDice = 18`, `ImprovementFlashDice = 19`, `CockroachRevive = 20`, `Music = 21`, `GooeyHit = 22`, `TierGoldDice = 23`, `TierSilverDice = 24`, `ShopSparkle = 25`, `Narcissist = 26`, `Tasty = 27`, `Precise = 28`, `Barrier = 29`, `Regeneration = 30`, `Poison = 31`, `Thorns = 32`, `ThornsAttack = 33`, `RelicTransformToWithered = 34`, `RelicTransformToMagnificent = 35`

### BattleFX.Context (2)
`Default = 0`, `Map = 1`

### BaseBoardSlot.Special (27)
`None = 0`, `HoneyCovered = 1`, `RoyalHoneyCovered = 2`, `CrownMarked = 3`, `Tangled = 4`, `Infected = 5`, `SmallOpening = 6`, `Exposed = 7`, `Spited = 8`, `Reaped = 9`, `Monitored = 10`, `ReapedAttack = 11`, `ReapedBlock = 12`, `Haste = 13`, `Sneak = 14`, `VoltPlus = 15`, `VoltMinus = 16`, `ElectricityConnected = 17`, `ElectricityInterrupted = 18`, `LeakSmall = 19`, `LeakMedium = 20`, `LeakBig = 21`, `Tick = 22`, `MonitoredTangled = 23`, `Catch = 24`, `Nullifier = 25`, `EvilEye = 26`

### BattleBoard.ExtraGraphics (2)
`None = 0`, `RedHoodMonitor = 1`

### BattleBoard.SlotDistanceStyle (2)
`FourBased = 0`, `SixBased = 1`

### BattleBoard.PlaceDiceContext (3)
`System = 0`, `Player = 1`, `Enemy = 2`

### BattleBoard.DiscardMode (3)
`NoDiscard = 0`, `DiscardToRemoveFromBoard = 1`, `DiscardToMoveSlot = 2`

### BoardSlot.ShapeResources.Position (3)
`Top = 0`, `Center = 1`, `Bottom = 2`

### BoardSlot.BoardDirection (4)
`Up = 0`, `Right = 1`, `Down = 2`, `Left = 3`

### BoardSlot.FlashActionMode (3)
`ExecuteActions = 0`, `ApplyBoost = 1`, `DoNothing = 2`

### ECardinalPosition (9)
`C = 0`, `N = 1`, `E = 2`, `S = 3`, `W = 4`, `NE = 5`, `NW = 6`, `SE = 7`, `SW = 8`

### ExtraSelectable.InteractionContext (2)
`Click = 0`, `Hover = 1`

### CinematicPlayer.FailReason (3)
`None = 0`, `Timeout = 1`, `Error = 2`

### ICurrencyData.VariationModeExecution (2)
`Direct = 0`, `Queue = 1`

### TimeScaleController.Context (9)
`SettingsButton = 1`, `GiveUpButton = 2`, `FeedbackMenu = 3`, `GameplayPersistentValue = 4`, `SettingsValueChange = 5`, `DebugMouseWheel = 100`, `DebugPauseKey = 101`, `DebugNextFrame = 102`, `DevMenuSpeedButtons = 103`

### DialogSystem.TextSpeed (4)
`Slow = 0`, `Default = 1`, `Fast = 2`, `VeryFast = 3`

### ActiveDice.DiceSuffixMode (4)
`None = 0`, `DiceWord = 1`, `DiceFaceAmount = 2`, `TypeWord = 3`

### ActiveDice.FontSizeMode (3)
`NoChanges = 0`, `LanguageDefault = 1`, `CodexTooltip = 2`

### BodySpriteInfo.SpriteInfoType (3)
`Hand = 0`, `Board = 1`, `ShopHover = 2`

### CustomSpriteHolder.ValueMode (2)
`Default = 0`, `Custom = 1`

### DiceBoxDice.Context (6)
`None = 0`, `DiceBox = 1`, `CursorDrag = 2`, `OptionSlot = 3`, `Dummy = 4`, `RunHistory = 5`

### DiceManager.StarterDeckType (4)
`CharacterDeck = 0`, `SpecificDeck = 1`, `MultipleDecks = 2`, `DiceList = 3`

### DiceManager.DiceFaceMode (2)
`Unchanged = 0`, `OverrideFaces = 1`

### GameObjectDice.CustomAnimation (4)
`None = 0`, `Ant = 1`, `Sneak = 2`, `EvilEye = 3`

### GameObjectDice.EnergyCostContext (3)
`OnlyCheck = 0`, `AddEnergy = 1`, `RemoveEnergy = 2`

### ModifierData.ReduceMode (6)
`None = 0`, `ValueHalf = 1`, `ValueDamage = 2`, `ValueReduceTurnEnd = 3`, `ValueReduceTurnStart = 4`, `ValueReduceToZero = 5`

### ModifierData.Modifier (74)
`None = 0`, `MuscleUp = 1`, `Aggro = 2`, `Tombstone = 3`, `Presence = 4`, `Resilient = 5`, `BlueCage = 6`, `GreenCage = 7`, `Vendetta = 8`, `Monitor = 9`, `Tasty = 10`, `FastTrack = 11`, `Copycat = 12`, `Booster = 13`, `Haze = 14`, `RedCage = 15`, `Vile = 16`, `Pestilence = 17`, `Disruptible = 18`, `Caregiver = 19`, `QuickCast = 20`, `Poison = 21`, `Thorns = 22`, `BadOmen = 23`, `Petty = 24`, `Rage = 25`, `Fortify = 26`, `Backstab = 27`, `Narcissist = 28`, `Neutralizer = 29`, `Spite = 30`, `Martyr = 31`, `Regeneration = 32`, `Barrier = 33`, `Reaper = 34`, `Organized = 35`, `Tangler = 36`, `Bonus = 37`, `Lucky = 38`, `Energizer = 39`, `Retaliate = 40`, `Generous = 41`, `Spawner = 42`, `Sedentary = 43`, `Hasty = 44`, `Timid = 45`, `Voltage = 46`, `Poisonous = 47`, `Might = 48`, `Lazy = 49`, `Bulwark = 50`, `StanceThrust = 51`, `StanceSweep = 52`, `StanceParry = 53`, `Weak = 54`, `Boosted = 55`, `Extra = 56`, `Brewmaster = 57`, `Pure = 58`, `Machination = 59`, `MonitorTangled = 60`, `Adamant = 61`, `Charge = 62`, `Pickpocket = 63`, `Coward = 64`, `Greedy = 65`, `Opulent = 66`, `Parry = 67`, `SuperTasty = 68`, `Corruption = 69`, `Perception = 70`, `Creative = 71`, `HighTempo = 72`, `Test = 999`

### MonitorModifierData.Phase (2)
`One = 0`, `Two = 1`

### TastyModifierData.Mode (9)
`DamagePlayer = 0`, `DamageRandomEnemy = 1`, `GrantBlockPlayer = 2`, `HealPlayer = 3`, `EnergyPlayer = 4`, `PoisonPlayer = 5`, `PoisonEnemy = 6`, `ThornsPlayer = 7`, `ThornsEnemy = 8`

### ReapedModifierData.Phase (2)
`One = 0`, `Two = 1`

### BaseStanceModifierData.Level (3)
`One = 0`, `Two = 1`, `Three = 2`

### ChargeModifierData.Mode (4)
`Inactive = 0`, `ChargingAttack = 1`, `ReceivedUnblockedDamage = 2`, `ReadyToAttack = 3`

### SuperTastyModifierData.Mode (3)
`GrantBlockPlayer = 0`, `HealPlayer = 1`, `IncreaseMaxHealthPlayer = 2`

### CreativeModifier.CreativeActions (5)
`NONE = 0`, `Sweep = 1`, `Cross = 2`, `Spin = 3`, `Pass = 4`

### PerceptionModifier.State (2)
`Dormant = 0`, `Awake = 1`

### SpellData.Name (2)
`None = 0`, `WeakSpot = 1`

### DLCManager.TitleDLC (3)
`None = 0`, `SupporterPack = 1`, `CinderPlush = 2`

### BattleAction.EnemyModifiers.Target (2)
`Player = 0`, `EnemySelf = 1`

### GiveDiceBattleAction.Target (3)
`DrawPile = 0`, `DiscardPile = 1`, `Hand = 2`

### EnemyPointer.Mode (2)
`Auto = 0`, `Select = 1`

### EntityComposedMarker.InfoMode (2)
`HorizontalInfo = 0`, `VerticalAndHorizontalInfo = 1`

### EntityMarker.PositionMode (2)
`Default = 0`, `ShieldActive = 1`

### EntityMarker.MarkerType (6)
`NONE = 0`, `Attack = 1`, `Shield = 2`, `Heal = 3`, `Boost = 4`, `Modifier = 5`

### Companion.Name (4)
`None = 0`, `BabyFly = 1`, `PotionMimic = 2`, `CinderPlush = 3`

### BeetballEnemy.BeetballAction (4)
`None = 0`, `RegularAttack = 1`, `AttackWithCatchSlot = 2`, `ChargeAction = 3`

### Enemy.Name (60)
`Fly = 0`, `Spider = 1`, `Moth = 2`, `Beetle = 3`, `Ant = 11`, `Axe = 12`, `Cockroach = 15`, `Cicada = 20`, `Plague = 29`, `CaterpillarHealPlayer = 30`, `CaterpillarBlockPlayer = 31`, `CaterpillarDamageEnemy = 32`, `CaterpillarDamagePlayer = 33`, `Spitter = 34`, `Nun = 35`, `Mage = 39`, `Spore = 40`, `CaterpillarEnergyPlayer = 43`, `CaterpillarPoisonPlayer = 44`, `SentientJewelBeetle = 45`, `CaterpillarPoisonEnemy = 46`, `CaterpillarThornsPlayer = 47`, `CaterpillarThornsEnemy = 48`, `BeeWorker = 49`, `Tick = 50`, `Sneaky = 52`, `Firefly = 53`, `MinionDragonfly = 54`, `RedHood = 301`, `Beetball = 302`, `Cicandela = 303`, `EliteDragonfly = 304`, `CaterpillarFamily = 201`, `ScorpionMount = 101`, `ScorpionRider = 102`, `CaterpillarBoss = 113`, `BeeQueen = 8`, `SickleMantis = 111`, `ScytheMantis = 112`, `MosquitoDuke = 114`, `Rain = 115`, `ProtoCockroach = 25`, `Revenger = 36`, `Jinxipede = 37`, `Irascible = 38`, `Backstabber = 41`, `Spiteful = 42`, `Propeller = 51`, `EliteFly = 4`, `EliteSpider = 5`, `EliteMoth = 6`, `EliteBeetle = 7`, `StrongerBeeQueen = 9`, `EndlessQueenBee = 26`, `LesserScorpionMount = 1101`, `LesserScorpionRider = 1102`, `EvilFrogFirstEncounter = 14`, `EvilFrogSecondEncounter = 10`, `EvilFrogThirdEncounter = 28`, `EndlessEvilFrog = 27`

### Enemy.ActionOrder (2)
`BeforePlayer = 0`, `AfterPlayer = 1`

### Enemy.TurnActions.TurnActionsConditions (3)
`NONE = 0`, `Alone = 1`, `Group = 2`

### EvilFrog.Phase (2)
`First = 0`, `Second = 1`

### EvilFrog.Variant (3)
`FirstEncounter = 0`, `SecondEncounter = 1`, `ThirdEncounter = 2`

### MosquitoDukeEnemy.Level (3)
`One = 0`, `Two = 1`, `Three = 2`

### RainEnemy.Phase (2)
`First = 1`, `Second = 2`

### RainEnemy.Level (5)
`One = 0`, `Two = 1`, `Three = 2`, `Four = 3`, `Five = 4`

### Entity.Side (2)
`Player = 0`, `Enemy = 1`

### Entity.BlockReductionMode (3)
`ReduceToZero = 0`, `ReduceToHalf = 1`, `KeepSameValue = 2`

### HealActiveData.Context (11)
`Dev = 0`, `Behaviour = 1`, `OtherEntity = 2`, `Reward = 3`, `Relic = 4`, `TileMap = 5`, `Dice = 6`, `ModifierDice = 7`, `ModifierEnemy = 8`, `Event = 9`, `Runestone = 10`

### Player.CharacterName (5)
`None = -1`, `Cinder = 0`, `Lisver = 1`, `Nera = 2`, `Mango = 3`

### VisualEntity.SpecialAttackType (3)
`None = 0`, `Flash = 1`, `Parry = 2`

### FXType (1)
`Boost = 0`

### BattleChallenges.Challenge (45)
`None = 0`, `NoBlockOverMaxHP = 1`, `OnOddTurnsNoAttackDice = 2`, `OnOddTurnsNoBlockDice = 3`, `OnOddTurnsNoHealDice = 4`, `OnOddTurnsNoBoostDice = 5`, `OnOddTurnsNoMirrorDice = 6`, `OnEvenTurnsNoAttackDice = 7`, `OnEvenTurnsNoBlockDice = 8`, `OnEvenTurnsNoHealDice = 9`, `OnEvenTurnsNoBoostDice = 10`, `OnEvenTurnsNoMirrorDice = 11`, `NoModifierGreaterThan10 = 13`, `NoFullHPOnBattleEnd = 14`, `NoBattleEndUntilTurn7 = 15`, `NoPotions = 16`, `OnOddTurnsNoEnemyDamage = 17`, `OnEvenTurnsNoEnemyDamage = 18`, `NoChangeEnemyTarget = 19`, `NoKillMoreThan2Enemies = 20`, `KillEnemyLowHPLast = 21`, `KillEnemiesWithLess5HP = 22`, `EndTurnWithEnergy = 23`, `OnOddTurnsNoD4s = 24`, `OnEvenTurnsNoD4s = 25`, `NoEnergyTurn1 = 26`, `HalfOrLessHPOnBattleEnd = 27`, `NoDiceNextToAnother = 28`, `NoMoreThan2OddDice = 29`, `NoMoreThan2EvenDice = 30`, `BoostDiceTargetAllSlots = 31`, `NoDrawButton = 100`, `NoRerollButton = 101`, `EndBattlePoisonous = 200`, `NoFlashDice = 201`, `NoUnlockDice = 300`, `OnOddTurnsNoMoreThreeDice = 301`, `OnEvenTurnsNoMoreThreeDice = 302`, `MaxParry5Damage = 400`, `EndBattle5MoreEnergy = 401`, `UltimateKillScorpionRider = 900`, `UltimateKillCaterpillarBaker = 901`, `UltimateKillQueenBee = 902`, `UltimateKillMantisDuo = 903`, `UltimateKillMosquitoDuke = 904`

### BattleChallenges.RewardType (24)
`None = 0`, `GainCoins = 100`, `GainHealHP = 110`, `GainMaxHP = 111`, `GainPermanentModifierBonus = 120`, `GainPermanentModifierLucky = 121`, `GainPermanentModifierBoosted = 122`, `GainPermanentModifierBrewmaster = 123`, `GainRemoveCurse = 190`, `IncludeRarityDice = 200`, `IncludeRarityRelic = 201`, `NextBattleEnemiesReduceHP = 300`, `NextBattleFullHandTurn1 = 301`, `NextBattleBlockTurn1 = 302`, `NextBattleEnergyTurn1 = 303`, `NextShopReducePrices = 400`, `NextEventTargetRoll = 500`, `IgnoreNextBattleTile = 600`, `ReplaceTileWithTreasure = 601`, `RevealAllUnknownAreaTiles = 602`, `ForceNextUnknownTileToDiscard = 603`, `ForceNextUnknownTileToRest = 604`, `ReplaceBossRelic = 900`, `TriggerTrueFinalBoss = 1000`

### BattleChallenges.DebugForceMode (4)
`Default = 0`, `ForceEnabled = 1`, `ForceDisabled = 2`, `ForceAllChallenges = 3`

### BattleChallenges.Context (2)
`Default = 0`, `EliteDragonfly = 1`

### BattleChallenges.ActiveChallenge.Status (3)
`Active = 0`, `Completed = 1`, `Failed = 2`

### BattleSystem.TurnState (24)
`None = 0`, `Intro = 1`, `StartTurn = 2`, `DrawDice = 3`, `PreBoardActions = 4`, `ShowBoard = 5`, `TurnBeginningActions = 6`, `EnemySelectDice = 7`, `SelectDice = 8`, `PerformFlashDice = 9`, `UseRunestones = 10`, `EnemyPhaseBeforePlayer = 11`, `PreExecuteDice = 12`, `ExecuteDice = 13`, `EnemyPhaseAfterPlayer = 14`, `EndTurn = 15`, `DefeatedAllEnemies = 16`, `WinBattle = 17`, `LoseBattle = 18`, `SelectReward = 19`, `ContinueAfterWin = 20`, `CompleteArea = 21`, `CompleteRun = 22`, `EndlessProposal = 23`

### BattleSystem.EndState (3)
`Win = 0`, `Lose = 1`, `ForceFinish = 2`

### SpawnPositions.Type (11)
`Default = 0`, `Single = 1`, `BeeQueenBoss = 2`, `EvilFrogBoss = 3`, `ScorpionBoss = 4`, `EndlessScorpionBoss = 5`, `MantisBosses = 6`, `CaterpillarBoss = 7`, `MosquitoDukeBoss = 8`, `EliteDragonfly = 9`, `Rain = 10`

### EventController.SystemMode (2)
`Simple = 0`, `OptionSlots = 1`

### FloorSystem.TestTileMode (2)
`FirstTile = 0`, `AllTiles = 1`

### InputActionType (51)
`CursorDisplacementLeft = 0`, `CursorDisplacementRight = 1`, `CursorDisplacementUp = 2`, `CursorDisplacementBottom = 3`, `CursorSelection = 4`, `CursorDeselection = 5`, `Sliders = 6`, `Scrolls = 7`, `BackpackShortcut = 8`, `BattleSettingsShortcut = 9`, `RelicsShortcut = 10`, `EnemiesShortcut = 11`, `ShowDicesDescription = 12`, `GoBackShortcut = 13`, `FinishTurnShortcut = 14`, `SkipShortcut = 15`, `DialogueFocusModeShortcut = 16`, `DiceBoxDiceShortcut = 17`, `EventOptionShortcut = 18`, `NextOptionShortcut = 19`, `PlayerShortcut = 20`, `TutorialShortcut = 21`, `DrawShortcut = 22`, `MapShortcut = 23`, `ConfirmShortcut = 24`, `ResetShortcut = 25`, `PlusOneShortcut = 26`, `MinusOneShortCut = 27`, `RestartRunShortcut = 28`, `QuitRunShortcut = 29`, `PreviousOptionShortcut = 30`, `NextCategoryShortcut = 31`, `PreviousCategoryShortcut = 32`, `ContinueShortcut = 33`, `StarRunShortcut = 34`, `ToggleOptionShortcut = 35`, `TwitterShortcut = 36`, `DiscordShortcut = 37`, `RerollShortcut = 38`, `TileOption1 = 39`, `TileOption2 = 40`, `TileOption3 = 41`, `ShowObtainableTooltipShortcut = 42`, `EndBackpackShortcut = 43`, `EndStatsShortcut = 44`, `IntroCinematicShortcut = 45`, `TrueEndingCinematicShortcut = 46`, `AltOptions = 47`, `PjProgression = 48`, `TitleScreenChallenges = 49`, `ChangeDifficulty = 50`

### CharacterLevel.Level (12)
`None = 0`, `One = 1`, `Two = 2`, `Three = 3`, `Four = 4`, `Five = 5`, `Six = 6`, `Seven = 7`, `Eight = 8`, `Nine = 9`, `Ten = 10`, `Max = 11`

### Localization.Language (11)
`English = 0`, `Spanish = 1`, `ChineseSimplified = 2`, `Japanese = 3`, `Russian = 4`, `Korean = 5`, `Turkish = 6`, `German = 7`, `French = 8`, `PortugueseBrazil = 9`, `Polish = 10`

### Localization.GenericString (13)
`EventContinue = 0`, `MessageUnlocked = 1`, `MessageAddDice = 2`, `WordDie = 3`, `WordDice = 4`, `WordProperty = 5`, `Tutorial = 6`, `CurrencySingular = 7`, `CurrencyPlural = 8`, `WordTier = 9`, `WordAverage = 10`, `Energy = 11`, `WordType = 12`

### Localization.GenericKeyword (82)
`Coin = 0`, `Coins = 1`, `Energy = 2`, `DrawPile = 3`, `Reroll = 4`, `FirstFace = 5`, `LastFace = 6`, `ExtraValue = 7`, `BattleBoard = 8`, `EvenValue = 9`, `OddValue = 10`, `EvenValues = 11`, `OddValues = 12`, `Attack = 13`, `Block = 14`, `Heal = 15`, `Boost = 16`, `HP = 17`, `Enemy = 18`, `Enemies = 34`, `EnemyTarget = 19`, `Damage = 20`, `DamageUnblocked = 21`, `DamageExcess = 22`, `DrawDice = 23`, `LockedDice = 24`, `Reward = 25`, `Rewards = 26`, `Turn = 27`, `Turns = 28`, `OddTurn = 29`, `OddTurns = 30`, `EvenTurn = 31`, `EvenTurns = 32`, `Hand = 33`, `Battle = 35`, `Battles = 36`, `EliteBattle = 37`, `EliteBattles = 38`, `EnemyFlyfant = 39`, `FaceValue = 40`, `RemoveRelic = 41`, `BoardSlot = 42`, `Kill = 43`, `KillAccurate = 44`, `KillPerfect = 45`, `Shop = 46`, `Forge = 47`, `Map = 48`, `Dungeon = 49`, `Loop = 50`, `Relic = 51`, `Relics = 52`, `CornerSlots = 53`, `CenterSlots = 54`, `DiscardDice = 55`, `Deck = 56`, `MaxHP = 57`, `RemoveDice = 58`, `DestroyDice = 59`, `Curse = 60`, `RemoveCurse = 61`, `ObtainDice = 62`, `DiceProperty = 63`, `DiceProperties = 64`, `MapTile = 65`, `UpgradeDice = 66`, `RemoveProperty = 67`, `NegativeExtraValue = 68`, `SkipDialogue = 69`, `ShowInformation = 70`, `FinishTurn = 71`, `SelectEnemy = 72`, `GenericSelection = 73`, `NextOptionTrigger = 74`, `PreviousOptionTrigger = 75`, `NextOptionSkin = 76`, `NextOptionCompanion = 77`, `Potion = 78`, `Flee = 79`, `Challenge = 80`, `Combine = 81`

### Localization.CustomString.Mode (2)
`RawText = 0`, `StringID = 1`

### UIElement (1)
`EventOption = 0`

### WordGenericTextID.GenericWord (14)
`Deck = 0`, `Dice = 1`, `Die = 2`, `Discard = 3`, `Done = 4`, `Exile = 5`, `Faces = 6`, `Finish = 7`, `Modifier = 8`, `Property = 9`, `Relics = 10`, `Tutorial = 11`, `Average = 12`, `Tier = 13`

### CameraController.Mode (4)
`None = 0`, `Map_FollowPlayer = 1`, `Battle_Static = 2`, `Free_Movement = 3`

### CanvasManager.RenderTransform.Conditions (4)
`ShopClosed = 0`, `ModifyDiceFacesClosed = 1`, `BackpackClosed = 2`, `BackpackNotModifyingDice = 3`

### GameManager.GameState (19)
`NONE = 0`, `GameBoot = 1`, `TitleScreen = 2`, `RunStart = 3`, `Map = 4`, `MapDeath = 5`, `MapAreaFinish = 6`, `FloorInit = 7`, `BattleFloor = 8`, `EventFloor = 9`, `LootFloor = 10`, `ShopFloor = 11`, `DungeonFloorInit = 12`, `DungeonFloorProgress = 13`, `FloorEndSuccess = 14`, `FloorEndFail = 15`, `GameOver = 16`, `RunFinish = 17`, `GameWin = 18`

### GameManager.BuildMode (3)
`Release = 0`, `Testing = 1`, `Demo = 2`

### GameplayContext.Context (24)
`NONE = 0`, `BattleMenu = 1`, `BattleBackpack = 2`, `Shopping = 3`, `FacesModification = 4`, `Battle = 5`, `TittleMenu = 6`, `DiceSelection = 7`, `RelicSelection = 8`, `RewardsSelection = 9`, `Event = 10`, `Map = 11`, `CharacterSelection = 12`, `BoardDicePlacement = 13`, `HighPriorityUI = 14`, `EndMenu = 15`, `NonInteractionTutorial = 16`, `InteractionTutorial = 17`, `FeedbackMenu = 18`, `PotionsSelection = 19`, `RunHistory = 20`, `RunHistoryEntry = 21`, `AltSubmenu = 22`, `Cinematic = 23`

### SoundManager.Theme (21)
`None = 0`, `Battle = 1`, `Tutorial = 15`, `Dungeon = 17`, `RoomEventChill = 30`, `RoomEventDangerous = 31`, `Victory = 40`, `GameOver = 41`, `Map = 80`, `TitleScreen = 100`, `CharacterSelection = 101`, `ScorpionBoss = 200`, `MantisBoss = 201`, `MantisBossFast = 20`, `CaterpillarBoss = 202`, `DukeMosquitoBoss = 203`, `QueenBeeBoss = 204`, `RainBossDialog = 205`, `RainBoss = 206`, `GeneralShop = 300`, `Forge = 301`

### SoundManager.Channel (6)
`PLAYER = 0`, `ENEMY = 1`, `SYSTEM = 2`, `EXTRA = 3`, `UI = 4`, `Length = 5`

### ActiveMapChunk.Params.PathMode (2)
`FullyProcedural = 0`, `ProceduralPerPath = 1`

### Grid.Directions.Direction (9)
`None = 0`, `Up = 1`, `UpRight = 2`, `Right = 3`, `DownRight = 4`, `Down = 5`, `DownLeft = 6`, `Left = 7`, `UpLeft = 8`

### Grid.OverrideColumnData.OverrideMode (2)
`FullColumn = 0`, `SpecificAmount = 1`

### Map.MapState (10)
`None = 0`, `SetupNewMapArea = 1`, `SetupLoadGame = 2`, `SetupEndlessMap = 3`, `SetupLoadEndless = 4`, `StartTurn = 5`, `SelectTile = 6`, `MovePlayer = 7`, `TileAction = 8`, `AfterTileFinish = 9`

### Map.Part (3)
`None = 0`, `Left = 1`, `Right = 2`

### MapTileLine.LineDirection (5)
`Straight = 0`, `DiagonalUpwards = 1`, `DiagonalDownwards = 2`, `DiagonalExtremeUpwards = 3`, `DiagonalExtremeDownwards = 4`

### TileShortcutOption (4)
`NONE = 99`, `Option1 = 0`, `Option2 = 1`, `Option3 = 2`

### MapToken.MovementType (2)
`PerTile = 0`, `Continuos = 1`

### ModifyDiceFaceData.ModifyMode (10)
`Increase = 0`, `Decrease = 1`, `Redistribute = 2`, `ChangeToSpecific = 3`, `ForceFacesValue = 4`, `ForceIncreaseValue = 5`, `ForceDecreaseValue = 6`, `ForceLowestFacesAndHighestLastFace = 7`, `ForceLadderValues = 8`, `RandomizeAllFaces = 9`

### ModifyDiceFaceData.PointsMode (3)
`BasedOnDiceShape = 0`, `CustomPoints = 1`, `Unlimited = 2`

### ObtainableRelic.Context (2)
`ObtainableRelic = 0`, `Shop = 1`

### Relics.ContextFrom (8)
`Unspecified = 0`, `Treasure = 1`, `EliteBattle = 2`, `BossBattle = 3`, `GeneralShop = 4`, `ForgeShop = 5`, `EventNode = 6`, `EliteDragonfly = 7`

### RewardData.SpriteContext (2)
`Unified = 0`, `Divided = 1`

### RewardData.Type (68)
`None = 0`, `ObtainDice = 1`, `RemoveDiceProperties = 76`, `UpgradeDice = 3`, `Heal = 4`, `EliteBattle = 5`, `Luck = 6`, `UpgradeDiceToSilver = 7`, `UpgradeTwoDiceToSilver = 41`, `UpgradeDiceToGold = 8`, `GrantHollow = 9`, `GrantHeavy = 10`, `GrantGrow = 11`, `GrantFlash = 14`, `GrantDoubleDip = 44`, `GrantRetry = 45`, `GrantToxic = 46`, `GrantPack = 47`, `GrantGroup = 48`, `GrantGravity = 50`, `GrantRetain = 51`, `GrantRandomProperty = 56`, `ObtainRelic = 12`, `ObtainRelicChoose = 13`, `ObtainRelicFromTreasure = 17`, `ObtainRelicFromEliteBattle = 18`, `ObtainRelicFromBossBattle = 19`, `ObtainCurrencyDrop = 15`, `ObtainCurrencyFixedValue = 16`, `ModifyDiceFaces_IncreaseByShape_ToSilver = 70`, `ModifyDiceFaces_IncreaseByShape_ToGold = 73`, `ModifyDiceFaces_RedistributeByShape = 71`, `ModifyDiceFaces_RandomlyIncreaseBy5Points_ToSilver = 72`, `ModifyDiceFaces_RandomlyIncreaseBy5Points_ToGold = 74`, `ModifyDiceFaces_RedistributeByShape_NoCost = 75`, `UpgradeThreeDiceToSilver = 29`, `UpgradeTwoDiceToGold = 30`, `ObtainDiceBasic = 31`, `ObtainDiceSilver = 32`, `ObtainDiceGold = 33`, `HealEnhanced = 34`, `HealLimited = 49`, `HealEndless = 55`, `HealRestEndless = 57`, `ObtainRelicsCommon = 35`, `ObtainRelicsUncommon = 36`, `ObtainRelicsRare = 37`, `ObtainRelicsEpic = 38`, `ObtainRelicsLegendary = 39`, `ObtainInitialRelics = 42`, `ObtainEnemyRelic = 43`, `ObtainDifficultyCurse = 52`, `ObtainEndlessCurse = 58`, `ObtainRelicDragonfly = 59`, `ObtainBossRelic = 60`, `RemoveDice = 2`, `RemoveTwoDice = 40`, `ObtainRunestones = 53`, `ObtainRunestonesFromBossBattle = 54`, `ChallengeObtainCurrency = 90`, `ChallengeHeal = 91`, `ChallengeRemoveCurse = 92`, `ChallengePermanentBonusModifier = 93`, `ChallengePermanentLuckyModifier = 94`, `ChallengePermanentBoostedModifier = 95`, `ChallengePermanentBrewmasterModifier = 96`, `ChallengeMaxHP = 97`, `SkipRewards = 100`

### RewardData.RerollMode (3)
`Selector = 0`, `Reward = 1`, `SelectorAndReward = 2`

### VisualReward.Context (3)
`Reward = 0`, `GeneralShop = 1`, `ForgeShop = 2`

### RunHistoryUI.DetailsNavMode (3)
`None = 0`, `Relics = 1`, `Dices = 2`

### Difficulty.Level (21)
`D0 = 0`, `D1 = 1`, `D2 = 2`, `D3 = 3`, `D4 = 4`, `D5 = 5`, `D6 = 6`, `D7 = 7`, `D8 = 8`, `D9 = 9`, `D10 = 10`, `D11 = 11`, `D12 = 12`, `D13 = 13`, `D14 = 14`, `D15 = 15`, `D16 = 16`, `D17 = 17`, `D18 = 18`, `D19 = 19`, `D20 = 20`

### Difficulty.VisualGroup (11)
`D0 = 0`, `D2 = 1`, `D4 = 2`, `D6 = 3`, `D8 = 4`, `D10 = 5`, `D12 = 6`, `D14 = 7`, `D16 = 8`, `D18 = 9`, `D20 = 10`

### RunInfo.NewRunData.TutorialMode (3)
`LetRunDecide = 0`, `ForceTutorialActive = 1`, `ForceTutorialInactive = 2`

### RunInfo.Stats.Grade (7)
`F = 0`, `E = 1`, `D = 2`, `C = 3`, `B = 4`, `A = 5`, `S = 6`

### SeedSystem.Rng.Context (9)
`General = 0`, `Battle = 1`, `Hand = 2`, `Dice = 3`, `Enemy = 4`, `Relic = 5`, `Event = 6`, `Reward = 7`, `Potion = 8`

### ObtainableRunestone.Context (2)
`Obtainable = 0`, `Shop = 1`

### Runestones.ContextFrom (8)
`Unspecified = 0`, `Battle = 1`, `EliteBattle = 2`, `BossBattle = 3`, `Treasure = 4`, `GeneralShop = 5`, `ForgeShop = 6`, `EventNode = 7`

### CatalogData.ModifierInfo.NameColorMode (3)
`NoColor = 0`, `UseDefault = 1`, `UseGradient = 2`

### CatalogData.ModifierInfo.DescriptionMode (3)
`RawText = 0`, `TextWithValue = 1`, `TextWithCustomParams = 2`

### CatalogData.BoardSlotSpecialInfo.Order (4)
`Back = 0`, `Front = 1`, `UnderSlot = 2`, `BackAndFront = 3`

### CatalogData.TermInfo.Term (4)
`None = 0`, `AccurateKill = 1`, `PerfectKill = 2`, `PermanentModifier = 3`

### CatalogData.BoardExtraGraphicInfo.SpriteData.SizeMode (2)
`NativeSpriteSize = 0`, `CustomSize = 1`

### CatalogData.ChallengeInfo.DescriptionMode (3)
`RawText = 0`, `TextWithValue = 1`, `TextWithCustomParams = 2`

### CatalogData.ChallengeInfo.ExtraInfo (2)
`None = 0`, `EnemiesKilledCurrentBattle = 1`

### CatalogData.NameContext (3)
`Hand = 0`, `Box = 1`, `Reward = 2`

### CinematicData.Title (13)
`None = 0`, `CinderIntroLong = 10`, `CinderIntroShort = 11`, `CinderTrueEnding = 12`, `LisverIntroLong = 20`, `LisverIntroShort = 21`, `LisverTrueEnding = 22`, `MangoIntroLong = 30`, `MangoIntroShort = 31`, `MangoTrueEnding = 32`, `NeraIntroLong = 40`, `NeraIntroShort = 41`, `NeraTrueEnding = 42`

### CinematicData.Type (4)
`None = 0`, `LongIntro = 1`, `ShortIntro = 2`, `TrueEnding = 3`

### CustomRunData.Title (2)
`None = 0`, `EndlessMode = 1`

### CustomRunData.CharacterFilter (2)
`AllAvailable = 0`, `Specific = 1`

### CustomRunData.StartingRelicMode (2)
`Default = 0`, `Specific = 1`

### CustomRunData.LengthMode (2)
`Fixed = 0`, `Endless = 1`

### CustomRunData.PhaseRules.Changes.ValueIncrease.Mode (2)
`Flat = 0`, `Growth = 1`

### CustomRunData.PhaseRules.ModifyReward.TargetTile (2)
`AllTypes = 0`, `SpecificTypes = 1`

### CustomRunData.PhaseRules.FloorMode (3)
`MinFloor = 0`, `MaxFloor = 1`, `FloorRange = 2`

### CustomRunData.PhaseRules.FloorFrequency (3)
`EveryFloor = 0`, `OnceEveryXFloors = 1`, `AdditiveEveryXFloors = 2`

### CustomRunData.PhaseRules.TargetRewardRow (2)
`FirstRewardRow = 0`, `AllRewardRows = 1`

### CustomRunData.CustomDiceReward.AreaMode (2)
`AllAreas = 0`, `SpecificAreas = 1`

### CustomTextData.Gradient.Type (219)
`NoGradient = 0`, `RuntimeGradient = 999`, `BattleBoard = 1`, `BoostIndicator = 2`, `TerrainBoard = 5`, `EntityHealth = 3`, `EntityMaxHealth = 20`, `EntityAttack = 6`, `EntityBlock = 4`, `EntityMaxBlock = 21`, `EntityHeal = 7`, `TargetEnemy = 9`, `DamageCounter = 11`, `BlockCounter = 12`, `HealCounter = 13`, `Events = 14`, `EventsUnavailable = 15`, `EventsDialog = 17`, `EventsObtainRelics = 16`, `EventsObtainDice = 18`, `EventsUpgradeDice = 19`, `TypeAttack = 101`, `TypeBlock = 102`, `TypeHeal = 103`, `TypeTerrain = 104`, `TypeMirror = 105`, `TypePrism = 106`, `TypeThornsBlock = 107`, `TypePoisonAttack = 108`, `TypeBarrierBlock = 109`, `TypeRegenerationHeal = 110`, `TypeSting = 111`, `TypeHoney = 112`, `TypeSneak = 113`, `TypeAnt = 114`, `TypeStickyBoost = 115`, `TypeRoyalJelly = 116`, `PropertyGlass = 201`, `PropertyHeavy = 202`, `PropertyGrow = 203`, `PropertyHollow = 204`, `PropertyFlash = 205`, `PropertyGravity = 206`, `PropertyDoubleDip = 207`, `PropertyRetry = 208`, `PropertyGroup = 209`, `PropertyPack = 210`, `PropertyPointy = 211`, `PropertyRetain = 212`, `PropertyToxic = 213`, `PropertyFaulty = 214`, `PropertyFilled = 215`, `PropertyUnstable = 216`, `PropertyDecadent = 217`, `DiceRarityCommon = 300`, `DiceRarityUncommon = 301`, `DiceRarityRare = 302`, `DiceRarityVeryRare = 303`, `DiceRarityLegendary = 304`, `FaceValue = 600`, `EvenFace = 601`, `OddFace = 602`, `RerollFace = 603`, `ExtraValue = 604`, `NegativeExtraValue = 605`, `TierBasic = 690`, `TierSilver = 691`, `TierGold = 692`, `SlotEffectTangled = 401`, `SlotEffectLeak = 402`, `SlotEffectHaste = 403`, `SlotEffectVoltMinus = 404`, `SlotEffectVoltPlus = 405`, `SlotEffectReaped = 406`, `SlotEffectMonitored = 407`, `SlotEffectInfected = 408`, `SlotEffectCatch = 409`, `SlotEffectNullifier = 410`, `SlotEffectExposed = 411`, `SlotEffectSmallOpening = 412`, `MovementDice = 501`, `MandatoryTile = 502`, `MapBoard = 503`, `MapTileGeneric = 504`, `SmallTileCurrency = 505`, `RelicDescriptionBattle = 701`, `ModifierIcon = 29`, `ModifierTarget = 35`, `ModifierGray = 36`, `ModifierMight = 801`, `ModifierFortify = 802`, `ModifierBackstab = 803`, `ModifierBonus = 804`, `ModifierWeak = 806`, `ModifierBarrier = 807`, `ModifierBrewmaster = 808`, `ModifierPure = 809`, `ModifierMachination = 810`, `ModifierNarcissist = 811`, `ModifierPetty = 812`, `ModifierEnergizer = 813`, `ModifierHasty = 814`, `ModifierPoisonous = 815`, `ModifierBoosted = 816`, `ModifierExtra = 817`, `ModifierLucky = 818`, `ModifierThorns = 819`, `ModifierRegen = 820`, `ModifierParry = 821`, `ModifierSuperTasty = 822`, `CinderSkin = 34`, `LisverSkin = 25`, `MangoSkin = 2000`, `NeraSkin = 2001`, `EnergyHolder = 8`, `SettingsInteractables = 10`, `AxeSlots = 22`, `BrownDITD = 23`, `OrangeTuto = 24`, `BoardColors = 26`, `SlotFinalValue = 27`, `EnergyColors = 28`, `ShadowFigure = 30`, `EndlessMode = 31`, `WishlistFeatures = 32`, `CurrencyValue = 33`, `BattleRerollButton = 37`, `MoveRerollButton = 38`, `DrawPileButton = 39`, `TurnNumber = 41`, `UnlockableCompleted = 42`, `DestroyDice = 43`, `EnemyFlyfant = 44`, `DifficultyD4 = 45`, `DrawPileColors = 46`, `FirstFace = 47`, `LastFace = 48`, `DrawDice = 49`, `LockedDice = 50`, `RewardColors = 51`, `HandColors = 52`, `BattleColors = 53`, `EliteBattleColors = 54`, `RemoveRelic = 55`, `BoardSlot = 56`, `KillColors = 57`, `ShopColors = 58`, `ForgeColors = 59`, `MapColors = 60`, `DungeonColors = 61`, `LoopColors = 62`, `DiscardDice = 63`, `DiceGenericOrange = 64`, `WeakSpots = 65`, `EventHighlightColors = 66`, `Curse = 67`, `ObtainDice = 68`, `DiceProperty = 69`, `ValuableColors = 70`, `CollectorColors = 71`, `SkipButton = 72`, `CancelButton = 73`, `SendFeedback = 74`, `DifficultyD6 = 75`, `TileRest = 76`, `PotionColors = 78`, `CompanionPotionMimic = 79`, `DifficultyD8 = 80`, `PermanentModifier = 81`, `FreeSample = 82`, `EvilFrog = 83`, `RunHistoryVictory = 84`, `RunHistoryDefeat = 85`, `CodexBestiaryName = 86`, `CodexBestiaryContent = 87`, `CodexBestiaryHeader = 88`, `DiscordColors = 89`, `CodexBestiaryUnlock = 90`, `MagicianSmoke = 91`, `TotemColorOne = 92`, `TotemColorTwo = 93`, `TotemColorThree = 94`, `FlowerColorOne = 95`, `FlowerColorTwo = 96`, `RelicMerchantRobe = 97`, `MosquitoDukeColors = 98`, `ChallengeColors = 99`, `TileDiscard = 100`, `SuccessColors = 9000`, `FailureColors = 9001`, `CharacterLevel = 9002`, `CinderExperience = 9003`, `LisverExperience = 9004`, `MangoExperience = 9005`, `NeraExperience = 9006`, `BattleBoardEnemy = 9007`, `GoldenWeapons = 9008`, `PotionNumbers = 9009`, `EvilEyeDormant = 9010`, `EvilEyeAwake = 9011`, `MapTileObscured = 9012`, `BattleUIAttack = 901`, `BattleUIBlock = 902`, `BattleUIHeal = 903`, `BattleUIBoost = 904`, `RewardTitleObtainDice = 1000`, `RewardTitleGrantProperty = 1001`, `RewardTitleRemoveDice = 1002`, `RewardTitleUpgradeDiceGold = 1003`, `RewardTitleUpgradeDiceSilver = 1004`, `RewardTitleHeal = 1005`, `RewardTitleObtainPotion = 1006`, `RewardTitleObtainCurrency = 1007`, `RewardTitleRandomProperty = 1008`, `RewardTitleObtainRelic = 1009`, `RewardTitleObtainEnemyRelic = 1010`, `RewardTitleRemoveDiceProperties = 1011`, `RewardTitleRedistributeDiceFaces = 1012`, `RewardTitleModifier = 1013`

### TextModeExtra.Title (5)
`None = 0`, `JapaneseWhiteWithUnderlay = 1`, `ChineseWhiteWithUnderlay = 2`, `KoreanWhiteWithUnderlay = 3`, `RussianWhiteWithUnderlay = 4`

### DeckData.Content (5)
`Dice = 0`, `RandomAllDice = 1`, `RandomBasicDice = 2`, `RandomQueenBeeReadyDice = 3`, `RandomScorpionReadyDice = 4`

### BaseLine.NextLineConditions (3)
`None = 0`, `HoveringTutorialButtonFalse = 1`, `IsGameStatePlayTrue = 2`

### DiceData.Type (37)
`None = 0`, `Attack = 1`, `AreaAttack = 20`, `SplashAttack = 21`, `PunctureAttack = 27`, `LethalAttack = 28`, `Block = 2`, `Heal = 3`, `Boost = 4`, `CloseBoost = 30`, `StickyBoost = 23`, `Mirror = 5`, `OffensiveMirror = 35`, `DefensiveMirror = 36`, `NurturingMirror = 37`, `Terrain = 12`, `Neutral = 14`, `Honey = 15`, `Prism = 16`, `Poison = 17`, `Thorns = 18`, `Regeneration = 19`, `Barrier = 22`, `Energizer = 24`, `Coin = 25`, `Fortify = 26`, `Draw = 29`, `Blood = 31`, `Reroll = 32`, `Hollower = 33`, `Void = 34`, `Backstab = 38`, `Sting = 39`, `Generic = 40`, `RoyalJelly = 41`, `DualEdge = 42`, `EvilEye = 43`

### DiceData.GeneralType (6)
`None = 0`, `Attacks = 1`, `Blocks = 2`, `Heals = 3`, `Boosts = 4`, `Mirrors = 5`

### DiceData.Face (18)
`Zero = 0`, `One = 1`, `Two = 2`, `Three = 3`, `Four = 4`, `Five = 5`, `Six = 6`, `Seven = 7`, `Eight = 8`, `Nine = 9`, `Ten = 10`, `Eleven = 11`, `Twelve = 12`, `Thirteen = 13`, `Fourteen = 14`, `Fifteen = 15`, `Sixteen = 16`, `GenericX = 999`

### DiceData.Tier (3)
`Basic = 0`, `Silver = 1`, `Gold = 2`

### DiceData.Rarity (6)
`Common = 0`, `Uncommon = 1`, `Rare = 2`, `VeryRare = 3`, `Legendary = 4`, `TBD = 9`

### DiceData.Property (23)
`None = 0`, `Glass = 1`, `Heavy = 2`, `Hollow = 3`, `Eternal = 4`, `Area = 6`, `Pierce = 7`, `Bomb = 8`, `Grow = 9`, `Bonded = 12`, `Flash = 10`, `Faulty = 13`, `DoubleDip = 14`, `Unstable = 15`, `Pointy = 16`, `Toxic = 17`, `Pack = 18`, `Group = 19`, `Gravity = 20`, `Retain = 21`, `Retry = 22`, `Filled = 23`, `Decadent = 24`

### DiceData.Surname (9)
`None = 0`, `Crappy = 1`, `Lucky = 2`, `Celestial = 3`, `Trickster = 4`, `Overkill = 5`, `Miracle = 6`, `Longshot = 7`, `Regular = 8`

### DiceData.Shape (3)
`D4 = 0`, `D6 = 1`, `D8 = 2`

### DiceInfo.BloodDiceBehavior (2)
`DealDamageToPlayer = 0`, `GrantMightToPlayer = 1`

### DiceFaceCatalog.FacesRNGMode (2)
`DefaultFaces = 0`, `RandomFaces = 1`

### PlayerSkinData.Name (13)
`NoSkin = 0`, `CinderDefault = 10`, `CinderDiceKnight = 11`, `CinderMaxLevel = 12`, `LisverDefault = 20`, `LisverDiceKnight = 21`, `LisverMaxLevel = 22`, `MangoDefault = 30`, `MangoDiceKnight = 31`, `MangoMaxLevel = 32`, `NeraDefault = 40`, `NeraDiceKnight = 41`, `NeraMaxLevel = 42`

### MultipleNodeData.Mode (2)
`ByArea = 0`, `RandomFromList = 1`

### NodeAddPermanentModifier.Mode (1)
`RandomFromList = 0`

### NodeBattleData.Mode (3)
`RandomMobFromList = 1`, `RandomMobFromSection = 2`, `LastNonBossBattle = 3`

### NodeBattleData.SectionMode (3)
`CurrentSection = 1`, `CurrentSectionWithIncrease = 2`, `SelectedSection = 3`

### NodeChangeDiceShapeData.TargetMode (2)
`Selected = 0`, `Random = 1`

### NodeData.RollData.Mode (4)
`SectionBased = 0`, `CustomRoll = 1`, `CustomRollByArea = 2`, `CustomSimpleBridge = 3`

### NodeDiscardDiceData.Mode (3)
`Selected = 1`, `Random = 2`, `RandomFromOptionSlots = 3`

### NodeDiscardPotionData.Mode (3)
`All = 0`, `Selected = 1`, `OneRandom = 2`

### NodeDiscardRelicOrCurseData.Mode (2)
`Selected = 1`, `Random = 2`

### NodeDiscardRelicOrCurseData.Type (2)
`Relic = 1`, `Curse = 2`

### NodeDuplicateRelicNode.Mode (2)
`Selected = 1`, `Random = 2`

### NodeEliteBattleData.Mode (2)
`SelectedMobsFromList = 1`, `RandomMobsFromSection = 2`

### NodeEliteBattleData.SectionMode (3)
`CurrentSection = 1`, `CurrentSectionWithIncrease = 2`, `SelectedSection = 3`

### Mode (2)
`Selected = 1`, `Random = 2`

### PropertyStyle (2)
`Specific = 1`, `Effect = 2`

### NodeModifyCurrencyData.Mode (2)
`IncreaseCurrency = 1`, `DecreaseCurrency = 2`

### NodeModifyHealthData.Mode (4)
`Heal = 1`, `IncreaseMaxHealth = 2`, `Damage = 3`, `DecreaseMaxHealth = 4`

### NodeModifyHealthData.Amount (3)
`Full = 1`, `Half = 2`, `Variable = 3`

### NodeRemovePropertiesData.Mode (2)
`Selected = 1`, `Random = 2`

### NodeReplacePotionData.Mode (1)
`ReplaceAllWithRandom = 0`

### NodeSelectDiceData.Mode (3)
`Rarity = 0`, `Tier = 1`, `Selection = 2`

### NodeSelectPotionData.Mode (3)
`SectionBased = 0`, `RarityBased = 1`, `RandomUntilFull = 3`

### NodeSelectRelicData.Mode (2)
`List = 0`, `AllAvailable = 1`

### NodeSkipToSpecificTile.TargetTile (1)
`AreaBoss = 0`

### NodeTransformDiceData.Mode (2)
`Selected = 1`, `Random = 2`

### NodeUpgradeDiceData.Mode (2)
`Selected = 1`, `Random = 2`

### NodeCurrencyAmountConditional.Mode (3)
`MinOnly = 0`, `MaxOnly = 1`, `MinAndMax = 2`

### EventOptionData.OutcomeExplanation.Mode (4)
`Automatic = 1`, `Custom = 2`, `Selected = 3`, `NoExplanation = 4`

### EventOptionData.SimpleCondition (6)
`None = 0`, `AtLeastOnePotion = 1`, `CanHaveMorePotions = 2`, `AtLeastOneDiceWithProperties = 3`, `AtLeastOneSilverDice = 4`, `AtLeastOneGoldDice = 5`

### EventOptionSlotsData.Slot.TypeRequirement (3)
`None = 0`, `All = 1`, `Custom = 2`

### EventOptionSlotsData.Slot.TierRequirement (3)
`None = 0`, `All = 1`, `Minimum = 2`

### EventOptionSlotsData.Slot.AutoSelectMode (2)
`HighestAverageDice = 0`, `RandomDice = 1`

### EventOptionSlotsData.Slot.PropertyRequirement (3)
`None = 0`, `All = 1`, `Custom = 2`

### EventOptionSlotsData.Slot.Result (4)
`None = 0`, `Discard = 1`, `Maim = 2`, `Clone = 3`

### EventData.Group (1)
`NoGroup = 0`

### EventData.Mood (2)
`Chill = 0`, `Dangerous = 1`

### EventData.Title (39)
`None = 0`, `AbandonedHouses = 1`, `ArrowTrap = 2`, `CryForHelp = 3`, `FairyWish = 4`, `FightOrFlight = 5`, `FracturedStone = 6`, `SacredBubbles = 7`, `SpiderNest = 8`, `StingyShopkeeper = 9`, `TerrorizingEncounter = 10`, `TheCollector = 11`, `TheDate = 12`, `UnfinishedMeal = 13`, `WarriorStatue = 14`, `RelicMerchant = 15`, `JesterReloaded = 16`, `SkippingInsect = 17`, `TheCollectorII = 18`, `DiceMagician = 19`, `LisverEncounter = 20`, `MirrorToPurgatory = 21`, `NerotinCrystals = 22`, `MulticoloredTotem = 23`, `LuckMachine = 24`, `SaunaLake = 25`, `EndlessCorridor = 26`, `PotionCauldron = 27`, `CloneMachine = 28`, `TwoFlowers = 29`, `RelicDisaster = 30`, `Bridge_Simple = 100`, `Bridge_Vertical = 101`, `Bridge_AnotherWorld = 102`, `Bridge_Rainbow = 103`, `Bridge_Sacrificial = 104`, `Bridge_Soothing = 105`, `Bridge_Promise = 106`, `Bridge_Gift = 107`

### GameData.RelicChanceMode (2)
`RarityBased = 0`, `CategoryBased = 1`

### GameData.DiceRewardMode (2)
`RarityBased = 0`, `DataBased = 1`

### MapAreaData.GenerationMode (2)
`Procedural = 0`, `Fixed = 1`

### MapAreaData.Position (4)
`InitPosition = 0`, `Area1 = 1`, `Area2 = 2`, `Area3 = 3`

### MapAreaData.Title (4)
`NoTitle = 0`, `BuriedCity = 1`, `MonarchsGrove = 2`, `DormantPalace = 3`

### MapSectionData.ChunkMode (3)
`Set = 0`, `Procedural = 1`, `Hybrid = 2`

### MapSectionData.Name (15)
`One_0 = 1`, `One_1 = 2`, `One_2 = 3`, `One_3 = 4`, `One_Boss = 9`, `Two_0 = 11`, `Two_1 = 12`, `Two_2 = 13`, `Two_3 = 14`, `Two_Boss = 19`, `Three_0 = 21`, `Three_1 = 22`, `Three_2 = 23`, `Three_3 = 24`, `Three_Boss = 29`

### MapAlchemyTileData.AlchemyTileInfo.Mode (3)
`Set = 0`, `RandomFromPool = 1`, `RandomFromAll = 2`

### MapBaseTileData.BaseTileInfo.RewardMode (3)
`DefaultRewards = 0`, `CustomRewards = 1`, `NoRewards = 2`

### MapBaseTileData.BaseTileInfo.UnknownMode (3)
`Never = 0`, `ChanceBased = 1`, `Always = 2`

### MapBaseTileData.Type (20)
`None = 0`, `Battle = 2`, `Event = 3`, `Movement = 4`, `Treasure = 5`, `Improve = 6`, `Alchemy = 7`, `Discard = 8`, `Rest = 9`, `Dungeon = 10`, `EliteBattle = 11`, `GenericShop = 12`, `ForgeShop = 13`, `Bridge = 14`, `Unknown = 50`, `Small_NoEffect = 100`, `Small_Heal = 101`, `Small_Damage = 102`, `Small_Currency = 103`, `Small_Movement = 104`

### MapBattleTileData.BattleTileInfo.Mode (4)
`Set = 0`, `RandomFromList = 1`, `RandomBasedOnSection = 2`, `ResolveOnLandBasedOnRunSeed = 3`

### MapBridgeTileData.Title (9)
`None = 0`, `Simple = 1`, `Vertical = 2`, `AnotherWorld = 3`, `UnderRainbow = 4`, `Sacrificial = 5`, `Promise = 6`, `Gift = 7`, `Soothing = 8`

### MapDungeonTileData.DungeonTileInfo.DungeonMode (2)
`SpecificDungeon = 0`, `RandomFromListOfDungeons = 1`

### MapDungeonTileData.DungeonTileInfo.Title (4)
`Anvil = 0`, `Hexen = 1`, `Mortar = 2`, `Arcane = 3`

### MapDungeonTileData.DungeonTileInfo.BuildMode (2)
`SetTiles = 0`, `Structure = 1`

### MapEliteBattleTileData.EliteBattleTileInfo.Mode (3)
`Set = 0`, `RandomBasedOnSection = 1`, `ResolveOnLandBasedOnRunSeed = 2`

### MapEventTileData.EventTileInfo.Mode (4)
`Set = 0`, `RandomBasedOnSection = 1`, `ResolveOnLandBasedOnRunSeed = 2`, `RandomFromList = 3`

### ShopTileInfo.Mode (2)
`Fixed = 0`, `Random = 1`

### MapMoveTileData.MoveTileInfo.Mode (2)
`SetAmount = 0`, `RandomAmount = 1`

### MobData.SpecialRule (2)
`None = 0`, `FledDragonglies = 1`

### NewsData.Update.UpdateType (2)
`Update = 1`, `Hotfix = 2`

### NewsData.Update.UpdateNameMode (3)
`NumberOnly = 1`, `NumberAndTitle = 2`, `TitleOnly = 3`

### PotionData.Title (58)
`Empty = 0`, `Block = 1`, `Jackpot = 2`, `Power = 3`, `Nuclear = 4`, `Mighty = 5`, `Slash = 6`, `Health = 8`, `Clover = 9`, `Bonus = 10`, `Fireball = 11`, `Haste = 12`, `Weighted = 13`, `Fortify = 14`, `Stun = 15`, `Rose = 16`, `Lottery = 17`, `Reckless = 18`, `Triangle = 19`, `Boosted = 20`, `Inspiring = 21`, `Energy = 22`, `Brewmaster = 23`, `Shatter = 24`, `Safeguard = 25`, `Mercurial = 26`, `Slot = 27`, `BlueSoul = 28`, `GreenSoul = 29`, `Sleight = 30`, `Soil = 31`, `Holy = 32`, `Mutation = 33`, `Reflection = 34`, `Verdant = 35`, `MinorBlock = 36`, `MinorHealth = 37`, `MajorBlock = 38`, `MajorHealth = 39`, `Jester = 40`, `START_SUPER_ID = 100`, `SuperMighty = 101`, `SuperGuardian = 102`, `SuperSlot = 103`, `SuperFireball = 104`, `SuperBrewmaster = 105`, `SuperBoosted = 106`, `SuperInspiring = 107`, `SuperBlock = 108`, `SuperSafeguard = 109`, `SuperBingo = 110`, `SuperBlueSoul = 111`, `SuperGreenSoul = 112`, `SuperVerdant = 113`, `SuperMercurial = 114`, `SuperHaste = 115`, `SuperJester = 116`, `SuperGhost = 117`

### PotionData.Rarity (4)
`Common = 1`, `Uncommon = 2`, `Rare = 3`, `Boss = 7`

### PotionData.ActionCondition.Condition (5)
`MoreOrEqual = 0`, `LessOrEqual = 1`, `Equal = 2`, `Even = 3`, `Odd = 4`

### PotionData.ActionResult.Effect (38)
`DamageTargetEnemy = 0`, `DamageAllEnemies = 1`, `DamagePlayer = 2`, `DamageForEachEnemyKilled = 18`, `GrantBlockPlayer = 3`, `GrantBlockForEachEnemyAlive = 9`, `GrantHealPlayer = 4`, `HealFullPlayer = 8`, `GrantHealForEachEnemyAlive = 10`, `ObtainOneRandomRunestone = 6`, `GainEnergy = 7`, `DrawDice = 11`, `ChangeDiceFacesToRandomValue = 13`, `AddTemporalDiceToDrawPile = 14`, `ApplyExtraValueToOneRandomSlot = 12`, `ApplyExtraValueToMultipleRandomSlots = 15`, `ApplyExtraValueToAllBoardSlots = 17`, `GainCoinPlayer = 5`, `GainCoinPlayerJackpotSeven = 16`, `GrantLuckyModPlayer = 100`, `GrantBonusModPlayer = 101`, `GrantMightModPlayer = 102`, `GrantThornsModPlayer = 103`, `GrantFortifyModPlayer = 104`, `GrantBoostedModPlayer = 105`, `GrantHastyModPlayer = 106`, `GrantExtraModPlayer = 107`, `GrantBrewmasterModPlayer = 108`, `GrantRandomModPlayer = 109`, `GrantPureModPlayer = 110`, `AddGrowDiceInHand = 201`, `AddGlassDiceInHand = 202`, `AddPointyDiceInHand = 203`, `AddHollowDiceInHand = 204`, `AddPermanentGlassOneDiceInHand = 251`, `AddPermanentRandomPropertyOneDiceInHand = 252`, `RemovePermanentRandomPropertyOneDiceInHand = 253`, `GrantTungstenModPlayer = 999`

### PotionData.ActionResult.ValueMode (2)
`RawValue = 0`, `ValueBasedOnDiceResult = 1`

### RunestoneDice.Shape (9)
`None = 0`, `D2 = 1`, `D4 = 2`, `D6 = 3`, `D8 = 4`, `D10 = 5`, `D12 = 6`, `D20 = 7`, `D100 = 8`

### RelicData.UseMode (2)
`PerTurn = 0`, `PerBattle = 1`

### RelicData.Title (195)
`Empty = 0`, `FrugalSnack = 1`, `SnakeEyes = 2`, `ProvidentialVial = 3`, `EvergreenSap = 4`, `ShinyStone = 6`, `ForgeHammer = 7`, `CrookedHourglass = 8`, `MarkOfTheChosenOne = 10`, `Raincoat = 11`, `BittersweetCake = 12`, `ToxicCloud = 13`, `ChocolateMuffin = 14`, `OrigamiFrog = 15`, `CrimsonBlade = 16`, `MedievalScope = 17`, `BucketOfPaint = 18`, `RiggedCoin = 19`, `BullsEye = 20`, `LoneIsland = 21`, `ShadowCloak = 25`, `ComfyCouch = 26`, `EnergyDrink = 27`, `TheJoker = 28`, `UnpolishedShards = 30`, `SuppressingEye = 31`, `LiquidDexterity = 32`, `SurvivalKit = 33`, `TransparentPrism = 34`, `LiquidKarma = 35`, `BlindfoldOfRage = 36`, `MakeshiftTrench = 37`, `MunchableDopamine = 38`, `ScavengerBeak = 39`, `MoonChakram = 40`, `StarYoyo = 41`, `BlindingLightbulb = 42`, `GoldenFruit = 43`, `DancingShurikens = 44`, `CorrosiveSkin = 45`, `NightmarePlasma = 46`, `BloodyFang = 47`, `PrismaticMango = 48`, `FrozenKnife = 49`, `MeltingKnife = 50`, `HeroicBackpack = 51`, `ShapeshiftingFin = 52`, `MinerHat = 53`, `MonkeyPaw = 54`, `PachinkoBalls = 55`, `TriangleAmulet = 56`, `TadpoleJar = 57`, `FingerlessMittens = 58`, `WinningBingo = 59`, `WatermelonSlice = 60`, `TwinScrolls = 61`, `ForgottenManifesto = 62`, `ChequeredBowtie = 63`, `ExtendoBoxingGlove = 64`, `LeatheredCase = 65`, `CopperBalance = 66`, `EmptyChest = 67`, `LeaderBadge = 68`, `BubbleGenerator = 69`, `DamagedZipper = 70`, `ExtremistAura = 71`, `SparePockets = 72`, `GreenGoo = 73`, `CounterfeitNote = 74`, `DeterminationJuice = 75`, `HiddenAce = 76`, `HighGround = 77`, `BubblyCement = 78`, `ScarletFlask = 79`, `RingOfTheBerserker = 80`, `TungstenHand = 81`, `OrangeClover = 82`, `ReptilianScales = 83`, `ElectricFlySwatter = 84`, `PendulumOfDoom = 85`, `AmphibianPlush = 86`, `SweetPastries = 87`, `BuildingBlocks = 88`, `MagneticRocks = 89`, `PorcelainWitch = 90`, `PromoTicket = 91`, `WingedSandals = 92`, `BentoBox = 93`, `ShiningFirefly = 94`, `DrosophilaPheromones = 96`, `TremblingChair = 97`, `RingOfExchange = 98`, `FakeDumbbell = 99`, `GuidanceFinger = 100`, `InsectRepellent = 101`, `NuclearButton = 102`, `SacredMaul = 103`, `BabyTick = 104`, `MercenaryLicense = 105`, `BobTheBob = 106`, `StickerPack = 107`, `RecommendationLetter = 108`, `TreeStump = 109`, `RedMarker = 110`, `CaffeinatedFeline = 111`, `TopHat = 112`, `DungeonBlueprints = 113`, `DevouringHole = 114`, `VisionaryEgg = 115`, `EquestrianPiece = 116`, `CorruptedMushroom = 117`, `WanderingHelmet = 118`, `HighWizardCloak = 119`, `LazarusHeart = 95`, `ColorfulLeaf = 120`, `IntertwinedSouls = 121`, `FragmentOfReality = 122`, `MagnificentFlower = 123`, `BrokenMemento = 124`, `UnrestrainedPotential = 126`, `CultFeather = 127`, `BlankSheet = 128`, `HandwrittenCode = 129`, `MaskOfApathy = 130`, `GargantuanShield = 132`, `RustyScrew = 133`, `ScorpionSkewer = 134`, `PieceOfCrown = 135`, `CocoaFilling = 136`, `ForbiddenEdge = 137`, `DeadlyVirus = 138`, `RegenerativeGenome = 139`, `FruitSalad = 140`, `StickyToe = 141`, `MetallicMuzzle = 142`, `TwinStars = 143`, `MiniatureCrate = 144`, `ToyShovel = 145`, `SymbolOfHope = 146`, `WoodenSpinningTop = 147`, `GlowingRuler = 148`, `BrightKey = 149`, `RubyMonocle = 150`, `ForeignMask = 151`, `WizardHat = 152`, `GobletOfPoison = 153`, `FancyCorkscrew = 154`, `RefillableBeer = 155`, `DollRepairSet = 156`, `PacifistDogma = 501`, `TangledMess = 502`, `PrimigenialFear = 503`, `DeathsVow = 504`, `CobwebBoots = 505`, `SepticBandage = 506`, `IdolizingEye = 507`, `ShatteredOpal = 508`, `Amnesia = 509`, `RippedPockets = 510`, `BottledInflation = 511`, `AcidicBlood = 512`, `DebilitatingBeverage = 513`, `ThickOxygen = 514`, `VenomDarts = 515`, `ExpiredHoneycomb = 516`, `MalfunctioningClock = 517`, `TurningTable = 518`, `LiquidPacience = 519`, `TormentedAura = 520`, `GastropodShell = 521`, `SinkingIsland = 522`, `OldSchoolGlasses = 523`, `WitheredFlower = 524`, `CorruptedHiddenAce = 700`, `CorruptedScarletFlask = 701`, `CorruptedBentoBox = 702`, `CorruptedRaincoat = 703`, `CorruptedShapeshiftingFin = 704`, `CorruptedToxicCloud = 705`, `CorruptedCorrosiveSkin = 706`, `CorruptedChocolateMuffin = 707`, `CorruptedJoker = 708`, `CorruptedEnergyDrink = 709`, `CorruptedStarYoyo = 710`, `CorruptedAmphibianPlush = 711`, `CorruptedSurvivalKit = 712`, `CorruptedHighWizardCloak = 713`, `CorruptedSuppressingEye = 714`, `CorruptedReptilianScales = 715`, `GoldplatedSetsquare = 9`, `NinjaKnives = 22`, `CrackedBomb = 23`, `LiarsMask = 24`, `ThornyShield = 29`, `CombatHandbook = 125`

### RelicData.Type (2)
`Relic = 1`, `Curse = 2`

### RelicData.Rarity (7)
`Common = 1`, `Uncommon = 2`, `Rare = 3`, `Epic = 4`, `Legendary = 5`, `Boss = 6`, `TBD = 9`

### RelicData.Style (3)
`Passive = 0`, `Aggressive = 1`, `Defensive = 2`

### RelicData.ConditionNumber (3)
`First = 0`, `Second = 1`, `Third = 2`

### RelicData.Category (5)
`None = 0`, `Starter = 1`, `Combo = 2`, `Useful = 3`, `Shop = 4`

### Achievements.Title (60)
`NoAchievement = 0`, `ReachArea2MonarchsGrove = 100`, `ReachArea3DormantPalace = 101`, `WinLisverEvent = 102`, `ReachFinalBattle = 103`, `DieFirstTime = 104`, `WinRunWithCinder = 105`, `WinRunWithLisver = 106`, `WinRunWithMango = 107`, `WinRunWithoutHealing = 108`, `WinRunMinimumHPOne = 109`, `WinRunLessThan40Mins = 110`, `Win3BattlesNoDamage = 111`, `Win5BattlesNoDamage = 112`, `Win10BattlesNoDamage = 113`, `DefeatFirstEnemy = 114`, `Defeat100Enemies = 115`, `Deal20DamageOneHit = 116`, `Deal99DamageOneHit = 117`, `Deal999DamageOneHit = 118`, `Spend100Coins = 119`, `Spend500Coins = 120`, `Spend1000Coins = 121`, `UpgradeDiceGoldTier = 123`, `DefeatTwoEnemiesOneTurn = 124`, `Collect20Relics = 125`, `DefeatCockroachNoAccurate = 126`, `Break50GlassDiceInOneRun = 127`, `FillAllBoardSlots = 128`, `StartBattleWith40HP = 129`, `AccurateKillEnemy25HP = 130`, `RemoveInitialRelic = 131`, `DefeatCaterpillarBoss = 132`, `DefeatScorpionBoss = 133`, `DefeatQueenBeeBoss = 134`, `DefeatMantisesBoss = 135`, `DefeatMosquitoDukeBoss = 136`, `WinRunD1Difficulty = 201`, `WinRunD2Difficulty = 202`, `WinRunD3Difficulty = 203`, `WinRunD4Difficulty = 204`, `WinRunD5Difficulty = 205`, `WinRunD6Difficulty = 206`, `CompletePotionsCodex = 138`, `WinRunD7Difficulty = 207`, `UnlockEndless = 139`, `ReachLoop1EndlessRun = 140`, `ReachLoop2EndlessRun = 137`, `UnlockChallenges = 141`, `WinRunNoFailChallenges = 144`, `Complete5ChallengesRun = 145`, `Complete30ChallengesTotal = 146`, `WinBossBattleNoDamage = 142`, `WinRunWithNera = 143`, `WinRunD8Difficulty = 208`, `DefeatArea1BossUltimateKill = 147`, `DefeatArea2BossUltimateKill = 148`, `DefeatRainBoss = 149`, `ReachMaxLevelAllCharacters = 150`, `WinRun20PlusDice = 151`

### LogTools.MessagePreSet (3)
`None = 0`, `CustomMessage = 1`, `FirewallMessage = 2`

### Tutorial.Mode (10)
`Off = 0`, `Intro = 2`, `FirstBattle = 1`, `EnemyModifiers = 20`, `EnemyModifiersReminderOffer = 21`, `EnemyModifiersReminderAccepted = 22`, `Map = 30`, `ChangeTarget = 40`, `Currency = 41`, `Relics = 42`

### DiceBox.BoxType (3)
`DrawPile = 0`, `DiscardPile = 1`, `Exile = 2`

### DiceDescriptionFace.FaceMode (2)
`Default = 0`, `Overriden = 1`

### DiceSelector.Context (2)
`Default = 0`, `BossReward = 1`

### DiceSelector.SkipMode (3)
`LetSystemDecide = 0`, `ForceSkippable = 1`, `ForceUnskippable = 2`

### BackpackUI.Requirement (2)
`None = 0`, `AtLeastOneProperty = 1`

### BackpackUI.OpenData.State (15)
`NotOpen = 0`, `ShowDice = 1`, `ShowDiceNoInteract = 2`, `UpgradeDice = 10`, `RemoveDice = 11`, `TransformDice = 12`, `GrantProperty = 13`, `TransformDiceToHeal = 14`, `ModifyDiceFaces = 15`, `RemoveDiceProperties = 16`, `RemoveRelic = 17`, `RemoveCurse = 18`, `DuplicateRelic = 19`, `ChangeDiceShape = 20`, `Event = 50`

### BackpackUI.DrawDiceMode (4)
`NoDraw = 0`, `BonusModifier = 1`, `UseCurrency = 2`, `UseEnergy = 3`

### RelicSelector.SkipMode (3)
`LetSystemDecide = 0`, `ForceSkippable = 1`, `ForceUnskippable = 2`

### RelicsUI.Context (4)
`Battle = 0`, `Backpack = 1`, `EndScreens = 2`, `Map = 3`

### ButtonControllerCallbacks.ButtonActionPointerTempo (6)
`OnPointerClickBeforeDelay = 0`, `OnPointerClickAfterDelay = 1`, `OnPointerEnterBeforeDelay = 2`, `OnPointerEnterAfterDelay = 3`, `OnPointerExitBeforeDelay = 4`, `OnPointerExitAfterDelay = 5`

### ButtonControllerCallbacks.ButtonActionAnimationTempo (3)
`OnPointerClickDuringAnimation = 0`, `OnPointerEnterDuringAnimation = 1`, `OnPointerExitDuringAnimation = 2`

### CustomButtonController.DelayType (2)
`AnimationDelay = 0`, `CustomDelay = 1`

### RerollButtonController.Type (9)
`DiceSelector = 0`, `RelicSelector = 1`, `RewardSelector = 2`, `DiceMovement = 3`, `AlchemyReward = 4`, `DiceBattle = 5`, `DiceBattleFree = 6`, `DiceBackpack = 7`, `DiceBackpackFree = 8`

### UnlockeableButtonController.UnlockeableButtonType (3)
`NONE = 0`, `NormalCinematics = 1`, `TrueEndCinematics = 2`

### CenterCameraButtonController.Type (2)
`Left = 0`, `Right = 1`

### Codex.Category.Type (6)
`Characters = 0`, `DiceTypes = 1`, `DiceProperties = 2`, `Relics = 3`, `Potions = 4`, `Enemies = 5`

### Codex.Item.State (3)
`Available = 0`, `Seen = 1`, `Locked = 2`

### Codex.ItemEnemy.UnlockPageMode (8)
`None = 0`, `EncounterEnemy = 1`, `KillEnemyOnce = 2`, `KillEnemyMultiple = 3`, `KillEnemyAccurate = 4`, `KillEnemyTurn1 = 5`, `KillEnemyNoDamage = 6`, `KillEnemyUltimate = 7`

### CodexEnemyCreditsButton.CreditsMode (2)
`CodexChroniclesEntry = 0`, `ArtContestWinner = 1`

### CodexSlot.Size (4)
`Default = 0`, `Character = 1`, `Potion = 2`, `Enemy = 3`

### CodexSlotCharacter.Page (5)
`None = 0`, `First = 1`, `Second = 2`, `Third = 3`, `Fourth = 4`

### CodexSlotEnemy.StatScore (6)
`Zero = 0`, `One = 1`, `Two = 2`, `Three = 3`, `Four = 4`, `Five = 5`

### CodexSlotEnemy.Page (4)
`First = 1`, `Second = 2`, `Third = 3`, `Fourth = 4`

### CodexUI.PageOrientation (2)
`LeftPage = 0`, `RightPage = 1`

### Unlockables.DebugForceMode (4)
`Default = 0`, `ForceAllLocked = 1`, `ForceAllUnlocked = 2`, `ForceAllUnlockedAndSeen = 3`

### CursorVisual.State (4)
`Open = 0`, `Close = 1`, `Click = 2`, `Hover = 3`

### CursorVisual.UpdateSync (3)
`VisualToCursor = 0`, `CursorToVisual = 1`, `VisualAndPointerToTarget = 2`

### EditorSeedMode (2)
`Random = 0`, `Fixed = 1`

### EndlessModeMessageUI.Option (3)
`NotPressed = 0`, `FinishRun = 1`, `ContinueEndless = 2`

### EnergyHolder.EnergyAnimationParameter (2)
`Spent = 0`, `Depleted = 1`

### EventOptionUI.SlotsData.Outcome (3)
`None = 0`, `Success = 1`, `Failure = 2`

### HeaderUI.Position (2)
`TopRight = 0`, `DownCenter = 1`

### HighlighterUI.ShapeData.Type (3)
`Square = 0`, `Circle = 1`, `Custom = 2`

### HSLController.State (4)
`Default = 0`, `LowestValue = 1`, `HigherValue = 2`, `CustomValue = 3`

### LayoutUIController.Type (1)
`Horizontal = 0`

### CreditGroup.Type (3)
`Names = 0`, `Space = 1`, `Items = 2`

### GroupsSelections.Group (44)
`Default = 0`, `Enemies = 1`, `Relics = 2`, `Dices = 3`, `BoardSlots = 4`, `DiceBoxDice = 5`, `EventOption = 6`, `Player = 7`, `Menu = 8`, `Map = 9`, `MapButton = 10`, `SettingsButton = 11`, `DrawButton = 12`, `BackpackButton = 13`, `Skip = 14`, `MenuPriority = 15`, `NONE = 16`, `RestartRunButton = 17`, `QuitRunButton = 18`, `NextOptionButton = 19`, `PreviousOptionButton = 20`, `NextCategoryButton = 21`, `PreviousCategoryButton = 22`, `Continue = 23`, `BackButton = 24`, `ConfirmButton = 25`, `ResetButton = 26`, `PlusOneButton = 27`, `MinusOneButton = 28`, `DiscordButton = 29`, `TwitterButton = 30`, `EnergyCounter = 31`, `DialogueSkipShortcut = 32`, `DialogueBox = 33`, `TutorialButton = 34`, `ShopItem = 35`, `Obtainable = 36`, `FeedbackUI = 37`, `Runestones = 38`, `FinishTurnButton = 39`, `DifficultyUI = 40`, `AltSelectable = 41`, `IntroCinematicButton = 42`, `TrueEndingCinematicButton = 43`

### SelectionManager.RelativePosition (9)
`NONE = 0`, `L = 1`, `R = 2`, `D = 3`, `U = 4`, `UR = 5`, `UL = 6`, `DR = 7`, `DL = 8`

### SelectionManager.NavigationMode (4)
`Horizontal = 0`, `Vertical = 1`, `Automatic = 2`, `Diagonal = 3`

### OriginsWishlistPopupUI.Location (5)
`None = 0`, `TitleScreenQuit = 1`, `GameplayQuit = 2`, `EndScreen = 3`, `EndlessMode = 4`

### RelicMarker.EffectInfo.Type (7)
`NoType = 0`, `Global = 1`, `Attack = 2`, `Block = 3`, `Heal = 4`, `Boost = 5`, `Energy = 6`

### RewardsUI.Source (9)
`Treasure = 1`, `Improve = 2`, `Alchemy = 3`, `Discard = 4`, `Rest = 5`, `Bridge = 8`, `Battle = 0`, `EliteBattle = 6`, `EventOutcome_BattleRewards = 7`

### ScreenManager.AnchorPosition (9)
`Top = 0`, `Bottom = 1`, `Left = 2`, `Right = 3`, `TopRight = 4`, `TopLeft = 5`, `BottomRight = 6`, `BottomLeft = 7`, `Center = 8`

### ShopUIConfig.ShopConfigType (2)
`GeneralShop = 0`, `Forge = 1`

### BaseTooltipBox.Placement (3)
`Central = 0`, `Left = 1`, `Right = 2`

### Tooltip.Mode (13)
`None = 0`, `Basic = 1`, `PlayerRelic = 2`, `EnemyRelic = 3`, `EventOutcome = 4`, `EnemyInformation = 5`, `PlayerInformation = 6`, `EventInformation = 7`, `Shop = 8`, `CodexLeft = 9`, `CodexRight = 10`, `RunestoneVisual = 11`, `SlotSpecial = 12`

### Tooltip.Orientation (5)
`Down = 0`, `Up = 1`, `Right = 2`, `Left = 3`, `None = 4`

### Tooltip.Skin (2)
`Default = 0`, `Dark = 1`

### CharacterSelectorInfoController.LoadMode (3)
`IgnoreAllDescriptions = 0`, `OnlyProperties = 1`, `LoadAll = 2`

### CustomText.StringType (2)
`Simple = 0`, `Custom = 1`

### CustomText.TextMode (12)
`FroggyNormal = 0`, `FroggyNumber = 1`, `MenuGold = 3`, `MenuBrown = 4`, `MenuGreen = 6`, `MenuBuriedCity = 9`, `MenuMonarchsGrove = 10`, `MenuDormantPalace = 11`, `MenuGoldNoOverride = 7`, `MenuBrownNoOverride = 8`, `BigGold = 5`, `BigBrown = 2`

### CustomTextEffects.FX (5)
`None = 0`, `Shake = 1`, `Wave = 2`, `Size = 3`, `Color = 4`

### CustomTextEffects.FXStart (3)
`None = 0`, `Stamp = 1`, `Drop = 2`

### VisualDice.BodyContext (3)
`Any = 0`, `Hand = 1`, `Board = 2`

### PropertiesPool.Side (2)
`Left = 0`, `Right = 1`

### BaseDirection.ChangeMode (3)
`NONE = 0`, `ROTATION = 1`, `SCALE = 2`

### Directions.Direction (9)
`NONE = 0`, `UP = 1`, `RIGHT = 2`, `DOWN = 3`, `LEFT = 4`, `UP_RIGHT = 5`, `UP_LEFT = 6`, `DOWN_LEFT = 7`, `DOWN_RIGHT = 8`

### MathExt.OperationType (2)
`Sum = 0`, `Subtract = 1`

### ObjectPool.ParentType (4)
`NONE = 0`, `CREATE_CUSTOM = 1`, `THIS_GAMEOBJECT = 2`, `EXISTING_GAMEOBJECT = 3`

### SceneController.SceneName (8)
`NONE = 0`, `TITLE_SCREEN = 1`, `MAP = 2`, `BATTLE = 3`, `DUNGEON = 4`, `GAME_OVER = 5`, `GAME_WIN = 6`, `Length = 7`

### Screenshots.CaptureMode (7)
`Everything = 0`, `NoDebug = 1`, `NoCanvasMode1 = 2`, `NoCanvasMode2 = 4`, `NoCanvasMode3 = 8`, `NoCanvasMode4 = 16`, `NoEntityCanvases = 32`

### SwapColor.SwapIndex (9)
`BorderColor = 49`, `Type1_BodyColor = 227`, `Type1_TopShadowColor = 139`, `Type1_BottomShadowColor = 167`, `Type1_ShineColor = 232`, `Type2_BodyColor = 222`, `Type2_TopShadowColor = 239`, `Type2_BottomShadowColor = 247`, `Type2_ShineColor = 181`

### BreatheUtil.SinWaveMode (3)
`NORMAL = 0`, `SMOOTH = 1`, `QUICK = 2`

### VectorUtil.AxisType (8)
`ZERO = 0`, `ONE = 1`, `UP = 2`, `DOWN = 3`, `LEFT = 4`, `RIGHT = 5`, `FORWARD = 6`, `BACK = 7`

### VectorUtil.Random (2)
`HORIZONTAL = 0`, `VERTICAL = 1`

### UserDataStore_File.DataFormat (2)
`Text = 0`, `Binary = 1`

### UserDataStore_KeyValue.ActionMappingSaveMode (2)
`ByController = 0`, `ByControllerElementRole = 1`

### UserDataStore_PlayerPrefs.ActionMappingSaveMode (2)
`ByController = 0`, `ByControllerElementRole = 1`

### PointerEventType (2)
`Mouse = 0`, `Touch = 1`

### ControllerElementGlyphBase.AllowedTypes (3)
`All = 0`, `Glyphs = 1`, `Text = 2`

### GlyphOrTextBase.TypeFlags (4)
`None = 0`, `Glyph = 1`, `Text = 2`, `All = -1`

### GlyphTools.Action2DHelper.Result (2)
`GoNext = 0`, `Quit = 1`

### UnityUITextMeshProGlyphHelper.Tag.TagType (3)
`ControllerElement = 0`, `Action = 1`, `Player = 2`

### UnityUITextMeshProGlyphHelper.DisplayType (3)
`Glyph = 0`, `Text = 1`, `GlyphOrText = 2`

### CalibrationWindow.ButtonIdentifier (4)
`Done = 0`, `Cancel = 1`, `Default = 2`, `Calibrate = 3`

### ControlMapper.MappingSet.ActionListMode (2)
`ActionCategory = 0`, `Action = 1`

### ControlMapper.LayoutElementSizeType (2)
`MinSize = 0`, `PreferredSize = 1`

### ControlMapper.WindowType (11)
`None = 0`, `ChooseJoystick = 1`, `JoystickAssignmentConflict = 2`, `ElementAssignment = 3`, `ElementAssignmentPrePolling = 4`, `ElementAssignmentPolling = 5`, `ElementAssignmentResult = 6`, `ElementAssignmentConflict = 7`, `Calibration = 8`, `CalibrateStep1 = 9`, `CalibrateStep2 = 10`

### InputBehaviorWindow.ButtonIdentifier (3)
`Done = 0`, `Cancel = 1`, `Default = 2`

### InputBehaviorWindow.PropertyType (2)
`JoystickAxisSensitivity = 0`, `MouseXYAxisSensitivity = 1`

### ThemeSettings.FontStyleOverride (5)
`Default = 0`, `Normal = 1`, `Bold = 2`, `Italic = 3`, `BoldAndItalic = 4`

### ControlRemappingDemo1.DialogHelper.DialogType (6)
`None = 0`, `JoystickConflict = 1`, `ElementConflict = 2`, `KeyConflict = 3`, `DeleteAssignmentConfirmation = 10`, `AssignElement = 11`

### ControlRemappingDemo1.QueueEntry.State (3)
`Waiting = 0`, `Confirmed = 1`, `Canceled = 2`

### ControlRemappingDemo1.QueueActionType (5)
`None = 0`, `JoystickAssignment = 1`, `ElementAssignment = 2`, `FallbackJoystickIdentification = 3`, `Calibrate = 4`

### ControlRemappingDemo1.ElementAssignmentChangeType (5)
`Add = 0`, `Replace = 1`, `Remove = 2`, `ReassignOrRemove = 3`, `ConflictCheck = 4`

### ControlRemappingDemo1.UserResponse (4)
`Confirm = 0`, `Cancel = 1`, `Custom1 = 2`, `Custom2 = 3`

### CustomPlatformType (1)
`MyPlatform = 0`

### PersistentStorage.eFileType (4)
`Raw = 0`, `Persistent = 1`, `Temporal = 2`, `Streaming = 3`

### ePluralType (6)
`Zero = 0`, `One = 1`, `Two = 2`, `Few = 3`, `Many = 4`, `Plural = 5`

### TranslationJob.eJobState (3)
`Running = 0`, `Succeeded = 1`, `Failed = 2`

### eLanguageDataFlags (3)
`DISABLED = 1`, `KEEP_LOADED = 2`, `NOT_LOADED = 4`

### LanguageSourceData.MissingTranslationAction (4)
`Empty = 0`, `Fallback = 1`, `ShowWarning = 2`, `ShowTerm = 3`

### LanguageSourceData.eAllowUnloadLanguages (3)
`Never = 0`, `OnlyInDevice = 1`, `EditorAndDevice = 2`

### LanguageSourceData.eGoogleUpdateFrequency (7)
`Always = 0`, `Never = 1`, `Daily = 2`, `Weekly = 3`, `Monthly = 4`, `OnlyOnce = 5`, `EveryOtherDay = 6`

### LanguageSourceData.eGoogleUpdateSynchronization (3)
`Manual = 0`, `OnSceneLoaded = 1`, `AsSoonAsDownloaded = 2`

### eSpreadsheetUpdateMode (4)
`None = 0`, `Replace = 1`, `Merge = 2`, `AddNewTerms = 3`

### Localize.TermModification (5)
`DontModify = 0`, `ToUpper = 1`, `ToLower = 2`, `ToUpperFirst = 3`, `ToTitle = 4`

### eTermType (12)
`Text = 0`, `Font = 1`, `Texture = 2`, `AudioClip = 3`, `GameObject = 4`, `Sprite = 5`, `Material = 6`, `Child = 7`, `Mesh = 8`, `TextMeshPFont = 9`, `Object = 10`, `Video = 11`

### TranslationFlag (2)
`Normal = 1`, `AutoTranslated = 2`

### IsolatedArabicLetters (41)
`Hamza = 65152`, `Alef = 65165`, `AlefHamza = 65155`, `WawHamza = 65157`, `AlefMaksoor = 65159`, `AlefMaksora = 64508`, `HamzaNabera = 65161`, `Ba = 65167`, `Ta = 65173`, `Tha2 = 65177`, `Jeem = 65181`, `H7aa = 65185`, `Khaa2 = 65189`, `Dal = 65193`, `Thal = 65195`, `Ra2 = 65197`, `Zeen = 65199`, `Seen = 65201`, `Sheen = 65205`, `S9a = 65209`, `Dha = 65213`, `T6a = 65217`, `T6ha = 65221`, `Ain = 65225`, `Gain = 65229`, `Fa = 65233`, `Gaf = 65237`, `Kaf = 65241`, `Lam = 65245`, `Meem = 65249`, `Noon = 65253`, `Ha = 65257`, `Waw = 65261`, `Ya = 65265`, `AlefMad = 65153`, `TaMarboota = 65171`, `PersianPe = 64342`, `PersianChe = 64378`, `PersianZe = 64394`, `PersianGaf = 64402`, `PersianGaf2 = 64398`

### GeneralArabicLetters (41)
`Hamza = 1569`, `Alef = 1575`, `AlefHamza = 1571`, `WawHamza = 1572`, `AlefMaksoor = 1573`, `AlefMagsora = 1609`, `HamzaNabera = 1574`, `Ba = 1576`, `Ta = 1578`, `Tha2 = 1579`, `Jeem = 1580`, `H7aa = 1581`, `Khaa2 = 1582`, `Dal = 1583`, `Thal = 1584`, `Ra2 = 1585`, `Zeen = 1586`, `Seen = 1587`, `Sheen = 1588`, `S9a = 1589`, `Dha = 1590`, `T6a = 1591`, `T6ha = 1592`, `Ain = 1593`, `Gain = 1594`, `Fa = 1601`, `Gaf = 1602`, `Kaf = 1603`, `Lam = 1604`, `Meem = 1605`, `Noon = 1606`, `Ha = 1607`, `Waw = 1608`, `Ya = 1610`, `AlefMad = 1570`, `TaMarboota = 1577`, `PersianPe = 1662`, `PersianChe = 1670`, `PersianZe = 1688`, `PersianGaf = 1711`, `PersianGaf2 = 1705`

### JSONBinaryTag (7)
`Array = 1`, `Class = 2`, `Value = 3`, `IntValue = 4`, `DoubleValue = 5`, `BoolValue = 6`, `FloatValue = 7`

