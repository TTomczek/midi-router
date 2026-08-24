# Implementation Plan: Select MIDI Devices

**Branch**: `004-select-midi-devices` | **Date**: 2026-08-24 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `/specs/004-select-midi-devices/spec.md`

## Summary

Add multi-selection to the existing MIDI device overview. Each selection is keyed by the
device's unique `EndpointDeviceId`, persisted with the existing application settings, and
exposed independently of row styling so later processing can consume the selected devices.
The existing device monitor remains responsible for discovery and refresh; the view model
owns selection state and applies it when snapshots change.

## Technical Context

**Language/Version**: C# with .NET 10

**Primary Dependencies**: WPF, Windows MIDI Services, `System.Text.Json`, existing
`ISettingsStore`/`JsonSettingsStore` abstractions

**Storage**: Existing per-user JSON settings file managed by `JsonSettingsStore`

**Testing**: xUnit tests in `midi-router.Tests`, using fake device providers and in-memory
settings stores; no physical MIDI hardware

**Target Platform**: Windows 11 with Windows MIDI Services

**Project Type**: Desktop WPF application

**Performance Goals**: A row click updates selection and its highlight within 1 second;
device refresh remains responsive for at least 20 listed devices

**Constraints**: Preserve asynchronous device monitoring, keep selection independent from
display names, remain usable when settings persistence fails, and avoid changing unrelated
theme, tray, or routing behavior

**Scale/Scope**: Existing device overview, typically tens of MIDI input devices; selection
and persistence only, with later processing consumption explicitly out of scope

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- **Modular Architecture**: PASS. Discovery stays in `MidiDeviceMonitor`; selection and
  persistence are separated behind view-model/settings boundaries; UI only binds state.
- **Background-Process Reliability**: PASS. Device refresh remains asynchronous, selected
  state is reconciled on snapshots, and persistence failures use the existing diagnostic
  callback rather than stopping monitoring.
- **Test-Driven Development**: PASS. The plan requires tests for toggle behavior, unique-ID
  identity, persistence, refresh/disconnect handling, and failure behavior before implementation.
- **Hardware-Isolated Integration Testing**: PASS. Tests use the existing
  `IMidiInputDeviceProvider` fake pattern and in-memory settings store.
- **Simplicity and Extensibility**: PASS. The design reuses the existing device ID,
  settings store, monitor, and WPF binding model without introducing a new framework.
- **Platform and Quality Gates**: PASS. The feature targets the existing Windows/.NET 10
  application and is validated with `dotnet build` and `dotnet test`.

## Project Structure

### Documentation (this feature)

```text
specs/004-select-midi-devices/
├── plan.md
├── research.md
├── data-model.md
├── contracts/
│   └── device-selection.md
├── quickstart.md
└── tasks.md                 # Created by /speckit-tasks
```

### Source Code (repository root)

```text
MidiInputDevice.cs             # Existing unique device ID model
MidiDeviceMonitor.cs           # Existing discovery and snapshot lifecycle
MidiInputDeviceViewModel.cs    # Selection state, persistence, and snapshot reconciliation
ApplicationSettings.cs         # Persisted selection setting
ApplicationSettingsCoordinator.cs # Shared settings ownership and updates
JsonSettingsStore.cs           # Existing JSON settings persistence
MainWindow.xaml                # Row selection and selected-row highlighting
MainWindow.xaml.cs             # Existing window wiring and lifecycle
midi-router.Tests/
├── MidiInputDeviceTests.cs
├── MidiDeviceMonitorTests.cs
├── MidiInputDeviceViewModelTests.cs
└── DeviceSelectionPersistenceTests.cs
```

**Structure Decision**: Extend the existing single-project WPF desktop application at the
repository root. Keep device discovery unchanged, add selection behavior to the existing
device view model, bind the list's selected state to the view model, and extend the existing
settings record/store rather than creating a parallel persistence system.

## Phase 0: Research

Research confirmed that:

- `MidiInputDevice.EndpointDeviceId` is already the unique device identifier and is used as
  the provider dictionary key and deterministic ordering tie-breaker.
- `ISettingsStore` and `JsonSettingsStore` are the established persistence boundary and
  already preserve unrelated appearance and tray settings in one JSON document.
- `MidiInputDeviceViewModel` receives complete immutable device snapshots and marshals them
  to the WPF dispatcher, making it the appropriate place to reconcile selection state.
- The current `ListView` has no selection interaction configured, so row highlighting can be
  added without changing the monitor/provider contract.

No unresolved technical clarifications remain.

## Phase 1: Design

The selected ID set is loaded once when the device view model starts, toggled on row
activation, and saved after every change. Snapshot application retains IDs for disconnected
devices for reconnect restoration, while only currently listed devices receive visible
selection state. The UI binds each row's selected state to the ID-backed selection model;
later processing consumes the selected ID set through the view-model contract.

The design artifacts are:

- [data-model.md](data-model.md): device, selection, persisted settings, and snapshot rules.
- [contracts/device-selection.md](contracts/device-selection.md): UI/view-model behavior
  and future processing consumption contract.
- [quickstart.md](quickstart.md): hardware-free automated and manual validation scenarios.

## Constitution Check (Post-Design)

- **Modularity**: PASS; provider/monitor discovery remains unchanged and selection is
  isolated to model, settings, view model, and UI binding.
- **Reliability**: PASS; refreshes do not clear saved IDs and persistence errors are surfaced
  without preventing the list from updating.
- **Testing**: PASS; deterministic fake-provider and in-memory-store tests cover every
  externally observable requirement.
- **Hardware isolation**: PASS; no planned test requires a physical device.
- **Simplicity**: PASS; the unique existing ID and existing settings pipeline are reused.
- **Quality gates**: PASS; implementation completion requires repository `dotnet build` and
  `dotnet test`.

## Complexity Tracking

No constitution violations or complexity exceptions.
