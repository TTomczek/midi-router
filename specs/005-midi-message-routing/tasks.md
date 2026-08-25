---

description: "Executable task list for MIDI message routing"
---

# Tasks: MIDI Message Routing

**Input**: Design documents from `/specs/005-midi-message-routing/`

**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/midi-routing.md, quickstart.md

**Tests**: Required by the project constitution. Follow red-green-refactor: write each test
before the implementation it validates.

## Phase 1: Setup

**Purpose**: Confirm the existing single-project structure and prepare the feature seams.

- [X] T001 Inspect the pinned Windows.Devices.Midi2 package metadata and generated API surface in `midi-router.csproj` and `obj/` to confirm the exact virtual-device and message projection types
- [ ] T002 [P] Add feature test file placeholders and shared fake endpoint helpers in `midi-router.Tests/MidiRoutingTestDoubles.cs`
- [X] T003 [P] Add routing-specific logging event definitions following existing patterns in `MidiRoutingLogging.cs`

---

## Phase 2: Foundational

**Purpose**: Build blocking portable contracts and settings primitives before route-specific work.

- [X] T004 [P] Define portable MIDI packet and message contracts in `MidiRoutingMessage.cs` without exposing WinRT types
- [X] T005 [P] Define endpoint connection, session, and virtual endpoint adapter interfaces in `IMidiRoutingEndpointProvider.cs`
- [X] T006 [P] Define reversible transformation stage contracts in `IMidiMessageTransformation.cs`
- [X] T007 [P] Add `DeviceChannelAssignments` to `ApplicationSettings.cs` with stable endpoint IDs and nullable/default-safe serialization
- [X] T008 [P] Normalize persisted channel assignments and report invalid settings in `ApplicationSettingsCoordinator.cs`
- [X] T009 [P] Implement channel allocation and conflict validation in `MidiChannelAllocator.cs` for internal values 0-15 and display values 1-16
- [X] T010 [P] Write foundational contract tests for packet copying, channel bounds, allocation order, and settings normalization in `midi-router.Tests/MidiRoutingTests.cs`

**Checkpoint**: Portable message, assignment, transformation, and endpoint contracts exist and
all foundational tests pass with no MIDI hardware.

---

## Phase 3: User Story 1 - Route Selected Device Messages (Priority: P1) 🎯 MVP

**Goal**: Receive messages from selected devices and emit them through one virtual MIDI endpoint.

**Independent Test**: With fake physical endpoints and a fake virtual endpoint, select devices,
inject messages, and verify selected input is emitted once while unselected input is ignored.

### Tests for User Story 1

- [X] T011 [P] [US1] Write routing-core tests for selected versus unselected input and one shared virtual output in `midi-router.Tests/MidiRoutingTests.cs`
- [ ] T012 [P] [US1] Write route lifecycle tests for selection, deselection, disconnect, reconnect, and independent route failure in `midi-router.Tests/MidiRouterLifecycleTests.cs`
- [ ] T013 [P] [US1] Write adapter contract tests for receive callback enqueueing and send failure reporting in `midi-router.Tests/WindowsMidiRoutingAdapterTests.cs`

### Implementation for User Story 1

- [X] T014 [US1] Implement the portable routing coordinator and per-device route state in `MidiRouter.cs`
- [X] T015 [US1] Implement queue-backed message dispatch so endpoint receive callbacks return promptly in `MidiRouterMessageDispatcher.cs`
- [X] T016 [US1] Implement selected-device synchronization with the existing device provider in `MidiRouterDeviceCoordinator.cs`
- [X] T017 [US1] Implement Windows MIDI Services session and physical endpoint connection adapter using `MidiSession` and `MidiEndpointConnection` in `WindowsMidiRoutingEndpointProvider.cs`
- [X] T018 [US1] Implement the pinned package's virtual MIDI endpoint adapter and external receive connection in `WindowsMidiRoutingEndpointProvider.cs`
- [X] T019 [US1] Wire channel assignment startup, selection changes, and disposal into `MidiInputDeviceViewModel.cs` and `MainWindow.xaml.cs`
- [X] T020 [US1] Surface virtual endpoint and send failures through the diagnostic path in `MidiRouter.cs`

