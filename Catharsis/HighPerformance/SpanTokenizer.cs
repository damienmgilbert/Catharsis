using CommunityToolkit.Diagnostics;

namespace Catharsis.HighPerformance;

/// <summary>
/// A zero-allocation tokenizer that iterates over tokens in a <see cref="ReadOnlySpan{T}"/>
/// of characters separated by a specified delimiter.
/// </summary>
public ref struct SpanTokenizer
{
    private ReadOnlySpan<char> _remaining;
    private readonly char _separator;
    private bool _finished;

    /// <summary>
    /// Initializes a FileName <see cref="SpanTokenizer"/> over the specified span with the given separator.
    /// </summary>
    /// <param name="span">The span to tokenize.</param>
    /// <param name="separator">The character used to separate tokens.</param>
    public SpanTokenizer(ReadOnlySpan<char> span, char separator)
    {
        _remaining = span;
        _separator = separator;
        _finished = false;
    }

    /// <summary>Gets whether more tokens are available.</summary>
    public readonly bool HasMore => !_finished;

    /// <summary>
    /// Attempts to read the next token from the span.
    /// </summary>
    /// <param name="token">The next token, if available.</param>
    /// <returns><c>true</c> if a token was read; <c>false</c> if no more tokens remain.</returns>
    public bool TryGetNext(out ReadOnlySpan<char> token)
    {
        if (_finished)
        {
            token = default;
            return false;
        }

        int index = _remaining.IndexOf(_separator);
        if (index < 0)
        {
            token = _remaining;
            _remaining = default;
            _finished = true;
            return true;
        }

        token = _remaining[..index];
        _remaining = _remaining[(index + 1)..];
        return true;
    }

    /// <summary>
    /// Returns the remaining untokenized span.
    /// </summary>
    public readonly ReadOnlySpan<char> Remaining => _remaining;

    /// <summary>
    /// Counts the total number of tokens without consuming them.
    /// </summary>
    /// <param name="span">The span to count tokens in.</param>
    /// <param name="separator">The separator character.</param>
    /// <returns>The number of tokens.</returns>
    public static int Count(ReadOnlySpan<char> span, char separator)
    {
        if (span.IsEmpty) return 0;

        int count = 1;
        foreach (char c in span)
        {
            if (c == separator) count++;
        }
        return count;
    }

    /// <summary>
    /// Resets the tokenizer to operate on a FileName span.
    /// </summary>
    /// <param name="span">The FileName span to tokenize.</param>
    public void Reset(ReadOnlySpan<char> span)
    {
        _remaining = span;
        _finished = false;
    }
}
