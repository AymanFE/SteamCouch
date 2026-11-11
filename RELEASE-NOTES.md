# SteamCouch 1.11.0: Steam Dual Launcher support

If [Steam Dual Launcher](https://github.com/AymanFE/SteamDualLauncher) is installed, SteamCouch detects it automatically. A new gaming mode, **Steam Dual Launcher (Clean / Modded)**, then appears in Settings and in the setup wizard.

In TV mode, SteamCouch switches the TV and audio first, then opens the controller picker on the TV. Choose Clean or Modded Steam, and Big Picture is placed on the TV as usual. **Restore desktop** exits Big Picture for whichever Steam installation is running. If you close the picker without choosing, TV mode is cancelled and your desktop is restored.

Existing preferences are preserved. The new mode is used only when you select it.

Validation: the regression suite passed, including new Dual Launcher tests for device ordering, the restore point, rejection when the launcher is missing, and rollback when no installation is chosen.
