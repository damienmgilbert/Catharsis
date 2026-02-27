using System.ComponentModel;
using System.Globalization;

namespace Catharsis.ComponentModel;

/// <summary>
/// Serializes component instances to <see cref="Dictionary{TKey,TValue}"/>
/// representations using <see cref="TypeDescriptor"/> property descriptors
/// and their associated <see cref="TypeConverter"/> instances.
/// </summary>
/// <remarks>
/// <para>
/// Each readable property whose <see cref="TypeConverter"/> supports conversion
/// to <see cref="string"/> is included in the output dictionary. Properties that
/// are read-only, have no string converter, or match the default value (when
/// <see cref="SkipDefaultValues"/> is <c>true</c>) are excluded.
/// </para>
/// </remarks>
public sealed class ComponentModelSerializer
{
    /// <summary>
    /// Gets or sets a value indicating whether properties at their default
    /// value should be omitted from the serialized output. Defaults to <c>false</c>.
    /// </summary>
    public bool SkipDefaultValues { get; set; }

    /// <summary>
    /// Gets or sets the culture used for type conversion. Defaults to
    /// <see cref="CultureInfo.InvariantCulture"/>.
    /// </summary>
    public CultureInfo Culture { get; set; } = CultureInfo.InvariantCulture;

    /// <summary>
    /// Gets or sets a predicate that filters which properties are serialized.
    /// If <c>null</c>, all eligible properties are included.
    /// </summary>
    public Func<PropertyDescriptor, bool>? PropertyFilter { get; set; }

    /// <summary>
    /// Serializes the specified component to a string dictionary using
    /// <see cref="TypeDescriptor"/> property descriptors.
    /// </summary>
    /// <param name="component">The component to serialize.</param>
    /// <returns>
    /// A dictionary mapping property names to their string-converted values.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="component"/> is <c>null</c>.
    /// </exception>
    public Dictionary<string, string?> Serialize(object component)
    {
        ArgumentNullException.ThrowIfNull(component);

        var properties = TypeDescriptor.GetProperties(component);
        var result = new Dictionary<string, string?>(properties.Count, StringComparer.Ordinal);

        foreach (PropertyDescriptor property in properties)
        {
            if (!ShouldSerialize(property, component))
                continue;

            var value = property.GetValue(component);
            var converter = property.Converter;

            var stringValue = value is null
                ? null
                : converter.CanConvertTo(typeof(string))
                    ? converter.ConvertToString(null, Culture, value)
                    : value.ToString();

            result[property.Name] = stringValue;
        }

        return result;
    }

    /// <summary>
    /// Serializes the specified component to a dictionary of typed values
    /// (no string conversion) using <see cref="TypeDescriptor"/> property descriptors.
    /// </summary>
    /// <param name="component">The component to serialize.</param>
    /// <returns>
    /// A dictionary mapping property names to their raw values.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="component"/> is <c>null</c>.
    /// </exception>
    public Dictionary<string, object?> SerializeRaw(object component)
    {
        ArgumentNullException.ThrowIfNull(component);

        var properties = TypeDescriptor.GetProperties(component);
        var result = new Dictionary<string, object?>(properties.Count, StringComparer.Ordinal);

        foreach (PropertyDescriptor property in properties)
        {
            if (!ShouldSerialize(property, component))
                continue;

            result[property.Name] = property.GetValue(component);
        }

        return result;
    }

    private bool ShouldSerialize(PropertyDescriptor property, object component)
    {
        if (PropertyFilter is not null && !PropertyFilter(property))
            return false;

        if (SkipDefaultValues && !property.ShouldSerializeValue(component))
            return false;

        return true;
    }
}
