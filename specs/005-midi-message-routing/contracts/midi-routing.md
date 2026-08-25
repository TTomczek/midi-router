# MIDI Routing Contract

This is the observable contract between the device list, routing core, platform adapters,
and external MIDI applications.

## Device-list contract

- Every listed device exposes a unique endpoint ID, selection state, and displayed channel.
- Displayed channels are integers 1-16.
- A valid channel change updates the row and persists by endpoint ID.
- Invalid or conflicting changes preserve the last valid assignment and report the reason.

## Physical-to-virtual contract

- Input is accepted only from selected, connected devices.
- Each accepted channel-bearing message is emitted once to the shared virtual endpoint
  with that device's assigned channel.
- Other message data is unchanged; channel-less messages are emitted unchanged.

## Virtual-to-physical contract

- A message on an active assigned channel is sent only to its associated device.
- The assigned channel is replaced with that device's original channel.
- Unknown or ambiguous channels produce no physical send and a diagnostic event.
- A failed send is reported and does not terminate unrelated routes.

## Lifecycle contract

- Selection starts or stops that device's route without changing other routes.
- Disconnection stops its route; reconnection restores persisted configuration by ID.
- Closing disposes physical and virtual connections and stops workers cleanly.
