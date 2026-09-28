using System.ComponentModel;

namespace Catharsis.ComponentModel;

///<summary>
///A dynamic <see cref="ICustomTypeDescriptor"/> that allows adding, removing, and replacing <see
///cref="PropertyDescriptor"/> entries at runtime. Useful for binding scenarios where the property shape of an object
///needs to be determined at runtime.
///</summary>
public class DynamicTypeDescriptor : CustomTypeDescriptor
{
    #region Fields
    readonly List<PropertyDescriptor> _properties = [];
    #endregion

    #region Constructors
    ///<summary>
    ///Initializes a new instance of <see cref="DynamicTypeDescriptor"/>.
    ///</summary>
    public DynamicTypeDescriptor()
    {
    }
    ///<summary>
    ///Initializes a new instance of <see cref="DynamicTypeDescriptor"/> that delegates to the specified parent
    ///descriptor for defaults.
    ///</summary>
    ///<param name="parent">The parent type descriptor.</param>
    public DynamicTypeDescriptor(ICustomTypeDescriptor? parent) : base(parent)
    {
    }
    #endregion

    #region Private methods
    static bool MatchesAttributes(PropertyDescriptor property, Attribute[]? attributes)
    {
        if((attributes is null) || (attributes.Length == 0))
        {
            return true;
        }

        foreach(Attribute attribute in attributes)
        {
            if(!property.Attributes.Contains(attribute))
            {
                return false;
            }
        }

        return true;
    }
    #endregion

    #region Public methods
    ///<summary>
    ///Adds a property descriptor to this type descriptor.
    ///</summary>
    ///<param name="property">The property descriptor to add.</param>
    ///<exception cref="ArgumentNullException"><paramref name="property"/> is <c>null</c>.</exception>
    ///<exception cref="ArgumentException">A property with the same name already exists.</exception>
    public void AddProperty(PropertyDescriptor property)
    {
        ArgumentNullException.ThrowIfNull(property);

        if(_properties.Any(p => p.Name == property.Name))
        {
            throw new ArgumentException($"A property named '{property.Name}' already exists.", nameof(property));
        }

        _properties.Add(property);
    }

    ///<inheritdoc/>
    public override PropertyDescriptorCollection GetProperties()
    {
        PropertyDescriptorCollection baseProperties = base.GetProperties();
        PropertyDescriptor[] merged = new PropertyDescriptor[baseProperties.Count + _properties.Count];

        baseProperties.CopyTo(merged, 0);
        _properties.CopyTo(merged, baseProperties.Count);

        return new PropertyDescriptorCollection(merged);
    }

    ///<inheritdoc/>
    public override PropertyDescriptorCollection GetProperties(Attribute[]? attributes)
    {
        PropertyDescriptorCollection baseProperties = base.GetProperties(attributes);
        PropertyDescriptor[] filtered = [.. _properties.Where(p => MatchesAttributes(p, attributes))];
        PropertyDescriptor[] merged = new PropertyDescriptor[baseProperties.Count + filtered.Length];

        baseProperties.CopyTo(merged, 0);
        filtered.CopyTo(merged, baseProperties.Count);

        return new PropertyDescriptorCollection(merged);
    }

    ///<summary>
    ///Removes a property descriptor by name.
    ///</summary>
    ///<param name="propertyName">The name of the property to remove.</param>
    ///<returns><c>true</c> if a property was removed; otherwise <c>false</c>.</returns>
    public bool RemoveProperty(string propertyName)
    {
        int index = _properties.FindIndex(p => p.Name == propertyName);

        if(index < 0)
        {
            return false;
        }

        _properties.RemoveAt(index);
        return true;
    }
    #endregion

    #region Public properties
    ///<summary>
    ///Gets the number of dynamic properties currently registered.
    ///</summary>
    public int PropertyCount => _properties.Count;
    #endregion
}
