---

description: "Task list for implementing minimize-to-tray behavior"
---

# Tasks: Minimize to Tray

**Input**: Design documents from `/specs/003-minimize-to-tray/`

**Prerequisites**: `plan.md`, `spec.md`, `research.md`, `data-model.md`,
`contracts/tray-interaction.md`, `quickstart.md`

**Tests**: Required by the project constitution. Write each behavior test first and confirm it
fails before implementing the corresponding behavior.

**Organization**: Tasks are grouped by user story to preserve independently testable increments.

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Confirm the existing project supports the planned Windows desktop integration.

- [X] T001 [P] Verify WPF and Windows Forms support remains enabled in `midi-router.csproj`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Establish compatible persisted settings and deterministic settings coordination before
window lifecycle work begins.

**Checkpoint**: Shared settings can load, update, and save appearance and tray preferences
without overwriting either value.

- [X] T002 [P] Add failing settings round-trip, missing-field default, and malformed-data tests in `midi-router.Tests/JsonSettingsStoreTests.cs`
- [X] T003 [P] Add failing tests for preserving both preferences and reporting load/save failures in `midi-router.Tests/MinimizeToTraySettingsTests.cs`
- [X] T004 Extend `ApplicationSettings` with the disabled-by-default `MinimizeToTray` field in `ApplicationSettings.cs`
- [X] T005 Update `JsonSettingsStore` serialization and normalization to preserve appearance, default missing tray values to disabled, and handle invalid values in `JsonSettingsStore.cs`
- [X] T006 Update settings coordination to load and save tray preference without overwriting appearance settings or swallowing diagnostics in `ThemeManager.cs` and `App.xaml.cs`

---

## Phase 3: User Story 1 - Minimize the Application to the Tray (Priority: P1) MVP

**Goal**: When enabled, minimizing hides the main window from the taskbar while the application
continues running with its tray icon available.

**Independent Test**: Enable the preference, minimize the window, and verify the hidden state,
tray availability, and uninterrupted background operation.

### Tests for User Story 1

- [X] T007 [P] [US1] Add failing enabled and disabled minimize-policy tests in `midi-router.Tests/MainWindowTrayTests.cs`

### Implementation for User Story 1

- [X] T008 [US1] Load the persisted tray preference during startup and pass the settings state to the main window in `App.xaml.cs` and `MainWindow.xaml.cs`
- [X] T009 [US1] Gate `OnStateChanged` so only the enabled preference hides the window, while disabled mode keeps normal taskbar minimization in `MainWindow.xaml.cs`
- [X] T010 [US1] Ensure the identifiable `NotifyIcon` remains available during hidden tray operation and is disposed only during normal shutdown in `MainWindow.xaml.cs`

**Checkpoint**: User Story 1 is independently testable with enabled and disabled minimize behavior.

---

## Phase 4: User Story 2 - Restore the Application from the Tray (Priority: P1)

**Goal**: A single left click on the tray icon restores the existing main window without
duplicating it or changing MIDI activity.

**Independent Test**: Minimize to the tray, left-click once, and verify the same window is visible,
normal, active, and usable.

### Tests for User Story 2

- [X] T011 [P] [US2] Add failing single-left-click restoration and no-duplicate-window contract tests in `midi-router.Tests/MainWindowTrayTests.cs`
- [X] T012 [P] [US2] Add a failing XAML/UI contract assertion for the tray-related setting entry in `midi-router.Tests/MainWindowThemeTests.cs`

### Implementation for User Story 2

- [X] T013 [US2] Wire the tray icon's single left-click event to restore the existing window and remove double-click-only restoration in `MainWindow.xaml.cs`
- [X] T014 [US2] Preserve window state, device monitoring, and routing state across tray restoration in `MainWindow.xaml.cs`

**Checkpoint**: User Stories 1 and 2 work independently; minimize and one-click restore are
complete.

---

## Phase 5: User Story 3 - Control Tray Behavior and Exit (Priority: P1)

**Goal**: Users can change and persist the tray preference, use normal taskbar minimization
when disabled, and stop the application from the tray menu.

**Independent Test**: Toggle the setting, restart, verify both minimize modes, and select the
tray stop action.

