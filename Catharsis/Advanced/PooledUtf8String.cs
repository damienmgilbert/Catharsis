using System.Buffers;
using System.Text;
using CommunityToolkit.Diagnostics;

namespace Catharsis.Advanced;

///<summary>
///A pooled UTF-8 string representation backed by <see cref="ArrayPool{T}"/>, enabling zero-copy UTF-8 operations and
///minimal allocation for string handling.
///</summary>
public sealed class PooledUtf8String : IDisposable, IEquatable<PooledUtf8String>
{
    #region Fields
    byte[] _buffer;
    bool _disposed;
    readonly int _length;
    readonly ArrayPool<byte> _pool;
    #endregion

    #region Constructors
    PooledUtf8String(byte[] buffer, int length, ArrayPool<byte> pool)
    {
        _buffer = buffer;
        _length = length;
        _pool = pool;
    }
    #endregion

    #region Operators
    ///<summary>
    ///Determines whether two pooled UTF-8 strings are not equal.
    ///</summary>
    public static bool operator !=(PooledUtf8String? left, PooledUtf8String? right)
    {
        return !(left == right);
    }

    ///<summary>
    ///Determines whether two pooled UTF-8 strings are equal.
    ///</summary>
    public static bool operator ==(PooledUtf8String? left, PooledUtf8String? right)
    {
        return (left is null) ? (right is null) : left.Equals(right);
    }
    #endregion

    #region Public methods
    ///<summary>
    ///Creates a <see cref="PooledUtf8String"/> from a .NET string.
    ///</summary>
    ///<param name="value">The source string.</param>
    ///<returns>A pooled UTF-8 string.</returns>
    public static PooledUtf8String Create(string value)
    {
        Guard.IsNotNull(value);
        return Create(value.AsSpan());
    }

    ///<summary>
    ///Creates a <see cref="PooledUtf8String"/> from a character span.
    ///</summary>
    ///<param name="chars">The source characters.</param>
    ///<returns>A pooled UTF-8 string.</returns>
    public static PooledUtf8String Create(ReadOnlySpan<char> chars)
    {
        ArrayPool<byte> pool = ArrayPool<byte>.Shared;
        int byteCount = Encoding.UTF8.GetByteCount(chars);
        byte[] buffer = pool.Rent(byteCount);
        int written = Encoding.UTF8.GetBytes(chars, buffer);
        return new PooledUtf8String(buffer, written, pool);
    }

    ///<inheritdoc/>
    public void Dispose()
    {
        if(_disposed)
        {
            return;
        }

        _disposed = true;
        _pool.Return(_buffer);
        _buffer = [];
    }

    ///<inheritdoc/>
    public bool Equals(PooledUtf8String? other)
    {
        if(other is null)
        {
            return false;
        }

        return Span.SequenceEqual(other.Span);
    }

    ///<inheritdoc/>
    public override bool Equals(object? obj) { return (obj is PooledUtf8String other) && Equals(other); }

    ///<summary>
    ///Creates a <see cref="PooledUtf8String"/> from existing UTF-8 bytes.
    ///</summary>
    ///<param name="utf8Bytes">The source UTF-8 bytes.</param>
    ///<returns>A pooled UTF-8 string.</returns>
    public static PooledUtf8String FromUtf8(ReadOnlySpan<byte> utf8Bytes)
    {
        ArrayPool<byte> pool = ArrayPool<byte>.Shared;
        byte[] buffer = pool.Rent(utf8Bytes.Length);
        utf8Bytes.CopyTo(buffer);
        return new PooledUtf8String(buffer, utf8Bytes.Length, pool);
    }

    ///<inheritdoc/>
    public override int GetHashCode()
    {
        HashCode hash = new();
        hash.AddBytes(Span);
        return hash.ToHashCode();
    }

    ///<summary>
    ///Decodes this UTF-8 string to a .NET string.
    ///</summary>
    ///<returns>The decoded string.</returns>
    public override string ToString()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return Encoding.UTF8.GetString(_buffer, 0, _length);
    }
    #endregion

    #region Public properties
    ///<summary>
    ///Gets the length of the UTF-8 string in bytes.
    ///</summary>
    public int ByteLength => _length;

    ///<summary>
    ///Gets a memory region over the UTF-8 bytes.
    ///</summary>
    public ReadOnlyMemory<byte> Memory
    {
        get
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            return _buffer.AsMemory(0, _length);
        }
    }

    ///<summary>
    ///Gets a span over the UTF-8 bytes.
    ///</summary>
    public ReadOnlySpan<byte> Span
    {
        get
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            return _buffer.AsSpan(0, _length);
        }
    }
    #endregion
}
