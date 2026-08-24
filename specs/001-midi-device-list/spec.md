# Feature Specification: MIDI Device List

**Feature Branch**: `001-midi-device-list`

**Created**: 2026-08-23

**Status**: Draft

**Input**: User description: "Midi Router shows a list of all connected Midi 1 and Midi 2 devices. The list items contain the name of the device and the Midi Version. The list dynamically updates when a device is connected or disconnected."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - View Connected MIDI Devices (Priority: P1)

As a MIDI Router user, I want to see all currently connected MIDI devices with their
names and MIDI versions so that I can understand which devices are available.

**Why this priority**: A trustworthy device overview is the foundation for using the
router and is valuable even without any other routing configuration.

**Independent Test**: Provide a set of connected MIDI 1 and MIDI 2 devices, open the
device overview, and verify that every device appears with the correct name and version.

**Acceptance Scenarios**:

1. **Given** MIDI 1 and MIDI 2 devices are connected, **When** the device overview is
   shown, **Then** it lists every connected device exactly once with its device name and
   corresponding MIDI version.
2. **Given** no MIDI devices are connected, **When** the device overview is shown, **Then**
   it displays an explicit empty state instead of a blank or misleading list.

---

### User Story 2 - See Device Connection Changes (Priority: P1)

As a MIDI Router user, I want the device overview to reflect connections and
disconnections automatically so that I can use a newly available device without
restarting or manually refreshing the application.

**Why this priority**: MIDI devices are commonly connected and removed while the
application is running, so stale availability information would make routing unreliable.

**Independent Test**: Start with one connected device, connect another device, then
disconnect the first device, and verify the overview changes after each event without
restarting the application.

**Acceptance Scenarios**:

1. **Given** the overview is visible and a previously absent MIDI device is connected,
   **When** the connection is detected, **Then** the new device appears with its name and
   MIDI version without user action.
2. **Given** a listed MIDI device is disconnected, **When** the disconnection is detected,
   **Then** that device is removed from the list without removing remaining devices.
3. **Given** a device is disconnected and then reconnected, **When** the reconnection is
   detected, **Then** the device appears again as one current entry with its correct
   details.

### Edge Cases

- If two connected devices have the same display name, both devices remain listed and
  neither is incorrectly merged with the other.
- If a device changes availability repeatedly in a short period, the final list reflects
  the devices that are currently connected and contains no duplicate entries.
- If device information cannot be read during a connection change, the application
  retains the other valid entries and presents an explicit status for the affected
  device or refresh operation.
- If a device reports an unsupported or unavailable MIDI version, the list presents an
  explicit unknown status rather than assigning MIDI 1 or MIDI 2 incorrectly.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The application MUST display a device overview containing every currently
  connected MIDI 1 and MIDI 2 device.
- **FR-002**: Each device entry MUST display the device's name and MIDI version.
- **FR-003**: The application MUST distinguish MIDI 1 and MIDI 2 using consistent,
  user-readable version labels.
- **FR-004**: The application MUST add a device to the overview automatically after its
  connection is detected.
- **FR-005**: The application MUST remove a device from the overview automatically after
  its disconnection is detected.
- **FR-006**: Automatic updates MUST preserve the entries and details of devices that
  remain connected.
- **FR-007**: The application MUST prevent duplicate entries for a single currently
  connected device.
- **FR-008**: The application MUST provide an explicit empty state when no supported MIDI
  devices are connected.
- **FR-009**: The application MUST present an explicit status when device enumeration or
  an update cannot obtain valid device information.
- **FR-010**: The application MUST remain usable while the device overview is updating.

### Key Entities *(include if feature involves data)*

- **MIDI Device**: A currently connected MIDI endpoint represented by a stable identity,
  display name, and MIDI version.
- **MIDI Version**: The user-visible classification of a device as MIDI 1, MIDI 2, or
  an explicit unknown/unavailable state when the version cannot be determined.
- **Device Overview**: The current set of device entries and its empty, populated, or
  update-error status.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: On initial display, 100% of connected supported devices appear exactly once
  with the correct name and MIDI version.
- **SC-002**: After a connection or disconnection is detected, the visible overview reflects
  the new device set within 2 seconds in at least 95% of observed events.
- **SC-003**: In a test set of at least 20 connected devices, adding or removing one device
  leaves all other entries present and unchanged.
- **SC-004**: Users can determine the name and MIDI version of any listed device without
  opening a secondary view or restarting the application.
- **SC-005**: When no supported devices are connected, 100% of test runs show an explicit
  empty state rather than stale device data.

## Assumptions

- The feature applies to users running the existing supported Windows environment with
  access to MIDI device services.
- A device has a stable identity that can be used to recognize the same device across
  connection updates; the identity itself does not need to be shown to users.
- Automatic updates are limited to connection and disconnection events; device renaming
  or MIDI capability changes while connected are outside this feature unless they trigger
  a new device update.
- The overview is read-only in this feature; routing configuration and device actions are
  outside its scope.
