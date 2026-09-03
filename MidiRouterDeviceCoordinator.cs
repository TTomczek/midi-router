using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace midi_router;

public sealed class MidiRouterDeviceCoordinator : IDisposable
{
    private readonly MidiInputDeviceViewModel devices;
    private readonly MidiRouter router;
    private readonly ILogger<MidiRouterDeviceCoordinator> logger;
    private readonly SemaphoreSlim synchronizationGate = new(1, 1);
    private int disposed;

    public MidiRouterDeviceCoordinator(
        MidiInputDeviceViewModel devices,
        MidiRouter router,
        ILogger<MidiRouterDeviceCoordinator>? logger = null)
    {
        this.devices = devices;
        this.router = router;
        this.logger = logger ?? LoggerFactory
            .Create(builder => builder.AddDebug())
            .CreateLogger<MidiRouterDeviceCoordinator>();
        devices.RoutingStateChanged += OnRoutingStateChanged;
        router.ActivityDetected += OnActivityDetected;
        router.Start();
        Synchronize();
    }

    private void OnActivityDetected(object? sender, string endpointDeviceId)
        => devices.MarkActivity(endpointDeviceId);

    private void OnRoutingStateChanged(object? sender, EventArgs args)
        => _ = SynchronizeInBackgroundAsync();

    private async Task SynchronizeInBackgroundAsync()
    {
        if (Volatile.Read(ref disposed) != 0)
        {
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        logger.BackgroundOperationStarted("routing synchronization");
        try
        {
            await synchronizationGate.WaitAsync().ConfigureAwait(false);
            try
            {
                if (Volatile.Read(ref disposed) != 0)
                {
                    return;
                }

                await Task.Run(Synchronize).ConfigureAwait(false);
            }
            finally
            {
                synchronizationGate.Release();
            }
            logger.BackgroundOperationCompleted("routing synchronization", stopwatch.ElapsedMilliseconds);
        }
        catch (InvalidOperationException exception)
        {
            logger.BackgroundOperationFailed(exception, "routing synchronization", stopwatch.ElapsedMilliseconds);
            devices.StatusMessageFromRouter(exception.Message);
        }
        catch (System.Runtime.InteropServices.COMException exception)
        {
            logger.BackgroundOperationFailed(exception, "routing synchronization", stopwatch.ElapsedMilliseconds);
            devices.StatusMessageFromRouter(
                $"MIDI routing could not be synchronized: {exception.Message}");
        }
        catch (Exception exception)
        {
            logger.BackgroundOperationFailed(exception, "routing synchronization", stopwatch.ElapsedMilliseconds);
            throw;
        }
    }

    private void Synchronize()
    {
        if (Volatile.Read(ref disposed) != 0)
        {
            return;
        }

        var selected = devices.Devices
            .Where(row => row.IsSelected && row.InternalChannel.HasValue)
            .ToDictionary(row => row.EndpointDeviceId, StringComparer.Ordinal);
        logger.RoutingSynchronizationStarted(selected.Count);

        foreach (var activeDeviceId in router.ActiveDeviceIds.ToArray())
        {
            if (!selected.ContainsKey(activeDeviceId))
            {
                router.Deactivate(activeDeviceId);
            }
        }

        foreach (var row in selected.Values)
        {
            if (!router.ActiveDeviceIds.Contains(row.EndpointDeviceId))
            {
                router.Activate(row.EndpointDeviceId, row.InternalChannel);
            }
            else
            {
                router.TryChangeChannel(row.EndpointDeviceId, row.InternalChannel!.Value);
            }
        }
    }

    public void Dispose()
    {
        if (Interlocked.Exchange(ref disposed, 1) != 0)
        {
            return;
        }

        devices.RoutingStateChanged -= OnRoutingStateChanged;
        router.ActivityDetected -= OnActivityDetected;
        var stopwatch = Stopwatch.StartNew();
        logger.ShutdownStepStarted("MIDI router");
        synchronizationGate.Wait();
        try
        {
            router.Dispose();
        }
        finally
        {
            synchronizationGate.Release();
        }
        logger.ShutdownStepCompleted("MIDI router coordinator", stopwatch.ElapsedMilliseconds);
    }
}
