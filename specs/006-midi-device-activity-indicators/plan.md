# Implementation Plan: MIDI Device Activity Indicators

**Branch**: `006-midi-device-activity-indicators` | **Date**: 2026-08-25 | **Spec**: [spec.md](spec.md)

## Summary

Add a transient, per-device activity state to the existing MIDI device rows and display it
as a small dot before each device name. Activity will be raised from the existing portable
routing message boundary so UI state remains separate from Windows MIDI Services. Replace
the fixed device-list sizing with a container-fitting layout that keeps entries within the
available width and avoids horizontal scrolling.

## Technical Context

**Language/Version**: C# / .NET 10

**Primary Dependencies**: WPF, Windows.Devices.Midi2 0.99.33-devpreview.3, Microsoft.Extensions.Logging

**Storage**: N/A; activity is transient UI state and is not persisted

**Testing**: xUnit unit and UI-contract-oriented tests with fake MIDI routing endpoints; no physical hardware

**Target Platform**: Windows 10.0.19041+ with Windows MIDI Services

**Project Type**: WPF desktop application with background MIDI routing

**Performance Goals**: Raise an activity notification within 250 ms of a received routing
message; expire inactive indicators within 2 seconds after messages stop; do not block
message callbacks or routing work on UI rendering.

**Constraints**: Preserve existing device discovery, selection, routing, channel assignment,
theme, and empty-state behavior. Keep device identity keyed by stable endpoint ID. The
device list must fit its parent and must not expose horizontal scrolling; long names must
remain identifiable without expanding the list.

**Scale/Scope**: Existing device list and routing scale, including up to 16 active routed
devices; one transient activity state per listed device; no message history or persistence.

## Constitution Check

- PASS: Device discovery, routing message handling, row state, and visual presentation remain
  separate modules with explicit event/property boundaries.
- PASS: Activity notifications use the existing message callback path without blocking that
  path; timer expiration and UI updates are isolated from routing.
- PASS: Tests will cover observable row activity, independent device signals, expiration,
  lifecycle cleanup, and responsive UI contract behavior using fakes.
- PASS: Windows MIDI Services remains confined to the existing adapter; portable tests do
  not require physical MIDI hardware.
- PASS: The smallest extension is a row activity state plus an existing-router notification;
  no new persistence layer or framework is introduced.

## Project Structure

```text
MidiInputDeviceRow.cs              # Per-device activity state and notifications
MidiInputDeviceViewModel.cs        # Maps received device activity to rows and lifecycle
MidiRouter.cs                       # Publishes the source device for received physical messages
MidiRouterDeviceCoordinator.cs      # Connects router activity to the device view model
MainWindow.xaml                     # Activity-dot template and responsive device list layout
ThemeResources\Light.xaml           # Activity color resource and list sizing styles if needed
ThemeResources\Dark.xaml            # Activity color resource and list sizing styles if needed

midi-router.Tests\
├── MidiInputDeviceViewModelTests.cs # Row activity and expiration behavior
├── MidiRoutingTests.cs              # Source-device activity event contract
└── MainWindowLayoutTests.cs         # Markup/UI contract checks where supported
```

**Structure Decision**: Keep the existing single WPF project and root-level feature classes.
Use the stable endpoint ID for activity lookup. The router emits a lightweight source-device
notification when a physical input message is received; the coordinator forwards it to the
view model, which updates the matching row on the WPF dispatcher. XAML owns the dot and
responsive layout presentation.

## Complexity Tracking

No constitution violations.

## Phase 0 Research Summary

See [research.md](research.md). The design reuses the existing `MidiRoutingMessage.SourceDeviceId`
and `IMidiRoutingEndpoint.MessageReceived` boundary, uses a cancellable per-row expiration
mechanism, and uses stretch/fill layout with non-scrolling text behavior rather than fixed
aggregate column widths.

## Phase 1 Design Decisions

- Activity is keyed by stable `EndpointDeviceId`, never by display name, so duplicate names
  remain independent.
- The portable router raises activity after identifying a physical source and before or
  alongside normal forwarding; the notification does not perform UI work or wait for expiry.
- The view model resolves the source ID to the current row and marshals property changes to
  the WPF dispatcher. Removed rows cancel/ignore pending expiry work.
- A new message refreshes the same row's expiration deadline, preventing rapid-message
  flicker while keeping the green state transient.
- The dot has an inactive theme-aware color and a green active color, with a fixed small
  footprint before the name.
- The device column and row content stretch to the containing list width; long names wrap or
  truncate within that cell, and horizontal scrolling is disabled for the device list.
- Existing fixed protocol/channel affordances remain usable; only the device-name region
  absorbs width changes.

## Constitution Check (Post-Design)

- PASS: The event/property contract keeps routing, view-model state, and XAML rendering
  independently testable.
- PASS: Message callbacks remain lightweight and no timer or dispatcher operation is placed
  in the Windows MIDI Services callback itself.
- PASS: Deterministic fake endpoints can assert source IDs, activity refresh, expiry, and
  cleanup without hardware.
- PASS: The design targets Windows/.NET 10 and preserves explicit existing unavailable and
  empty states.
- PASS: No speculative abstraction or persistent model is added; all new state has a direct
  UI consumer and testable behavior.
