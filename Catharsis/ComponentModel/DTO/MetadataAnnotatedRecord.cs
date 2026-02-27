using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Catharsis.ComponentModel.DTO;

/// <summary>
/// A <see cref="BindableRecord{T}"/> that exposes the DataAnnotations and
/// <see cref="TypeDescriptor"/> metadata from <typeparamref name="T"/> in a
/// queryable form, suitable for building dynamic UI from record attributes.
/// </summary>
/// <typeparam name="T">The record type whose metadata to surface.</typeparam>
/// <remarks>
/// <para>
/// Property metadata is read once at construction time and cached. Use
/// <see cref="GetPropertyMetadata"/> to query attributes for a specific
/// property, or <see cref="AllPropertyMetadata"/> for the full set.
/// </para>
/// </remarks>
public class MetadataAnnotatedRecord<T> : BindableRecord<T>
    where T : class
{
    private readonly IReadOnlyList<PropertyMetadataEntry> _metadata;

    /// <summary>
    /// Initializes a new instance of <see cref="MetadataAnnotatedRecord{T}"/>.
    /// </summary>
    /// <param name="value">The initial record value.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="value"/> is <c>null</c>.
    /// </exception>
    public MetadataAnnotatedRecord(T value) : base(value)
    {
        _metadata = BuildMetadata();
    }

    /// <summary>
    /// Gets the metadata entries for all public properties of <typeparamref name="T"/>.
    /// </summary>
    public IReadOnlyList<PropertyMetadataEntry> AllPropertyMetadata => _metadata;

    /// <summary>
    /// Gets the metadata for the specified property.
    /// </summary>
    /// <param name="propertyName">The property name.</param>
    /// <returns>The metadata entry, or <c>null</c> if the property was not found.</returns>
    public PropertyMetadataEntry? GetPropertyMetadata(string propertyName)
    {
        ArgumentNullException.ThrowIfNull(propertyName);

        foreach (var entry in _metadata)
        {
            if (string.Equals(entry.PropertyName, propertyName, StringComparison.Ordinal))
                return entry;
        }

        return null;
    }

    private static IReadOnlyList<PropertyMetadataEntry> BuildMetadata()
    {
        var properties = TypeDescriptor.GetProperties(typeof(T));
        var entries = new List<PropertyMetadataEntry>(properties.Count);

        foreach (PropertyDescriptor prop in properties)
        {
            var displayAttr = prop.Attributes.OfType<DisplayAttribute>().FirstOrDefault();
            var requiredAttr = prop.Attributes.OfType<RequiredAttribute>().FirstOrDefault();

            entries.Add(new PropertyMetadataEntry(
                PropertyName: prop.Name,
                DisplayName: displayAttr?.GetName() ?? prop.DisplayName,
                Description: displayAttr?.GetDescription() ?? prop.Description,
                PropertyType: prop.PropertyType,
                IsReadOnly: prop.IsReadOnly,
                IsRequired: requiredAttr is not null,
                Category: prop.Category,
                Attributes: prop.Attributes.Cast<Attribute>().ToArray()));
        }

        return entries;
    }
}

/// <summary>
/// An immutable record containing metadata for a single property,
/// combining DataAnnotations and <see cref="TypeDescriptor"/> information.
/// </summary>
/// <param name="PropertyName">The property name.</param>
/// <param name="DisplayName">The display-friendly name.</param>
/// <param name="Description">The property description, or <c>null</c>.</param>
/// <param name="PropertyType">The CLR type of the property.</param>
/// <param name="IsReadOnly">Whether the property is read-only.</param>
/// <param name="IsRequired">Whether the property is required.</param>
/// <param name="Category">The category for grouping.</param>
/// <param name="Attributes">All attributes applied to the property.</param>
public sealed record PropertyMetadataEntry(
    string PropertyName,
    string DisplayName,
    string? Description,
    Type PropertyType,
    bool IsReadOnly,
    bool IsRequired,
    string? Category,
    IReadOnlyList<Attribute> Attributes);
