using CommunityToolkit.Diagnostics;
using System.Buffers;

namespace Catharsis.Buffers;

///<summary>
///Manages pooled memory allocations using <see cref="MemoryPool{T}"/> and <see cref="ArrayPool{T}"/>, providing a
///centralized point for renting and returning buffers.
///</summary>
public sealed class MemoryPoolManager : IDisposable
{
    #region Fields
    private int _activeRentals;
    private readonly ArrayPool<byte> _arrayPool;
    private bool _disposed;
    private readonly MemoryPool<byte> _memoryPool;
    #endregion

    #region Constructors
    ///<summary>
    ///Initializes a new <see cref="MemoryPoolManager"/> with the shared pools.
    ///</summary>
    public MemoryPoolManager() : this(ArrayPool<byte>.Shared, MemoryPool<byte>.Shared)
    {
    }

    ///<summary>
    ///Initializes a new <see cref="MemoryPoolManager"/> with the specified pools.
    ///</summary>
    ///<param name="arrayPool">The array pool to use for renting arrays.</param>
    ///<param name="memoryPool">The memory pool to use for renting memory blocks.</param>
    public MemoryPoolManager(ArrayPool<byte> arrayPool, MemoryPool<byte> memoryPool)
    {
        Guard.IsNotNull(arrayPool);
        Guard.IsNotNull(memoryPool);

        _arrayPool = arrayPool;
        _memoryPool = memoryPool;
    }
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
    }

    ///<summary>
    ///Rents a byte array of at least the specified minimum length.
    ///</summary>
    ///<param name="minimumLength">The minimum required length of the array.</param>
    ///<returns>A pooled byte array.</returns>
    public byte[] RentArray(int minimumLength)
    {
        Guard.IsGreaterThanOrEqualTo(minimumLength, 0);
        ObjectDisposedException.ThrowIf(_disposed, this);

        Interlocked.Increment(ref _activeRentals);
        return _arrayPool.Rent(minimumLength);
    }

    ///<summary>
    ///Rents an <see cref="IMemoryOwner{T}"/> block of at least the specified minimum length.
    ///</summary>
    ///<param name="minimumLength">The minimum required length. Use -1 for default.</param>
    ///<returns>An <see cref="IMemoryOwner{T}"/> that must be disposed to return memory to the pool.</returns>
    public IMemoryOwner<byte> RentMemory(int minimumLength = -1)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        Interlocked.Increment(ref _activeRentals);
        IMemoryOwner<byte> owner = _memoryPool.Rent(minimumLength);
        return new TrackedMemoryOwner(this, owner);
    }

    ///<summary>
    ///Returns a previously rented byte array to the pool.
    ///</summary>
    ///<param name="array">The array to return.</param>
    ///<param name="clearArray">Whether to clear the array before returning it.</param>
    public void ReturnArray(byte[] array, bool clearArray = false)
    {
        Guard.IsNotNull(array);
        ObjectDisposedException.ThrowIf(_disposed, this);

        _arrayPool.Return(array, clearArray);
        Interlocked.Decrement(ref _activeRentals);
    }
    #endregion

    #region Public properties
    ///<summary>
    ///Gets the number of currently active (unreturned) rentals.
    ///</summary>
    public int ActiveRentals => _activeRentals;
    #endregion

    private sealed class TrackedMemoryOwner(MemoryPoolManager manager, IMemoryOwner<byte> inner) : IMemoryOwner<byte>
    {
        #region Fields
        private bool _disposed;
        #endregion

        #region Public methods
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            inner.Dispose();
            Interlocked.Decrement(ref manager._activeRentals);
        }
        #endregion

        #region Public properties
        public Memory<byte> Memory => _disposed ? throw new ObjectDisposedException(nameof(TrackedMemoryOwner)) : inner.Memory;
        #endregion
    }
}
