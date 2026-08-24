---

description: "Task list for selecting and persisting MIDI devices by unique device ID"
---

# Tasks: Select MIDI Devices

**Input**: Design documents from `/specs/004-select-midi-devices/`

**Prerequisites**: `plan.md`, `spec.md`, `research.md`, `data-model.md`,
`contracts/device-selection.md`, `quickstart.md`

**Tests**: Required by the project constitution. Follow red-green-refactor: write each
behavior test first and verify it fails before implementing the corresponding behavior.

## Phase 1: Setup

**Purpose**: Establish the baseline and confirm the existing project is ready for changes.

- [X] T001 Run the existing repository baseline checks with `dotnet build` and `dotnet test` from `D:\Projekte\midi-router`

---

## Phase 2: Foundational

**Purpose**: Provide shared settings ownership and persisted unique-ID storage without
overwriting appearance or tray preferences.

**⚠️ CRITICAL**: Complete this phase before any user story implementation.

- [X] T002 Add failing settings serialization tests for a selected unique device ID set in `midi-router.Tests\JsonSettingsStoreTests.cs`
- [X] T003 Add failing settings normalization tests for empty and duplicate device IDs in `midi-router.Tests\JsonSettingsStoreTests.cs`
- [X] T004 Extend `ApplicationSettings` in `ApplicationSettings.cs` with a persisted collection of selected unique device IDs while preserving existing defaults
- [X] T005 Update `JsonSettingsStore.cs` to serialize, load, and normalize selected unique device IDs without dropping appearance or tray settings
- [X] T006 Add `ApplicationSettingsCoordinator.cs` to own the loaded settings snapshot, expose safe updates, and serialize changes through the existing `ISettingsStore`
- [X] T007 Refactor `ThemeManager.cs` and `App.xaml.cs` to use the shared `ApplicationSettingsCoordinator` so theme, tray, and device-selection updates cannot overwrite one another
- [X] T008 Add coordinator preservation and persistence-failure tests in `midi-router.Tests\ApplicationSettingsCoordinatorTests.cs`

**Checkpoint**: Shared settings can safely persist selected unique device IDs alongside
existing preferences.

---

## Phase 3: User Story 1 - Select Devices for Processing (Priority: P1) 🎯 MVP

**Goal**: Let users toggle multiple device rows and expose the selected unique device ID set
for later processing.

**Independent Test**: Use fake devices with distinct unique IDs, click/toggle their rows
through the view model, and verify that one or more exact IDs are selected without using
display names.

### Tests for User Story 1

- [X] T009 [US1] Add failing view-model tests for selecting and deselecting one device by unique ID in `midi-router.Tests\MidiInputDeviceViewModelTests.cs`
- [X] T010 [US1] Add failing view-model tests for retaining multiple selections and independently selecting same-name devices in `midi-router.Tests\MidiInputDeviceViewModelTests.cs`
- [X] T011 [US1] Add failing contract tests for the exposed selected unique ID set in `midi-router.Tests\MidiInputDeviceViewModelTests.cs`

### Implementation for User Story 1

- [X] T012 [US1] Add the ID-backed selection state, toggle operation, and read-only selected-device-ID projection to `MidiInputDeviceViewModel.cs`
- [X] T013 [US1] Add selection-state accessors to `MidiInputDevice.cs` or the row presentation model without changing `EndpointDeviceId` identity semantics
- [X] T014 [US1] Wire row activation in `MainWindow.xaml` and `MainWindow.xaml.cs` to toggle only the clicked device's unique `EndpointDeviceId`
- [X] T015 [US1] Verify the selected ID set remains available as a non-visual view-model contract for later processing in `MidiInputDeviceViewModel.cs`

**Checkpoint**: User Story 1 is independently functional and testable without physical
MIDI hardware.

---

## Phase 4: User Story 2 - Recognize Selected Devices (Priority: P1)

**Goal**: Highlight each selected row immediately and keep highlighting synchronized with
the ID-backed selection state.

**Independent Test**: Toggle mixed rows in a populated list and verify each row's highlight
matches its own selected unique device ID.

### Tests for User Story 2

- [X] T016 [P] [US2] Add failing WPF/view-model binding tests for selected and unselected row state in `midi-router.Tests\MidiInputDeviceViewModelTests.cs`
- [X] T017 [P] [US2] Add failing tests for selection-state updates after a toggle in `midi-router.Tests\MidiInputDeviceViewModelTests.cs`

### Implementation for User Story 2

- [X] T018 [US2] Add selected-row binding and a distinct selected-row visual style to `MainWindow.xaml`
- [X] T019 [US2] Raise the required property or collection notifications from `MidiInputDeviceViewModel.cs` so highlights update immediately after toggles
- [X] T020 [US2] Preserve readable text and control contrast for selected rows using the existing theme resources in `MainWindow.xaml`

**Checkpoint**: Users can identify every selected device from row highlighting without
opening another view.

---

## Phase 5: User Story 3 - Restore Device Selection (Priority: P1)

