# Quickstart: MIDI Device Activity Indicators

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

The tests should use fake routing endpoints and an in-memory device provider. They should
cover:

1. A message activates only the row with the matching stable endpoint ID.
2. Repeated messages refresh activity rather than causing premature expiration.
3. Activity returns to inactive after messages stop.
4. Removing a device clears its activity and prevents stale expiration updates.
5. Existing selection, channel, enumeration, and empty-state behavior remains unchanged.
6. The device-list markup or control contract places the dot before the name and prevents
   horizontal overflow.

See [data-model.md](data-model.md) for state and identity rules and
[contracts/device-activity.md](contracts/device-activity.md) for observable UI behavior.

## Manual UI validation

With the application running and at least two MIDI input devices available:

1. Display both devices in the input list and confirm each has an inactive dot before its
   name.
2. Select/activate the devices according to the existing routing workflow.
3. Send a MIDI message from only the first device and confirm only its dot turns green.
4. Stop sending messages and confirm that dot returns to inactive shortly afterward.
5. Send repeated messages and confirm the dot stays responsive without changing row order,
   name, version, selection, or channel.
6. Resize the application and confirm the list follows its outer element without a horizontal
   scrollbar; verify a long device name remains bounded and identifiable.
7. Disconnect an active device and confirm its row and indicator disappear while remaining
   devices continue to display normally.
