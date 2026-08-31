using Microsoft.Extensions.Logging;

namespace midi_router;

public sealed class MidiRouter : IDisposable
{
    private readonly IMidiRoutingEndpointProvider endpoints;
    private readonly IMidiMessageTransformation transformation;
    private readonly ILogger<MidiRouter> logger;
    private readonly MidiChannelAllocator allocator = new();
    private readonly Dictionary<string, IMidiRoutingEndpoint> physical = new(StringComparer.Ordinal);
    private readonly Dictionary<int, string> byChannel = new();
    private readonly Dictionary<string, int> lastOriginalChannels = new(StringComparer.Ordinal);
    private IMidiRoutingEndpoint? virtualEndpoint;
    private int disposed;

    public MidiRouter(
        IMidiRoutingEndpointProvider endpoints,
        IMidiMessageTransformation? transformation = null,
        ILogger<MidiRouter>? logger = null)
    {
        this.endpoints = endpoints;
        this.transformation = transformation ?? new MidiChannelTransformation();
        this.logger = logger ?? LoggerFactory
            .Create(builder => builder.AddDebug())
            .CreateLogger<MidiRouter>();
    }

    public event EventHandler<string>? Diagnostic;
    public event EventHandler<string>? ActivityDetected;
    public IReadOnlyDictionary<string, int> Assignments => allocator.Assignments;
    public IReadOnlyCollection<string> ActiveDeviceIds => physical.Keys;

    public void Start()
    {
        ObjectDisposedException.ThrowIf(Volatile.Read(ref disposed) != 0, this);
        if (virtualEndpoint is null)
        {
            virtualEndpoint = endpoints.OpenVirtual(MidiRoutingConstants.VirtualDeviceName);
        }
        virtualEndpoint.MessageReceived += OnVirtualMessageReceived;
        virtualEndpoint.Open();
        logger.LogInformation(
            "MIDI virtual routing endpoint started: {DeviceName}.",
            MidiRoutingConstants.VirtualDeviceName);
    }

    public bool Activate(string deviceId, int? channel = null)
    {
        ObjectDisposedException.ThrowIf(Volatile.Read(ref disposed) != 0, this);
        if (physical.ContainsKey(deviceId))
        {
            return true;
        }

        var assigned = channel.HasValue
            ? allocator.TryAssign(deviceId, channel.Value)
            : allocator.TryAssignNext(deviceId, out _);
        if (!assigned || !allocator.Assignments.TryGetValue(deviceId, out var selectedChannel))
        {
            Diagnostic?.Invoke(this, $"No available MIDI channel for {deviceId}.");
            return false;
        }

        var endpoint = endpoints.OpenPhysical(deviceId);
        endpoint.MessageReceived += OnPhysicalMessageReceived;
        try
        {
            endpoint.Open();
        }
        catch
        {
            endpoint.MessageReceived -= OnPhysicalMessageReceived;
            endpoint.Dispose();
            allocator.Remove(deviceId);
            Diagnostic?.Invoke(this, $"Could not open MIDI device {deviceId}.");
            return false;
        }

        physical[deviceId] = endpoint;
        byChannel[selectedChannel] = deviceId;
        logger.RouteStarted(deviceId);
        return true;
    }

    public void Deactivate(string deviceId)
    {
        if (!physical.Remove(deviceId, out var endpoint))
        {
            return;
        }

        endpoint.MessageReceived -= OnPhysicalMessageReceived;
        endpoint.Dispose();
        if (allocator.Assignments.TryGetValue(deviceId, out var channel))
        {
            byChannel.Remove(channel);
        }
        logger.RouteStopped(deviceId);
    }

    public bool TryChangeChannel(string deviceId, int channel)
    {
        if (!physical.ContainsKey(deviceId) ||
            !allocator.Assignments.TryGetValue(deviceId, out var previousChannel) ||
            previousChannel == channel)
        {
            return physical.ContainsKey(deviceId);
        }

        allocator.Remove(deviceId);
        if (!allocator.TryAssign(deviceId, channel))
        {
            allocator.TryAssign(deviceId, previousChannel);
            Diagnostic?.Invoke(this, $"MIDI channel {channel + 1} is already assigned.");
            return false;
        }

        byChannel.Remove(previousChannel);
        byChannel[channel] = deviceId;
        logger.LogInformation(
            "MIDI channel assignment changed: {DeviceId}, previousChannel={PreviousChannel}, channel={Channel}.",
            deviceId, previousChannel + 1, channel + 1);
        return true;
    }

    private void OnPhysicalMessageReceived(object? sender, MidiRoutingMessage message)
    {
        if (sender is not IMidiRoutingEndpoint endpoint ||
            physical.FirstOrDefault(pair => ReferenceEquals(pair.Value, endpoint)).Key is not { } deviceId ||
            !allocator.Assignments.TryGetValue(deviceId, out var channel) ||
            virtualEndpoint is null)
        {
            return;
        }

        LogMessage("physical", deviceId, message);
        ActivityDetected?.Invoke(this, deviceId);
        var transformed = transformation.Forward(message with { SourceDeviceId = deviceId }, channel);
        if (message.Channel is int originalChannel)
        {
            lastOriginalChannels[deviceId] = originalChannel;
        }

        if (!virtualEndpoint.Send(transformed))
        {
            Diagnostic?.Invoke(this, $"Could not send MIDI message from {deviceId} to virtual endpoint.");
        }
    }

    private void OnVirtualMessageReceived(object? sender, MidiRoutingMessage message)
    {
        LogMessage("virtual", message.SourceDeviceId ?? "virtual-endpoint", message);
        var channel = message.Channel;
        if (!channel.HasValue || !byChannel.TryGetValue(channel.Value, out var deviceId) ||
            !physical.TryGetValue(deviceId, out var endpoint))
        {
            Diagnostic?.Invoke(this, "Virtual MIDI message has no unambiguous destination.");
            return;
        }

        var originalChannel = message.OriginalChannel ??
            (lastOriginalChannels.TryGetValue(deviceId, out var previousChannel)
                ? previousChannel
                : 0);
        if (!endpoint.Send(transformation.Reverse(message, originalChannel)))
        {
            Diagnostic?.Invoke(this, $"Could not send MIDI message to {deviceId}.");
        }
    }

    private void LogMessage(string direction, string endpointId, MidiRoutingMessage message)
    {
        if (logger.IsEnabled(LogLevel.Trace))
        {
            logger.MidiMessageReceived(
                endpointId,
                direction,
                string.Join(" ", message.Words.Select(word => word.ToString("X8"))));
        }
    }

    public void Dispose()
    {
        if (Interlocked.Exchange(ref disposed, 1) != 0)
        {
            return;
        }

        foreach (var deviceId in physical.Keys.ToArray())
        {
            Deactivate(deviceId);
        }

        if (virtualEndpoint is not null)
        {
            virtualEndpoint.MessageReceived -= OnVirtualMessageReceived;
            virtualEndpoint.Dispose();
            logger.LogInformation("MIDI virtual routing endpoint stopped.");
            virtualEndpoint = null;
        }

        endpoints.Dispose();
    }
}
