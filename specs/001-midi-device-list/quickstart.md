# Quickstart: Validate the MIDI Device List

## Prerequisites

- Windows 11 with Windows MIDI Services available and running.
- .NET SDK required by the project file.
- A MIDI 1 device, a MIDI 2 device, or deterministic test doubles for automated tests.

## Automated validation

From the repository root:

```powershell
dotnet build
dotnet test
```

The tests must cover initial enumeration, MIDI version classification, duplicate names,
empty state, add/remove/reconnect events, rapid event convergence, read failures,
service unavailability, UI-thread publication, and cancellation during shutdown.

## Manual end-to-end validation

1. Start the application with no MIDI endpoints connected and confirm the explicit empty
   state is shown.
2. Connect one MIDI 1 endpoint and one MIDI 2 endpoint. Confirm each appears exactly
   once with its name and corresponding version.
3. Disconnect one endpoint. Confirm it disappears while the other entry remains unchanged.
4. Reconnect the endpoint and confirm it returns as one current entry.
5. Rapidly connect and disconnect an endpoint. Confirm the final list matches the
   currently connected endpoints and contains no duplicates.
6. Stop or make the MIDI service unavailable and confirm the application shows an
   explicit unavailable/degraded status and records a structured diagnostic event.

The watcher and snapshot semantics are defined in
[contracts/device-overview.md](contracts/device-overview.md); entity and lifecycle
rules are in [data-model.md](data-model.md).
