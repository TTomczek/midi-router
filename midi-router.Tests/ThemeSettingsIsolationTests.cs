using Xunit;

namespace midi_router.Tests;

public sealed class ThemeSettingsIsolationTests
{
    [Fact]
    public void ThemeSelectionOnlyInvokesPaletteApplication()
    {
        var paletteApplications = 0;
        using var manager = new ThemeManager(
            new Store(),
            new OsTheme(),
            _ => paletteApplications++);

        manager.Select(AppearanceMode.Dark);

        Assert.Equal(1, paletteApplications);
    }

    private sealed class Store : ISettingsStore
    {
        public ApplicationSettings Load() => new();
        public void Save(ApplicationSettings settings) { }
    }

    private sealed class OsTheme : IOperatingSystemThemeProvider
    {
        public bool IsDarkMode => false;
        public event EventHandler? ThemeChanged { add { } remove { } }
        public void Dispose() { }
    }
}
