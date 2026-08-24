using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace midi_router;

public sealed class MidiInputDeviceRow(MidiInputDevice device, bool isSelected) : INotifyPropertyChanged
{
    private bool isSelected = isSelected;

    public string EndpointDeviceId => device.EndpointDeviceId;
    public string Name => device.Name;
    public MidiVersion Version => device.Version;

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
