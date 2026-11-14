# Compile DiceLoader et tous les mods de src\Mods, puis les déploie dans le dossier du jeu.
#   .\build.ps1                          (dossier du jeu par défaut : ..\Die in the Dungeon)
#   .\build.ps1 -GameDir "D:\Jeux\Die in the Dungeon"
param(
    [string]$GameDir = (Join-Path $PSScriptRoot "..\Die in the Dungeon"),
    [string]$Configuration = "Release"
)
$ErrorActionPreference = "Stop"
$GameDir = (Resolve-Path $GameDir).Path

if (-not (Test-Path "$GameDir\BepInEx\interop\Assembly-CSharp.dll")) {
    Write-Host "Les assemblies d'interop n'existent pas encore." -ForegroundColor Yellow
    Write-Host "1) Lance .\install.ps1   2) Démarre le jeu une fois et attends le menu principal   3) Relance ce script." -ForegroundColor Yellow
    exit 1
}

$projects = @("$PSScriptRoot\src\DiceLoader\DiceLoader.csproj") + (Get-ChildItem "$PSScriptRoot\src\Mods" -Recurse -Filter *.csproj | ForEach-Object FullName)
foreach ($p in $projects) {
    Write-Host "==> $([IO.Path]::GetFileNameWithoutExtension($p))" -ForegroundColor Cyan
    dotnet build $p -c $Configuration -p:GameDir="$GameDir" -nologo -v:minimal
    if ($LASTEXITCODE -ne 0) { throw "Échec de compilation : $p" }
}
Write-Host "`nTerminé. Lance le jeu, puis appuie sur F1 pour ouvrir le menu DiceLoader." -ForegroundColor Green
