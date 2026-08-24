# Quickstart: Select MIDI Devices

## Prerequisites

- Windows with the .NET 10 SDK installed.
- Repository checked out at the feature branch.
- No physical MIDI hardware is required for automated validation.

## Automated validation

Run from the repository root:

```powershell
dotnet build
dotnet test
```

The tests should use a fake `IMidiInputDeviceProvider` and an in-memory `ISettingsStore`.
They should cover:

1. Clicking an unselected row selects its unique device ID and highlights the row.
2. Clicking the same row again removes only that ID and its highlight.
3. Multiple distinct IDs remain selected simultaneously, including devices sharing a name.
4. A saved ID restores after view-model recreation when the device is available.
5. A disconnected selected ID is hidden while other rows retain their selection.
6. Reconnecting the same unique ID restores selection.
7. Persistence failure leaves the list usable and reports the failure.

See [data-model.md](data-model.md) for identity and lifecycle rules and
[contracts/device-selection.md](contracts/device-selection.md) for observable behavior.

## Manual UI validation

With the application running and at least two MIDI input devices available:

1. Click one device row and confirm it is highlighted.
2. Click a second row and confirm both rows remain highlighted.
3. Click the first row again and confirm only its highlight is removed.
4. Restart the application and confirm the selected device IDs are highlighted again.
5. Disconnect and reconnect a selected device and confirm its selection returns when the
   same unique device ID becomes available.
