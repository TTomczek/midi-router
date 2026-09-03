using Microsoft.Extensions.Logging;
using System.Diagnostics;
using Windows.Devices.Midi2;
using Windows.Devices.Midi2.Enumeration;
using Windows.Devices.Midi2.Transports.Virtual;

namespace midi_router;

public sealed class WindowsMidiRoutingEndpointProvider : IMidiRoutingEndpointProvider
{
    private readonly ILogger<WindowsMidiRoutingEndpointProvider> logger;
    private readonly string sessionName;
    private readonly MidiSession session;
    private readonly List<WindowsMidiRoutingEndpoint> endpoints = new();
    private MidiVirtualDevice? virtualDevice;
    private int disposed;

    public WindowsMidiRoutingEndpointProvider(
        string sessionName = MidiRoutingConstants.VirtualDeviceName,
        ILogger<WindowsMidiRoutingEndpointProvider>? logger = null)
    {
        this.logger = logger ?? LoggerFactory
            .Create(builder => builder.AddDebug())
            .CreateLogger<WindowsMidiRoutingEndpointProvider>();
        this.sessionName = sessionName;
        if (!MidiApi.EnsureServiceAvailable())
        {
            throw new InvalidOperationException("Windows MIDI Services is unavailable.");
        }

        session = MidiSession.Create(sessionName);
        this.logger.SessionCreated(sessionName, session.SessionId);
    }

    public IMidiRoutingEndpoint OpenPhysical(string endpointDeviceId)
    {
        ObjectDisposedException.ThrowIf(Volatile.Read(ref disposed) != 0, this);
        var endpoint = new WindowsMidiRoutingEndpoint(
            session.CreateEndpointConnection(endpointDeviceId), null, logger);
        endpoints.Add(endpoint);
        logger.LogDebug("MIDI physical endpoint connection created: {EndpointId}.", endpointDeviceId);
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
        logger.VirtualDeviceCreated(name, virtualDevice.DeviceEndpointDeviceId);
        var endpoint = new WindowsMidiRoutingEndpoint(
            session.CreateEndpointConnection(virtualDevice.DeviceEndpointDeviceId),
            virtualDevice, logger);
        endpoints.Add(endpoint);
        return endpoint;
    }

    public void Dispose()
    {
        if (Interlocked.Exchange(ref disposed, 1) != 0)
        {
            return;
        }

        logger.LogInformation(
            "MIDI routing endpoint provider shutdown started: endpointCount={EndpointCount}, virtualDevicePresent={VirtualDevicePresent}.",
            endpoints.Count, virtualDevice is not null);
        var stopwatch = Stopwatch.StartNew();
        foreach (var endpoint in endpoints.ToArray())
        {
            logger.LogDebug("MIDI routing endpoint shutdown started: connectionId={ConnectionId}, virtual={IsVirtual}.",
                endpoint.ConnectionId, endpoint.IsVirtual);
            endpoint.Dispose();
            session.DisconnectEndpointConnection(endpoint.ConnectionId);
            logger.LogDebug("MIDI routing endpoint shutdown completed: connectionId={ConnectionId}.",
                endpoint.ConnectionId);
        }

        endpoints.Clear();
        var sessionId = session.SessionId;
        virtualDevice?.Cleanup();
        if (virtualDevice is not null)
        {
            logger.VirtualDeviceDestroyed(MidiRoutingConstants.VirtualDeviceName);
        }
        session.Dispose();
        logger.SessionDestroyed(sessionName, sessionId);
        virtualDevice = null;
        logger.LogInformation(
            "MIDI routing endpoint provider shutdown completed: elapsedMs={ElapsedMs}.",
            stopwatch.ElapsedMilliseconds);
    }

    private sealed class WindowsMidiRoutingEndpoint : IMidiRoutingEndpoint
    {
        private readonly MidiEndpointConnection connection;
        private readonly MidiVirtualDevice? virtualDevice;
        private readonly ILogger logger;
        private int disposed;

        public WindowsMidiRoutingEndpoint(
            MidiEndpointConnection connection,
            MidiVirtualDevice? virtualDevice = null,
            ILogger? logger = null)
        {
            this.connection = connection;
            this.virtualDevice = virtualDevice;
            this.logger = logger ?? LoggerFactory
                .Create(builder => builder.AddDebug())
                .CreateLogger("MIDI endpoint");
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
            logger.EndpointConnectionOpened(
                connection.ConnectedEndpointDeviceId, connection.ConnectionId, IsVirtual);
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
                () => MessageReceived?.Invoke(this, message),
                exception => logger.LogError(exception, "MIDI endpoint message processing failed."));
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref disposed, 1) != 0)
            {
                return;
            }

            connection.MessageReceived -= OnMessageReceived;
            if (virtualDevice is not null)
            {
                connection.RemoveMessageProcessingPlugin(virtualDevice.PluginId);
            }
            logger.EndpointConnectionClosed(connection.ConnectionId, IsVirtual);
        }
    }
}
