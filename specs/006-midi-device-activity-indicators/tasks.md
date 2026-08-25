---

description: "Task list for MIDI device activity indicators"
---

# Tasks: MIDI Device Activity Indicators

**Input**: Design documents from `specs/006-midi-device-activity-indicators/`

**Prerequisites**: `plan.md`, `spec.md`, `research.md`, `data-model.md`, `contracts/device-activity.md`, `quickstart.md`

**Tests**: Required by the project constitution. Tests must be written first and run without physical MIDI hardware.

**Organization**: Tasks are grouped by user story so each story can be implemented and tested independently.

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Confirm the existing single-project structure and validation commands before feature work.

- [X] T001 [P] Confirm the existing WPF and xUnit project structure in `midi-router.csproj` and `midi-router.Tests\midi-router.Tests.csproj`
- [X] T002 [P] Confirm the baseline build and test commands from `specs/006-midi-device-activity-indicators/quickstart.md`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Establish the shared activity contract and test timing boundary before story-specific work.

**Critical**: This phase must complete before either user story implementation begins.

- [X] T003 Define the source-device activity event contract in `MidiRouter.cs` and `specs/006-midi-device-activity-indicators/contracts/device-activity.md`
- [X] T004 [P] Add deterministic fake message and endpoint helpers for activity tests in `midi-router.Tests\MidiRoutingTests.cs` and `midi-router.Tests\MidiInputDeviceViewModelTests.cs`
- [X] T005 Establish the transient activity duration and cancellation behavior used by rows in `MidiInputDeviceRow.cs`

**Checkpoint**: Shared source identity, timing, and test seams are defined; user-story work can proceed.

---

## Phase 3: User Story 1 - See MIDI Device Activity (Priority: P1) 🎯 MVP

**Goal**: Show an independent green activity dot for each device that receives MIDI messages, with reliable expiration and cleanup.

**Independent Test**: Using fake endpoints, raise messages from two endpoint IDs and verify only the matching row activates, repeated messages refresh the state, and activity expires after messages stop.

### Tests for User Story 1

> **Write these tests first and confirm they fail before implementation.**

- [X] T006 [P] [US1] Add a router source-activity event test in `midi-router.Tests\MidiRoutingTests.cs` that asserts the physical endpoint ID is emitted for received messages
- [X] T007 [P] [US1] Add row activity and `INotifyPropertyChanged` tests in `midi-router.Tests\MidiInputDeviceViewModelTests.cs` for activation, refresh, independent device IDs, and expiration
- [X] T008 [P] [US1] Add device-removal cleanup coverage in `midi-router.Tests\MidiInputDeviceViewModelTests.cs` to prevent stale expiration updates

### Implementation for User Story 1

- [X] T009 [US1] Add a source-device activity event to `MidiRouter.cs` when a physical message is identified without blocking forwarding
- [X] T010 [US1] Implement per-device active state, expiration refresh, and disposal-safe cancellation in `MidiInputDeviceRow.cs`
- [X] T011 [US1] Subscribe `MidiInputDeviceViewModel.cs` to router activity through `MidiRouterDeviceCoordinator.cs`, resolve rows by `EndpointDeviceId`, and marshal updates to the WPF dispatcher
- [X] T012 [US1] Add the activity-dot binding and accessible state metadata immediately before the device name in `MainWindow.xaml`
- [X] T013 [US1] Add theme-aware inactive and green active activity colors in `ThemeResources\Light.xaml` and `ThemeResources\Dark.xaml`

**Checkpoint**: User Story 1 is independently testable with fake endpoints and displays per-device transient activity.

---

## Phase 4: User Story 2 - View the Complete Device Name Without Horizontal Scrolling (Priority: P1)

**Goal**: Make the device list fit its outer element at changing widths while preserving the activity dot, names, protocol, channel, selection, and empty state.

**Independent Test**: Render or inspect the device list at widths from 240 pixels upward with long names and verify the list remains bounded without horizontal scrolling.

### Tests for User Story 2

> **Write these tests first and confirm they fail before implementation.**

- [X] T014 [P] [US2] Add responsive layout contract coverage for the device list in `midi-router.Tests\MainWindowLayoutTests.cs`
- [X] T015 [P] [US2] Extend `midi-router.Tests\MidiInputDeviceViewModelTests.cs` with regression coverage proving width-related presentation changes do not alter row contents, selection, or channel assignments

