# Data Model: MIDI Device Activity Indicators

## MIDI Device Entry

Represents one currently listed MIDI input device.

| Field | Type | Rules |
|-------|------|-------|
| `EndpointDeviceId` | stable string | Required identity; unique within the current device list |
| `Name` | string | Existing display name; remains unchanged by activity |
| `Version` | MIDI version | Existing protocol label; remains unchanged by activity |
| `IsSelected` | boolean | Existing selection state; independent of activity |
| `IsActive` | boolean | False by default; true only during the recent-message window |

## Activity Indicator

The transient visual state associated with a MIDI Device Entry.

| State | Meaning | Transition |
|-------|---------|------------|
| Inactive | No recent message from this device | Initial state and expiration state |
| Active | A message from this device was recently received | Set by matching endpoint ID |

### State rules

- A message for endpoint ID `X` sets only entry `X` to Active.
- A subsequent message for `X` refreshes its expiration deadline.
- Expiration returns `X` to Inactive only if no newer message has arrived.
- Removing `X` removes its activity state and makes pending expiration harmless.
- Activity does not change selection, channel assignment, name, version, ordering, or persistence.

## Device List Container

The existing visual container that presents all current entries.

| Property | Rule |
|----------|------|
| Available width | Determines the usable width of the device-name region |
| Horizontal overflow | Must not produce a horizontal scrollbar |
| Long names | Must stay within the device-name region and remain identifiable |
| Empty list | Uses the existing explicit empty state without activity dots |

## Relationships

- A Device List Container contains zero or more MIDI Device Entries.
- Each MIDI Device Entry has exactly one Activity Indicator state.
- A received routing message references at most one entry through `SourceDeviceId`.
