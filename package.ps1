param([string]$Version = '1.10.0')
$ErrorActionPreference = 'Stop'
if ($Version -notmatch '^\d+\.\d+\.\d+([-.][A-Za-z0-9.]+)?$') { throw 'Invalid release version.' }
$stage = Join-Path $PSScriptRoot ('build\package-' + [guid]::NewGuid().ToString('N'))
& (Join-Path $PSScriptRoot 'build.ps1') -OutputDirectory $stage
$app = Join-Path $stage 'SteamCouch.exe'
$versionFound = (Get-Item -LiteralPath $app).VersionInfo.FileVersion
if ($versionFound -ne "$Version.0") { throw "App version $versionFound does not match release version $Version." }
$test = Start-Process -FilePath $app -ArgumentList '--self-test' -WindowStyle Hidden -Wait -PassThru
if ($test.ExitCode -ne 0) { throw "Self-tests failed. See $stage\data\last-error.txt" }
Get-Content -LiteralPath (Join-Path $stage 'data\self-test.txt')
foreach ($file in @('README.md','LICENSE','THIRD-PARTY-NOTICES.md','CHANGELOG.md')) { Copy-Item -LiteralPath (Join-Path $PSScriptRoot $file) -Destination $stage }
$dist = Join-Path $PSScriptRoot 'dist'
New-Item -ItemType Directory -Path $dist -Force | Out-Null
$zip = Join-Path $dist "SteamCouch-v$Version-windows-x64.zip"
# Deliberately omit data, diagnostics, personal settings, and developer files.
$include = @('SteamCouch.exe','SteamCouch.exe.config','assets','tools','README.md','LICENSE','THIRD-PARTY-NOTICES.md','CHANGELOG.md') | ForEach-Object { Join-Path $stage $_ }
Compress-Archive -LiteralPath $include -DestinationPath $zip -Force
$hash = (Get-FileHash -LiteralPath $zip -Algorithm SHA256).Hash.ToLowerInvariant()
"$hash  $([IO.Path]::GetFileName($zip))" | Set-Content -LiteralPath (Join-Path $dist 'SHA256SUMS.txt') -Encoding ASCII
Write-Output "Packaged $zip"
