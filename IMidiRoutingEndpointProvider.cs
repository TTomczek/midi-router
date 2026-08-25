namespace midi_router;

public interface IMidiRoutingEndpoint : IDisposable
{
    event EventHandler<MidiRoutingMessage>? MessageReceived;
    bool IsOpen { get; }
    void Open();
    bool Send(MidiRoutingMessage message);
}

public interface IMidiRoutingEndpointProvider : IDisposable
{
    IMidiRoutingEndpoint OpenPhysical(string endpointDeviceId);
    IMidiRoutingEndpoint OpenVirtual(string name);
}
