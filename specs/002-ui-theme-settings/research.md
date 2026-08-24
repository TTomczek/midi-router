# Research: UI Theme Settings

## Decision: Use WPF application resource dictionaries for the theme palettes

**Rationale**: WPF resources allow the light and dark palettes to share one set of named
brushes, text styles, control styles, and layout values. Replacing the active palette at
application scope updates the existing window and settings menu consistently without
duplicating view logic.

**Alternatives considered**: Inline colors in each control would be difficult to keep
consistent and would make OS-driven switching incomplete. A new third-party theme framework
would add dependencies and complexity beyond the three required modes.

## Decision: Persist only the semantic appearance mode in JSON

**Rationale**: The setting needs to distinguish an explicit Light or Dark choice from OS
default, so the file stores one validated value rather than a resolved color scheme. This
allows OS default to follow later operating-system changes.

**Alternatives considered**: Persisting resolved colors would make OS default stale.
Registry-backed application settings would not satisfy the requirement for a settings file
and would couple persistence to a platform-specific storage mechanism.

## Decision: Resolve OS default through an isolated Windows provider

**Rationale**: The Windows provider can read the user's configured application appearance
and subscribe to the corresponding user-preference change notification. Keeping this behind
an interface makes mode resolution and change handling deterministic in tests.

**Alternatives considered**: Reading the OS preference directly from the window would couple
view code to Windows APIs and make failure behavior difficult to test. Checking only once at
startup would not satisfy the requirement to follow OS changes while OS default is active.

## Decision: Treat settings failures as non-fatal and diagnostically visible

**Rationale**: A missing or invalid preference must fall back to OS default, while read/write
errors must remain distinguishable from an intentionally empty configuration. The existing
application logging/status mechanisms can surface a concise warning without stopping MIDI
device discovery.

**Alternatives considered**: Treating every file error as an empty successful settings file
would hide operational problems. Failing application startup would make an optional
appearance preference disrupt the router.

## Sources

- [WPF Resource Management](https://learn.microsoft.com/dotnet/desktop/wpf/advanced/resources-overview)
- [WPF Control Authoring and Styling](https://learn.microsoft.com/dotnet/desktop/wpf/controls/control-authoring-overview)
- [Windows personalization registry settings](https://learn.microsoft.com/windows/apps/desktop/modernize/apply-windows-themes)
