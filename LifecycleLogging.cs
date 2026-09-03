using Microsoft.Extensions.Logging;

namespace midi_router;

public static partial class LifecycleLogging
{
    [LoggerMessage(1300, LogLevel.Information, "Application startup completed.")]
    public static partial void StartupCompleted(this ILogger logger);

    [LoggerMessage(1301, LogLevel.Information, "Application shutdown started: exitCode={ExitCode}, threadId={ThreadId}.")]
    public static partial void ShutdownStarted(this ILogger logger, int exitCode, int threadId);

    [LoggerMessage(1302, LogLevel.Information, "Application shutdown step started: {Step}.")]
    public static partial void ShutdownStepStarted(this ILogger logger, string step);

    [LoggerMessage(1303, LogLevel.Information, "Application shutdown step completed: {Step}, elapsedMs={ElapsedMs}.")]
    public static partial void ShutdownStepCompleted(this ILogger logger, string step, long elapsedMs);

    [LoggerMessage(1304, LogLevel.Error, "Application shutdown step failed: {Step}, elapsedMs={ElapsedMs}.")]
    public static partial void ShutdownStepFailed(
        this ILogger logger, Exception exception, string step, long elapsedMs);

    [LoggerMessage(1305, LogLevel.Debug, "Application background operation started: {Operation}.")]
    public static partial void BackgroundOperationStarted(this ILogger logger, string operation);

    [LoggerMessage(1306, LogLevel.Debug, "Application background operation completed: {Operation}, elapsedMs={ElapsedMs}.")]
    public static partial void BackgroundOperationCompleted(
        this ILogger logger, string operation, long elapsedMs);

    [LoggerMessage(1307, LogLevel.Error, "Application background operation failed: {Operation}, elapsedMs={ElapsedMs}.")]
    public static partial void BackgroundOperationFailed(
        this ILogger logger, Exception exception, string operation, long elapsedMs);
}
