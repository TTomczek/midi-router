# Data Model: MIDI Device List

## MIDI Device

Represents one currently enumerated MIDI endpoint.

| Field | Type | Rules |
|---|---|---|
| `EndpointDeviceId` | String | Required; stable identity within the current watcher; unique in the overview. |
| `Name` | String | Required for display; use the SDK-provided user-respecting name. |
| `MidiVersion` | Enumeration | `Midi1`, `Midi2`, or `Unknown` when the source cannot classify it. |
| `Availability` | Enumeration | `Connected` while present in the watcher map; removed when disconnected. |

The endpoint ID, not the display name, defines uniqueness. Two endpoints with the same
name remain separate entries.

## Device Overview

Represents the view projection currently exposed to the user.

| Field | Type | Rules |
|---|---|---|
| `Devices` | Ordered set of MIDI Device | Contains each current endpoint at most once. |
| `State` | Enumeration | `Loading`, `Ready`, `Empty`, `Degraded`, or `Unavailable`. |
| `StatusMessage` | Optional String | User-readable for empty, degraded, or unavailable states. |

Ordering should be deterministic for a stable UI and tests; use the SDK's enumeration
order unless product requirements later define a user-selected sort.

## State Transitions

```text
Created -> Loading
Loading -> Ready       (enumeration completed with one or more endpoints)
Loading -> Empty       (enumeration completed with no endpoints)
Loading -> Unavailable (MIDI service unavailable or initial enumeration failure)
Ready -> Ready         (add/remove/update reconciled with endpoints remaining)
Ready -> Empty         (last endpoint removed)
Empty -> Ready         (endpoint added)
Ready/Empty -> Degraded (a change cannot be fully read; valid entries retained)
Degraded -> Ready/Empty (next successful reconciliation)
Any active state -> Unavailable (service/watch failure)
Any active state -> Stopped (application shutdown)
```

An event for an endpoint already present is idempotent. A removal for an unknown ID is
logged at debug level and does not remove another endpoint.
