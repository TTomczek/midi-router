using Microsoft.Extensions.Logging;

namespace midi_router;

public static partial class MidiRoutingLogging
{
    [LoggerMessage(1100, LogLevel.Information, "MIDI route started: {DeviceId}.")]
    public static partial void RouteStarted(this ILogger logger, string deviceId);

    [LoggerMessage(1101, LogLevel.Information, "MIDI route stopped: {DeviceId}.")]
    public static partial void RouteStopped(this ILogger logger, string deviceId);

    [LoggerMessage(1102, LogLevel.Warning, "MIDI routing diagnostic: {Message}.")]
    public static partial void RoutingDiagnostic(this ILogger logger, string message);

    [LoggerMessage(1103, LogLevel.Error, "MIDI routing send failed: {Message}.")]
    public static partial void RoutingSendFailed(this ILogger logger, string message);
}
