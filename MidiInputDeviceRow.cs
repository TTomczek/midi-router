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
    private CancellationTokenSource? activityCancellation;
    private bool isActive;

    public string EndpointDeviceId => device.EndpointDeviceId;
    public string Name => device.Name;
    public MidiVersion Version => device.Version;
    public int? InternalChannel => internalChannel;
    public int? DisplayChannel => internalChannel is int channel ? channel + 1 : null;
    public bool IsActive
    {
        get => isActive;
        private set
        {
            if (isActive == value)
            {
                return;
            }

            isActive = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsActive)));
        }
    }

    public void MarkActivity(TimeSpan duration)
    {
        activityCancellation?.Cancel();
        activityCancellation?.Dispose();
        var cancellation = new CancellationTokenSource();
        activityCancellation = cancellation;
        IsActive = true;
        _ = ExpireActivityAsync(cancellation, duration);
    }

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

    private async Task ExpireActivityAsync(
        CancellationTokenSource cancellation,
        TimeSpan duration)
    {
        try
        {
            await Task.Delay(duration, cancellation.Token).ConfigureAwait(false);
            if (ReferenceEquals(activityCancellation, cancellation))
            {
                IsActive = false;
                activityCancellation = null;
            }
        }
        catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
        {
        }
        finally
        {
            cancellation.Dispose();
        }
    }

    public void Dispose()
    {
        activityCancellation?.Cancel();
        activityCancellation?.Dispose();
        activityCancellation = null;
        IsActive = false;
    }
}
