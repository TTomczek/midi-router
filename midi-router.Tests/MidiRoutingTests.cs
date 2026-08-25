using Microsoft.Extensions.Logging;
using Xunit;

namespace midi_router.Tests;

public sealed class MidiRoutingTests
{
    [Fact]
    public void AllocatorUsesAscendingChannelsAndRejectsConflicts()
    {
        var allocator = new MidiChannelAllocator();

        Assert.True(allocator.TryAssignNext("a", out var first));
        Assert.True(allocator.TryAssignNext("b", out var second));
        Assert.Equal(0, first);
        Assert.Equal(1, second);
        Assert.False(allocator.TryAssign("c", 1));
        Assert.True(allocator.TryAssign("c", 15));
    }

    [Fact]
    public void ChannelTransformationPreservesOtherMessageDataAndReverses()
    {
        var message = new MidiRoutingMessage(new[] { 0x20903C7Fu });
        var transformation = new MidiChannelTransformation();

        var forward = transformation.Forward(message, 2);
        var reverse = transformation.Reverse(forward, 0);

        Assert.Equal(2, forward.Channel);
        Assert.Equal(0, reverse.Channel);
        Assert.Equal(0x3C7Fu, forward.Words[0] & 0xFFFFu);
    }

    [Fact]
    public void ChannelLessMessageIsCopiedUnchanged()
    {
        var message = new MidiRoutingMessage(new[] { 0x10000000u });
        var transformed = new MidiChannelTransformation().Forward(message, 4);

        Assert.Null(transformed.Channel);
        Assert.Equal(message.Words, transformed.Words);
    }

    [Fact]
    public void RouterRoutesPhysicalMessagesAndReturnsVirtualMessages()
    {
        using var provider = new FakeEndpointProvider();
        using var router = new MidiRouter(provider);
        router.Start();
        Assert.True(router.Activate("device-a", 2));

        var physical = provider.Physical["device-a"];
        physical.Raise(new MidiRoutingMessage(new[] { 0x20903C7Fu }));

        var virtualMessage = Assert.Single(provider.Virtual.Sent);
        Assert.Equal(2, virtualMessage.Channel);

        provider.Virtual.Raise(virtualMessage with { OriginalChannel = 0 });

        var returned = Assert.Single(physical.Sent);
        Assert.Equal(0, returned.Channel);
    }

    [Fact]
    public void RouterPublishesPhysicalSourceActivity()
    {
        using var provider = new FakeEndpointProvider();
        using var router = new MidiRouter(provider);
        string? sourceDeviceId = null;
        router.ActivityDetected += (_, deviceId) => sourceDeviceId = deviceId;
        router.Start();
        Assert.True(router.Activate("device-a", 2));

        provider.Physical["device-a"].Raise(new MidiRoutingMessage(new[] { 0x20903C7Fu }));

        Assert.Equal("device-a", sourceDeviceId);
    }

    [Fact]
    public void RouterLogsReceivedMessagesAtTraceLevel()
    {
        var providerLogger = new CapturingLoggerProvider();
        using var loggerFactory = LoggerFactory.Create(builder =>
            builder.SetMinimumLevel(LogLevel.Trace).AddProvider(providerLogger));
        using var endpointProvider = new FakeEndpointProvider();
        using var router = new MidiRouter(
            endpointProvider,
            logger: loggerFactory.CreateLogger<MidiRouter>());
        router.Start();
        Assert.True(router.Activate("device-a", 2));

        endpointProvider.Physical["device-a"].Raise(new MidiRoutingMessage(new[] { 0x20903C7Fu }));

        Assert.Contains(providerLogger.Entries, entry =>
            entry.Level == LogLevel.Trace &&
            entry.Message.Contains("device-a", StringComparison.Ordinal) &&
            entry.Message.Contains("20903C7F", StringComparison.Ordinal));
    }

    [Fact]
    public void RouterDoesNotRouteUnknownVirtualChannel()
    {
        using var provider = new FakeEndpointProvider();
        using var router = new MidiRouter(provider);
        var diagnostics = new List<string>();
        router.Diagnostic += (_, message) => diagnostics.Add(message);
        router.Start();

        provider.Virtual.Raise(new MidiRoutingMessage(new[] { 0x20903C7Fu }));

        Assert.Empty(provider.Physical);
        Assert.NotEmpty(diagnostics);
    }

    private sealed class FakeEndpointProvider : IMidiRoutingEndpointProvider
    {
        public Dictionary<string, FakeEndpoint> Physical { get; } = new();
        public FakeEndpoint Virtual { get; } = new();

        public IMidiRoutingEndpoint OpenPhysical(string endpointDeviceId)
            => Physical[endpointDeviceId] = new FakeEndpoint();

        public IMidiRoutingEndpoint OpenVirtual(string name) => Virtual;
        public void Dispose() { }
    }

    private sealed class FakeEndpoint : IMidiRoutingEndpoint
    {
        public event EventHandler<MidiRoutingMessage>? MessageReceived;
        public bool IsOpen { get; private set; }
        public List<MidiRoutingMessage> Sent { get; } = new();

        public void Open() => IsOpen = true;
        public bool Send(MidiRoutingMessage message)
        {
            Sent.Add(message);
            return true;
        }

        public void Raise(MidiRoutingMessage message) => MessageReceived?.Invoke(this, message);
        public void Dispose() => IsOpen = false;
    }

    private sealed class CapturingLoggerProvider : ILoggerProvider
    {
        public List<(LogLevel Level, string Message)> Entries { get; } = new();

        public ILogger CreateLogger(string categoryName) => new CapturingLogger(this);
        public void Dispose() { }

        private sealed class CapturingLogger(CapturingLoggerProvider provider) : ILogger
        {
            public IDisposable BeginScope<TState>(TState state)
                where TState : notnull
                => NullScope.Instance;

            public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

            public void Log<TState>(
                LogLevel logLevel,
                EventId eventId,
                TState state,
                Exception? exception,
                Func<TState, Exception?, string> formatter)
                => provider.Entries.Add((logLevel, formatter(state, exception)));
        }

        private sealed class NullScope : IDisposable
        {
            public static NullScope Instance { get; } = new();
            public void Dispose() { }
        }
    }
}
