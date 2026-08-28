using System.IO;
using System.Text.Json;

namespace midi_router;

public sealed class ProfileStore
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };
    private readonly string directory;

    public ProfileStore(string? directory = null)
    {
        directory ??= Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "MIDI Router", "Profiles");
        this.directory = directory;
    }

    public IReadOnlyList<Profile> LoadAll()
    {
        if (!Directory.Exists(directory))
            return Array.Empty<Profile>();

        var profiles = new List<Profile>();
        foreach (var file in Directory.EnumerateFiles(directory, "*.json"))
        {
            try
            {
                var profile = JsonSerializer.Deserialize<Profile>(File.ReadAllText(file), Options);
                if (profile is not null && !string.IsNullOrWhiteSpace(profile.Id) &&
                    !string.IsNullOrWhiteSpace(profile.Name))
                    profiles.Add(Normalize(profile));
            }
            catch (JsonException) { }
            catch (IOException) { }
        }
        return profiles.OrderBy(p => p.Name, StringComparer.CurrentCultureIgnoreCase).ToArray();
    }

    public void Save(Profile profile)
    {
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, $"{profile.Id}.json");
        var temporaryPath = $"{path}.{Guid.NewGuid():N}.tmp";
        try
        {
            File.WriteAllText(temporaryPath, JsonSerializer.Serialize(Normalize(profile), Options));
            File.Move(temporaryPath, path, true);
        }
        finally
        {
            if (File.Exists(temporaryPath)) File.Delete(temporaryPath);
        }
    }

    public void Delete(Profile profile)
    {
        var path = Path.Combine(directory, $"{profile.Id}.json");
        if (File.Exists(path)) File.Delete(path);
    }

    private static Profile Normalize(Profile profile) => profile with
    {
        SelectedDeviceIds = (profile.SelectedDeviceIds ?? Array.Empty<string>())
            .Where(id => !string.IsNullOrWhiteSpace(id)).Distinct(StringComparer.Ordinal).ToArray(),
        DeviceChannelAssignments = (profile.DeviceChannelAssignments ?? new Dictionary<string, int>())
            .Where(p => !string.IsNullOrWhiteSpace(p.Key) &&
                        p.Value is >= MidiChannelAllocator.FirstChannel and <= MidiChannelAllocator.LastChannel)
            .ToDictionary(p => p.Key, p => p.Value, StringComparer.Ordinal)
    };
}
