# Research: MIDI Message Routing

## Decision: Use Windows MIDI Services sessions and endpoint connections

**Rationale**: The project already uses `Windows.Devices.Midi2` for endpoint enumeration.
The SDK reference identifies `MidiSession` as the owner of endpoint connections and
`MidiEndpointConnection` as the send/receive boundary. Connections are configured before
`Open()` and should be disconnected through the owning session.

**Alternatives considered**: Legacy Windows MIDI 1.0 APIs would duplicate the existing
MIDI 2.0 integration and weaken UMP support.

## Decision: Queue work from message callbacks

**Rationale**: `MessageReceived` is synchronous and the SDK warns that handlers must be
fast enough to drain incoming queues. The adapter copies or normalizes the packet and
enqueues it; transformation and sending happen outside the callback.

**Alternatives considered**: Persistence, UI updates, or endpoint sends directly in the
callback could block receipt and affect unrelated routes.

## Decision: Use reversible pipeline stages

**Rationale**: The channel change must be reversed for messages returning from the virtual
endpoint, and future modifications must be addable without changing route ownership.

**Alternatives considered**: A channel-only conditional in the view model couples UI state
to transport and cannot support future modifications.

## Decision: Assign unique internal channels 0-15

**Rationale**: The specification defines 0 as the first internal channel and 15 as maximum.
The UI adds one for the 1-16 representation. Unique active channels make return routing
unambiguous.

**Alternatives considered**: Duplicate channels would make responses ambiguous and require
arbitrary routing, which the specification forbids.

## Decision: Isolate the virtual endpoint behind an adapter

**Rationale**: The supplied SDK reference documents sessions and endpoint connections, but
the exact virtual-device creation surface is version-sensitive and not exposed by the
landing page. The pinned package must be checked during implementation; portable tests use
an adapter contract.

**Alternatives considered**: Referencing WinRT virtual-device types throughout the core
would make tests hardware-dependent and violate the constitution.

## Sources

- [Windows MIDI Services SDK reference](https://microsoft.github.io/MIDI/sdk-reference/)
- [`MidiSession`](https://raw.githubusercontent.com/microsoft/MIDI/main/docs/sdk-reference/MidiSession.md)
- [`MidiEndpointConnection`](https://raw.githubusercontent.com/microsoft/MIDI/main/docs/sdk-reference/MidiEndpointConnection.md)
- [`IMidiMessageReceivedEventSource`](https://raw.githubusercontent.com/microsoft/MIDI/main/docs/sdk-reference/IMidiMessageReceivedEventSource.md)
- [`MidiMessage32`](https://raw.githubusercontent.com/microsoft/MIDI/main/docs/sdk-reference/MidiMessage32.md)
