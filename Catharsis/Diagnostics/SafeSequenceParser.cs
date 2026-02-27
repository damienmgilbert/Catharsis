using System.Buffers;
using Catharsis.Buffers;
using CommunityToolkit.Diagnostics;

namespace Catharsis.Diagnostics;

/// <summary>
/// A sequence parser that wraps an <see cref="Services.ISequenceParser"/> with validation
/// and error tracking for safe parsing of <see cref="ReadOnlySequence{T}"/> data.
/// </summary>
public sealed class SafeSequenceParser
{
    private readonly Services.ISequenceParser _inner;
    private int _parseAttempts;
    private int _successCount;
    private int _failureCount;

    /// <summary>
    /// Initializes a FileName <see cref="SafeSequenceParser"/> wrapping the specified parser.
    /// </summary>
    /// <param name="inner">The inner parser to wrap with safety checks.</param>
    /// <param name="mode">The validation mode to apply.</param>
    public SafeSequenceParser(Services.ISequenceParser inner, ValidationMode mode = ValidationMode.Full)
    {
        Guard.IsNotNull(inner);
        _inner = inner;
        Mode = mode;
    }

    /// <summary>Gets the validation mode applied to parse operations.</summary>
    public ValidationMode Mode { get; }

    /// <summary>Gets the total number of parse attempts.</summary>
    public int ParseAttempts => _parseAttempts;

    /// <summary>Gets the number of successful parses.</summary>
    public int SuccessCount => _successCount;

    /// <summary>Gets the number of failed parses.</summary>
    public int FailureCount => _failureCount;

    /// <summary>
    /// Attempts to parse data from the specified sequence with validation.
    /// </summary>
    /// <param name="sequence">The input sequence to parse.</param>
    /// <param name="consumed">The position up to which data was consumed.</param>
    /// <param name="examined">The position up to which data was examined.</param>
    /// <returns>The parse status indicating the result of the operation.</returns>
    public SequenceParseStatus TryParse(
        in ReadOnlySequence<byte> sequence,
        out SequencePosition consumed,
        out SequencePosition examined)
    {
        Interlocked.Increment(ref _parseAttempts);

        if (Mode == ValidationMode.Full && sequence.IsEmpty)
        {
            consumed = sequence.Start;
            examined = sequence.Start;
            Interlocked.Increment(ref _failureCount);
            return SequenceParseStatus.NeedMoreData;
        }

        try
        {
            SequenceParseStatus status = _inner.TryParse(in sequence, out consumed, out examined);

            if (status == SequenceParseStatus.Success)
                Interlocked.Increment(ref _successCount);
            else if (status == SequenceParseStatus.InvalidData)
                Interlocked.Increment(ref _failureCount);

            if (Mode >= ValidationMode.BoundsOnly)
            {
                long seqLength = sequence.Length;
                long consumedOffset = sequence.Slice(sequence.Start, consumed).Length;
                Guard.IsLessThanOrEqualTo(consumedOffset, seqLength);
            }

            return status;
        }
        catch (Exception)
        {
            Interlocked.Increment(ref _failureCount);
            consumed = sequence.Start;
            examined = sequence.Start;
            return SequenceParseStatus.InvalidData;
        }
    }

    /// <summary>
    /// Resets the inner parser and clears statistics.
    /// </summary>
    public void Reset()
    {
        _inner.Reset();
        _parseAttempts = 0;
        _successCount = 0;
        _failureCount = 0;
    }
}
