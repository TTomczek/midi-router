namespace midi_router;

public sealed record ApplicationSettings(
    AppearanceMode AppearanceMode = AppearanceMode.OsDefault,
    bool MinimizeToTray = false,
    IReadOnlyList<string>? SelectedDeviceIds = null,
    IReadOnlyDictionary<string, int>? DeviceChannelAssignments = null,
    string? ActiveProfileId = null);
