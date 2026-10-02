# SteamCouch 1.9.0 — Monitor choices and optional TV standby

- Choose Leave on, Disconnect, Turn off (DDC/CI), or Black screens for other monitors during TV mode.
- Keep black-covered monitors connected with brightness unchanged; covers follow layout changes and disappear on return. Double-click a black cover to return to desktop.
- Exclude the selected TV from other-monitor actions.
- Save monitor power states before changing them and wake desktop monitors before restoring the saved layout.
- Add an optional Turn off TV when returning to desktop checkbox in Google TV network settings and setup wizard; off by default.
- Send TV standby only after successful desktop restoration. Activation rollback and incomplete restoration leave the TV on; TV-command failures cannot undo desktop recovery.
- Save the preferences in display profiles and document them in the in-app setup guide.

Monitor power control requires compatible DDC/CI hardware/firmware. Some powered-off displays need their physical power button for recovery. TV standby requires network authorization; waking that TV again still depends on its network standby settings.

Validation: core regression suite; all monitor choices; TV exclusion; profile/settings persistence; targeted power recovery and rejected-command rollback; TV standby opt-in/restart/error paths; native black-window placement on connected displays; UI/DPI checks. A read-only check found DDC/CI power status available on three connected non-TV monitor panels. No physical monitor-off or TV standby commands were sent for this update.