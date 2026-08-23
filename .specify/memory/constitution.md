<!--
Sync Impact Report
- Version change: none → 1.0.0
- Modified principles: none; initial constitution established
- Added sections: Core Principles, Technical Constraints, Development Workflow
- Removed sections: none
- Follow-up TODOs: RATIFICATION_DATE requires confirmation if 2026-08-21 is not the adoption date
-->
# Midi-Router Constitution

## Core Principles

### I. Lightweight and Modular
Midi-Router MUST remain lightweight and modular. Features MUST have a clear,
limited responsibility and MUST interact through explicit, stable contracts.
Dependencies between modules MUST be kept minimal and justified. This keeps the
background process maintainable, testable, and suitable for incremental extension.

### II. Background-Process First
The application MUST treat unattended background execution as the primary
runtime scenario. Features MUST avoid blocking the process, MUST support orderly
startup and shutdown, and MUST handle recoverable failures without terminating
unrelated functionality. User-interface concerns MUST remain separate from
background processing concerns.

### III. Tested Features
Every feature MUST have automated tests covering its intended behavior and
important failure paths before it is considered complete. Tests MUST be
repeatable and MUST run independently of physical MIDI hardware where practical;
hardware-dependent behavior MUST be isolated behind testable abstractions. This
ensures that modular background behavior remains reliable as the application grows.

### IV. Documented Behavior
Every feature MUST include user-facing and developer-facing documentation
appropriate to its scope. Documentation MUST describe configuration, observable
behavior, usage, and relevant limitations. Public contracts and behavior changes
MUST be updated in the same change as the implementation so the documentation
remains an accurate operational reference.

### V. Simplicity and Explicit Change
The implementation MUST prefer the simplest design that satisfies the
requirements. New abstractions, dependencies, and runtime complexity MUST have a
clear benefit and MUST be justified in the change description. Breaking changes
MUST be identified explicitly and accompanied by migration guidance when needed.

## Technical Constraints

Midi-Router MUST target .NET 8 on Windows and MUST follow the existing WPF
application model unless a documented architectural decision changes that
constraint. Core routing and processing logic MUST remain separable from WPF so
that it can be tested without a graphical environment. Configuration and external
MIDI resources MUST be accessed through replaceable abstractions where they affect
feature behavior.

## Development Workflow

Changes MUST be reviewed against this constitution. A feature is complete only
when its implementation, automated tests, and relevant documentation are present
and validated. Changes MUST include tests for regressions and MUST preserve the
existing module boundaries. Reviewers MUST reject unexplained complexity,
untested feature behavior, or stale documentation.

## Governance

This constitution defines the non-negotiable engineering principles for
Midi-Router and supersedes conflicting project practices. Amendments MUST be
proposed as repository changes, explain their rationale and impact, and update
the version and last-amended date. Changes to principles or sections MUST be
reviewed before merging.

The constitution uses semantic versioning: MAJOR for incompatible removals or
redefinitions, MINOR for new principles or materially expanded guidance, and
PATCH for clarifications and non-semantic wording changes. Every feature change
and review MUST verify compliance with the current constitution. Any exception
MUST be documented with its scope, rationale, owner, and expiration or review
date.

**Version**: 1.0.0 | **Ratified**: TODO(RATIFICATION_DATE): confirm adoption date | **Last Amended**: 2026-08-22
