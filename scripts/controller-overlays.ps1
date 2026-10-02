param([ValidateSet('Read','Disable','Enable','Restore','Test')][string]$Action='Read',[int]$RestoreNexus=-1,[int]$RestoreConnect=-1)
$ErrorActionPreference='Stop'
try {
 [Windows.Management.Core.ApplicationDataManager,Windows.Management.Core,ContentType=WindowsRuntime] | Out-Null
 $data=[Windows.Management.Core.ApplicationDataManager]::CreateForPackageFamily('Microsoft.XboxGamingOverlay_8wekyb3d8bbwe')
 $values=[System.Collections.Generic.IDictionary[string,object]]$data.LocalSettings.Values
 $names=@('OpenControllerBarOverNonGameOnNexus','OpenControllerBarOnGamepadConnect')
 if($Action -eq 'Test') {
  $name='SteamCouchTest-'+[guid]::NewGuid().ToString('N')
  $container=$data.LocalSettings.CreateContainer($name,0)
  $testValues=[System.Collections.Generic.IDictionary[string,object]]$container.Values
  try {
   foreach($key in $names){$testValues.set_Item($key,$false);if($testValues[$key] -ne $false){throw 'Controller disable verification failed.'};$testValues.set_Item($key,$true);if($testValues[$key] -ne $true){throw 'Controller enable verification failed.'}}
  } finally {$data.LocalSettings.DeleteContainer($name)}
  'PASS: Windows app-data controller preference writes and reads in an isolated container.'
  exit 0
 }
 if($Action -eq 'Restore') {
  for($i=0;$i -lt 2;$i++){$value=@($RestoreNexus,$RestoreConnect)[$i];if($value -eq -1){[void]$values.Remove($names[$i])}else{$values.set_Item($names[$i],[bool]$value)}}
  $data.SignalDataChanged()
 } elseif($Action -ne 'Read') {
  $enabled=$Action -eq 'Enable'
  foreach($name in $names){$values.set_Item($name,$enabled)}
  $data.SignalDataChanged()
  foreach($name in $names){if($values[$name] -ne $enabled){throw 'Windows could not update Controller Bar.'}}
 }
 $nexus=$true;$connect=$true
 if($values.ContainsKey($names[0])){$nexus=[bool]$values[$names[0]]}
 if($values.ContainsKey($names[1])){$connect=[bool]$values[$names[1]]}
 @{Nexus=$nexus;Connect=$connect;NexusExists=$values.ContainsKey($names[0]);ConnectExists=$values.ContainsKey($names[1])} | ConvertTo-Json -Compress
} catch { [Console]::Error.WriteLine($_.Exception.Message); exit 1 }
