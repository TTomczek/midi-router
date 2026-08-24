# Device Selection Contract

## Scope

This contract defines the observable behavior between the device-list UI, its view model,
and later processing features. It is not a public network API.

## Device Row Interaction

For each currently listed `MidiInputDevice`:

- one row click toggles the selection for that row's `EndpointDeviceId`;
- a selected row is highlighted;
- an unselected row is not highlighted;
- selecting one row does not clear other selected rows;
- two rows with the same display name remain independently selectable.

## Selection State

The view-model selection state exposes the selected unique device IDs independently of row
visual styling. Consumers must compare device IDs, not display names, to determine whether a
device is selected.

## Persistence Contract

- Every successful toggle updates the persisted selected ID set.
- Loading restores selection for currently available matching IDs.
- Saved IDs for unavailable devices are retained but do not create visible rows.
- A device reconnecting with the same unique ID regains its selection.
- Persistence failures are surfaced through the existing status or diagnostic mechanism and
  do not stop device monitoring or clear unrelated settings.

## Snapshot Contract

When a device snapshot is applied, the visible list contains the snapshot's current devices,
and each row's selected state is derived only from its unique device ID and saved/current
selection set. Existing selected IDs not present in the snapshot remain eligible for
reconnection.
