using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Catharsis.ComponentModel.DTO;

///<summary>
///A <see cref="BindableRecord{T}"/> that exposes the DataAnnotations and <see cref="TypeDescriptor"/> metadata from
public class MetadataAnnotatedRecord<T>(T value) : BindableRecord<T>(value) where T : class
{
    #region Fields
    private readonly List<PropertyMetadataEntry> _metadata = BuildMetadata();
    #endregion

    #region Private methods
    private static List<PropertyMetadataEntry> BuildMetadata()
    {
        PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(typeof(T));
        List<PropertyMetadataEntry> entries = [ with(properties.Count) ];

        foreach(PropertyDescriptor prop in properties)
        {
            DisplayAttribute? displayAttr = prop.Attributes.OfType<DisplayAttribute>().FirstOrDefault();
            RequiredAttribute? requiredAttr = prop.Attributes.OfType<RequiredAttribute>().FirstOrDefault();

            entries.Add(
            new PropertyMetadataEntry(
            PropertyName: prop.Name,
            DisplayName: displayAttr?.GetName() ?? prop.DisplayName,
            Description: displayAttr?.GetDescription() ?? prop.Description,
            PropertyType: prop.PropertyType,
            IsReadOnly: prop.IsReadOnly,
            IsRequired: requiredAttr is not null,
            Category: prop.Category,
            Attributes: [ .. prop.Attributes.Cast<Attribute>() ]));
        }

        return entries;
    }
    #endregion

    #region Public methods
    ///<summary>
    ///Gets the metadata for the specified property.
    ///</summary>
    ///<param name="propertyName">The property name.</param>
    ///<returns>The metadata entry, or <c>null</c> if the property was not found.</returns>
    public PropertyMetadataEntry? GetPropertyMetadata(string propertyName)
    {
        ArgumentNullException.ThrowIfNull(propertyName);

        foreach(PropertyMetadataEntry entry in _metadata)
        {
            if(string.Equals(entry.PropertyName, propertyName, StringComparison.Ordinal))
            {
                return entry;
            }
        }

        return null;
    }
    #endregion

    #region Public properties
    ///<summary>
    ///Gets the metadata entries for all public properties of <typeparamref name="T"/>.
    ///</summary>
    public IReadOnlyList<PropertyMetadataEntry> AllPropertyMetadata => _metadata;
    #endregion
}
