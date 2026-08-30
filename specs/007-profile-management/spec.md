# Feature Specification: Profile Management

**Feature Branch**: `007-profile-management`
**Created**: 2026-08-30
**Status**: Draft
**Input**: User description: "Add profiles to the application. A profile consists of a human readable name, the selected device, their assigned channels and a last edited date. Every profile should be saved in seperate file. The profiles should be able to be switched between different users. The active profile is selectable by a drop down, showing the name of the profile, on the left of the settings button. If there are multiple profiles with the same name, the application adds an ongoing number in the visual representation of the profile in the drop down. The first entry in the drop down lets the user create a new empty profile. Clicking this option transform the option to an input field to enter the profiles name. Pressing enter creates the profile. Double clicking an existing profile lets the user rename a profile by turning the profile option to an input field like with the profile creation. A - button on the left of each profile option in the drop down lets the user delete a profile after confirming a dialog. The last remaining profile can't be deleted."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Switch Between Profiles (Priority: P1)

As a user, I want to select a saved profile so that I can quickly change between different users' device and channel assignments.

**Why this priority**: Switching profiles is the primary value of the feature and must work with existing saved configurations.

**Independent Test**: Create two profiles with different device and channel assignments, select each from the drop-down, and confirm the application reflects the selected profile.

**Acceptance Scenarios**:

1. **Given** multiple saved profiles exist, **When** the user opens the profile drop-down, **Then** the profiles are listed by human-readable name and the active profile is selected.
2. **Given** two profiles have different assignments, **When** the user selects the second profile, **Then** its selected device and assigned channels become active.
3. **Given** a profile has been saved, **When** the application is restarted, **Then** that profile and its assignments remain available.

---

### User Story 2 - Create and Edit Profiles (Priority: P1)

As a user, I want to create an empty profile and rename existing profiles so that profiles can represent different people or setups.

**Why this priority**: Users need a simple way to establish and maintain named configurations before they can benefit from switching.

**Independent Test**: Use the first drop-down entry to create a profile, enter a name, then right-click it, choose Rename, and enter a different name.

**Acceptance Scenarios**:

1. **Given** the profile drop-down is open, **When** the user selects the first entry, **Then** that entry becomes an input field for a profile name.
2. **Given** the profile-name dialog is open, **When** the user enters a non-empty name and presses Enter or confirms the dialog, **Then** a new empty profile is created, saved, and selected.
3. **Given** an existing profile is listed, **When** the user right-clicks it and chooses Rename, **Then** a profile-name dialog opens containing its current name.
4. **Given** the rename dialog is open, **When** the user enters a non-empty name and presses Enter or confirms the dialog, **Then** the profile is renamed and saved.

---

### User Story 3 - Remove Profiles Safely (Priority: P2)

As a user, I want to delete obsolete profiles while being protected from accidentally deleting the final configuration.

**Why this priority**: Profile cleanup is useful, but safe deletion is less central than creating and switching profiles.

**Independent Test**: Create at least two profiles, delete one through its context menu and confirmation dialog, then attempt to delete the remaining profile.

**Acceptance Scenarios**:

1. **Given** at least two profiles exist, **When** the user right-clicks a profile and chooses Delete, **Then** a confirmation dialog asks whether to delete that profile.
2. **Given** the delete confirmation is shown, **When** the user confirms, **Then** the profile is removed from the drop-down and its saved data is removed.
3. **Given** exactly one profile exists, **When** the user opens its context menu, **Then** no Delete action is shown or enabled and the profile cannot be deleted.
4. **Given** the delete confirmation is shown, **When** the user cancels, **Then** the profile remains unchanged.
5. **Given** the active profile is deleted and profiles remain before and after it, **When** deletion is confirmed, **Then** the previous profile in list order becomes active.
6. **Given** the active profile is the first profile and is deleted, **When** deletion is confirmed, **Then** the following profile becomes active.

### Edge Cases

