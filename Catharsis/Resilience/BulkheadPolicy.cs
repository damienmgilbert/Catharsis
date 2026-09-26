namespace Catharsis.Resilience;

///<summary>
///Limits the number of concurrent executions of an operation, isolating it from resource exhaustion caused by an
///unbounded number of simultaneous callers. Callers beyond the concurrency limit wait for a slot, bounded by an
///optional queue length; once both the execution slots and the queue are full, further callers are rejected
///immediately with <see cref="BulkheadRejectedException"/>.
///</summary>
public sealed class BulkheadPolicy : IAsyncPolicy, IDisposable
{
    #region Fields
    readonly SemaphoreSlim _executionSlots;
    readonly SemaphoreSlim? _queueSlots;
    bool _disposed;
    #endregion

    #region Public methods
    ///<summary>
    ///Creates a bulkhead policy with the specified concurrency limit and, optionally, a bounded waiting queue.
    ///</summary>
    ///<param name="maxConcurrency">The maximum number of operations that may execute at once.</param>
    ///<param name="maxQueueLength">
    ///The maximum number of callers that may wait for a free execution slot. Defaults to 0, meaning callers are
    ///rejected immediately once all execution slots are busy.
    ///</param>
    ///<exception cref="ArgumentOutOfRangeException">
    ///<paramref name="maxConcurrency"/> is less than 1, or <paramref name="maxQueueLength"/> is negative.
    ///</exception>
    public BulkheadPolicy(int maxConcurrency, int maxQueueLength = 0)
    {
        if(maxConcurrency < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(maxConcurrency), "Maximum concurrency must be at least 1.");
        }

        if(maxQueueLength < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxQueueLength), "Maximum queue length must not be negative.");
        }

        _executionSlots = new SemaphoreSlim(maxConcurrency, maxConcurrency);
        _queueSlots = maxQueueLength > 0 ? new SemaphoreSlim(maxQueueLength, maxQueueLength) : null;
        MaxConcurrency = maxConcurrency;
        MaxQueueLength = maxQueueLength;
    }

    ///<inheritdoc/>
    public void Dispose()
    {
        if(_disposed)
        {
            return;
        }

        _disposed = true;
        _executionSlots.Dispose();
        _queueSlots?.Dispose();
    }

    ///<summary>
    ///Asynchronously executes the specified operation, subject to the bulkhead's concurrency and queue limits.
    ///</summary>
    ///<typeparam name="TResult">The return type of the operation.</typeparam>
    ///<param name="operation">The async operation to execute. Receives a <see cref="CancellationToken"/>.</param>
    ///<param name="cancellationToken">A cancellation token that can abandon a queued wait.</param>
    ///<returns>The result of the successful operation.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="operation"/> is <c>null</c>.</exception>
    ///<exception cref="BulkheadRejectedException">Both the execution slots and the queue are full.</exception>
    public async Task<TResult> ExecuteAsync<TResult>(Func<CancellationToken, Task<TResult>> operation, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);
        ObjectDisposedException.ThrowIf(_disposed, this);

        if(_executionSlots.Wait(0, cancellationToken))
        {
            try
            {
                return await operation(cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                _executionSlots.Release();
            }
        }

        if(_queueSlots is null || !_queueSlots.Wait(0, cancellationToken))
        {
            throw new BulkheadRejectedException();
        }

        try
        {
            await _executionSlots.WaitAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _queueSlots.Release();
        }

        try
        {
            return await operation(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _executionSlots.Release();
        }
    }

    ///<summary>
    ///Asynchronously executes the specified operation, subject to the bulkhead's concurrency and queue limits.
    ///</summary>
    ///<param name="operation">The async operation to execute. Receives a <see cref="CancellationToken"/>.</param>
    ///<param name="cancellationToken">A cancellation token that can abandon a queued wait.</param>
    ///<exception cref="ArgumentNullException"><paramref name="operation"/> is <c>null</c>.</exception>
    ///<exception cref="BulkheadRejectedException">Both the execution slots and the queue are full.</exception>
    public async Task ExecuteAsync(Func<CancellationToken, Task> operation, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);

        await ExecuteAsync<object?>(async ct =>
        {
            await operation(ct).ConfigureAwait(false);
            return null;
        }, cancellationToken).ConfigureAwait(false);
    }
    #endregion

    #region Public properties
    ///<summary>
    ///Gets the configured maximum concurrency.
    ///</summary>
    public int MaxConcurrency { get; }

    ///<summary>
    ///Gets the configured maximum queue length.
    ///</summary>
    public int MaxQueueLength { get; }

    ///<summary>
    ///Gets the number of execution slots currently available.
    ///</summary>
    public int AvailableConcurrency => _executionSlots.CurrentCount;
    #endregion
}
