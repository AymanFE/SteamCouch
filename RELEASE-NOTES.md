# SteamCouch 1.9.2 — TV standby fix

Fix PC freezing and monitor blackouts reported after SteamCouch put a TCL C6K into standby. TV logs showed the old Android Sleep command entered normal standby, while the remote entered Screenless standby. SteamCouch now uses a short remote-style Power press, and the user confirmed the issue was resolved in a full TV-mode-to-desktop test.

- Check the TV power state before sending Power and leave already-asleep TVs unchanged.
- Refuse unknown or changing power states and never retry a power toggle.
- Keep TV standby optional and send it only after successful desktop restoration.
- Update the in-app setup guide.

Download the setup executable to install SteamCouch, or the ZIP for a portable copy. Both include the required TV-control libraries and retain the HDMI input fix from 1.9.1.

Validation: regression tests cover awake/asleep/unknown/changing power states and a lost response after Power. Real TCL C6K logs confirmed the new standby path matches the remote; the full-session hardware test was confirmed successful by the user. Behavior on other TV models still depends on their firmware.
