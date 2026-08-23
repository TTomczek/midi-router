# Data Model: Input MIDI Device Browser UI

## Entity: InputMidiDevice

- **Purpose**: Represents one discoverable input MIDI endpoint shown in the UI.
- **Fields**:
  - `name` (string, required): Human-readable device name from the MIDI backend.
  - `version` (enum, required): `Midi1` or `Midi2`, determined from the Windows MIDI Services endpoint format.
  - `availabilityStatus` (enum, required): `Available` for currently discoverable devices in the active result set.
- **Validation Rules**:
  - `name` must be non-empty after trimming.
  - Duplicate names are allowed because multiple physical/virtual devices can share the same label.

## Entity: DeviceListViewState

- **Purpose**: Captures the state of the device list screen.
- **Fields**:
  - `devices` (collection of `InputMidiDevice`)
  - `deviceCount` (integer)
  - `statusMessage` (string)
  - `state` (enum): `Loading`, `Loaded`, `Empty`, `Error`
- **Validation Rules**:
  - `deviceCount` must equal `devices` collection size.
  - `state = Empty` requires `devices` to be empty.
  - `state = Error` requires a user-facing status message describing failure and retry path.

## State Transitions

1. `Loading` -> `Loaded` when device discovery succeeds with one or more devices.
2. `Loading` -> `Empty` when discovery succeeds with zero devices.
3. `Loading` -> `Error` when discovery fails.
4. Any terminal state (`Loaded`, `Empty`, `Error`) -> `Loading` when user triggers refresh.