### Implementation for User Story 2

- [X] T016 [US2] Replace fixed aggregate device-list sizing with a parent-fitting layout and disable horizontal overflow in `MainWindow.xaml`
- [X] T017 [US2] Make the device-name cell stretch within the available width and keep long names bounded and identifiable in `MainWindow.xaml`
- [X] T018 [US2] Preserve usable protocol and channel columns while the device-name region absorbs width changes in `MainWindow.xaml`
- [X] T019 [US2] Verify the activity dot remains immediately before the bounded name across list resizing in `MainWindow.xaml`

**Checkpoint**: User Story 2 is independently testable at narrow and wide container sizes without a horizontal scrollbar.

---

## Phase 5: Polish & Cross-Cutting Concerns

**Purpose**: Validate integration, regression behavior, and the complete feature contract.

- [X] T020 [P] Run the complete automated regression suite from the repository root (`.`) with fake MIDI endpoints using `dotnet test`
- [X] T021 [P] Review activity event, row disposal, device disconnect, and dispatcher lifecycle handling in `MidiRouter.cs`, `MidiRouterDeviceCoordinator.cs`, `MidiInputDeviceViewModel.cs`, and `MidiInputDeviceRow.cs`
- [X] T022 Run all manual scenarios in `specs/006-midi-device-activity-indicators/quickstart.md`
- [X] T023 Confirm the final implementation satisfies `specs/006-midi-device-activity-indicators/contracts/device-activity.md` and update only directly affected documentation if needed

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies; baseline confirmation can run in parallel.
- **Foundational (Phase 2)**: Depends on Setup; blocks both user stories.
- **User Story 1 (Phase 3)**: Depends on Foundational and delivers the MVP.
- **User Story 2 (Phase 4)**: Depends on Foundational; its dot-placement regression assumes the activity-dot contract from US1, but layout work can be developed in parallel after the contract is defined.
- **Polish (Phase 5)**: Depends on all desired user stories.

### User Story Dependencies

- **US1 (P1)**: Can start after Phase 2; no dependency on US2.
- **US2 (P1)**: Can start after Phase 2; final visual verification depends on the US1 dot contract, but layout behavior is otherwise independent.

### Within Each User Story

- Write and fail the story tests before implementation.
- Implement portable state and routing behavior before dispatcher integration.
- Integrate view-model state before XAML bindings.
- Complete the story checkpoint before treating it as delivered.

## Parallel Opportunities

- T001 and T002 can run in parallel.
- T004 can run in parallel with T003 after setup.
- T006, T007, and T008 can be written in parallel before US1 implementation.
- T014 and T015 can be written in parallel before US2 implementation.
- After Phase 2, US1 and US2 can be assigned to separate developers, with US2 consuming the agreed dot contract.
- T020 and T021 can run in parallel after both stories are implemented.

## Parallel Example: User Story 1

```text
Task: "Add a router source-activity event test in midi-router.Tests\MidiRoutingTests.cs"
Task: "Add row activity and INotifyPropertyChanged tests in midi-router.Tests\MidiInputDeviceViewModelTests.cs"
Task: "Add device-removal cleanup coverage in midi-router.Tests\MidiInputDeviceViewModelTests.cs"
```

## Parallel Example: User Story 2

```text
Task: "Add responsive layout contract coverage in midi-router.Tests\MainWindowLayoutTests.cs"
Task: "Extend row regression coverage in midi-router.Tests\MidiInputDeviceViewModelTests.cs"
```

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Setup and Foundational phases.
2. Write failing US1 tests, then implement the source activity event, row state, view-model wiring, and dot styling.
3. Run the US1 tests and demonstrate independent activity for multiple fake devices.
4. Stop at the US1 checkpoint if a minimal activity-indicator release is required.

### Incremental Delivery

1. Deliver US1 as the activity-indicator MVP.
2. Add US2 responsive layout while preserving the US1 binding and device-row behavior.
3. Run Polish validation and the complete quickstart scenarios.

### Format Validation

All implementation tasks use the required `- [ ] T###` checklist format. User-story tasks include exactly one `[US1]` or `[US2]` label, parallelizable tasks include `[P]`, and every task names one or more concrete repository paths.
