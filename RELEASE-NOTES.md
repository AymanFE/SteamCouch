# SteamCouch 1.3.1

Settings now includes **Disable Game Bar & Controller Bar**. Enable it to stop the controller's Xbox/Guide button opening Game Bar or Controller Bar alongside Steam's menu. It also disables Controller Bar appearing on controller connection. The change applies immediately across Windows for the current user. Win + G still opens Game Bar; turning the option off restores the controller shortcut.

The switch reads the actual Windows setting when SteamCouch starts. Existing display, audio, launcher, shortcut, and startup preferences are preserved.

Download SteamCouch-v1.3.1-windows-x64.zip, extract all files together, and open SteamCouch.exe. Keep your existing data folder when updating.

Validation: isolated registry and Windows app-data controller-setting tests, settings persistence, orchestration tests, repeated dropdown dismissal checks, normal and compact layouts, maximized Home, three real monitor DPI transitions, and simulated 100–300% scaling passed. A physical controller button press has not been tested.
