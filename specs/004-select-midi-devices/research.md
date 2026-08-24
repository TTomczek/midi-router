# Research: Select MIDI Devices

## Decision: Use the existing endpoint device ID as the unique selection key

**Rationale**: `MidiInputDevice` already exposes `EndpointDeviceId`, and
`WindowsMidiInputDeviceProvider` uses it as the dictionary key. It is therefore already
available to both discovery and UI state and remains distinct when display names collide.

**Alternatives considered**: Display name was rejected because names are not unique.
Generating a new application-local identifier was rejected because it would not survive
device refreshes or reconnects.

## Decision: Extend the existing application settings document

**Rationale**: `JsonSettingsStore` already provides atomic JSON persistence and
`ApplicationSettings` already carries user preferences. Adding the selected unique device
ID set preserves existing appearance and tray settings and avoids a second settings file.

**Alternatives considered**: A separate selection file was rejected because it duplicates
the established persistence path and complicates consistency and error reporting.

## Decision: Keep selection reconciliation in the device view model

**Rationale**: `MidiDeviceMonitor` publishes complete snapshots and is intentionally focused
on device discovery. `MidiInputDeviceViewModel` already owns the observable device list and
dispatches snapshot updates to WPF, so it can apply selection state without coupling the
provider to presentation concerns.

**Alternatives considered**: Putting selection in the provider was rejected because it would
mix user preference state with hardware enumeration. A UI-only selection was rejected
because later processing needs a stable, non-visual selected set.

## Decision: Retain disconnected IDs for reconnect restoration

**Rationale**: Persisted selection represents user intent, while the visible list represents
current availability. Keeping an unavailable unique ID allows the same device to regain its
selection when it reconnects without displaying unavailable devices.

**Alternatives considered**: Removing IDs immediately on disconnect was rejected because it
would force users to reselect devices after temporary disconnections.
