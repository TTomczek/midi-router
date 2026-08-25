using System.IO;
using System.Text;
using Microsoft.Extensions.Logging;

namespace midi_router;

public sealed class RotatingFileLoggerProvider : ILoggerProvider
{
    private readonly string filePath;
    private readonly long maxFileSizeBytes;
    private readonly int retainedFileCount;
    private readonly object synchronization = new();
    private bool disposed;

    public RotatingFileLoggerProvider(
        string filePath,
        long maxFileSizeBytes = 5 * 1024 * 1024,
        int retainedFileCount = 5)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("A log file path is required.", nameof(filePath));
        }

        if (maxFileSizeBytes <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxFileSizeBytes));
        }

        if (retainedFileCount < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(retainedFileCount));
        }

        this.filePath = filePath;
        this.maxFileSizeBytes = maxFileSizeBytes;
        this.retainedFileCount = retainedFileCount;
        Directory.CreateDirectory(Path.GetDirectoryName(filePath) ?? ".");
    }

    public ILogger CreateLogger(string categoryName)
        => new RotatingFileLogger(this, categoryName);

    internal void Write(string categoryName, LogLevel level, EventId eventId, string message, Exception? exception)
    {
        var timestamp = DateTimeOffset.Now.ToString("yyyy-MM-dd HH:mm:ss.fff zzz");
        var exceptionText = exception is null ? string.Empty : Environment.NewLine + exception;
        var line = $"{timestamp} [{level}] {categoryName} ({eventId.Id}) {message}{exceptionText}{Environment.NewLine}";
        var bytes = Encoding.UTF8.GetBytes(line);

        lock (synchronization)
        {
            ObjectDisposedException.ThrowIf(disposed, this);
            RotateIfNeeded(bytes.Length);
            using var stream = new FileStream(
                filePath,
                FileMode.Append,
                FileAccess.Write,
                FileShare.Read,
                bufferSize: 4096,
                options: FileOptions.SequentialScan);
            stream.Write(bytes);
        }
    }

    private void RotateIfNeeded(int nextMessageSize)
    {
        var currentLength = File.Exists(filePath) ? new FileInfo(filePath).Length : 0;
        if (currentLength == 0 || currentLength + nextMessageSize <= maxFileSizeBytes)
        {
            return;
        }

        for (var index = retainedFileCount - 1; index >= 1; index--)
        {
            var source = $"{filePath}.{index}";
            var destination = $"{filePath}.{index + 1}";
            if (File.Exists(destination))
            {
                File.Delete(destination);
            }

            if (File.Exists(source))
            {
                File.Move(source, destination);
            }
        }

        var firstArchive = $"{filePath}.1";
        if (File.Exists(firstArchive))
        {
            File.Delete(firstArchive);
        }

        File.Move(filePath, firstArchive);
    }

    public void Dispose()
    {
        lock (synchronization)
        {
            disposed = true;
        }
    }

    private sealed class RotatingFileLogger(
        RotatingFileLoggerProvider provider,
        string categoryName) : ILogger
    {
        public IDisposable BeginScope<TState>(TState state)
            where TState : notnull
            => NullScope.Instance;

        public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            ArgumentNullException.ThrowIfNull(formatter);
            provider.Write(categoryName, logLevel, eventId, formatter(state, exception), exception);
        }
    }

    private sealed class NullScope : IDisposable
    {
        public static NullScope Instance { get; } = new();
        public void Dispose() { }
    }
}