**Checkpoint**: US1 independently routes selected fake-device messages to the fake virtual
endpoint and keeps unrelated routes operating after one route fails.

---

## Phase 4: User Story 2 - Configure Device Channels (Priority: P1)

**Goal**: Show and edit each device's channel assignment, automatically allocating the next
free internal channel and rejecting invalid or conflicting values.

**Independent Test**: Populate the device list, inspect display values, assign channels 1-16,
and verify invalid/conflicting edits preserve the previous valid assignment.

### Tests for User Story 2

- [X] T021 [P] [US2] Write channel transformation tests covering MIDI 1 channel-bearing messages, preserved data, channel-less messages, and reverse restoration in `midi-router.Tests/MidiRoutingTests.cs`
- [X] T022 [P] [US2] Write assignment tests covering automatic ascending allocation and conflict rejection in `midi-router.Tests/MidiRoutingTests.cs`
- [X] T023 [P] [US2] Write device-row/view-model tests for displayed channel values, immediate updates, persistence, and duplicate device names in `midi-router.Tests/MidiInputDeviceViewModelTests.cs`

### Implementation for User Story 2

- [X] T024 [US2] Implement the initial reversible channel transformation for channel-bearing UMP/MIDI 1 messages in `MidiChannelTransformation.cs`
- [X] T025 [US2] Add atomic assignment, allocation, conflict, and exhaustion operations to `MidiChannelAllocator.cs` and `MidiRouter.cs`
- [X] T026 [US2] Add channel assignment state, display conversion, and change notifications to `MidiInputDeviceRow.cs`
- [X] T027 [US2] Expose assignment changes and persisted endpoint-ID mappings through `MidiInputDeviceViewModel.cs`
- [X] T028 [US2] Add the channel column and valid-value editor/status feedback to `MainWindow.xaml` and `MainWindow.xaml.cs`
- [X] T029 [US2] Connect channel assignment changes to the existing `ApplicationSettingsCoordinator` persistence flow in `MidiInputDeviceViewModel.cs`

**Checkpoint**: US1 continues to route messages, and US2 visibly/configurably assigns unique
channels with correct internal/user-facing conversion.

---

## Phase 5: User Story 3 - Return Responses to the Originating Device (Priority: P1)

**Goal**: Consume messages from the virtual endpoint, resolve the assigned channel, reverse
the pipeline, and send only to the associated physical device.

**Independent Test**: Inject virtual messages for multiple assigned channels and verify exact
device routing, original channel restoration, and safe handling of unknown channels.

### Tests for User Story 3

- [ ] T030 [P] [US3] Write virtual-input routing tests for exact channel-to-device mapping, original-channel restoration, and no arbitrary send for unknown/ambiguous channels in `midi-router.Tests/MidiRouterOutputTests.cs`
- [ ] T031 [P] [US3] Write bidirectional exchange tests for 100 messages and channel-less responses in `midi-router.Tests/MidiRouterBidirectionalTests.cs`
- [ ] T032 [P] [US3] Write virtual endpoint failure and worker shutdown tests in `midi-router.Tests/MidiRouterLifecycleTests.cs`

### Implementation for User Story 3

- [X] T033 [US3] Implement reverse transformation and assigned-channel lookup in `MidiRouter.cs`
- [X] T034 [US3] Add virtual-endpoint receive queue processing and physical send dispatch in `WindowsMidiRoutingEndpointProvider.cs`
- [X] T035 [US3] Add explicit unknown/ambiguous channel diagnostics and per-route send error isolation in `MidiRouter.cs`
- [X] T036 [US3] Complete Windows adapter packet conversion for received/sent UMP words while preserving timestamps and non-channel data in `WindowsMidiRoutingEndpointProvider.cs`
- [X] T037 [US3] Ensure router shutdown disposes all owned endpoint connections in `MidiRouter.cs`

**Checkpoint**: US3 completes a bidirectional fake exchange and never routes an unknown
virtual channel to an arbitrary device.

---

## Phase 6: User Story 4 - Preserve Routing Configuration (Priority: P1)

**Goal**: Persist and restore valid channel assignments by stable endpoint identity across
application restarts and device availability changes.

