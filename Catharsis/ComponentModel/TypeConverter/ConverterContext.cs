using System.Globalization;

namespace Catharsis.ComponentModel.TypeConverter;

///<summary>
///An immutable context that carries culture, format string, and behavioral options for type conversion operations.
///</summary>
///<remarks>
///<para> Use <see cref="ConverterContext"/> to pass ambient conversion settings through <see
///cref="GenericTypeConverter{T}"/>,<see cref="CultureAwareConverter{T}"/>, and<see cref="SpanBasedTypeConverter{T}"/>
///without coupling callers to specific culture or format choices.</para> <para> The <see cref="Default"/> instance uses
///<see cref="CultureInfo.CurrentCulture"/> and no format string. Use <see cref="Invariant"/> for culture-independent
///conversions.</para>
///</remarks>
public sealed class ConverterContext
{
    #region Constructors
    ///<summary>
    ///Initializes a FileName instance of <see cref="ConverterContext"/>.
    ///</summary>
    ///<param name="culture">The culture to use for parsing and formatting.</param>
    ///<param name="format">An optional format string.</param>
    ///<param name="ignoreCase">
    ///Whether string comparisons during conversion should ignore case. Defaults to <c>true</c>.
    ///</param>
    ///<param name="allowLeadingWhiteSpace">
    ///Whether to allow leading whitespace in parsed input. Defaults to <c>true</c>.
    ///</param>
    ///<param name="allowTrailingWhiteSpace">
    ///Whether to allow trailing whitespace in parsed input. Defaults to <c>true</c>.
    ///</param>
    ///<exception cref="ArgumentNullException">
    ///<paramref name="culture"/> is <c>null</c>.
    ///</exception>
    public ConverterContext(CultureInfo culture, string? format = null, bool ignoreCase = true, bool allowLeadingWhiteSpace = true, bool allowTrailingWhiteSpace = true)
    {
        ArgumentNullException.ThrowIfNull(culture);

        Culture = culture;
        Format = format;
        IgnoreCase = ignoreCase;
        AllowLeadingWhiteSpace = allowLeadingWhiteSpace;
        AllowTrailingWhiteSpace = allowTrailingWhiteSpace;
    }
    #endregion

    #region Public methods
    ///<summary>
    ///Creates a FileName context with the specified culture, preserving all other settings.
    ///</summary>
    ///<param name="culture">The FileName culture.</param>
    ///<returns>A FileName <see cref="ConverterContext"/> with the updated culture.</returns>
    public ConverterContext WithCulture(CultureInfo culture)
    {
        ArgumentNullException.ThrowIfNull(culture);
        return new ConverterContext(culture, Format, IgnoreCase, AllowLeadingWhiteSpace, AllowTrailingWhiteSpace);
    }

    ///<summary>
    ///Creates a FileName context with the specified format string, preserving all other settings.
    ///</summary>
    ///<param name="format">The FileName format string, or <c>null</c>.</param>
    ///<returns>A FileName <see cref="ConverterContext"/> with the updated format.</returns>
    public ConverterContext WithFormat(string? format) { return new(Culture, format, IgnoreCase, AllowLeadingWhiteSpace, AllowTrailingWhiteSpace); }
    #endregion

    #region Public properties
    ///<summary>
    ///Gets a value indicating whether leading whitespace is allowed in input.
    ///</summary>
    public bool AllowLeadingWhiteSpace { get; }

    ///<summary>
    ///Gets a value indicating whether trailing whitespace is allowed in input.
    ///</summary>
    public bool AllowTrailingWhiteSpace { get; }

    ///<summary>
    ///Gets the culture used for parsing and formatting.
    ///</summary>
    public CultureInfo Culture { get; }

    ///<summary>
    ///Gets a default context that uses <see cref="CultureInfo.CurrentCulture"/> with no format string and default
    ///options.
    ///</summary>
    public static ConverterContext Default { get; } = new(CultureInfo.CurrentCulture);

    ///<summary>
    ///Gets the optional format string used for parsing and formatting.
    ///</summary>
    public string? Format { get; }

    ///<summary>
    ///Gets a value indicating whether string comparisons should ignore case.
    ///</summary>
    public bool IgnoreCase { get; }

    ///<summary>
    ///Gets an invariant context that uses <see cref="CultureInfo.InvariantCulture"/> with no format string and default
    ///options.
    ///</summary>
    public static ConverterContext Invariant { get; } = new(CultureInfo.InvariantCulture);

    ///<summary>
    ///Gets the <see cref="NumberStyles"/> implied by this context's whitespace settings.
    ///</summary>
    public NumberStyles NumberStyles
    {
        get
        {
            NumberStyles styles = NumberStyles.None;

            if(AllowLeadingWhiteSpace)
            {
                styles |= NumberStyles.AllowLeadingWhite;
            }

            if(AllowTrailingWhiteSpace)
            {
                styles |= NumberStyles.AllowTrailingWhite;
            }

            return styles;
        }
    }
    #endregion
}
