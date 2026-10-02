param([string]$OutputDirectory = (Join-Path $PSScriptRoot 'build\SteamCouch'))
$ErrorActionPreference = 'Stop'
$compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
if (!(Test-Path -LiteralPath $compiler)) { throw 'The Windows .NET Framework x64 compiler is required.' }
New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
foreach ($directory in @('assets','tools\display','tools\audio')) { New-Item -ItemType Directory -Path (Join-Path $OutputDirectory $directory) -Force | Out-Null }
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'assets\steamcouch.png'),(Join-Path $PSScriptRoot 'assets\steamcouch.ico') -Destination (Join-Path $OutputDirectory 'assets') -Force
Expand-Archive -LiteralPath (Join-Path $PSScriptRoot 'vendor\multimonitortool-x64.zip') -DestinationPath (Join-Path $OutputDirectory 'tools\display') -Force
Expand-Archive -LiteralPath (Join-Path $PSScriptRoot 'vendor\soundvolumeview-x64.zip') -DestinationPath (Join-Path $OutputDirectory 'tools\audio') -Force
$sources = @('Program.cs','NativeDisplay.cs','ModernUI.cs','SteamClient.cs','XboxClient.cs','AssemblyInfo.cs') | ForEach-Object { Join-Path (Join-Path $PSScriptRoot 'src') $_ }
$icon = Join-Path $PSScriptRoot 'assets\steamcouch.ico'
$manifest = Join-Path $PSScriptRoot 'src\SteamCouch.manifest'
$output = Join-Path $OutputDirectory 'SteamCouch.exe'
& $compiler /nologo /target:winexe /platform:x64 /optimize+ /r:System.Windows.Forms.dll /r:System.Drawing.dll /r:System.Web.Extensions.dll /r:Microsoft.VisualBasic.dll /win32icon:$icon /win32manifest:$manifest /out:$output $sources
if ($LASTEXITCODE -ne 0) { throw 'Compilation failed.' }
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'src\SteamCouch.exe.config') -Destination ($output+'.config') -Force
Write-Output "Built $output"
