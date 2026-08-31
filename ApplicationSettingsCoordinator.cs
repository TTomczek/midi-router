using System.IO;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace midi_router;

public sealed class ApplicationSettingsCoordinator
{
    private readonly ISettingsStore store;
    private readonly Action<string>? report;
    private readonly ILogger<ApplicationSettingsCoordinator> logger;

    public ApplicationSettingsCoordinator(
        ISettingsStore store,
        Action<string>? report = null,
        ILogger<ApplicationSettingsCoordinator>? logger = null)
    {
        this.store = store;
        this.report = report;
        this.logger = logger ?? LoggerFactory
            .Create(builder => builder.AddDebug())
            .CreateLogger<ApplicationSettingsCoordinator>();
    }

    public ApplicationSettings Settings { get; private set; } = new();

    public void Load()
    {
        try
        {
            Settings = Normalize(store.Load());
        }
        catch (IOException exception)
        {
            Settings = new ApplicationSettings();
            report?.Invoke($"Application settings could not be loaded: {exception.Message}");
        }
        catch (UnauthorizedAccessException exception)
        {
            Settings = new ApplicationSettings();
            report?.Invoke($"Application settings could not be loaded: {exception.Message}");
        }
        catch (JsonException exception)
        {
            Settings = new ApplicationSettings();
            report?.Invoke($"Application settings could not be loaded: {exception.Message}");
        }
    }

    public void Update(Func<ApplicationSettings, ApplicationSettings> update, string settingName)
    {
        Settings = Normalize(update(Settings));
        logger.SettingsChanged(settingName);
        try
        {
            store.Save(Settings);
        }
        catch (IOException exception)
        {
            report?.Invoke($"{settingName} settings could not be saved: {exception.Message}");
        }
        catch (UnauthorizedAccessException exception)
        {
            report?.Invoke($"{settingName} settings could not be saved: {exception.Message}");
        }
    }

    private static ApplicationSettings Normalize(ApplicationSettings settings)
        => settings with
        {
            AppearanceMode = settings.AppearanceMode.Normalize(),
            SelectedDeviceIds = (settings.SelectedDeviceIds ?? Array.Empty<string>())
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct(StringComparer.Ordinal)
                .ToArray(),
            DeviceChannelAssignments = (settings.DeviceChannelAssignments ??
                new Dictionary<string, int>())
                .Where(pair =>
                    !string.IsNullOrWhiteSpace(pair.Key) &&
                    pair.Value is >= MidiChannelAllocator.FirstChannel and <= MidiChannelAllocator.LastChannel)
                .GroupBy(pair => pair.Key, StringComparer.Ordinal)
                .Select(group => group.First())
                .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal),
            ActiveProfileId = string.IsNullOrWhiteSpace(settings.ActiveProfileId)
                ? null
                : settings.ActiveProfileId.Trim()
        };
}
