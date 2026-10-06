using Catharsis.Buffers;
using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Catharsis.Services;

///<summary>
///A high-performance logger that writes log entries into pooled buffers before flushing to an underlying <see
///cref="ILogger"/>, reducing allocation overhead in high-throughput logging scenarios.
///</summary>
public sealed partial class BufferLogger : IDisposable
{
    #region Fields
    private readonly PooledStringBuilder _builder;
    private bool _disposed;
    private readonly ILogger _innerLogger;
    private int _pendingEntries;
    private readonly Lock _syncLock = new();
    #endregion

    #region Constructors
    ///<summary>
    ///Initializes a new <see cref="BufferLogger"/> wrapping the specified logger.
    ///</summary>
    ///<param name="logger">The underlying logger to flush entries to.</param>
    ///<param name="bufferCapacity">The initial buffer capacity for log message assembly.</param>
    public BufferLogger(ILogger logger, int bufferCapacity = 4096)
    {
        Guard.IsNotNull(logger);
        Guard.IsGreaterThan(bufferCapacity, 0);

        _innerLogger = logger;
        _builder = new PooledStringBuilder(bufferCapacity);
    }
    #endregion

    #region Private methods
    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Flushing {EntryCount} buffered log entries:\n{Content}")]
    partial void LogFlush(int entryCount, string content);
    #endregion

    #region Public methods
    ///<summary>
    ///Flushes and disposes the buffer logger.
    ///</summary>
    public void Dispose()
    {
        if(_disposed)
        {
            return;
        }

        Flush();

        _disposed = true;
        _builder.Dispose();
    }

    ///<summary>
    ///Flushes all pending log entries to the underlying logger.
    ///</summary>
    public void Flush()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        string output;
        int entries;

        lock(_syncLock)
        {
            if(_pendingEntries == 0)
            {
                return;
            }

            output = _builder.ToString();
            entries = _pendingEntries;
            _builder.Clear();
            _pendingEntries = 0;
        }

        LogFlush(entries, output);
    }

    ///<summary>
    ///Buffers a log entry at the specified level.
    ///</summary>
    ///<param name="level">The log level.</param>
    ///<param name="message">The log message.</param>
    public void Log(LogLevel level, ReadOnlySpan<char> message)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        lock(_syncLock)
        {
            _builder.Append('[');
            _builder.Append(level.ToString().AsSpan());
            _builder.Append("] ".AsSpan());
            _builder.Append(message);
            _builder.AppendLine();
            _pendingEntries++;
        }
    }

    ///<summary>
    ///Buffers a log entry at the specified level.
    ///</summary>
    ///<param name="level">The log level.</param>
    ///<param name="message">The log message.</param>
    public void Log(LogLevel level, string message) => Log(level, message.AsSpan());
    #endregion

    #region Public properties
    ///<summary>
    ///Gets the number of pending (unflushed) log entries.
    ///</summary>
    public int PendingEntries => _pendingEntries;
    #endregion
}
