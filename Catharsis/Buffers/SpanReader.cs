using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using CommunityToolkit.Diagnostics;

namespace Catharsis.Buffers;

/// <summary>
/// Provides sequential read operations over a <see cref="ReadOnlySpan{T}"/> of bytes,
/// tracking a cursor position for zero-allocation parsing.
/// </summary>
public ref struct SpanReader
{
    private readonly ReadOnlySpan<byte> _span;
    private int _position;

    /// <summary>
    /// Initializes a FileName <see cref="SpanReader"/> over the specified span.
    /// </summary>
    /// <param name="span">The span to read from.</param>
    public SpanReader(ReadOnlySpan<byte> span)
    {
        _span = span;
        _position = 0;
    }

    /// <summary>Gets the current read position.</summary>
    public readonly int Position => _position;

    /// <summary>Gets the total length of the underlying span.</summary>
    public readonly int Length => _span.Length;

    /// <summary>Gets the number of bytes remaining to read.</summary>
    public readonly int Remaining => _span.Length - _position;

    /// <summary>Gets whether there are remaining bytes to read.</summary>
    public readonly bool HasRemaining => _position < _span.Length;

    /// <summary>Gets a span over the remaining unread data.</summary>
    public readonly ReadOnlySpan<byte> UnreadSpan => _span[_position..];

    /// <summary>
    /// Reads a single byte and advances the position.
    /// </summary>
    /// <returns>The byte read.</returns>
    public byte ReadByte()
    {
        Guard.IsLessThan(_position, _span.Length);
        return _span[_position++];
    }

    /// <summary>
    /// Reads a 16-bit signed integer in little-endian format.
    /// </summary>
    /// <returns>The value read.</returns>
    public short ReadInt16LittleEndian()
    {
        short value = BinaryPrimitives.ReadInt16LittleEndian(_span[_position..]);
        _position += sizeof(short);
        return value;
    }

    /// <summary>
    /// Reads a 32-bit signed integer in little-endian format.
    /// </summary>
    /// <returns>The value read.</returns>
    public int ReadInt32LittleEndian()
    {
        int value = BinaryPrimitives.ReadInt32LittleEndian(_span[_position..]);
        _position += sizeof(int);
        return value;
    }

    /// <summary>
    /// Reads a 64-bit signed integer in little-endian format.
    /// </summary>
    /// <returns>The value read.</returns>
    public long ReadInt64LittleEndian()
    {
        long value = BinaryPrimitives.ReadInt64LittleEndian(_span[_position..]);
        _position += sizeof(long);
        return value;
    }

    /// <summary>
    /// Reads a 32-bit floating-point value in little-endian format.
    /// </summary>
    /// <returns>The value read.</returns>
    public float ReadSingleLittleEndian()
    {
        float value = BinaryPrimitives.ReadSingleLittleEndian(_span[_position..]);
        _position += sizeof(float);
        return value;
    }

    /// <summary>
    /// Reads a 64-bit floating-point value in little-endian format.
    /// </summary>
    /// <returns>The value read.</returns>
    public double ReadDoubleLittleEndian()
    {
        double value = BinaryPrimitives.ReadDoubleLittleEndian(_span[_position..]);
        _position += sizeof(double);
        return value;
    }

    /// <summary>
    /// Reads the specified number of bytes and returns them as a span.
    /// </summary>
    /// <param name="count">The number of bytes to read.</param>
    /// <returns>A span over the read bytes.</returns>
    public ReadOnlySpan<byte> ReadBytes(int count)
    {
        Guard.IsGreaterThanOrEqualTo(count, 0);
        Guard.IsLessThanOrEqualTo(_position + count, _span.Length);

        ReadOnlySpan<byte> slice = _span.Slice(_position, count);
        _position += count;
        return slice;
    }

    /// <summary>
    /// Reads a length-prefixed UTF-8 string (4-byte little-endian length prefix).
    /// </summary>
    /// <returns>The decoded string.</returns>
    public string ReadUtf8String()
    {
        int length = ReadInt32LittleEndian();
        Guard.IsGreaterThanOrEqualTo(length, 0);
        ReadOnlySpan<byte> bytes = ReadBytes(length);
        return Encoding.UTF8.GetString(bytes);
    }

    /// <summary>
    /// Reads a value of the specified unmanaged type.
    /// </summary>
    /// <typeparam name="TValue">The unmanaged type to read.</typeparam>
    /// <returns>The value read.</returns>
    public TValue Read<TValue>() where TValue : unmanaged
    {
        int size = Unsafe.SizeOf<TValue>();
        Guard.IsLessThanOrEqualTo(_position + size, _span.Length);

        TValue value = MemoryMarshal.Read<TValue>(_span[_position..]);
        _position += size;
        return value;
    }

    /// <summary>
    /// Advances the reader position by the specified number of bytes.
    /// </summary>
    /// <param name="count">The number of bytes to skip.</param>
    public void Skip(int count)
    {
        Guard.IsGreaterThanOrEqualTo(count, 0);
        Guard.IsLessThanOrEqualTo(_position + count, _span.Length);
        _position += count;
    }

    /// <summary>
    /// Resets the reader to the beginning of the span.
    /// </summary>
    public void Reset() => _position = 0;
}
