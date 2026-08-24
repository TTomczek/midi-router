# Data Model: Minimize to Tray

## ApplicationSettings

The existing persisted application settings record is extended with:

| Field | Type | Required | Default | Description |
|-------|------|----------|---------|-------------|
| `AppearanceMode` | existing appearance mode | Yes | OS default | Existing appearance preference; must remain unchanged. |
| `MinimizeToTray` | Boolean | Yes | `false` | Whether minimizing the main window hides it from the taskbar and keeps it available from the tray. |

### Validation and compatibility

- `MinimizeToTray` accepts only a boolean value when read from persisted settings.
- Missing `MinimizeToTray` values deserialize as `false` for compatibility with existing files.
- Invalid or unreadable settings use the disabled fallback and report the failure through the
  established diagnostic path.
- Saving the new value must preserve the current appearance value.
- Writes continue to use the existing safe persistence behavior rather than partially replacing
  the settings file.

## Tray Icon

| Attribute | Description |
|-----------|-------------|
| Identity | The single identifiable MIDI Router notification-area icon. |
| Visibility | Present while the application is running; hidden and disposed during normal shutdown. |
| Left-click action | Restores the existing main window without creating another window. |
| Right-click action | Opens the tray context menu. |

## Tray Context Menu

The menu contains a stop/exit action that invokes normal application shutdown. Dismissing the
menu leaves the application running and does not restore the window.

## State Transitions

```text
Visible window
  ├─ minimize-to-tray enabled + minimize → Hidden in tray
  └─ minimize-to-tray disabled + minimize → Minimized taskbar window

Hidden in tray
  ├─ single left-click tray icon → Visible window
  ├─ right-click + dismiss → Hidden in tray
  └─ right-click + stop → Stopped, tray icon removed
```
