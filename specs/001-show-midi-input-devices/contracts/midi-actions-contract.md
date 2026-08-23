# Contract: MIDI Actions Backend

## Purpose

Define the behavioral contract that all MIDI actions in this feature must satisfy while using Windows MIDI Services as the backend.

## Scope

- In scope for this feature: listing input MIDI devices and refreshing the list.
- Out of scope for this feature (but governed by this backend rule): advanced routing, session management, and message processing flows.

## Contract Rules

1. **Single backend rule**: MIDI actions MUST use Windows MIDI Services APIs via Windows Runtime device discovery; legacy WinMM-based action paths are not permitted for feature behavior.
2. **Provider abstraction rule**: UI-facing logic MUST consume MIDI actions through repository abstractions (e.g., `IMidiInputDeviceProvider`) rather than direct platform calls.
3. **Discovery result contract**:
   - Returns a list of zero or more input devices.
   - Each returned item includes a human-readable name.
   - Duplicate names are valid and must be preserved as separate entries.
4. **Failure contract**:
   - Discovery failures are surfaced to caller as explicit failure outcomes (exception/error path).
   - Caller maps failures to user-visible error state with retry capability.
5. **Responsiveness contract**:
   - Refresh operations must not leave the UI in an unresponsive state.
   - State changes follow `Loading -> Loaded|Empty|Error` transitions documented in `data-model.md`.

## Acceptance Mapping

- FR-001, FR-002: satisfied by discovery result contract.
- FR-003, FR-007: satisfied by responsiveness and refresh-state behavior.
- FR-004, FR-006: satisfied by empty/failure contracts and UI state mapping.
