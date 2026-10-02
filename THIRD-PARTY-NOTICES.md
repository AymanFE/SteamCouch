# Third-party notices

## NirSoft utilities

The portable release includes MultiMonitorTool and SoundVolumeView by Nir Sofer.
Original, unmodified distribution archives are kept in `vendor/`; release builds
extract every file from each archive into `tools/display` and `tools/audio`.

- MultiMonitorTool: https://www.nirsoft.net/utils/multi_monitor_tool.html
- SoundVolumeView: https://www.nirsoft.net/utils/sound_volume_view.html

These utilities are freeware, not MIT-licensed. Their included readme files
contain the complete terms. The following distribution condition appears in both:

> This utility is released as freeware. You are allowed to freely distribute this utility via floppy disk, CD-ROM, Internet, or in any other way, as long as you don't charge anything for this and you don't sell it or distribute it as a part of commercial product. If you distribute this utility, you must include all files in the distribution package, without any modification !

SteamCouch's MIT license covers its original source code and documentation. It
does not relicense these bundled utilities. Anyone making a commercial fork
must handle those dependencies under their original terms or replace them.

## Steam branding

Steam and the Steam logo are trademarks of Valve Corporation. SteamCouch is an
independent community utility and is not affiliated with or endorsed by Valve.
The MIT license grants no rights to Valve's trademarks. Icon generation notes
are recorded in `assets/README.md`.

## Bundled HDMI-CEC software

Both installer and portable packages include the unmodified native libCEC
8.1.7 client and library from Pulse-Eight, under GPL-2.0-or-later. The complete
upstream license and matching native source archive are in `tools/cec/`.
SteamCouch runs the separate command-line program; it does not link libCEC.
Its original code remains MIT licensed.

The required Microsoft C++ v14 x64 runtime DLLs (14.51.36247.0) are included
alongside the native client, under Microsoft's own redistribution/use terms.
The signed Pulse-Eight USB-CEC driver installer is also included unchanged;
it carries the Windows Driver Kit DPInst redistributable. Driver setup is
optional and may require Windows administrator permission. No driver is
installed merely by extracting or launching the portable app.

See `tools/cec/BUNDLED-NOTICES.txt`, `tools/cec/LICENSE.md` and the source
archive for provenance, complete upstream notices and dependency details.
.NET 8 bindings, Python/Node.js bindings, firmware flashers and firmware
bootloader components are not required by SteamCouch and are not bundled.
## Bundled Android Debug Bridge

Both distributions include unmodified adb.exe, AdbWinApi.dll, AdbWinUsbApi.dll
and libwinpthread-1.dll from Google's Windows Platform Tools 37.0.1.
Only the required native ADB components are redistributed, with the complete
upstream NOTICE.txt and provenance in tools/adb/BUNDLED-NOTICES.txt.
These open source components keep their own Apache/BSD and other included
notices; SteamCouch's MIT license does not relicense them.

Official distribution: https://developer.android.com/tools/releases/platform-tools
Android ADB source: https://android.googlesource.com/platform/packages/modules/adb/
