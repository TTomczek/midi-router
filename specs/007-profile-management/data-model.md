# Profile Data Model

## Profile

Represents one user's or setup's MIDI configuration.

| Field | Type | Rules |
|---|---|---|
| `Id` | Stable identifier | Required, unique across profiles, unchanged by rename, safe to associate with one file |
| `Name` | String | Required after trimming; whitespace-only values are invalid; preserves duplicate names |
| `SelectedDeviceIds` | Collection of device identifiers | May be empty for a new profile; identifiers are unique and matched by ordinal identity |
| `DeviceChannelAssignments` | Map of device identifier to channel | Channel values are 1-16 at the profile boundary; invalid entries are rejected or normalized consistently with existing settings rules |
| `LastEdited` | Date/time | Required; updated on creation, rename, device selection, and channel assignment changes |

Each profile is persisted in its own JSON file named from `Id`. The file contains only profile-owned state; application-wide appearance and tray preferences remain in the global settings file.

## Profile Collection State

- The profile manager loads all valid profile files from the established local application-data profile directory.
- At least one profile exists after initialization. If no profile files exist, it creates and saves one initial profile, optionally seeded from legacy selected-device/channel settings.
- The active profile is identified by stable `Id`, not by display name.
- The active profile identifier is persisted in global application settings whenever selection changes so restart restores the previous profile; if that identifier is unavailable, the first profile in deterministic list order becomes active and replaces the stale value.
- Profile list order is deterministic (persisted creation/list order or stable identifier order) so duplicate numbering does not change unpredictably.

## Display Label

The visible label is derived from `Name` and the ordered profile list:

- The first profile with a given name is displayed as `Name`.
- Further profiles with that exact name are displayed as `Name (2)`, `Name (3)`, etc.
- The suffix is presentation-only and is never written into `Name`.

## State Transitions

| Operation | Preconditions | Result |
|---|---|---|
| Create | Trimmed name is non-empty | New empty profile is persisted, becomes active, and gets a current `LastEdited` |
| Rename | Existing profile; trimmed name is non-empty | Same `Id`, updated `Name` and `LastEdited`, persisted |
| Switch | Target profile is loaded | Target becomes active and its device/channel state is applied |
| Delete | More than one profile; user confirmed | Profile file is removed; if active, the previous list profile becomes active when available, otherwise the following profile becomes active and is applied |
| Delete final profile | Never permitted | No delete button is shown; no file or active state changes |
| Edit device/channels | Active profile exists | Profile state and `LastEdited` are updated and persisted |

## Error and Recovery Rules

- A failed profile read/write is surfaced through the existing status/diagnostic path.
- A failed save does not replace the last successfully persisted file or discard the last known good in-memory profile.
- A malformed or inaccessible profile file does not prevent other profiles from loading; the issue is reported and the application ensures a usable profile remains.
- An unavailable selected device remains in the profile by identifier, allowing reconnection restoration and explicit existing device-status reporting.
