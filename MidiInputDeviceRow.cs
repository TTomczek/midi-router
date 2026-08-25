using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace midi_router;

public sealed class MidiInputDeviceRow(
    MidiInputDevice device,
    bool isSelected,
    int? internalChannel = null) : INotifyPropertyChanged
{
    private bool isSelected = isSelected;
    private int? internalChannel = internalChannel;

    public string EndpointDeviceId => device.EndpointDeviceId;
    public string Name => device.Name;
    public MidiVersion Version => device.Version;
    public int? InternalChannel => internalChannel;
    public int? DisplayChannel => internalChannel is int channel ? channel + 1 : null;

    public void SetChannel(int? channel)
    {
        if (internalChannel == channel)
        {
            return;
        }

        internalChannel = channel;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(InternalChannel)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DisplayChannel)));
    }

    public bool IsSelected
    {
        get => isSelected;
        set
        {
            if (isSelected == value)
            {
                return;
            }

            isSelected = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}
