# Feature Specification: MIDI Message Routing

**Feature Branch**: `005-midi-message-routing`
**Created**: 2026-08-24
**Status**: Draft
**Input**: User description: "For all selected devices the application should listen for midi messages. For all midi messages the application should be able to perform a number of modifications to the midi messages. As a first modification the application should be able to modify the channel of the message. The channel to use for the mid message of a device should be configurable by the user in the device list. As a default the next free channel should be choosen. The first available channel is 0 and the maximum usable channel is 15. The channel should be shown as 1 - 16 to the user. The configured channel per device should be persisted. The modified messages should be send to a virtual midi device of the application for other application to use. The virtual midi device can also receive Midi messages and revert the modifications and send the message to the respected device. For example Device a sends a message with channel 0. Midi router receives the messages, modifies it based on the configuration to, e.g channel 2, and sends it to its virtual device. A DAW software receives the modified message and sends an answer back. Midi Router receives that answer and reverts all modifications of the pipeline. In this example the modification of the channel and sends the message back to device A based in the confired channel. Other modifications can be added in the future."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Route Selected Device Messages (Priority: P1)

As a MIDI Router user, I want messages from every selected device to be routed through
the application so that another application can use them through one virtual MIDI device.

**Why this priority**: Routing selected devices is the core value of the feature and
enables external applications to consume MIDI input.

**Independent Test**: Select one or more available devices, send messages from each
device, and verify that the corresponding messages arrive at the application's virtual
MIDI device.

**Acceptance Scenarios**:

1. **Given** a device is selected and connected, **When** it sends a MIDI message,
   **Then** the application receives and processes that message.
2. **Given** multiple devices are selected and connected, **When** each sends a MIDI
   message, **Then** each message is routed to the virtual MIDI device.
3. **Given** a device is not selected, **When** it sends a MIDI message, **Then** the
   application does not route that message through this feature.

---

### User Story 2 - Configure Device Channels (Priority: P1)

As a MIDI Router user, I want to see and configure the channel assigned to each device
in the device list so that messages from different devices can be distinguished.

**Why this priority**: Channel assignment is the first useful message modification and
must be controllable without leaving the device list.

**Independent Test**: Inspect selected devices, change a channel, and verify the displayed
value, accepted range, and resulting message channel.

**Acceptance Scenarios**:

1. **Given** a device is listed, **When** the user views the device list, **Then** its
   assigned channel is shown as a user-facing value from 1 through 16.
2. **Given** no channel has been configured for a device, **When** the device receives a
   message, **Then** it is assigned the next unused channel, starting with channel 1.
3. **Given** a device has an assigned channel, **When** the user changes it to another
   available value from 1 through 16, **Then** future outgoing messages use that channel.
4. **Given** the user enters a channel outside 1 through 16, **When** the value is
   applied, **Then** the application rejects it and preserves the last valid assignment.

---

### User Story 3 - Return Responses to the Originating Device (Priority: P1)

As a MIDI Router user, I want messages received from the virtual MIDI device to be
returned to the correct physical device with the original channel restored so that
external applications can communicate with the hardware.

**Why this priority**: Bidirectional routing makes the transformation pipeline useful
for interactive instruments and controllers rather than only for one-way monitoring.

**Independent Test**: Route a message from a device to the virtual MIDI device, send a
response back using the assigned channel, and verify that it reaches the originating
device on its original channel.

**Acceptance Scenarios**:

1. **Given** a device message was routed using an assigned channel, **When** the virtual
   MIDI device receives a response on that assigned channel, **Then** the application
   routes it to the originating device with the original channel restored.
2. **Given** several devices have distinct assigned channels, **When** the virtual MIDI
   device receives messages on those channels, **Then** each message is returned only to
   the device associated with that channel.
3. **Given** the virtual MIDI device receives a message whose channel has no device
   assignment, **When** the application processes it, **Then** it does not send the
   message to an arbitrary device and surfaces the routing issue through the established
   status or diagnostic mechanism.

---

### User Story 4 - Preserve Routing Configuration (Priority: P1)

As a MIDI Router user, I want channel assignments to persist so that routing behaves
consistently after restarting the application.

**Why this priority**: Persistent assignments prevent unexpected channel changes in
repeat-use setups.

**Independent Test**: Configure assignments, restart the application, and verify that
the same available devices retain their assignments.

**Acceptance Scenarios**:

1. **Given** a device has a valid saved channel assignment, **When** the application
   restarts and the device is available, **Then** the assignment is restored.
2. **Given** a device has no saved assignment, **When** the application starts, **Then**
   it receives the next available channel according to the default assignment rule.
3. **Given** a saved assignment refers to an unavailable device, **When** the application
   starts, **Then** the assignment does not prevent available devices from being routed.

### Edge Cases

- If more than 16 selected devices require simultaneous distinct channels, the application
  assigns the available channels and clearly marks additional devices as unavailable for
  channel-based routing until a channel is freed.
