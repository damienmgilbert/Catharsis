using System.Threading.Channels;
using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Catharsis.Services;

///<summary>
///A bounded-channel-backed background work queue: callers enqueue delegates, and a single background loop drains
///and runs them one at a time, in order, isolating callers from exceptions thrown by individual work items.
///</summary>
public sealed partial class BackgroundQueueService : IAsyncDisposable
{
    #region Fields
    readonly Channel<Func<CancellationToken, Task>> _channel;
    readonly ILogger<BackgroundQueueService> _logger;
    readonly Task _processingLoop;
    readonly CancellationTokenSource _stoppingSource = new();
    bool _disposed;
    long _queuedCount;
    #endregion

    #region Constructors
    ///<summary>
    ///Initializes a new <see cref="BackgroundQueueService"/> and immediately starts its background processing loop.
    ///</summary>
    ///<param name="logger">The logger for diagnostic output.</param>
    ///<param name="capacity">The maximum number of queued-but-not-yet-run work items.</param>
    ///<exception cref="ArgumentOutOfRangeException"><paramref name="capacity"/> is not positive.</exception>
    public BackgroundQueueService(ILogger<BackgroundQueueService> logger, int capacity = 100)
    {
        Guard.IsNotNull(logger);
        Guard.IsGreaterThan(capacity, 0);

        _logger = logger;
        _channel = Channel.CreateBounded<Func<CancellationToken, Task>>(capacity);
        _processingLoop = ProcessQueueAsync(_stoppingSource.Token);
    }
    #endregion

    #region Private methods
    async Task ProcessQueueAsync(CancellationToken stoppingToken)
    {
        try
        {
            await foreach (Func<CancellationToken, Task> workItem in _channel.Reader.ReadAllAsync(stoppingToken).ConfigureAwait(false))
            {
                try
                {
                    await workItem(stoppingToken).ConfigureAwait(false);
                }
                catch (Exception exception) when (exception is not OperationCanceledException)
                {
                    LogWorkItemFailed(exception);
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
    }
    #endregion

    #region Public methods
    ///<summary>
    ///Queues a work item for background execution. Waits if the queue is currently full.
    ///</summary>
    ///<param name="workItem">The work item to run in the background.</param>
    ///<param name="cancellationToken">A token that cancels the wait for queue space.</param>
    ///<returns>A task that completes once the work item has been queued (not once it has run).</returns>
    ///<exception cref="ArgumentNullException"><paramref name="workItem"/> is <c>null</c>.</exception>
    public async ValueTask EnqueueAsync(Func<CancellationToken, Task> workItem, CancellationToken cancellationToken = default)
    {
        Guard.IsNotNull(workItem);
        ObjectDisposedException.ThrowIf(_disposed, this);

        await _channel.Writer.WriteAsync(workItem, cancellationToken).ConfigureAwait(false);
        Interlocked.Increment(ref _queuedCount);
    }

    ///<summary>
    ///Stops accepting new work, cancels the running background loop's token, and waits for it to finish.
    ///</summary>
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        _channel.Writer.TryComplete();
        await _stoppingSource.CancelAsync().ConfigureAwait(false);

        try
        {
            await _processingLoop.ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
        }

        _stoppingSource.Dispose();
    }
    #endregion

    #region Public properties
    ///<summary>
    ///Gets the total number of work items enqueued so far.
    ///</summary>
    public long QueuedCount => Interlocked.Read(ref _queuedCount);
    #endregion

    #region Log messages
    [LoggerMessage(EventId = 1, Level = LogLevel.Error, Message = "Background work item failed.")]
    partial void LogWorkItemFailed(Exception exception);
    #endregion
}
