param([string]$OutputDirectory = (Join-Path $PSScriptRoot 'build\SteamCouch'))
$ErrorActionPreference = 'Stop'
$compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
if (!(Test-Path -LiteralPath $compiler)) { throw 'The Windows .NET Framework x64 compiler is required.' }
New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
foreach ($directory in @('assets','tools\display','tools\audio','tools\windows')) { New-Item -ItemType Directory -Path (Join-Path $OutputDirectory $directory) -Force | Out-Null }
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'assets\steamcouch.png'),(Join-Path $PSScriptRoot 'assets\steamcouch.ico') -Destination (Join-Path $OutputDirectory 'assets') -Force
Expand-Archive -LiteralPath (Join-Path $PSScriptRoot 'vendor\multimonitortool-x64.zip') -DestinationPath (Join-Path $OutputDirectory 'tools\display') -Force
Expand-Archive -LiteralPath (Join-Path $PSScriptRoot 'vendor\soundvolumeview-x64.zip') -DestinationPath (Join-Path $OutputDirectory 'tools\audio') -Force
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'scripts\controller-overlays.ps1') -Destination (Join-Path $OutputDirectory 'tools\windows\controller-overlays.ps1') -Force
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'scripts\apply-update.ps1') -Destination (Join-Path $OutputDirectory 'tools\windows\apply-update.ps1') -Force
$sources = @('Program.cs','CecClient.cs','CecUI.cs','WindowPlacement.cs','NativeDisplay.cs','ModernUI.cs','SteamClient.cs','XboxClient.cs','FeatureCore.cs','QuickMenuCore.cs','QuickMenuWindow.cs','QuickMenuIntegration.cs','PlayniteClient.cs','OptionalCore.cs','GameActivity.cs','OptionalUI.cs','Updates.cs','ExtendedUI.cs','SetupWizard.cs','AssemblyInfo.cs') | ForEach-Object { Join-Path (Join-Path $PSScriptRoot 'src') $_ }
$icon = Join-Path $PSScriptRoot 'assets\steamcouch.ico'
$manifest = Join-Path $PSScriptRoot 'src\SteamCouch.manifest'
$output = Join-Path $OutputDirectory 'SteamCouch.exe'
& $compiler /nologo /target:winexe /platform:x64 /optimize+ /r:System.Windows.Forms.dll /r:System.Drawing.dll /r:System.Web.Extensions.dll /r:Microsoft.VisualBasic.dll /r:System.IO.Compression.dll /r:System.IO.Compression.FileSystem.dll /win32icon:$icon /win32manifest:$manifest /out:$output $sources
if ($LASTEXITCODE -ne 0) { throw 'Compilation failed.' }
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'src\SteamCouch.exe.config') -Destination ($output+'.config') -Force
Write-Output "Built $output"
