# Quickstart: Validate MIDI Message Routing

## Prerequisites

- Windows with Windows MIDI Services available.
- .NET 10 SDK.
- Normal repository dependencies restored.
- Automated tests use fakes and do not require physical MIDI hardware.

## Automated validation

From the repository root:

```powershell
dotnet build
dotnet test
```

Tests should prove the contracts in [midi-routing.md](contracts/midi-routing.md):

1. Selected input reaches the fake virtual endpoint with its assigned channel; unselected
   input does not.
2. Automatic allocation chooses internal channels 0, 1, and so on; display values are 1, 2.
3. Invalid and duplicate assignments are rejected without changing existing assignments.
4. A virtual response maps by assigned channel to exactly one device and restores its
   original channel.
5. Channel-less messages retain their data.
6. A failing route reports an error while another route continues.
7. Settings save, reload, and restore assignments by endpoint ID.

## Manual Windows validation

1. Start the application with Windows MIDI Services available.
2. Select two connected MIDI input devices and confirm distinct displayed channels.
3. Send messages from both devices and observe them in a MIDI monitor or DAW connected to
   the application's virtual MIDI endpoint.
4. Confirm configured channels and unchanged message data.
5. Send a response on one assigned channel and verify only that physical device receives it
   on its original channel.
6. Change an assignment, restart, and confirm the value is restored.
7. Deselect or disconnect one device and confirm the other route continues.
