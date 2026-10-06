namespace Catharsis.Concurrency;

///<summary>
///An async-friendly countdown latch, analogous to <see cref="CountdownEvent"/> but awaitable without blocking a
///thread. All waiters are released once the count reaches zero.
///</summary>
public sealed class AsyncCountdownEvent
{
    #region Fields
    readonly Lock _gate = new();
    readonly TaskCompletionSource _tcs = new(TaskCreationOptions.RunContinuationsAsynchronously);
    int _remaining;
    #endregion

    #region Public methods
    ///<summary>
    ///Creates a countdown latch with the specified initial count.
    ///</summary>
    ///<param name="initialCount">The number of signals required before waiters are released.</param>
    ///<exception cref="ArgumentOutOfRangeException"><paramref name="initialCount"/> is negative.</exception>
    public AsyncCountdownEvent(int initialCount)
    {
        if (initialCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(initialCount), "Initial count must not be negative.");
        }

        _remaining = initialCount;

        if (initialCount == 0)
        {
            _tcs.TrySetResult();
        }
    }

    ///<summary>
    ///Decrements the count by the specified amount, releasing all waiters once it reaches zero.
    ///</summary>
    ///<param name="count">The number of signals to apply. Defaults to 1.</param>
    ///<exception cref="ArgumentOutOfRangeException"><paramref name="count"/> is less than 1.</exception>
    ///<exception cref="InvalidOperationException"><paramref name="count"/> exceeds the current count.</exception>
    public void Signal(int count = 1)
    {
        if (count < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(count), "Count must be at least 1.");
        }

        lock (_gate)
        {
            if (count > _remaining)
            {
                throw new InvalidOperationException("Signal count exceeds the current count.");
            }

            _remaining -= count;

            if (_remaining > 0)
            {
                return;
            }
        }

        _tcs.TrySetResult();
    }

    ///<summary>
    ///Asynchronously waits until the count reaches zero.
    ///</summary>
    ///<param name="cancellationToken">A cancellation token that can abandon the wait.</param>
    ///<returns>A task that completes once the count reaches zero.</returns>
    public Task WaitAsync(CancellationToken cancellationToken = default) => _tcs.Task.WaitAsync(cancellationToken);
    #endregion

    #region Public properties
    ///<summary>
    ///Gets the number of remaining signals required before waiters are released.
    ///</summary>
    public int CurrentCount
    {
        get
        {
            lock (_gate)
            {
                return _remaining;
            }
        }
    }
    #endregion
}
