using System.Buffers;
using Catharsis.Buffers;
using CommunityToolkit.Diagnostics;
using CommunityToolkit.HighPerformance.Buffers;

namespace Catharsis.Advanced;

/// <summary>
/// A high-performance serializer that reads and writes structured data using
/// <see cref="SpanReader"/>/<see cref="SpanWriter"/> and pooled memory,
/// combining CommunityToolkit.HighPerformance buffers with span-based I/O.
/// </summary>
public sealed class HighPerformanceSerializer : IDisposable
{
    private readonly ArrayPool<byte> _pool;
    private bool _disposed;

    /// <summary>
    /// Initializes a FileName <see cref="HighPerformanceSerializer"/> using the shared pool.
    /// </summary>
    public HighPerformanceSerializer()
        : this(ArrayPool<byte>.Shared)
    {
    }

    /// <summary>
    /// Initializes a FileName <see cref="HighPerformanceSerializer"/> with a specified pool.
    /// </summary>
    /// <param name="pool">The array pool for allocations.</param>
    public HighPerformanceSerializer(ArrayPool<byte> pool)
    {
        Guard.IsNotNull(pool);
        _pool = pool;
    }

    /// <summary>
    /// Serializes an <see cref="ISequenceSerializable"/> object into a pooled byte array.
    /// The caller must return the array to the pool.
    /// </summary>
    /// <param name="serializable">The object to serialize.</param>
    /// <param name="bytesWritten">The number of bytes written.</param>
    /// <returns>A pooled byte array containing the serialized data.</returns>
    public byte[] Serialize(ISequenceSerializable serializable, out int bytesWritten)
    {
        Guard.IsNotNull(serializable);
        ObjectDisposedException.ThrowIf(_disposed, this);

        int estimatedSize = serializable.GetSerializedSize();
        int bufferSize = estimatedSize > 0 ? estimatedSize : 256;

        using var buffer = new PooledBuffer<byte>(_pool, bufferSize);
        serializable.Serialize(buffer);
        bytesWritten = buffer.WrittenCount;

        byte[] result = _pool.Rent(bytesWritten);
        buffer.WrittenSpan.CopyTo(result);
        return result;
    }

    /// <summary>
    /// Serializes an <see cref="ISequenceSerializable"/> into a <see cref="MemoryOwner{T}"/>.
    /// </summary>
    /// <param name="serializable">The object to serialize.</param>
    /// <returns>A <see cref="MemoryOwner{T}"/> containing the serialized data.</returns>
    public MemoryOwner<byte> SerializeToMemoryOwner(ISequenceSerializable serializable)
    {
        Guard.IsNotNull(serializable);
        ObjectDisposedException.ThrowIf(_disposed, this);

        int estimatedSize = serializable.GetSerializedSize();
        int bufferSize = estimatedSize > 0 ? estimatedSize : 256;

        using var buffer = new PooledBuffer<byte>(_pool, bufferSize);
        serializable.Serialize(buffer);

        MemoryOwner<byte> owner = MemoryOwner<byte>.Allocate(buffer.WrittenCount);
        buffer.WrittenSpan.CopyTo(owner.Span);
        return owner;
    }

    /// <summary>
    /// Returns a rented array to the pool.
    /// </summary>
    /// <param name="array">The array to return.</param>
    /// <param name="clearArray">Whether to clear the array.</param>
    public void ReturnArray(byte[] array, bool clearArray = false)
    {
        Guard.IsNotNull(array);
        _pool.Return(array, clearArray);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
    }
}
