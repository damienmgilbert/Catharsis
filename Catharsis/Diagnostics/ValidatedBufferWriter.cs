using System.Buffers;
using CommunityToolkit.Diagnostics;

namespace Catharsis.Diagnostics;

/// <summary>
/// A wrapper around <see cref="IBufferWriter{T}"/> that validates all operations
/// using CommunityToolkit <see cref="Guard"/> assertions based on the configured <see cref="ValidationMode"/>.
/// </summary>
/// <typeparam name="T">The type of elements in the buffer.</typeparam>
public sealed class ValidatedBufferWriter<T> : IBufferWriter<T>
{
    private readonly IBufferWriter<T> _inner;
    private int _pendingAdvance;
    private int _lastSpanSize;

    /// <summary>
    /// Initializes a new <see cref="ValidatedBufferWriter{T}"/> wrapping the specified writer.
    /// </summary>
    /// <param name="inner">The inner buffer writer to delegate to.</param>
    /// <param name="mode">The validation mode to apply.</param>
    public ValidatedBufferWriter(IBufferWriter<T> inner, ValidationMode mode = ValidationMode.Full)
    {
        Guard.IsNotNull(inner);
        _inner = inner;
        Mode = mode;
    }

    /// <summary>Gets the validation mode applied to operations.</summary>
    public ValidationMode Mode { get; }

    /// <summary>Gets the total number of elements advanced through this writer.</summary>
    public long TotalAdvanced { get; private set; }

    /// <inheritdoc />
    public void Advance(int count)
    {
        if (Mode >= ValidationMode.BoundsOnly)
        {
            Guard.IsGreaterThanOrEqualTo(count, 0);
            Guard.IsLessThanOrEqualTo(count, _lastSpanSize - _pendingAdvance, nameof(count));
        }

        _inner.Advance(count);
        _pendingAdvance += count;
        TotalAdvanced += count;
    }

    /// <inheritdoc />
    public Memory<T> GetMemory(int sizeHint = 0)
    {
        if (Mode >= ValidationMode.BoundsOnly)
            Guard.IsGreaterThanOrEqualTo(sizeHint, 0);

        Memory<T> memory = _inner.GetMemory(sizeHint);

        if (Mode == ValidationMode.Full)
            Guard.IsGreaterThan(memory.Length, 0, "The inner writer returned an empty memory block.");

        _lastSpanSize = memory.Length;
        _pendingAdvance = 0;
        return memory;
    }

    /// <inheritdoc />
    public Span<T> GetSpan(int sizeHint = 0)
    {
        if (Mode >= ValidationMode.BoundsOnly)
            Guard.IsGreaterThanOrEqualTo(sizeHint, 0);

        Span<T> span = _inner.GetSpan(sizeHint);

        if (Mode == ValidationMode.Full)
            Guard.IsGreaterThan(span.Length, 0, "The inner writer returned an empty span.");

        _lastSpanSize = span.Length;
        _pendingAdvance = 0;
        return span;
    }
}
