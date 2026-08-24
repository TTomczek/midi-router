namespace midi_router;

public interface IOperatingSystemThemeProvider : IDisposable
{
    bool IsDarkMode { get; }

    event EventHandler? ThemeChanged;
}
