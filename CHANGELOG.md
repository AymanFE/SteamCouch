# Changelog

## 1.10.1

- Add a clear Enable / Disable performance display action to the tray menu.
- Disabling immediately closes the HUD and capture helper, saves the preference and updates the Settings switch. Re-enabling restores the selected level.
- Verify both tray actions persist their state and document the Settings and level-zero alternatives.

## 1.10.0

- Add an optional performance display with levels 0–4, frame-time graph, CPU/GPU usage, temperature, component power, GPU clocks, RAM/VRAM and per-core CPU activity where available.
- Choose display, corner, size, opacity and individual metrics. Cycle levels in the TV quick menu or with a configurable controller shortcut, including at your desk.
- Bundle PresentMon 2.6.0 and SDL 3.4.18 in installer and portable builds. FPS capture uses a separate permission helper when Windows requires it; turning the display off pauses collection, and the helper exits with SteamCouch.
- Add optional background controller battery monitoring, quick-menu/tray readings and separate low-battery popup, sound and vibration settings. Show actual percentages, qualitative Xbox charge levels, or unavailable readings without guesses.
- Filter a controller dongle's extra keyboard, media and vendor HID interfaces instead of listing them as controllers.
- Add optional Return to game through the quick menu, tray or configurable controller shortcut. Validate window/process identity and fall back to an open launcher when the game has closed.

Read Windows controller battery reports independently of XInput: the GameSir G7 Pro dongle now reports 100%, confirmed against GameSir Nexus. Missing/invalid capacities and ambiguous identical-controller matches retain the standard reading. Hardware limits: low-battery alerts require a real reading from the driver. Current GPU sensor coverage uses NVIDIA's installed NVML library; unsupported sensors, including CPU temperature and CPU watts, remain explicitly unavailable. The overlay is designed for windowed/borderless games and does not inject into games.

## 1.9.2

- Use a guarded short remote-style Power press for Google TV standby instead of Android Sleep, which enters a different standby path on TCL C6K.
- Leave already-asleep TVs unchanged; refuse unknown or changing power states and never retry a power toggle.

Validation: regression tests cover awake/asleep/unknown/changing states and a lost response after Power. TCL logs confirm remote Power enters Screenless standby while Sleep enters normal standby. A full TV-mode-to-desktop test reached Screenless standby and the user confirmed the PC freezing/monitor blackout issue was resolved.

## 1.9.1

- Fix Google TV HDMI selection being overridden by an old saved TCL hardware input URI.
- Discover HDMI inputs from the TV after wake-up and use the selected port, including in the setup wizard and display profiles.
- Migrate old hardware input URIs to automatic selection in settings; keep explicit custom input overrides and explain them in the setup guide.

Validation: regression suite, settings migration and UI/DPI checks; all four HDMI mappings verified against a TCL C6K without switching its input.

## 1.9.0

- Choose Leave on, Disconnect, Turn off (DDC/CI), or Black screens for other monitors during TV mode.
- Keep black-covered monitors connected with brightness unchanged; covers follow layout changes and disappear on return. Double-click a black cover to return to desktop.
- Exclude the selected TV from other-monitor actions.
- Save monitor power states before changing them and wake desktop monitors before restoring the saved layout.
- Add an optional Turn off TV when returning to desktop checkbox in Google TV network settings and setup wizard; off by default.
- Send TV standby only after successful desktop restoration. Activation rollback and incomplete restoration leave the TV on; TV-command failures cannot undo desktop recovery.
- Save the preferences in display profiles and document them in the in-app setup guide.

Monitor power control requires compatible DDC/CI hardware/firmware. Some powered-off displays need their physical power button for recovery. TV standby requires network authorization; waking that TV again still depends on its network standby settings.

Validation: core regression suite; all monitor choices; TV exclusion; profile/settings persistence; targeted power recovery and rejected-command rollback; TV standby opt-in/restart/error paths; native black-window placement on connected displays; UI/DPI checks. A read-only check found DDC/CI power status available on three connected non-TV monitor panels. No physical monitor-off or TV standby commands were sent for this update.


## 1.8.1

- Release SteamCouch's private TV connection helper before applying updates, preventing locked dependency files.

- Add a scrollable TV setup guide in TV & audio settings and the setup wizard, including debugging, PC approval, Wireless debugging pairing, HDMI selection and standby troubleshooting.


## 1.8.0

- Add optional Google TV / Android TV network wake and HDMI selection, with an editable TV address and connection port.
- Add an optional TV-control step to the first-run setup wizard, with connection approval and Wireless debugging code pairing.
- Include ADB 37.0.1, native dependencies and upstream notices in both installer and portable versions.
- Support optional Wake-on-LAN, wake delay, and a model-specific Android input URI.
- Preserve network preferences in display profiles. Connection failures stop activation before Windows display/audio changes.
- Keep CEC available as an alternative; both methods are off by default.

Requires TV debugging approval and an available network connection. Standby wake and HDMI input support depend on TV settings/firmware. Returning to desktop leaves the TV on.

Validation: bundled ADB native smoke test; input validation; wake ordering; unauthorized/error handling; simulated activation failure safety; profile/settings persistence; UI/DPI checks; installer install/reinstall/uninstall checks. Physical testing: the authorized TCL connection and its model-specific HDMI 1 route passed through the installed app. Standby wake failed with the current TV power settings; network standby configuration still needs verification.


## 1.7.1

- Include libCEC 8.1.7, its native library and the required Microsoft C++ runtime in both installer and portable packages. No separate libCEC/runtime download is needed.
- Select bundled CEC software automatically and preserve that selection when the portable folder moves.
- Include the official USB-CEC driver installer; optional setup is available from the app and installer.
- Add a Windows setup executable with per-user installation, Start menu shortcut and uninstaller. Settings survive upgrades/uninstall; running/active TV sessions block replacement.
- Include dependency notices, checksums and matching native libCEC source.

