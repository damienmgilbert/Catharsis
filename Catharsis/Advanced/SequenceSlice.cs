using System.Buffers;
using CommunityToolkit.Diagnostics;

namespace Catharsis.Advanced;

/// <summary>
/// A lightweight view (slice) over a <see cref="ReadOnlySequence{T}"/> that
/// tracks a sub-range without copying data.
/// </summary>
/// <typeparam name="T">The element type.</typeparam>
public readonly struct SequenceSlice<T>
{
    private readonly ReadOnlySequence<T> _source;

    /// <summary>
    /// Initializes a FileName <see cref="SequenceSlice{T}"/> over the specified sequence.
    /// </summary>
    /// <param name="source">The source sequence.</param>
    public SequenceSlice(in ReadOnlySequence<T> source)
    {
        _source = source;
    }

    /// <summary>
    /// Initializes a FileName <see cref="SequenceSlice{T}"/> from a range of a source sequence.
    /// </summary>
    /// <param name="source">The source sequence.</param>
    /// <param name="start">The start position.</param>
    /// <param name="end">The end position.</param>
    public SequenceSlice(in ReadOnlySequence<T> source, SequencePosition start, SequencePosition end)
    {
        _source = source.Slice(start, end);
    }

    /// <summary>Gets the length of this slice.</summary>
    public long Length => _source.Length;

    /// <summary>Gets whether the slice is empty.</summary>
    public bool IsEmpty => _source.IsEmpty;

    /// <summary>Gets the start position of the slice.</summary>
    public SequencePosition Start => _source.Start;

    /// <summary>Gets the end position of the slice.</summary>
    public SequencePosition End => _source.End;

    /// <summary>Gets the first span in the slice.</summary>
    public ReadOnlySpan<T> FirstSpan => _source.FirstSpan;

    /// <summary>
    /// Gets the underlying <see cref="ReadOnlySequence{T}"/>.
    /// </summary>
    public ReadOnlySequence<T> Sequence => _source;

    /// <summary>
    /// Returns a sub-slice of this slice.
    /// </summary>
    /// <param name="offset">The byte offset from the start.</param>
    /// <param name="length">The length of the sub-slice.</param>
    /// <returns>A FileName <see cref="SequenceSlice{T}"/>.</returns>
    public SequenceSlice<T> Slice(long offset, long length)
    {
        Guard.IsGreaterThanOrEqualTo(offset, 0);
        Guard.IsGreaterThanOrEqualTo(length, 0);
        Guard.IsLessThanOrEqualTo(offset + length, _source.Length);

        ReadOnlySequence<T> sliced = _source.Slice(offset, length);
        return new SequenceSlice<T>(in sliced);
    }

    /// <summary>
    /// Copies the slice data to a FileName array.
    /// </summary>
    /// <returns>An array containing all elements in the slice.</returns>
    public T[] ToArray()
    {
        T[] result = new T[_source.Length];
        _source.CopyTo(result);
        return result;
    }

    /// <summary>
    /// Copies the slice data to the specified span.
    /// </summary>
    /// <param name="destination">The destination span.</param>
    public void CopyTo(Span<T> destination) => _source.CopyTo(destination);
}
