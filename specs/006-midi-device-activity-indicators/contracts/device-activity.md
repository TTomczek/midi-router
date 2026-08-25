# UI Contract: MIDI Device Activity

## Device row contract

Each listed device row exposes:

- The existing device name and MIDI version.
- A small dot immediately before the device name.
- An inactive dot appearance by default.
- A green dot while the associated endpoint has recent MIDI activity.
- The existing selection and channel controls without activity changing their values.

The row is identified by stable endpoint ID, not by display name.

## Activity event contract

When a physical MIDI message is received:

1. The source endpoint ID is identified.
2. The matching listed row enters the active state within 250 ms.
3. Rows with different endpoint IDs remain inactive.
4. A later message from the same endpoint refreshes its active deadline.
5. After messages stop, the row returns to inactive within 2 seconds.

Messages for endpoints no longer present in the list have no visible effect.

## Layout contract

- The device list's rendered width never exceeds its outer element.
- The list does not show or require horizontal scrolling.
- The device-name region adapts when the outer element width changes.
- Long names remain bounded to that region and remain identifiable.
- Empty and connection-error states remain visible through the existing status behavior.

## Regression contract

Device enumeration, connection/disconnection updates, duplicate-name handling, selection,
channel assignment, MIDI version labels, theme behavior, and existing status messages remain
unchanged except for the added activity dot.
