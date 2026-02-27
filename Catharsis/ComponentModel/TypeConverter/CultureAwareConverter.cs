using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Catharsis.ComponentModel.TypeConverter;

/// <summary>
/// A <see cref="System.ComponentModel.TypeConverter"/> that performs culture-aware
/// conversions for types implementing <see cref="IParsable{T}"/> and
/// <see cref="IFormattable"/>, with optional <see cref="ISpanParsable{T}"/>
/// and <see cref="ISpanFormattable"/> support for zero-allocation paths.
/// </summary>
/// <typeparam name="T">
/// The target type. Must implement <see cref="IParsable{T}"/> and
/// <see cref="IFormattable"/>.
/// </typeparam>
/// <remarks>
/// <para>
/// Conversion from <see cref="string"/> uses <see cref="IParsable{T}.Parse"/>
/// with the culture from the converter context. When <typeparamref name="T"/>
/// also implements <see cref="ISpanParsable{T}"/>, the
/// <see cref="TryParseSpan"/> method provides a zero-allocation alternative.
/// </para>
/// <para>
/// Conversion to <see cref="string"/> uses <see cref="IFormattable.ToString"/>
/// with the configured format and culture.
/// </para>
/// </remarks>
public class CultureAwareConverter<T> : System.ComponentModel.TypeConverter
    where T : IParsable<T>, IFormattable
{
    private readonly ConverterContext _context;

    /// <summary>
    /// Initializes a new instance of <see cref="CultureAwareConverter{T}"/>
    /// using the specified context.
    /// </summary>
    /// <param name="context">
    /// The converter context providing culture and format settings.
    /// If <c>null</c>, <see cref="ConverterContext.Default"/> is used.
    /// </param>
    public CultureAwareConverter(ConverterContext? context = null)
    {
        _context = context ?? ConverterContext.Default;
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
            var effectiveCulture = culture ?? _context.Culture;
            var input = PrepareInput(text);
            return T.Parse(input, effectiveCulture);
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

        if (value is T typedValue && destinationType == typeof(string))
        {
            var effectiveCulture = culture ?? _context.Culture;
            return typedValue.ToString(_context.Format, effectiveCulture);
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
            var input = PrepareInput(text);
            return T.TryParse(input, _context.Culture, out _);
        }

        return false;
    }

    /// <summary>
    /// Attempts to parse a value of <typeparamref name="T"/> from a
    /// <see cref="ReadOnlySpan{T}"/> of <see cref="char"/> using
    /// <see cref="ISpanParsable{T}"/> when available.
    /// </summary>
    /// <param name="span">The character span to parse.</param>
    /// <param name="result">
    /// When this method returns, contains the parsed value if successful;
    /// otherwise, the default value of <typeparamref name="T"/>.
    /// </param>
    /// <returns><c>true</c> if parsing succeeded; otherwise, <c>false</c>.</returns>
    public bool TryParseSpan(ReadOnlySpan<char> span, [MaybeNullWhen(false)] out T result)
    {
        var text = span.Trim().ToString();
        return T.TryParse(text, _context.Culture, out result!);
    }

    /// <summary>
    /// Attempts to format a value of <typeparamref name="T"/> into a character
    /// span using <see cref="ISpanFormattable"/> when available.
    /// </summary>
    /// <param name="value">The value to format.</param>
    /// <param name="destination">The destination span.</param>
    /// <param name="charsWritten">The number of characters written.</param>
    /// <returns>
    /// <c>true</c> if the value was successfully written; otherwise, <c>false</c>.
    /// </returns>
    public bool TryFormatSpan(T value, Span<char> destination, out int charsWritten)
    {
        if (value is ISpanFormattable spanFormattable)
            return spanFormattable.TryFormat(destination, out charsWritten, _context.Format, _context.Culture);

        var formatted = value.ToString(_context.Format, _context.Culture);

        if (formatted is null)
        {
            charsWritten = 0;
            return true;
        }

        if (formatted.Length <= destination.Length)
        {
            formatted.AsSpan().CopyTo(destination);
            charsWritten = formatted.Length;
            return true;
        }

        charsWritten = 0;
        return false;
    }

    private string PrepareInput(string text)
    {
        if (_context.AllowLeadingWhiteSpace && _context.AllowTrailingWhiteSpace)
            return text.Trim();

        if (_context.AllowLeadingWhiteSpace)
            return text.TrimStart();

        if (_context.AllowTrailingWhiteSpace)
            return text.TrimEnd();

        return text;
    }
}
