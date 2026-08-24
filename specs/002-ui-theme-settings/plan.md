# Implementation Plan: UI Theme Settings

**Branch**: `002-ui-theme-settings` | **Date**: 2026-08-24 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `specs/002-ui-theme-settings/spec.md`

## Summary

Add a simple, discoverable appearance menu to the existing WPF window. The application will
expose Light, Dark, and OS default as a persisted preference, resolve OS default at runtime,
and apply centralized resource dictionaries without coupling appearance behavior to MIDI
device monitoring.

## Technical Context

**Language/Version**: C# on .NET 10 with the repository's `net10.0-windows10.0.22621` target.

**Primary Dependencies**: WPF, Windows Forms tray integration, `System.Text.Json`, and the
existing xUnit test stack. No new UI framework is required.

**Storage**: A JSON settings file in the existing per-user application data location.

**Testing**: xUnit with deterministic settings-file paths, fake OS-theme providers, and
resource-selection tests; no physical MIDI hardware or interactive desktop is required.

**Target Platform**: Windows 10/11 supported by the project, with OS appearance detection
isolated behind a testable provider.

**Project Type**: WPF desktop application with a background MIDI device-monitoring service.

**Performance Goals**: Theme selection becomes visible within 1 second; startup preference
loading and initial theme selection add no perceptible delay to the main window.

**Constraints**: The application must remain usable when the settings file is missing,
malformed, inaccessible, or unwritable. Theme resources must cover all currently visible
controls and preserve readable contrast. OS preference changes must be observed while OS
default is active. MIDI monitoring lifecycle and tray behavior remain unchanged.

**Scale/Scope**: One application window and one per-user appearance preference. This feature
does not add general settings infrastructure, theme customization, synchronization, or
changes to routing/device behavior.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Plan evidence | Status |
|---|---|---|
| Modular Architecture | Persistence, OS detection, theme application, and WPF menu are separate responsibilities; MIDI monitoring remains untouched. | PASS |
| Background-Process Reliability | File failures use explicit diagnostics and safe OS-default behavior; OS preference notifications are unsubscribed during shutdown. | PASS |
| Test-Driven Development | Tests cover mode parsing, persistence, fallback, resource selection, menu state, and OS-change behavior before implementation. | PASS |
| Hardware-Isolated Integration Testing | Theme tests use fake OS and file providers and do not access MIDI hardware. | PASS |
| Simplicity and Extensibility | Three fixed modes and centralized theme resources avoid speculative settings or styling frameworks. | PASS |
| Platform and Runtime Constraints | Uses the existing Windows/.NET 10 WPF target and isolates Windows OS-theme access. | PASS |
| Development Workflow and Quality Gates | `dotnet build` and `dotnet test` remain required from the repository root. | PASS |

No constitution violations require complexity justification.

## Project Structure

### Documentation (this feature)

```text
specs/002-ui-theme-settings/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── ui-theme-settings.md
└── tasks.md                 # Created by /speckit-tasks
```

### Source Code (repository root)

```text
App.xaml(.cs)                         # Application resources and theme lifecycle
MainWindow.xaml(.cs)                  # Gear button, settings menu, and bindings
AppearanceMode.cs                     # Light/Dark/OS default value object
ISettingsStore.cs                     # Testable persisted-settings boundary
JsonSettingsStore.cs                  # Per-user JSON settings implementation
IOperatingSystemThemeProvider.cs      # Testable OS appearance boundary
WindowsOperatingSystemThemeProvider.cs
ThemeManager.cs                       # Preference resolution and resource switching
ThemeResources/Light.xaml             # Light palette and shared control styles
ThemeResources/Dark.xaml              # Dark palette and shared control styles
midi-router.Tests/
├── AppearanceModeTests.cs
├── JsonSettingsStoreTests.cs
└── ThemeManagerTests.cs
```

**Structure Decision**: Preserve the repository's single root-level WPF project and test
project. Add focused root-level classes to match the current layout, and place only reusable
visual resources under `ThemeResources/`; no additional project or framework is warranted.

## Complexity Tracking

No violations.
