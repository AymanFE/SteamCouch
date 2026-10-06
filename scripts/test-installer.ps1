param([string]$Version='1.10.0')
$ErrorActionPreference='Stop'
if($Version -notmatch '^\d+\.\d+\.\d+$'){throw 'Invalid test version'}
$key='HKCU:\Software\Microsoft\Windows\CurrentVersion\Uninstall\{95DFF2A4-0BBB-42D2-BE69-CCBF224D7AA7}_is1'
if(Test-Path $key){throw 'Existing installer registration; use a clean test machine'}
$root=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\build'))
$target=[IO.Path]::GetFullPath((Join-Path $root ('installer-test-'+[guid]::NewGuid().ToString('N'))))
if(!$target.StartsWith($root+'\',[StringComparison]::OrdinalIgnoreCase)){throw 'Test path escapes workspace'}
$installer=Join-Path $PSScriptRoot "..\dist\SteamCouch-v$Version-setup-x64.exe"
$args=@('/VERYSILENT','/SUPPRESSMSGBOXES','/NORESTART','/CURRENTUSER','/NOICONS','/TASKS=',('/DIR="'+$target+'"'))
$installed=$false
try {
 $p=Start-Process -FilePath $installer -ArgumentList $args -WindowStyle Hidden -PassThru -Wait
 if($p.ExitCode -ne 0){throw "Installer failed: $($p.ExitCode)"};$installed=$true
 foreach($file in @('SteamCouch.exe','tools\controllers\SDL3.dll','tools\controllers\LICENSE.txt','tools\performance\PresentMon.exe','tools\performance\PresentMon-LICENSE.txt','tools\performance\PresentMon-THIRD_PARTY.txt','tools\adb\adb.exe','tools\adb\AdbWinApi.dll','tools\adb\AdbWinUsbApi.dll','tools\adb\libwinpthread-1.dll','tools\adb\NOTICE.txt','tools\cec\cec-client.exe','tools\cec\cec.dll','tools\cec\msvcp140.dll','tools\cec\vcruntime140.dll','tools\cec\vcruntime140_1.dll','tools\cec\driver\p8-usbcec-driver-installer.exe','tools\cec\sources\libcec-8.1.7-source.zip')){if(!(Test-Path (Join-Path $target $file))){throw "Missing installed dependency: $file"}}
 $app=Join-Path $target 'SteamCouch.exe';$test=Start-Process -FilePath $app -ArgumentList --self-test -WindowStyle Hidden -PassThru -Wait
 if($test.ExitCode -ne 0){throw 'Installed application regression tests failed'}
 $settings=Join-Path $target 'data\settings.json';Set-Content -LiteralPath $settings '{"Startup":false,"CecEnabled":false,"CecHdmiPort":4}'
 $hash=(Get-FileHash -LiteralPath $settings -Algorithm SHA256).Hash
 $server=Start-Process -FilePath (Join-Path $target 'tools\adb\adb.exe') -ArgumentList '-P 5039 start-server' -WindowStyle Hidden -PassThru
 if(!$server.WaitForExit(10000)){throw 'TV helper startup timed out'}
 if($server.ExitCode -ne 0){throw 'Could not start private TV helper for upgrade test'}
 $p=Start-Process -FilePath $installer -ArgumentList $args -WindowStyle Hidden -PassThru -Wait
 if($p.ExitCode -ne 0 -or (Get-FileHash -LiteralPath $settings -Algorithm SHA256).Hash -ne $hash){throw 'Reinstall changed preferences or failed'}
 $p=Start-Process -FilePath (Join-Path $target 'unins000.exe') -ArgumentList '/VERYSILENT /SUPPRESSMSGBOXES /NORESTART' -WindowStyle Hidden -PassThru -Wait
 if($p.ExitCode -ne 0 -or (Test-Path $app) -or !(Test-Path $settings)){throw 'Uninstall failed or deleted preferences'};$installed=$false
 if(Test-Path $key){throw 'Uninstall registration left behind'}
 'PASS: installer deploys all dependencies; installed CEC runtime loads; app regression suite passes; reinstall preserves settings; uninstall removes app and preserves settings.' | Set-Content (Join-Path $root 'installer-test.txt')
 Get-Content (Join-Path $root 'installer-test.txt')
} finally {
 if($installed -and (Test-Path (Join-Path $target 'unins000.exe'))){Start-Process -FilePath (Join-Path $target 'unins000.exe') -ArgumentList '/VERYSILENT /SUPPRESSMSGBOXES /NORESTART' -WindowStyle Hidden -Wait}
}
