# Installe BepInEx 6 (IL2CPP, build be.793) dans le dossier du jeu.
# BepInEx est la couche d'injection (Doorstop -> CoreCLR -> Il2CppInterop) sur laquelle repose DiceLoader.
#   .\install.ps1 [-GameDir "chemin\du\jeu"] [-Uninstall]
param(
    [string]$GameDir = (Join-Path $PSScriptRoot "..\Die in the Dungeon"),
    [switch]$Uninstall
)
$ErrorActionPreference = "Stop"
$GameDir = (Resolve-Path $GameDir).Path
if (-not (Test-Path "$GameDir\GameAssembly.dll")) { throw "GameAssembly.dll introuvable dans $GameDir" }

if ($Uninstall) {
    # Désactive l'injection sans rien supprimer : il suffit de renommer winhttp.dll
    if (Test-Path "$GameDir\winhttp.dll") { Rename-Item "$GameDir\winhttp.dll" "winhttp.dll.disabled" -Force }
    Write-Host "Mods désactivés (winhttp.dll -> winhttp.dll.disabled). Relance install.ps1 pour les réactiver." -ForegroundColor Yellow
    exit 0
}

if (Test-Path "$GameDir\winhttp.dll.disabled") { Rename-Item "$GameDir\winhttp.dll.disabled" "winhttp.dll" -Force }
if (-not (Test-Path "$GameDir\BepInEx\core\BepInEx.Unity.IL2CPP.dll")) {
    if (-not (Test-Path "$PSScriptRoot\vendor\BepInEx")) {
        Write-Host "Téléchargement de BepInEx 6.0.0-be.793…"
        $zip = Join-Path $env:TEMP "ditd-bepinex.zip"
        Invoke-WebRequest "https://builds.bepinex.dev/projects/bepinex_be/793/BepInEx-Unity.IL2CPP-win-x64-6.0.0-be.793%2Bdbc82f0.zip" -OutFile $zip
        Expand-Archive -Force $zip "$PSScriptRoot\vendor\BepInEx"
        Remove-Item $zip
    }
    Copy-Item -Recurse -Force "$PSScriptRoot\vendor\BepInEx\*" $GameDir
    Write-Host "BepInEx installé." -ForegroundColor Green
} else {
    Write-Host "BepInEx est déjà installé." -ForegroundColor Green
}
New-Item -ItemType Directory -Force "$GameDir\Mods" | Out-Null

# Console BepInEx activée pour voir les logs en direct
$cfg = "$GameDir\BepInEx\config\BepInEx.cfg"
if (-not (Test-Path $cfg)) {
    New-Item -ItemType Directory -Force (Split-Path $cfg) | Out-Null
    "[Logging.Console]`r`nEnabled = true`r`n" | Set-Content -Encoding utf8 $cfg
}

Write-Host "`nÉtape suivante : lance le jeu UNE fois. Le premier démarrage génère BepInEx\interop (1 à 3 minutes, l'écran peut rester noir)."
Write-Host "Ferme ensuite le jeu et lance .\build.ps1"
