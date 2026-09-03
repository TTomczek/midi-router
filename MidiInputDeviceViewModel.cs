using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using WpfApplication = System.Windows.Application;

namespace midi_router;

public sealed class MidiInputDeviceViewModel : INotifyPropertyChanged, IDisposable
{
    private readonly MidiDeviceMonitor monitor;
    private readonly ApplicationSettingsCoordinator? settings;
    private readonly ProfileManager? profileManager;
    private readonly ILogger<MidiInputDeviceViewModel> logger;
    private readonly ObservableCollection<MidiInputDeviceRow> rows = new();
    private readonly HashSet<string> selectedDeviceIds;
    private readonly Dictionary<string, int> channelAssignments;
    private int disposed;
    private DeviceOverviewState state = DeviceOverviewState.Loading;
    private string? statusMessage;
    private static readonly TimeSpan ActivityDuration = TimeSpan.FromMilliseconds(250);

    public MidiInputDeviceViewModel(
        IMidiInputDeviceProvider provider,
        ApplicationSettingsCoordinator? settings = null,
        ILoggerFactory? loggerFactory = null,
        ProfileManager? profileManager = null)
    {
        this.settings = settings;
        this.profileManager = profileManager;
        this.logger = loggerFactory?.CreateLogger<MidiInputDeviceViewModel>() ?? LoggerFactory
            .Create(builder => builder.AddDebug())
            .CreateLogger<MidiInputDeviceViewModel>();
        var initialProfile = profileManager?.ActiveProfile;
        selectedDeviceIds = new HashSet<string>(
            initialProfile?.SelectedDeviceIds ?? settings?.Settings.SelectedDeviceIds ?? Array.Empty<string>(),
            StringComparer.Ordinal);
        channelAssignments = (initialProfile?.DeviceChannelAssignments ??
            settings?.Settings.DeviceChannelAssignments ??
            new Dictionary<string, int>())
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        Devices = new ReadOnlyObservableCollection<MidiInputDeviceRow>(rows);
        monitor = new MidiDeviceMonitor(
            provider,
            loggerFactory?.CreateLogger<MidiDeviceMonitor>());
        monitor.SnapshotAvailable += OnSnapshotAvailable;
        if (profileManager is not null)
        {
            profileManager.ActiveProfileChanged += OnActiveProfileChanged;
        }
    }

    public ReadOnlyObservableCollection<MidiInputDeviceRow> Devices { get; }
    public IReadOnlyList<int> ChannelOptions { get; } =
        Enumerable.Range(1, MidiChannelAllocator.LastChannel + 1).ToArray();

    public IReadOnlyCollection<string> SelectedDeviceIds => selectedDeviceIds;

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
    public event EventHandler? RoutingStateChanged;

    public Task RefreshAsync(CancellationToken cancellationToken = default)
        => monitor.StartAsync(cancellationToken);

