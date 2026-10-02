$ErrorActionPreference='Stop'
$root=Join-Path $PSScriptRoot ('..\build\update-restart-test-'+[guid]::NewGuid().ToString('N'))
$launches=New-Object 'System.Collections.Generic.List[string]'
function Start-Process { param($FilePath,$ArgumentList,$WindowStyle,[switch]$PassThru) $launches.Add([string]$ArgumentList); return [pscustomobject]@{HasExited=$false} }
foreach($trayMode in @($false,$true)) {
 $target=Join-Path $root ([string]$trayMode)
 $files=Join-Path $target 'data\update-staging\test\files'
 New-Item -ItemType Directory -Path $files -Force | Out-Null
 Set-Content (Join-Path $target 'SteamCouch.exe') 'previous'
 Set-Content (Join-Path $target 'data\settings.json') 'keep my preferences'
 Set-Content (Join-Path $files 'SteamCouch.exe') 'updated'
 & (Join-Path $PSScriptRoot 'apply-update.ps1') -Stage $files -Target $target -WaitPid 0 -Tray:$trayMode
 $expected=if($trayMode){'--tray'}else{'--settings'}
 if($launches[$launches.Count-1] -ne $expected){throw 'Wrong update restart mode'}
 if((Get-Content (Join-Path $target 'data\settings.json') -Raw).Trim() -ne 'keep my preferences'){throw 'Settings changed'}
 if((Get-Content (Join-Path $target 'SteamCouch.exe') -Raw).Trim() -ne 'updated'){throw 'Update not installed'}
}
Write-Output 'PASS: installer copies update, preserves preferences and restarts in requested tray/settings mode.'
