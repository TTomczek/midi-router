# Research: MIDI Device List

## Decision: Use `MidiEndpointDeviceWatcher` as the source of record

**Rationale**: The official Windows MIDI Services SDK describes
`Windows.Devices.Midi2.Enumeration.MidiEndpointDeviceWatcher` as the recommended
enumeration class for applications that need add, remove, and property-change
notifications. Its `EnumeratedEndpointDevices` map is dynamic and keyed by the full
endpoint device ID, so the application should look up current objects by ID rather than
cache removed objects.

**Alternatives considered**: Repeated `FindAll()` snapshots would require polling,
increase latency, and create race windows. The generic Windows device watcher would
require duplicating MIDI-specific filtering and property retrieval.

## Decision: Enumerate `AllStandardEndpoints`

**Rationale**: The SDK documents `AllStandardEndpoints` as the normal application filter:
it includes native Universal MIDI Packet Format endpoints and native MIDI 1 byte-stream
endpoints while excluding diagnostics and responder endpoints. This directly matches the
feature's MIDI 1 and MIDI 2 scope.

**Alternatives considered**: Enumerating only native UMP endpoints would omit MIDI 1
devices. Enumerating all endpoint categories would expose diagnostic/internal endpoints
that users should not see.

## Decision: Classify displayed version from endpoint native format

**Rationale**: `StandardNativeUniversalMidiPacketFormat` represents native UMP endpoints,
which are displayed as MIDI 2; `StandardNativeMidi1ByteFormat` represents native MIDI 1
byte-stream endpoints. The adapter must retain the endpoint ID as identity and must not
infer version from a mutable display name.

**Alternatives considered**: Using translated port names is unstable and can change with
user configuration. Showing both translated and native representations would duplicate
one physical endpoint and violate the feature's one-entry requirement.

## Decision: Keep SDK callbacks short and serialize work off the callback path

**Rationale**: The SDK guidance says watcher events are raised synchronously and
long-running work must not occur in handlers. Handlers will capture the event/endpoint
ID, enqueue a bounded/coalesced change, and return. A single coordinator will serialize
reconciliation and publish immutable snapshots to the UI dispatcher.

**Alternatives considered**: Updating WPF collections directly from callbacks risks
cross-thread exceptions and reentrancy. Uncoordinated tasks can apply stale snapshots
after newer device changes.

## Decision: Explicit operational states and structured logs

**Rationale**: Service availability, enumeration completion, empty results, update
failures, and shutdown are distinct states. Log entries should use stable event names,
include endpoint IDs only where diagnostically necessary, and never turn an exception
into a false successful empty list. User-facing state should remain concise while
diagnostic details go to logging.

**Alternatives considered**: Silent fallback to an empty list hides service failures and
can cause users to route to stale or missing devices. Logging only unstructured strings
reduces supportability for a background process.

## Sources

- [Windows MIDI Services SDK reference](https://microsoft.github.io/MIDI/sdk-reference/)
- [MidiEndpointDeviceWatcher reference](https://github.com/microsoft/MIDI/blob/main/docs/sdk-reference/Enumeration/MidiEndpointDeviceWatcher.md)
- [MidiEndpointDeviceInformation reference](https://github.com/microsoft/MIDI/blob/main/docs/sdk-reference/Enumeration/MidiEndpointDeviceInformation.md)
- [How to watch endpoints](https://github.com/microsoft/MIDI/blob/main/docs/kb/how-to-watch-endpoints.md)
- [MidiApi reference](https://github.com/microsoft/MIDI/blob/main/docs/sdk-reference/MidiApi.md)
