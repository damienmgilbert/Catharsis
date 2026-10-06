using System.ComponentModel;
using System.Globalization;

namespace Catharsis.ComponentModel;

///<summary>
///Serializes component instances to <see cref="Dictionary{TKey,TValue}"/> representations using <see
///cref="TypeDescriptor"/> property descriptors and their associated <see cref="TypeConverter"/> instances.
///</summary>
///<remarks>
public sealed class ComponentModelSerializer
{
    #region Private methods
    private bool ShouldSerialize(PropertyDescriptor property, object component)
    {
        if((PropertyFilter is not null) && !PropertyFilter(property))
        {
            return false;
        }

        if(SkipDefaultValues && !property.ShouldSerializeValue(component))
        {
            return false;
        }

        return true;
    }
    #endregion

    #region Public methods
    ///<summary>
    ///Serializes the specified component to a string dictionary using <see cref="TypeDescriptor"/> property
    ///descriptors.
    ///</summary>
    ///<param name="component">The component to serialize.</param>
    ///<returns>
    ///A dictionary mapping property names to their string-converted values.
    ///</returns>
    ///<exception cref="ArgumentNullException">
    public Dictionary<string, string?> Serialize(object component)
    {
        ArgumentNullException.ThrowIfNull(component);

        PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(component);
        Dictionary<string, string?> result = new(properties.Count, StringComparer.Ordinal);

        foreach(PropertyDescriptor property in properties)
        {
            if(!ShouldSerialize(property, component))
            {
                continue;
            }

            object? value = property.GetValue(component);
            System.ComponentModel.TypeConverter converter = property.Converter;

            string? stringValue = (value is null) ? null : (converter.CanConvertTo(typeof(string)) ? converter.ConvertToString(null, Culture, value) : value.ToString());

            result[property.Name] = stringValue;
        }

        return result;
    }

    ///<summary>
    ///Serializes the specified component to a dictionary of typed values (no string conversion) using <see
    ///cref="TypeDescriptor"/> property descriptors.
    ///</summary>
    ///<param name="component">The component to serialize.</param>
    ///<returns>
    ///A dictionary mapping property names to their raw values.
    ///</returns>
    ///<exception cref="ArgumentNullException">
    public Dictionary<string, object?> SerializeRaw(object component)
    {
        ArgumentNullException.ThrowIfNull(component);

        PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(component);
        Dictionary<string, object?> result = new(properties.Count, StringComparer.Ordinal);

        foreach(PropertyDescriptor property in properties)
        {
            if(!ShouldSerialize(property, component))
            {
                continue;
            }

            result[property.Name] = property.GetValue(component);
        }

        return result;
    }
    #endregion

    #region Public properties
    ///<summary>
    ///Gets or sets the culture used for type conversion. Defaults to <see cref="CultureInfo.InvariantCulture"/>.
    ///</summary>
    public CultureInfo Culture { get; set; } = CultureInfo.InvariantCulture;

    ///<summary>
    ///Gets or sets a predicate that filters which properties are serialized. If <c>null</c>, all eligible properties
    ///are included.
    ///</summary>
    public Func<PropertyDescriptor, bool>? PropertyFilter { get; set; }

    ///<summary>
    ///Gets or sets a value indicating whether properties at their default value should be omitted from the serialized
    ///output. Defaults to <c>false</c>.
    ///</summary>
    public bool SkipDefaultValues { get; set; }
    #endregion
}
