# Implementation Plan: MIDI Message Routing

**Branch**: `005-midi-message-routing` | **Date**: 2026-08-24 | **Spec**: [spec.md](spec.md)

## Summary

Add a hardware-isolated routing core that owns selected endpoint connections, assigns one
unique MIDI channel per active device, transforms channel-bearing messages toward one
virtual endpoint, and reverses that transform for messages returning from the virtual
endpoint. Windows MIDI Services access stays behind adapters; the existing settings and
device-list view model are extended, and deterministic fakes validate the routing core.

## Technical Context

**Language/Version**: C# / .NET 10
**Primary Dependencies**: Windows.Devices.Midi2 0.99.33-devpreview.3, WPF, Microsoft.Extensions.Logging
**Storage**: Existing JSON settings store, extended with device channel assignments
**Testing**: xUnit unit and contract tests with fake endpoint/session providers; no physical hardware
**Target Platform**: Windows 10.0.19041+ with Windows MIDI Services
**Project Type**: WPF desktop application with background MIDI routing
**Performance Goals**: Handle 16 active routes without cross-routing; drain SDK receive events
immediately and enqueue work so normal callbacks do not block.
**Constraints**: Internal channels 0-15, displayed as 1-16; one active device per routed
channel; platform integration isolated and failures visible.
**Scale/Scope**: Up to 16 simultaneously routed selected devices, one shared virtual MIDI
endpoint, channel replacement as the only implemented transformation.

## Constitution Check

- PASS: Discovery, endpoint access, transformation, UI, and lifecycle remain separate.
- PASS: Route failures are isolated and reported; receive handlers enqueue quickly.
- PASS: Tests cover transformations, routing decisions, lifecycle, and failures with fakes.
- PASS: Windows MIDI Services is confined to platform adapters targeting Windows/.NET 10.
- PASS: New abstractions have routing or test consumers and support future transformations.

## Project Structure

```text
ApplicationSettings.cs
ApplicationSettingsCoordinator.cs
MidiInputDevice.cs
MidiInputDeviceRow.cs
MidiInputDeviceViewModel.cs
WindowsMidiInputDeviceProvider.cs
MidiRouting*.cs                 # portable routing, transform, and lifecycle contracts
WindowsMidiRouting*.cs          # Windows MIDI Services endpoint adapters
MainWindow.xaml(.cs)            # channel column and assignment interaction

midi-router.Tests/
├── MidiChannelTransformTests.cs
├── MidiRouterTests.cs
├── MidiRoutingSettingsTests.cs
└── WindowsMidiAdapterTests.cs
```

**Structure Decision**: Keep the existing single WPF project and root-level feature classes.
Portable routing contracts and logic are separate from Windows MIDI Services adapters; tests
remain in `midi-router.Tests` and use fake endpoint providers.

## Complexity Tracking

No constitution violations. The virtual endpoint adapter and transform pipeline are required
consumers of the bidirectional routing requirement, not speculative abstractions.

## Phase 0 Research Summary

Windows MIDI Services exposes `MidiSession` as the owner of endpoint connections.
`MidiEndpointConnection` is configured with handlers before `Open()`, sends complete UMPs
with `SendSingleMessagePacket` or word-based methods, and raises `MessageReceived`
synchronously; handlers must therefore enqueue quickly. `MidiMessage32` represents short
MIDI 1.0-in-UMP messages and exposes `Word0`, allowing a pure channel-bit transform for
channel-bearing MIDI 1 messages while preserving other words/message types.

The pinned package must be inspected during implementation for the exact virtual-device
creation API and projection signatures. The adapter contract isolates that uncertainty and
does not leak WinRT types into portable routing tests.

References:
- https://microsoft.github.io/MIDI/sdk-reference/
- https://raw.githubusercontent.com/microsoft/MIDI/main/docs/sdk-reference/MidiSession.md
- https://raw.githubusercontent.com/microsoft/MIDI/main/docs/sdk-reference/MidiEndpointConnection.md
- https://raw.githubusercontent.com/microsoft/MIDI/main/docs/sdk-reference/IMidiMessageReceivedEventSource.md
- https://raw.githubusercontent.com/microsoft/MIDI/main/docs/sdk-reference/MidiMessage32.md

## Phase 1 Design Decisions

- Store assignments by stable endpoint ID alongside selected IDs.
- Use nullable assignments before allocation; scan internal channels 0-15 in ascending order
  and reserve a channel atomically.
- Display `internal channel + 1`; reject values outside 1-16 and assignment conflicts.
- Route physical input through per-device connections into one shared virtual connection.
- Treat virtual channel as the return-route key; unknown or ambiguous channels are dropped
  and reported, never guessed.
- Preserve original channel in routing context for reverse transformation.
- Stop only the affected connection on deselection/disconnect; keep other routes.
- Use queues for each direction, with explicit diagnostics for send failures.

## Constitution Check (Post-Design)

- PASS: Explicit portable interfaces isolate endpoint/session integration and device lifecycle.
- PASS: Synchronous callbacks enqueue work; route processing and sending are independent.
- PASS: Unit/contract tests exercise routing and transformation without hardware.
- PASS: Windows-specific code remains in adapters and unavailable services produce diagnostics.
- PASS: One shared router and one transformation pipeline satisfy current scope without extra
projects or framework layers.
