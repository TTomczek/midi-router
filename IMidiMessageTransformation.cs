namespace midi_router;

public interface IMidiMessageTransformation
{
    MidiRoutingMessage Forward(MidiRoutingMessage message, int assignedChannel);
    MidiRoutingMessage Reverse(MidiRoutingMessage message, int originalChannel);
}