**Independent Test**: Save assignments, construct a new coordinator/view model, reconnect
matching IDs, and verify values restore while unavailable IDs do not block active routes.

### Tests for User Story 4

- [ ] T038 [P] [US4] Write persistence round-trip tests for endpoint-ID channel assignments and restart restoration in `midi-router.Tests/MidiRoutingSettingsTests.cs`
- [ ] T039 [P] [US4] Write invalid/missing/unavailable assignment tests for normalization, automatic allocation, and persistence failure reporting in `midi-router.Tests/MidiRoutingSettingsTests.cs`

### Implementation for User Story 4

- [X] T040 [US4] Integrate restored channel assignments with route activation and automatic allocation in `MidiRouterDeviceCoordinator.cs`
- [X] T041 [US4] Preserve assignments for disconnected devices without reserving channels for inactive routes in `MidiRouter.cs`
- [X] T042 [US4] Persist successful explicit and automatic assignments using `ApplicationSettingsCoordinator` while retaining in-memory operation on save failure in `MidiInputDeviceViewModel.cs`
- [X] T043 [US4] Restore channel display and assignment state during device-list snapshots in `MidiInputDeviceRow.cs` and `MidiInputDeviceViewModel.cs`

**Checkpoint**: US4 restores all valid available assignments by endpoint ID after restart and
does not prevent unrelated devices from routing.

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Validate the complete feature, documentation, lifecycle, and quality gates.

- [X] T044 [P] Update user-visible routing/channel status text and runtime prerequisite documentation in `README.md`
- [X] T045 [P] Add logging for route start/stop, assignment changes, transformation failures, and virtual endpoint diagnostics in `MidiDeviceLogging.cs` and `MidiRoutingLogging.cs`
- [X] T046 [P] Review UI accessibility names and keyboard/editor behavior for the channel control in `MainWindow.xaml`
- [X] T047 Run the complete automated validation from `specs/005-midi-message-routing/quickstart.md` using `dotnet build` and `dotnet test`
- [ ] T048 Run the manual Windows MIDI validation scenarios from `specs/005-midi-message-routing/quickstart.md` and document any unavailable-service diagnostics in the established status surface

## Dependencies & Execution Order

### Phase Dependencies

- Setup (Phase 1) precedes Foundational (Phase 2).
- Foundational (Phase 2) blocks all user stories.
- User stories can proceed in parallel after Phase 2, but US2 depends logically on the
  routing contracts from US1 and US3 depends on the active route/channel map from US1 and US2.
- US4 depends on the settings and assignment behavior from US2.
- Polish depends on the desired user stories being complete.

### User Story Dependencies

- US1 (P1): starts after Phase 2; MVP.
- US2 (P1): starts after Phase 2; integrates with US1's router but its assignment/transform
  tests are independently executable.
- US3 (P1): depends on US1 route lifecycle and US2 channel mapping.
- US4 (P1): depends on US2 persistence fields and assignment operations.

### Parallel Opportunities

- T002-T003 can run in parallel.
- T004-T010 can be split by contracts, settings, allocator, and tests after setup.
- T011-T013 are parallel test-writing tasks.
- T021-T023 are parallel test-writing tasks.
- T030-T032 and T038-T039 are parallel test-writing tasks.
- T044-T046 are parallel polish tasks.
- Once foundational work is complete, separate contributors can work on US1 adapters,
  US2 transformation/UI, and US4 persistence tests, provided shared-file edits are coordinated.

## Implementation Strategy

### MVP First

1. Complete Setup and Foundational phases.
2. Complete US1 with fake endpoints and Windows adapters.
3. Validate selected-device input reaches the virtual endpoint.

### Incremental Delivery

1. Add US2 channel assignment and forward transformation.
2. Add US3 reverse routing and bidirectional exchange.
3. Add US4 restart persistence and reconnect behavior.
4. Complete cross-cutting diagnostics, documentation, and manual validation.

### Format Validation

All implementation tasks use `- [ ]`, sequential `T###` IDs, `[P]` only where parallel work
is safe, `[US#]` on every user-story task, and an explicit repository-relative file path.
