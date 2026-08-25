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

    [LoggerMessage(1104, LogLevel.Information, "MIDI device selection changed: {DeviceId}, selected={Selected}.")]
    public static partial void DeviceSelectionChanged(
        this ILogger logger,
        string deviceId,
        bool selected);

    [LoggerMessage(1105, LogLevel.Information, "MIDI channel selection changed: {DeviceId}, channel={Channel}.")]
    public static partial void ChannelSelectionChanged(
        this ILogger logger,
        string deviceId,
        int channel);
}
