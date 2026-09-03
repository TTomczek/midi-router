namespace midi_router;

/// <summary>A separately persisted MIDI configuration.</summary>
public sealed record Profile
{
    public Profile()
    {
    }

    public Profile(string name)
        : this(Guid.NewGuid().ToString("N"), name, Array.Empty<string>(),
            new Dictionary<string, int>(StringComparer.Ordinal), DateTime.UtcNow)
    {
    }

    public Profile(Guid id, string name,
        IEnumerable<string>? selectedDeviceIds = null,
        IReadOnlyDictionary<string, int>? deviceChannelAssignments = null,
        DateTime? lastEdited = null)
        : this(id.ToString("N"), name, selectedDeviceIds, deviceChannelAssignments, lastEdited)
    {
    }

    public Profile(
        string id,
        string name,
        IEnumerable<string>? selectedDeviceIds = null,
        IReadOnlyDictionary<string, int>? deviceChannelAssignments = null,
        DateTime? lastEdited = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Id = id;
        Name = name.Trim();
        SelectedDeviceIds = selectedDeviceIds?.ToArray() ?? Array.Empty<string>();
        DeviceChannelAssignments = deviceChannelAssignments is null
            ? new Dictionary<string, int>(StringComparer.Ordinal)
            : new Dictionary<string, int>(deviceChannelAssignments, StringComparer.Ordinal);
        LastEdited = lastEdited ?? DateTime.UtcNow;
    }

    public string Id { get; init; } = Guid.NewGuid().ToString("N");
    public string Name { get; init; } = "Profile";
    public IReadOnlyList<string>? SelectedDeviceIds { get; init; } = Array.Empty<string>();
    public IReadOnlyDictionary<string, int>? DeviceChannelAssignments { get; init; } =
        new Dictionary<string, int>(StringComparer.Ordinal);
    public DateTime LastEdited { get; init; } = DateTime.UtcNow;

    public Profile Normalize()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            throw new ArgumentException("Profile name cannot be empty.", nameof(Name));
        }
        var devices = (SelectedDeviceIds ?? Array.Empty<string>())
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var assignments = (DeviceChannelAssignments ??
            new Dictionary<string, int>())
            .Where(pair => !string.IsNullOrWhiteSpace(pair.Key) &&
                pair.Value is >= MidiChannelAllocator.FirstChannel and <= MidiChannelAllocator.LastChannel)
            .GroupBy(pair => pair.Key, StringComparer.Ordinal)
            .Select(group => group.First())
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        return this with
        {
            Id = string.IsNullOrWhiteSpace(Id) ? Guid.NewGuid().ToString("N") : Id.Trim(),
            Name = string.IsNullOrWhiteSpace(Name) ? "Profile" : Name.Trim(),
            SelectedDeviceIds = devices,
            DeviceChannelAssignments = assignments,
            LastEdited = LastEdited == default ? DateTime.UtcNow : LastEdited
        };
    }

}
