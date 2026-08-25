using System.Collections.ObjectModel;
using System.Windows;
using Windows.Devices.Midi2;
using Windows.Devices.Midi2.Enumeration;
using midi_router;

namespace MidiMessageMonitor;

public partial class MainWindow : Window
{
    private const int MaximumMessages = 500;
    private const string SessionName = "Midi Router Message Monitor";
    private readonly ObservableCollection<DisplayedMessage> messages = new();
    private readonly Dictionary<string, IMidiRoutingEndpoint> endpoints = new(StringComparer.Ordinal);
    private WindowsMidiRoutingEndpointProvider? endpointProvider;
    private MidiEndpointDeviceWatcher? watcher;
    private int disposed;

    public MainWindow()
    {
        InitializeComponent();
        Messages.ItemsSource = messages;
        ContentRendered += StartListening;
    }

    private async void StartListening(object? sender, EventArgs e)
    {
        ContentRendered -= StartListening;
        try
        {
            var provider = await Task.Run(
                () => new WindowsMidiRoutingEndpointProvider(SessionName));
            if (Volatile.Read(ref disposed) != 0)
            {
                provider.Dispose();
                return;
            }

            endpointProvider = provider;
            watcher = MidiEndpointDeviceWatcher.Create(
                MidiEndpointDeviceInformationFilters.AllStandardEndpoints |
                MidiEndpointDeviceInformationFilters.VirtualDeviceResponder);
            watcher.Added += OnDeviceAdded;
            watcher.Removed += OnDeviceRemoved;
            watcher.EnumerationCompleted += OnEnumerationCompleted;
            watcher.Start();
        }
        catch (Exception exception)
        {
            StatusText.Text = $"Could not start MIDI monitoring: {exception.Message}";
            endpointProvider?.Dispose();
            endpointProvider = null;
        }
    }

    private void OnEnumerationCompleted(MidiEndpointDeviceWatcher sender, object args)
    {
        var devices = sender.EnumeratedEndpointDevices.Values.Where(candidate =>
            string.Equals(candidate.Name, MidiRoutingConstants.VirtualDeviceName,
                StringComparison.OrdinalIgnoreCase)).ToArray();
        if (devices.Length == 0)
        {
            Dispatcher.BeginInvoke(() => StatusText.Text = "Midi Router device not found.");
            return;
        }

        foreach (var device in devices)
        {
            TryOpenDevice(device);
        }
    }

    private void OnDeviceAdded(
        MidiEndpointDeviceWatcher sender,
        MidiEndpointDeviceInformationAddedEventArgs args)
    {
        if (string.Equals(
                args.AddedDevice.Name,
                MidiRoutingConstants.VirtualDeviceName,
                StringComparison.OrdinalIgnoreCase))
        {
            TryOpenDevice(args.AddedDevice);
        }
    }

    private void OnDeviceRemoved(
        MidiEndpointDeviceWatcher sender,
        MidiEndpointDeviceInformationRemovedEventArgs args)
    {
        if (endpoints.ContainsKey(args.EndpointDeviceId))
        {
            CloseEndpoint(args.EndpointDeviceId);
            if (endpoints.Count == 0)
            {
                Dispatcher.BeginInvoke(() => StatusText.Text = "Midi Router disconnected. Waiting for device...");
            }
        }
    }

    private void TryOpenDevice(MidiEndpointDeviceInformation device)
    {
        if (endpoints.ContainsKey(device.EndpointDeviceId))
        {
            return;
        }

        try
        {
            var endpoint = endpointProvider!.OpenPhysical(device.EndpointDeviceId);
            endpoints.Add(device.EndpointDeviceId, endpoint);
            endpoint.MessageReceived += OnMessageReceived;
            endpoint.Open();
            Dispatcher.BeginInvoke(() => StatusText.Text = "Listening for MIDI messages.");
        }
        catch (Exception exception)
        {
            CloseEndpoint(device.EndpointDeviceId);
            Dispatcher.BeginInvoke(() => StatusText.Text = $"Could not open Midi Router: {exception.Message}");
        }
    }

    private void OnMessageReceived(object? sender, MidiRoutingMessage message)
    {
        var displayed = new DisplayedMessage(
            DateTime.Now.ToString("HH:mm:ss.fff"),
            message.Channel is int channel ? (channel + 1).ToString() : "-",
            string.Join(" ", message.Words.Select(word => word.ToString("X8"))));
        Dispatcher.BeginInvoke(() =>
        {
            messages.Insert(0, displayed);
            while (messages.Count > MaximumMessages)
            {
                messages.RemoveAt(messages.Count - 1);
            }
        });
    }

    private void ClearButton_Click(object sender, RoutedEventArgs e) => messages.Clear();

    private void CloseEndpoint(string endpointDeviceId)
    {
        if (!endpoints.Remove(endpointDeviceId, out var endpoint))
        {
            return;
        }

        endpoint.MessageReceived -= OnMessageReceived;
        endpoint.Dispose();
    }

    private void CloseAllEndpoints()
    {
        foreach (var endpointDeviceId in endpoints.Keys.ToArray())
        {
            CloseEndpoint(endpointDeviceId);
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        if (Interlocked.Exchange(ref disposed, 1) == 0)
        {
            CloseAllEndpoints();
            if (watcher is not null)
            {
                watcher.Added -= OnDeviceAdded;
                watcher.Removed -= OnDeviceRemoved;
                watcher.EnumerationCompleted -= OnEnumerationCompleted;
            }
            watcher?.Stop();
            watcher = null;
            endpointProvider?.Dispose();
        }

        base.OnClosed(e);
    }

    private sealed record DisplayedMessage(string Time, string Channel, string Data);
}
