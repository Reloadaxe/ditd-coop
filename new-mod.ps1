# Crée un nouveau mod à partir d'ExampleMod.
#   .\new-mod.ps1 -Name MonSuperMod [-Author "Moi"]
param(
    [Parameter(Mandatory = $true)][ValidatePattern('^[A-Za-z_][A-Za-z0-9_]*$')][string]$Name,
    [string]$Author = $env:USERNAME
)
$ErrorActionPreference = "Stop"
$dst = Join-Path $PSScriptRoot "src\Mods\$Name"
if (Test-Path $dst) { throw "Le mod $Name existe déjà." }
New-Item -ItemType Directory $dst | Out-Null

$src = Join-Path $PSScriptRoot "src\Mods\ExampleMod"
(Get-Content "$src\ExampleMod.csproj" -Raw) -replace 'ExampleMod', $Name | Set-Content -Encoding utf8 "$dst\$Name.csproj"
$code = Get-Content "$src\ExampleMod.cs" -Raw
$code = $code -replace '"dicemodding\.example", "Example Mod"', ('"' + $Author.ToLower() + '.' + $Name.ToLower() + '", "' + $Name + '"') `
              -replace 'Author = "DiceModding"', ('Author = "' + $Author + '"') `
              -replace '\bExampleMod\b', $Name
Set-Content -Encoding utf8 "$dst\$Name.cs" $code
Write-Host "Mod créé : $dst" -ForegroundColor Green
Write-Host "Compile-le avec .\build.ps1 ; la DLL ira dans <jeu>\Mods\$Name\"
