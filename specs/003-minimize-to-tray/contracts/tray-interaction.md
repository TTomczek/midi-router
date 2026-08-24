# Tray Interaction Contract

## Scope

This contract defines the observable Windows desktop behavior for the MIDI Router tray
feature. It is consumed by users and by the main window lifecycle.

## Setting

The settings interface exposes one `Minimize to tray` toggle with two states:

- Enabled: the minimize button hides the window and keeps the application running in the
  notification area.
- Disabled: the minimize button performs ordinary Windows taskbar minimization.

Changing the toggle takes effect for subsequent minimize actions and is persisted for the local
user.

## Notification-Area Interactions

| User action | Required outcome |
|-------------|------------------|
| Minimize button while enabled | Window is hidden; no taskbar entry remains; application continues running. |
| One left click on tray icon | Existing main window is shown, normal, active, and usable. |
| Right click on tray icon | Context menu opens with a stop action. |
| Select stop action | Normal application shutdown occurs and the tray icon is removed. |
| Dismiss context menu | Application remains running and in its prior hidden state. |
| Minimize button while disabled | Window remains represented in the Windows taskbar as a normal minimized window. |

## Failure Behavior

If the setting cannot be loaded, the application starts with minimize-to-tray disabled and
reports the load failure. If it cannot be saved, the current session continues using the
selected value and reports the save failure.
