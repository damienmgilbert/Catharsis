using System.ComponentModel;
using System.Globalization;

namespace Catharsis.ComponentModel.TypeConverter;

/// <summary>
/// A strongly-typed <see cref="System.ComponentModel.TypeConverter"/> for enum types that
/// supports culture-aware string conversion, <see cref="FlagsAttribute"/> enums,
/// and <see cref="Span{T}"/>-based parsing for zero-allocation scenarios.
/// </summary>
/// <typeparam name="TEnum">The enum type. Must be a value type and an enum.</typeparam>
/// <remarks>
/// <para>
/// This converter handles conversion between <typeparamref name="TEnum"/> and
/// <see cref="string"/>, as well as underlying integer types. For
/// <see cref="FlagsAttribute"/> enums, comma-separated flag names are supported.
/// </para>
/// <para>
/// Use <see cref="TryParseSpan"/> for zero-allocation parsing from a
/// <see cref="ReadOnlySpan{T}"/> of <see cref="char"/>.
/// </para>
/// </remarks>
public sealed class EnumTypeConverter<TEnum> : System.ComponentModel.TypeConverter
    where TEnum : struct, Enum
{
    private static readonly Type s_enumType = typeof(TEnum);
    private static readonly Type s_underlyingType = Enum.GetUnderlyingType(s_enumType);
    private static readonly bool s_isFlags = s_enumType.IsDefined(typeof(FlagsAttribute), inherit: false);

    /// <summary>
    /// Gets a value indicating whether <typeparamref name="TEnum"/> has
    /// the <see cref="FlagsAttribute"/>.
    /// </summary>
    public static bool IsFlags => s_isFlags;

    /// <inheritdoc />
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) =>
        sourceType == typeof(string) ||
        sourceType == s_underlyingType ||
        base.CanConvertFrom(context, sourceType);

    /// <inheritdoc />
    public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType) =>
        destinationType == typeof(string) ||
        destinationType == s_underlyingType ||
        base.CanConvertTo(context, destinationType);

    /// <inheritdoc />
    public override object? ConvertFrom(
        ITypeDescriptorContext? context,
        CultureInfo? culture,
        object value)
    {
        if (value is string text)
        {
            var trimmed = text.AsSpan().Trim();

            if (trimmed.IsEmpty)
                throw new FormatException($"Cannot convert an empty string to {s_enumType.Name}.");

            if (TryParseSpan(trimmed, out var result))
                return result;

            throw new FormatException(
                $"'{text}' is not a valid value for {s_enumType.Name}.");
        }

        if (value.GetType() == s_underlyingType)
            return (TEnum)Enum.ToObject(s_enumType, value);

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

        if (value is TEnum enumValue)
        {
            if (destinationType == typeof(string))
                return FormatEnum(enumValue);

            if (destinationType == s_underlyingType)
                return Convert.ChangeType(enumValue, s_underlyingType, culture);
        }

        return base.ConvertTo(context, culture, value, destinationType);
    }

    /// <inheritdoc />
    public override bool IsValid(ITypeDescriptorContext? context, object? value)
    {
        if (value is TEnum)
            return true;

        if (value is string text)
            return TryParseSpan(text.AsSpan().Trim(), out _);

        return false;
    }

    /// <inheritdoc />
    public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext? context) =>
        new(Enum.GetValues<TEnum>());

    /// <inheritdoc />
    public override bool GetStandardValuesSupported(ITypeDescriptorContext? context) => true;

    /// <inheritdoc />
    public override bool GetStandardValuesExclusive(ITypeDescriptorContext? context) => !s_isFlags;

    /// <summary>
    /// Attempts to parse a <typeparamref name="TEnum"/> value from a
    /// <see cref="ReadOnlySpan{T}"/> of <see cref="char"/> without
    /// allocating a string.
    /// </summary>
    /// <param name="span">The character span to parse.</param>
    /// <param name="result">
    /// When this method returns, contains the parsed enum value if successful;
    /// otherwise, the default value of <typeparamref name="TEnum"/>.
    /// </param>
    /// <returns><c>true</c> if parsing succeeded; otherwise, <c>false</c>.</returns>
    public static bool TryParseSpan(ReadOnlySpan<char> span, out TEnum result) =>
        Enum.TryParse(span, ignoreCase: true, out result);

    /// <summary>
    /// Parses a <typeparamref name="TEnum"/> value from a
    /// <see cref="ReadOnlySpan{T}"/> of <see cref="char"/>.
    /// </summary>
    /// <param name="span">The character span to parse.</param>
    /// <returns>The parsed enum value.</returns>
    /// <exception cref="FormatException">
    /// <paramref name="span"/> is not a valid representation of
    /// <typeparamref name="TEnum"/>.
    /// </exception>
    public static TEnum ParseSpan(ReadOnlySpan<char> span)
    {
        if (TryParseSpan(span, out var result))
            return result;

        throw new FormatException(
            $"'{span.ToString()}' is not a valid value for {s_enumType.Name}.");
    }

    /// <summary>
    /// Formats an enum value to its string representation.
    /// For flags enums, produces a comma-separated list of flag names.
    /// </summary>
    /// <param name="value">The enum value to format.</param>
    /// <returns>The string representation.</returns>
    public static string FormatEnum(TEnum value) => value.ToString();

    /// <summary>
    /// Attempts to format an enum value into a character span without
    /// allocating a string.
    /// </summary>
    /// <param name="value">The enum value to format.</param>
    /// <param name="destination">The destination span to write into.</param>
    /// <param name="charsWritten">
    /// When this method returns, contains the number of characters written.
    /// </param>
    /// <returns>
    /// <c>true</c> if the value was successfully formatted into the destination;
    /// otherwise, <c>false</c>.
    /// </returns>
    public static bool TryFormatSpan(TEnum value, Span<char> destination, out int charsWritten) =>
        Enum.TryFormat(value, destination, out charsWritten);
}
