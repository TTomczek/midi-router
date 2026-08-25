using Windows.Devices.Midi2;
using Windows.Devices.Midi2.Enumeration;
using Windows.Devices.Midi2.Transports.Virtual;

namespace midi_router;

public sealed class WindowsMidiRoutingEndpointProvider : IMidiRoutingEndpointProvider
{
    private readonly MidiSession session;
    private readonly List<WindowsMidiRoutingEndpoint> endpoints = new();
    private MidiVirtualDevice? virtualDevice;
    private int disposed;

    public WindowsMidiRoutingEndpointProvider(
        string sessionName = MidiRoutingConstants.VirtualDeviceName)
    {
        if (!MidiApi.EnsureServiceAvailable())
        {
            throw new InvalidOperationException("Windows MIDI Services is unavailable.");
        }

        session = MidiSession.Create(sessionName);
    }

    public IMidiRoutingEndpoint OpenPhysical(string endpointDeviceId)
    {
        ObjectDisposedException.ThrowIf(Volatile.Read(ref disposed) != 0, this);
        var endpoint = new WindowsMidiRoutingEndpoint(
            session.CreateEndpointConnection(endpointDeviceId));
        endpoints.Add(endpoint);
        return endpoint;
    }

    public IMidiRoutingEndpoint OpenVirtual(string name)
    {
        ObjectDisposedException.ThrowIf(Volatile.Read(ref disposed) != 0, this);
        if (virtualDevice is not null)
        {
            return endpoints.Single(endpoint => endpoint.IsVirtual);
        }

        var declaredEndpointInfo = new MidiDeclaredEndpointInfo
        {
            Name = name,
            ProductInstanceId = "MIDI_ROUTER",
            SpecificationVersionMajor = 1,
            SpecificationVersionMinor = 1,
            SupportsMidi10Protocol = true,
            SupportsMidi20Protocol = false,
            SupportsReceivingJitterReductionTimestamps = false,
            SupportsSendingJitterReductionTimestamps = false,
            HasStaticFunctionBlocks = false
        };
        var config = new MidiVirtualDeviceCreationConfig(
            name,
            "MIDI Router virtual endpoint",
            MidiRoutingConstants.VirtualDeviceName,
            declaredEndpointInfo)
        {
            CreateOnlyUmpEndpoints = false
        };

        virtualDevice = MidiVirtualDeviceManager.CreateVirtualDevice(config);
        var endpoint = new WindowsMidiRoutingEndpoint(
            session.CreateEndpointConnection(virtualDevice.DeviceEndpointDeviceId),
            virtualDevice);
        endpoints.Add(endpoint);
        return endpoint;
    }

    public void Dispose()
    {
        if (Interlocked.Exchange(ref disposed, 1) != 0)
        {
            return;
        }

        foreach (var endpoint in endpoints.ToArray())
        {
            session.DisconnectEndpointConnection(endpoint.ConnectionId);
            endpoint.Dispose();
        }

        endpoints.Clear();
        session.Dispose();
        virtualDevice = null;
    }

    private sealed class WindowsMidiRoutingEndpoint : IMidiRoutingEndpoint
    {
        private readonly MidiEndpointConnection connection;
        private readonly MidiVirtualDevice? virtualDevice;
        private int disposed;

        public WindowsMidiRoutingEndpoint(
            MidiEndpointConnection connection,
            MidiVirtualDevice? virtualDevice = null)
        {
            this.connection = connection;
            this.virtualDevice = virtualDevice;
            IsVirtual = virtualDevice is not null;
            if (virtualDevice is not null)
            {
                connection.AddMessageProcessingPlugin(virtualDevice);
            }

            connection.MessageReceived += OnMessageReceived;
        }

        public bool IsVirtual { get; }
        public Guid ConnectionId => connection.ConnectionId;
        public event EventHandler<MidiRoutingMessage>? MessageReceived;
        public bool IsOpen => connection.IsOpen;

        public void Open()
        {
            ObjectDisposedException.ThrowIf(Volatile.Read(ref disposed) != 0, this);
            if (!connection.Open())
            {
                throw new InvalidOperationException(
                    $"Could not open MIDI endpoint connection for {connection.ConnectedEndpointDeviceId}.");
            }
        }

        public bool Send(MidiRoutingMessage message)
        {
            ObjectDisposedException.ThrowIf(Volatile.Read(ref disposed) != 0, this);
            if (!connection.IsOpen || message.Words.Count is < 1 or > 4)
            {
                return false;
            }

            var timestamp = message.Timestamp <= 0 ? 0UL : (ulong)message.Timestamp;
            var result = message.Words.Count switch
            {
                1 => connection.SendSingleMessageWords(timestamp, message.Words[0]),
                2 => connection.SendSingleMessageWords(timestamp, message.Words[0], message.Words[1]),
                3 => connection.SendSingleMessageWords(
                    timestamp, message.Words[0], message.Words[1], message.Words[2]),
                _ => connection.SendSingleMessageWords(
                    timestamp, message.Words[0], message.Words[1], message.Words[2], message.Words[3])
            };
            return MidiEndpointConnection.SendMessageSucceeded(result);
        }

        private void OnMessageReceived(
            IMidiMessageReceivedEventSource sender,
            MidiMessageReceivedEventArgs args)
        {
            var words = new List<uint>(4);
            var wordCount = args.AppendWordsToList(words);
            var message = new MidiRoutingMessage(
                words.Take(wordCount).ToArray(),
                checked((long)args.Timestamp));
            MidiRouterMessageDispatcher.Enqueue(
                () => MessageReceived?.Invoke(this, message));
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref disposed, 1) != 0)
            {
                return;
            }

            connection.MessageReceived -= OnMessageReceived;
        }
    }
}
