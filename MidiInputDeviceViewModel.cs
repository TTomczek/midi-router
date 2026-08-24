using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using WpfApplication = System.Windows.Application;

namespace midi_router;

public sealed class MidiInputDeviceViewModel : INotifyPropertyChanged, IDisposable
{
    private readonly MidiDeviceMonitor monitor;
    private readonly ApplicationSettingsCoordinator? settings;
    private readonly ObservableCollection<MidiInputDeviceRow> rows = new();
    private readonly HashSet<string> selectedDeviceIds;
    private int disposed;
    private DeviceOverviewState state = DeviceOverviewState.Loading;
    private string? statusMessage;

    public MidiInputDeviceViewModel(
        IMidiInputDeviceProvider provider,
        ApplicationSettingsCoordinator? settings = null)
    {
        this.settings = settings;
        selectedDeviceIds = new HashSet<string>(
            settings?.Settings.SelectedDeviceIds ?? Array.Empty<string>(),
            StringComparer.Ordinal);
        Devices = new ReadOnlyObservableCollection<MidiInputDeviceRow>(rows);
        monitor = new MidiDeviceMonitor(provider);
        monitor.SnapshotAvailable += OnSnapshotAvailable;
    }

    public ReadOnlyObservableCollection<MidiInputDeviceRow> Devices { get; }

    public IReadOnlyCollection<string> SelectedDeviceIds => selectedDeviceIds;

    public DeviceOverviewState State
    {
        get => state;
        private set => SetField(ref state, value);
    }

    public string? StatusMessage
    {
        get => statusMessage;
        private set => SetField(ref statusMessage, value);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public Task RefreshAsync(CancellationToken cancellationToken = default)
        => monitor.StartAsync(cancellationToken);

    public void ToggleSelection(string endpointDeviceId)
    {
        if (!selectedDeviceIds.Add(endpointDeviceId))
        {
            selectedDeviceIds.Remove(endpointDeviceId);
        }

        foreach (var row in rows.Where(row => row.EndpointDeviceId == endpointDeviceId))
        {
            row.IsSelected = selectedDeviceIds.Contains(endpointDeviceId);
        }

        settings?.Update(
            current => current with { SelectedDeviceIds = selectedDeviceIds.ToArray() },
            "Device selection");
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedDeviceIds)));
    }

    private void OnSnapshotAvailable(object? sender, DeviceOverviewSnapshot snapshot)
    {
        void Apply()
        {
            rows.Clear();
            foreach (var device in snapshot.Devices)
            {
                rows.Add(new MidiInputDeviceRow(
                    device,
                    selectedDeviceIds.Contains(device.EndpointDeviceId)));
            }

            State = snapshot.State;
            StatusMessage = snapshot.StatusMessage;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Devices)));
        }

        if (WpfApplication.Current?.Dispatcher.CheckAccess() == false)
        {
            WpfApplication.Current.Dispatcher.Invoke(Apply);
        }
        else
        {
            Apply();
        }
    }

    private void SetField<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (!EqualityComparer<T>.Default.Equals(field, value))
        {
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    public void Dispose()
    {
        if (Interlocked.Exchange(ref disposed, 1) != 0)
        {
            return;
        }

        monitor.SnapshotAvailable -= OnSnapshotAvailable;
        monitor.Dispose();
    }
}
