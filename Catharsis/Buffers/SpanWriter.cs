using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using CommunityToolkit.Diagnostics;

namespace Catharsis.Buffers;

///<summary>
///Provides sequential write operations over a <see cref="Span{T}"/> of bytes, tracking a cursor position for zero-
///allocation serialization.
///</summary>
public ref struct SpanWriter
{
    #region Struct fields
    readonly Span<byte> _span;
    int _position;
    #endregion

    #region Constructors
    ///<summary>
    ///Initializes a new <see cref="SpanWriter"/> over the specified span.
    ///</summary>
    ///<param name="span">The span to write to.</param>
    public SpanWriter(Span<byte> span)
    {
        _span = span;
        _position = 0;
    }
    #endregion

    #region Public methods
    ///<summary>
    ///Resets the writer to the beginning.
    ///</summary>
    public void Reset() { _position = 0; }

    ///<summary>
    ///Advances the writer position by the specified number of bytes (writing zeros).
    ///</summary>
    ///<param name="count">The number of bytes to skip.</param>
    public void Skip(int count)
    {
        Guard.IsGreaterThanOrEqualTo(count, 0);
        Guard.IsLessThanOrEqualTo(_position + count, _span.Length);
        _span.Slice(_position, count).Clear();
        _position += count;
    }

    ///<summary>
    ///Writes an unmanaged value of the specified type.
    ///</summary>
    ///<typeparam name="TValue">The unmanaged type to write.</typeparam>
    ///<param name="value">The value to write.</param>
    public void Write<TValue>(TValue value) where TValue : unmanaged
    {
        int size = Unsafe.SizeOf<TValue>();
        Guard.IsLessThanOrEqualTo(_position + size, _span.Length);
        MemoryMarshal.Write(_span[_position..], in value);
        _position += size;
    }

    ///<summary>
    ///Writes a single byte and advances the position.
    ///</summary>
    ///<param name="value">The byte to write.</param>
    public void WriteByte(byte value)
    {
        Guard.IsLessThan(_position, _span.Length);
        _span[_position++] = value;
    }

    ///<summary>
    ///Writes a span of bytes and advances the position.
    ///</summary>
    ///<param name="source">The bytes to write.</param>
    public void WriteBytes(ReadOnlySpan<byte> source)
    {
        Guard.IsLessThanOrEqualTo(_position + source.Length, _span.Length);
        source.CopyTo(_span[_position..]);
        _position += source.Length;
    }

    ///<summary>
    ///Writes a 64-bit floating-point value in little-endian format.
    ///</summary>
    ///<param name="value">The value to write.</param>
    public void WriteDoubleLittleEndian(double value)
    {
        BinaryPrimitives.WriteDoubleLittleEndian(_span[_position..], value);
        _position += sizeof(double);
    }

    ///<summary>
    ///Writes a 16-bit signed integer in little-endian format.
    ///</summary>
    ///<param name="value">The value to write.</param>
    public void WriteInt16LittleEndian(short value)
    {
        BinaryPrimitives.WriteInt16LittleEndian(_span[_position..], value);
        _position += sizeof(short);
    }

    ///<summary>
    ///Writes a 32-bit signed integer in little-endian format.
    ///</summary>
    ///<param name="value">The value to write.</param>
    public void WriteInt32LittleEndian(int value)
    {
        BinaryPrimitives.WriteInt32LittleEndian(_span[_position..], value);
        _position += sizeof(int);
    }

    ///<summary>
    ///Writes a 64-bit signed integer in little-endian format.
    ///</summary>
    ///<param name="value">The value to write.</param>
    public void WriteInt64LittleEndian(long value)
    {
        BinaryPrimitives.WriteInt64LittleEndian(_span[_position..], value);
        _position += sizeof(long);
    }

    ///<summary>
    ///Writes a 32-bit floating-point value in little-endian format.
    ///</summary>
    ///<param name="value">The value to write.</param>
    public void WriteSingleLittleEndian(float value)
    {
        BinaryPrimitives.WriteSingleLittleEndian(_span[_position..], value);
        _position += sizeof(float);
    }

    ///<summary>
    ///Writes a string as UTF-8 bytes prefixed by a 4-byte little-endian length.
    ///</summary>
    ///<param name="value">The string to write.</param>
    public void WriteUtf8String(ReadOnlySpan<char> value)
    {
        int byteCount = Encoding.UTF8.GetByteCount(value);
        WriteInt32LittleEndian(byteCount);
        Guard.IsLessThanOrEqualTo(_position + byteCount, _span.Length);
        Encoding.UTF8.GetBytes(value, _span[_position..]);
        _position += byteCount;
    }
    #endregion

    #region Public properties
    ///<summary>
    ///Gets a span over the remaining unwritten portion.
    ///</summary>
    public readonly Span<byte> FreeSpan => _span[_position..];

    ///<summary>
    ///Gets the total length of the underlying span.
    ///</summary>
    public readonly int Length => _span.Length;

    ///<summary>
    ///Gets the current write position.
    ///</summary>
    public readonly int Position => _position;

    ///<summary>
    ///Gets the number of bytes remaining to write.
    ///</summary>
    public readonly int Remaining => _span.Length - _position;
    #endregion
}
