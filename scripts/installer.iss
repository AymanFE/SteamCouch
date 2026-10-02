#ifndef AppVersion
  #define AppVersion "1.7.1"
#endif
#ifndef PackageRoot
  #error PackageRoot must point to the clean portable package directory
#endif
[Setup]
AppId={{95DFF2A4-0BBB-42D2-BE69-CCBF224D7AA7}
AppName=SteamCouch
AppVersion={#AppVersion}
AppPublisher=AymanFE
AppPublisherURL=https://github.com/AymanFE/SteamCouch
DefaultDirName={localappdata}\Programs\SteamCouch
DefaultGroupName=SteamCouch
PrivilegesRequired=lowest
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
MinVersion=10.0.19041
OutputDir=..\dist
OutputBaseFilename=SteamCouch-v{#AppVersion}-setup-x64
SetupIconFile=..\assets\steamcouch.ico
UninstallDisplayIcon={app}\SteamCouch.exe
LicenseFile=..\LICENSE
InfoBeforeFile=..\THIRD-PARTY-NOTICES.md
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
AppMutex=Local\TVLounge.SingleInstance
CloseApplications=no
RestartApplications=no
[Tasks]
Name: "desktopicon"; Description: "Create a desktop shortcut"; Flags: unchecked
Name: "cecdriver"; Description: "Set up the USB-CEC adapter driver (optional; Windows permission may be required)"; Flags: unchecked
[Files]
Source: "{#PackageRoot}\*"; DestDir: "{app}"; Excludes: "data\*"; Flags: ignoreversion recursesubdirs createallsubdirs
[Icons]
Name: "{group}\SteamCouch"; Filename: "{app}\SteamCouch.exe"; Parameters: "--settings"
Name: "{autodesktop}\SteamCouch"; Filename: "{app}\SteamCouch.exe"; Parameters: "--settings"; Tasks: desktopicon
[Run]
Filename: "{app}\tools\cec\driver\p8-usbcec-driver-installer.exe"; Verb: "runas"; Flags: shellexec waituntilterminated; Tasks: cecdriver
Filename: "{app}\SteamCouch.exe"; Parameters: "--settings"; Description: "Open SteamCouch"; Flags: nowait postinstall skipifsilent
[Code]
function PrepareToInstall(var NeedsRestart: Boolean): String;
begin
  Result := '';
  if FileExists(ExpandConstant('{app}\data\restore.json')) then
    Result := 'Return to desktop mode in SteamCouch before installing an update.';
end;
function InitializeUninstall(): Boolean;
begin
  Result := not FileExists(ExpandConstant('{app}\data\restore.json')) and not CheckForMutexes('Local\TVLounge.SingleInstance');
  if not Result then MsgBox('Return to desktop mode and exit SteamCouch before uninstalling.', mbError, MB_OK);
end;
procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
var Command: String;
begin
  if CurUninstallStep = usUninstall then
    if RegQueryStringValue(HKCU, 'Software\Microsoft\Windows\CurrentVersion\Run', 'SteamCouch', Command) then
      if Pos(Lowercase(ExpandConstant('{app}\SteamCouch.exe')), Lowercase(Command)) > 0 then
        RegDeleteValue(HKCU, 'Software\Microsoft\Windows\CurrentVersion\Run', 'SteamCouch');
end;
