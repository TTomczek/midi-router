using Microsoft.Extensions.Logging;

namespace midi_router;

public static partial class MidiDeviceLogging
{
    [LoggerMessage(1000, LogLevel.Error, "Windows MIDI Services is unavailable.")]
    public static partial void ServiceUnavailable(this ILogger logger);

    [LoggerMessage(1001, LogLevel.Information, "MIDI endpoint enumeration started.")]
    public static partial void EnumerationStarted(this ILogger logger);

    [LoggerMessage(1002, LogLevel.Information, "MIDI endpoint enumeration completed.")]
    public static partial void EnumerationCompleted(this ILogger logger);

    [LoggerMessage(1003, LogLevel.Debug, "MIDI endpoint added: {EndpointId}.")]
    public static partial void EndpointAdded(this ILogger logger, string endpointId);

    [LoggerMessage(1004, LogLevel.Debug, "MIDI endpoint removed: {EndpointId}.")]
    public static partial void EndpointRemoved(this ILogger logger, string endpointId);

    [LoggerMessage(1005, LogLevel.Warning, "MIDI endpoint information could not be read: {EndpointId}.")]
    public static partial void EndpointReadFailed(this ILogger logger, Exception exception, string endpointId);

    [LoggerMessage(1006, LogLevel.Information, "MIDI endpoint watcher stopped.")]
    public static partial void WatcherStopped(this ILogger logger);
}
