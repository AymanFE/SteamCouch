param([Parameter(Mandatory=$true)][string]$Stage,[Parameter(Mandatory=$true)][string]$Target,[int]$WaitPid,[switch]$Tray)
$ErrorActionPreference='Stop'
$targetRoot=[IO.Path]::GetFullPath($Target).TrimEnd('\')
$stageRoot=[IO.Path]::GetFullPath($Stage).TrimEnd('\')
$expected=[IO.Path]::Combine($targetRoot,'data','update-staging')+'\'
if(!$stageRoot.StartsWith($expected,[StringComparison]::OrdinalIgnoreCase) -or [IO.Path]::GetFileName($stageRoot) -ne 'files'){throw 'Invalid update staging path'}
if(!(Test-Path -LiteralPath ([IO.Path]::Combine($stageRoot,'SteamCouch.exe')))){throw 'Update executable is missing'}
$allowed=@('SteamCouch.exe','SteamCouch.exe.config','assets','tools','README.md','LICENSE','THIRD-PARTY-NOTICES.md','CHANGELOG.md')
$items=@(Get-ChildItem -LiteralPath $stageRoot -Recurse -File)
foreach($file in $items){$relative=$file.FullName.Substring($stageRoot.Length+1);if($relative.Split('\')[0] -notin $allowed -or ($file.Attributes -band [IO.FileAttributes]::ReparsePoint)){throw 'Unexpected update file'}}
$backup=[IO.Path]::Combine($targetRoot,'data','update-backups',[guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $backup -Force | Out-Null
$installed=New-Object 'System.Collections.Generic.List[string]'
try {
 $process=if($WaitPid -gt 0){Get-Process -Id $WaitPid -ErrorAction SilentlyContinue}
 if($process -and !$process.WaitForExit(30000)){throw 'SteamCouch did not exit; update postponed.'}
 foreach($file in $items){
  $relative=$file.FullName.Substring($stageRoot.Length+1);$destination=[IO.Path]::GetFullPath([IO.Path]::Combine($targetRoot,$relative));if(!$destination.StartsWith($targetRoot+'\',[StringComparison]::OrdinalIgnoreCase)){throw 'Invalid update destination'}
  $old=[IO.Path]::Combine($backup,$relative)
  if(Test-Path -LiteralPath $destination){New-Item -ItemType Directory -Path ([IO.Path]::GetDirectoryName($old)) -Force | Out-Null;Copy-Item -LiteralPath $destination -Destination $old}
  New-Item -ItemType Directory -Path ([IO.Path]::GetDirectoryName($destination)) -Force | Out-Null
  $installed.Add($relative);Copy-Item -LiteralPath $file.FullName -Destination $destination -Force
 }
 $new=Start-Process -FilePath ([IO.Path]::Combine($targetRoot,'SteamCouch.exe')) -ArgumentList --settings -WindowStyle Hidden -PassThru
 Start-Sleep -Seconds 3
 if($new.HasExited){throw 'The updated app could not start.'}
 'Update installed; previous version saved in data/update-backups.' | Set-Content -LiteralPath ([IO.Path]::Combine($targetRoot,'data','update-result.txt'))
} catch {
 $failure=$_.Exception.Message
 if($installed.Count -eq 0 -and $process -and !$process.HasExited){$failure | Set-Content -LiteralPath ([IO.Path]::Combine($targetRoot,'data','update-result.txt'));exit 1}
 foreach($relative in $installed){$destination=[IO.Path]::GetFullPath([IO.Path]::Combine($targetRoot,$relative));$old=[IO.Path]::Combine($backup,$relative);if(!$destination.StartsWith($targetRoot+'\',[StringComparison]::OrdinalIgnoreCase)){continue};if(Test-Path -LiteralPath $old){Copy-Item -LiteralPath $old -Destination $destination -Force}else{Remove-Item -LiteralPath $destination -ErrorAction SilentlyContinue}}
 $failure | Set-Content -LiteralPath ([IO.Path]::Combine($targetRoot,'data','update-result.txt'))
 Start-Process -FilePath ([IO.Path]::Combine($targetRoot,'SteamCouch.exe')) -ArgumentList --settings -WindowStyle Hidden
 exit 1
}
