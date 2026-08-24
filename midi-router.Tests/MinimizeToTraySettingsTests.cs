using Xunit;

namespace midi_router.Tests;

public sealed class MinimizeToTraySettingsTests
{
    [Fact]
    public void SelectingTrayPreferencePreservesAppearance()
    {
        var store = new Store(new ApplicationSettings(AppearanceMode.Dark));
        using var manager = new ThemeManager(store, new OsTheme(), _ => { });

        manager.Load();
        manager.SelectMinimizeToTray(true);

        Assert.Equal(AppearanceMode.Dark, store.Settings.AppearanceMode);
        Assert.True(store.Settings.MinimizeToTray);
        Assert.True(manager.MinimizeToTray);
    }

    [Fact]
    public void LoadingRestoresTrayPreference()
    {
        var store = new Store(new ApplicationSettings(AppearanceMode.Light, true));
        using var manager = new ThemeManager(store, new OsTheme(), _ => { });

        manager.Load();

        Assert.True(manager.MinimizeToTray);
    }

    [Fact]
    public void SaveFailureReportsDiagnosticAndKeepsSessionValue()
    {
        var messages = new List<string>();
        using var manager = new ThemeManager(
            new ThrowingStore(),
            new OsTheme(),
            _ => { },
            messages.Add);

        manager.SelectMinimizeToTray(true);

        Assert.True(manager.MinimizeToTray);
        Assert.Single(messages);
        Assert.Contains("settings could not be saved", messages[0], StringComparison.OrdinalIgnoreCase);
    }

    private sealed class Store(ApplicationSettings initial) : ISettingsStore
    {
        public ApplicationSettings Settings { get; private set; } = initial;
        public ApplicationSettings Load() => Settings;
        public void Save(ApplicationSettings settings) => Settings = settings;
    }

    private sealed class ThrowingStore : ISettingsStore
    {
        public ApplicationSettings Load() => new();
        public void Save(ApplicationSettings settings) => throw new IOException("write failed");
    }

    private sealed class OsTheme : IOperatingSystemThemeProvider
    {
        public bool IsDarkMode => false;
        public event EventHandler? ThemeChanged { add { } remove { } }
        public void Dispose() { }
    }
}
