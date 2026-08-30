using System.IO;
using System.Text.Json;

namespace midi_router;

public sealed class JsonProfileStore : IProfileStore
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly string directoryPath;

    public JsonProfileStore(string? directoryPath = null)
    {
        this.directoryPath = directoryPath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "MIDI Router", "Profiles");
    }

    public string DirectoryPath => directoryPath;

    public IEnumerable<string> ListProfileIds()
    {
        if (!Directory.Exists(directoryPath))
        {
            return Array.Empty<string>();
        }

        return Directory.EnumerateFiles(directoryPath, "*.json", SearchOption.TopDirectoryOnly)
            .Select(Path.GetFileNameWithoutExtension)
            .Where(id => !string.IsNullOrWhiteSpace(id) && IsSafeId(id!))
            .Select(id => id!)
            .OrderBy(id => id, StringComparer.Ordinal)
            .ToArray();
    }

    public Profile Load(string profileId)
    {
        var path = GetPath(profileId);
        var profile = JsonSerializer.Deserialize<Profile>(File.ReadAllText(path), SerializerOptions)
            ?? throw new JsonException($"Profile '{profileId}' is empty.");
        if (!string.Equals(profile.Id, profileId, StringComparison.Ordinal))
        {
            throw new JsonException($"Profile '{profileId}' has an inconsistent identifier.");
        }
        return profile.Normalize();
    }

    public void Save(Profile profile)
    {
        var normalized = profile.Normalize();
        if (!IsSafeId(normalized.Id))
        {
            throw new ArgumentException("Profile identifier is not safe for file storage.", nameof(profile));
        }

        Directory.CreateDirectory(directoryPath);
        var path = GetPath(normalized.Id);
        var temporaryPath = $"{path}.{Guid.NewGuid():N}.tmp";
        try
        {
            File.WriteAllText(temporaryPath, JsonSerializer.Serialize(normalized, SerializerOptions));
            File.Move(temporaryPath, path, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporaryPath))
            {
                File.Delete(temporaryPath);
            }
        }
    }

    public void Delete(string profileId)
    {
        var path = GetPath(profileId);
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    private string GetPath(string profileId)
    {
        if (!IsSafeId(profileId))
        {
            throw new ArgumentException("Profile identifier is not safe for file storage.", nameof(profileId));
        }
        return Path.Combine(directoryPath, $"{profileId}.json");
    }

    private static bool IsSafeId(string id)
        => !string.IsNullOrWhiteSpace(id) &&
           id.IndexOfAny(Path.GetInvalidFileNameChars()) < 0 &&
           !id.Contains(Path.DirectorySeparatorChar) &&
           !id.Contains(Path.AltDirectorySeparatorChar) &&
           !string.Equals(id, ".", StringComparison.Ordinal) &&
           !string.Equals(id, "..", StringComparison.Ordinal);
}
