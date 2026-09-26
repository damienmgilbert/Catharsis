using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;

namespace Catharsis.ComponentModel.TypeConverter;

///<summary>
///A generic <see cref="System.ComponentModel.TypeConverter"/> that uses delegates for <see cref="ConvertFrom"/> and
///<see cref="ConvertTo"/> operations, providing a reusable, culture-aware conversion base for any type <typeparamref
///name="T"/>.
///</summary>
///<typeparam name="T">The target type this converter handles.</typeparam>
///<remarks>
///<para> Supply <c>convertFrom</c> and <c>convertTo</c> delegates at construction time to define the conversion logic.
///Both delegates receive the<see cref="ITypeDescriptorContext"/>, <see cref="CultureInfo"/>, and the source
///value.</para> <para> By default, conversion from <see cref="string"/> is supported if a<c>convertFrom</c> delegate is
///provided, and conversion to<see cref="string"/> is supported if a <c>convertTo</c> delegate is provided. Override
///<see cref="GetSupportedSourceTypes"/> or<see cref="GetSupportedDestinationTypes"/> to extend the supported type
///set.</para>
///</remarks>
///<remarks>
///Initializes a FileName instance of <see cref="GenericTypeConverter{T}"/>.
///</remarks>
///<param name="convertFrom">
///A delegate that converts a source value to <typeparamref name="T"/>. May be <c>null</c> if conversion from other
///types is not supported.
///</param>
///<param name="convertTo">
///A delegate that converts a <typeparamref name="T"/> value to a destination type. May be <c>null</c> if conversion
///to other types is not supported.
///</param>
public class GenericTypeConverter<T>(Func<ITypeDescriptorContext?, CultureInfo?, object, T?>? convertFrom = null, Func<ITypeDescriptorContext?, CultureInfo?, T, Type, object?>? convertTo = null) : System.ComponentModel.TypeConverter
{
    #region Fields
    readonly Func<ITypeDescriptorContext?, CultureInfo?, object, T?>? _convertFrom = convertFrom;
    readonly Func<ITypeDescriptorContext?, CultureInfo?, T, Type, object?>? _convertTo = convertTo;

    #endregion
    #region Constructors
    #endregion

    #region Protected methods
    ///<summary>
    ///Gets the set of destination types that <see cref="ConvertTo"/> supports. The default implementation returns <c>[
    ///typeof(string) ]</c>.
    ///</summary>
    ///<returns>An array of supported destination types.</returns>
    protected virtual Type[] GetSupportedDestinationTypes() { return[ typeof(string), typeof(InstanceDescriptor) ]; }
    ///<summary>
    ///Gets the set of source types that <see cref="ConvertFrom"/> supports. The default implementation returns <c>[
    ///typeof(string) ]</c>.
    ///</summary>
    ///<returns>An array of supported source types.</returns>
    protected virtual Type[] GetSupportedSourceTypes() { return[ typeof(string) ]; }
    #endregion

    #region Public methods
    ///<inheritdoc/>
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
    {
        if(_convertFrom is null)
        {
            return base.CanConvertFrom(context, sourceType);
        }

        foreach(Type supported in GetSupportedSourceTypes())
        {
            if(supported.IsAssignableFrom(sourceType))
            {
                return true;
            }
        }

        return base.CanConvertFrom(context, sourceType);
    }

    ///<inheritdoc/>
    public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
    {
        if(destinationType is null)
        {
            return false;
        }

        if(_convertTo is null)
        {
            return base.CanConvertTo(context, destinationType);
        }

        foreach(Type supported in GetSupportedDestinationTypes())
        {
            if(supported.IsAssignableFrom(destinationType))
            {
                return true;
            }
        }

        return base.CanConvertTo(context, destinationType);
    }

    ///<inheritdoc/>
    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        if((_convertFrom is not null) && CanConvertFrom(context, value.GetType()))
        {
            return _convertFrom(context, culture ?? CultureInfo.CurrentCulture, value);
        }

        return base.ConvertFrom(context, culture, value);
    }

    ///<inheritdoc/>
    public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
    {
        ArgumentNullException.ThrowIfNull(destinationType);

        if((_convertTo is not null) && (value is T typed) && CanConvertTo(context, destinationType))
        {
            return _convertTo(context, culture ?? CultureInfo.CurrentCulture, typed, destinationType);
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

        if(value is null)
        {
            return false;
        }

        try
        {
            ConvertFrom(context, CultureInfo.CurrentCulture, value);
            return true;
        } catch
        {
            return false;
        }
    }
    #endregion
}
