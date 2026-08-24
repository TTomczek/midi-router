# Implementation Plan: Minimize to Tray

**Branch**: `003-minimize-to-tray` | **Date**: 2026-08-24 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `/specs/003-minimize-to-tray/spec.md`

## Summary

Add an opt-in minimize-to-tray behavior to the Windows desktop application. The existing
notification-area integration will remain the lifecycle boundary, while the existing JSON
settings store will persist the new preference alongside the appearance setting. The window
will only hide after a minimize event when the preference is enabled; a single left click will
restore it, and the tray context menu will provide a normal application shutdown action.

## Technical Context

**Language/Version**: C# on .NET 10

**Primary Dependencies**: WPF, Windows Forms `NotifyIcon`, existing settings and MIDI provider
abstractions

**Storage**: Existing JSON settings file in the local user's application-data directory

**Testing**: xUnit tests in `midi-router.Tests`; UI contract tests inspect XAML and behavior is
tested through injectable lifecycle/settings collaborators

**Target Platform**: Windows 10/11, supported by the existing Windows target framework

**Project Type**: Windows desktop application

**Performance Goals**: Minimize and restore interactions complete within 2 seconds; tray
handling must not block MIDI discovery or message routing

**Constraints**: No physical MIDI hardware in tests; preserve existing appearance settings and
normal taskbar minimization when the preference is disabled; surface settings persistence errors
through the established diagnostic mechanism

**Scale/Scope**: One main window, one per-user preference, one notification-area icon, and one
tray context menu; no multi-window or cross-machine synchronization support

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Gate | Status |
|-----------|------|--------|
| I. Modular Architecture | Keep settings, tray lifecycle, and MIDI processing separated behind focused boundaries. | PASS |
| II. Background-Process Reliability | Define explicit minimize, restore, stop, startup fallback, and cleanup behavior; report persistence failures. | PASS |
| III. Test-Driven Development | Add failing tests first for setting persistence, event behavior, and tray/UI contracts. | PASS |
| IV. Hardware-Isolated Integration Testing | Keep tests independent of physical MIDI hardware and isolate Windows lifecycle integration. | PASS |
| V. Simplicity and Extensibility | Extend existing settings and tray paths rather than adding a new framework or duplicate application pathway. | PASS |
| Platform and Quality Gates | Keep Windows/.NET 10 targeting and require `dotnet build` and `dotnet test`. | PASS |

## Project Structure

### Documentation (this feature)

```text
specs/[###-feature]/
├── plan.md              # This file (/speckit-plan command output)
├── research.md          # Phase 0 output (/speckit-plan command)
├── data-model.md        # Phase 1 output (/speckit-plan command)
├── quickstart.md        # Phase 1 output (/speckit-plan command)
├── contracts/           # Phase 1 output (/speckit-plan command)
└── tasks.md             # Phase 2 output (/speckit-tasks command - NOT created by /speckit-plan)
```

### Source Code (repository root)

```text
midi-router.csproj
├── App.xaml.cs
├── MainWindow.xaml
├── MainWindow.xaml.cs
├── ApplicationSettings.cs
├── ISettingsStore.cs
├── JsonSettingsStore.cs
├── ThemeManager.cs
└── [tray/settings lifecycle boundary, if extracted]

midi-router.Tests/
├── JsonSettingsStoreTests.cs
├── ThemeManagerTests.cs
├── MainWindowThemeTests.cs
└── [minimize-to-tray and settings contract tests]
```

**Structure Decision**: Keep the single-project Windows desktop layout. Extend the existing
root-level settings and main-window files, adding a focused tray/settings lifecycle boundary
only if needed to keep persistence and window behavior independently testable. Add deterministic
tests to the existing `midi-router.Tests` project.

The repository is a single Windows desktop project with tests in a sibling test project.
Feature planning artifacts remain under `specs/003-minimize-to-tray`; production changes will
use the existing repository-root C# files and test files rather than introducing new
application layers.

## Complexity Tracking

No constitution violations identified.

## Implementation Design

1. Extend `ApplicationSettings` with a boolean minimize-to-tray preference whose default is
   disabled, preserving the existing appearance field and JSON compatibility.
2. Extend the existing settings coordination so theme and tray preferences are loaded together
   and saved without overwriting each other. The tray preference must have an injectable
   settings boundary for deterministic tests, normalize invalid data to disabled, and report
   read/write failures without stopping the application.
3. Update `MainWindow` to consult the preference in `OnStateChanged`, hide only for the enabled
   case, restore on one left-click, and retain the existing cleanup path for the tray icon and
   menu.
4. Keep the tray menu's stop action on the existing normal close/shutdown path and ensure
   dismissal or restoration does not create additional windows.
5. Expose the setting through the existing settings menu and keep its state synchronized with
   the persisted preference.
6. Add tests for defaults, round trips, invalid values, persistence diagnostics, setting
   changes, minimize decisions, restoration events, and XAML/settings discoverability. Tests
   must use fakes and source-level UI contracts where WPF/notification-area interaction cannot
   run headlessly.

## Constitution Check - Post-Design

| Principle | Verification | Status |
|-----------|--------------|--------|
| I. Modular Architecture | Settings persistence remains behind `ISettingsStore`; tray lifecycle stays in the window/lifecycle boundary; MIDI view-model code is unchanged. | PASS |
| II. Background-Process Reliability | Minimize, restore, context-menu dismissal, stop, startup fallback, and tray disposal are explicitly covered by the contract and quickstart. | PASS |
| III. Test-Driven Development | The implementation work is planned around red-green-refactor tests for persistence, lifecycle policy, and UI contracts. | PASS |
| IV. Hardware-Isolated Integration Testing | Automated scenarios use settings fakes and do not require MIDI hardware; manual tray validation is separate. | PASS |
| V. Simplicity and Extensibility | Existing WPF/Windows Forms and JSON infrastructure is reused; no new external dependency is planned. | PASS |
| Platform and Quality Gates | Design remains Windows/.NET 10-specific and retains repository `dotnet build` and `dotnet test` gates. | PASS |
