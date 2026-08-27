using System.Collections.Concurrent;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace midi_router.Tests;

public sealed class MidiDeviceMonitorTests
{
    [Fact]
    public async Task RefreshPublishesReadyDevicesInDeterministicOrder()
    {
        using var provider = new FakeProvider(
            new[]
            {
                new MidiInputDevice("b", "Zeta", MidiVersion.Midi2),
                new MidiInputDevice("a", "Alpha", MidiVersion.Midi1)
            });
        using var monitor = new MidiDeviceMonitor(provider, NullLogger<MidiDeviceMonitor>.Instance);
        var snapshots = new ConcurrentQueue<DeviceOverviewSnapshot>();
        monitor.SnapshotAvailable += (_, snapshot) => snapshots.Enqueue(snapshot);

        await monitor.StartAsync();

        var snapshot = Assert.Single(snapshots);
        Assert.Equal(DeviceOverviewState.Ready, snapshot.State);
        Assert.Equal(new[] { "Alpha", "Zeta" }, snapshot.Devices.Select(x => x.Name));
    }

    [Fact]
    public async Task EmptyProviderPublishesEmptyState()
    {
        using var provider = new FakeProvider(Array.Empty<MidiInputDevice>());
        using var monitor = new MidiDeviceMonitor(provider, NullLogger<MidiDeviceMonitor>.Instance);
        DeviceOverviewSnapshot? snapshot = null;
        monitor.SnapshotAvailable += (_, value) => snapshot = value;

        await monitor.StartAsync();

        Assert.NotNull(snapshot);
        Assert.Equal(DeviceOverviewState.Empty, snapshot.State);
        Assert.Equal("No MIDI devices are connected.", snapshot.StatusMessage);
    }

    [Fact]
    public async Task DisposalSuppressesLaterPublications()
    {
        using var provider = new FakeProvider(Array.Empty<MidiInputDevice>());
        var monitor = new MidiDeviceMonitor(provider, NullLogger<MidiDeviceMonitor>.Instance);
        var publicationCount = 0;
        monitor.SnapshotAvailable += (_, _) => publicationCount++;

        monitor.Dispose();
        provider.RaiseChanged();
        await Task.Delay(50);

        Assert.Equal(0, publicationCount);
    }

    [Fact]
    public async Task DeviceChangesPublishTheCurrentEndpointSet()
    {
        using var provider = new FakeProvider(Array.Empty<MidiInputDevice>());
        using var monitor = new MidiDeviceMonitor(provider, NullLogger<MidiDeviceMonitor>.Instance);
        var snapshots = new ConcurrentQueue<DeviceOverviewSnapshot>();
        monitor.SnapshotAvailable += (_, snapshot) => snapshots.Enqueue(snapshot);

        await monitor.StartAsync();
        provider.Set(new MidiInputDevice("endpoint-a", "Keyboard", MidiVersion.Midi2));
        provider.RaiseChanged();
        await Task.Delay(50);

        Assert.Contains(snapshots, snapshot =>
            snapshot.State == DeviceOverviewState.Ready &&
            snapshot.Devices.Single().EndpointDeviceId == "endpoint-a");
    }

    [Fact]
    public async Task LegacyApiModeAppendsWarningAboutHiddenMidi1Devices()
    {
        using var provider = new FakeProvider(Array.Empty<MidiInputDevice>())
        {
            ApiMode = MidiApiMode.HybridLegacy
        };
        using var monitor = new MidiDeviceMonitor(provider, NullLogger<MidiDeviceMonitor>.Instance);
        DeviceOverviewSnapshot? snapshot = null;
        monitor.SnapshotAvailable += (_, value) => snapshot = value;

        await monitor.StartAsync();

        Assert.NotNull(snapshot);
        Assert.Contains("hybrid legacy", snapshot!.StatusMessage);
        Assert.Contains("Full Windows MIDI Services mode", snapshot.StatusMessage);
    }

    private sealed class FakeProvider(IEnumerable<MidiInputDevice> devices) : IMidiInputDeviceProvider
    {
        private readonly Dictionary<string, MidiInputDevice> values =
            devices.ToDictionary(x => x.EndpointDeviceId);

        public event EventHandler? DevicesChanged;
        public event EventHandler<Exception>? ProviderError { add { } remove { } }
        public IReadOnlyDictionary<string, MidiInputDevice> CurrentDevices => values;
        public bool IsAvailable { get; private set; }
        public MidiApiMode ApiMode { get; set; } = MidiApiMode.Full;

        public void Start() => IsAvailable = true;
        public void RaiseChanged() => DevicesChanged?.Invoke(this, EventArgs.Empty);
        public void Set(MidiInputDevice device) => values[device.EndpointDeviceId] = device;
        public void Dispose() { }
    }
}
