using System.Collections.ObjectModel;

namespace midi_router;

public sealed class ProfileManager
{
    private readonly ProfileStore store;
    private readonly ApplicationSettingsCoordinator settings;
    private readonly ObservableCollection<Profile> profiles = new();

    public ProfileManager(ProfileStore store, ApplicationSettingsCoordinator settings)
    {
        this.store = store;
        this.settings = settings;
        Profiles = new ReadOnlyObservableCollection<Profile>(profiles);
        Load();
    }

    public ReadOnlyObservableCollection<Profile> Profiles { get; }
    public Profile ActiveProfile { get; private set; } = null!;
    public event EventHandler? ActiveProfileChanged;

    public void Select(string id)
    {
        var profile = profiles.FirstOrDefault(p => p.Id == id);
        if (profile is null || profile.Id == ActiveProfile.Id) return;
        ActiveProfile = profile;
        PersistActive();
        ActiveProfileChanged?.Invoke(this, EventArgs.Empty);
    }

    public Profile Create(string name = "New profile")
    {
        name = UniqueName(name);
        var profile = new Profile(Guid.NewGuid().ToString("N"), name, DateTime.UtcNow);
        profiles.Add(profile);
        store.Save(profile);
        ActiveProfile = profile;
        PersistActive();
        ActiveProfileChanged?.Invoke(this, EventArgs.Empty);
        return profile;
    }

    public bool Rename(string id, string name)
    {
        var profile = profiles.FirstOrDefault(p => p.Id == id);
        name = name.Trim();
        if (profile is null || name.Length == 0) return false;
        var updated = profile with { Name = UniqueName(name, profile.Id), LastModified = DateTime.UtcNow };
        Replace(updated);
        return true;
    }

    public bool Delete(string id)
    {
        if (profiles.Count <= 1) return false;
        var profile = profiles.FirstOrDefault(p => p.Id == id);
        if (profile is null) return false;
        store.Delete(profile);
        profiles.Remove(profile);
        if (ActiveProfile.Id == id)
        {
            ActiveProfile = profiles[0];
            PersistActive();
            ActiveProfileChanged?.Invoke(this, EventArgs.Empty);
        }
        return true;
    }

    public void UpdateActive(Func<Profile, Profile> update)
    {
        var updated = update(ActiveProfile) with { LastModified = DateTime.UtcNow };
        Replace(updated);
    }

    private void Load()
    {
        profiles.Clear();
        foreach (var profile in store.LoadAll())
            profiles.Add(profile);
        if (profiles.Count == 0)
        {
            // Preserve selections from the pre-profile settings file when creating the first profile.
            var legacy = new Profile(Guid.NewGuid().ToString("N"), "Default", DateTime.UtcNow,
                settings.Settings.SelectedDeviceIds, settings.Settings.DeviceChannelAssignments);
            profiles.Add(legacy);
            store.Save(legacy);
        }
        ActiveProfile = profiles.FirstOrDefault(p => p.Id == settings.Settings.CurrentProfileId)
            ?? profiles[0];
        PersistActive();
    }

    private void Replace(Profile updated)
    {
        var existing = profiles.FirstOrDefault(profile => profile.Id == updated.Id);
        if (existing is null)
            return;
        var index = profiles.IndexOf(existing);
        store.Save(updated);
        profiles[index] = updated;
        if (ActiveProfile.Id == updated.Id) ActiveProfile = updated;
    }

    private string UniqueName(string name, string? exceptId = null)
    {
        if (!profiles.Any(p => p.Id != exceptId && string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase)))
            return name;
        var number = 2;
        while (profiles.Any(p => p.Id != exceptId &&
            string.Equals(p.Name, $"{name} ({number})", StringComparison.OrdinalIgnoreCase))) number++;
        return $"{name} ({number})";
    }

    private void PersistActive() => settings.Update(s => s with { CurrentProfileId = ActiveProfile.Id }, "Profile");
}
