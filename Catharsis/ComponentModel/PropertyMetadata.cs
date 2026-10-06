using System.ComponentModel;

namespace Catharsis.ComponentModel;

///<summary>
///Immutable metadata describing a property for use with dynamic descriptor infrastructure. Captures the property name,
///type, owning component type, read-only state, and associated attributes.
///</summary>
///<remarks>
///Use <see cref="PropertyMetadata"/> to register property shapes in a <see cref="ComponentMetadataRegistry"/> without
///requiring reflection at descriptor-creation time.
///</remarks>
public sealed class PropertyMetadata
{
    #region Constructors

    ///<summary>
    ///Initializes a new instance of <see cref="PropertyMetadata"/>.
    ///</summary>
    ///<param name="name">The property name.</param>
    ///<param name="propertyType">The CLR type of the property value.</param>
    ///<param name="componentType">The type that owns this property.</param>
    ///<param name="isReadOnly">Whether the property is read-only.</param>
    ///<param name="defaultValue">The default value for the property, or <c>null</c>.</param>
    ///<param name="attributes">Optional attributes to associate with the property.</param>
    ///<exception cref="ArgumentException"><paramref name="name"/> is null or whitespace.</exception>
    ///<exception cref="ArgumentNullException">
    public PropertyMetadata(string name, Type propertyType, Type componentType, bool isReadOnly = false, object? defaultValue = null, params Attribute[] attributes)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(propertyType);
        ArgumentNullException.ThrowIfNull(componentType);

        Name = name;
        PropertyType = propertyType;
        ComponentType = componentType;
        IsReadOnly = isReadOnly;
        DefaultValue = defaultValue;
        Attributes = (attributes.Length > 0) ? (new AttributeCollection(attributes)) : AttributeCollection.Empty;
    }
    #endregion

    #region Public methods
    ///<summary>
    ///Returns a string representation containing the property name and type.
    ///</summary>
    public override string ToString() => $"{Name} ({PropertyType.Name})";
    #endregion

    #region Public properties
    ///<summary>
    ///Gets the attributes associated with this property.
    ///</summary>
    public AttributeCollection Attributes { get; }

    ///<summary>
    ///Gets the type that owns this property.
    ///</summary>
    public Type ComponentType { get; }

    ///<summary>
    ///Gets the default value for the property, or <c>null</c>.
    ///</summary>
    public object? DefaultValue { get; }

    ///<summary>
    ///Gets a value indicating whether the property is read-only.
    ///</summary>
    public bool IsReadOnly { get; }

    ///<summary>
    ///Gets the property name.
    ///</summary>
    public string Name { get; }

    ///<summary>
    ///Gets the CLR type of the property value.
    ///</summary>
    public Type PropertyType { get; }
    #endregion
}
