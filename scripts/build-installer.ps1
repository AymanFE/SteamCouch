param([string]$Version='1.10.1',[string]$Compiler=(Join-Path $PSScriptRoot '..\build\inno\ISCC.exe'))
$ErrorActionPreference='Stop'
if(!(Test-Path -LiteralPath $Compiler)){throw 'Install Inno Setup 7 and pass -Compiler with the path to ISCC.exe.'}
$root=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$zip=Join-Path $root "dist\SteamCouch-v$Version-windows-x64.zip"
$stage=Join-Path $root ('build\installer-package-'+[guid]::NewGuid().ToString('N'))
Expand-Archive -LiteralPath $zip -DestinationPath $stage
& $Compiler "/DAppVersion=$Version" "/DPackageRoot=$stage" (Join-Path $PSScriptRoot 'installer.iss')
if($LASTEXITCODE -ne 0){throw 'Installer compilation failed'}
$installer=Join-Path $root "dist\SteamCouch-v$Version-setup-x64.exe"
$files=@($zip,$installer)
$files | ForEach-Object { '{0}  {1}' -f (Get-FileHash -LiteralPath $_ -Algorithm SHA256).Hash.ToLowerInvariant(),[IO.Path]::GetFileName($_) } | Set-Content (Join-Path $root 'dist\SHA256SUMS.txt') -Encoding ASCII
