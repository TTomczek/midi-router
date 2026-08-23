# Phase 0 Research: Input MIDI Device Browser UI

## Decision 1: Use Windows MIDI Services as the only MIDI backend

- **Decision**: All MIDI actions for this feature will use Windows MIDI Services APIs, replacing legacy WinMM-based enumeration in the provider implementation path.
- **Rationale**: The planning directive explicitly requires Windows MIDI Services for all MIDI actions. A single backend removes ambiguity, reduces divergent behavior, and aligns future MIDI functionality with one runtime model.
- **Alternatives considered**:
  - Continue with WinMM (`winmm.dll`) enumeration: rejected due to explicit directive.
  - Add dual backend (WinMM + Windows MIDI Services): rejected because extra branching adds complexity and weakens consistency.

## Decision 2: Preserve abstraction boundary with provider contract

- **Decision**: Keep `IMidiInputDeviceProvider` as the UI-facing contract and implement its behavior via Windows MIDI Services internally.
- **Rationale**: This preserves modularity, keeps UI logic unchanged, and allows tests to remain hardware-independent with stubs/mocks.
- **Alternatives considered**:
  - Bind UI directly to Windows MIDI Services calls: rejected because it couples UI to platform APIs and reduces testability.
  - Introduce multiple new intermediary layers immediately: rejected as unnecessary for current scope.

## Decision 3: Keep refresh user-driven for this feature slice

- **Decision**: Maintain explicit manual refresh behavior as the primary update flow for this feature.
- **Rationale**: Manual refresh is already in the accepted scope and is independently testable. It also avoids introducing extra lifecycle/event complexity during backend migration.
- **Alternatives considered**:
  - Automatic hot-plug detection in this feature: deferred; useful but outside current scoped requirement.
  - Continuous background polling: rejected to avoid unnecessary runtime churn for this initial slice.

## Decision 4: Validate through unit tests and runnable UX checks

- **Decision**: Validate behavior with existing xUnit view-model tests plus quickstart-driven manual UI checks for real devices.
- **Rationale**: This satisfies constitution requirements for tested and documented behavior while respecting hardware-independent automated testing.
- **Alternatives considered**:
  - Hardware-dependent automated tests: rejected for CI reliability and repeatability concerns.
  - Manual-only verification: rejected because it would weaken regression safety.
