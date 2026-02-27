using System.ComponentModel;
using System.Globalization;

namespace Catharsis.ComponentModel;

/// <summary>
/// Deserializes <see cref="Dictionary{TKey,TValue}"/> representations back
/// into component instances using <see cref="TypeDescriptor"/> property descriptors
/// and their associated <see cref="TypeConverter"/> instances.
/// </summary>
/// <remarks>
/// <para>
/// Each entry in the input dictionary is matched to a writable property on
/// the target component by name. The value is converted from its source type
/// (typically <see cref="string"/>) using the property's <see cref="TypeConverter"/>.
/// </para>
/// </remarks>
public sealed class ComponentModelDeserializer
{
    /// <summary>
    /// Gets or sets the culture used for type conversion. Defaults to
    /// <see cref="CultureInfo.InvariantCulture"/>.
    /// </summary>
    public CultureInfo Culture { get; set; } = CultureInfo.InvariantCulture;

    /// <summary>
    /// Gets or sets a value indicating whether missing properties in the
    /// target should be silently ignored. Defaults to <c>true</c>.
    /// </summary>
    public bool IgnoreMissingProperties { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether conversion errors should be
    /// silently ignored. Defaults to <c>false</c>.
    /// </summary>
    public bool IgnoreConversionErrors { get; set; }

    /// <summary>
    /// Deserializes a string dictionary into the specified target component.
    /// </summary>
    /// <typeparam name="T">The type of the target component.</typeparam>
    /// <param name="data">The serialized property data.</param>
    /// <param name="target">The target component to populate.</param>
    /// <returns>The populated <paramref name="target"/>.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="data"/> or <paramref name="target"/> is <c>null</c>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// A key in <paramref name="data"/> does not match a writable property and
    /// <see cref="IgnoreMissingProperties"/> is <c>false</c>.
    /// </exception>
    public T Deserialize<T>(IReadOnlyDictionary<string, string?> data, T target) where T : notnull
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(target);

        var properties = TypeDescriptor.GetProperties(target);

        foreach (var (key, stringValue) in data)
        {
            var property = properties[key];

            if (property is null || property.IsReadOnly)
            {
                if (!IgnoreMissingProperties)
                {
                    throw new InvalidOperationException(
                        $"Property '{key}' does not exist or is read-only on type '{typeof(T).Name}'.");
                }

                continue;
            }

            ApplyValue(property, target, stringValue);
        }

        return target;
    }

    /// <summary>
    /// Deserializes a raw value dictionary into the specified target component.
    /// </summary>
    /// <typeparam name="T">The type of the target component.</typeparam>
    /// <param name="data">The property data with raw values.</param>
    /// <param name="target">The target component to populate.</param>
    /// <returns>The populated <paramref name="target"/>.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="data"/> or <paramref name="target"/> is <c>null</c>.
    /// </exception>
    public T DeserializeRaw<T>(IReadOnlyDictionary<string, object?> data, T target) where T : notnull
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(target);

        var properties = TypeDescriptor.GetProperties(target);

        foreach (var (key, value) in data)
        {
            var property = properties[key];

            if (property is null || property.IsReadOnly)
            {
                if (!IgnoreMissingProperties)
                {
                    throw new InvalidOperationException(
                        $"Property '{key}' does not exist or is read-only on type '{typeof(T).Name}'.");
                }

                continue;
            }

            ApplyRawValue(property, target, value);
        }

        return target;
    }

    /// <summary>
    /// Creates a new instance of <typeparamref name="T"/> and deserializes
    /// the string dictionary into it.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the target component. Must have a parameterless constructor.
    /// </typeparam>
    /// <param name="data">The serialized property data.</param>
    /// <returns>A new instance of <typeparamref name="T"/> populated from <paramref name="data"/>.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="data"/> is <c>null</c>.
    /// </exception>
    public T Deserialize<T>(IReadOnlyDictionary<string, string?> data) where T : notnull, new()
    {
        ArgumentNullException.ThrowIfNull(data);
        return Deserialize(data, new T());
    }

    private void ApplyValue(PropertyDescriptor property, object target, string? stringValue)
    {
        try
        {
            if (stringValue is null)
            {
                property.SetValue(target, null);
                return;
            }

            var converter = property.Converter;

            if (converter.CanConvertFrom(typeof(string)))
            {
                var converted = converter.ConvertFromString(null, Culture, stringValue);
                property.SetValue(target, converted);
            }
            else if (!IgnoreConversionErrors)
            {
                throw new InvalidOperationException(
                    $"Cannot convert string to '{property.PropertyType.Name}' for property '{property.Name}'.");
            }
        }
        catch (Exception) when (IgnoreConversionErrors)
        {
            // Silently skip conversion errors when configured to do so.
        }
    }

    private void ApplyRawValue(PropertyDescriptor property, object target, object? value)
    {
        try
        {
            if (value is null || property.PropertyType.IsInstanceOfType(value))
            {
                property.SetValue(target, value);
                return;
            }

            var converter = property.Converter;

            if (converter.CanConvertFrom(value.GetType()))
            {
                var converted = converter.ConvertFrom(null, Culture, value);
                property.SetValue(target, converted);
            }
            else if (!IgnoreConversionErrors)
            {
                throw new InvalidOperationException(
                    $"Cannot convert '{value.GetType().Name}' to '{property.PropertyType.Name}' " +
                    $"for property '{property.Name}'.");
            }
        }
        catch (Exception) when (IgnoreConversionErrors)
        {
            // Silently skip conversion errors when configured to do so.
        }
    }
}