    public void ToggleSelection(string endpointDeviceId)
    {
        if (!selectedDeviceIds.Add(endpointDeviceId))
        {
            selectedDeviceIds.Remove(endpointDeviceId);
        }
        logger.DeviceSelectionChanged(endpointDeviceId, selectedDeviceIds.Contains(endpointDeviceId));

        foreach (var row in rows.Where(row => row.EndpointDeviceId == endpointDeviceId))
        {
            row.IsSelected = selectedDeviceIds.Contains(endpointDeviceId);
        }

        if (selectedDeviceIds.Contains(endpointDeviceId))
        {
            AssignMissingChannel(endpointDeviceId);
        }

        PersistProfileState("Device selection");
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedDeviceIds)));
        RoutingStateChanged?.Invoke(this, EventArgs.Empty);
    }

    private void OnSnapshotAvailable(object? sender, DeviceOverviewSnapshot snapshot)
    {
        void Apply()
        {
            foreach (var row in rows)
            {
                row.Dispose();
            }
            rows.Clear();
            foreach (var device in snapshot.Devices)
            {
                var assignedChannel = channelAssignments.TryGetValue(
                    device.EndpointDeviceId, out var channel) ? channel : (int?)null;
                rows.Add(new MidiInputDeviceRow(
                    device,
                    selectedDeviceIds.Contains(device.EndpointDeviceId),
                    assignedChannel));
            }
            foreach (var row in rows.Where(row => row.IsSelected))
            {
                AssignMissingChannel(row.EndpointDeviceId);
            }

            State = snapshot.State;
            StatusMessage = snapshot.StatusMessage;
            logger.LogDebug(
                "MIDI device snapshot applied: state={State}, deviceCount={DeviceCount}.",
                snapshot.State, snapshot.Devices.Count);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Devices)));
            RoutingStateChanged?.Invoke(this, EventArgs.Empty);
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

    public bool SetChannel(string endpointDeviceId, int displayChannel)
    {
        if (displayChannel is < 1 or > 16 ||
            rows.All(row => row.EndpointDeviceId != endpointDeviceId))
        {
            StatusMessage = "MIDI channel must be between 1 and 16.";
            return false;
        }

        var internalChannel = displayChannel - 1;
        if (channelAssignments.Any(pair =>
                pair.Key != endpointDeviceId && pair.Value == internalChannel))
        {
            StatusMessage = $"MIDI channel {displayChannel} is already assigned.";
            return false;
        }

        channelAssignments[endpointDeviceId] = internalChannel;
        logger.ChannelSelectionChanged(endpointDeviceId, displayChannel);
        foreach (var row in rows.Where(row => row.EndpointDeviceId == endpointDeviceId))
        {
            row.SetChannel(internalChannel);
        }

        PersistProfileState("MIDI channel assignment");
        StatusMessage = null;
        RoutingStateChanged?.Invoke(this, EventArgs.Empty);
        return true;
    }

    internal void StatusMessageFromRouter(string message)
    {
        if (WpfApplication.Current?.Dispatcher.CheckAccess() == false)
        {
            WpfApplication.Current.Dispatcher.BeginInvoke(
                new Action(() => StatusMessage = message));
            return;
        }

        StatusMessage = message;
    }

    internal void MarkActivity(string endpointDeviceId)
    {
        void Apply()
        {
            rows.FirstOrDefault(row => row.EndpointDeviceId == endpointDeviceId)
                ?.MarkActivity(ActivityDuration);
        }

        if (WpfApplication.Current?.Dispatcher.CheckAccess() == false)
        {
            WpfApplication.Current.Dispatcher.BeginInvoke(Apply);
            return;
        }

        Apply();
    }

    private void AssignMissingChannel(string endpointDeviceId)
    {
        if (channelAssignments.ContainsKey(endpointDeviceId))
        {
            return;
        }

        var used = rows
            .Where(row => row.IsSelected && row.EndpointDeviceId != endpointDeviceId)
            .Select(row => row.InternalChannel)
            .OfType<int>()
            .ToHashSet();
        var channel = Enumerable.Range(
                MidiChannelAllocator.FirstChannel,
                MidiChannelAllocator.LastChannel - MidiChannelAllocator.FirstChannel + 1)
            .FirstOrDefault(candidate => !used.Contains(candidate), -1);
        if (channel < MidiChannelAllocator.FirstChannel)
        {
            StatusMessage = "No MIDI channels are available for the selected device.";
            return;
        }

        channelAssignments[endpointDeviceId] = channel;
        foreach (var row in rows.Where(row => row.EndpointDeviceId == endpointDeviceId))
        {
            row.SetChannel(channel);
        }

        PersistProfileState("MIDI channel assignment");
    }

    private void PersistProfileState(string settingName)
    {
        if (profileManager is not null)
        {
            if (!profileManager.UpdateActiveState(selectedDeviceIds, channelAssignments))
            {
                StatusMessage = $"{settingName} could not be saved.";
            }
            return;
        }

        settings?.Update(
            current => current with
            {
                SelectedDeviceIds = selectedDeviceIds.ToArray(),
                DeviceChannelAssignments = channelAssignments.ToDictionary(
                    pair => pair.Key, pair => pair.Value, StringComparer.Ordinal)
            },
            settingName);
    }

    private void OnActiveProfileChanged(object? sender, EventArgs e)
    {
        var profile = profileManager?.ActiveProfile;
        if (profile is null)
        {
            return;
        }

        selectedDeviceIds.Clear();
        foreach (var id in profile.SelectedDeviceIds ?? Array.Empty<string>())
        {
            selectedDeviceIds.Add(id);
        }
        channelAssignments.Clear();
        foreach (var pair in profile.DeviceChannelAssignments ??
            new Dictionary<string, int>())
        {
            channelAssignments[pair.Key] = pair.Value;
        }
        logger.LogInformation(
            "MIDI active profile applied: profileId={ProfileId}, selectedDevices={SelectedDevices}, channelAssignments={ChannelAssignments}.",
            profile.Id, selectedDeviceIds.Count, channelAssignments.Count);
        foreach (var row in rows)
        {
            row.IsSelected = selectedDeviceIds.Contains(row.EndpointDeviceId);
            row.SetChannel(channelAssignments.TryGetValue(row.EndpointDeviceId, out var channel)
                ? channel : (int?)null);
        }
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedDeviceIds)));
        RoutingStateChanged?.Invoke(this, EventArgs.Empty);
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
        if (profileManager is not null)
        {
            profileManager.ActiveProfileChanged -= OnActiveProfileChanged;
        }
        foreach (var row in rows)
        {
            row.Dispose();
        }
        var stopwatch = Stopwatch.StartNew();
        logger.ShutdownStepStarted("MIDI device monitor");
        monitor.Dispose();
        logger.ShutdownStepCompleted("MIDI input device view model", stopwatch.ElapsedMilliseconds);
    }
}
