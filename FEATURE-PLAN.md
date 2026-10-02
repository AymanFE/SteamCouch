# Optional feature controls — implemented in 1.6.0

Each addition is individually controlled in Settings. Preserve existing choices during upgrades. Do not enable new automatic behavior just because the app was updated.

| Addition | Control and default | Behavior when disabled |
| --- | --- | --- |
| Controller shortcut editor | Each action has its own enable switch. New actions are off by default. Keep the existing quick-menu preference. | Do not poll or trigger the disabled action. Reject duplicate enabled bindings. |
| Controller tester | Open explicitly using a Test controller button. | No tester window or automatic launch. Testing must suppress all shortcuts. |
| Automatic desktop return | Separate switch, off by default. | Launcher exit does not change displays or audio. Manual restoration still works. |
| Sleep from the quick menu | Separate switch, off by default. | Hide the Sleep action and disable any associated controller binding. When enabled, require confirmation and successful desktop restoration before sleeping. |
| Per-game profiles | Master switch plus an enabled switch for each profile, all off by default. | Do not watch games or change HDR, resolution, or refresh rate. Profiles remain saved. |
| Playnite launcher | Optional launcher selection; retain the current launcher during upgrades. | Do not start, close, or monitor Playnite unless it is selected. |

Controller navigation and TV/desktop toggling must remain available when automatic return and per-game profiles are disabled. Keep an ordinary-button shortcut fallback for drivers that cannot expose the Xbox/Guide button.

After Save settings, disabling an option stops its background behavior. If a per-game adjustment is currently active, restore its prior session settings safely. Do not delete saved preferences when an option is switched off.

Use SteamCouch's existing charcoal and light-blue design. Explain options in ordinary language and provide a preview/test wherever it avoids unnecessary display changes.
