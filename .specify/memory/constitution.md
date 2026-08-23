<!--
Sync Impact Report
- Version change: unratified scaffold -> 1.0.0
- Modified principles: placeholder principles -> Modular Architecture; Background-Process
  Reliability; Test-Driven Development; Hardware-Isolated Integration Testing; Simplicity
  and Extensibility
- Added sections: Platform and Runtime Constraints; Development Workflow and Quality Gates
- Removed sections: none
- Follow-up TODOs: RATIFICATION_DATE remains TODO because the original adoption date is
  not documented in the repository.
-->

# MIDI Router Constitution

## Core Principles

### I. Modular Architecture
The application MUST be organized into focused, independently understandable modules with
explicit interfaces. Device discovery, message routing, channel transformation, user
interface, and process-lifecycle concerns MUST remain separable so that a change in one
concern does not require unrelated changes. This preserves the lightweight design and
enables replacement or extension of platform integrations.

### II. Background-Process Reliability
The router MUST remain safe and predictable while operating primarily as a background
process. Message handling MUST avoid blocking the routing path, failures in one device
MUST NOT silently corrupt or stop unrelated routes, and lifecycle transitions such as
startup, minimization to the Windows tray, shutdown, and device refresh MUST have explicit
behavior. Operational failures MUST be surfaced through the application's established
status or diagnostic mechanisms.

### III. Test-Driven Development
New behavior and behavior changes MUST follow the red-green-refactor cycle: a failing
automated test is written first, the smallest implementation is added to make it pass,
and the design is then improved without changing behavior. Tests MUST cover observable
contracts, including message transformation, routing decisions, device enumeration
states, and failure handling. This is non-negotiable because the application interacts
with hardware and asynchronous background processes that are difficult to validate
manually.

### IV. Hardware-Isolated Integration Testing
Tests MUST be runnable without physical MIDI hardware. Platform and device access MUST
be abstracted behind interfaces or providers, and integration tests MUST use deterministic
fakes or test doubles for unavailable hardware. Tests that verify contracts across
modules MUST be added when a shared interface, message format, or device lifecycle
behavior changes, protecting the boundary between portable application logic and
Windows MIDI Services.

### V. Simplicity and Extensibility
Implementations MUST choose the smallest design that satisfies the current requirement
while preserving clear extension points for additional MIDI versions, message types,
devices, and routing policies. New abstractions MUST have a demonstrated consumer or
testable contract; speculative frameworks and duplicate pathways MUST NOT be introduced.
This keeps the application lightweight without making future supported behavior costly to
add.

## Platform and Runtime Constraints

MIDI Router MUST target Windows and .NET 8 as documented project requirements. Windows
MIDI Services is the authoritative runtime integration for MIDI device discovery and
endpoints. Platform-specific code MUST be isolated behind testable boundaries, and the
application MUST handle unavailable runtime services or empty device lists with an
explicit user-visible or diagnostic status rather than an unexplained failure.

## Development Workflow and Quality Gates

Every change MUST include or update automated tests for its externally observable
behavior. A change is ready for review only when `dotnet build` and `dotnet test` pass from
the repository root, and tests do not depend on physical MIDI hardware. Reviews MUST
check modularity, lifecycle safety, error visibility, and constitution compliance.
Documentation MUST be updated when user-visible behavior, runtime prerequisites, or
development commands change.

## Governance

This constitution is the highest-level project governance document. When another
practice conflicts with it, the constitution takes precedence until it is amended.
Contributors MUST identify applicable principles during design and review, and reviewers
MUST reject changes that violate a non-negotiable rule unless the constitution is amended
in the same change.

Amendments MUST document the affected principles or sections, the reason for the change,
and any migration or compatibility impact. The amendment MUST update the version and last
amended date. Versioning follows semantic versioning: MAJOR for incompatible removals or
redefinitions, MINOR for new principles or materially expanded governance, and PATCH for
clarifications and non-semantic corrections.

Compliance MUST be reviewed for every feature plan, implementation, and pull request.
The development workflow quality gates are the minimum evidence of compliance; a
maintainer MAY require additional tests or review for changes affecting hardware,
asynchronous processing, or public interfaces. Any unresolved exception MUST be recorded
with an owner and a plan to remove it.

**Version**: 1.0.0 | **Ratified**: TODO(RATIFICATION_DATE): original adoption date is not documented | **Last Amended**: 2026-08-23