**Goal**: Persist selected unique device IDs, restore them on startup, and reconcile them
with device connect/disconnect snapshots.

**Independent Test**: Select IDs using an in-memory settings store, recreate the view model,
apply available/unavailable device snapshots, and verify restoration and reconnect behavior.

### Tests for User Story 3

- [X] T021 [US3] Add persistence and restart tests for selected unique IDs in `midi-router.Tests\MidiInputDeviceViewModelTests.cs`
- [X] T022 [P] [US3] Add snapshot reconciliation tests for disconnect, reconnect, empty lists, and duplicate-name devices in `midi-router.Tests\MidiInputDeviceViewModelTests.cs`
- [X] T023 [US3] Add settings-error tests proving enumeration remains usable and the persistence failure is reported in `midi-router.Tests\ApplicationSettingsCoordinatorTests.cs`

### Implementation for User Story 3

- [X] T024 [US3] Load the persisted selected unique ID set during `MidiInputDeviceViewModel.cs` initialization and save each successful toggle through `ApplicationSettingsCoordinator.cs`
- [X] T025 [US3] Reconcile incoming `DeviceOverviewSnapshot` values by `EndpointDeviceId` in `MidiInputDeviceViewModel.cs`, retaining unavailable IDs for reconnect restoration
- [X] T026 [US3] Surface selection load/save failures through the existing status or diagnostic callback while keeping `MidiDeviceMonitor.cs` refreshes active
- [X] T027 [US3] Update `App.xaml.cs` and `MainWindow.xaml.cs` construction/lifecycle wiring to use the shared coordinator and dispose selection resources correctly
- [X] T028 [US3] Add selected-ID persistence documentation to `README.md` without documenting implementation internals

**Checkpoint**: All three user stories work together while preserving existing device
discovery, theme, tray, and routing behavior.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Validate the complete feature and protect unrelated behavior.

- [X] T029 [P] Verify through `midi-router.Tests\ApplicationSettingsCoordinatorTests.cs` that selection persistence does not overwrite existing settings
- [X] T030 [P] Verify through `midi-router.Tests\MidiInputDeviceViewModelTests.cs` that selection reconciliation does not change device ordering or snapshot contents
- [X] T031 Run all automated scenarios in `specs\004-select-midi-devices\quickstart.md`
- [X] T032 Run final repository checks with `dotnet build` and `dotnet test` from `D:\Projekte\midi-router`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies.
- **Foundational (Phase 2)**: Depends on T001 and blocks all user stories.
- **User Story 1 (Phase 3)**: Depends on T002-T008.
- **User Story 2 (Phase 4)**: Depends on T012-T015 from User Story 1 because highlighting binds
  to the selection state.
- **User Story 3 (Phase 5)**: Depends on T004-T008 and T012; its snapshot and persistence
  behavior can then be completed independently of the visual polish in User Story 2.
- **Polish (Phase 6)**: Depends on all desired user story checkpoints.

### Parallel Opportunities

- T002 and T003 can run in parallel.
- T009-T011 can run in parallel before User Story 1 implementation.
- T016-T017 can run in parallel before User Story 2 implementation.
- T021-T023 can run in parallel before User Story 3 implementation.
- T029 and T030 can run in parallel after the user stories are complete.
- Once Phase 2 is complete, separate developers can work on the independent test groups for
  User Stories 1 and 3; User Story 2 follows the selection-state contract from User Story 1.

## Parallel Example: User Story 1

```text
Task T009: Add toggle tests in midi-router.Tests\MidiInputDeviceViewModelTests.cs
Task T010: Add multi-selection and same-name tests in midi-router.Tests\MidiInputDeviceViewModelTests.cs
Task T011: Add selected-ID contract tests in midi-router.Tests\MidiInputDeviceViewModelTests.cs
```

## Parallel Example: User Story 3

```text
Task T021: Add restart persistence tests in midi-router.Tests\DeviceSelectionPersistenceTests.cs
Task T022: Add disconnect/reconnect tests in midi-router.Tests\MidiInputDeviceViewModelTests.cs
Task T023: Add persistence-error tests in midi-router.Tests\DeviceSelectionPersistenceTests.cs
```

## Implementation Strategy

### MVP First (User Story 1)

1. Complete Setup and Foundational phases.
2. Implement User Story 1 and validate unique-ID toggle behavior.
3. Stop and demonstrate the selected ID set before adding visual polish and persistence.

### Incremental Delivery

1. Add User Story 1 for multi-device selection.
2. Add User Story 2 for immediate row highlighting.
3. Add User Story 3 for restart and reconnect persistence.
4. Complete cross-cutting regression checks and quickstart validation.

### Traceability

- FR-001 to FR-003: T009-T015
- FR-004 to FR-005: T016-T020
- FR-006 to FR-009: T002-T008, T021, T024
- FR-010 to FR-012: T022-T027
- FR-013 to FR-014: T011, T015, T029-T030

All tasks use the required checklist format with sequential IDs, story labels on story
tasks, `[P]` only for parallelizable tasks, and explicit repository-relative file paths.
