# Research: MIDI Device Activity Indicators

## Decision: Reuse the existing physical-message source identity

**Rationale**: `MidiRoutingMessage` already carries `SourceDeviceId`, and `MidiRouter` already
identifies the physical endpoint before forwarding a message. Emitting an activity signal at
that boundary avoids duplicating Windows MIDI Services subscriptions and preserves the
existing adapter boundary.

**Alternatives considered**: Opening a second endpoint connection for every listed device
would duplicate device ownership and lifecycle behavior. Inferring activity from display names
would merge devices with duplicate names and violate stable identity requirements.

## Decision: Keep activity transient and non-persistent

**Rationale**: Activity represents recent communication, not user configuration. A per-row
active state with a refreshed expiration deadline satisfies the brief, survives repeated
messages predictably, and does not affect settings or routing assignments.

**Alternatives considered**: Persisting last-message timestamps would add storage and stale
state without user value. A global indicator would not distinguish simultaneous device input.

## Decision: Use the existing dispatcher and observable row properties

**Rationale**: `MidiInputDeviceViewModel` already marshals snapshot changes to the WPF
dispatcher and `MidiInputDeviceRow` already implements `INotifyPropertyChanged`. Extending
these patterns keeps UI updates thread-safe and avoids introducing a second state-management
mechanism.

**Alternatives considered**: Updating controls directly from endpoint callbacks risks thread
violations and couples transport code to WPF. A separate UI event bus would be unnecessary
for one observable row property.

## Decision: Make the device-name region absorb available width

**Rationale**: The current `GridView` uses fixed aggregate widths, allowing the list to exceed
its parent. A stretch-oriented device cell with bounded text behavior lets protocol and
channel controls retain usable space while the name region adapts to the outer element.

**Alternatives considered**: Keeping fixed widths and relying on a horizontal scrollbar
contradicts the requirement. Removing the protocol or channel columns would regress existing
device information and routing controls.

## Decision: Validate layout and timing through observable contracts

**Rationale**: Hardware-independent fakes can raise messages and assert the matching row
changes, independent rows remain unchanged, repeated messages refresh activity, and removed
rows no longer receive expiry updates. Markup or control-level checks can verify the dot
placement and non-horizontal-scrolling layout without requiring physical MIDI hardware.

**Alternatives considered**: Manual-only validation cannot reliably prove asynchronous
expiration or device isolation and conflicts with the constitution's testing requirements.
