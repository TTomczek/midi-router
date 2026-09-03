namespace midi_router;

public sealed record MidiRoutingMessage(
    IReadOnlyList<uint> Words,
    long Timestamp = 0,
    string? SourceDeviceId = null,
    int? OriginalChannel = null)
{
    public int? Channel => MidiChannelCodec.ReadChannel(Words);

}

public static class MidiChannelCodec
{
    public static int? ReadChannel(IReadOnlyList<uint> words)
    {
        if (words.Count == 0)
        {
            return null;
        }

        var messageType = (words[0] >> 28) & 0xF;
        if (messageType != 0x2)
        {
            return null;
        }

        var status = (words[0] >> 16) & 0xFF;
        var statusNibble = status >> 4;
        return statusNibble is >= 0x8 and <= 0xE
            ? (int)(status & 0xF)
            : null;
    }

    public static IReadOnlyList<uint> ReplaceChannel(
        IReadOnlyList<uint> words,
        int channel)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(channel, 0);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(channel, 15);

        if (ReadChannel(words) is null)
        {
            return words.ToArray();
        }

        var copy = words.ToArray();
        copy[0] = (copy[0] & ~0x000F0000u) | ((uint)channel << 16);
        return copy;
    }
}
