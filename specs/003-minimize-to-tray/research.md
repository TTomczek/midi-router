# Research: Minimize to Tray

## Decision: Extend the existing settings record and JSON store

**Rationale**: `ApplicationSettings` and `JsonSettingsStore` already provide the per-user
settings boundary, atomic writes, enum normalization, and the established error-reporting
pattern used by appearance settings. Adding one boolean with a disabled default preserves
existing settings files and satisfies the requested persistence without a second file or
storage abstraction.

**Alternatives considered**: A separate tray-settings file would duplicate persistence logic
and complicate startup consistency. Storing the choice only in window state would lose it on
restart.

## Decision: Reuse the existing `NotifyIcon` and window lifecycle

**Rationale**: The project already targets Windows Forms and creates one `NotifyIcon`,
`ContextMenuStrip`, restore action, and disposal path in `MainWindow`. The smallest safe change
is to gate the existing `OnStateChanged` hide behavior and change the icon interaction to the
specified single-left-click restore behavior.

**Alternatives considered**: Introducing a third-party tray library would add a dependency
without solving a current capability gap. Moving tray ownership into a new process would make
shutdown and MIDI lifecycle coordination more complex.

## Decision: Keep tray state in the main window and preserve normal close semantics

**Rationale**: The feature has one application window and an existing `OnClosed` cleanup path.
Using the normal close path for the tray stop action ensures the device view model, tray icon,
and menu are disposed consistently.

**Alternatives considered**: Calling process termination directly could bypass cleanup and
violate background-process reliability. A separate application service is unnecessary unless
additional windows or lifecycle consumers emerge.

## Decision: Use a disabled default for absent or invalid preferences

**Rationale**: This is the specified safe fallback and preserves ordinary Windows minimize
behavior for users who have not enabled the feature. Invalid persisted values must not prevent
startup.

**Alternatives considered**: Enabling tray minimization by default would change existing window
behavior unexpectedly and make the taskbar entry disappear without an explicit user choice.

## Decision: Test behavior through deterministic collaborators plus UI contracts

**Rationale**: Existing tests use xUnit, fake settings stores, and source-level XAML contracts.
The tray icon and WPF window need not be created in headless tests to verify preference
semantics, event policy, cleanup ownership, and visible settings controls.

**Alternatives considered**: Tests requiring an interactive Windows desktop would be slow,
flaky, and unsuitable for the repository's hardware-independent quality gate.