### Tests for User Story 3

- [X] T015 [P] [US3] Add failing settings-menu discoverability and enabled-state persistence assertions in `midi-router.Tests/MainWindowThemeTests.cs` and `midi-router.Tests/MinimizeToTraySettingsTests.cs`
- [X] T016 [P] [US3] Add failing tray context-menu stop and cleanup tests in `midi-router.Tests/MainWindowTrayTests.cs`

### Implementation for User Story 3

- [X] T017 [US3] Add a two-state `Minimize to tray` control to the existing settings menu in `MainWindow.xaml`
- [X] T018 [US3] Add setting-change handling that updates the session behavior and persists the selected value while retaining appearance settings in `MainWindow.xaml.cs` and `App.xaml.cs`
- [X] T019 [US3] Keep the tray context menu stop action on the normal close path and guarantee icon/menu cleanup in `MainWindow.xaml.cs`
- [X] T020 [US3] Surface tray-setting load and save failures through the established diagnostic mechanism without stopping the application in `App.xaml.cs` and `MainWindow.xaml.cs`

**Checkpoint**: All three user stories are independently functional and the persisted setting
survives restart.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Complete documentation and repository-level validation.

- [X] T021 [P] Document minimize-to-tray behavior, setting defaults, restore interaction, and tray stop action in `README.md`
- [X] T022 Run the automated build and test gates from the repository root with `dotnet build` and `dotnet test`
- [X] T023 Review the manual scenarios in `specs/003-minimize-to-tray/quickstart.md` for interactive Windows validation, including both setting states and failure-path behavior
- [X] T024 Review changed files for hardware-independent tests, preserved MIDI behavior, explicit lifecycle cleanup, and compliance with `specs/003-minimize-to-tray/contracts/tray-interaction.md`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies.
- **Foundational (Phase 2)**: Depends on Setup and blocks all user stories.
- **User Story 1 (Phase 3)**: Depends on Foundational; delivers the MVP.
- **User Story 2 (Phase 4)**: Depends on User Story 1 because it completes the same tray lifecycle.
- **User Story 3 (Phase 5)**: Depends on Foundational and integrates the lifecycle from User Stories 1 and 2.
- **Polish (Phase 6)**: Depends on all desired user stories.

### Within Each User Story

- Write and run the relevant tests before implementation.
- Complete shared settings work before window lifecycle integration.
- Keep tasks touching `MainWindow.xaml.cs` sequential.
- Run the story checkpoint before moving to the next story.

### Parallel Opportunities

- T002 and T003 can run in parallel because they initially affect separate test files.
- T007 can run in parallel with no implementation task until its test failure is observed.
- T011 and T012 can run in parallel because they affect separate test files.
- T015 and T016 can run in parallel because they affect separate test files.
- T021 can run in parallel with final code review after implementation is complete.

## Implementation Strategy

### MVP First

1. Complete Setup and Foundational phases.
2. Complete User Story 1, including failing tests and conditional minimize behavior.
3. Validate that enabled mode hides the window and disabled mode uses the taskbar.

### Incremental Delivery

1. Add User Story 2 for single-click restoration.
2. Add User Story 3 for the setting UI, persistence, stop action, and diagnostics.
3. Run the complete quickstart and repository quality gates.

### Traceability

| User story | Main requirements | Primary files |
|------------|-------------------|---------------|
| US1 | FR-003, FR-004, FR-005, FR-009, FR-013 | `ApplicationSettings.cs`, `JsonSettingsStore.cs`, `App.xaml.cs`, `MainWindow.xaml.cs` |
| US2 | FR-006, FR-013 | `MainWindow.xaml.cs`, `midi-router.Tests/MainWindowTrayTests.cs` |
| US3 | FR-001, FR-002, FR-007, FR-008, FR-010, FR-011, FR-012 | `MainWindow.xaml`, `MainWindow.xaml.cs`, `App.xaml.cs`, settings tests |

## Notes

- Every task uses the required checkbox, sequential ID, optional parallel marker, story label
  where applicable, and concrete repository file path.
- No new dependency or physical MIDI hardware is required.
- The task list intentionally leaves checklist files unchanged; those are reviewer-owned gates.
