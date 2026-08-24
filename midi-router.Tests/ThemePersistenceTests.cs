using Xunit;

namespace midi_router.Tests;

public sealed class ThemePersistenceTests
{
    [Fact]
    public void NewManagerRestoresPersistedMode()
    {
        var store = new Store();
        using var os = new OsTheme();
        using (var first = new ThemeManager(store, os, _ => { }))
        {
            first.Select(AppearanceMode.Dark);
        }

        using var second = new ThemeManager(store, os, _ => { });
        second.Load();

        Assert.Equal(AppearanceMode.Dark, second.CurrentMode);
        Assert.Equal(ThemePalette.Dark, second.CurrentPalette);
    }

    private sealed class Store : ISettingsStore
    {
        public ApplicationSettings Settings { get; private set; } = new();
        public ApplicationSettings Load() => Settings;
        public void Save(ApplicationSettings settings) => Settings = settings;
    }

    private sealed class OsTheme : IOperatingSystemThemeProvider
    {
        public bool IsDarkMode => false;
        public event EventHandler? ThemeChanged { add { } remove { } }
        public void Dispose() { }
    }
}
