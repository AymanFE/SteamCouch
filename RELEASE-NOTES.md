# SteamCouch 1.10.0 — Performance, controller batteries and Return to game

Three optional gaming tools, available in desktop and TV mode. Each is off by default and can be enabled separately in Settings.

- **Performance display:** levels 0–4, with FPS, a frame-time graph, CPU/GPU usage, supported temperatures and component power, clocks, RAM/VRAM and per-core CPU activity. Choose metrics, screen, corner, size and opacity; cycle levels from the quick menu or a controller shortcut.
- **Controller batteries:** background readings plus separate low-battery popup, sound and vibration options. Read Windows battery reports independently of XInput's wired classification. The GameSir G7 Pro dongle now shows 100%, matching GameSir Nexus. Preserve Xbox charge categories and show unavailable when the driver supplies no valid data.
- **Return to game:** refocus the last observed game or an already-open launcher from the quick menu, tray or configurable controller shortcut.

The installer and portable ZIP include SDL and PresentMon. If Windows denies FPS collection, use Enable FPS capture in Settings to approve the separate helper; SteamCouch stays unelevated. Level 0 pauses capture, and disabling the feature closes the helper.

Use borderless/windowed games for the performance display; exclusive fullscreen can cover it. NVIDIA sensors use the installed driver. CPU temperature/power and unsupported GPU sensors remain unavailable. Component power is not total PC power. Battery availability depends on the device and driver; ambiguous identical-controller matches retain the standard reading.

Validation: regression checks cover frame parsing, stale readings, battery capacities and charging, low-battery alert cooldown, controller matching, process identity, shortcut conflicts and settings persistence. UI scaling and HUD input/focus checks passed. Installer deployment, reinstall and uninstall checks passed. GameSir battery was verified on the connected controller against Nexus. Live elevated gameplay FPS and Return to game across all game titles have not been exhaustively tested.

Download the setup executable to install SteamCouch, or extract the whole ZIP for a portable copy. Existing settings are preserved. The Google TV standby fix from 1.9.2 is retained.
