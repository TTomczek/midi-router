namespace midi_router;

public sealed class MidiChannelAllocator
{
    public const int FirstChannel = 0;
    public const int LastChannel = 15;

    private readonly Dictionary<string, int> assignments = new(StringComparer.Ordinal);

    public IReadOnlyDictionary<string, int> Assignments => assignments;

    public bool TryAssignNext(string deviceId, out int channel)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(deviceId);
        if (assignments.ContainsKey(deviceId))
        {
            channel = assignments[deviceId];
            return true;
        }

        var used = assignments.Values.ToHashSet();
        for (var candidate = FirstChannel; candidate <= LastChannel; candidate++)
        {
            if (!used.Contains(candidate))
            {
                assignments[deviceId] = candidate;
                channel = candidate;
                return true;
            }
        }

        channel = default;
        return false;
    }

    public bool TryAssign(string deviceId, int channel)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(deviceId);
        if (channel is < FirstChannel or > LastChannel)
        {
            return false;
        }

        if (assignments.TryGetValue(deviceId, out var current) && current == channel)
        {
            return true;
        }

        if (assignments.Values.Contains(channel))
        {
            return false;
        }

        assignments[deviceId] = channel;
        return true;
    }

    public bool Remove(string deviceId) => assignments.Remove(deviceId);

    public void Load(IEnumerable<KeyValuePair<string, int>> saved)
    {
        assignments.Clear();
        foreach (var pair in saved)
        {
            TryAssign(pair.Key, pair.Value);
        }
    }
}
