namespace Catharsis.Common;

///<summary>
///An async-compatible lock that serializes access to buffer operations, preventing concurrent buffer mutations using
///<see cref="SemaphoreSlim"/>.
///</summary>
public sealed class AsyncBufferLock : IDisposable
{
    #region Fields
    bool _disposed;
    readonly SemaphoreSlim _semaphore = new(1, 1);
    #endregion

    #region Public methods
    ///<inheritdoc/>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _semaphore.Dispose();
    }

    ///<summary>
    ///Synchronously acquires the lock. Dispose the returned handle to release.
    ///</summary>
    ///<returns>A disposable handle that releases the lock when disposed.</returns>
    public IDisposable Lock()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        _semaphore.Wait();
        return new LockHandle(_semaphore);
    }

    ///<summary>
    ///Asynchronously acquires the lock. Dispose the returned handle to release.
    ///</summary>
    ///<param name="cancellationToken">A cancellation token.</param>
    ///<returns>A disposable handle that releases the lock when disposed.</returns>
    public async Task<IDisposable> LockAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        return new LockHandle(_semaphore);
    }

    ///<summary>
    ///Asynchronously acquires the lock. Dispose the returned handle to release.
    ///</summary>
    ///<param name="cancellationToken">A cancellation token.</param>
    ///<returns>A disposable handle that releases the lock when disposed.</returns>
    public async ValueTask<IDisposable> LockValueAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        return new LockHandle(_semaphore);
    }

    ///<summary>
    ///Attempts to acquire the lock without blocking.
    ///</summary>
    ///<param name="handle">The lock handle if acquired.</param>
    ///<returns><c>true</c> if the lock was acquired; otherwise <c>false</c>.</returns>
    public bool TryLock(out IDisposable? handle)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (_semaphore.Wait(0))
        {
            handle = new LockHandle(_semaphore);
            return true;
        }

        handle = null;
        return false;
    }
    #endregion

    #region Public properties
    ///<summary>
    ///Gets whether the lock is currently held.
    ///</summary>
    public bool IsLocked => _semaphore.CurrentCount == 0;
    #endregion

    sealed class LockHandle(SemaphoreSlim semaphore) : IDisposable
    {
        #region Fields
        int _released;
        #endregion

        #region Public methods
        public void Dispose()
        {
            if (Interlocked.Exchange(ref _released, 1) == 0)
            {
                semaphore.Release();
            }
        }
        #endregion
    }
}
