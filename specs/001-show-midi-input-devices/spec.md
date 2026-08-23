# Feature Specification: Input MIDI Device Browser UI

**Feature Branch**: `001-show-midi-input-devices`

**Created**: 2026-08-22

**Status**: Draft

**Input**: User description: "Create a simple but modern ui that shows all Input MIDI devices with their names."

## User Scenarios & Testing *(mandatory)*

<!--
  IMPORTANT: User stories should be PRIORITIZED as user journeys ordered by importance.
  Each user story/journey must be INDEPENDENTLY TESTABLE - meaning if you implement just ONE of them,
  you should still have a viable MVP (Minimum Viable Product) that delivers value.

  Assign priorities (P1, P2, P3, etc.) to each story, where P1 is the most critical.
  Think of each story as a standalone slice of functionality that can be:
  - Developed independently
  - Tested independently
  - Deployed independently
  - Demonstrated to users independently
-->

### User Story 1 - View connected input devices (Priority: P1)

As a user, I can open the MIDI device screen and immediately see all currently available input MIDI devices by name.

**Why this priority**: Showing the available input devices is the core value of the feature and enables all downstream routing tasks.

**Independent Test**: Can be fully tested by opening the screen with one or more input devices connected and confirming the list shows each device name once.

**Acceptance Scenarios**:

1. **Given** at least one input MIDI device is available, **When** the user opens the screen, **Then** the UI displays a list of available input devices with their names.
2. **Given** no input MIDI devices are available, **When** the user opens the screen, **Then** the UI shows an explicit empty state message that no input devices are detected.

---

### User Story 2 - Keep the list current (Priority: P2)

As a user, I can refresh the screen view so that newly connected or disconnected input devices are reflected in the visible list.

**Why this priority**: Device availability changes frequently in music workflows, so users need a reliable way to see the current state.

**Independent Test**: Can be tested by opening the screen, changing connected devices, performing a refresh action, and confirming the list updates accordingly.

**Acceptance Scenarios**:

1. **Given** the device list is visible, **When** the user performs a refresh action, **Then** the list updates to match currently available input MIDI devices.

---

### User Story 3 - Read list comfortably (Priority: P3)

As a user, I can quickly scan device names in a clean, modern layout without visual clutter.

**Why this priority**: Readability and visual clarity improve confidence and speed when selecting or verifying devices.

**Independent Test**: Can be tested by reviewing the screen and confirming that device names are clearly legible, consistently spaced, and visually distinct from status or helper text.

**Acceptance Scenarios**:

1. **Given** the list is rendered, **When** a user scans the screen, **Then** device names are easy to read and the interface appears minimal and modern.

---

### Edge Cases

- Two or more devices report the same display name; the UI still presents each as a separate entry.
- A device disconnects during an active refresh; the list completes refresh and reflects only currently available devices.
- Device discovery fails temporarily; the UI preserves a usable state and shows a clear error message with retry guidance.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST provide a dedicated screen that lists all currently available input MIDI devices.
- **FR-002**: The system MUST display each input device using a human-readable device name.
- **FR-003**: The system MUST include a visible refresh action that reloads the list of input MIDI devices on user request.
- **FR-004**: The system MUST show a clear empty state when no input MIDI devices are available.
- **FR-005**: The system MUST present device names in a simple, modern visual style with consistent spacing and typography.
- **FR-006**: The system MUST handle temporary device discovery failures by showing a clear user-facing error state and allowing retry.
- **FR-007**: The system MUST keep the interface responsive while device discovery and refresh operations are in progress.
- **FR-008**: The system MUST detect MIDI 1 and MIDI 2 input endpoints and display the MIDI version for each listed device.

### Key Entities *(include if feature involves data)*

- **Input MIDI Device**: A discoverable MIDI input source shown in the UI; key attributes are device name and current availability status.
- **Device List View State**: The visible state of the screen; includes loading state, populated list state, empty state, and error state.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: In user testing, at least 95% of participants can identify whether their input MIDI device is available within 10 seconds of opening the screen.
- **SC-002**: For 95% of refresh attempts, the updated device list is shown within 2 seconds of user action.
- **SC-003**: At least 90% of participants rate the screen as clear and modern in post-task feedback.
- **SC-004**: At least 99% of list render attempts complete without requiring users to restart the application session.

## Assumptions

- The feature targets users who have one or more local MIDI input devices connected to the host machine.
- This scope is limited to displaying input devices and their names; device selection, routing, and configuration are out of scope.
- Device names are sourced from the system-reported MIDI device metadata and are shown as provided.
- Existing application navigation already provides a way to open this screen.
