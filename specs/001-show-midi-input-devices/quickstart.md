# Quickstart: Input MIDI Device Browser UI

## Prerequisites

- Windows machine
- .NET 8 SDK installed
- Windows MIDI Services available on the machine (Windows Runtime MIDI discovery)
- Optional: one or more input MIDI devices (physical or virtual)

## Setup

From repository root:

```powershell
dotnet restore
dotnet build
dotnet test
```

## Run

From repository root:

```powershell
dotnet run --project .\midi-router.csproj
```

## Validation Scenarios

1. **Initial load with devices**
   - Start the app with at least one input MIDI device available.
   - Expected: a list is shown with each available input device name.

2. **Empty state**
   - Start the app with no input MIDI devices available.
   - Expected: explicit empty state status message and zero listed devices.

3. **Manual refresh**
   - Start app, then connect or disconnect an input MIDI device.
   - Click **Aktualisieren**.
   - Expected: list updates to current availability and status text reflects the new count/state.

4. **Failure behavior**
   - Simulate provider discovery failure in tests or controlled runtime conditions.
   - Expected: device list clears, error status is displayed, and user can retry via refresh.

## Cross-References

- Data model and state transitions: [`data-model.md`](./data-model.md)
- MIDI backend contract and constraints: [`contracts/midi-actions-contract.md`](./contracts/midi-actions-contract.md)
- Feature requirements and measurable outcomes: [`spec.md`](./spec.md)
