using Xunit;

namespace midi_router.Tests;

public sealed class MidiInputDeviceViewModelTests
{
    [Fact]
    public async Task ToggleSelectionUsesUniqueDeviceId()
    {
        using var provider = new FakeProvider(
            new MidiInputDevice("id-a", "Keyboard", MidiVersion.Midi1));
        using var viewModel = new MidiInputDeviceViewModel(provider);

        await viewModel.RefreshAsync();
        viewModel.ToggleSelection("id-a");

        Assert.Contains("id-a", viewModel.SelectedDeviceIds);
        Assert.True(viewModel.Devices.Single().IsSelected);

        viewModel.ToggleSelection("id-a");

        Assert.Empty(viewModel.SelectedDeviceIds);
        Assert.False(viewModel.Devices.Single().IsSelected);
    }

    [Fact]
    public async Task SameNameDevicesRemainIndependentlySelectable()
    {
        using var provider = new FakeProvider(
            new MidiInputDevice("id-a", "Controller", MidiVersion.Midi1),
            new MidiInputDevice("id-b", "Controller", MidiVersion.Midi2));
        using var viewModel = new MidiInputDeviceViewModel(provider);

        await viewModel.RefreshAsync();
        viewModel.ToggleSelection("id-a");

        Assert.Equal(new[] { "id-a" }, viewModel.SelectedDeviceIds);
        Assert.True(viewModel.Devices.Single(row => row.EndpointDeviceId == "id-a").IsSelected);
        Assert.False(viewModel.Devices.Single(row => row.EndpointDeviceId == "id-b").IsSelected);
    }

    [Fact]
    public void RowSelectionRaisesPropertyChanged()
    {
        var row = new MidiInputDeviceRow(
            new MidiInputDevice("id-a", "Keyboard", MidiVersion.Midi1),
            false);
        var changed = new List<string?>();
        row.PropertyChanged += (_, args) => changed.Add(args.PropertyName);

        row.IsSelected = true;

        Assert.Contains(nameof(MidiInputDeviceRow.IsSelected), changed);
    }

    [Fact]
    public async Task SelectedDevicesReceiveAscendingAutomaticChannels()
    {
        using var provider = new FakeProvider(
            new MidiInputDevice("id-a", "A", MidiVersion.Midi1),
            new MidiInputDevice("id-b", "B", MidiVersion.Midi1));
        using var viewModel = new MidiInputDeviceViewModel(provider);

        await viewModel.RefreshAsync();
        viewModel.ToggleSelection("id-a");
        viewModel.ToggleSelection("id-b");

        Assert.Equal(1, viewModel.Devices.Single(row => row.EndpointDeviceId == "id-a").DisplayChannel);
        Assert.Equal(2, viewModel.Devices.Single(row => row.EndpointDeviceId == "id-b").DisplayChannel);
    }

    [Fact]
    public async Task ReconnectRestoresSelectedDeviceById()
    {
        using var provider = new FakeProvider(
            new MidiInputDevice("id-a", "Keyboard", MidiVersion.Midi1));
        var coordinator = new ApplicationSettingsCoordinator(new Store());
        coordinator.Load();
        using var viewModel = new MidiInputDeviceViewModel(provider, coordinator);

        await viewModel.RefreshAsync();
        viewModel.ToggleSelection("id-a");
        provider.Set();
        provider.RaiseChanged();
        await Task.Delay(50);

        Assert.Empty(viewModel.Devices);

        provider.Set(new MidiInputDevice("id-a", "Keyboard", MidiVersion.Midi1));
        provider.RaiseChanged();
        await Task.Delay(50);

        Assert.True(viewModel.Devices.Single().IsSelected);
    }

    private sealed class FakeProvider(params MidiInputDevice[] initial) : IMidiInputDeviceProvider
    {
        private readonly Dictionary<string, MidiInputDevice> values =
            initial.ToDictionary(device => device.EndpointDeviceId);

        public event EventHandler? DevicesChanged;
        public event EventHandler<Exception>? ProviderError
        {
            add { }
            remove { }
        }
        public IReadOnlyDictionary<string, MidiInputDevice> CurrentDevices => values;
        public bool IsAvailable { get; private set; }

        public void Start() => IsAvailable = true;
        public void Set(MidiInputDevice? device = null)
        {
            values.Clear();
            if (device is not null)
            {
                values[device.EndpointDeviceId] = device;
            }
        }

        public void RaiseChanged() => DevicesChanged?.Invoke(this, EventArgs.Empty);
        public void Dispose() { }
    }

    private sealed class Store : ISettingsStore
    {
        public ApplicationSettings Settings { get; private set; } = new();
        public ApplicationSettings Load() => Settings;
        public void Save(ApplicationSettings settings) => Settings = settings;
    }
}
