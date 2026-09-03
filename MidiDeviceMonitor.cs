using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace midi_router;

public sealed class MidiDeviceMonitor : IDisposable
{
    private readonly IMidiInputDeviceProvider provider;
    private readonly ILogger<MidiDeviceMonitor> logger;
    private readonly SemaphoreSlim refreshGate = new(1, 1);
    private int disposed;

    public MidiDeviceMonitor(
        IMidiInputDeviceProvider provider,
        ILogger<MidiDeviceMonitor>? logger = null)
    {
        this.provider = provider;
        this.logger = logger ?? LoggerFactory
            .Create(builder => builder.AddDebug())
            .CreateLogger<MidiDeviceMonitor>();
        provider.DevicesChanged += OnDevicesChanged;
        provider.ProviderError += OnProviderError;
    }

    public event EventHandler<DeviceOverviewSnapshot>? SnapshotAvailable;

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        provider.Start();
        await RefreshAsync(cancellationToken).ConfigureAwait(false);
    }

    private void OnDevicesChanged(object? sender, EventArgs args)
    {
        _ = RefreshAsync();
    }

    private void OnProviderError(object? sender, Exception exception)
    {
        logger.EndpointReadFailed(exception, "provider");
        Publish(new DeviceOverviewSnapshot(
            provider.CurrentDevices.Values.OrderBy(device => device.Name).ToArray(),
            DeviceOverviewState.Degraded,
            "Some MIDI device information is temporarily unavailable."));
    }

    public async Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        if (Volatile.Read(ref disposed) != 0)
        {
            return;
        }

        await refreshGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (Volatile.Read(ref disposed) != 0)
            {
                return;
            }

            var devices = provider.CurrentDevices.Values
                .OrderBy(device => device.Name, StringComparer.OrdinalIgnoreCase)
                .ThenBy(device => device.EndpointDeviceId, StringComparer.Ordinal)
                .ToArray();
            var state = !provider.IsAvailable
                ? DeviceOverviewState.Unavailable
                : devices.Length == 0
                    ? DeviceOverviewState.Empty
                    : DeviceOverviewState.Ready;
            var message = state switch
            {
                DeviceOverviewState.Unavailable => "Windows MIDI Services is unavailable.",
                DeviceOverviewState.Empty => "No MIDI devices are connected.",
                _ => null
            };

            if (state != DeviceOverviewState.Unavailable && provider.ApiMode != MidiApiMode.Full)
            {
                var modeName = provider.ApiMode == MidiApiMode.Legacy ? "legacy WinMM-only" : "hybrid legacy";
                var apiModeWarning =
                    $"Windows MIDI Services is running in {modeName} mode, so physical MIDI 1.0 " +
                    "devices using WinMM, the legacy usbaudio.sys driver, or vendor MIDI 1 drivers " +
                    "will not appear or send messages here. Enable Full Windows MIDI Services mode " +
                    "in the MIDI Settings app to use them.";
                message = message is null ? apiModeWarning : $"{message} {apiModeWarning}";
            }

            Publish(new DeviceOverviewSnapshot(devices, state, message));
        }
        finally
        {
            refreshGate.Release();
        }
    }

    private void Publish(DeviceOverviewSnapshot snapshot)
    {
        if (Volatile.Read(ref disposed) == 0)
        {
            SnapshotAvailable?.Invoke(this, snapshot);
        }
    }

    public void Dispose()
    {
        if (Interlocked.Exchange(ref disposed, 1) != 0)
        {
            return;
        }

        provider.DevicesChanged -= OnDevicesChanged;
        provider.ProviderError -= OnProviderError;
        var stopwatch = Stopwatch.StartNew();
        logger.ShutdownStepStarted("MIDI input device provider");
        provider.Dispose();
        logger.ShutdownStepCompleted("MIDI input device provider", stopwatch.ElapsedMilliseconds);
        Publish(new DeviceOverviewSnapshot(
            Array.Empty<MidiInputDevice>(),
            DeviceOverviewState.Stopped,
            "MIDI device monitoring stopped."));
        refreshGate.Dispose();
        logger.ShutdownStepCompleted("MIDI device monitor", stopwatch.ElapsedMilliseconds);
    }
}
