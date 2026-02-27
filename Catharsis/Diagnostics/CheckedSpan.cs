using CommunityToolkit.Diagnostics;

namespace Catharsis.Diagnostics;

/// <summary>
/// A bounds-checked wrapper around a <see cref="Span{T}"/> that uses CommunityToolkit
/// <see cref="Guard"/> for all index and range validation.
/// </summary>
/// <typeparam name="T">The element type.</typeparam>
public ref struct CheckedSpan<T>
{
    private readonly Span<T> _span;

    /// <summary>
    /// Initializes a new <see cref="CheckedSpan{T}"/> over the specified span.
    /// </summary>
    /// <param name="span">The span to wrap with bounds checks.</param>
    /// <param name="mode">The validation mode to apply.</param>
    public CheckedSpan(Span<T> span, ValidationMode mode = ValidationMode.Full)
    {
        _span = span;
        Mode = mode;
    }

    /// <summary>Gets the validation mode for this span.</summary>
    public ValidationMode Mode { get; }

    /// <summary>Gets the length of the underlying span.</summary>
    public readonly int Length => _span.Length;

    /// <summary>Gets whether the span is empty.</summary>
    public readonly bool IsEmpty => _span.IsEmpty;

    /// <summary>
    /// Gets or sets the element at the specified index with bounds validation.
    /// </summary>
    /// <param name="index">The zero-based element index.</param>
    public ref T this[int index]
    {
        get
        {
            if (Mode >= ValidationMode.BoundsOnly)
                Guard.IsInRange(index, 0, _span.Length);

            return ref _span[index];
        }
    }

    /// <summary>
    /// Returns a checked slice of this span.
    /// </summary>
    /// <param name="start">The start index.</param>
    /// <param name="length">The length of the slice.</param>
    /// <returns>A new <see cref="CheckedSpan{T}"/> over the slice.</returns>
    public CheckedSpan<T> Slice(int start, int length)
    {
        if (Mode >= ValidationMode.BoundsOnly)
        {
            Guard.IsGreaterThanOrEqualTo(start, 0);
            Guard.IsGreaterThanOrEqualTo(length, 0);
            Guard.IsLessThanOrEqualTo(start + length, _span.Length);
        }

        return new CheckedSpan<T>(_span.Slice(start, length), Mode);
    }

    /// <summary>
    /// Copies the contents of this span to a destination span with validation.
    /// </summary>
    /// <param name="destination">The destination span.</param>
    public readonly void CopyTo(Span<T> destination)
    {
        if (Mode >= ValidationMode.BoundsOnly)
            Guard.IsGreaterThanOrEqualTo(destination.Length, _span.Length);

        _span.CopyTo(destination);
    }

    /// <summary>
    /// Fills the span with the specified value.
    /// </summary>
    /// <param name="value">The value to fill with.</param>
    public readonly void Fill(T value) => _span.Fill(value);

    /// <summary>
    /// Clears all elements to their default value.
    /// </summary>
    public readonly void Clear() => _span.Clear();

    /// <summary>
    /// Gets the underlying span. Use with caution as this bypasses validation.
    /// </summary>
    /// <returns>The raw underlying span.</returns>
    public readonly Span<T> AsSpan() => _span;
}
