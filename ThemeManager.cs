using System.IO;

namespace midi_router;

public enum ThemePalette
{
    Light,
    Dark
}

public sealed class ThemeManager : IDisposable
{
    private readonly ApplicationSettingsCoordinator settingsCoordinator;
    private readonly IOperatingSystemThemeProvider operatingSystemTheme;
    private readonly Action<ThemePalette> applyPalette;
    private ApplicationSettings settings = new();
    private bool disposed;

    public ThemeManager(
        ISettingsStore settingsStore,
        IOperatingSystemThemeProvider operatingSystemTheme,
        Action<ThemePalette> applyPalette,
        Action<string>? report = null)
        : this(
            new ApplicationSettingsCoordinator(settingsStore, report),
            operatingSystemTheme,
            applyPalette)
    {
    }

    public ThemeManager(
        ApplicationSettingsCoordinator settingsCoordinator,
        IOperatingSystemThemeProvider operatingSystemTheme,
        Action<ThemePalette> applyPalette)
    {
        this.settingsCoordinator = settingsCoordinator;
        this.operatingSystemTheme = operatingSystemTheme;
        this.applyPalette = applyPalette;
        operatingSystemTheme.ThemeChanged += OnOperatingSystemThemeChanged;
    }

    public AppearanceMode CurrentMode { get; private set; } = AppearanceMode.OsDefault;

    public bool MinimizeToTray { get; private set; }

    public ThemePalette CurrentPalette { get; private set; } = ThemePalette.Light;

    public event EventHandler? Changed;

    public void Load()
    {
        try
        {
            settingsCoordinator.Load();
            var loadedSettings = settingsCoordinator.Settings;
            settings = loadedSettings with { AppearanceMode = loadedSettings.AppearanceMode.Normalize() };
            CurrentMode = settings.AppearanceMode;
            MinimizeToTray = settings.MinimizeToTray;
        }
        catch (Exception exception) when (
            exception is IOException ||
            exception is UnauthorizedAccessException ||
            exception is System.Text.Json.JsonException)
        {
            settings = new ApplicationSettings();
            CurrentMode = AppearanceMode.OsDefault;
            MinimizeToTray = false;
        }

        ApplyCurrentPalette();
    }

    public void Select(AppearanceMode mode)
    {
        CurrentMode = mode.Normalize();
        settingsCoordinator.Update(
            current => current with { AppearanceMode = CurrentMode },
            "Appearance");
        settings = settingsCoordinator.Settings;
        ApplyCurrentPalette();
    }

    public void SelectMinimizeToTray(bool enabled)
    {
        MinimizeToTray = enabled;
        settingsCoordinator.Update(
            current => current with { MinimizeToTray = enabled },
            "Minimize-to-tray");
        settings = settingsCoordinator.Settings;
        Changed?.Invoke(this, EventArgs.Empty);
    }

    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        disposed = true;
        operatingSystemTheme.ThemeChanged -= OnOperatingSystemThemeChanged;
        operatingSystemTheme.Dispose();
    }

    private void OnOperatingSystemThemeChanged(object? sender, EventArgs e)
    {
        if (CurrentMode == AppearanceMode.OsDefault)
        {
            ApplyCurrentPalette();
        }
    }

    private void ApplyCurrentPalette()
    {
        CurrentPalette = CurrentMode == AppearanceMode.Dark ||
            (CurrentMode == AppearanceMode.OsDefault && operatingSystemTheme.IsDarkMode)
            ? ThemePalette.Dark
            : ThemePalette.Light;
        applyPalette(CurrentPalette);
        Changed?.Invoke(this, EventArgs.Empty);
    }
}
