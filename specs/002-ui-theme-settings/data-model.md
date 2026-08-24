# Data Model: UI Theme Settings

## Appearance Mode

Represents the user's semantic appearance preference.

| Value | Meaning | Resolution |
|---|---|---|
| `Light` | Always use the light palette | Light |
| `Dark` | Always use the dark palette | Dark |
| `OsDefault` | Follow the operating system | Current OS preference |

Only these three values are valid. Unknown, missing, or malformed persisted values resolve
to `OsDefault`.

## Application Settings

Represents the persisted per-user preferences.

| Field | Type | Rules |
|---|---|---|
| `AppearanceMode` | Appearance Mode | Required in memory; defaults to `OsDefault` when absent or invalid |

The file contains only user preferences owned by this feature and is written using a
replace-safe operation so a failed write does not silently produce a partial settings file.

## Resolved Theme

Represents the palette currently applied to the application.

| Field | Type | Rules |
|---|---|---|
| `Palette` | `Light` or `Dark` | Derived from `AppearanceMode` and the current OS preference |
| `Source` | Appearance Mode | The selected mode, used to determine whether OS changes should reapply |

## State Transitions

```text
Startup -> Load settings
Load settings -> Apply explicit Light/Dark
Load settings -> Resolve OS default -> Apply Light/Dark
User selects mode -> Persist mode -> Apply resolved palette
OS preference changes while OsDefault -> Apply new resolved palette
Read/write failure -> Keep running -> Use OsDefault when resolution is unavailable
Application shutdown -> Unsubscribe OS notifications -> Release resources
```

Selecting an explicit mode ignores subsequent OS preference changes until the user selects
OS default again.
