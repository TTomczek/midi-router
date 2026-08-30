# Profile Drop-down UI Contract

## Placement and contents

- The active-profile drop-down is immediately to the left of the Settings button in the main window header.
- The first entry is a create-profile action, followed by one entry per saved profile.
- The create-profile entry does not open a context menu.
- Existing profile entries display their derived visible label and a context menu with Rename and Delete actions.
- The entire horizontal area of an existing profile entry is right-clickable, not only the name text.
- The final remaining profile has no Delete action.

## Create interaction

1. Selecting the first entry opens a generally styled profile-name dialog.
2. Confirming the dialog with a trimmed non-empty value creates and selects a new empty profile.
3. Empty or whitespace-only values do not create a profile and provide validation feedback.
4. Escape or closing the dialog without confirmation cancels creation and restores the create entry.

## Rename interaction

1. Right-clicking an existing profile entry and selecting Rename opens a generally styled profile-name dialog containing the current name.
2. Confirming the dialog with a trimmed non-empty value renames the same profile and restores its derived label.
3. Escape, closing without confirmation, or invalid input leaves the existing name unchanged.

## Delete interaction

1. Right-clicking a profile and selecting Delete opens a confirmation dialog naming the profile to be deleted.
2. Confirming removes the profile and its saved data; cancelling leaves it unchanged.
3. The UI never shows or enables a Delete action, confirmation, or deletion action for the final remaining profile.

## Selection behavior

- Selecting an existing profile closes the drop-down, marks it active, and applies its selected devices and channel assignments.
- Labels remain distinguishable when names duplicate, while renaming does not alter profile identity.
- Deleting the active profile selects the previous list entry when available; otherwise it selects the following entry.
- The last selected profile is restored on restart; if it no longer exists, the first listed profile is selected.
- After startup, the drop-down's displayed selection must match the restored active profile rather than the create entry or an empty value.
- Updating device or channel assignments must not clear the active profile selection.
