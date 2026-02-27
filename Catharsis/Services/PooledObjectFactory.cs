using System.Collections.Concurrent;
using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Catharsis.Services;

/// <summary>
/// A factory that creates and pools reusable objects using a thread-safe object pool pattern,
/// integrated with DI and logging.
/// </summary>
/// <typeparam name="T">The type of objects to pool. Must have a parameterless constructor.</typeparam>
public sealed class PooledObjectFactory<T> : IDisposable where T : class, new()
{
    private readonly ConcurrentBag<T> _pool = [];
    private readonly ILogger<PooledObjectFactory<T>> _logger;
    private readonly int _maxPoolSize;
    private int _totalCreated;
    private int _totalReturned;
    private bool _disposed;

    /// <summary>
    /// Initializes a new <see cref="PooledObjectFactory{T}"/> with the specified logger and pool size.
    /// </summary>
    /// <param name="logger">The logger for diagnostic output.</param>
    /// <param name="maxPoolSize">The maximum number of objects to keep in the pool.</param>
    public PooledObjectFactory(ILogger<PooledObjectFactory<T>> logger, int maxPoolSize = 64)
    {
        Guard.IsNotNull(logger);
        Guard.IsGreaterThan(maxPoolSize, 0);

        _logger = logger;
        _maxPoolSize = maxPoolSize;
    }

    /// <summary>Gets the number of objects currently available in the pool.</summary>
    public int AvailableCount => _pool.Count;

    /// <summary>Gets the total number of objects created by this factory.</summary>
    public int TotalCreated => _totalCreated;

    /// <summary>Gets the total number of objects returned to the pool.</summary>
    public int TotalReturned => _totalReturned;

    /// <summary>
    /// Rents an object from the pool, or creates a new one if the pool is empty.
    /// </summary>
    /// <returns>A pooled or newly created object.</returns>
    public T Rent()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (_pool.TryTake(out T? item))
        {
            _logger.LogTrace("Rented pooled {TypeName} instance.", typeof(T).Name);
            return item;
        }

        Interlocked.Increment(ref _totalCreated);
        _logger.LogTrace("Created new {TypeName} instance (total: {Total}).", typeof(T).Name, _totalCreated);
        return new T();
    }

    /// <summary>
    /// Returns an object to the pool for reuse.
    /// </summary>
    /// <param name="item">The object to return.</param>
    public void Return(T item)
    {
        Guard.IsNotNull(item);
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (_pool.Count < _maxPoolSize)
        {
            _pool.Add(item);
            Interlocked.Increment(ref _totalReturned);
            _logger.LogTrace("Returned {TypeName} to pool.", typeof(T).Name);
        }
        else
        {
            _logger.LogTrace("Pool full; discarding {TypeName} instance.", typeof(T).Name);
            (item as IDisposable)?.Dispose();
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        while (_pool.TryTake(out T? item))
            (item as IDisposable)?.Dispose();

        _logger.LogDebug("PooledObjectFactory<{TypeName}> disposed. Created: {Created}, Returned: {Returned}.",
            typeof(T).Name, _totalCreated, _totalReturned);
    }
}
