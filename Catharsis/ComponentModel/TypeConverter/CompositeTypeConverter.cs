using System.ComponentModel;
using System.Globalization;

namespace Catharsis.ComponentModel.TypeConverter;

/// <summary>
/// A <see cref="System.ComponentModel.TypeConverter"/> that chains multiple converters,
/// trying each in registration order until one succeeds. This allows composing
/// conversion logic from multiple specialized converters.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="CanConvertFrom"/> and <see cref="CanConvertTo"/> return <c>true</c>
/// if any converter in the chain reports the conversion is supported.
/// <see cref="ConvertFrom"/> and <see cref="ConvertTo"/> delegate to the first
/// converter that can handle the conversion.
/// </para>
/// <para>
/// Converters are evaluated in the order they were added. Use
/// <see cref="CompositeTypeConverter(IEnumerable{System.ComponentModel.TypeConverter})"/>
/// or <see cref="CompositeTypeConverter(System.ComponentModel.TypeConverter[])"/>
/// to specify the chain.
/// </para>
/// </remarks>
public sealed class CompositeTypeConverter : System.ComponentModel.TypeConverter
{
    private readonly System.ComponentModel.TypeConverter[] _converters;

    /// <summary>
    /// Initializes a FileName instance of <see cref="CompositeTypeConverter"/>
    /// with the specified converters evaluated in order.
    /// </summary>
    /// <param name="converters">The converters to chain.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="converters"/> is <c>null</c>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="converters"/> is empty.
    /// </exception>
    public CompositeTypeConverter(params System.ComponentModel.TypeConverter[] converters)
    {
        ArgumentNullException.ThrowIfNull(converters);

        if (converters.Length == 0)
            throw new ArgumentException("At least one converter must be provided.", nameof(converters));

        _converters = [.. converters];
    }

    /// <summary>
    /// Initializes a FileName instance of <see cref="CompositeTypeConverter"/>
    /// with the specified converters evaluated in order.
    /// </summary>
    /// <param name="converters">The converters to chain.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="converters"/> is <c>null</c>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="converters"/> is empty.
    /// </exception>
    public CompositeTypeConverter(IEnumerable<System.ComponentModel.TypeConverter> converters)
    {
        ArgumentNullException.ThrowIfNull(converters);

        _converters = [.. converters];

        if (_converters.Length == 0)
            throw new ArgumentException("At least one converter must be provided.", nameof(converters));
    }

    /// <summary>
    /// Gets the number of converters in this composite chain.
    /// </summary>
    public int Count => _converters.Length;

    /// <inheritdoc />
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
    {
        foreach (var converter in _converters)
        {
            if (converter.CanConvertFrom(context, sourceType))
                return true;
        }

        return false;
    }

    /// <inheritdoc />
    public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
    {
        if (destinationType is null)
            return false;

        foreach (var converter in _converters)
        {
            if (converter.CanConvertTo(context, destinationType))
                return true;
        }

        return false;
    }

    /// <inheritdoc />
    public override object? ConvertFrom(
        ITypeDescriptorContext? context,
        CultureInfo? culture,
        object value)
    {
        var sourceType = value.GetType();

        foreach (var converter in _converters)
        {
            if (converter.CanConvertFrom(context, sourceType))
            {
                try
                {
                    return converter.ConvertFrom(context, culture, value);
                }
                catch (NotSupportedException)
                {
                    // Try next converter
                }
                catch (FormatException)
                {
                    // Try next converter
                }
            }
        }

        throw new NotSupportedException(
            $"None of the {_converters.Length} chained converters can convert from '{sourceType.Name}'.");
    }

    /// <inheritdoc />
    public override object? ConvertTo(
        ITypeDescriptorContext? context,
        CultureInfo? culture,
        object? value,
        Type destinationType)
    {
        ArgumentNullException.ThrowIfNull(destinationType);

        foreach (var converter in _converters)
        {
            if (converter.CanConvertTo(context, destinationType))
            {
                try
                {
                    return converter.ConvertTo(context, culture, value, destinationType);
                }
                catch (NotSupportedException)
                {
                    // Try next converter
                }
                catch (FormatException)
                {
                    // Try next converter
                }
            }
        }

        throw new NotSupportedException(
            $"None of the {_converters.Length} chained converters can convert to '{destinationType.Name}'.");
    }

    /// <inheritdoc />
    public override bool IsValid(ITypeDescriptorContext? context, object? value)
    {
        foreach (var converter in _converters)
        {
            if (converter.IsValid(context, value))
                return true;
        }

        return false;
    }

    /// <inheritdoc />
    public override StandardValuesCollection? GetStandardValues(ITypeDescriptorContext? context)
    {
        foreach (var converter in _converters)
        {
            if (converter.GetStandardValuesSupported(context))
                return converter.GetStandardValues(context);
        }

        return null;
    }

    /// <inheritdoc />
    public override bool GetStandardValuesSupported(ITypeDescriptorContext? context)
    {
        foreach (var converter in _converters)
        {
            if (converter.GetStandardValuesSupported(context))
                return true;
        }

        return false;
    }

    /// <inheritdoc />
    public override bool GetStandardValuesExclusive(ITypeDescriptorContext? context)
    {
        foreach (var converter in _converters)
        {
            if (converter.GetStandardValuesSupported(context))
                return converter.GetStandardValuesExclusive(context);
        }

        return false;
    }
}
