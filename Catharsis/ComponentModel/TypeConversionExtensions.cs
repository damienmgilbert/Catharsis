using System.ComponentModel;

namespace Catharsis.ComponentModel;

/// <summary>
/// Extension methods that leverage <see cref="TypeDescriptor"/>,
/// <see cref="TypeConverter"/>, and <see cref="PropertyDescriptor"/>
/// to simplify common type-conversion and property-inspection tasks.
/// </summary>
public static class TypeConversionExtensions
{
    /// <summary>
    /// Converts the specified value to type <typeparamref name="TTarget"/>
    /// using the <see cref="TypeConverter"/> registered for the target type.
    /// </summary>
    /// <typeparam name="TTarget">The target type to convert to.</typeparam>
    /// <param name="value">The value to convert.</param>
    /// <returns>The converted value.</returns>
    /// <exception cref="NotSupportedException">The conversion is not supported.</exception>
    public static TTarget? ConvertTo<TTarget>(this object? value)
    {
        if (value is null)
            return default;

        if (value is TTarget typed)
            return typed;

        var converter = TypeDescriptor.GetConverter(typeof(TTarget));

        if (converter.CanConvertFrom(value.GetType()))
            return (TTarget?)converter.ConvertFrom(value);

        var sourceConverter = TypeDescriptor.GetConverter(value.GetType());

        if (sourceConverter.CanConvertTo(typeof(TTarget)))
            return (TTarget?)sourceConverter.ConvertTo(value, typeof(TTarget));

        throw new NotSupportedException(
            $"Cannot convert from '{value.GetType().Name}' to '{typeof(TTarget).Name}'.");
    }

    /// <summary>
    /// Attempts to convert the specified value to type <typeparamref name="TTarget"/>
    /// using registered <see cref="TypeConverter"/> instances.
    /// </summary>
    /// <typeparam name="TTarget">The target type to convert to.</typeparam>
    /// <param name="value">The value to convert.</param>
    /// <param name="result">
    /// When this method returns, contains the converted value if successful;
    /// otherwise the default value for <typeparamref name="TTarget"/>.
    /// </param>
    /// <returns><c>true</c> if the conversion succeeded; otherwise <c>false</c>.</returns>
    public static bool TryConvertTo<TTarget>(this object? value, out TTarget? result)
    {
        try
        {
            result = value.ConvertTo<TTarget>();
            return true;
        }
        catch
        {
            result = default;
            return false;
        }
    }

    /// <summary>
    /// Gets the <see cref="TypeConverter"/> for the specified type using
    /// <see cref="TypeDescriptor.GetConverter(Type)"/>.
    /// </summary>
    /// <param name="type">The type whose converter to retrieve.</param>
    /// <returns>The <see cref="TypeConverter"/> for the type.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="type"/> is <c>null</c>.</exception>
    public static TypeConverter GetTypeConverter(this Type type)
    {
        ArgumentNullException.ThrowIfNull(type);
        return TypeDescriptor.GetConverter(type);
    }

    /// <summary>
    /// Gets all browsable property descriptors for the specified object
    /// using <see cref="TypeDescriptor.GetProperties(object)"/>.
    /// </summary>
    /// <param name="component">The object whose properties to retrieve.</param>
    /// <returns>An enumerable of <see cref="PropertyDescriptor"/> instances.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="component"/> is <c>null</c>.</exception>
    public static IEnumerable<PropertyDescriptor> GetBrowsableProperties(this object component)
    {
        ArgumentNullException.ThrowIfNull(component);

        return TypeDescriptor
            .GetProperties(component)
            .Cast<PropertyDescriptor>()
            .Where(p => p.IsBrowsable);
    }

    /// <summary>
    /// Gets a property descriptor by name for the specified component,
    /// or <c>null</c> if the property does not exist.
    /// </summary>
    /// <param name="component">The object whose property to retrieve.</param>
    /// <param name="propertyName">The name of the property.</param>
    /// <returns>
    /// The <see cref="PropertyDescriptor"/> for the property, or <c>null</c> if not found.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="component"/> or <paramref name="propertyName"/> is <c>null</c>.
    /// </exception>
    public static PropertyDescriptor? GetPropertyDescriptor(this object component, string propertyName)
    {
        ArgumentNullException.ThrowIfNull(component);
        ArgumentNullException.ThrowIfNull(propertyName);

        return TypeDescriptor
            .GetProperties(component)
            .Cast<PropertyDescriptor>()
            .FirstOrDefault(p => p.Name == propertyName);
    }

    /// <summary>
    /// Gets the display name for the specified component, using the
    /// <see cref="DisplayNameAttribute"/> if present, or the type name otherwise.
    /// </summary>
    /// <param name="component">The component whose display name to retrieve.</param>
    /// <returns>The display name string.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="component"/> is <c>null</c>.</exception>
    public static string GetComponentDisplayName(this object component)
    {
        ArgumentNullException.ThrowIfNull(component);

        var attribute = TypeDescriptor.GetAttributes(component)
            .OfType<DisplayNameAttribute>()
            .FirstOrDefault();

        return attribute is not null && !string.IsNullOrEmpty(attribute.DisplayName)
            ? attribute.DisplayName
            : component.GetType().Name;
    }

    /// <summary>
    /// Gets the description for the specified component, using the
    /// <see cref="DescriptionAttribute"/> if present.
    /// </summary>
    /// <param name="component">The component whose description to retrieve.</param>
    /// <returns>The description string, or <see cref="string.Empty"/> if none is set.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="component"/> is <c>null</c>.</exception>
    public static string GetComponentDescription(this object component)
    {
        ArgumentNullException.ThrowIfNull(component);

        var attribute = TypeDescriptor.GetAttributes(component)
            .OfType<DescriptionAttribute>()
            .FirstOrDefault();

        return attribute?.Description ?? string.Empty;
    }

    /// <summary>
    /// Gets the category for the specified component, using the
    /// <see cref="CategoryAttribute"/> if present.
    /// </summary>
    /// <param name="component">The component whose category to retrieve.</param>
    /// <returns>The category string, or <see cref="string.Empty"/> if none is set.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="component"/> is <c>null</c>.</exception>
    public static string GetComponentCategory(this object component)
    {
        ArgumentNullException.ThrowIfNull(component);

        var attribute = TypeDescriptor.GetAttributes(component)
            .OfType<CategoryAttribute>()
            .FirstOrDefault();

        return attribute?.Category ?? string.Empty;
    }
}
