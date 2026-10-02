# SteamCouch 1.6.1 — Display scaling and reliability fixes

- Fix oversized Steam Big Picture on mixed-DPI setups using fresh native monitor bounds and physical-pixel window coordinates, verified after placement and allowed to settle during startup.
- Apply the same placement correction to Playnite and refresh quick-menu geometry after display changes.
- Wait for quick-menu initialization before applying game profiles or displaying sleep confirmation.
- Isolate concurrent display/audio inventory files.
- Recheck TV-session state after background detection and preserve automatic-return tracking when unrelated settings are saved.
- Block the controller mute shortcut behind sleep/display confirmations.
- Honor tray/settings restart preferences after updates and rollbacks.

Validation: real Steam placement on three connected monitors; physical window placement; six concurrent device inventories; full self-tests; UI/menu/DPI checks; isolated updater restart and settings preservation. Complete TV-only hardware transitions and live per-game HDR changes were not repeated for this patch.
