# SteamCouch 1.9.1 — HDMI input switching fix

Changing the Google TV HDMI setting now selects the requested input instead of being overridden by an old saved TCL hardware input command.

- Discover HDMI ports from the TV after wake-up, including TCL Google TVs.
- Apply the selected port in settings, the setup wizard and display profiles.
- Migrate old hardware input commands to automatic selection and update the in-app setup guide.

Download the setup executable to install SteamCouch, or the ZIP for a portable copy. Both include the required TV-control libraries.

Validation: regression suite and settings/UI/DPI checks passed. All four HDMI mappings were verified against a TCL C6K; this verification did not switch the TV's input.
