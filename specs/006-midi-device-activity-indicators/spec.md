# Feature Specification: MIDI Device Activity Indicators

**Feature Branch**: `006-midi-device-activity-indicators`

**Created**: 2026-08-25

**Status**: Draft

**Input**: User description: "Add a small dot in front of Device name in the Midi device list of the midi Router application. The dot should light up shortly in green when a midi message of the device is received. The width of the device list dynamically fits the outer element so it doesn't need a horizontal scrollbar."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - See MIDI Device Activity (Priority: P1)

As a MIDI Router user, I want each listed device to have an activity indicator so that I
can immediately tell which device is producing MIDI messages.

**Why this priority**: Visible activity confirms that a connected device is communicating
without requiring the user to inspect or route messages.

**Independent Test**: Display one or more connected devices, send a MIDI message from a
selected device, and verify that only that device's indicator lights green briefly before
returning to its inactive appearance.

**Acceptance Scenarios**:

1. **Given** a connected device is listed and inactive, **When** the device sends a MIDI
   message, **Then** the dot directly before its name lights green shortly after receipt.
2. **Given** one device sends a MIDI message, **When** other devices remain silent, **Then**
   only the sending device's dot lights up.
3. **Given** a device's activity dot is lit, **When** no further message is received during
   the activity indication period, **Then** the dot returns to its inactive appearance.
4. **Given** a device sends messages repeatedly, **When** messages continue to arrive, **Then**
   its indicator remains visibly responsive without affecting the device name or list order.

---

### User Story 2 - View the Complete Device Name Without Horizontal Scrolling (Priority: P1)

As a MIDI Router user, I want the device list to fit its surrounding element so that I can
read and use the list without managing a horizontal scrollbar.

**Why this priority**: The device name and activity state are useful only when the list remains
readable and accessible in the available application layout.

**Independent Test**: Show the device list inside its surrounding element at multiple window
sizes, including names longer than the available width, and verify that the list fits without
horizontal scrolling.

**Acceptance Scenarios**:

1. **Given** the outer element changes width, **When** the device list is displayed, **Then**
   the list adjusts to the available width without extending beyond the outer element.
2. **Given** a listed device has a long name, **When** the list is narrower than that name,
   **Then** the name remains usable within the list without creating a horizontal scrollbar.
3. **Given** multiple devices are listed, **When** the available width changes, **Then** all
   entries retain their activity dots and readable names without changing the list's device
   contents.

### Edge Cases

- If a device sends multiple messages before the indicator returns to inactive, the indicator
  remains lit or is refreshed rather than flickering unpredictably.
- If messages from different devices arrive close together, each device's indicator reflects
  only its own messages.
- If the device list is empty, no activity indicator is shown and the existing explicit empty
  state remains usable.
- If a device is disconnected while its indicator is lit, its entry and indicator disappear
  with the device.
- If the available width is very small, device names remain within the list bounds and the
  list does not introduce horizontal scrolling.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: Each listed MIDI device MUST display a small activity dot immediately before its
  device name.
- **FR-002**: The activity dot MUST have a distinct inactive appearance when no recent MIDI
  message from that device has been received.
- **FR-003**: When a MIDI message is received from a listed device, the application MUST show
  that device's activity dot in green shortly after receipt.
- **FR-004**: The green activity indication MUST automatically return to the inactive appearance
  after a short period without a new message from that device.
- **FR-005**: Each device's activity state MUST be independent, so activity from one device MUST
  NOT light the dot for another device.
- **FR-006**: Repeated messages from a device MUST refresh or maintain its activity indication
  without changing the device name, device identity, or list ordering.
- **FR-007**: The device list MUST dynamically fit the width of its containing outer element.
- **FR-008**: The device list MUST NOT require or display a horizontal scrollbar when the outer
  element or a device name is wider than the currently available list width.
- **FR-009**: Device entries MUST remain identifiable and usable when the available list width
  changes.
- **FR-010**: Removing a device from the list MUST also remove its associated activity state.
- **FR-011**: The existing device list contents, device names, version labels, connection updates,
  and empty state MUST remain available unless directly affected by this feature.

### Key Entities *(include if feature involves data)*

- **MIDI Device Entry**: A listed connected device with its existing identity and display
  details, plus an activity state.
- **Activity Indicator**: The per-device visual state that is inactive by default and briefly
  green after a message from its associated device.
- **Device List Container**: The surrounding application element whose current width determines
  the usable width of the device list.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: In 100% of tested message events from a listed device, that device's indicator
  becomes green within 250 milliseconds of message receipt.
- **SC-002**: In 100% of tested message events, no device other than the message source shows
  a green activity indicator.
- **SC-003**: In 95% of tested activity events, the indicator returns to inactive within 2
  seconds after messages stop, while remaining responsive to continued messages.
- **SC-004**: Across window and container widths from 240 pixels upward, 100% of device-list
  views fit within the outer element without a horizontal scrollbar.
- **SC-005**: Users can identify the active device and read each device entry without opening
  another view or manually resizing the list in at least 95% of usability test attempts.
- **SC-006**: Existing device discovery, connection/disconnection updates, device details, and
  empty-state behavior pass unchanged in the feature's regression scenarios.

## Assumptions

- A received MIDI message can be associated with the currently listed device that produced it.
- "Shortly" means the green state is visible within 250 milliseconds and lasts briefly, with
  a default inactive transition no later than 2 seconds after messages stop.
- The inactive dot uses the application's existing visual conventions; the requested green
  state is the only required new color distinction.
- Long device names may wrap, truncate with an accessible way to identify the full name, or
  otherwise remain usable, provided they do not force horizontal scrolling.
- The existing device list and its connection lifecycle remain the source of which devices are
  displayed; this feature does not add routing controls or message logging.
