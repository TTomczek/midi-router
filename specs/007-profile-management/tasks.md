---

description: "Task list for implementing local MIDI profile management"
---

# Tasks: Profile Management

**Input**: Design documents from `/specs/007-profile-management/`

**Prerequisites**: `plan.md`, `spec.md`, `research.md`, `data-model.md`, `contracts/profile-ui.md`, `quickstart.md`

**Tests**: Required by the project constitution. Write tests first and use hardware-free fakes.

**Organization**: Tasks are grouped by user story; foundational profile infrastructure is completed before story work.

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Establish the files and composition points needed for profile work.

- [X] T001 [P] Add profile source and test file entries to `midi-router.csproj` and `midi-router.Tests/midi-router.Tests.csproj` only where project configuration requires explicit inclusion
- [X] T002 [P] Document profile storage location, switching behavior, and duplicate-name labels in `README.md`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Build the profile domain, persistence boundary, and active-state plumbing required by every user story.

**⚠️ CRITICAL**: No user story work can begin until this phase is complete.

- [X] T003 [P] Define the profile entity, stable identifier, date field, and profile-owned device/channel state in `Profile.cs`
- [X] T004 [P] Define injectable per-profile storage operations for listing, loading, saving, and deleting profile files in `IProfileStore.cs`
- [X] T005 [P] Add profile directory resolution, JSON serialization, atomic writes, normalization, malformed-file handling, and per-file deletion in `JsonProfileStore.cs`
- [X] T006 Add profile collection loading, active-profile tracking, deterministic ordering, duplicate display-label generation, minimum-one-profile enforcement, and error reporting in `ProfileManager.cs`
- [X] T007 Extend `ApplicationSettings.cs` and `ApplicationSettingsCoordinator.cs` with global active-profile persistence while keeping appearance and tray settings global
- [X] T008 Refactor `MidiInputDeviceViewModel.cs` to read and persist selected devices and channel assignments through the active profile without changing existing device-monitor behavior
- [X] T009 Compose `JsonProfileStore`, `ProfileManager`, and the active-profile-aware device view model during startup in `App.xaml.cs`
- [X] T010 [P] Write failing domain and persistence tests for profile normalization, separate-file round trips, stable IDs, and last-edited values in `midi-router.Tests/ProfileTests.cs` and `midi-router.Tests/JsonProfileStoreTests.cs`
- [X] T011 [P] Write failing manager tests for initialization, active-profile restoration, deterministic duplicate labels, persistence failures, and legacy settings migration in `midi-router.Tests/ProfileManagerTests.cs`
- [X] T012 Run the foundational tests, confirm they fail for the missing behavior, then implement the smallest changes in `Profile.cs`, `JsonProfileStore.cs`, and `ProfileManager.cs` needed to make them pass

**Checkpoint**: Profile state can be loaded independently of hardware, saved one file per profile, and exposed as the active configuration.

---

## Phase 3: User Story 1 - Switch Between Profiles (Priority: P1) 🎯 MVP

**Goal**: Let users select a saved profile beside Settings and immediately apply its device/channel assignments.

**Independent Test**: With two saved profiles containing different assignments, select each profile and verify the active device/channel state changes and survives restart.

### Tests for User Story 1

- [X] T013 [P] [US1] Add failing manager/view-model integration tests for switching profiles and applying selected devices and channel assignments in `midi-router.Tests/ProfileManagerTests.cs` and `midi-router.Tests/MidiInputDeviceViewModelTests.cs`
- [X] T014 [P] [US1] Add failing layout assertions for the profile selector placement and selected-profile binding beside Settings in `midi-router.Tests/MainWindowLayoutTests.cs`

### Implementation for User Story 1

- [X] T015 [US1] Implement active-profile switching notifications and application of the target profile's device/channel state in `ProfileManager.cs` and `MidiInputDeviceViewModel.cs`
- [X] T016 [US1] Add the profile selector immediately left of the Settings button, including the create entry, visible labels, and active selection binding, in `MainWindow.xaml`
- [X] T017 [US1] Wire profile selection, startup synchronization, and profile-change refresh behavior in `MainWindow.xaml.cs`
- [X] T018 [US1] Make device and channel edits update the active profile's last-edited date and save through the profile manager in `MidiInputDeviceViewModel.cs`

**Checkpoint**: User Story 1 is independently functional: profiles are selectable, applied, persisted, and restored.

---

## Phase 4: User Story 2 - Create and Edit Profiles (Priority: P1)

**Goal**: Create empty profiles and rename profiles inline from the drop-down.

**Independent Test**: Select the first entry, create a named empty profile with Enter, right-click it, choose Rename, and verify both operations persist.

### Tests for User Story 2

- [X] T019 [P] [US2] Add failing manager tests for trimmed-name validation, empty-profile creation, rename commit/cancel, last-edited updates, and duplicate-name identity preservation in `midi-router.Tests/ProfileManagerTests.cs`
- [X] T020 [P] [US2] Add failing UI contract/layout assertions for the styled create/rename dialog and profile context-menu behavior in `midi-router.Tests/MainWindowLayoutTests.cs`

### Implementation for User Story 2

- [X] T021 [US2] Implement create, rename, trimmed-name validation, commit/cancel state, and last-edited persistence operations in `ProfileManager.cs`
- [X] T022 [US2] Implement the generally styled profile-name dialog for first-entry creation with Enter/confirm/cancel handling in `ProfileNameDialog.cs`, `MainWindow.xaml`, and `MainWindow.xaml.cs`
- [X] T023 [US2] Implement right-click Rename context-menu handling through the generally styled name dialog with validation feedback and commit/cancel handling in `ProfileNameDialog.cs` and `MainWindow.xaml.cs`
- [X] T024 [US2] Refresh visible duplicate-name labels and active selection after create or rename in `ProfileManager.cs` and `MainWindow.xaml.cs`

