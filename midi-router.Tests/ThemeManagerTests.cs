using Xunit;

namespace midi_router.Tests;

public sealed class ThemeManagerTests
{
    [Theory]
    [InlineData(AppearanceMode.Light, false, ThemePalette.Light)]
    [InlineData(AppearanceMode.Dark, false, ThemePalette.Dark)]
    [InlineData(AppearanceMode.OsDefault, false, ThemePalette.Light)]
    [InlineData(AppearanceMode.OsDefault, true, ThemePalette.Dark)]
    public void SelectResolvesPalette(AppearanceMode mode, bool osDark, ThemePalette expected)
    {
        using var os = new FakeOperatingSystemTheme(osDark);
        using var manager = new ThemeManager(
            new FakeSettingsStore(),
            os,
            palette => Assert.Equal(expected, palette));

        manager.Select(mode);

        Assert.Equal(expected, manager.CurrentPalette);
    }

    [Fact]
    public void OsDefaultFollowsOperatingSystemChanges()
    {
        var palettes = new List<ThemePalette>();
        using var os = new FakeOperatingSystemTheme(false);
        using var manager = new ThemeManager(new FakeSettingsStore(), os, palettes.Add);

        manager.Select(AppearanceMode.OsDefault);
        os.SetDark(true);

        Assert.Equal(ThemePalette.Dark, palettes[^1]);
    }

    [Fact]
    public void ExplicitModeIgnoresOperatingSystemChanges()
    {
        var palettes = new List<ThemePalette>();
        using var os = new FakeOperatingSystemTheme(false);
        using var manager = new ThemeManager(new FakeSettingsStore(), os, palettes.Add);

        manager.Select(AppearanceMode.Light);
        os.SetDark(true);

        Assert.Equal(ThemePalette.Light, palettes[^1]);
    }

    [Fact]
    public void SettingsFailureFallsBackAndReportsDiagnostic()
    {
        var messages = new List<string>();
        using var manager = new ThemeManager(
            new ThrowingSettingsStore(),
            new FakeOperatingSystemTheme(false),
            _ => { },
            messages.Add);

        manager.Load();

        Assert.Equal(AppearanceMode.OsDefault, manager.CurrentMode);
        Assert.Single(messages);
    }

    private sealed class FakeSettingsStore : ISettingsStore
    {
        public ApplicationSettings Settings { get; private set; } = new();
        public ApplicationSettings Load() => Settings;
        public void Save(ApplicationSettings settings) => Settings = settings;
    }

    private sealed class ThrowingSettingsStore : ISettingsStore
    {
        public ApplicationSettings Load() => throw new IOException("read failed");
        public void Save(ApplicationSettings settings) => throw new IOException("write failed");
    }

    private sealed class FakeOperatingSystemTheme(bool isDark) : IOperatingSystemThemeProvider
    {
        public bool IsDarkMode { get; private set; } = isDark;
        public event EventHandler? ThemeChanged;
        public void SetDark(bool value)
        {
            IsDarkMode = value;
            ThemeChanged?.Invoke(this, EventArgs.Empty);
        }
        public void Dispose() { }
    }
}
