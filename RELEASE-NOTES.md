# SteamCouch 1.3.0

Choose Steam Big Picture or Windows 11 Xbox mode in Settings. SteamCouch enables
your selected TV, makes it primary, switches playback audio, and launches the
selected gaming mode. Returning to desktop exits that mode before restoring the
saved displays and audio.

Xbox mode requires the Xbox app and Xbox mode enabled in Windows Settings >
Gaming. Confirm Windows' Win + F11 shortcut works before selecting it in
SteamCouch. Xbox mode may cover secondary screens, even if they remain connected.

The launcher choice is saved with the recovery snapshot, so restoration still
uses the correct mode after an app restart or a settings change. Entry failures
roll back the saved setup; exit failures still restore displays and audio while
retaining a retry point. Existing settings default to Steam Big Picture.

Download SteamCouch-v1.3.0-windows-x64.zip, extract all files together, and open
SteamCouch.exe. Includes the required NirSoft tools and DPI configuration.

Validation: Steam regression tests, Xbox orchestration/recovery tests with fake
devices, and Settings/layout checks passed. Actual Xbox-mode entry on the
development PC remains unverified pending Windows' Xbox-mode configuration.