**Checkpoint**: User Stories 1 and 2 are independently functional: users can create, name, rename, switch, and persist profiles.

---

## Phase 5: User Story 3 - Remove Profiles Safely (Priority: P2)

**Goal**: Delete confirmed profiles while preventing deletion of the final remaining profile.

**Independent Test**: With two profiles, cancel and confirm deletion through the dialog; then verify the final profile has no usable delete action.

### Tests for User Story 3

- [X] T025 [P] [US3] Add failing manager tests for confirmed deletion, cancellation, active-profile fallback, per-file removal, and rejection of final-profile deletion in `midi-router.Tests/ProfileManagerTests.cs`
- [X] T026 [P] [US3] Add failing UI assertions for profile context-menu actions, confirmation dialog invocation, and absent final Delete action in `midi-router.Tests/MainWindowLayoutTests.cs`

### Implementation for User Story 3

- [X] T027 [US3] Implement confirmed deletion, profile-file removal, active-profile fallback, and minimum-one-profile enforcement in `ProfileManager.cs` and `JsonProfileStore.cs`
- [X] T028 [US3] Add Rename and Delete context-menu actions to each profile option and hide/disable Delete when only one profile remains in `MainWindow.xaml`
- [X] T036 [US3] Prevent the create-profile entry from opening a context menu on right-click in `MainWindow.xaml` and `MainWindow.xaml.cs`
- [X] T037 [US3] Make the entire existing profile entry horizontal area right-clickable by stretching and hit-testing its item template in `MainWindow.xaml`
- [X] T029 [US3] Implement confirmation dialog handling, cancellation, deletion errors, and post-delete selection refresh in `MainWindow.xaml.cs`

**Checkpoint**: All user stories are independently functional and safe against accidental final-profile deletion.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Complete quality gates and prove the documented end-to-end behavior.

- [X] T030 [P] Add profile persistence and switching diagnostics using the existing logging/status mechanisms in `ProfileManager.cs` and `MainWindow.xaml.cs`
- [X] T031 [P] Extend `midi-router.Tests/JsonSettingsStoreTests.cs` and `midi-router.Tests/ApplicationSettingsCoordinatorTests.cs` for active-profile persistence and compatibility with existing global settings
- [X] T035 [P] Verify remembered-profile restoration and first-profile fallback when the saved active profile is unavailable in `midi-router.Tests/ProfileManagerTests.cs`
- [X] T038 [US1] Ensure startup selection is assigned from the loaded active profile after the selector items and value path are initialized in `MainWindow.xaml.cs`
- [X] T039 [US1] Preserve the active profile selection when profile state updates refresh profile items in `ProfileManager.cs` and `MainWindow.xaml.cs`
- [X] T032 [P] Review `MainWindow.xaml` and `MainWindow.xaml.cs` for keyboard accessibility names, focus behavior, and theme-resource consistency for all profile controls
- [X] T033 Run the scenarios in `specs/007-profile-management/quickstart.md` and resolve any discrepancies in the implementation or documentation
- [X] T034 Run `dotnet build` and `dotnet test` from the repository root and address failures caused by the profile feature

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies; T001 and T002 can run in parallel.
- **Foundational (Phase 2)**: Depends on Setup; T003-T005 and T010-T011 can begin in parallel, while T006-T009 and T012 depend on the domain/store decisions and tests.
- **User Stories (Phases 3-5)**: Depend on the foundational checkpoint. US1 is the MVP; US2 and US3 can proceed in parallel after foundational work if shared UI files are coordinated.
- **Polish (Phase 6)**: Depends on the desired user stories being complete.

### User Story Dependencies

- **User Story 1 (P1)**: Depends on Phase 2; no dependency on another user story.
- **User Story 2 (P1)**: Depends on Phase 2 and uses the selector created by US1; implementation should preserve US1 switching behavior.
- **User Story 3 (P2)**: Depends on Phase 2 and uses the selector created by US1; implementation should preserve US1/US2 state and labels.

### Within Each User Story

- Write tests first and confirm they fail.
- Implement manager/domain behavior before WPF event wiring.
- Complete persistence and application integration before declaring the story complete.

## Parallel Execution Examples

### Foundational

```text
Task T003: Define Profile.cs
Task T004: Define IProfileStore.cs
Task T010: Write ProfileTests.cs and JsonProfileStoreTests.cs
Task T011: Write ProfileManagerTests.cs
```

### User Story 1

```text
Task T013: Write switching integration tests
Task T014: Write profile-selector layout tests
```

### User Story 2

```text
Task T019: Write create/rename manager tests
Task T020: Write inline-edit UI tests
```

### User Story 3

```text
Task T025: Write deletion lifecycle tests
Task T026: Write delete-control UI tests
```

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Setup and Foundational phases.
2. Complete US1 switching, active-state application, and persistence.
3. Run the US1 independent test and stop for an MVP demonstration.

### Incremental Delivery

1. Add US2 creation and rename while retaining US1 switching.
2. Add US3 confirmation-based deletion and final-profile protection.
3. Complete cross-cutting diagnostics, accessibility, quickstart validation, build, and full tests.

### Format Validation

All tasks use the required `- [ ] T### [P?] [US#?] description with file path` format. Setup, foundational, and polish tasks intentionally omit story labels; story-phase tasks include `[US1]`, `[US2]`, or `[US3]`.
