using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Catharsis.ComponentModel.TypeConverter;

///<summary>
///A thread-safe registry that maps types to <see cref="System.ComponentModel.TypeConverter"/> instances, providing
///centralized converter lookup, registration, and integration with <see cref="TypeDescriptor"/>.
///</summary>
///<remarks>
public sealed class ConverterRegistry
{
    #region Fields
    private readonly ConcurrentDictionary<Type, System.ComponentModel.TypeConverter> _converters = new();
    #endregion

    #region Public methods
    ///<summary>
    ///Removes all registered converters.
    ///</summary>
    public void Clear() => _converters.Clear();

    ///<summary>
    ///Converts a value of type <typeparamref name="T"/> to the specified destination type using the registered (or
    ///default) converter.
    ///</summary>
    ///<typeparam name="T">The source type.</typeparam>
    ///<param name="value">The value to convert.</param>
    ///<param name="destinationType">The target type.</param>
    ///<param name="culture">
    ///The culture to use for conversion, or <c>null</c> for the current culture.
    ///</param>
    ///<returns>The converted value.</returns>
    ///<exception cref="NotSupportedException">The conversion is not supported.</exception>
    public object? ConvertFrom<T>(T value, Type destinationType, CultureInfo? culture = null)
    {
        ArgumentNullException.ThrowIfNull(destinationType);

        System.ComponentModel.TypeConverter converter = GetConverter<T>();
        return converter.ConvertTo(null, culture ?? CultureInfo.CurrentCulture, value, destinationType);
    }

    ///<summary>
    ///Converts the specified value to type <typeparamref name="T"/> using the registered (or default) converter.
    ///</summary>
    ///<typeparam name="T">The target type.</typeparam>
    ///<param name="value">The value to convert.</param>
    ///<param name="culture">
    ///The culture to use for conversion, or <c>null</c> for the current culture.
    ///</param>
    ///<returns>The converted value.</returns>
    ///<exception cref="NotSupportedException">The conversion is not supported.</exception>
    public T? ConvertTo<T>(object value, CultureInfo? culture = null)
    {
        System.ComponentModel.TypeConverter converter = GetConverter<T>();

        if(converter.CanConvertFrom(value.GetType()))
        {
            return (T?)converter.ConvertFrom(null, culture ?? CultureInfo.CurrentCulture, value);
        }

        throw new NotSupportedException($"Converter for '{typeof(T).Name}' cannot convert from '{value.GetType().Name}'.");
    }

    ///<summary>
    ///Gets the registered converter for type <typeparamref name="T"/>, falling back to <see
    ///cref="TypeDescriptor.GetConverter(Type)"/> if no explicit registration exists.
    ///</summary>
    ///<typeparam name="T">The type to get a converter for.</typeparam>
    ///<returns>The converter for the type.</returns>
    public System.ComponentModel.TypeConverter GetConverter<T>() => GetConverter(typeof(T));

    ///<summary>
    ///Gets the registered converter for the specified type, falling back to <see
    ///cref="TypeDescriptor.GetConverter(Type)"/> if no explicit registration exists.
    ///</summary>
    ///<param name="type">The type to get a converter for.</param>
    ///<returns>The converter for the type.</returns>
    ///<exception cref="ArgumentNullException">
    public System.ComponentModel.TypeConverter GetConverter(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        if(_converters.TryGetValue(type, out System.ComponentModel.TypeConverter? converter))
        {
            return converter;
        }

        return TypeDescriptor.GetConverter(type);
    }

    ///<summary>
    ///Gets a value indicating whether a converter is explicitly registered for the specified type.
    ///</summary>
    ///<typeparam name="T">The type to check.</typeparam>
    ///<returns><c>true</c> if a converter is registered; otherwise, <c>false</c>.</returns>
    public bool IsRegistered<T>() => _converters.ContainsKey(typeof(T));

