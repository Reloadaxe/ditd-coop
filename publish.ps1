# Publie une mise à jour : compile le loader, les mods et le launcher, remplit dist/ et génère dist/manifest.json.
# Les launchers des joueurs détectent la nouvelle version au démarrage (via raw.githubusercontent.com).
#
#   .\publish.ps1 -Version 0.2.0 -Changelog "Correctifs synchro" [-LauncherVersion 1.0.1] [-Push]
#   -GameDir : dossier du jeu local (il faut BepInEx\interop pour compiler les mods)
param(
    [Parameter(Mandatory = $true)][string]$Version,
    [string]$Changelog = "",
    [string]$LauncherVersion,
    [string]$GameDir = (Join-Path $PSScriptRoot "..\Die in the Dungeon"),
    [switch]$Push
)
$ErrorActionPreference = "Stop"
$root = $PSScriptRoot
$dist = Join-Path $root "dist"
$files = Join-Path $dist "files"
$GameDir = (Resolve-Path $GameDir).Path

# Version courante du launcher : on la garde si -LauncherVersion n'est pas fourni
$verFile = Join-Path $root "version.json"
$ver = if (Test-Path $verFile) { Get-Content $verFile -Raw | ConvertFrom-Json } else { [pscustomobject]@{ mods = "0.0.0"; launcher = "1.0.0" } }
if (-not $LauncherVersion) { $LauncherVersion = $ver.launcher }

if (-not (Test-Path "$GameDir\BepInEx\interop\Assembly-CSharp.dll")) { throw "Interop introuvable dans $GameDir : lance le jeu une fois avec BepInEx." }

function Build($proj, [string[]]$extra = @()) {
    Write-Host "==> $proj" -ForegroundColor Cyan
    dotnet build (Join-Path $root $proj) -c Release -nologo -v:minimal "-p:GameDir=$GameDir" @extra
    if ($LASTEXITCODE -ne 0) { throw "Échec de compilation : $proj" }
}

Build "src\DiceLoader\DiceLoader.csproj"
$mods = Get-ChildItem (Join-Path $root "src\Mods") -Recurse -Filter *.csproj
foreach ($m in $mods) { Build $m.FullName.Substring($root.Length + 1) }
Build "launcher\DitdLauncher.csproj" @("-p:Version=$LauncherVersion")

# ---- Contenu publié ----
# Optional = nom de la case à cocher dans le launcher ($null = toujours installé)
$entries = @(
    @{ src = "src\DiceLoader\bin\Release\DiceLoader.dll"; path = "BepInEx/plugins/DiceLoader/DiceLoader.dll"; optional = $null },
    @{ src = "src\DiceLoader\bin\Release\DiceLoader.xml"; path = "BepInEx/plugins/DiceLoader/DiceLoader.xml"; optional = $null }
)
$optionalMods = @{ "DevTools" = "DevTools (triches)"; "ExampleMod" = "ExampleMod (exemple)" }
foreach ($m in $mods) {
    $name = $m.BaseName
    $entries += @{ src = "src\Mods\$name\bin\Release\$name.dll"; path = "Mods/$name/$name.dll"; optional = $optionalMods[$name] }
}

if (Test-Path $files) { Remove-Item -Recurse -Force $files }
New-Item -ItemType Directory -Force $files | Out-Null

$list = @()
foreach ($e in $entries) {
    $srcPath = Join-Path $root $e.src
    $fileName = Split-Path $e.src -Leaf
    Copy-Item $srcPath (Join-Path $files $fileName)
    $item = [ordered]@{
        path    = $e.path
        url     = "files/$fileName"
        sha256  = (Get-FileHash $srcPath -Algorithm SHA256).Hash.ToLower()
        size    = (Get-Item $srcPath).Length
        version = $Version
    }
    if ($e.optional) { $item.optional = $e.optional }
    $list += [pscustomobject]$item
}

$launcherExe = Join-Path $root "launcher\bin\Release\net48\DitdCoopLauncher.exe"
Copy-Item $launcherExe (Join-Path $dist "DitdCoopLauncher.exe") -Force

$manifest = [ordered]@{
    version   = $Version
    changelog = $Changelog
    launcher  = [ordered]@{
        path    = "DitdCoopLauncher.exe"
        url     = "DitdCoopLauncher.exe"
        version = $LauncherVersion
        sha256  = (Get-FileHash $launcherExe -Algorithm SHA256).Hash.ToLower()
        size    = (Get-Item $launcherExe).Length
    }
    bepinex   = [ordered]@{
        version = "6.0.0-be.793"
        url     = "https://builds.bepinex.dev/projects/bepinex_be/793/BepInEx-Unity.IL2CPP-win-x64-6.0.0-be.793%2Bdbc82f0.zip"
    }
    files     = $list
    remove    = @()
}
[IO.File]::WriteAllText((Join-Path $dist "manifest.json"), ($manifest | ConvertTo-Json -Depth 5), (New-Object Text.UTF8Encoding($false)))
[pscustomobject]@{ mods = $Version; launcher = $LauncherVersion } | ConvertTo-Json | Set-Content -Encoding utf8 $verFile

Write-Host "`ndist/ prêt : mods v$Version, launcher v$LauncherVersion" -ForegroundColor Green

if ($Push) {
    git -C $root add -A
    git -C $root commit -m "Release v$Version$(if ($Changelog) { " - $Changelog" })"
    git -C $root push
    if ($LASTEXITCODE -ne 0) { throw "git push a échoué" }
    Write-Host "Publié. Les joueurs recevront la mise à jour au prochain démarrage du launcher (cache GitHub : jusqu'à environ 5 min)." -ForegroundColor Green
} else {
    Write-Host "Vérifie puis publie avec : git add -A; git commit -m 'v$Version'; git push   (ou relance avec -Push)"
}
