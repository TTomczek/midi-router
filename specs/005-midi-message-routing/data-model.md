# Data Model: MIDI Message Routing

## MIDI Device

- `EndpointDeviceId`: stable unique endpoint identity; required and case-sensitive.
- `Name`: display name; not used for routing identity.
- `Version`: known MIDI protocol version.
- `IsSelected`: current user selection; active routing follows this state.
- `IsAvailable`: whether the endpoint is connected.

## Device Channel Assignment

- `EndpointDeviceId`: key to the device.
- `InternalChannel`: nullable before allocation; otherwise an integer from 0 through 15.
- `DisplayChannel`: derived as `InternalChannel + 1`, shown as 1 through 16.
- `IsExplicit`: optional UI distinction between configured and automatic allocation.

Validation: internal values outside 0-15 and user-facing values outside 1-16 are invalid.
Two active selected devices may not share a channel. Saved assignments for unavailable
devices do not reserve an active channel.

## MIDI Message

- `Words`: complete UMP words in transmission order.
- `Timestamp`: source timestamp when available.
- `Channel`: optional parsed channel for channel-bearing messages.
- `SourceDeviceId`: populated for physical input.
- `AssignedChannel`: channel used at the virtual endpoint.
- `OriginalChannel`: channel restored for reverse routing.

Channel transformation changes only channel bits for channel-bearing messages. Channel-less
messages retain their words and are routed without replacement.

## Routing Pipeline

- `PhysicalRoute`: selected device identity, input connection, output capability, and state.
- `VirtualRoute`: shared virtual endpoint connection and receive state.
- `ChannelMap`: active bijection between assigned channel and device identity.
- `TransformationStages`: ordered reversible stages, initially channel replacement only.

States are `Inactive -> Starting -> Active -> Degraded -> Stopping -> Inactive`. A device
failure changes only that route; a virtual endpoint failure affects the shared route and is
surfaced.

## Persistence

Extend `ApplicationSettings` with a collection or map of endpoint IDs to internal channel
assignments. Normalize invalid IDs and values on load. Save valid changes through
`ApplicationSettingsCoordinator`; report failures while retaining in-memory state.
