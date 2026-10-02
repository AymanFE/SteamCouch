# SteamCouch 1.7.0 — Optional HDMI-CEC TV control

- Optionally wake a compatible TV and request the PC's HDMI input before activating TV mode.
- Configure the installed libCEC program, HDMI input and wake delay under TV & audio.
- Test power/input commands without changing the Windows display layout.
- Save TV-control settings with each display profile.
- Missing software, adapter failures and timeouts stop activation before desktop displays/audio change.

Off by default. Requires enabled TV CEC, separately installed libCEC and a compatible PC-side CEC interface, commonly a USB adapter. Ordinary GPU HDMI output usually lacks CEC. Network control is not included. TV responses and receiver routing vary; desktop return leaves the TV on. libCEC is not bundled.

Validation: protocol ordering/error tests, real subprocess streams/exit-code handling/timeout cleanup with a test fixture, activation integration, settings/profile persistence and UI/DPI checks. Physical TV wake/input switching was not tested because no compatible CEC setup was available.
