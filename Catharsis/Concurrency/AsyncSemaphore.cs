namespace Catharsis.Concurrency;

///<summary>
///An async-friendly counting semaphore that limits the number of concurrent callers. Acquiring the semaphore returns a
///disposable handle that releases the slot exactly once when disposed, so callers cannot forget to release it.
///</summary>
///<example>
public sealed class AsyncSemaphore : IDisposable
{
    #region Fields
    private bool _disposed;
    private readonly SemaphoreSlim _semaphore;
    #endregion

    #region Constructors
    ///<summary>
    ///Creates a semaphore with the specified number of initially available slots and no upper bound on releases.
    ///</summary>
    ///<param name="initialCount">The initial number of available slots.</param>
    ///<exception cref="ArgumentOutOfRangeException"><paramref name="initialCount"/> is negative.</exception>
    public AsyncSemaphore(int initialCount) { _semaphore = new SemaphoreSlim(initialCount); }

    ///<summary>
    ///Creates a semaphore with the specified number of initially available slots, bounded by a maximum slot count.
    ///</summary>
    ///<param name="initialCount">The initial number of available slots.</param>
    ///<param name="maxCount">The maximum number of slots that may be available at once.</param>
    ///<exception cref="ArgumentOutOfRangeException">
    public AsyncSemaphore(int initialCount, int maxCount) { _semaphore = new SemaphoreSlim(initialCount, maxCount); }
    #endregion

    #region Public methods
    ///<inheritdoc/>
    public void Dispose()
    {
        if(_disposed)
        {
            return;
        }

        _disposed = true;
        _semaphore.Dispose();
    }

    ///<summary>
    ///Attempts to acquire a slot without blocking.
    ///</summary>
    ///<param name="handle">The acquired handle, or <c>null</c> if no slot was available.</param>
    ///<returns><c>true</c> if a slot was acquired; otherwise <c>false</c>.</returns>
    public bool TryWait(out IDisposable? handle)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if(_semaphore.Wait(0))
        {
            handle = new SemaphoreHandle(_semaphore);
            return true;
        }

        handle = null;
        return false;
    }

    ///<summary>
    ///Synchronously acquires a slot. Dispose the returned handle to release it.
    ///</summary>
    ///<returns>A disposable handle that releases the slot when disposed.</returns>
    public IDisposable Wait()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        _semaphore.Wait();
        return new SemaphoreHandle(_semaphore);
    }

    ///<summary>
    ///Asynchronously acquires a slot. Dispose the returned handle to release it.
    ///</summary>
    ///<param name="cancellationToken">A cancellation token that can abandon the wait.</param>
    ///<returns>A disposable handle that releases the slot when disposed.</returns>
    public async Task<IDisposable> WaitAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        return new SemaphoreHandle(_semaphore);
    }

    ///<summary>
    ///Asynchronously acquires a slot. Dispose the returned handle to release it.
    ///</summary>
    ///<param name="cancellationToken">A cancellation token that can abandon the wait.</param>
    ///<returns>A disposable handle that releases the slot when disposed.</returns>
    public async ValueTask<IDisposable> WaitValueAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        return new SemaphoreHandle(_semaphore);
    }
    #endregion

    #region Public properties
    ///<summary>
    ///Gets the number of slots currently available.
    ///</summary>
    public int CurrentCount => _semaphore.CurrentCount;
    #endregion

    private sealed class SemaphoreHandle(SemaphoreSlim semaphore) : IDisposable
    {
        #region Fields
        private int _released;
        #endregion

        #region Public methods
        public void Dispose()
        {
            if(Interlocked.Exchange(ref _released, 1) == 0)
            {
                semaphore.Release();
            }
        }
        #endregion
    }
}
