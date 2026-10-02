# SteamCouch 1.8.1 — Google TV network control

- Include an in-app TV setup tutorial, accessible from TV & audio and the wizard.
- Add optional Google TV / Android TV network wake and HDMI selection, with an editable TV address and connection port.
- Add an optional TV-control step to the first-run setup wizard, with connection approval and Wireless debugging code pairing.
- Include ADB 37.0.1, native dependencies and upstream notices in both installer and portable versions.
- Support optional Wake-on-LAN, wake delay, and a model-specific Android input URI.
- Preserve network preferences in display profiles. Connection failures stop activation before Windows display/audio changes.
- Keep CEC available as an alternative; both methods are off by default.

Requires TV debugging approval and an available network connection. Standby wake and HDMI input support depend on TV settings/firmware. Returning to desktop leaves the TV on.

Validation: bundled ADB native smoke test; input validation; wake ordering; unauthorized/error handling; simulated activation failure safety; profile/settings persistence; UI/DPI checks; installer install/reinstall/uninstall checks. Physical testing: the authorized TCL connection and its model-specific HDMI 1 route passed through the installed app. Standby wake failed with the current TV power settings; network standby configuration still needs verification.