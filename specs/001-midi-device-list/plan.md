# Implementation Plan: MIDI Device List

**Branch**: `001-midi-device-list` | **Date**: 2026-08-23 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `specs/001-midi-device-list/spec.md`

## Summary

Replace the current one-time device refresh with a continuously maintained list backed by
the Windows MIDI Services `Windows.Devices.Midi2` endpoint watcher. Use the SDK's
`AllStandardEndpoints` filter to include native MIDI 2 UMP endpoints and native MIDI 1
byte-stream endpoints, classify each endpoint from its native format, and project
watcher changes into the WPF UI through a serialized update pipeline. Add structured
logging, explicit service/enumeration error states, cancellation-aware shutdown, and
hardware-independent tests.

## Technical Context

**Language/Version**: C# on .NET 10 with the repository's
`net10.0-windows10.0.22621` target.

**Primary Dependencies**: `Windows.Devices.Midi2` `0.99.33-devpreview.3`,
`Microsoft.Windows.CsWinRT` 2.2.0, WPF, Windows Forms tray integration, and the
repository's existing xUnit test stack. Use `Microsoft.Extensions.Logging` abstractions
with a desktop-appropriate provider if the implementation adds logging packages.

**Storage**: None. The endpoint watcher is the source of record; the UI maintains only
the current in-memory projection.

**Testing**: xUnit with `Microsoft.NET.Test.Sdk`; provider and coordinator contracts use
deterministic fakes and do not require physical MIDI hardware.

**Target Platform**: Windows 11 with Windows MIDI Services available; .NET 10 and the
supported Windows SDK/runtime prerequisites remain those declared by the project file.

**Project Type**: WPF desktop application with a background device-monitoring service.

**Performance Goals**: Initial device list is publishable immediately after watcher
enumeration completes; 95% of add/remove events update the visible list within 2 seconds.
Event callbacks must return quickly and must not perform blocking work.

**Constraints**: No physical MIDI hardware in automated tests; no UI-thread access from
SDK watcher callbacks; no unbounded event queue; updates must be serialized, coalesced
where safe, and cancelled during shutdown. A single endpoint identity must produce at
most one visible item.

**Scale/Scope**: The current connected endpoint set, validated with at least 20 devices.
This feature covers endpoint listing and connectivity changes only; message routing,
endpoint connections, device actions, persistence, and arbitrary property changes are
out of scope.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Plan evidence | Status |
|---|---|---|
| Modular Architecture | SDK watcher, device projection/coordinator, and WPF presentation are separate boundaries. | PASS |
| Background-Process Reliability | Serialized updates, cancellation, bounded/coalesced work, explicit lifecycle and error states. | PASS |
| Test-Driven Development | Tests precede implementation for enumeration, classification, updates, errors, and shutdown. | PASS |
| Hardware-Isolated Integration Testing | SDK access is behind a provider/watcher contract with deterministic fakes. | PASS |
| Simplicity and Extensibility | One standard SDK watcher and one projection avoid duplicate enumeration paths. | PASS |
| Platform and Runtime Constraints | Uses the existing Windows MIDI2 SDK integration, Windows target, and .NET 10. | PASS |
| Development Workflow and Quality Gates | `dotnet build` and `dotnet test` remain required gates. | PASS |

No constitution violations require complexity justification.

## Project Structure

### Documentation (this feature)

```text
specs/001-midi-device-list/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── device-overview.md
└── tasks.md                 # Created by /speckit-tasks
```

### Source Code (repository root)

```text
MainWindow.xaml(.cs)                         # WPF device overview binding/lifecycle
MidiInputDevice.cs                           # Existing/expanded UI-facing device model
MidiInputDeviceViewModel.cs                  # Existing/expanded observable projection
WindowsMidiInputDeviceProvider.cs             # Windows.Devices.Midi2 adapter
MidiDeviceMonitor.cs                          # Watcher event and serialized update coordinator
MidiDeviceLogging.cs                          # Logging categories/event helpers if needed
midi-router.Tests/
├── MidiInputDeviceViewModelTests.cs
├── MidiDeviceMonitorTests.cs
└── WindowsMidiInputDeviceProviderTests.cs
```

**Structure Decision**: Preserve the existing single WPF project and test project. Add
focused root-level domain/integration classes because the repository currently has no
source directories; introduce directories only if implementation volume makes ownership
clearer.

## Complexity Tracking

No violations.
