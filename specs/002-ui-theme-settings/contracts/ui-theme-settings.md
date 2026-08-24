# UI Contract: Theme Settings

## Entry Point

- The main window exposes one gear icon in the upper-left corner.
- The icon has an accessible name such as `Settings` and can be activated by pointer or
  keyboard.
- Activating it opens a settings menu anchored to the icon and leaves the main view usable.

## Appearance Choices

The menu exposes exactly three mutually exclusive choices:

| Label | Semantic value | Selected behavior |
|---|---|---|
| Light | `Light` | Applies the light palette immediately |
| Dark | `Dark` | Applies the dark palette immediately |
| OS default | `OsDefault` | Applies the current OS palette and follows later OS changes |

The current choice is visibly selected while the menu is open. Selecting a choice updates
the menu state and persists the semantic value.

## Persistence Contract

- The settings store reads the per-user JSON file during application startup.
- Missing or invalid appearance data returns `OsDefault`.
- A write failure does not terminate the application and is reported through the established
  diagnostic/status path.
- The persisted semantic value is restored on the next normal startup.

## Theme Resource Contract

Both palettes provide equivalent named resources for the main window, device list, status
text, settings menu, controls, borders, and interaction states. Resource names are shared
between palettes so switching does not require view-specific branching. Text and primary
controls remain readable in either palette.

## Isolation Contract

Theme changes may update visual resources and settings status only. They must not start,
stop, refresh, or otherwise alter MIDI device discovery, routing, tray behavior, or window
minimization behavior.