A compatible physical CEC adapter/interface and enabled TV CEC remain necessary. Adapter drivers may require Windows permission. Firmware flashing and unused .NET 8/Python/Node bindings are not included.

Validation: bundled native libCEC loads and reports 8.1.7; all core/CEC tests; settings and profile persistence; UI/DPI checks; isolated installer install/upgrade/uninstall checks. Physical TV wake/input switching is still unverified without a CEC adapter.

## 1.7.0

- Optionally wake a compatible TV and request the PC's HDMI input before activating TV mode.
- Configure the installed libCEC program, HDMI input and wake delay under TV & audio.
- Test power/input commands without changing the Windows display layout.
- Save TV-control settings with each display profile.
- Missing software, adapter failures and timeouts stop activation before desktop displays/audio change.

Off by default. Requires enabled TV CEC, separately installed libCEC and a compatible PC-side CEC interface, commonly a USB adapter. Ordinary GPU HDMI output usually lacks CEC. Network control is not included. TV responses and receiver routing vary; desktop return leaves the TV on. libCEC is not bundled.

Validation: protocol ordering/error tests, real subprocess streams/exit-code handling/timeout cleanup with a test fixture, activation integration, settings/profile persistence and UI/DPI checks. Physical TV wake/input switching was not tested because no compatible CEC setup was available.

## 1.6.1

- Fix oversized Steam Big Picture on mixed-DPI setups using fresh native monitor bounds and physical-pixel window coordinates, verified after placement and allowed to settle during startup.
- Apply the same placement correction to Playnite and refresh quick-menu geometry after display changes.
- Wait for quick-menu initialization before applying game profiles or displaying sleep confirmation.
- Isolate concurrent display/audio inventory files.
- Recheck TV-session state after background detection and preserve automatic-return tracking when unrelated settings are saved.
- Block the controller mute shortcut behind sleep/display confirmations.
- Honor tray/settings restart preferences after updates and rollbacks.

Validation: real Steam placement on three connected monitors; physical window placement; six concurrent device inventories; full self-tests; UI/menu/DPI checks; isolated updater restart and settings preservation. Complete TV-only hardware transitions and live per-game HDR changes were not repeated for this patch.

## 1.6.0

- Added individually enabled controller action slots and a live controller tester that suppresses shortcuts while testing.
- Added optional automatic desktop return for Steam Big Picture and Playnite with an eight-second grace period and conservative launcher-child app detection.
- Added an optional Sleep tile and controller action, requiring confirmation and successful desktop restoration before requesting Windows sleep.
- Added an opt-in per-game profile manager for HDR, resolution and refresh rate, with individual game switches, exact executable matching, safe confirmation and session restoration.
- Added Playnite fullscreen as an optional launcher in Settings and setup, including desktop-mode restoration.
- Preserved existing settings and kept all new automatic behavior off by default.

## 1.5.0

- Added a controller-operated quick menu on the session TV for volume, mute, audio output, HDR, resolution, refresh rate, Windows VRR assistance and desktop return.
- Added View/Select + Y/A/B/X menu bindings and an optional View/Select + Xbox TV/desktop toggle on compatible drivers.
- Added 20-second display-change confirmation with a background rollback watchdog and persistent interrupted-change recovery.
- Added TV HDR and Windows VRR preferences to settings and display profiles, restoring previous preferences when returning to desktop.
- Kept the light-blue theme and rendered the menu at TV-relative sizes with crisp text.

## 1.4.0 — Couch controls and easier setup

- Add a configurable held controller combination to enter TV mode and restore desktop, with release-to-rearm behavior and switching/dialog suppression.
- Add optional temporary system/display awake requests during TV mode only.
- Add display/audio/launcher profiles while preserving global preferences.
- Add a four-step first-run setup wizard, Steam-path selection, and optional TV/desktop round-trip test.
- Add troubleshooting checks with a reviewable report and local export.
- Add daily/manual GitHub update checks, release notes access, checksum-validated downloads, and optional idle installation with backups and rollback.
- Preserve centered Home and compact/mixed-DPI layouts across all five pages.

## 1.3.1 — Controller shortcut setting

- Add one Settings switch for Game Bar and Controller Bar button shortcuts and Controller Bar opening on connection.
- Apply the change immediately for the current Windows user, with Win + G still available.
- Reflect Windows and Game Bar app-data preferences at startup, notify running components, and roll back failed changes.
- Keep compact Settings accessible with vertical scrolling across mixed-DPI monitors.

## 1.3.0 — Xbox-mode support

- Fix dropdown menus crashing when dismissed by a click outside the menu.

- Choose Steam Big Picture or Xbox mode in Settings.
- Verify Xbox-mode entry and the selected primary display.
- Persist the chosen launcher for correct recovery after app restart.
- Restore displays and audio even if exiting Xbox mode fails, retaining a retry point.


## 1.2.0 — First public release

- Configurable TV/display and playback audio switching.
- Steam Big Picture launch on the TV and automatic exit on return to desktop.
- Optional TV-only mode, with native Windows display snapshots for restoration.
- Persistent recovery, customizable global shortcut, and optional Windows startup.
- Charcoal sidebar interface with Home, TV & audio, and Settings pages, light-blue accents, and the SteamCouch sofa icon.
- Portable Windows x64 package with included dependencies and checksums.

- Native per-monitor DPI rendering and stable layouts across mixed-scale displays.
- Windows startup applies immediately, reports actual registration, and migrates the old app name.
