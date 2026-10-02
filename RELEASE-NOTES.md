# SteamCouch 1.6.0 — Optional couch controls

All five additions are optional. Existing preferences stay intact, and new automatic behavior stays off until enabled and saved.

- Configure three independently enabled controller actions using Select + A/B/X/Y, with overlapping bindings rejected. Test connected controller buttons, triggers and sticks without activating shortcuts.
- Opt into automatic desktop return when Steam Big Picture or Playnite fullscreen closes. An eight-second grace period and launcher-child app detection reduce accidental restoration during games.
- Opt into a Sleep tile and shortcut. Confirm before sleeping; desktop restoration must complete successfully first. Games are not force-closed.
- Opt into per-game HDR, resolution and refresh-rate profiles. Enable each game separately, select its actual executable and confirm display changes. Previous TV-session settings restore after game exit or disabling the feature.
- Choose Playnite fullscreen as a third launcher. Playnite uses its documented fullscreen/desktop commands and is not installed automatically.

Xbox mode still uses manual desktop return. Launcher-child detection is conservative; background apps can require manual restoration. Protected game executables may not allow profile detection. The quick menu works best with borderless/windowed games.

Validation: opt-in defaults; shortcut conflict and sleep-dependency tests; automatic-return grace and active-game guards; full-path game matching; simulated Playnite device ordering/restoration; isolated video rollback/recovery and VRR preservation tests; optional settings persistence; menu/profile/tester previews; main-app DPI checks from 100–300%. PC sleep, live per-game display changes, and a real Playnite installation have not been exercised during verification.
