---

description: "Task list for MIDI device list implementation"
---

# Tasks: MIDI Device List

**Input**: Design documents from `/specs/001-midi-device-list/`

**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/

**Tests**: Required by the project constitution and plan. Follow red-green-refactor:
write each test before the implementation it validates.

**Organization**: Tasks are grouped by user story so each story can be implemented and
validated independently after the foundational monitoring boundary exists.

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Align project configuration and establish the implementation/test file layout.

- [X] T001 Confirm the .NET 10 `net10.0-windows10.0.22621` target and matching runtime prerequisites in `README.md` and `midi-router.csproj`
- [X] T002 [P] Add and configure the selected `Microsoft.Extensions.Logging` provider and application logging defaults in `midi-router.csproj` and `App.xaml.cs`
- [X] T003 [P] Establish test file organization for device monitoring in `midi-router.Tests/midi-router.Tests.csproj`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Create boundaries and lifecycle infrastructure required by both user stories.

**CRITICAL**: No user story work can begin until this phase is complete.

- [X] T004 Define the device identity, MIDI version, overview state, and immutable snapshot types in `MidiInputDevice.cs`
- [X] T005 Define the watcher/provider abstraction and add/remove/update/completed/stopped event contract in `IMidiInputDeviceProvider.cs`
- [X] T006 [P] Write failing tests for snapshot uniqueness, duplicate display names, state transitions, and unknown versions in `midi-router.Tests/MidiInputDeviceTests.cs`
- [X] T007 [P] Write failing tests for serialized update ordering, cancellation, and post-shutdown publication suppression in `midi-router.Tests/MidiDeviceMonitorTests.cs`
- [X] T008 Implement bounded/coalesced event intake and single-consumer reconciliation in `MidiDeviceMonitor.cs`
- [X] T009 Implement explicit service, enumeration, degraded, empty, unavailable, and stopped error-state mapping with structured event IDs in `MidiDeviceLogging.cs`
- [X] T010 Wire deterministic UI-dispatcher publication and disposal/cancellation ownership into `MidiInputDeviceViewModel.cs`

**Checkpoint**: Monitoring boundaries, lifecycle rules, error states, and deterministic
test doubles are available; user stories can now be implemented independently.

---

## Phase 3: User Story 1 - View Connected MIDI Devices (Priority: P1) 🎯 MVP

**Goal**: Show every currently connected supported MIDI endpoint exactly once with its
name and MIDI version, including an explicit empty state.

**Independent Test**: A deterministic provider supplies MIDI 1 and MIDI 2 endpoint data;
the initial snapshot contains every endpoint once with the correct user-facing values, or
an explicit empty state when the provider supplies none.

### Tests for User Story 1

> **NOTE: Write these tests FIRST and ensure they FAIL before implementation.**

- [ ] T011 [P] [US1] Write the initial enumeration, empty-state, and at-least-20-device acceptance tests in `midi-router.Tests/MidiInputDeviceViewModelTests.cs`
- [ ] T012 [P] [US1] Write the MIDI 1/MIDI 2 native-format classification contract tests in `midi-router.Tests/WindowsMidiInputDeviceProviderTests.cs`
- [ ] T013 [P] [US1] Write the duplicate-name and endpoint-identity acceptance tests in `midi-router.Tests/MidiInputDeviceViewModelTests.cs`

### Implementation for User Story 1

- [X] T014 [US1] Implement the Windows MIDI2 service availability check and standard endpoint watcher creation in `WindowsMidiInputDeviceProvider.cs`
- [X] T015 [US1] Implement endpoint projection using `MidiEndpointDeviceInformation.EndpointDeviceId`, `Name`, and native format in `WindowsMidiInputDeviceProvider.cs`
- [X] T016 [US1] Implement initial enumeration completion, deterministic snapshot ordering, and `Ready`/`Empty` publication in `MidiDeviceMonitor.cs`
- [X] T017 [US1] Bind device name, MIDI version, list state, and status message to the WPF view in `MainWindow.xaml` and `MainWindow.xaml.cs`
- [X] T018 [US1] Integrate the provider, monitor, and view model lifecycle at application startup and close in `MainWindow.xaml.cs`

**Checkpoint**: User Story 1 is independently usable and demonstrates the MVP device
overview without requiring physical hardware in automated tests.

---

## Phase 4: User Story 2 - See Device Connection Changes (Priority: P1)

**Goal**: Keep the overview synchronized with endpoint additions, removals, reconnects,
and transient update failures without restarting the application.

**Independent Test**: A deterministic watcher emits add, remove, reconnect, duplicate,
rapid, and failure events; each resulting snapshot converges to the current endpoint set
while preserving unaffected entries.

### Tests for User Story 2

> **NOTE: Write these tests FIRST and ensure they FAIL before implementation.**

