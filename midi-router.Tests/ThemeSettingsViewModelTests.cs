using Xunit;

namespace midi_router.Tests;

public sealed class ThemeSettingsViewModelTests
{
    [Fact]
    public void ExposesExactlyThreeAppearanceOptions()
    {
        using var manager = CreateManager();
        using var viewModel = new ThemeSettingsViewModel(manager);

        Assert.Equal(
            new[] { AppearanceMode.Light, AppearanceMode.Dark, AppearanceMode.OsDefault },
            viewModel.Options);
    }

    [Fact]
    public void CommandSelectsRequestedMode()
    {
        using var manager = CreateManager();
        using var viewModel = new ThemeSettingsViewModel(manager);

        viewModel.SelectModeCommand.Execute(AppearanceMode.Dark);

        Assert.Equal(AppearanceMode.Dark, viewModel.CurrentMode);
    }

    private static ThemeManager CreateManager()
        => new(new Store(), new OsTheme(), _ => { });

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
