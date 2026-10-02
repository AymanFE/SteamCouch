# Changelog

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
