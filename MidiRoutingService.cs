using System.Runtime.InteropServices;
using Windows.Devices.Midi2;
using Windows.Devices.Midi2.Enumeration;
using Windows.Devices.Midi2.Transports.Virtual;
using Windows.Devices.Midi2.Utilities.Messages;

namespace midi_router;

public interface IMidiRoutingService : IDisposable
{
    void Start(IReadOnlyList<MidiInputDevice> devices);
    void Stop();
    void SetTargetChannel(MidiInputDevice device, int channel);
}

public sealed class MidiRoutingException(string message, Exception innerException)
    : InvalidOperationException(message, innerException);

public sealed class NullMidiRoutingService : IMidiRoutingService
{
    public void Start(IReadOnlyList<MidiInputDevice> devices) { }
    public void Stop() { }
    public void SetTargetChannel(MidiInputDevice device, int channel) { }
    public void Dispose() { }
}

public sealed class WindowsMidiRoutingService : IMidiRoutingService
{
    private readonly Dictionary<string, MidiRoute> _routes = [];
    private MidiSession? _session;

    public void Start(IReadOnlyList<MidiInputDevice> devices)
    {
        Stop();

        try
        {
            if (!MidiApi.EnsureServiceAvailable())
            {
                throw new InvalidOperationException("Windows MIDI Services ist nicht verfügbar.");
            }

            if (!MidiVirtualDeviceManager.IsTransportAvailable)
            {
                throw new InvalidOperationException("Der virtuelle MIDI-Transport ist nicht verfügbar.");
            }

            _session = MidiSession.Create("MIDI Router");
            foreach (var device in devices)
            {
                if (string.IsNullOrWhiteSpace(device.EndpointDeviceId))
                {
                    throw new InvalidOperationException($"Das MIDI-Gerät \"{device.Name}\" besitzt keine Endpoint-ID.");
                }

                var route = MidiRoute.Create(_session, device, device.TargetChannel);
                _routes.Add(device.EndpointDeviceId, route);
            }
        }
        catch (Exception exception) when (exception is COMException or InvalidOperationException or UnauthorizedAccessException)
        {
            Stop();
            throw new MidiRoutingException(
                $"MIDI-Eingabegeräte konnten nicht auf virtuelle Ports geroutet werden: {exception.Message}",
                exception);
        }
    }

    public void SetTargetChannel(MidiInputDevice device, int channel)
    {
        ArgumentNullException.ThrowIfNull(device);
        ValidateChannel(channel);

        if (_routes.TryGetValue(device.EndpointDeviceId, out var route))
        {
            route.TargetChannel = channel;
        }
    }

    public void Stop()
    {
        foreach (var route in _routes.Values)
        {
            route.Dispose();
        }

        _routes.Clear();
        _session?.Dispose();
        _session = null;
    }

    public void Dispose() => Stop();

    private static void ValidateChannel(int channel)
    {
        if (channel is < 1 or > 16)
        {
            throw new ArgumentOutOfRangeException(nameof(channel), channel, "Ein MIDI-Kanal muss zwischen 1 und 16 liegen.");
        }
    }

    private sealed class MidiRoute : IDisposable
    {
        private readonly MidiVirtualDevice _virtualDevice;
        private readonly MidiEndpointConnection _inputConnection;
        private readonly MidiEndpointConnection _outputConnection;
        private MidiChannel _targetChannel = new(0);

        private MidiRoute(
            MidiVirtualDevice virtualDevice,
            MidiEndpointConnection inputConnection,
            MidiEndpointConnection outputConnection,
            int targetChannel)
        {
            _virtualDevice = virtualDevice;
            _inputConnection = inputConnection;
            _outputConnection = outputConnection;
            TargetChannel = targetChannel;
            _inputConnection.MessageReceived += OnMessageReceived;
        }

        public int TargetChannel
        {
            set
            {
                if (value is < 1 or > 16)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, "Ein MIDI-Kanal muss zwischen 1 und 16 liegen.");
                }

                _targetChannel = new MidiChannel((byte)(value - 1));
            }
        }

        public static MidiRoute Create(MidiSession session, MidiInputDevice device, int targetChannel)
        {
            var endpointInfo = new MidiDeclaredEndpointInfo(
                $"MIDI Router - {device.Name}",
                $"MIDI_ROUTER_{Guid.NewGuid():N}",
                supportsMidi10Protocol: true,
                supportsMidi20Protocol: true,
                supportsReceivingJitterReductionTimestamps: false,
                supportsSendingJitterReductionTimestamps: false,
                hasStaticFunctionBlocks: false,
                declaredFunctionBlockCount: 0,
                specificationVersionMajor: 1,
                specificationVersionMinor: 1);
            var userSuppliedInfo = new MidiEndpointUserSuppliedInfo
            {
                Name = $"MIDI Router - {device.Name}",
                Description = $"Gerouteter MIDI-Eingang von {device.Name}"
            };
            var config = new MidiVirtualDeviceCreationConfig(
                $"MIDI Router - {device.Name}",
                $"Eingang von {device.Name}",
                "MIDI Router",
                endpointInfo,
                new MidiDeclaredDeviceIdentity(),
                userSuppliedInfo);
            config.CreateOnlyUmpEndpoints = false;

            var functionBlock = new MidiFunctionBlock
            {
                Number = 0,
                Name = device.Name,
                IsActive = true,
                UIHint = MidiFunctionBlockUIHint.Bidirectional,
                FirstGroup = new MidiGroup(0),
                GroupCount = 1,
                Direction = MidiFunctionBlockDirection.Bidirectional,
                RepresentsMidi10Connection = MidiFunctionBlockRepresentsMidi10Connection.Not10,
                MaxSystemExclusive8Streams = 0,
                MidiCIMessageVersionFormat = 0
            };
            config.FunctionBlocks.Add(functionBlock);

            var virtualDevice = MidiVirtualDeviceManager.CreateVirtualDevice(config);
            if (virtualDevice is null)
            {
                throw new InvalidOperationException(
                    $"Der virtuelle MIDI-Port für \"{device.Name}\" konnte nicht erstellt werden. " +
                    "Bitte prüfen Sie, ob der virtuelle MIDI-Transport in Windows MIDI Services verfügbar ist.");
            }

            var inputConnection = session.CreateEndpointConnection(device.EndpointDeviceId);
            var outputConnection = session.CreateEndpointConnection(virtualDevice.DeviceEndpointDeviceId);
            if (inputConnection is null || outputConnection is null)
            {
                throw new InvalidOperationException($"Die MIDI-Verbindungen für \"{device.Name}\" konnten nicht erstellt werden.");
            }

            outputConnection.AddMessageProcessingPlugin(virtualDevice);
            if (!inputConnection.Open() || !outputConnection.Open())
            {
                throw new InvalidOperationException($"Die Verbindung für \"{device.Name}\" konnte nicht geöffnet werden.");
            }

            return new MidiRoute(virtualDevice, inputConnection, outputConnection, targetChannel);
        }

        private void OnMessageReceived(IMidiMessageReceivedEventSource sender, MidiMessageReceivedEventArgs args)
        {
            var packet = args.GetMessagePacket();
            var words = packet.GetAllWords();

            if (packet.MessageType is MidiMessageType.Midi1ChannelVoice32 or MidiMessageType.Midi2ChannelVoice64)
            {
                words[0] = MidiMessageHelper.ReplaceChannelInMessageFirstWord(words[0], _targetChannel);
            }

            _outputConnection.SendMultipleMessagesWordList(args.Timestamp, words);
        }

        public void Dispose()
        {
            _inputConnection.MessageReceived -= OnMessageReceived;
        }
    }
}
