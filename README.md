# DitD Coop : multijoueur, modloader et SDK pour Die in the Dungeon (v5.1.0)

## Pour les joueurs : le launcher

1. Télécharge **[`dist/DitdCoopLauncher.exe`](dist/DitdCoopLauncher.exe)** (Windows 10/11, rien d'autre à installer).
2. Lance-le et indique le **dossier du jeu**, celui qui contient `Die in the Dungeon.exe`.
3. Clique sur **JOUER**. Le launcher installe BepInEx, DiceLoader et les mods, les met à jour, puis lance le jeu.
   - Le premier lancement du jeu prend 1 à 3 minutes, le temps que BepInEx prépare le jeu.
   - En jeu, **F1** ouvre le menu des mods ; la coop se lance depuis *Multijoueur Coop > Options*.
4. À chaque démarrage, le launcher vérifie ce dépôt et télécharge seulement ce qui a changé, lui-même compris.
   Le bouton « Désactiver les mods » permet de retrouver le jeu d'origine.

## Pour les développeurs : publier une mise à jour

```powershell
.\publish.ps1 -Version 0.2.0 -Changelog "Ce qui change" -Push               # mods + launcher (même version de launcher)
.\publish.ps1 -Version 0.2.1 -LauncherVersion 1.0.1 -Changelog "…" -Push   # nouvelle version du launcher aussi
```
Le script compile tout et recopie les binaires dans `dist/files/`. Il génère `dist/manifest.json` (fichiers, SHA-256, versions), puis commit et pousse.
Les launchers lisent `https://raw.githubusercontent.com/Reloadaxe/ditd-coop/main/dist/manifest.json`. Le dépôt doit donc être **public**.
Pour compiler, il faut le jeu en local avec `BepInEx\interop` déjà généré (`-GameDir` si le jeu n'est pas dans `..\Die in the Dungeon`).

---

```
ditd-coop/
├─ launcher/                 ← launcher Windows (WinForms .NET 4.8) : installation, mises à jour, auto-mise à jour
├─ dist/                     ← ce que téléchargent les launchers (manifest.json, files/, DitdCoopLauncher.exe)
├─ publish.ps1               ← compile et publie une version
├─ SDK/
│  ├─ GUIDE.md               ← architecture du jeu (états, combat, dés, reliques, sauvegardes) et RVA clés
│  ├─ api/Classes.md         ← index de 1 898 types en 20 catégories
│  ├─ api/Enums.md           ← 323 enums (reliques, ennemis, dés, modificateurs, potions…)
│  ├─ api/Assembly-CSharp.cs ← (local, hors dépôt) toutes les classes, champs et méthodes du jeu
│  ├─ dump/                  ← (local, hors dépôt) dump Il2CppDumper brut
│  └─ tools/                 ← scripts IDA / Ghidra
├─ src/
│  ├─ DiceLoader/            ← le modloader (plugin BepInEx 6 IL2CPP)
│  │  ├─ API/DiceMod.cs      ← classe de base des mods et attributs
│  │  ├─ API/Game.cs         ← façade sur le jeu (PV, or, énergie, reliques, deck, ennemis…)
│  │  ├─ API/DiceEvents.cs   ← événements du jeu (run, floor, combat, tours, morts, reliques)
│  │  ├─ API/UI.cs           ← helpers IMGUI compatibles IL2CPP
│  │  └─ Core/               ← chargement des mods, hooks Harmony, menu F1
│  └─ Mods/
│     ├─ Multiplayer/        ← coop 2 ou 3 joueurs en TCP (voir son README)
│     ├─ ExampleMod/         ← exemple commenté (config, événements, patch Harmony)
│     └─ DevTools/           ← panneau de triche et de debug
├─ install.ps1  build.ps1  new-mod.ps1
└─ DiceModding.sln
```

## Comment ça marche

```
Die in the Dungeon.exe
  └─ winhttp.dll (Doorstop)            → démarre le runtime .NET 6 dans le processus
       └─ BepInEx 6 IL2CPP              → génère l'interop C# des classes du jeu, Harmony
            └─ DiceLoader (plugin)       → charge <jeu>/Mods/**/*.dll, hooks, événements, menu F1
                 └─ tes mods (DiceMod)
```

## Installation (une seule fois)

```powershell
cd DiceModding
.\install.ps1          # copie BepInEx dans le dossier du jeu (déjà fait ici)
```
1. **Lance le jeu une fois.** Le premier démarrage génère `BepInEx\interop\` (1 à 3 minutes, l'écran peut rester noir ; suis la console). Attends le menu principal, puis ferme le jeu.
2. Compile le loader et les mods :
   ```powershell
   .\build.ps1
   ```
3. Relance le jeu et appuie sur **F1** pour ouvrir le menu DiceLoader. Il liste les mods, permet de les activer ou désactiver à chaud et donne accès à leurs options.

Pour désinstaller : `.\install.ps1 -Uninstall` désactive l'injection sans rien supprimer.

## Créer un mod

```powershell
.\new-mod.ps1 -Name MonMod
# édite src\Mods\MonMod\MonMod.cs
.\build.ps1
```

```csharp
using DiceLoader;

[DiceModInfo("moi.monmod", "Mon Mod", "1.0.0", Author = "Moi", Description = "…")]
public class MonMod : DiceMod
{
    public override void OnRunStart(CurrentRunData run, bool loaded) { if (!loaded) Game.AddGold(100); }
    public override void OnTurnStart(BattleSystem battle, int turn)  => Game.Schedule(() => Game.Energy += 1);
    public override void OnEnemyDeath(Enemy enemy)                    => Logger.LogInfo($"{enemy.EnemyName} est mort");
    public override void OnSettingsGUI()                              { if (UI.Button("Soin complet")) Game.HealFull(); }
}
```

### API `DiceMod`
| Membre | Rôle |
|---|---|
| `Info`, `Logger`, `Config`, `Harmony`, `Directory`, `Enabled` | Métadonnées, log, config (`BepInEx/config/mods/<id>.cfg`), Harmony dédié, dossier du mod |
| `OnLoad` / `OnEnable` / `OnDisable` | Cycle de vie. Les patchs de `Harmony` sont retirés automatiquement à la désactivation |
| `OnUpdate` / `OnGUI` / `OnSettingsGUI` | Chaque frame / overlay IMGUI / page d'options dans le menu F1 |
| `OnGameStateChanged(prev, cur)` | États du GameManager : TitleScreen, Map, BattleFloor, ShopFloor… |
| `OnRunStart(run, loaded)` / `OnRunEnd(run, won)` | Début ou reprise d'une run, fin de run |
| `OnFloorStart(floor)` / `OnFloorEnd(floor, success)` | Salles |
| `OnBattleStart` / `OnTurnStart(turn)` / `OnTurnStateChanged(state)` / `OnBattleEnd(result)` | Combat |
| `OnEnemyDeath(enemy)` / `OnPlayerDeath(player)` | Morts |
| `OnRelicAdded(relic)` / `OnRelicRemoved(relic)` | Reliques |

Les mêmes événements existent en version C# dans `DiceEvents` (`DiceEvents.TurnStart += …`).
Les exceptions d'un mod sont interceptées et affichées en rouge dans le menu F1 ; elles ne font jamais planter le jeu.

### API `Game`
`Manager`, `Floor`, `Battle`, `Player`, `Run`, `Dice`, `Relics`, `Currency`, `Map`, `Runestones` ·
`State`, `InRun`, `InBattle`, `Turn`, `TurnState`, `Version`, `DevMode` ·
`Health`, `MaxHealth`, `Block`, `SetHealth`, `Heal`, `HealFull`, `SetMaxHealth`, `SetBlock` ·
`Gold`, `AddGold` · `Energy`, `DiceDrawPerTurn`, `Deck` ·
`RelicTitles`, `HasRelic`, `AddRelic`, `RemoveRelic` · `Enemies`, `KillAllEnemies`, `ForceFinishBattle` ·
`SaveRun`, `SaveGame`, `SavePath` · `Safe(...)`, `Schedule(action, delay)`.

Pour tout le reste, toutes les classes du jeu sont directement utilisables en C#. Cherche-les dans `SDK/api/Classes.md` et `SDK/api/Assembly-CSharp.cs`, puis patche-les avec Harmony (exemple dans `ExampleMod.cs` et `SDK/GUIDE.md`).

### Règles IL2CPP à connaître
- Les classes du jeu sont des **wrappers** autour d'objets natifs : compare-les à `null` et gère le cas où l'objet a été détruit (`Game.Safe(() => …)`).
- Les listes du jeu sont des `Il2CppSystem.Collections.Generic.List<T>` : parcours-les avec `foreach`.
- Pour créer ton propre `MonoBehaviour`, il faut un constructeur `(IntPtr ptr) : base(ptr)` et l'appel `ClassInjector.RegisterTypeInIl2Cpp<T>()`.
- Patche en Harmony les méthodes synchrones plutôt que les coroutines MEC `_Xxx()`, qui renvoient `IEnumerator<float>`.
- Pour le clavier, utilise `Event.current` dans `OnGUI`. Le jeu passe par Rewired et l'InputSystem.

## Logs
`<jeu>\BepInEx\LogOutput.log`, plus la console BepInEx activée par `install.ps1`.
