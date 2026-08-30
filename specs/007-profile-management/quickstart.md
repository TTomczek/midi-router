# Profile Management Validation Guide

## Prerequisites

- Windows 10 build 19041 or newer
- .NET 10 SDK
- Repository checked out with dependencies restored
- No physical MIDI hardware required for automated validation

## Automated validation

From the repository root:

```powershell
dotnet test
```

The profile unit and store tests should verify name trimming, duplicate labels, per-file round trips, last-edited updates, error preservation, and the cannot-delete-final-profile rule. Hardware-isolated view-model tests should verify that switching applies each profile's device and channel state.

## Manual end-to-end scenarios

1. Start the application and confirm the profile drop-down appears immediately left of Settings with at least one profile.
2. Select the first entry, enter `Alice`, and press Enter. Confirm a new empty profile is created and selected.
3. Configure a device and channel, switch to another profile, and confirm the active device/channel state changes; switch back and confirm it is restored.
4. Create another profile named `Alice`. Confirm the duplicate names receive sequential visible labels while the stored names remain `Alice`.
5. Double-click a profile, rename it in the styled dialog, press Enter or confirm the dialog, restart the application, and confirm the rename and assignments persist.
6. Right-click a profile, choose Delete, cancel the confirmation, and confirm the profile remains. Repeat and accept the dialog to delete it.
7. With one profile left, confirm its minus action is absent or disabled and deletion cannot proceed.

See [data-model.md](data-model.md) for persistence and lifecycle rules and [contracts/profile-ui.md](contracts/profile-ui.md) for the observable UI contract.
