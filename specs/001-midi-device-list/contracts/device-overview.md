# Device Overview Contract

## Purpose

Define the observable contract between the MIDI endpoint monitor and the WPF device
overview. This is an internal application contract, not a public network API.

## Device Snapshot

The monitor publishes a complete snapshot after initial enumeration and after each
reconciled endpoint change:

```text
Snapshot {
  devices: [
    {
      endpointDeviceId: non-empty stable identifier,
      name: non-empty display name,
      midiVersion: "MIDI 1" | "MIDI 2" | "Unknown"
    }
  ],
  state: "Loading" | "Ready" | "Empty" | "Degraded" | "Unavailable" | "Stopped",
  statusMessage: optional user-readable message
}
```

The WPF layer receives snapshots on its UI thread. It never reads or mutates the SDK
watcher's dynamic map directly.

## Event Semantics

- Initial publication occurs only after the watcher's `EnumerationCompleted` event.
- An added endpoint appears in the next snapshot with its name and MIDI version.
- A removed endpoint is absent from the next snapshot; other entries are preserved.
- Repeated add/update events for one endpoint are idempotent.
- A failed endpoint read retains valid entries and publishes `Degraded` with a status.
- An empty successful snapshot publishes `Empty`, not `Unavailable`.
- Watcher stop during application close publishes `Stopped` and prevents later UI updates.

## Logging Contract

Structured events use stable names such as `Midi.ServiceUnavailable`,
`Midi.EnumerationStarted`, `Midi.EnumerationCompleted`, `Midi.EndpointAdded`,
`Midi.EndpointRemoved`, `Midi.EndpointReadFailed`, `Midi.WatcherStopped`, and
`Midi.UpdateQueueCoalesced`. Exceptions are attached to failure events; endpoint IDs are
included only for diagnostics and must not be displayed as user-facing device names.
