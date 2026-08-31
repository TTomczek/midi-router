using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace midi_router;

public sealed class ProfileListItem : INotifyPropertyChanged
{
    private Profile? profile;
    private string displayName;
    private bool isActive;
    private bool isDeletable;

    public ProfileListItem(Profile profile)
    {
        this.profile = profile;
        displayName = profile.Name;
    }

    private ProfileListItem()
    {
        displayName = "Create profile";
        IsCreate = true;
    }

    public static ProfileListItem CreateEntry { get; } = new();
    public bool IsCreate { get; }
    public bool IsDeletable
    {
        get => isDeletable;
        internal set => SetField(ref isDeletable, value);
    }
    public bool CanRename => !IsCreate;
    public string Id => profile?.Id ?? string.Empty;
    public string Name => profile?.Name ?? string.Empty;
    public DateTime LastEdited => profile?.LastEdited ?? default;
    public string DisplayName
    {
        get => displayName;
        internal set => SetField(ref displayName, value);
    }
    public bool IsActive
    {
        get => isActive;
        internal set => SetField(ref isActive, value);
    }

    internal void Update(Profile value)
    {
        profile = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LastEdited)));
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void SetField<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return;
        }
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}

public sealed class ProfileManager
{
    private readonly IProfileStore store;
    private readonly ApplicationSettingsCoordinator? settingsCoordinator;
    private readonly Action<string>? report;
    private readonly ILogger<ProfileManager> logger;
    private readonly List<Profile> profiles = new();
    private readonly ObservableCollection<ProfileListItem> profileItems = new();
    private readonly ReadOnlyObservableCollection<ProfileListItem> readOnlyProfileItems;

    public ProfileManager(
        IProfileStore store,
        ApplicationSettingsCoordinator? settingsCoordinator = null,
        Action<string>? report = null,
        ILogger<ProfileManager>? logger = null)
    {
        this.store = store;
        this.settingsCoordinator = settingsCoordinator;
        this.report = report;
        this.logger = logger ?? LoggerFactory
            .Create(builder => builder.AddDebug())
            .CreateLogger<ProfileManager>();
        readOnlyProfileItems = new ReadOnlyObservableCollection<ProfileListItem>(profileItems);
    }

    public IReadOnlyList<Profile> Profiles => profiles;
    public ReadOnlyObservableCollection<ProfileListItem> ProfileItems => readOnlyProfileItems;
    public string? ActiveProfileId { get; private set; }
    public Profile? ActiveProfile => profiles.FirstOrDefault(profile =>
        string.Equals(profile.Id, ActiveProfileId, StringComparison.Ordinal));

    public event EventHandler? ActiveProfileChanged;
    public event EventHandler? ProfilesChanged;
    public event EventHandler<string>? Diagnostic;

    public void Load()
    {
        profiles.Clear();
        IEnumerable<string> profileIds;
        try
        {
            profileIds = store.ListProfileIds().Distinct(StringComparer.Ordinal)
                .OrderBy(id => id, StringComparer.Ordinal).ToArray();
        }
        catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException)
        {
            Report($"Profiles could not be listed: {exception.Message}");
            profileIds = Array.Empty<string>();
        }
        foreach (var id in profileIds)
        {
            try
            {
                var profile = store.Load(id).Normalize();
                if (!string.Equals(profile.Id, id, StringComparison.Ordinal))
                {
                    Report($"Profile '{id}' was skipped because its identifier is invalid.");
                    continue;
                }
                profiles.Add(profile);
                logger.ProfileLoaded(profile.Id, profile.Name);
            }
            catch (Exception exception) when (exception is IOException ||
                exception is UnauthorizedAccessException || exception is JsonException ||
                exception is ArgumentException)
            {
                Report($"Profile '{id}' could not be loaded: {exception.Message}");
            }
        }

        if (profiles.Count == 0)
        {
            var legacy = settingsCoordinator?.Settings;
            var initial = new Profile(
                Guid.NewGuid().ToString("N"),
                "Default",
                legacy?.SelectedDeviceIds,
                legacy?.DeviceChannelAssignments,
                DateTime.UtcNow).Normalize();
            try
            {
                store.Save(initial);
                profiles.Add(initial);
            }
            catch (Exception exception) when (exception is IOException ||
                exception is UnauthorizedAccessException)
            {
                Report($"Initial profile could not be saved: {exception.Message}");
                profiles.Add(initial);
            }
        }

