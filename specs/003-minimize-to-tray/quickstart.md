# Quickstart: Minimize to Tray

## Prerequisites

- Windows 10 or Windows 11
- .NET 10 SDK
- Repository checkout with the existing Windows MIDI dependencies restored
- No physical MIDI hardware required for automated validation

## Automated validation

From the repository root:

```powershell
dotnet build
dotnet test
```

The tests should verify settings compatibility and persistence, disabled defaults, diagnostic
handling, setting-menu discoverability, minimize policy, single-click restoration, tray stop
shutdown, and preservation of MIDI lifecycle behavior. See [data-model.md](data-model.md) and
[contracts/tray-interaction.md](contracts/tray-interaction.md) for the persisted fields and
observable interaction contract.

## Manual end-to-end validation

1. Start the application and open the existing settings menu.
2. Enable **Minimize to tray**, then minimize the main window using its minimize button.
3. Confirm the window is absent from the taskbar, the tray icon is visible, and MIDI device
   monitoring continues.
4. Left-click the tray icon once and confirm the same main window is restored and active.
5. Minimize again, right-click the tray icon, and choose the stop action. Confirm the application
   exits and the tray icon disappears.
6. Start the application again, disable **Minimize to tray**, and minimize the window. Confirm
   that it appears as a normal minimized taskbar window.
7. Restart once more and confirm the selected setting remains disabled. Repeat with the setting
   enabled to verify both persisted states.

## Failure-path validation

Use the existing test seam for a settings store that fails to load or save. Confirm the
application remains usable, uses the disabled fallback on load failure, retains the current
session choice on save failure, and reports each failure through the established diagnostic
mechanism.
