using System.IO;

namespace midi_router;

public enum ThemePalette
{
    Light,
    Dark
}

public sealed class ThemeManager : IDisposable
{
    private readonly ISettingsStore settingsStore;
    private readonly IOperatingSystemThemeProvider operatingSystemTheme;
    private readonly Action<ThemePalette> applyPalette;
    private readonly Action<string>? report;
    private ApplicationSettings settings = new();
    private bool disposed;

    public ThemeManager(
        ISettingsStore settingsStore,
        IOperatingSystemThemeProvider operatingSystemTheme,
        Action<ThemePalette> applyPalette,
        Action<string>? report = null)
    {
        this.settingsStore = settingsStore;
        this.operatingSystemTheme = operatingSystemTheme;
        this.applyPalette = applyPalette;
        this.report = report;
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
            var loadedSettings = settingsStore.Load();
            settings = loadedSettings with
            {
                AppearanceMode = loadedSettings.AppearanceMode.Normalize()
            };
            CurrentMode = settings.AppearanceMode;
            MinimizeToTray = settings.MinimizeToTray;
        }
        catch (IOException exception)
        {
            settings = new ApplicationSettings();
            CurrentMode = AppearanceMode.OsDefault;
            MinimizeToTray = false;
            report?.Invoke($"Appearance settings could not be loaded: {exception.Message}");
        }
        catch (UnauthorizedAccessException exception)
        {
            settings = new ApplicationSettings();
            CurrentMode = AppearanceMode.OsDefault;
            MinimizeToTray = false;
            report?.Invoke($"Appearance settings could not be loaded: {exception.Message}");
        }
        catch (System.Text.Json.JsonException exception)
        {
            settings = new ApplicationSettings();
            CurrentMode = AppearanceMode.OsDefault;
            MinimizeToTray = false;
            report?.Invoke($"Appearance settings could not be loaded: {exception.Message}");
        }

        ApplyCurrentPalette();
    }

    public void Select(AppearanceMode mode)
    {
        CurrentMode = mode.Normalize();
        settings = settings with { AppearanceMode = CurrentMode };
        SaveSettings(settings, "Appearance");
        ApplyCurrentPalette();
    }

    public void SelectMinimizeToTray(bool enabled)
    {
        MinimizeToTray = enabled;
        settings = settings with { MinimizeToTray = enabled };
        SaveSettings(settings, "Minimize-to-tray");
        Changed?.Invoke(this, EventArgs.Empty);
    }

    private void SaveSettings(ApplicationSettings value, string settingName)
    {
        try
        {
            settingsStore.Save(value);
        }
        catch (IOException exception)
        {
            report?.Invoke($"{settingName} settings could not be saved: {exception.Message}");
        }
        catch (UnauthorizedAccessException exception)
        {
            report?.Invoke($"{settingName} settings could not be saved: {exception.Message}");
        }
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