- Names containing leading or trailing whitespace are stored and displayed without surrounding whitespace; a name containing only whitespace is rejected and no profile is created or renamed.
- Multiple profiles may have the same underlying name. Their drop-down labels receive sequential, one-based suffixes such as “Name (2)” and “Name (3)” so each visible option is distinguishable.
- If a selected device is no longer available, the profile remains saved, the unavailable selection is surfaced through the existing device/status behavior, and switching to another profile remains possible.
- If a profile file cannot be read or written, the user receives an explicit error and the last successfully saved profile data is not silently replaced.
- Pressing Escape or closing a creation/rename dialog without confirming cancels that edit without creating or renaming a profile.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST maintain at least one profile at all times.
- **FR-002**: Each profile MUST contain a human-readable name, its selected device, its assigned channels, and a last edited date.
- **FR-003**: The system MUST save each profile in its own separate persistent file.
- **FR-004**: The system MUST restore all saved profiles and the previously active profile when the application starts.
- **FR-004a**: The system MUST persist the active profile whenever the user selects a profile; if the remembered profile is unavailable at startup, the system MUST select and persist the first profile in list order.
- **FR-004b**: After startup, the profile drop-down MUST display the restored active profile as its selected entry.
- **FR-004c**: Updating a profile's device or channel assignments MUST preserve the active profile selection shown in the drop-down.
- **FR-005**: The system MUST provide an active-profile drop-down immediately to the left of the settings button.
- **FR-006**: The drop-down MUST show each profile's human-readable name and mark the active profile as selected.
- **FR-007**: When multiple profiles share a name, the drop-down MUST append sequential one-based numbering to their visible labels while retaining the original name as the profile name.
- **FR-008**: The first drop-down entry MUST initiate creation of a new empty profile when selected.
- **FR-008a**: The first create-profile entry MUST NOT open a profile context menu on right-click.
- **FR-008b**: Right-clicking anywhere within an existing profile entry, including its empty horizontal space, MUST open that profile's context menu.
- **FR-009**: Profile creation MUST open a generally styled name dialog from the first entry and MUST create and select the profile only after the user enters a non-empty trimmed name and confirms it.
- **FR-010**: A user MUST be able to right-click an existing profile option and choose Rename to open a generally styled name dialog containing its current name.
- **FR-011**: Profile renaming MUST persist the new non-empty trimmed name when the dialog is confirmed, and MUST leave the profile unchanged when the dialog is cancelled.
- **FR-012**: Each existing profile option MUST provide a context menu with Rename and Delete actions; Delete MUST initiate confirmation when more than one profile exists, and when only one profile remains no Delete action may be shown or enabled.
- **FR-013**: The system MUST delete a profile and its separate saved file only after the user confirms the deletion.
- **FR-013a**: When the active profile is deleted, the system MUST activate the previous profile in list order when one exists; otherwise it MUST activate the following profile.
- **FR-014**: When the active profile changes, the system MUST apply that profile's selected device and assigned channels to the application.
- **FR-015**: Changes to a profile's device or channel assignments, creation, and rename MUST update its last edited date and persist the profile.
- **FR-016**: The system MUST surface profile persistence errors to the user or through the application's established diagnostic/status mechanism and MUST preserve the last successfully saved data.

### Key Entities

- **Profile**: A named, switchable configuration for one user or setup, containing a selected device, assigned channels, and last edited date.
- **Profile Label**: The drop-down presentation of a profile name, including a sequential suffix when needed to distinguish duplicate names.
- **Channel Assignment**: The channels associated with a profile's selected device and routing configuration.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Users can switch from one saved profile to another and see the corresponding device and channel assignments applied within 2 seconds.
- **SC-002**: Users can create a named profile from the drop-down in under 30 seconds without leaving the main application view.
- **SC-003**: 100% of confirmed profile creations, renames, assignment changes, and deletions are reflected after an application restart.
- **SC-004**: 100% of attempts to delete the final remaining profile are prevented.
- **SC-005**: In usability testing, at least 90% of users can create, rename, switch, and delete a profile correctly on their first attempt.

## Assumptions

- Profiles are local application data and do not require account authentication or synchronization between machines.
- “Different users” means users of the same application installation can switch among saved profiles; profile ownership and permissions are out of scope.
- A new profile starts with no selected device and no assigned channels.
- Existing device and channel selection behavior is reused, including its handling of unavailable devices.
- Profile files are stored in the application's established user-data location and use a format that can represent all profile fields.
- The last edited date is displayed or otherwise available wherever the profile's details are shown, while the drop-down remains focused on the profile name.
