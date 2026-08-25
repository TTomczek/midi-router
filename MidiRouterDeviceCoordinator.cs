namespace midi_router;

public sealed class MidiRouterDeviceCoordinator : IDisposable
{
    private readonly MidiInputDeviceViewModel devices;
    private readonly MidiRouter router;
    private readonly SemaphoreSlim synchronizationGate = new(1, 1);
    private int disposed;

    public MidiRouterDeviceCoordinator(
        MidiInputDeviceViewModel devices,
        MidiRouter router)
    {
        this.devices = devices;
        this.router = router;
        devices.RoutingStateChanged += OnRoutingStateChanged;
        router.Start();
        Synchronize();
    }

    private void OnRoutingStateChanged(object? sender, EventArgs args)
        => _ = SynchronizeInBackgroundAsync();

    private async Task SynchronizeInBackgroundAsync()
    {
        await synchronizationGate.WaitAsync().ConfigureAwait(false);
        try
        {
            await Task.Run(Synchronize).ConfigureAwait(false);
        }
        catch (InvalidOperationException exception)
        {
            devices.StatusMessageFromRouter(exception.Message);
        }
        catch (System.Runtime.InteropServices.COMException exception)
        {
            devices.StatusMessageFromRouter(
                $"MIDI routing could not be synchronized: {exception.Message}");
        }
        finally
        {
            synchronizationGate.Release();
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
        router.Dispose();
    }
}
