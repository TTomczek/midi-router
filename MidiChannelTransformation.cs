namespace midi_router;

public sealed class MidiChannelTransformation : IMidiMessageTransformation
{
    public MidiRoutingMessage Forward(MidiRoutingMessage message, int assignedChannel)
        => message with
        {
            Words = MidiChannelCodec.ReplaceChannel(message.Words, assignedChannel),
            OriginalChannel = message.Channel
        };

    public MidiRoutingMessage Reverse(MidiRoutingMessage message, int originalChannel)
        => message with
        {
            Words = MidiChannelCodec.ReplaceChannel(message.Words, originalChannel)
        };
}