        var requested = settingsCoordinator?.Settings.ActiveProfileId;
        ActiveProfileId = profiles.Any(profile => profile.Id == requested)
            ? requested
            : profiles[0].Id;
        PersistActiveProfile();
        RebuildItems();
        ProfilesChanged?.Invoke(this, EventArgs.Empty);
        ActiveProfileChanged?.Invoke(this, EventArgs.Empty);
        logger.LogDebug("MIDI profiles loaded: count={ProfileCount}, activeProfileId={ActiveProfileId}.",
            profiles.Count, ActiveProfileId);
    }

    public void Initialize() => Load();
    public void LoadProfiles() => Load();

    public bool Select(string profileId)
    {
        if (!profiles.Any(profile => profile.Id == profileId))
        {
            return false;
        }

        if (ActiveProfileId == profileId)
        {
            return true;
        }

        ActiveProfileId = profileId;
        PersistActiveProfile();
        RebuildItems();
        ActiveProfileChanged?.Invoke(this, EventArgs.Empty);
        logger.ProfileSelected(profileId);
        return true;
    }

    public bool SwitchProfile(string profileId) => Select(profileId);

    public Profile Create(string name)
    {
        var normalizedName = ValidateName(name);
        var profile = new Profile(Guid.NewGuid().ToString("N"), normalizedName);
        store.Save(profile);
        profiles.Add(profile);
        profiles.Sort((left, right) => StringComparer.Ordinal.Compare(left.Id, right.Id));
        ActiveProfileId = profile.Id;
        PersistActiveProfile();
        RebuildItems();
        ProfilesChanged?.Invoke(this, EventArgs.Empty);
        ActiveProfileChanged?.Invoke(this, EventArgs.Empty);
        logger.ProfileCreated(profile.Id, profile.Name);
        return profile;
    }

    public Profile CreateProfile(string name) => Create(name);

    public bool TryCreate(string? name, out Profile? profile)
    {
        profile = null;
        if (string.IsNullOrWhiteSpace(name))
        {
            Report("Profile name cannot be empty.");
            return false;
        }
        try
        {
            profile = Create(name);
            return true;
        }
        catch (ArgumentException exception)
        {
            Report(exception.Message);
            return false;
        }
        catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException)
        {
            Report($"Profile could not be saved: {exception.Message}");
            return false;
        }
    }

    public bool Rename(string profileId, string name)
    {
        var normalizedName = ValidateName(name);
        var existing = profiles.FirstOrDefault(profile => profile.Id == profileId)
            ?? throw new KeyNotFoundException($"Profile '{profileId}' was not found.");
        var updated = existing with { Name = normalizedName, LastEdited = DateTime.UtcNow };
        store.Save(updated);
        Replace(updated);
        RebuildItems();
        ProfilesChanged?.Invoke(this, EventArgs.Empty);
        logger.ProfileRenamed(updated.Id, updated.Name);
        return true;
    }

    public bool RenameProfile(string profileId, string name) => Rename(profileId, name);

    public bool TryRename(string profileId, string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            Report("Profile name cannot be empty.");
            return false;
        }
        try
        {
            return Rename(profileId, name);
        }
        catch (ArgumentException exception)
        {
            Report(exception.Message);
            return false;
        }
        catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException)
        {
            Report($"Profile could not be saved: {exception.Message}");
            return false;
        }
    }

    public bool Delete(string profileId)
    {
        if (profiles.Count <= 1 || !profiles.Any(profile => profile.Id == profileId))
        {
            return false;
        }
        try
        {
            store.Delete(profileId);
        }
        catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException)
        {
            Report($"Profile could not be deleted: {exception.Message}");
            return false;
        }
        var deletedIndex = profiles.FindIndex(profile => profile.Id == profileId);
        profiles.RemoveAt(deletedIndex);
        if (ActiveProfileId == profileId)
        {
            var replacementIndex = deletedIndex > 0 ? deletedIndex - 1 : 0;
            ActiveProfileId = profiles[replacementIndex].Id;
            PersistActiveProfile();
            ActiveProfileChanged?.Invoke(this, EventArgs.Empty);
        }

        RebuildItems();
        ProfilesChanged?.Invoke(this, EventArgs.Empty);
        logger.ProfileDeleted(profileId);
        return true;
    }

    public bool DeleteProfile(string profileId) => Delete(profileId);

    public bool UpdateActiveState(
        IEnumerable<string> selectedDeviceIds,
        IReadOnlyDictionary<string, int> channelAssignments)
    {
        var active = ActiveProfile;
        if (active is null)
        {
            return false;
        }
        var updated = active with
        {
            SelectedDeviceIds = selectedDeviceIds.ToArray(),
            DeviceChannelAssignments = new Dictionary<string, int>(
                channelAssignments, StringComparer.Ordinal),
            LastEdited = DateTime.UtcNow
        };
        try
        {
            store.Save(updated);
        }
        catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException)
        {
            Report($"Profile could not be saved: {exception.Message}");
            return false;
        }
        Replace(updated);
        profileItems.FirstOrDefault(item => item.Id == updated.Id)?.Update(updated);
        logger.ProfileStateChanged(updated.Id, updated.SelectedDeviceIds?.Count ?? 0,
            updated.DeviceChannelAssignments?.Count ?? 0);
        return true;
    }

    public string GetDisplayName(Profile profile)
        => profileItems.FirstOrDefault(item => item.Id == profile.Id)?.DisplayName ?? profile.Name;

    public string GetDisplayLabel(string profileId)
        => profileItems.FirstOrDefault(item => item.Id == profileId)?.DisplayName ?? string.Empty;

    private static string ValidateName(string name)
    {
        var normalized = name.Trim();
        if (normalized.Length == 0)
        {
            throw new ArgumentException("Profile name cannot be empty.", nameof(name));
        }
        return normalized;
    }

    private void Replace(Profile updated)
    {
        var index = profiles.FindIndex(profile => profile.Id == updated.Id);
        if (index >= 0)
        {
            profiles[index] = updated;
        }
    }

    private void RebuildItems()
    {
        profileItems.Clear();
        profileItems.Add(ProfileListItem.CreateEntry);
        var counts = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var profile in profiles)
        {
            counts.TryGetValue(profile.Name, out var count);
            count++;
            counts[profile.Name] = count;
            var item = new ProfileListItem(profile)
            {
                DisplayName = count == 1 ? profile.Name : $"{profile.Name} ({count})",
                IsActive = profile.Id == ActiveProfileId,
                IsDeletable = profiles.Count > 1
            };
            profileItems.Add(item);
        }
    }

    private void PersistActiveProfile()
    {
        settingsCoordinator?.Update(
            current => current with { ActiveProfileId = ActiveProfileId },
            "Active profile");
    }

    private void Report(string message)
    {
        report?.Invoke(message);
        Diagnostic?.Invoke(this, message);
    }
}
