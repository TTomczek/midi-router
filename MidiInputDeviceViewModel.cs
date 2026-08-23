using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace midi_router;

public enum DeviceListState
{
    Loading,
    Loaded,
    Empty,
    Error
}

public sealed class MidiInputDeviceViewModel : INotifyPropertyChanged
{
    private readonly IMidiInputDeviceProvider _deviceProvider;
    private readonly IMidiRoutingService _routingService;
    private string _statusMessage = "Bereit";
    private DeviceListState _state;

    public MidiInputDeviceViewModel(
        IMidiInputDeviceProvider deviceProvider,
        IMidiRoutingService? routingService = null)
    {
        this._deviceProvider = deviceProvider;
        _routingService = routingService ?? new NullMidiRoutingService();
    }

    public ObservableCollection<MidiInputDevice> Devices { get; } = [];

    public IReadOnlyList<int> ChannelNumbers { get; } = Enumerable.Range(1, 16).ToArray();

    public int TargetChannel(MidiInputDevice device) => device.TargetChannel;

    public void SetTargetChannel(MidiInputDevice device, int channel)
    {
        if (channel is < 1 or > 16)
        {
            throw new ArgumentOutOfRangeException(nameof(channel), channel, "Ein MIDI-Kanal muss zwischen 1 und 16 liegen.");
        }

        device.TargetChannel = channel;
        _routingService.SetTargetChannel(device, channel);
        OnPropertyChanged(nameof(TargetChannel));
    }

    public int DeviceCount => Devices.Count;

    public string StatusMessage
    {
        get => _statusMessage;
        private set
        {
            if (_statusMessage == value)
            {
                return;
            }

            _statusMessage = value;
            OnPropertyChanged();
        }
    }

    public DeviceListState State
    {
        get => _state;
        private set
        {
            if (_state == value)
            {
                return;
            }

            _state = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsEmptyState));
            OnPropertyChanged(nameof(IsErrorState));
            OnPropertyChanged(nameof(IsLoadingState));
        }
    }

    public bool IsEmptyState => State == DeviceListState.Empty;

    public bool IsErrorState => State == DeviceListState.Error;

    public bool IsLoadingState => State == DeviceListState.Loading;

    public void Refresh()
    {
        RefreshAsync().GetAwaiter().GetResult();
    }

    public async Task RefreshAsync()
    {
        State = DeviceListState.Loading;
        StatusMessage = "MIDI-Eingabegeräte werden geladen...";

        try
        {
            _routingService.Stop();
            var devices = await Task.Run(_deviceProvider.GetDevices);
            Devices.Clear();
            foreach (var device in devices)
            {
                Devices.Add(device);
            }

            _routingService.Start(Devices);
            OnPropertyChanged(nameof(DeviceCount));
            if (Devices.Count == 0)
            {
                State = DeviceListState.Empty;
                StatusMessage = "Keine MIDI-Eingabegeräte gefunden";
            }
            else
            {
                State = DeviceListState.Loaded;
                StatusMessage = $"{Devices.Count} MIDI-Eingabegerät{(Devices.Count == 1 ? "" : "e")} verfügbar";
            }
        }
        catch (Exception exception) when (exception is InvalidOperationException or COMException)
        {
            _routingService.Stop();
            Devices.Clear();
            OnPropertyChanged(nameof(DeviceCount));
            State = DeviceListState.Error;
            StatusMessage = exception is MidiRoutingException
                ? exception.Message
                : "MIDI-Eingabegeräte konnten nicht gelesen werden. Bitte erneut aktualisieren.";
        }
    }

    public void Dispose() => _routingService.Dispose();

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
