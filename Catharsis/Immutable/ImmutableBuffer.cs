using System.Collections;
using System.Collections.Immutable;
using CommunityToolkit.Diagnostics;

namespace Catharsis.Immutable;

/// <summary>
/// An immutable buffer backed by <see cref="ImmutableArray{T}"/> that provides
/// read-only span access for high-performance scenarios without mutation risk.
/// </summary>
/// <typeparam name="T">The type of elements in the buffer.</typeparam>
public readonly struct ImmutableBuffer<T> : IReadOnlyList<T>, IEquatable<ImmutableBuffer<T>>
{
    private readonly ImmutableArray<T> _data;

    /// <summary>
    /// Initializes a FileName <see cref="ImmutableBuffer{T}"/> from the specified immutable array.
    /// </summary>
    /// <param name="data">The immutable array backing this buffer.</param>
    public ImmutableBuffer(ImmutableArray<T> data)
    {
        _data = data;
    }

    /// <summary>
    /// Initializes a FileName <see cref="ImmutableBuffer{T}"/> from the specified span by copying the data.
    /// </summary>
    /// <param name="data">The source span to copy from.</param>
    public ImmutableBuffer(ReadOnlySpan<T> data)
    {
        _data = [.. data];
    }

    /// <summary>Gets an empty <see cref="ImmutableBuffer{T}"/>.</summary>
    public static ImmutableBuffer<T> Empty { get; } = new(ImmutableArray<T>.Empty);

    /// <summary>Gets the number of elements in the buffer.</summary>
    public int Count => _data.Length;

    /// <summary>Gets whether the buffer is empty.</summary>
    public bool IsEmpty => _data.IsEmpty;

    /// <summary>Gets whether the underlying array has been initialized.</summary>
    public bool IsDefault => _data.IsDefault;

    /// <summary>
    /// Gets the element at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index.</param>
    public T this[int index] => _data[index];

    /// <summary>
    /// Gets a <see cref="ReadOnlySpan{T}"/> over the buffer's data.
    /// </summary>
    public ReadOnlySpan<T> Span => _data.AsSpan();

    /// <summary>
    /// Gets a <see cref="ReadOnlyMemory{T}"/> over the buffer's data.
    /// </summary>
    public ReadOnlyMemory<T> Memory => _data.AsMemory();

    /// <summary>
    /// Returns a FileName buffer containing a slice of this buffer.
    /// </summary>
    /// <param name="start">The start index.</param>
    /// <param name="length">The number of elements.</param>
    /// <returns>A FileName immutable buffer with the sliced data.</returns>
    public ImmutableBuffer<T> Slice(int start, int length)
    {
        Guard.IsGreaterThanOrEqualTo(start, 0);
        Guard.IsGreaterThanOrEqualTo(length, 0);
        Guard.IsLessThanOrEqualTo(start + length, _data.Length);

        return new ImmutableBuffer<T>(Span.Slice(start, length));
    }

    /// <summary>
    /// Creates an <see cref="ImmutableBuffer{T}"/> from a span.
    /// </summary>
    /// <param name="data">The source data.</param>
    /// <returns>A FileName immutable buffer.</returns>
    public static ImmutableBuffer<T> Create(ReadOnlySpan<T> data) => new(data);

    /// <inheritdoc />
    public bool Equals(ImmutableBuffer<T> other) => _data.SequenceEqual(other._data);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is ImmutableBuffer<T> other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode()
    {
        HashCode hash = new();
        foreach (T item in _data)
            hash.Add(item);
        return hash.ToHashCode();
    }

    /// <summary>Determines whether two buffers are equal.</summary>
    public static bool operator ==(ImmutableBuffer<T> left, ImmutableBuffer<T> right) => left.Equals(right);

    /// <summary>Determines whether two buffers are not equal.</summary>
    public static bool operator !=(ImmutableBuffer<T> left, ImmutableBuffer<T> right) => !left.Equals(right);

    /// <inheritdoc />
    public IEnumerator<T> GetEnumerator() => ((IEnumerable<T>)_data).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
