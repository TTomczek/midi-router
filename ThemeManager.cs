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

    public ThemePalette CurrentPalette { get; private set; } = ThemePalette.Light;

    public event EventHandler? Changed;

    public void Load()
    {
        try
        {
            CurrentMode = settingsStore.Load().AppearanceMode.Normalize();
        }
        catch (IOException exception)
        {
            CurrentMode = AppearanceMode.OsDefault;
            report?.Invoke($"Appearance settings could not be loaded: {exception.Message}");
        }
        catch (UnauthorizedAccessException exception)
        {
            CurrentMode = AppearanceMode.OsDefault;
            report?.Invoke($"Appearance settings could not be loaded: {exception.Message}");
        }
        catch (System.Text.Json.JsonException exception)
        {
            CurrentMode = AppearanceMode.OsDefault;
            report?.Invoke($"Appearance settings could not be loaded: {exception.Message}");
        }

        ApplyCurrentPalette();
    }

    public void Select(AppearanceMode mode)
    {
        CurrentMode = mode.Normalize();
        try
        {
            settingsStore.Save(new ApplicationSettings(CurrentMode));
        }
        catch (IOException exception)
        {
            report?.Invoke($"Appearance settings could not be saved: {exception.Message}");
        }
        catch (UnauthorizedAccessException exception)
        {
            report?.Invoke($"Appearance settings could not be saved: {exception.Message}");
        }

        ApplyCurrentPalette();
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
