---

description: "Executable task list for UI theme settings"
---

# Tasks: UI Theme Settings

**Input**: Design documents from `specs/002-ui-theme-settings/`

**Prerequisites**: `plan.md`, `spec.md`, `research.md`, `data-model.md`,
`contracts/ui-theme-settings.md`, and `quickstart.md`

**Tests**: Required by the project constitution. Write tests first and confirm they fail
before implementing each behavior.

## Phase 1: Setup

**Purpose**: Establish the feature's source and resource locations without adding projects or
new framework dependencies.

- [X] T001 [P] Create the `ThemeResources` directory and include it in `midi-router.csproj` as WPF resource content
- [X] T002 [P] Create test-file placeholders for appearance behavior in `midi-router.Tests/AppearanceModeTests.cs`, `midi-router.Tests/JsonSettingsStoreTests.cs`, and `midi-router.Tests/ThemeManagerTests.cs`

---

## Phase 2: Foundational

**Purpose**: Define the shared seams that keep persistence, Windows integration, theme
application, and the existing MIDI lifecycle independently testable.

- [X] T003 [P] [US1] Define the `AppearanceMode` values and parsing/default rules in `AppearanceMode.cs`
- [X] T004 [P] [US3] Define the persisted application-settings shape in `ApplicationSettings.cs`
- [X] T005 [P] Define the settings read/write abstraction and failure result contract in `ISettingsStore.cs`
- [X] T006 [P] Define the OS appearance query and change-notification abstraction in `IOperatingSystemThemeProvider.cs`
- [X] T007 Define theme-manager responsibilities, diagnostics propagation, and disposal boundaries in `ThemeManager.cs`

**Checkpoint**: Shared contracts are available; no user story implementation should bypass
these seams or directly modify MIDI monitoring.

---

## Phase 3: User Story 1 - Choose an Application Theme (Priority: P1) 🎯 MVP

**Goal**: Users can select Light, Dark, or OS default and see the appropriate palette.

**Independent Test**: Use a fake OS-theme provider and in-memory settings store to select all
three modes and verify the resolved palette changes immediately and OS-default resolution
matches the provider.

### Tests for User Story 1

- [X] T008 [P] [US1] Add failing tests for valid appearance values and invalid-value fallback to `OsDefault` in `midi-router.Tests/AppearanceModeTests.cs`
- [X] T009 [P] [US1] Add failing tests for explicit Light, explicit Dark, and OS-default palette resolution in `midi-router.Tests/ThemeManagerTests.cs`
- [X] T010 [P] [US1] Add failing tests that OS preference changes reapply only while `OsDefault` is selected in `midi-router.Tests/ThemeManagerTests.cs`

### Implementation for User Story 1

- [X] T011 [US1] Implement appearance-mode parsing and default behavior in `AppearanceMode.cs`
- [X] T012 [P] [US1] Create equivalent light palette resources for window, device list, status text, menus, controls, borders, and interaction states in `ThemeResources/Light.xaml`
- [X] T013 [P] [US1] Create equivalent dark palette resources with readable contrast in `ThemeResources/Dark.xaml`
- [X] T014 [US1] Implement OS-theme resolution, palette switching, and OS-change subscription in `ThemeManager.cs`
- [X] T015 [US1] Wire application startup and application-scope resource dictionary replacement to `ThemeManager` in `App.xaml` and `App.xaml.cs`

**Checkpoint**: The theme engine can be demonstrated independently with all three modes,
including OS-default updates, without requiring the settings menu or physical MIDI devices.

---

## Phase 4: User Story 2 - Access Theme Settings (Priority: P1)

**Goal**: Users can discover the gear icon in the upper-left corner and choose exactly one
of the three appearance modes from an accessible settings menu.

**Independent Test**: Launch the main window, activate the gear with pointer and keyboard,
verify the three mutually exclusive choices and selected state, then dismiss the menu while
the MIDI device view remains unchanged.

### Tests for User Story 2

- [X] T016 [P] [US2] Add failing view-model tests for the three mutually exclusive menu choices and current selection state in `midi-router.Tests/ThemeSettingsViewModelTests.cs`
- [X] T017 [P] [US2] Add failing UI contract checks for the gear accessibility name, upper-left placement, menu labels, and keyboard activation in `midi-router.Tests/MainWindowThemeTests.cs`

### Implementation for User Story 2

- [X] T018 [US2] Implement menu state and selection commands that delegate to `ThemeManager` in `ThemeSettingsViewModel.cs`
- [X] T019 [US2] Add the upper-left gear button, accessible label, anchored settings menu, and mutually exclusive Light/Dark/OS default controls in `MainWindow.xaml`
- [X] T020 [US2] Connect `ThemeSettingsViewModel` to the existing window lifecycle without changing device refresh, tray, minimization, or close behavior in `MainWindow.xaml.cs`
- [X] T021 [US2] Apply shared palette resources to all existing main-window controls and status presentation in `MainWindow.xaml`

