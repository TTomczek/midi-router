namespace midi_router;

public interface IMidiInputDeviceProvider : IDisposable
{
    event EventHandler? DevicesChanged;
    event EventHandler<Exception>? ProviderError;
    IReadOnlyDictionary<string, MidiInputDevice> CurrentDevices { get; }
    bool IsAvailable { get; }
    void Start();
}
