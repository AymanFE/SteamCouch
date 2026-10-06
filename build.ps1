param([string]$OutputDirectory = (Join-Path $PSScriptRoot 'build\SteamCouch'))
$ErrorActionPreference = 'Stop'
$compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
if (!(Test-Path -LiteralPath $compiler)) { throw 'The Windows .NET Framework x64 compiler is required.' }
New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
foreach ($directory in @('assets','tools\display','tools\audio','tools\windows','tools\cec')) { New-Item -ItemType Directory -Path (Join-Path $OutputDirectory $directory) -Force | Out-Null }
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'assets\steamcouch.png'),(Join-Path $PSScriptRoot 'assets\steamcouch.ico') -Destination (Join-Path $OutputDirectory 'assets') -Force
Expand-Archive -LiteralPath (Join-Path $PSScriptRoot 'vendor\multimonitortool-x64.zip') -DestinationPath (Join-Path $OutputDirectory 'tools\display') -Force
Expand-Archive -LiteralPath (Join-Path $PSScriptRoot 'vendor\soundvolumeview-x64.zip') -DestinationPath (Join-Path $OutputDirectory 'tools\audio') -Force
$cecBundle = Join-Path $PSScriptRoot 'vendor\libcec-runtime-x64-8.1.7.zip'
if ((Get-FileHash -LiteralPath $cecBundle -Algorithm SHA256).Hash -ne 'A083281D9959A1E1C76518DE97AEAE87D91EA98B0572EA17722C516C921FD779') { throw 'CEC vendor bundle checksum mismatch.' }
Expand-Archive -LiteralPath $cecBundle -DestinationPath (Join-Path $OutputDirectory 'tools\cec') -Force
$adbBundle=Join-Path $PSScriptRoot 'vendor\adb-runtime-win-37.0.1.zip'
if ((Get-FileHash -LiteralPath $adbBundle -Algorithm SHA256).Hash -ne 'EEFFD599A1688DAFEA2AF1C49D1E5CFE5EF727FC6847DC85BA3F1C4BD7FADC27'){throw 'ADB vendor bundle checksum mismatch.'}
Expand-Archive -LiteralPath $adbBundle -DestinationPath (Join-Path $OutputDirectory 'tools\adb') -Force
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'scripts\controller-overlays.ps1') -Destination (Join-Path $OutputDirectory 'tools\windows\controller-overlays.ps1') -Force
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'scripts\apply-update.ps1') -Destination (Join-Path $OutputDirectory 'tools\windows\apply-update.ps1') -Force
$sdl=Join-Path $PSScriptRoot 'vendor\SDL3-3.4.18-win32-x64.zip'
if((Get-FileHash -LiteralPath $sdl -Algorithm SHA256).Hash -ne '75C2C0FC74E7D1206AAEDC22893A35887332F4F85F5C8B74FDDC10D177985F2C'){throw 'SDL controller bundle checksum mismatch.'}
Expand-Archive -LiteralPath $sdl -DestinationPath (Join-Path $OutputDirectory 'tools\controllers') -Force
$present=Join-Path $PSScriptRoot 'vendor\PresentMon-2.6.0-x64.exe'
if((Get-FileHash -LiteralPath $present -Algorithm SHA256).Hash -ne 'B2A706BC6AD475749E3B7E3409263AA1E6906D45BDCF993F6DBC0F660188F1AF'){throw 'PresentMon checksum mismatch.'}
New-Item -ItemType Directory -Path (Join-Path $OutputDirectory 'tools\performance') -Force | Out-Null
Copy-Item -LiteralPath $present -Destination (Join-Path $OutputDirectory 'tools\performance\PresentMon.exe') -Force
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'vendor\PresentMon-LICENSE.txt'),(Join-Path $PSScriptRoot 'vendor\PresentMon-THIRD_PARTY.txt') -Destination (Join-Path $OutputDirectory 'tools\performance') -Force
$sources = @('Program.cs','ControllerBattery.cs','WindowsControllerBattery.cs','PerformanceCapture.cs','HardwareTelemetry.cs','GameFocus.cs','PerformanceHud.cs','GamingToolsUI.cs','GamingToolsTests.cs','CecClient.cs','CecUI.cs','GoogleTv.cs','GoogleTvUI.cs','GoogleTvGuide.cs','WindowPlacement.cs','OtherMonitors.cs','OtherMonitorsUI.cs','NativeDisplay.cs','ModernUI.cs','SteamClient.cs','XboxClient.cs','FeatureCore.cs','QuickMenuCore.cs','QuickMenuWindow.cs','QuickMenuIntegration.cs','PlayniteClient.cs','OptionalCore.cs','GameActivity.cs','OptionalUI.cs','Updates.cs','ExtendedUI.cs','SetupWizard.cs','AssemblyInfo.cs') | ForEach-Object { Join-Path (Join-Path $PSScriptRoot 'src') $_ }
$icon = Join-Path $PSScriptRoot 'assets\steamcouch.ico'
$manifest = Join-Path $PSScriptRoot 'src\SteamCouch.manifest'
$output = Join-Path $OutputDirectory 'SteamCouch.exe'
$winMetadata=Join-Path $env:WINDIR 'System32\WinMetadata'
$framework=Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319'
& $compiler /r:"$winMetadata\Windows.Gaming.winmd" /r:"$winMetadata\Windows.Foundation.winmd" /r:"$winMetadata\Windows.Devices.winmd" /r:"$winMetadata\Windows.System.winmd" /r:"$framework\System.Runtime.dll" /r:"$framework\System.Runtime.WindowsRuntime.dll" /nologo /target:winexe /platform:x64 /optimize+ /r:System.Windows.Forms.dll /r:System.Drawing.dll /r:System.Web.Extensions.dll /r:Microsoft.VisualBasic.dll /r:System.IO.Compression.dll /r:System.IO.Compression.FileSystem.dll /win32icon:$icon /win32manifest:$manifest /out:$output $sources
if ($LASTEXITCODE -ne 0) { throw 'Compilation failed.' }
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'src\SteamCouch.exe.config') -Destination ($output+'.config') -Force
Write-Output "Built $output"
