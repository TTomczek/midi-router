# Implementation Plan: Input MIDI Device Browser UI

**Branch**: `001-show-midi-input-devices` | **Date**: 2026-08-22 | **Spec**: [`spec.md`](./spec.md)

**Input**: Feature specification from `/specs/001-show-midi-input-devices/spec.md` and planning directive: "Use Windows Midi Services for all Midi Actions"

## Summary

Deliver a modern, responsive UI that lists all available input MIDI devices by name, supports manual refresh, and provides clear empty/error states. All MIDI actions for this feature (device discovery now, and extension points for future actions) will use Windows MIDI Services as the only MIDI backend.

## Technical Context

**Language/Version**: C# on .NET 8 (`net8.0-windows`)

**Primary Dependencies**: WPF, Windows MIDI Services runtime/SDK integration for MIDI discovery and actions, existing view-model abstraction (`IMidiInputDeviceProvider`)

**Storage**: N/A

**Testing**: xUnit via `dotnet test` with hardware-independent provider stubs/mocks

**Target Platform**: Windows desktop (Windows 11+ with Windows MIDI Services available)

**Project Type**: Desktop application (WPF) with separate test project

**Performance Goals**: Meet spec outcomes: list visibility within 10 seconds for 95% of users; 95% of refresh actions show updated list within 2 seconds

**Constraints**: Use Windows MIDI Services for all MIDI actions; keep UI responsive during refresh; preserve modular separation between UI and MIDI access abstractions

**Scale/Scope**: Single-screen device browser flow, local machine device inventory (typical workstation-scale device counts)

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- **I. Lightweight and Modular**: PASS — continue using provider abstraction so UI remains isolated from MIDI backend implementation details.
- **II. Background-Process First**: PASS — refresh/update behavior will avoid blocking user interaction and keep UI concerns separate from MIDI discovery logic.
- **III. Tested Features**: PASS — unit tests remain hardware-independent through abstractions; add/update tests for refresh, empty, and failure paths.
- **IV. Documented Behavior**: PASS — update README and feature quickstart with Windows MIDI Services behavior and validation flow.
- **V. Simplicity and Explicit Change**: PASS — replace legacy backend path with a single Windows MIDI Services backend, minimizing parallel code paths.

**Post-Design Re-check**: PASS — Phase 0/1 artifacts keep boundaries explicit, preserve testability, and do not introduce unnecessary complexity.

## Project Structure

### Documentation (this feature)

```text
specs/001-show-midi-input-devices/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── midi-actions-contract.md
└── tasks.md
```

### Source Code (repository root)

```text
App.xaml
MainWindow.xaml
MainWindow.xaml.cs
MidiInputDeviceProvider.cs
MidiInputDeviceViewModel.cs
midi-router.csproj
midi-router.Tests/
└── MidiInputDeviceViewModelTests.cs
```

**Structure Decision**: Keep the existing single-project WPF + test-project structure. Implement MIDI backend changes in `MidiInputDeviceProvider.cs`, keep UI behavior in `MidiInputDeviceViewModel.cs` and `MainWindow.xaml`, and validate behavior in `midi-router.Tests`.

## Complexity Tracking

No constitutional violations identified; no complexity exceptions required.
