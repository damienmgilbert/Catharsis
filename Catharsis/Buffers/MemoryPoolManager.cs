using System.Buffers;
using CommunityToolkit.Diagnostics;

namespace Catharsis.Buffers;

/// <summary>
/// Manages pooled memory allocations using <see cref="MemoryPool{T}"/> and <see cref="ArrayPool{T}"/>,
/// providing a centralized point for renting and returning buffers.
/// </summary>
public sealed class MemoryPoolManager : IDisposable
{
    private readonly ArrayPool<byte> _arrayPool;
    private readonly MemoryPool<byte> _memoryPool;
    private int _activeRentals;
    private bool _disposed;

    /// <summary>
    /// Initializes a FileName <see cref="MemoryPoolManager"/> with the shared pools.
    /// </summary>
    public MemoryPoolManager()
        : this(ArrayPool<byte>.Shared, MemoryPool<byte>.Shared)
    {
    }

    /// <summary>
    /// Initializes a FileName <see cref="MemoryPoolManager"/> with the specified pools.
    /// </summary>
    /// <param name="arrayPool">The array pool to use for renting arrays.</param>
    /// <param name="memoryPool">The memory pool to use for renting memory blocks.</param>
    public MemoryPoolManager(ArrayPool<byte> arrayPool, MemoryPool<byte> memoryPool)
    {
        Guard.IsNotNull(arrayPool);
        Guard.IsNotNull(memoryPool);

        _arrayPool = arrayPool;
        _memoryPool = memoryPool;
    }

    /// <summary>Gets the number of currently active (unreturned) rentals.</summary>
    public int ActiveRentals => _activeRentals;

    /// <summary>
    /// Rents a byte array of at least the specified minimum length.
    /// </summary>
    /// <param name="minimumLength">The minimum required length of the array.</param>
    /// <returns>A pooled byte array.</returns>
    public byte[] RentArray(int minimumLength)
    {
        Guard.IsGreaterThanOrEqualTo(minimumLength, 0);
        ObjectDisposedException.ThrowIf(_disposed, this);

        Interlocked.Increment(ref _activeRentals);
        return _arrayPool.Rent(minimumLength);
    }

    /// <summary>
    /// Returns a previously rented byte array to the pool.
    /// </summary>
    /// <param name="array">The array to return.</param>
    /// <param name="clearArray">Whether to clear the array before returning it.</param>
    public void ReturnArray(byte[] array, bool clearArray = false)
    {
        Guard.IsNotNull(array);
        ObjectDisposedException.ThrowIf(_disposed, this);

        _arrayPool.Return(array, clearArray);
        Interlocked.Decrement(ref _activeRentals);
    }

    /// <summary>
    /// Rents an <see cref="IMemoryOwner{T}"/> block of at least the specified minimum length.
    /// </summary>
    /// <param name="minimumLength">The minimum required length. Use -1 for default.</param>
    /// <returns>An <see cref="IMemoryOwner{T}"/> that must be disposed to return memory to the pool.</returns>
    public IMemoryOwner<byte> RentMemory(int minimumLength = -1)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        Interlocked.Increment(ref _activeRentals);
        IMemoryOwner<byte> owner = _memoryPool.Rent(minimumLength);
        return new TrackedMemoryOwner(this, owner);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
    }

    private sealed class TrackedMemoryOwner(MemoryPoolManager manager, IMemoryOwner<byte> inner) : IMemoryOwner<byte>
    {
        private bool _disposed;

        public Memory<byte> Memory => _disposed
            ? throw new ObjectDisposedException(nameof(TrackedMemoryOwner))
            : inner.Memory;

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            inner.Dispose();
            Interlocked.Decrement(ref manager._activeRentals);
        }
    }
}
