# Implementation Plan: Profile Management

**Branch**: `007-profile-management` | **Date**: 2026-08-30 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `/specs/007-profile-management/spec.md`

## Summary

Add locally persisted, independently switchable MIDI profiles while keeping application-wide appearance and tray preferences separate. A profile manager will own profile discovery, per-file persistence, active-profile selection, name validation, duplicate-name display labels, and safe create/rename/delete operations. The existing device view model will read and write the active profile's device and channel state, and the main window will expose profile selection beside the settings button with a consistently styled name dialog for creation and renaming.

## Technical Context

**Language/Version**: C# on .NET 10 (`net10.0-windows10.0.22621`)

**Primary Dependencies**: WPF, existing `ApplicationSettingsCoordinator`/JSON serialization, Windows MIDI Services providers, xUnit

**Storage**: JSON files in `%LOCALAPPDATA%\MIDI Router`; one global settings file and one file per profile

**Testing**: xUnit unit tests, hardware-free fakes, `dotnet build`, and `dotnet test`

**Target Platform**: Windows 10 build 19041 or newer with .NET 10 and optional Windows MIDI Services availability

**Project Type**: Desktop WPF application

**Performance Goals**: Profile selection applies the saved state within 2 seconds; profile file operations complete without blocking MIDI message routing

**Constraints**: Preserve the existing background routing lifecycle; profile persistence errors must be surfaced; tests must not require physical MIDI hardware; at least one profile must always remain

**Scale/Scope**: Local single-installation profile management for the existing application; expected profile counts are small enough for loading the profile list at startup

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- **Modular Architecture**: PASS. Profile persistence and profile lifecycle are isolated from device discovery, routing, and presentation; explicit interfaces will separate storage and profile management.
- **Background-Process Reliability**: PASS. Profile changes happen on the UI/configuration path and are applied through existing view-model/coordinator boundaries; routing remains independently owned, and persistence failures are reported.
- **Test-Driven Development**: PASS. Tests will cover profile model validation, per-file persistence, duplicate labels, lifecycle rules, and integration with active device/channel state before implementation.
- **Hardware-Isolated Integration Testing**: PASS. Profile and switching tests use fake providers and do not require Windows MIDI hardware.
- **Simplicity and Extensibility**: PASS. The design reuses the existing JSON and settings normalization patterns and introduces only the profile store/manager boundaries needed by the feature.
- **Platform and Runtime Constraints**: PASS. The plan targets the existing Windows/.NET 10 WPF application and preserves explicit unavailable-device status behavior.

## Project Structure

### Documentation (this feature)

```text
specs/007-profile-management/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── profile-ui.md
└── tasks.md
```

### Source Code (repository root)

```text
/
├── ApplicationSettings.cs
├── ApplicationSettingsCoordinator.cs
├── ISettingsStore.cs
├── JsonSettingsStore.cs
├── MainWindow.xaml
├── MainWindow.xaml.cs
├── MidiInputDeviceViewModel.cs
├── Profile.cs
├── IProfileStore.cs
├── JsonProfileStore.cs
├── ProfileManager.cs
└── midi-router.Tests/
    ├── ProfileTests.cs
    ├── JsonProfileStoreTests.cs
    ├── ProfileManagerTests.cs
    ├── MidiInputDeviceViewModelTests.cs
    └── MainWindowLayoutTests.cs
```

**Structure Decision**: Keep the existing single-project WPF layout. Add profile domain and persistence types at the repository root, compose them in `App.xaml.cs`/`MainWindow`, and extend the existing hardware-free test project.

## Phase 0: Research

See [research.md](research.md) for decisions on per-profile JSON storage, migration, active-state application, duplicate labels, and WPF interaction behavior.

## Phase 1: Design

See [data-model.md](data-model.md) for profile fields, validation, normalization, and lifecycle rules. See [contracts/profile-ui.md](contracts/profile-ui.md) for the observable UI contract. See [quickstart.md](quickstart.md) for the runnable validation scenarios.

## Post-Design Constitution Check

- **Modularity**: PASS. `IProfileStore` handles file I/O, `ProfileManager` handles profile lifecycle and labels, and the device view model remains the device/routing state boundary.
- **Reliability and error visibility**: PASS. Atomic per-file writes, explicit load/save diagnostics, and preservation of the last known good in-memory state are required.
- **Testability**: PASS. Store paths and providers are injectable; all profile behavior is testable without hardware or a live WPF window.
- **Simplicity**: PASS. No database, account system, synchronization service, or speculative profile hierarchy is introduced.

## Complexity Tracking

No constitution violations or complexity exceptions require justification.
