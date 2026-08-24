namespace midi_router;

public enum AppearanceMode
{
    OsDefault,
    Light,
    Dark
}

public static class AppearanceModeExtensions
{
    public static AppearanceMode Parse(string? value)
        => Enum.TryParse<AppearanceMode>(value, ignoreCase: true, out var mode) &&
            Enum.IsDefined(mode)
            ? mode
            : AppearanceMode.OsDefault;

    public static AppearanceMode Normalize(this AppearanceMode mode)
        => Enum.IsDefined(mode) ? mode : AppearanceMode.OsDefault;
}
