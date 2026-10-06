namespace Catharsis.Concurrency;

///<summary>
///Provides async-friendly mutual exclusion scoped to an individual key, so that callers holding different keys never
///block each other. A per-key semaphore is created on first use and removed automatically once no caller holds or
///awaits it.
///</summary>
///<typeparam name="TKey">The key type. Must support equality comparison.</typeparam>
///<param name="comparer">The equality comparer used to match keys, or <c>null</c> to use the default comparer.</param>
///<example>
///<code>
///KeyedAsyncLock&lt;string&gt; locks = new();
///
///using(await locks.LockAsync(userId))
///{
///    // Only one caller per distinct userId executes this block at a time.
///}
///</code>
///</example>
public sealed class KeyedAsyncLock<TKey>(IEqualityComparer<TKey>? comparer = null) where TKey : notnull
{
    #region Fields
    readonly Dictionary<TKey, Entry> _entries = new(comparer);
    readonly Lock _gate = new();
    #endregion

    #region Public methods
    ///<summary>
    ///Synchronously acquires the lock for the specified key. Dispose the returned handle to release it.
    ///</summary>
    ///<param name="key">The key identifying the lock to acquire.</param>
    ///<returns>A disposable handle that releases the lock when disposed.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="key"/> is <c>null</c>.</exception>
    public IDisposable Lock(TKey key)
    {
        Entry entry = Acquire(key);
        entry.Semaphore.Wait();
        return new ReleaseHandle(this, key, entry);
    }

    ///<summary>
    ///Asynchronously acquires the lock for the specified key. Dispose the returned handle to release it.
    ///</summary>
    ///<param name="key">The key identifying the lock to acquire.</param>
    ///<param name="cancellationToken">A cancellation token that can abandon the wait.</param>
    ///<returns>A disposable handle that releases the lock when disposed.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="key"/> is <c>null</c>.</exception>
    public async Task<IDisposable> LockAsync(TKey key, CancellationToken cancellationToken = default)
    {
        Entry entry = Acquire(key);

        try
        {
            await entry.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            Release(key, entry);
            throw;
        }

        return new ReleaseHandle(this, key, entry);
    }

    Entry Acquire(TKey key)
    {
        ArgumentNullException.ThrowIfNull(key);

        lock(_gate)
        {
            if(!_entries.TryGetValue(key, out Entry? entry))
            {
                entry = new Entry();
                _entries.Add(key, entry);
            }

            entry.RefCount++;
            return entry;
        }
    }

    void Release(TKey key, Entry entry)
    {
        lock(_gate)
        {
            entry.RefCount--;

            if(entry.RefCount == 0 && _entries.TryGetValue(key, out Entry? current) && current == entry)
            {
                _entries.Remove(key);
                entry.Semaphore.Dispose();
            }
        }
    }
    #endregion

    #region Public properties
    ///<summary>
    ///Gets the number of keys that currently have an active or waiting lock holder.
    ///</summary>
    public int ActiveKeyCount
    {
        get
        {
            lock(_gate)
            {
                return _entries.Count;
            }
        }
    }
    #endregion

    sealed class Entry
    {
        #region Public properties
        public SemaphoreSlim Semaphore { get; } = new(1, 1);

        public int RefCount { get; set; }
        #endregion
    }

    sealed class ReleaseHandle(KeyedAsyncLock<TKey> owner, TKey key, Entry entry) : IDisposable
    {
        #region Fields
        int _released;
        #endregion

        #region Public methods
        public void Dispose()
        {
            if(Interlocked.Exchange(ref _released, 1) == 0)
            {
                entry.Semaphore.Release();
                owner.Release(key, entry);
            }
        }
        #endregion
    }
}
