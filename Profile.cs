namespace midi_router;

public sealed record Profile(
    string Id,
    string Name,
    DateTime LastModified,
    IReadOnlyList<string>? SelectedDeviceIds = null,
    IReadOnlyDictionary<string, int>? DeviceChannelAssignments = null);