- If two devices have the same display name, channel ownership and response routing use
  each device's unique identity rather than its name.
- If a configured channel is already assigned to another selected device, the application
  rejects the conflicting assignment or requires the conflict to be resolved before it
  takes effect; it does not silently reroute either device.
- Messages that do not carry a MIDI channel pass through unchanged except for routing.
- If a selected device disconnects, its active input is stopped and the remaining routes
  continue operating; its saved assignment may be retained for reconnection.
- If the virtual MIDI device or a physical device cannot be opened, the application
  surfaces the failure and continues unrelated routes where possible.
- If configuration persistence fails, the current session remains usable and the failure
  is surfaced through the established status or diagnostic mechanism.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The application MUST listen for MIDI messages from every currently
  connected device selected in the device list.
- **FR-002**: The application MUST route messages from selected devices through a single
  virtual MIDI device that other applications can use.
- **FR-003**: The application MUST support an ordered message-modification pipeline so
  additional modifications can be added without changing the routing concept.
- **FR-004**: The first supported modification MUST replace the channel of channel-bearing
  MIDI messages with the channel assigned to the originating device.
- **FR-005**: The application MUST allow the user to configure a device's assigned channel
  from the device list.
- **FR-006**: The application MUST accept channel values 0 through 15 internally and MUST
  display those values to users as 1 through 16.
- **FR-007**: When no channel is configured, the application MUST assign the next unused
  channel, considering channels in ascending order from 0 through 15.
- **FR-008**: The application MUST prevent two simultaneously routed selected devices from
  using the same assigned channel.
- **FR-009**: The application MUST persist each device's valid channel assignment using
  the device's unique identity.
- **FR-010**: The application MUST restore valid saved channel assignments when the
  corresponding devices become available.
- **FR-011**: The virtual MIDI device MUST accept messages from external applications.
- **FR-012**: For a message received from the virtual MIDI device on an assigned channel,
  the application MUST identify the associated device, revert the channel modification,
  and send the message to that device.
- **FR-013**: The application MUST preserve enough routing context for a response to be
  returned to the originating device without relying on display names.
- **FR-014**: The application MUST NOT send a virtual-device message with an unknown or
  ambiguous channel to an arbitrary physical device.
- **FR-015**: The application MUST keep independent device routes operating when another
  device or route encounters an operational failure, where the platform permits.
- **FR-016**: The application MUST surface device, virtual-device, assignment, and
  persistence failures through the established status or diagnostic mechanism.
- **FR-017**: When a device is deselected or disconnected, the application MUST stop
  routing its messages while preserving unrelated active routes.
- **FR-018**: MIDI messages that do not contain a channel MUST retain their message data
  when routed.

### Key Entities *(include if feature involves data)*

- **MIDI Device**: A physical or virtual MIDI endpoint with a unique identity, display
  name, availability, and routing state.
- **Device Channel Assignment**: The persisted association between a device identity and
  one channel from 0 through 15, displayed to users as 1 through 16.
- **MIDI Message**: A message with message data and, where applicable, a channel that may
  be transformed during routing.
- **Routing Pipeline**: The ordered processing context that applies modifications on the
  way to the virtual device and reverses them on the way to a physical device.
- **Virtual MIDI Device**: The application-provided endpoint used by external applications
  to receive modified messages and send responses.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: In hardware-free automated scenarios, 100% of messages from selected
  devices reach the virtual MIDI device with the configured channel when the destination
  is available.
- **SC-002**: Users can configure a valid device channel from the device list in under
  30 seconds, and the new value is visibly reflected immediately.
- **SC-003**: Across 100 bidirectional message exchanges, 100% of responses received on
  assigned channels reach the correct originating device with its original channel.
- **SC-004**: After a normal restart, 100% of valid saved channel assignments for
  currently available devices are restored.
- **SC-005**: The system handles 16 simultaneously routed selected devices with distinct
  channels without cross-routing messages.
- **SC-006**: 100% of attempted assignments outside the supported channel range or
  conflicting with another active assignment are rejected with an understandable user
  or diagnostic indication.
- **SC-007**: In usability testing, at least 95% of users can identify and understand
  each device's displayed channel assignment without additional instructions.

## Assumptions

- Device identities remain stable when a device disconnects and later reconnects.
- Channel assignment is unique among simultaneously active selected devices because the
  assigned channel identifies the return route.
- The virtual MIDI device is one shared application endpoint for external applications.
- The initial modification applies only to channel-bearing MIDI messages; other message
  data is preserved.
- Device selection remains the control determining which physical devices are actively
  routed; bulk routing of unselected devices is out of scope.
- Messages arriving from the virtual device are expected to use the assigned channel to
  identify their destination.
- Additional transformations beyond channel replacement are out of scope for this
  iteration but the routing pipeline must allow them to be added later.
- Existing device discovery, selection, status, and persistence conventions remain the
  source of truth for those concerns.
