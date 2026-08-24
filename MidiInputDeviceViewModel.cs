using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using WpfApplication = System.Windows.Application;

namespace midi_router;

public sealed class MidiInputDeviceViewModel : INotifyPropertyChanged, IDisposable
{
    private readonly MidiDeviceMonitor monitor;
    private readonly ObservableCollection<MidiInputDevice> devices = new();
    private int disposed;
    private DeviceOverviewState state = DeviceOverviewState.Loading;
    private string? statusMessage;

    public MidiInputDeviceViewModel(IMidiInputDeviceProvider provider)
    {
        Devices = new ReadOnlyObservableCollection<MidiInputDevice>(devices);
        monitor = new MidiDeviceMonitor(provider);
        monitor.SnapshotAvailable += OnSnapshotAvailable;
    }

    public ReadOnlyObservableCollection<MidiInputDevice> Devices { get; }

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

    private void OnSnapshotAvailable(object? sender, DeviceOverviewSnapshot snapshot)
    {
        void Apply()
        {
            devices.Clear();
            foreach (var device in snapshot.Devices)
            {
                devices.Add(device);
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
