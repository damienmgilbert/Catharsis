using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Catharsis.ComponentModel.TypeConverter;

///<summary>
///A <see cref="System.ComponentModel.TypeConverter"/> that performs culture-aware conversions for types implementing
public class CultureAwareConverter<T>(ConverterContext? context = null) : System.ComponentModel.TypeConverter where T : IParsable<T>, IFormattable
{
    #region Fields
    private readonly ConverterContext _context = context ?? ConverterContext.Default;
    #endregion

    #region Private methods
    private string PrepareInput(string text)
    {
        if(_context.AllowLeadingWhiteSpace && _context.AllowTrailingWhiteSpace)
        {
            return text.Trim();
        }

        if(_context.AllowLeadingWhiteSpace)
        {
            return text.TrimStart();
        }

        if(_context.AllowTrailingWhiteSpace)
        {
            return text.TrimEnd();
        }

        return text;
    }
    #endregion

    #region Public methods
    ///<inheritdoc/>
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) => (sourceType == typeof(string)) || base.CanConvertFrom(context, sourceType);

    ///<inheritdoc/>
    public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType) => (destinationType == typeof(string)) || base.CanConvertTo(context, destinationType);

    ///<inheritdoc/>
    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        if(value is string text)
        {
            CultureInfo effectiveCulture = culture ?? _context.Culture;
            string input = PrepareInput(text);
            return T.Parse(input, effectiveCulture);
        }

        return base.ConvertFrom(context, culture, value);
    }

    ///<inheritdoc/>
    public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
    {
        ArgumentNullException.ThrowIfNull(destinationType);

        if((value is T typedValue) && (destinationType == typeof(string)))
        {
            CultureInfo effectiveCulture = culture ?? _context.Culture;
            return typedValue.ToString(_context.Format, effectiveCulture);
        }

        return base.ConvertTo(context, culture, value, destinationType);
    }

    ///<inheritdoc/>
    public override bool IsValid(ITypeDescriptorContext? context, object? value)
    {
        if(value is T)
        {
            return true;
        }

        if(value is string text)
        {
            string input = PrepareInput(text);
            return T.TryParse(input, _context.Culture, out _);
        }

        return false;
    }

    ///<summary>
    ///Attempts to format a value of <typeparamref name="T"/> into a character span using <see cref="ISpanFormattable"/>
    ///when available.
    ///</summary>
    ///<param name="value">The value to format.</param>
    ///<param name="destination">The destination span.</param>
    ///<param name="charsWritten">The number of characters written.</param>
    ///<returns>
    public bool TryFormatSpan(T value, Span<char> destination, out int charsWritten)
    {
        if(value is ISpanFormattable spanFormattable)
        {
            return spanFormattable.TryFormat(destination, out charsWritten, _context.Format, _context.Culture);
        }

        string formatted = value.ToString(_context.Format, _context.Culture);

        if(formatted is null)
        {
            charsWritten = 0;
            return true;
        }

        if(formatted.Length <= destination.Length)
        {
            formatted.AsSpan().CopyTo(destination);
            charsWritten = formatted.Length;
            return true;
        }

        charsWritten = 0;
        return false;
    }

    ///<summary>
    ///Attempts to parse a value of <typeparamref name="T"/> from a <see cref="ReadOnlySpan{T}"/> of <see cref="char"/>
    ///using <see cref="ISpanParsable{T}"/> when available.
    ///</summary>
    ///<param name="span">The character span to parse.</param>
    ///<param name="result">
    ///When this method returns, contains the parsed value if successful; otherwise, the default value of <typeparamref
    ///name="T"/>.
    ///</param>
    ///<returns><c>true</c> if parsing succeeded; otherwise, <c>false</c>.</returns>
    public bool TryParseSpan(ReadOnlySpan<char> span, [MaybeNullWhen(false)] out T result)
    {
        string text = span.Trim().ToString();
        return T.TryParse(text, _context.Culture, out result!);
    }
    #endregion

    #region Public properties
    ///<summary>
    ///Gets the converter context used by this instance.
    ///</summary>
    public ConverterContext Context => _context;
    #endregion
}
