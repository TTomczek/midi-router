using Microsoft.Extensions.Logging;
using Windows.Devices.Midi2;
using Windows.Devices.Midi2.Enumeration;

namespace midi_router;

public sealed class WindowsMidiInputDeviceProvider : IMidiInputDeviceProvider
{
    private readonly ILogger<WindowsMidiInputDeviceProvider> logger;
    private MidiEndpointDeviceWatcher? watcher;
    private int disposed;

    public WindowsMidiInputDeviceProvider(
        ILogger<WindowsMidiInputDeviceProvider>? logger = null)
    {
        this.logger = logger ?? LoggerFactory
            .Create(builder => builder.AddDebug())
            .CreateLogger<WindowsMidiInputDeviceProvider>();
    }

    public event EventHandler? DevicesChanged;
    public event EventHandler<Exception>? ProviderError;

    public IReadOnlyDictionary<string, MidiInputDevice> CurrentDevices
    {
        get
        {
            var currentWatcher = watcher;
            if (currentWatcher is null)
            {
                return new Dictionary<string, MidiInputDevice>();
            }

            var devices = new Dictionary<string, MidiInputDevice>();
            foreach (var pair in currentWatcher.EnumeratedEndpointDevices)
            {
                if (string.Equals(
                        pair.Value.Name,
                        MidiRoutingConstants.VirtualDeviceName,
                        StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                try
                {
                    var transport = pair.Value.GetTransportSuppliedInfo();
                    devices[pair.Key] = MidiDeviceProjection.FromEndpoint(
                        pair.Key,
                        pair.Value.Name,
                        transport.NativeDataFormat);
                }
                catch (Exception exception)
                {
                    logger.EndpointReadFailed(exception, pair.Key);
                    ProviderError?.Invoke(this, exception);
                }
            }

            return devices;
        }
    }

    public bool IsAvailable { get; private set; }

    public void Start()
    {
        ObjectDisposedException.ThrowIf(Volatile.Read(ref disposed) != 0, this);
        if (watcher is not null)
        {
            return;
        }

        try
        {
            IsAvailable = MidiApi.EnsureServiceAvailable();
            if (!IsAvailable)
            {
                logger.ServiceUnavailable();
                return;
            }

            watcher = MidiEndpointDeviceWatcher.Create(
                MidiEndpointDeviceInformationFilters.AllStandardEndpoints);
            watcher.Added += OnAdded;
            watcher.Removed += OnRemoved;
            watcher.EnumerationCompleted += OnEnumerationCompleted;
            watcher.Stopped += OnStopped;
            watcher.Start();
            logger.EnumerationStarted();
        }
        catch (Exception exception)
        {
            IsAvailable = false;
            logger.LogError(exception, "Midi.EnumerationStartFailed");
            ProviderError?.Invoke(this, exception);
        }
    }

    private void OnAdded(MidiEndpointDeviceWatcher sender, MidiEndpointDeviceInformationAddedEventArgs args)
    {
        logger.EndpointAdded(args.AddedDevice.EndpointDeviceId);
        DevicesChanged?.Invoke(this, EventArgs.Empty);
    }

    private void OnRemoved(MidiEndpointDeviceWatcher sender, MidiEndpointDeviceInformationRemovedEventArgs args)
    {
        logger.EndpointRemoved(args.EndpointDeviceId);
        DevicesChanged?.Invoke(this, EventArgs.Empty);
    }

    private void OnEnumerationCompleted(MidiEndpointDeviceWatcher sender, object args)
    {
        logger.EnumerationCompleted();
        DevicesChanged?.Invoke(this, EventArgs.Empty);
    }

    private void OnStopped(MidiEndpointDeviceWatcher sender, object args)
    {
        logger.WatcherStopped();
        DevicesChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Dispose()
    {
        if (Interlocked.Exchange(ref disposed, 1) != 0)
        {
            return;
        }

        if (watcher is not null)
        {
            watcher.Added -= OnAdded;
            watcher.Removed -= OnRemoved;
            watcher.EnumerationCompleted -= OnEnumerationCompleted;
            watcher.Stopped -= OnStopped;
            watcher.Stop();
            watcher = null;
        }
    }
}
