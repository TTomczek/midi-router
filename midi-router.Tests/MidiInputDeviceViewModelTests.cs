using Xunit;

namespace midi_router.Tests;

public sealed class MidiInputDeviceViewModelTests
{
    [Fact]
    public void Refresh_LoadsDevicesAndUpdatesStatus()
    {
        var provider = new StubProvider(
            [new MidiInputDevice("Keyboard"), new MidiInputDevice("Drum Pad")]);
        var viewModel = new MidiInputDeviceViewModel(provider);

        viewModel.Refresh();

        Assert.Equal(2, viewModel.DeviceCount);
        Assert.Collection(viewModel.Devices,
            device => Assert.Equal("Keyboard", device.Name),
            device => Assert.Equal("Drum Pad", device.Name));
        Assert.Equal("2 MIDI-Eingabegeräte verfügbar", viewModel.StatusMessage);
        Assert.Equal(DeviceListState.Loaded, viewModel.State);
    }

    [Fact]
    public void Refresh_WithNoDevicesShowsEmptyStatus()
    {
        var viewModel = new MidiInputDeviceViewModel(new StubProvider());

        viewModel.Refresh();

        Assert.Empty(viewModel.Devices);
        Assert.Equal("Keine MIDI-Eingabegeräte gefunden", viewModel.StatusMessage);
        Assert.Equal(DeviceListState.Empty, viewModel.State);
    }

    [Fact]
    public void Refresh_WhenProviderFailsClearsDevicesAndShowsError()
    {
        var viewModel = new MidiInputDeviceViewModel(new StubProvider(throwOnCall: 1));

        viewModel.Refresh();

        Assert.Empty(viewModel.Devices);
        Assert.Equal("MIDI-Eingabegeräte konnten nicht gelesen werden. Bitte erneut aktualisieren.", viewModel.StatusMessage);
        Assert.Equal(DeviceListState.Error, viewModel.State);
    }

    [Fact]
    public void Refresh_WithChangedProviderSnapshotUpdatesDeviceList()
    {
        var provider = new StubProvider(
            [new MidiInputDevice("Keyboard")],
            [new MidiInputDevice("Keyboard"), new MidiInputDevice("Drum Pad")]);
        var viewModel = new MidiInputDeviceViewModel(provider);

        viewModel.Refresh();
        viewModel.Refresh();

        Assert.Equal(2, viewModel.DeviceCount);
        Assert.Collection(viewModel.Devices,
            device => Assert.Equal("Keyboard", device.Name),
            device => Assert.Equal("Drum Pad", device.Name));
        Assert.Equal(DeviceListState.Loaded, viewModel.State);
    }

    [Fact]
    public void DevicesExposeMidiVersion()
    {
        var provider = new StubProvider(
            [new MidiInputDevice("Legacy Keyboard"), new MidiInputDevice("USB Synth", MidiVersion.Midi2)]);
        var viewModel = new MidiInputDeviceViewModel(provider);

        viewModel.Refresh();

        Assert.Collection(viewModel.Devices,
            device => Assert.Equal("MIDI 1", device.VersionDisplayName),
            device => Assert.Equal("MIDI 2", device.VersionDisplayName));
    }

    [Fact]
    public void SetTargetChannelUpdatesDevice()
    {
        var device = new MidiInputDevice("Keyboard");
        var viewModel = new MidiInputDeviceViewModel(new StubProvider([device]));

        viewModel.SetTargetChannel(device, 12);

        Assert.Equal(12, device.TargetChannel);
    }

    [Fact]
    public void SetTargetChannelRejectsValuesOutsideMidiRange()
    {
        var device = new MidiInputDevice("Keyboard");
        var viewModel = new MidiInputDeviceViewModel(new StubProvider([device]));

        Assert.Throws<ArgumentOutOfRangeException>(() => viewModel.SetTargetChannel(device, 17));
        Assert.Equal(1, device.TargetChannel);
    }

    private sealed class StubProvider(params IReadOnlyList<MidiInputDevice>[] snapshots) : IMidiInputDeviceProvider
    {
        private readonly IReadOnlyList<IReadOnlyList<MidiInputDevice>> snapshots = snapshots.Length == 0
            ? [Array.Empty<MidiInputDevice>()]
            : snapshots;
        private readonly int? throwOnCall;
        private int callCount;

        public StubProvider(int throwOnCall) : this([Array.Empty<MidiInputDevice>()])
        {
            this.throwOnCall = throwOnCall;
        }

        public IReadOnlyList<MidiInputDevice> GetDevices()
        {
            callCount++;

            if (throwOnCall.HasValue && callCount == throwOnCall.Value)
            {
                throw new InvalidOperationException();
            }

            var snapshotIndex = Math.Min(callCount - 1, snapshots.Count - 1);
            return snapshots[snapshotIndex];
        }
    }
}
