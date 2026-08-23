# Tasks: Input MIDI Device Browser UI

**Input**: Design documents from `/specs/001-show-midi-input-devices/`

**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/midi-actions-contract.md, quickstart.md

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Align project dependencies and baseline documentation for Windows MIDI Services.

- [X] T001 Add Windows MIDI Services package references and metadata in `D:\Projekte\midi-router\midi-router.csproj`
- [X] T002 Add test-project package/version alignment for new MIDI dependencies in `D:\Projekte\midi-router\midi-router.Tests\midi-router.Tests.csproj`
- [X] T003 [P] Document Windows MIDI Services prerequisite and install expectations in `D:\Projekte\midi-router\README.md`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Build shared MIDI backend infrastructure required by all user stories.

**⚠️ CRITICAL**: No user story implementation starts before this phase completes.

- [X] T004 Create Windows MIDI Services-backed provider implementation in `D:\Projekte\midi-router\MidiInputDeviceProvider.cs`
- [X] T005 [P] Normalize provider error mapping for discovery failures in `D:\Projekte\midi-router\MidiInputDeviceProvider.cs`
- [X] T006 [P] Add provider-focused unit tests using abstractions (no hardware dependency) in `D:\Projekte\midi-router\midi-router.Tests\MidiInputDeviceViewModelTests.cs`
- [X] T007 Wire main window startup to use the Windows MIDI Services provider path in `D:\Projekte\midi-router\MainWindow.xaml.cs`

**Checkpoint**: Shared backend is ready; user stories can proceed.

---

## Phase 3: User Story 1 - View connected input devices (Priority: P1) 🎯 MVP

**Goal**: Show all currently available input MIDI devices by name when the screen opens.

**Independent Test**: Open the app with zero and non-zero available devices; verify names, device count, and empty message are correct.

- [X] T008 [P] [US1] Add/adjust successful-load and empty-state assertions in `D:\Projekte\midi-router\midi-router.Tests\MidiInputDeviceViewModelTests.cs`
- [X] T009 [US1] Update initial-load status/count behavior for discovered devices in `D:\Projekte\midi-router\MidiInputDeviceViewModel.cs`
- [X] T010 [US1] Bind list presentation and device-name rendering for initial load in `D:\Projekte\midi-router\MainWindow.xaml`

**Checkpoint**: User Story 1 is independently functional and testable.

---

## Phase 4: User Story 2 - Keep the list current (Priority: P2)

**Goal**: Refresh action updates the list to match current device availability.

**Independent Test**: Change connected devices, click refresh, and verify updated list and status.

- [X] T011 [P] [US2] Add refresh-after-change test coverage with sequential provider responses in `D:\Projekte\midi-router\midi-router.Tests\MidiInputDeviceViewModelTests.cs`
- [X] T012 [US2] Extend stub provider behavior for multi-refresh snapshots in `D:\Projekte\midi-router\midi-router.Tests\MidiInputDeviceViewModelTests.cs`
- [X] T013 [US2] Implement refresh-state transitions (`Loading -> Loaded|Empty|Error`) in `D:\Projekte\midi-router\MidiInputDeviceViewModel.cs`
- [X] T014 [US2] Ensure refresh button interaction and status updates stay responsive in `D:\Projekte\midi-router\MainWindow.xaml.cs`

**Checkpoint**: User Stories 1 and 2 both work independently.

---

## Phase 5: User Story 3 - Read list comfortably (Priority: P3)

**Goal**: Maintain a simple, modern, easy-to-scan device list experience.

**Independent Test**: Review screen readability and verify clear visual distinction among list entries, counts, and status messages.

- [X] T015 [US3] Refine typography, spacing, and card hierarchy for better scanability in `D:\Projekte\midi-router\MainWindow.xaml`
- [X] T016 [US3] Add explicit visual treatment for empty/error list states in `D:\Projekte\midi-router\MainWindow.xaml`
- [X] T017 [P] [US3] Improve user-facing status text clarity for loaded, empty, and error states in `D:\Projekte\midi-router\MidiInputDeviceViewModel.cs`

**Checkpoint**: All user stories are independently functional.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Final consistency, documentation, and feature validation across stories.

- [X] T018 [P] Update backend contract wording to match final implementation decisions in `D:\Projekte\midi-router\specs\001-show-midi-input-devices\contracts\midi-actions-contract.md`
- [X] T019 [P] Update validation/run guidance to match implemented behavior in `D:\Projekte\midi-router\specs\001-show-midi-input-devices\quickstart.md`
- [X] T020 Document final user-visible behavior and limitations in `D:\Projekte\midi-router\README.md`

---

## Dependencies & Execution Order

### Phase Dependencies

- Phase 1 -> Phase 2 -> Phase 3 -> Phase 4 -> Phase 5 -> Phase 6
- User stories begin only after Phase 2 is complete.

### User Story Dependencies

- **US1 (P1)**: Starts after Phase 2; no dependency on other stories.
- **US2 (P2)**: Starts after Phase 2; depends on US1 list surface being present.
- **US3 (P3)**: Starts after US1 baseline UI exists; can overlap with late US2 implementation if no file conflicts.

### Parallel Opportunities

- Phase 1: T003 can run in parallel with T001-T002.
- Phase 2: T005 and T006 can run in parallel after T004 starts stabilizing interfaces.
- US1: T008 can run in parallel with early work on T009.
- US2: T011 can run in parallel with T013.
- US3: T017 can run in parallel with T015-T016.
- Polish: T018 and T019 can run in parallel before T020 final readme pass.

---

## Parallel Example: User Story 2

```bash
Task: "T011 [US2] Add refresh-after-change test coverage in midi-router.Tests/MidiInputDeviceViewModelTests.cs"
Task: "T013 [US2] Implement refresh-state transitions in MidiInputDeviceViewModel.cs"
```

---

## Implementation Strategy

### MVP First (US1 only)

1. Complete Phase 1 (Setup).
2. Complete Phase 2 (Foundational).
3. Complete Phase 3 (US1).
4. Validate US1 independently before expanding scope.

### Incremental Delivery

1. Deliver US1 as MVP.
2. Add US2 refresh behavior.
3. Add US3 readability refinements.
4. Finish with cross-cutting polish and documentation sync.