    ///<summary>
    ///Gets a value indicating whether a converter is explicitly registered for the specified type.
    ///</summary>
    ///<param name="type">The type to check.</param>
    ///<returns><c>true</c> if a converter is registered; otherwise, <c>false</c>.</returns>
    public bool IsRegistered(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);
        return _converters.ContainsKey(type);
    }

    ///<summary>
    ///Registers a <see cref="System.ComponentModel.TypeConverter"/> for the specified type. Replaces any existing
    ///registration.
    ///</summary>
    ///<typeparam name="T">The type to register a converter for.</typeparam>
    ///<param name="converter">The converter to register.</param>
    ///<returns>This instance, for fluent chaining.</returns>
    ///<exception cref="ArgumentNullException">
    public ConverterRegistry Register<T>(System.ComponentModel.TypeConverter converter)
    {
        ArgumentNullException.ThrowIfNull(converter);
        _converters[typeof(T)] = converter;
        return this;
    }

    ///<summary>
    ///Registers a <see cref="System.ComponentModel.TypeConverter"/> for the specified type. Replaces any existing
    ///registration.
    ///</summary>
    ///<param name="type">The type to register a converter for.</param>
    ///<param name="converter">The converter to register.</param>
    ///<returns>This instance, for fluent chaining.</returns>
    ///<exception cref="ArgumentNullException">
    public ConverterRegistry Register(Type type, System.ComponentModel.TypeConverter converter)
    {
        ArgumentNullException.ThrowIfNull(type);
        ArgumentNullException.ThrowIfNull(converter);
        _converters[type] = converter;
        return this;
    }

    ///<summary>
    ///Registers a <see cref="GenericTypeConverter{T}"/> built from the specified delegates.
    ///</summary>
    ///<typeparam name="T">The type to register a converter for.</typeparam>
    ///<param name="convertFrom">
    ///A delegate that converts a source value to <typeparamref name="T"/>.
    ///</param>
    ///<param name="convertTo">
    ///A delegate that converts a <typeparamref name="T"/> value to a destination type.
    ///</param>
    ///<returns>This instance, for fluent chaining.</returns>
    public ConverterRegistry Register<T>(Func<ITypeDescriptorContext?, CultureInfo?, object, T?>? convertFrom = null, Func<ITypeDescriptorContext?, CultureInfo?, T, Type, object?>? convertTo = null)
    {
        _converters[typeof(T)] = new GenericTypeConverter<T>(convertFrom, convertTo);
        return this;
    }

    ///<summary>
    ///Registers an <see cref="EnumTypeConverter{TEnum}"/> for the specified enum type.
    ///</summary>
    ///<typeparam name="TEnum">The enum type to register.</typeparam>
    ///<returns>This instance, for fluent chaining.</returns>
    public ConverterRegistry RegisterEnum<TEnum>() where TEnum : struct, Enum
    {
        _converters[typeof(TEnum)] = new EnumTypeConverter<TEnum>();
        return this;
    }

    ///<summary>
    ///Registers a <see cref="SpanBasedTypeConverter{T}"/> built from the specified span-based delegates.
    ///</summary>
    ///<typeparam name="T">The type to register a converter for.</typeparam>
    ///<param name="tryParse">
    ///A delegate that attempts to parse <typeparamref name="T"/> from a character span.
    ///</param>
    ///<param name="tryFormat">
    ///An optional delegate that attempts to format <typeparamref name="T"/> into a character span.
    ///</param>
    ///<param name="context">Optional converter context.</param>
    ///<returns>This instance, for fluent chaining.</returns>
    ///<exception cref="ArgumentNullException">
    public ConverterRegistry RegisterSpanBased<T>(SpanParseDelegate<T> tryParse, SpanFormatDelegate<T>? tryFormat = null, ConverterContext? context = null)
    {
        ArgumentNullException.ThrowIfNull(tryParse);
        _converters[typeof(T)] = new SpanBasedTypeConverter<T>(tryParse, tryFormat, context);
        return this;
    }

    ///<summary>
    ///Attempts to convert the specified value to type <typeparamref name="T"/> using the registered (or default)
    ///converter.
    ///</summary>
    ///<typeparam name="T">The target type.</typeparam>
    ///<param name="value">The value to convert.</param>
    ///<param name="result">
    ///When this method returns, contains the converted value if successful; otherwise, the default value of
    public bool TryConvertTo<T>(object value, [MaybeNullWhen(false)] out T result, CultureInfo? culture = null)
    {
        try
        {
            result = ConvertTo<T>(value, culture)!;
            return result is not null;
        } catch
        {
            result = default;
            return false;
        }
    }

    ///<summary>
    ///Attempts to get the explicitly registered converter for the specified type.
    ///</summary>
    ///<param name="type">The type to look up.</param>
    ///<param name="converter">
    ///When this method returns, contains the converter if found; otherwise, <c>null</c>.
    ///</param>
    ///<returns><c>true</c> if an explicit registration was found; otherwise, <c>false</c>.</returns>
    public bool TryGetConverter(Type type, [NotNullWhen(true)] out System.ComponentModel.TypeConverter? converter)
    {
        ArgumentNullException.ThrowIfNull(type);
        return _converters.TryGetValue(type, out converter);
    }

    ///<summary>
    ///Removes the registered converter for the specified type.
    ///</summary>
    ///<typeparam name="T">The type to unregister.</typeparam>
    ///<returns>
    public bool Unregister<T>() => _converters.TryRemove(typeof(T), out _);

    ///<summary>
    ///Removes the registered converter for the specified type.
    ///</summary>
    ///<param name="type">The type to unregister.</param>
    ///<returns>
    public bool Unregister(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);
        return _converters.TryRemove(type, out _);
    }
    #endregion

    #region Public properties
    ///<summary>
    ///Gets the number of explicitly registered converters.
    ///</summary>
    public int Count => _converters.Count;
    #endregion
}
