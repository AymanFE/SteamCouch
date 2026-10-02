# SteamCouch

<p align="center"><img src="assets/steamcouch.png" width="128" alt="SteamCouch sofa icon"></p>

**Your TV, your audio, and your gaming launcher. One shortcut.**

SteamCouch is a small Windows tray app for moving between desktop use and couch
gaming. It reconnects your selected TV, makes it the main display, switches
playback audio, and opens your choice of Steam Big Picture or Xbox mode. Press the same shortcut again to
exit the selected mode and restore your previous display layout and audio devices.
Steam stays running.

## Download

**[Download the latest Windows release](https://github.com/AymanFE/SteamCouch/releases/latest)**

Choose `SteamCouch-v1.3.0-windows-x64.zip` under **Assets**. You do not need the
"Source code" downloads unless you want to build or modify the app.

1. Extract the whole ZIP to a writable folder, such as Documents or a folder of your choice.
2. Open `SteamCouch.exe`.
3. Open **TV & audio** and select your TV and audio output. Automatic audio works when the TV name matches, or exactly one playback output becomes available when the TV connects.
4. In **Settings**, choose **Steam Big Picture** or **Xbox mode** under **Launch gaming mode**, then click **Save settings**. Open **Home** and choose **Activate TV mode**, or press **Ctrl + Alt + F12**.
5. Press the shortcut again to exit gaming mode and return to your desktop.

The shortcut can be changed. Closing the window keeps SteamCouch running in the
system tray. Right-click its tray icon for settings, restore, or quit. Starting
with Windows is optional and off by default.

The build is unsigned, so Windows may identify it as an unknown publisher.
Download it from this repository's Releases page; a SHA-256 checksum file is
provided alongside the ZIP.

## Features

- Choose your TV and playback device.
- Keep the other monitors on, or use only the TV.
- Choose Steam Big Picture or Windows 11 Xbox mode, with the selected TV made primary before launch.
- Exit the selected gaming mode before restoring desktop displays and audio.
- Restore monitor positions, resolutions, refresh rates, primary display, and the three previous playback defaults.
- Recover a saved desktop setup after restarting SteamCouch.
- Customize the shortcut, Steam location, device wait time, and Windows startup option.
- Dark interface with light-blue accents, native DPI scaling, and a centered Home layout when maximized.

## Requirements and behavior

- Windows 10 or Windows 11, **64-bit**, with .NET Framework 4.8.
- Steam installed and signed in for automatic Big Picture launching.
- A powered-on TV connected to the PC, with its correct HDMI/display input selected.
- An extended-desktop setup. Mirrored/duplicated displays are not supported.

"Reconnect" means enabling a display that Windows has disconnected in Display
Settings. The app cannot reconnect an unplugged cable or reliably power on a TV.

SteamCouch changes playback defaults, not microphone settings or volume levels.
Games with a fixed audio-output preference may need to be set to the Windows
default output. Running games are not force-closed. If a game or Steam dialog
prevents Big Picture from exiting, finish it and retry **Restore desktop**.

Tested on one Windows PC with multiple monitors and an HDMI TV; behavior may
vary by GPU driver, monitor sleep behavior, and Steam configuration.

## Troubleshooting

- **TV is missing:** turn it on, select the PC input, and click **Refresh devices**. Check Windows Display Settings and the cable if needed.
- **Audio selection is ambiguous:** temporarily extend the TV in Windows, refresh devices, and explicitly choose its audio output.
- **Steam uses another screen:** set Steam's preferred display to Automatic/Primary and try again.
- **Shortcut is occupied:** choose another modifier/key combination and save.
- **Restore fails:** reconnect the original monitors/audio devices and retry **Restore desktop**. Keep the `data` folder: it contains the saved restore point.

For troubleshooting, run `SteamCouch.exe --diagnose`. Device details and logs are
written locally under `data`. They may contain device identifiers and local
paths, so review them before attaching them to a public issue.

## Build from source

Clone or download this repository. From Windows PowerShell in its folder:

```powershell
.\build.ps1
.\build\SteamCouch\SteamCouch.exe
```

The build uses the .NET Framework compiler included with Windows. No .NET SDK,
Node.js, Python, or third-party build package is required. The two bundled
NirSoft archives are extracted locally; the build does not download tools.

To build, test, and package a release:

```powershell
.\package.ps1
```

The ZIP and its SHA-256 checksum are written to `dist`. Packaging uses an
explicit file list; personal settings, logs, restore points, and diagnostics
are never included. Automated orchestration tests use fake devices and do not
switch your hardware. Hardware testing is separate and opt-in.

## Local data and updates

Keep the executable, `SteamCouch.exe.config`, `assets`, and `tools` together. Settings and restore points
are stored beside the executable in `data`; the app makes no telemetry requests.
Return to desktop mode and quit SteamCouch before replacing the program files.
Keep your `data` folder to preserve your settings.

## License

SteamCouch's original code is licensed under [MIT](LICENSE). The bundled NirSoft
utilities keep their own freeware terms; see [third-party notices](THIRD-PARTY-NOTICES.md).
SteamCouch is a community utility, not an official Valve product.

The Start with Windows switch applies immediately and launches into the system tray at sign-in. Keep SteamCouch.exe.config beside the executable for native per-monitor DPI scaling.

## Xbox mode
Xbox mode requires a supported Windows 11 installation and the Xbox app. Enable Xbox mode under Windows Settings > Gaming first; confirm Win + F11 enters and exits it. SteamCouch uses that Windows shortcut and verifies the mode through Windows'' gaming-experience API. It checks that the Xbox window is full screen on the selected primary TV. Xbox mode may cover secondary screens even when SteamCouch keeps them connected. SteamCouch does not install feature enablers or change Windows'' Xbox-mode configuration. Win + F11 cannot also be used as SteamCouch''s shortcut for Xbox mode.

## Controller Xbox button
In Settings, enable **Disable Xbox button opening Game Bar** to prevent the controller's Xbox/Guide button opening both Steam's menu and Game Bar. The switch applies immediately to the current Windows user, including outside SteamCouch. Win + G remains available. Turn the switch off to restore the controller shortcut. SteamCouch reads the current Windows setting each time it starts.
