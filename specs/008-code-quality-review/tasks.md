# Code Quality and Reliability Review Tasks

## Phase 1: Confirmed defect fixes

- [x] T001 [US1] Release allocator and original-channel state when deactivating a route in `MidiRouter.cs`
- [x] T002 [US1] Serialize router endpoint state and message handling in `MidiRouter.cs`
- [x] T003 [US1] Prevent unhandled MIDI callback exceptions in `MidiRouterMessageDispatcher.cs` and `WindowsMidiRoutingEndpointProvider.cs`
- [x] T004 [US1] Coordinate routing synchronization and router disposal in `MidiRouterDeviceCoordinator.cs`
- [x] T005 [US1] Make startup failures recoverable instead of rethrowing from `MainWindow.xaml.cs`
- [x] T006 [US1] Correct window shutdown ordering in `MainWindow.xaml.cs`
- [x] T007 [US1] Remove unreachable stopped-monitor publication and disposal race in `MidiDeviceMonitor.cs`

## Phase 2: Regression coverage

- [x] T008 [P] [US1] Add channel-release regression coverage in `midi-router.Tests/MidiRoutingTests.cs`
- [ ] T009 [US1] Add concurrent synchronization and disposal regression coverage in `midi-router.Tests`
- [ ] T010 [US1] Add callback-exception containment coverage for `MidiRouterMessageDispatcher.cs`

## Phase 3: Cleanup and architecture

- [x] T011 [P] [US2] Remove unused compatibility aliases and helper methods from `ProfileManager.cs`, `Profile.cs`, and `MidiRoutingMessage.cs`
- [ ] T012 [US2] Extract application composition and lifecycle wiring from `MainWindow.xaml.cs` into an application bootstrap service
- [ ] T013 [US2] Move WinRT endpoint projection out of the application model layer in `MidiInputDevice.cs`
- [ ] T014 [US2] Consolidate logger-factory ownership so services do not create independent logging infrastructure
- [ ] T015 [P] [US2] Normalize formatting in `MainWindow.xaml.cs`

## Dependencies

- T001-T007 are independent defect fixes, except T004 and T006 both affect shutdown behavior.
- T008 depends on T001.
- T009 depends on T004 and T006.
- T010 depends on T003.
- T012-T014 are independent refactoring work after the reliability fixes.

## Independent test criteria

- **US1:** Deactivating a route frees its channel; concurrent callbacks do not corrupt router state; shutdown completes without background exceptions; startup failures remain visible in the UI.
- **US2:** Application composition remains behaviorally identical while platform-specific construction and logging ownership are isolated.

## Implementation strategy

Deliver US1 first because it prevents routing failures and shutdown crashes. Complete the remaining concurrency tests before beginning the broader architecture refactor. Keep the architecture work incremental and preserve the existing public behavior.
