using Xunit;

namespace midi_router.Tests;

public sealed class ProfileManagerTests
{
    [Fact]
    public void InitializesOneProfileAndNumbersDuplicateNames()
    {
        var store = new FakeStore();
        var manager = new ProfileManager(store);
        manager.Load();
        var first = manager.Create("Studio");
        manager.Create("Studio");

        Assert.Equal(3, manager.Profiles.Count);
        Assert.Equal("Create profile", manager.ProfileItems[0].DisplayName);
        Assert.Contains("Default", manager.ProfileItems.Select(item => item.DisplayName));
        Assert.Contains("Studio", manager.ProfileItems.Select(item => item.DisplayName));
        Assert.Contains("Studio (2)", manager.ProfileItems.Select(item => item.DisplayName));
    }

    [Fact]
    public void CannotDeleteFinalProfileAndRenameKeepsIdentity()
    {
        var store = new FakeStore();
        var manager = new ProfileManager(store);
        manager.Load();
        var id = manager.ActiveProfileId!;

        Assert.False(manager.Delete(id));
        manager.Rename(id, "  Renamed ");

        Assert.Equal(id, manager.ActiveProfileId);
        Assert.Equal("Renamed", manager.ActiveProfile!.Name);
    }

    [Fact]
    public void RestoresActiveProfileFromGlobalSettings()
    {
        var store = new FakeStore();
        var settingsStore = new SettingsStore(
            new ApplicationSettings(ActiveProfileId: "second"));
        var settings = new ApplicationSettingsCoordinator(settingsStore);
        settings.Load();
        store.Save(new Profile("first", "First"));
        store.Save(new Profile("second", "Second"));

        var manager = new ProfileManager(store, settings);
        manager.Load();

        Assert.Equal("second", manager.ActiveProfileId);
    }

    [Fact]
    public void UsesFirstProfileAndPersistsItWhenRememberedProfileIsUnavailable()
    {
        var store = new FakeStore();
        store.Save(new Profile("first", "First"));
        store.Save(new Profile("second", "Second"));
        var settingsStore = new SettingsStore(
            new ApplicationSettings(ActiveProfileId: "missing"));
        var settings = new ApplicationSettingsCoordinator(settingsStore);
        settings.Load();

        var manager = new ProfileManager(store, settings);
        manager.Load();

        Assert.Equal("first", manager.ActiveProfileId);
        Assert.Equal("first", settings.Settings.ActiveProfileId);
    }

    [Fact]
    public void DeletesConfirmedProfileAndFallsBackWhenActive()
    {
        var store = new FakeStore();
        var manager = new ProfileManager(store);
        manager.Load();
        var second = manager.Create("Second");

        Assert.True(manager.Delete(second.Id));
        Assert.DoesNotContain(second.Id, manager.Profiles.Select(profile => profile.Id));
        Assert.Single(manager.Profiles);
        Assert.NotNull(manager.ActiveProfile);
    }

    [Fact]
    public void DeletesActiveProfileSelectingPreviousThenFollowingProfile()
    {
        var store = new FakeStore();
        store.Save(new Profile("a", "First"));
        store.Save(new Profile("b", "Second"));
        store.Save(new Profile("c", "Third"));
        var manager = new ProfileManager(store);
        manager.Load();

        manager.Select("b");
        Assert.True(manager.Delete("b"));
        Assert.Equal("a", manager.ActiveProfileId);

        Assert.True(manager.Delete("a"));
        Assert.Equal("c", manager.ActiveProfileId);
    }

    private sealed class FakeStore : IProfileStore
    {
        private readonly Dictionary<string, Profile> values = new(StringComparer.Ordinal);
        public IEnumerable<string> ListProfileIds() => values.Keys;
        public Profile Load(string profileId) => values[profileId];
        public void Save(Profile profile) => values[profile.Id] = profile;
        public void Delete(string profileId) => values.Remove(profileId);
    }

    private sealed class SettingsStore(ApplicationSettings initial) : ISettingsStore
    {
        private ApplicationSettings settings = initial;
        public ApplicationSettings Load() => settings;
        public void Save(ApplicationSettings value) => settings = value;
    }
}