- [ ] T019 [P] [US2] Write add/remove/reconnect acceptance tests in `midi-router.Tests/MidiDeviceMonitorTests.cs`
- [ ] T020 [P] [US2] Write rapid-event coalescing and stale-snapshot prevention tests in `midi-router.Tests/MidiDeviceMonitorTests.cs`
- [ ] T021 [P] [US2] Write partial endpoint-read failure and recovery-state tests in `midi-router.Tests/MidiDeviceMonitorTests.cs`
- [ ] T022 [P] [US2] Write callback latency, UI-thread publication, and shutdown cancellation tests in `midi-router.Tests/MidiDeviceMonitorTests.cs`

### Implementation for User Story 2

- [X] T023 [US2] Connect watcher Added, Removed, EnumerationCompleted, and Stopped events to the monitor queue in `WindowsMidiInputDeviceProvider.cs`
- [X] T024 [US2] Reconcile current watcher-map entries by endpoint ID and preserve valid entries on partial failures in `MidiDeviceMonitor.cs`
- [X] T025 [US2] Implement idempotent reconnect handling, unknown-removal diagnostics, and final-state convergence in `MidiDeviceMonitor.cs`
- [X] T026 [US2] Publish degraded and unavailable status messages without replacing failures with a false empty state in `MidiInputDeviceViewModel.cs`
- [X] T027 [US2] Ensure watcher callbacks never block and monitor disposal prevents all later UI updates in `WindowsMidiInputDeviceProvider.cs` and `MidiDeviceMonitor.cs`

**Checkpoint**: User Stories 1 and 2 are independently testable and the list remains
accurate through the full supported endpoint lifecycle.

---

## Phase 5: Polish & Cross-Cutting Concerns

**Purpose**: Complete quality gates, documentation, and operational readiness.

- [ ] T028 [P] Add structured logging assertions for service failure, enumeration completion, endpoint changes, queue coalescing, and watcher stop in `midi-router.Tests/MidiDeviceLoggingTests.cs`
- [ ] T029 [P] Add accessibility and localization-ready resource keys for list labels and status messages in `Resources/Strings.resx` and `MainWindow.xaml`
- [X] T030 Update runtime prerequisites, automatic hot-plug behavior, and build/test instructions in `README.md`
- [ ] T031 Run the end-to-end scenarios, including the 20-device scale scenario, from `specs/001-midi-device-list/quickstart.md` and record any requirement gaps in `specs/001-midi-device-list/contracts/device-overview.md`
- [X] T032 Run repository quality gates with `dotnet build` and `dotnet test` from the repository root and resolve failures in the affected source or test files

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies; T002 and T003 can run in parallel with T001.
- **Foundational (Phase 2)**: Depends on Phase 1; T006 and T007 can be written in
  parallel, then T008-T010 implement their shared boundaries.
- **User Story 1 (Phase 3)**: Depends on Phase 2; T011-T013 can be written in parallel,
  followed by provider, monitor, and WPF integration.
- **User Story 2 (Phase 4)**: Depends on the foundational monitor and US1 provider/view-model
  boundary; T019-T022 can be written in parallel before T023-T027.
- **Polish (Phase 5)**: Depends on the desired user stories being complete.

### User Story Dependencies

- **User Story 1 (P1)**: Depends only on Foundational; this is the recommended MVP.
- **User Story 2 (P1)**: Depends on Foundational and the provider/view-model integration
  established for US1, but has independent acceptance tests and delivery value.

### Parallel Opportunities

- T002, T003, T006, and T007 can be performed in parallel when their files do not overlap.
- T011-T013 can be performed in parallel because they are separate test concerns.
- T019-T022 can be performed in parallel because they are separate lifecycle test concerns.
- T028-T030 can be performed in parallel after implementation stabilizes.

## Parallel Example: User Story 1

```text
Task: "Write initial enumeration and empty-state tests in midi-router.Tests/MidiInputDeviceViewModelTests.cs"
Task: "Write MIDI native-format classification tests in midi-router.Tests/WindowsMidiInputDeviceProviderTests.cs"
Task: "Write identity and duplicate-name tests in midi-router.Tests/MidiInputDeviceViewModelTests.cs"
```

## Parallel Example: User Story 2

```text
Task: "Write add/remove/reconnect tests in midi-router.Tests/MidiDeviceMonitorTests.cs"
Task: "Write rapid-event convergence tests in midi-router.Tests/MidiDeviceMonitorTests.cs"
Task: "Write failure and recovery tests in midi-router.Tests/MidiDeviceMonitorTests.cs"
Task: "Write callback and shutdown tests in midi-router.Tests/MidiDeviceMonitorTests.cs"
```

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Setup and Foundational phases.
2. Write and fail the US1 tests.
3. Implement Windows MIDI2 enumeration and the read-only WPF overview.
4. Run the US1 independent tests and the repository quality gates.
5. Stop for MVP validation before adding live change handling.

### Incremental Delivery

1. Deliver US1 with deterministic initial enumeration and empty/error states.
2. Add US2 watcher events, serialized reconciliation, and reconnect behavior.
3. Complete structured diagnostics, accessibility/localization requirements, documentation,
   quickstart validation, and final quality gates.

### Format Validation

All implementation tasks use `- [ ]`, a sequential `T###` identifier, optional `[P]`,
required `[US#]` labels in user-story phases, and an explicit repository file path.