**Checkpoint**: The complete settings-menu journey is independently usable and does not
change MIDI discovery or routing behavior.

---

## Phase 5: User Story 3 - Keep the Theme Choice (Priority: P1)

**Goal**: The selected semantic mode survives restart, while missing or invalid settings
fall back safely to OS default and failures remain visible.

**Independent Test**: Use a temporary settings-file path to save each valid mode, create a new
store/manager, restore it, and verify missing, malformed, unreadable, and unwritable cases
remain non-fatal and report diagnostics.

### Tests for User Story 3

- [X] T022 [P] [US3] Add failing tests for JSON round-trip, missing file, malformed JSON, and unknown mode fallback in `midi-router.Tests/JsonSettingsStoreTests.cs`
- [X] T023 [P] [US3] Add failing tests for read/write failure reporting and non-fatal OS-default fallback in `midi-router.Tests/ThemeManagerTests.cs`
- [X] T024 [P] [US3] Add failing restart-flow tests proving the selected semantic mode is restored in `midi-router.Tests/ThemePersistenceTests.cs`

### Implementation for User Story 3

- [X] T025 [US3] Implement JSON settings read/write with per-user default path, validation, and replace-safe persistence in `JsonSettingsStore.cs`
- [X] T026 [US3] Load persisted settings during startup and persist every menu selection through the settings abstraction in `ThemeManager.cs`
- [X] T027 [US3] Surface settings read/write diagnostics through the existing application status/logging path without stopping the router in `ThemeManager.cs` and `App.xaml.cs`
- [X] T028 [US3] Dispose settings and OS-theme subscriptions during application/window shutdown in `App.xaml.cs` and `MainWindow.xaml.cs`

**Checkpoint**: A normal restart restores the selected mode, and all specified settings-file
failure cases preserve a usable application.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Validate the integrated feature, accessibility, visual consistency, and
regression boundaries.

- [X] T029 [P] Review both palette dictionaries for equivalent keys, readable contrast, and complete coverage of visible controls in `ThemeResources/Light.xaml` and `ThemeResources/Dark.xaml`
- [X] T030 [P] Add regression assertions that theme selection does not invoke MIDI refresh or alter device-monitor lifecycle in `midi-router.Tests/ThemeSettingsIsolationTests.cs`
- [X] T031 Run `dotnet build` from the repository root and resolve feature-related compile/resource errors
- [X] T032 Run `dotnet test` from the repository root and resolve feature-related test failures
- [X] T033 Execute the manual scenarios in `specs/002-ui-theme-settings/quickstart.md` and record any required implementation corrections in the affected source files

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies; T001 and T002 can run in parallel.
- **Foundational (Phase 2)**: Depends on Phase 1; T003-T006 can run in parallel, then T007.
- **User Story 1 (Phase 3)**: Depends on Phase 2; tests T008-T010 precede implementation
  T011-T015.
- **User Story 2 (Phase 4)**: Depends on T014-T015 for theme application and can then proceed
  independently; tests T016-T017 precede implementation T018-T021.
- **User Story 3 (Phase 5)**: Depends on T005, T007, and T014; tests T022-T024 precede
  implementation T025-T028.
- **Polish (Phase 6)**: Depends on all selected user stories.

### User Story Dependencies

- **US1 (P1)**: Foundational only; MVP story.
- **US2 (P1)**: Requires the theme engine from US1, but its menu behavior is independently
  testable once the engine contract exists.
- **US3 (P1)**: Uses the theme engine and settings abstraction; persistence is isolated from
  the menu and can be tested independently with temporary files.

### Parallel Opportunities

- T001-T002 can run in parallel.
- T003-T006 can run in parallel.
- T008-T010 can run in parallel; T012-T013 can run in parallel after their tests are written.
- T016-T017 can run in parallel; T019 and T021 can run in parallel after T018's contract is
  established.
- T022-T024 can run in parallel; T029-T030 can run in parallel.

## Implementation Strategy

### MVP First (User Story 1)

1. Complete Setup and Foundational phases.
2. Implement and test the three appearance modes and resource switching.
3. Validate US1 independently with fake OS/settings boundaries.
4. Continue with the menu and persistence stories for the complete feature.

### Incremental Delivery

1. Deliver the theme engine and palette selection (US1).
2. Add the discoverable settings menu without changing MIDI behavior (US2).
3. Add JSON persistence and failure diagnostics (US3).
4. Run polish and quickstart validation.

## Notes

- Every task uses the required `- [ ] T###` checklist format.
- `[P]` is used only where tasks can work on separate files without incomplete
  dependencies.
- Story labels trace user-story work to the feature specification.
