using System.Buffers;
using System.Text;
using CommunityToolkit.Diagnostics;

namespace Catharsis.Buffers;

///<summary>
///A pooled alternative to <see cref="StringBuilder"/> that uses <see cref="ArrayPool{T}"/> to minimize allocations
///during string construction.
///</summary>
public sealed class PooledStringBuilder : IBufferWriter<char>, IDisposable
{
    #region Fields
    char[] _buffer;
    bool _disposed;
    readonly ArrayPool<char> _pool;
    int _position;
    #endregion

    #region Constructors
    ///<summary>
    ///Initializes a FileName <see cref="PooledStringBuilder"/> with the specified initial capacity.
    ///</summary>
    ///<param name="initialCapacity">The initial buffer capacity.</param>
    public PooledStringBuilder(int initialCapacity = 256) : this(ArrayPool<char>.Shared, initialCapacity)
    {
    }

    ///<summary>
    ///Initializes a FileName <see cref="PooledStringBuilder"/> with the specified pool and capacity.
    ///</summary>
    ///<param name="pool">The array pool to rent from.</param>
    ///<param name="initialCapacity">The initial buffer capacity.</param>
    public PooledStringBuilder(ArrayPool<char> pool, int initialCapacity = 256)
    {
        Guard.IsNotNull(pool);
        Guard.IsGreaterThan(initialCapacity, 0);

        _pool = pool;
        _buffer = _pool.Rent(initialCapacity);
    }
    #endregion

    #region Private methods
    void EnsureCapacity(int required)
    {
        if(required <= _buffer.Length)
        {
            return;
        }

        int newSize = Math.Max(_buffer.Length * 2, required);
        char[] newBuffer = _pool.Rent(newSize);
        _buffer.AsSpan(0, _position).CopyTo(newBuffer);
        _pool.Return(_buffer);
        _buffer = newBuffer;
    }
    #endregion

    #region Public methods
    ///<inheritdoc/>
    public void Advance(int count)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        Guard.IsGreaterThanOrEqualTo(count, 0);
        Guard.IsLessThanOrEqualTo(_position + count, _buffer.Length);
        _position += count;
    }

    ///<summary>
    ///Appends a single character to the builder.
    ///</summary>
    ///<param name="value">The character to append.</param>
    public void Append(char value)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        EnsureCapacity(_position + 1);
        _buffer[_position++] = value;
    }

    ///<summary>
    ///Appends a span of characters to the builder.
    ///</summary>
    ///<param name="value">The characters to append.</param>
    public void Append(ReadOnlySpan<char> value)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if(value.IsEmpty)
        {
            return;
        }

        EnsureCapacity(_position + value.Length);
        value.CopyTo(_buffer.AsSpan(_position));
        _position += value.Length;
    }

    ///<summary>
    ///Appends a string to the builder.
    ///</summary>
    ///<param name="value">The string to append.</param>
    public void Append(string? value)
    {
        if(value is not null)
        {
            Append(value.AsSpan());
        }
    }

    ///<summary>
    ///Appends the string representation of the specified value to the builder.
    ///</summary>
    ///<typeparam name="TValue">The type of the value.</typeparam>
    ///<param name="value">The value to append.</param>
    public void Append<TValue>(TValue value) where TValue : ISpanFormattable
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        int maxChars = Math.Max(256, _buffer.Length - _position);
        EnsureCapacity(_position + maxChars);

        if(value.TryFormat(_buffer.AsSpan(_position), out int charsWritten, default, null))
        {
            _position += charsWritten;
        } else
        {
            Append(value.ToString(null, null));
        }
    }

    ///<summary>
    ///Appends a line break to the builder.
    ///</summary>
    public void AppendLine() { Append(Environment.NewLine.AsSpan()); }

    ///<summary>
    ///Appends a string followed by a line break.
    ///</summary>
    ///<param name="value">The string to append.</param>
    public void AppendLine(string? value)
    {
        Append(value);
        AppendLine();
    }

    ///<summary>
    ///Clears all written characters without releasing the buffer.
    ///</summary>
    public void Clear() { _position = 0; }

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
        _position = 0;
    }

    ///<inheritdoc/>
    public Memory<char> GetMemory(int sizeHint = 0)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        EnsureCapacity(_position + Math.Max(sizeHint, 1));
        return _buffer.AsMemory(_position);
    }

    ///<inheritdoc/>
    public Span<char> GetSpan(int sizeHint = 0)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        EnsureCapacity(_position + Math.Max(sizeHint, 1));
        return _buffer.AsSpan(_position);
    }

    ///<summary>
    ///Returns the accumulated string and resets the builder.
    ///</summary>
    ///<returns>The built string.</returns>
    public override string ToString() { return new string(_buffer, 0, _position); }

    ///<summary>
    ///Returns the accumulated string and disposes the builder.
    ///</summary>
    ///<returns>The built string.</returns>
    public string ToStringAndDispose()
    {
        string result = ToString();
        Dispose();
        return result;
    }
    #endregion

    #region Public properties
    ///<summary>
    ///Gets the current capacity of the internal buffer.
    ///</summary>
    public int Capacity => _buffer.Length;

    ///<summary>
    ///Gets the number of characters written.
    ///</summary>
    public int Length => _position;

    ///<summary>
    ///Gets a <see cref="ReadOnlySpan{T}"/> over the written characters.
    ///</summary>
    public ReadOnlySpan<char> WrittenSpan => _buffer.AsSpan(0, _position);
    #endregion
}
