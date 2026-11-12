param(
    [string] $Configuration = 'Release'
)
# Builds Buff Tracker and packs it the way Disfigure-Mod-Manager expects: a zip whose entries
# live under BepInEx/plugins/ (forward slashes), plus a .sha256 file next to it.
#
# Usage: tools/package.ps1
# Output: dist/BuffTracker-<version>.zip and dist/BuffTracker-<version>.zip.sha256
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.IO.Compression.FileSystem

$root = Split-Path -Parent $PSScriptRoot
$name = 'Disfigure.Mods.BuffTracker'
$project = Join-Path $root "src\$name\$name.csproj"

dotnet build $project -c $Configuration
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

$dll = Join-Path $root "src\$name\bin\$Configuration\net6.0\$name.dll"
$version = ((Get-Item $dll).VersionInfo.ProductVersion -split '\+')[0]

$dist = Join-Path $root 'dist'
$staging = Join-Path $dist 'staging'
if (Test-Path $staging) { [System.IO.Directory]::Delete($staging, $true) }
$plugin = Join-Path $staging "BepInEx\plugins\$name"
New-Item -ItemType Directory -Force $plugin | Out-Null
Copy-Item $dll $plugin
Copy-Item (Join-Path $root 'LICENSE') $plugin

$zip = Join-Path $dist "BuffTracker-$version.zip"
if (Test-Path $zip) { [System.IO.File]::Delete($zip) }
# ZipFile writes entry names with forward slashes, which the mod manager requires.
[System.IO.Compression.ZipFile]::CreateFromDirectory($staging, $zip)
[System.IO.Directory]::Delete($staging, $true)

$hash = (Get-FileHash $zip -Algorithm SHA256).Hash.ToLowerInvariant()
Set-Content -Path "$zip.sha256" -Value "$hash  $(Split-Path $zip -Leaf)" -NoNewline

$zipFile = [System.IO.Compression.ZipFile]::OpenRead($zip)
try {
    "Packed $zip"
    $zipFile.Entries | ForEach-Object { "  $($_.FullName)  $($_.Length)" }
    "SHA-256: $hash"
} finally {
    $zipFile.Dispose()
}
