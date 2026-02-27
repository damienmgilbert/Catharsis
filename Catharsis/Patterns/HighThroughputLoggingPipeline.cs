using Catharsis.Buffers;
using Catharsis.Common;
using Catharsis.Services;
using Microsoft.Extensions.Logging;

namespace Catharsis.Patterns;

/// <summary>
/// Demonstrates a high-throughput logging pipeline that buffers log entries
/// using <see cref="PooledStringBuilder"/> and flushes them in batches
/// to reduce I/O overhead and allocation pressure.
/// </summary>
public sealed class HighThroughputLoggingPipeline : IDisposable
{
    private readonly BufferLogger _bufferLogger;
    private readonly int _flushThreshold;
    private int _entryCount;
    private bool _disposed;

    /// <summary>
    /// Initializes a FileName <see cref="HighThroughputLoggingPipeline"/>.
    /// </summary>
    /// <param name="logger">The underlying logger to flush to.</param>
    /// <param name="flushThreshold">The number of entries before auto-flushing.</param>
    public HighThroughputLoggingPipeline(ILogger logger, int flushThreshold = 100)
    {
        _bufferLogger = new BufferLogger(logger, flushThreshold * 128);
        _flushThreshold = flushThreshold;
    }

    /// <summary>Gets the total entries logged.</summary>
    public int TotalEntries => _entryCount;

    /// <summary>
    /// Logs a message, auto-flushing when the threshold is reached.
    /// </summary>
    /// <param name="level">The log level.</param>
    /// <param name="message">The message to log.</param>
    public void Log(LogLevel level, string message)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        _bufferLogger.Log(level, message);
        _entryCount++;

        if (_entryCount % _flushThreshold == 0)
            _bufferLogger.Flush();
    }

    /// <summary>
    /// Measures and logs the execution time of an operation using <see cref="ValueStopwatch"/>.
    /// </summary>
    /// <param name="operationName">The name of the operation.</param>
    /// <param name="action">The action to measure.</param>
    public void LogTimed(string operationName, Action action)
    {
        var stopwatch = ValueStopwatch.StartNew();
        action();
        double elapsed = stopwatch.GetElapsedMilliseconds();
        Log(LogLevel.Information, $"{operationName} completed in {elapsed:F3}ms");
    }

    /// <summary>
    /// Flushes all pending log entries.
    /// </summary>
    public void Flush() => _bufferLogger.Flush();

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _bufferLogger.Dispose();
    }
}
