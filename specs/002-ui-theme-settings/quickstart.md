# Quickstart: Validate UI Theme Settings

## Prerequisites

- Windows 10/11 and the .NET SDK required by the project file.
- No physical MIDI hardware is required for automated validation.

## Automated validation

From the repository root:

```powershell
dotnet build
dotnet test
```

Tests should cover the mode value contract, missing/malformed settings, successful
read/write, write failures, explicit palette selection, OS-default resolution, OS preference
changes, and preservation of unrelated MIDI view-model behavior.

## Manual end-to-end validation

1. Start the application and confirm the existing MIDI device view appears with the gear
   icon in the upper-left corner.
2. Open the gear menu and confirm it contains only Light, Dark, and OS default, with one
   current selection.
3. Select Light and Dark and confirm the complete visible UI changes immediately while the
   device list and status remain functional.
4. Select OS default, change the Windows appearance preference, and confirm the application
   follows the new palette.
5. Restart the application and confirm the last selected mode is restored.
6. Temporarily remove or invalidate the settings file and confirm the application remains
   usable in OS default mode and surfaces the settings problem through diagnostics/status.

The menu and persistence contract is defined in
[contracts/ui-theme-settings.md](contracts/ui-theme-settings.md); entities and transitions
are defined in [data-model.md](data-model.md).
