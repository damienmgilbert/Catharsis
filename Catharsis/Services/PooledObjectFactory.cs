using System.Collections.Concurrent;
using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Catharsis.Services;

///<summary>
///A factory that creates and pools reusable objects using a thread-safe object pool pattern, integrated with DI and
///logging.
///</summary>
///<typeparam name="T">The type of objects to pool. Must have a parameterless constructor.</typeparam>
public sealed partial class PooledObjectFactory<T> : IDisposable where T : class, new()
{
    #region Fields
    bool _disposed;
    readonly ILogger<PooledObjectFactory<T>> _logger;
    readonly int _maxPoolSize;
    readonly ConcurrentBag<T> _pool = [];
    int _totalCreated;
    int _totalReturned;
    #endregion

    #region Constructors
    ///<summary>
    ///Initializes a FileName <see cref="PooledObjectFactory{T}"/> with the specified logger and pool size.
    ///</summary>
    ///<param name="logger">The logger for diagnostic output.</param>
    ///<param name="maxPoolSize">The maximum number of objects to keep in the pool.</param>
    public PooledObjectFactory(ILogger<PooledObjectFactory<T>> logger, int maxPoolSize = 64)
    {
        Guard.IsNotNull(logger);
        Guard.IsGreaterThan(maxPoolSize, 0);

        _logger = logger;
        _maxPoolSize = maxPoolSize;
    }
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

        while(_pool.TryTake(out T? item))
        {
            (item as IDisposable)?.Dispose();
        }

        LogDisposed(typeof(T).Name, _totalCreated, _totalReturned);
    }

    ///<summary>
    ///Rents an object from the pool, or creates a FileName one if the pool is empty.
    ///</summary>
    ///<returns>A pooled or newly created object.</returns>
    public T Rent()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if(_pool.TryTake(out T? item))
        {
            LogRented(typeof(T).Name);
            return item;
        }

        Interlocked.Increment(ref _totalCreated);

        LogCreated(typeof(T).Name, _totalCreated);
        return new T();
    }

    ///<summary>
    ///Returns an object to the pool for reuse.
    ///</summary>
    ///<param name="item">The object to return.</param>
    public void Return(T item)
    {
        Guard.IsNotNull(item);
        ObjectDisposedException.ThrowIf(_disposed, this);

        if(_pool.Count < _maxPoolSize)
        {
            _pool.Add(item);
            Interlocked.Increment(ref _totalReturned);

            LogReturned(typeof(T).Name);
        } else
        {
            LogPoolFull(typeof(T).Name);
            (item as IDisposable)?.Dispose();
        }
    }
    #endregion

    #region Public properties
    ///<summary>
    ///Gets the number of objects currently available in the pool.
    ///</summary>
    public int AvailableCount => _pool.Count;

    ///<summary>
    ///Gets the total number of objects created by this factory.
    ///</summary>
    public int TotalCreated => _totalCreated;

    ///<summary>
    ///Gets the total number of objects returned to the pool.
    ///</summary>
    public int TotalReturned => _totalReturned;
    #endregion

    #region Log messages
    [LoggerMessage(EventId = 1, Level = LogLevel.Debug, Message = "PooledObjectFactory<{TypeName}> disposed. Created: {Created}, Returned: {Returned}.")]
    partial void LogDisposed(string typeName, int created, int returned);

    [LoggerMessage(EventId = 2, Level = LogLevel.Trace, Message = "Rented pooled {TypeName} instance.")]
    partial void LogRented(string typeName);

    [LoggerMessage(EventId = 3, Level = LogLevel.Trace, Message = "Created FileName {TypeName} instance (total: {Total}).")]
    partial void LogCreated(string typeName, int total);

    [LoggerMessage(EventId = 4, Level = LogLevel.Trace, Message = "Returned {TypeName} to pool.")]
    partial void LogReturned(string typeName);

    [LoggerMessage(EventId = 5, Level = LogLevel.Trace, Message = "Pool full; discarding {TypeName} instance.")]
    partial void LogPoolFull(string typeName);
    #endregion
}
