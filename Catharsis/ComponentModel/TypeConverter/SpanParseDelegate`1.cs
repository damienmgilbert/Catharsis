using System.Diagnostics.CodeAnalysis;

namespace Catharsis.ComponentModel.TypeConverter;

/// <summary>
/// A delegate that attempts to parse a value of <typeparamref name="T"/> from
/// a <see cref="ReadOnlySpan{T}"/> of <see cref="char"/>.
/// </summary>
/// <typeparam name="T">The target type.</typeparam>
/// <param name="span">The character span to parse.</param>
/// <param name="provider">The format provider for culture-aware parsing.</param>
/// <param name="result">
/// When this method returns, contains the parsed value if successful;
/// otherwise, the default value of <typeparamref name="T"/>.
/// </param>
/// <returns><c>true</c> if parsing succeeded; otherwise, <c>false</c>.</returns>
public delegate bool SpanParseDelegate<T>(
    ReadOnlySpan<char> span,
    IFormatProvider? provider,
    [MaybeNullWhen(false)] out T result);

/// <summary>
/// A delegate that attempts to format a value of <typeparamref name="T"/> into
/// a <see cref="Span{T}"/> of <see cref="char"/>.
/// </summary>
/// <typeparam name="T">The source type.</typeparam>
/// <param name="value">The value to format.</param>
/// <param name="destination">The destination span.</param>
/// <param name="provider">The format provider for culture-aware formatting.</param>
/// <param name="charsWritten">The number of characters written.</param>
/// <returns><c>true</c> if formatting succeeded; otherwise, <c>false</c>.</returns>
public delegate bool SpanFormatDelegate<T>(
    T value,
    Span<char> destination,
    IFormatProvider? provider,
    out int charsWritten);

