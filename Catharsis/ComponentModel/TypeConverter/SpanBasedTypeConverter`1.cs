using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Catharsis.ComponentModel.TypeConverter;

/// <summary>
/// A <see cref="System.ComponentModel.TypeConverter"/> that uses
/// <see cref="ReadOnlySpan{T}"/>-based delegates for zero-allocation
/// parsing and formatting of <typeparamref name="T"/> values.
/// </summary>
/// <typeparam name="T">The target type this converter handles.</typeparam>
/// <remarks>
/// <para>
/// This converter is designed for high-performance scenarios where avoiding
/// string allocations during conversion is important. It falls back to
/// standard string-based conversion when the span-based path is not available
/// for a given call site.
/// </para>
/// <para>
/// The <see cref="TryParseSpan"/> and <see cref="TryFormatSpan"/> methods
/// provide direct access to the span-based conversion delegates without
/// going through the <see cref="System.ComponentModel.TypeConverter"/>
/// infrastructure.
/// </para>
/// </remarks>
public sealed class SpanBasedTypeConverter<T> : System.ComponentModel.TypeConverter
{
    private readonly SpanParseDelegate<T> _tryParse;
    private readonly SpanFormatDelegate<T>? _tryFormat;
    private readonly ConverterContext _context;

    /// <summary>
    /// Initializes a new instance of <see cref="SpanBasedTypeConverter{T}"/>.
    /// </summary>
    /// <param name="tryParse">
    /// A delegate that attempts to parse <typeparamref name="T"/> from a character span.
    /// </param>
    /// <param name="tryFormat">
    /// An optional delegate that attempts to format <typeparamref name="T"/>
    /// into a character span. If <c>null</c>, <see cref="object.ToString"/>
    /// is used for conversion to <see cref="string"/>.
    /// </param>
    /// <param name="context">
    /// The converter context providing culture and format settings.
    /// If <c>null</c>, <see cref="ConverterContext.Invariant"/> is used.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="tryParse"/> is <c>null</c>.
    /// </exception>
    public SpanBasedTypeConverter(
        SpanParseDelegate<T> tryParse,
        SpanFormatDelegate<T>? tryFormat = null,
        ConverterContext? context = null)
    {
        ArgumentNullException.ThrowIfNull(tryParse);

        _tryParse = tryParse;
        _tryFormat = tryFormat;
        _context = context ?? ConverterContext.Invariant;
    }

    /// <summary>
    /// Gets the converter context used by this instance.
    /// </summary>
    public ConverterContext Context => _context;

    /// <inheritdoc />
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) =>
        sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);

    /// <inheritdoc />
    public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType) =>
        destinationType == typeof(string) || base.CanConvertTo(context, destinationType);

    /// <inheritdoc />
    public override object? ConvertFrom(
        ITypeDescriptorContext? context,
        CultureInfo? culture,
        object value)
    {
        if (value is string text)
        {
            var span = text.AsSpan();

            if (_context.AllowLeadingWhiteSpace || _context.AllowTrailingWhiteSpace)
                span = span.Trim();

            var provider = (IFormatProvider?)culture ?? _context.Culture;

            if (_tryParse(span, provider, out var result))
                return result;

            throw new FormatException(
                $"Cannot convert '{text}' to {typeof(T).Name}.");
        }

        return base.ConvertFrom(context, culture, value);
    }

    /// <inheritdoc />
    public override object? ConvertTo(
        ITypeDescriptorContext? context,
        CultureInfo? culture,
        object? value,
        Type destinationType)
    {
        ArgumentNullException.ThrowIfNull(destinationType);

        if (value is T typed && destinationType == typeof(string))
        {
            var provider = (IFormatProvider?)culture ?? _context.Culture;

            if (_tryFormat is not null)
            {
                Span<char> buffer = stackalloc char[256];

                if (_tryFormat(typed, buffer, provider, out var charsWritten))
                    return new string(buffer[..charsWritten]);
            }

            return typed?.ToString();
        }

        return base.ConvertTo(context, culture, value, destinationType);
    }

    /// <inheritdoc />
    public override bool IsValid(ITypeDescriptorContext? context, object? value)
    {
        if (value is T)
            return true;

        if (value is string text)
        {
            var span = text.AsSpan().Trim();
            return _tryParse(span, _context.Culture, out _);
        }

        return false;
    }

    /// <summary>
    /// Attempts to parse a value of <typeparamref name="T"/> from a
    /// <see cref="ReadOnlySpan{T}"/> of <see cref="char"/> using the
    /// configured parse delegate and culture.
    /// </summary>
    /// <param name="span">The character span to parse.</param>
    /// <param name="result">
    /// When this method returns, contains the parsed value if successful;
    /// otherwise, the default value of <typeparamref name="T"/>.
    /// </param>
    /// <returns><c>true</c> if parsing succeeded; otherwise, <c>false</c>.</returns>
    public bool TryParseSpan(ReadOnlySpan<char> span, [MaybeNullWhen(false)] out T result) =>
        _tryParse(span.Trim(), _context.Culture, out result);

    /// <summary>
    /// Attempts to format a value of <typeparamref name="T"/> into a character
    /// span using the configured format delegate and culture.
    /// </summary>
    /// <param name="value">The value to format.</param>
    /// <param name="destination">The destination span.</param>
    /// <param name="charsWritten">The number of characters written.</param>
    /// <returns>
    /// <c>true</c> if the value was successfully written; otherwise, <c>false</c>.
    /// Returns <c>false</c> if no format delegate was configured.
    /// </returns>
    public bool TryFormatSpan(T value, Span<char> destination, out int charsWritten)
    {
        if (_tryFormat is null)
        {
            charsWritten = 0;
            return false;
        }

        return _tryFormat(value, destination, _context.Culture, out charsWritten);
    }
}
