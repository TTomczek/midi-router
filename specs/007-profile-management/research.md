# Profile Management Research

## Decision: Separate profile files with atomic JSON replacement

**Rationale**: The existing application already uses JSON settings and atomic temporary-file replacement. Reusing that serialization and write pattern keeps the feature consistent, makes each profile independently readable/deletable, and avoids introducing a database for a small local collection.

**Alternatives considered**: A single profiles index file would violate the requirement that every profile be saved separately. A database would add unnecessary deployment and migration complexity.

## Decision: Keep application-wide settings separate from profile state

**Rationale**: Appearance mode and minimize-to-tray behavior apply to the application, not to a person/device setup. Device selection and channel assignments move to the active profile so switching changes only profile-owned routing configuration.

**Alternatives considered**: Duplicating global settings into every profile would create conflicting sources of truth and make global preferences unexpectedly change during profile switching.

## Decision: Stable generated profile identifiers

**Rationale**: File names and selection values need to remain stable even when a profile is renamed or two profiles share a name. A generated identifier is separate from the human-readable name and is safe for file naming.

**Alternatives considered**: Using the name as the file name would require unsafe character handling and would make renaming/deleting ambiguous for duplicate names.

## Decision: Sequential duplicate labels are derived at display time

**Rationale**: The stored name remains the exact trimmed user name. Visible labels are recalculated in list order, with the first duplicate shown without a suffix and later duplicates shown as `Name (2)`, `Name (3)`, and so on. This prevents stale numbering after rename or deletion.

**Alternatives considered**: Persisting suffixes would make presentation data part of the profile identity and could produce gaps or unstable labels.

## Decision: Profile manager owns lifecycle and persistence error boundaries

**Rationale**: Creation, rename, deletion, active selection, last-edited updates, and minimum-one-profile enforcement form one cohesive state machine. The manager can reject invalid operations before writing and can keep the last successfully saved state when a write fails.

**Alternatives considered**: Letting the WPF code manipulate files directly would couple UI events to storage and make lifecycle rules difficult to test.

## Decision: Migrate existing device/channel settings into the initial profile

**Rationale**: Existing users should retain their current device and channel configuration when profiles are first introduced. On first profile initialization, those legacy values become the initial profile's state; subsequent profile files are authoritative.

**Alternatives considered**: Ignoring legacy values would silently reset existing routing behavior. Keeping both legacy and profile values indefinitely would create conflicting persistence sources.

## Decision: Modal WPF name dialog with explicit commit and cancel

**Rationale**: Creation and rename use one consistently styled modal dialog, keeping the drop-down compact while providing a clear focused editing surface. Enter or confirmation commits a trimmed non-empty name; Escape, closing, or invalid input leaves the profile unchanged and reports validation feedback consistently with existing status behavior.

**Alternatives considered**: Inline editing in the drop-down is less consistent with the application's other focused interactions and makes the compact profile list harder to use.
