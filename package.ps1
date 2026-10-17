# Builds the plugin and creates a ready-to-extract release zip containing BepInEx and the plugin.
# Usage: powershell -ExecutionPolicy Bypass -File package.ps1 [-GameDir "D:\path\to\DoubleDragonGaiden_ROTD"]
param([string]$GameDir)
$ErrorActionPreference = 'Stop'
$ProgressPreference = 'SilentlyContinue'
Set-Location $PSScriptRoot

$Version = '1.1.0'
$Name = "DoubleDragonGaiden-ROTD-WidescreenFix-$Version"
$BepInExUrl = 'https://github.com/BepInEx/BepInEx/releases/download/v5.4.23.5/BepInEx_win_x64_5.4.23.5.zip'
$BepInExLicenseUrl = 'https://raw.githubusercontent.com/BepInEx/BepInEx/master/LICENSE'
$PluginPath = 'BepInEx\plugins\WidescreenFix.dll'

# 1) Build
$buildArgs = @('build', '-c', 'Release', '-nologo', '-v', 'q')
if ($GameDir) { $buildArgs += "-p:GameDir=$GameDir" }
& dotnet @buildArgs
if ($LASTEXITCODE -ne 0) { throw 'Build failed' }
$dll = Join-Path $PSScriptRoot 'bin\Release\netstandard2.1\WidescreenFix.dll'

# 2) Download BepInEx (cached)
[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
$cache = Join-Path $PSScriptRoot '.cache'
New-Item -ItemType Directory -Force $cache | Out-Null
$bepZip = Join-Path $cache ([IO.Path]::GetFileName([Uri]::UnescapeDataString($BepInExUrl)))
if (-not (Test-Path $bepZip)) { Invoke-WebRequest $BepInExUrl -OutFile $bepZip -UseBasicParsing }
$bepLicense = Join-Path $cache 'BepInEx-LICENSE.txt'
if (-not (Test-Path $bepLicense)) { Invoke-WebRequest $BepInExLicenseUrl -OutFile $bepLicense -UseBasicParsing }

# 3) Stage: BepInEx + plugin
$release = Join-Path $PSScriptRoot 'release'
$stage = Join-Path $release $Name
if (Test-Path $stage) { Remove-Item -Recurse -Force $stage }
Expand-Archive $bepZip $stage
Remove-Item (Join-Path $stage 'changelog.txt') -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Force (Split-Path (Join-Path $stage $PluginPath)) | Out-Null
Copy-Item $dll (Join-Path $stage $PluginPath)
Copy-Item $bepLicense (Join-Path $stage 'BepInEx\LICENSE-BepInEx.txt')

# 4) Zip with forward-slash paths (portable), plus the bare DLL
Add-Type -AssemblyName System.IO.Compression, System.IO.Compression.FileSystem
$zipPath = Join-Path $release "$Name.zip"
if (Test-Path $zipPath) { Remove-Item $zipPath }
$zip = [IO.Compression.ZipFile]::Open($zipPath, 'Create')
try {
    Get-ChildItem $stage -Recurse -File -Force | ForEach-Object {
        $entry = $_.FullName.Substring($stage.Length + 1).Replace('\', '/')
        [void][IO.Compression.ZipFileExtensions]::CreateEntryFromFile($zip, $_.FullName, $entry, 'Optimal')
    }
} finally { $zip.Dispose() }
Copy-Item $dll (Join-Path $release 'WidescreenFix.dll')
Remove-Item -Recurse -Force $stage

Write-Host "Created $zipPath ($([math]::Round((Get-Item $zipPath).Length / 1MB, 1)) MB)"
