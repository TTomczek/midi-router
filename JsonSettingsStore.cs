using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace midi_router;

public sealed class JsonSettingsStore : ISettingsStore
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly string filePath;

    public JsonSettingsStore(string? filePath = null)
    {
        this.filePath = filePath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "MIDI Router",
            "settings.json");
    }

    public ApplicationSettings Load()
    {
        if (!File.Exists(filePath))
        {
            return new ApplicationSettings();
        }

        var json = File.ReadAllText(filePath);
        var settings = JsonSerializer.Deserialize<ApplicationSettings>(json, SerializerOptions);
        return settings is null
            ? new ApplicationSettings()
            : Normalize(settings);
    }

    public void Save(ApplicationSettings settings)
    {
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var temporaryPath = $"{filePath}.{Guid.NewGuid():N}.tmp";
        try
        {
            File.WriteAllText(temporaryPath, JsonSerializer.Serialize(
                Normalize(settings),
                SerializerOptions));
            File.Move(temporaryPath, filePath, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporaryPath))
            {
                File.Delete(temporaryPath);
            }
        }
    }

    private static AppearanceMode Normalize(AppearanceMode mode)
        => Enum.IsDefined(mode) ? mode : AppearanceMode.OsDefault;

    private static ApplicationSettings Normalize(ApplicationSettings settings)
        => settings with
        {
            AppearanceMode = Normalize(settings.AppearanceMode),
            SelectedDeviceIds = (settings.SelectedDeviceIds ?? Array.Empty<string>())
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct(StringComparer.Ordinal)
                .ToArray()
        };
}
