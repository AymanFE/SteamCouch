# SteamCouch 1.7.1 — Included CEC software and Windows installer

- Include libCEC 8.1.7, its native library and the required Microsoft C++ runtime in both installer and portable packages. No separate libCEC/runtime download is needed.
- Select bundled CEC software automatically and preserve that selection when the portable folder moves.
- Include the official USB-CEC driver installer; optional setup is available from the app and installer.
- Add a Windows setup executable with per-user installation, Start menu shortcut and uninstaller. Settings survive upgrades/uninstall; running/active TV sessions block replacement.
- Include dependency notices, checksums and matching native libCEC source.

A compatible physical CEC adapter/interface and enabled TV CEC remain necessary. Adapter drivers may require Windows permission. Firmware flashing and unused .NET 8/Python/Node bindings are not included.

Validation: bundled native libCEC loads and reports 8.1.7; all core/CEC tests; settings and profile persistence; UI/DPI checks; isolated installer install/upgrade/uninstall checks. Physical TV wake/input switching is still unverified without a CEC adapter.
