using System.Buffers;
using System.Collections;
using System.Collections.Immutable;
using CommunityToolkit.Diagnostics;

namespace Catharsis.Immutable;

///<summary>
///An immutable sequence that wraps a <see cref="ReadOnlySequence{T}"/> as a persistent data structure backed by <see
///cref="ImmutableArray{T}"/> segments.
///</summary>
///<typeparam name="T">The element type.</typeparam>
///<remarks>
///Initializes a new <see cref="ImmutableSequence{T}"/> from the specified data.
///</remarks>
///<param name="data">The immutable array backing this sequence.</param>
public readonly struct ImmutableSequence<T>(ImmutableArray<T> data) : IReadOnlyList<T>, IEquatable<ImmutableSequence<T>>
{
    #region Struct fields
    readonly ImmutableArray<T> _data = data;

    #endregion
    #region Constructors
    #endregion

    #region Operators
    ///<summary>
    ///Determines whether two sequences are not equal.
    ///</summary>
    public static bool operator !=(ImmutableSequence<T> left, ImmutableSequence<T> right)
    {
        return !left.Equals(right);
    }

    ///<summary>
    ///Determines whether two sequences are equal.
    ///</summary>
    public static bool operator ==(ImmutableSequence<T> left, ImmutableSequence<T> right)
    {
        return left.Equals(right);
    }
    #endregion

    #region Indexers
    ///<summary>
    ///Gets the element at the specified index.
    ///</summary>
    ///<param name="index">The zero-based index.</param>
    public T this[int index] => _data[index];
    #endregion

    #region Explicit interface implementations
    IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
    #endregion

    #region Public methods
    ///<summary>
    ///Returns a new sequence with the specified element appended.
    ///</summary>
    ///<param name="item">The item to append.</param>
    ///<returns>A new immutable sequence with the item appended.</returns>
    public ImmutableSequence<T> Add(T item) { return new(_data.Add(item)); }

    ///<summary>
    ///Creates an <see cref="ImmutableSequence{T}"/> from a span.
    ///</summary>
    ///<param name="data">The source data.</param>
    ///<returns>A new immutable sequence.</returns>
    public static ImmutableSequence<T> Create(ReadOnlySpan<T> data)
    {
        if (data.IsEmpty)
        {
            return Empty;
        }

        return new ImmutableSequence<T>([.. data]);
    }

    ///<summary>
    ///Creates an <see cref="ImmutableSequence{T}"/> from a <see cref="ReadOnlySequence{T}"/> by copying the data into
    ///an immutable array.
    ///</summary>
    ///<param name="sequence">The source sequence.</param>
    ///<returns>A new immutable sequence.</returns>
    public static ImmutableSequence<T> CreateFrom(in ReadOnlySequence<T> sequence)
    {
        if (sequence.IsEmpty)
        {
            return Empty;
        }

        ImmutableArray<T>.Builder builder = ImmutableArray.CreateBuilder<T>((int)sequence.Length);

        foreach (ReadOnlyMemory<T> segment in sequence)
        {
            ReadOnlySpan<T> span = segment.Span;
            for (int i = 0; i < span.Length; i++)
            {
                builder.Add(span[i]);
            }
        }

        return new ImmutableSequence<T>(builder.ToImmutable());
    }

    ///<inheritdoc/>
    public bool Equals(ImmutableSequence<T> other) { return _data.SequenceEqual(other._data); }
    ///<inheritdoc/>
    public override bool Equals(object? obj) { return (obj is ImmutableSequence<T> other) && Equals(other); }
    ///<inheritdoc/>
    public IEnumerator<T> GetEnumerator() { return ((IEnumerable<T>)_data).GetEnumerator(); }

    ///<inheritdoc/>
    public override int GetHashCode()
    {
        HashCode hash = new();
        foreach (T item in _data)
        {
            hash.Add(item);
        }

        return hash.ToHashCode();
    }

    ///<summary>
    ///Returns a new sequence containing a slice of this sequence.
    ///</summary>
    ///<param name="start">The start index.</param>
    ///<param name="length">The number of elements.</param>
    ///<returns>A new immutable sequence.</returns>
    public ImmutableSequence<T> Slice(int start, int length)
    {
        Guard.IsGreaterThanOrEqualTo(start, 0);
        Guard.IsGreaterThanOrEqualTo(length, 0);
        Guard.IsLessThanOrEqualTo(start + length, _data.Length);

        return Create(Span.Slice(start, length));
    }
    #endregion

    #region Public properties
    ///<summary>
    ///Gets the number of elements in the sequence.
    ///</summary>
    public int Count => _data.Length;

    ///<summary>
    ///Gets an empty <see cref="ImmutableSequence{T}"/>.
    ///</summary>
    public static ImmutableSequence<T> Empty { get; } = new([]);

    ///<summary>
    ///Gets whether the sequence is empty.
    ///</summary>
    public bool IsEmpty => _data.IsEmpty;

    ///<summary>
    ///Gets a <see cref="ReadOnlySpan{T}"/> over the sequence data.
    ///</summary>
    public ReadOnlySpan<T> Span => _data.AsSpan();
    #endregion
}
