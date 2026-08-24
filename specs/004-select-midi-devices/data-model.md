# Data Model: Select MIDI Devices

## MIDI Device

The existing `MidiInputDevice` record represents a currently discoverable input device.

| Field | Type | Rules |
|---|---|---|
| `EndpointDeviceId` | string | Required unique device ID; selection identity and provider key |
| `Name` | string | Required display name; not an identity key |
| `Version` | MIDI version | Existing MIDI 1, MIDI 2, or unknown classification |

## Device Selection State

The device selection model maps unique device IDs to a selected/unselected state.

| State | Meaning |
|---|---|
| Selected | The device ID is in the selected ID set and its visible row is highlighted |
| Unselected | The device ID is not in the selected ID set and its row uses normal styling |
| Unavailable | The device ID remains saved but is absent from the current device snapshot |

Selection is a set operation: selecting an unselected ID adds it; selecting a selected ID
removes it. Multiple IDs may be selected at once.

## Persisted Device Selection

`ApplicationSettings` carries the selected device ID set in the existing settings document.
The value is a collection of unique non-empty device IDs. Duplicate IDs are normalized away
on load and save. Invalid or unreadable settings do not prevent device enumeration; the
application reports the persistence issue and uses an empty selected set when no valid set
is available.

## Device Overview Snapshot

The existing snapshot remains the source of current availability:

- devices present in the snapshot are shown and receive selected state by ID;
- saved IDs absent from the snapshot are retained for reconnect restoration but are not shown;
- device additions and removals do not change the selected state of other IDs;
- duplicate device IDs are not valid and must not produce duplicate visible rows.

## State Transitions

```text
Unselected --row click--> Selected
Selected   --row click--> Unselected
Selected   --disconnect--> Unavailable (saved ID retained)
Unavailable --same ID reconnects--> Selected
Unavailable --application restart--> Selected only if same ID is available
```
