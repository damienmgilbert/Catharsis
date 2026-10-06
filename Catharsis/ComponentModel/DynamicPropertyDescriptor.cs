using System.ComponentModel;

namespace Catharsis.ComponentModel;

///<summary>
///A <see cref="PropertyDescriptor"/> implementation that uses delegates for get and set operations, supporting dynamic
///property definitions without requiring compile-time property accessors.
///</summary>
///<remarks>
///<para> The getter and setter delegates receive the component instance and operate on it directly. If no setter is
///provided, the property is treated as read-only.</para> <para> Use <see cref="WithMergedAttributes"/> to produce a
///new descriptor with additional attributes merged onto the existing set.</para>
///</remarks>
public sealed class DynamicPropertyDescriptor : PropertyDescriptor
{
    #region Fields
    readonly Type _componentType;
    readonly object? _defaultValue;
    readonly Func<object, object?> _getter;
    readonly Type _propertyType;
    readonly Action<object, object?>? _setter;
    #endregion

    #region Constructors
    ///<summary>
    ///Initializes a new instance of <see cref="DynamicPropertyDescriptor"/> from <see cref="PropertyMetadata"/>
    ///and delegate accessors.
    ///</summary>
    ///<param name="metadata">The property metadata.</param>
    ///<param name="getter">A delegate that retrieves the property value.</param>
    ///<param name="setter">
    ///An optional delegate that sets the property value. If <c>null</c>, the property is read-only.
    ///</param>
    ///<exception cref="ArgumentNullException">
    ///<paramref name="metadata"/> or <paramref name="getter"/> is <c>null</c>.
    ///</exception>
    public DynamicPropertyDescriptor(PropertyMetadata metadata, Func<object, object?> getter, Action<object, object?>? setter = null) : base(metadata?.Name ?? throw new ArgumentNullException(nameof(metadata)), ToAttributeArray(metadata.Attributes))
    {
        ArgumentNullException.ThrowIfNull(getter);

        _propertyType = metadata.PropertyType;
        _componentType = metadata.ComponentType;
        _getter = getter;
        _setter = metadata.IsReadOnly ? null : setter;
        _defaultValue = metadata.DefaultValue;
    }

    ///<summary>
    ///Initializes a new instance of <see cref="DynamicPropertyDescriptor"/>.
    ///</summary>
    ///<param name="name">The property name.</param>
    ///<param name="propertyType">The CLR type of the property value.</param>
    ///<param name="componentType">The type that owns this property.</param>
    ///<param name="getter">A delegate that retrieves the property value from a component.</param>
    ///<param name="setter">
    ///An optional delegate that sets the property value on a component. If <c>null</c>, the property is read-only.
    ///</param>
    ///<param name="defaultValue">The default value for the property.</param>
    ///<param name="attributes">Optional attributes for the property.</param>
    ///<exception cref="ArgumentNullException">
    ///<paramref name="propertyType"/>, <paramref name="componentType"/>, or <paramref name="getter"/> is <c>null</c>.
    ///</exception>
    public DynamicPropertyDescriptor(string name, Type propertyType, Type componentType, Func<object, object?> getter, Action<object, object?>? setter = null, object? defaultValue = null, params Attribute[] attributes) : base(name, attributes)
    {
        ArgumentNullException.ThrowIfNull(propertyType);
        ArgumentNullException.ThrowIfNull(componentType);
        ArgumentNullException.ThrowIfNull(getter);

        _propertyType = propertyType;
        _componentType = componentType;
        _getter = getter;
        _setter = setter;
        _defaultValue = defaultValue;
    }
    #endregion

    #region Private methods
    static Attribute[] ToAttributeArray(AttributeCollection collection)
    {
        Attribute[] result = new Attribute[collection.Count];

        for (int i = 0; i < collection.Count; i++)
        {
            result[i] = collection[i];
        }

        return result;
    }
    #endregion

    #region Public methods
    ///<inheritdoc/>
    public override bool CanResetValue(object component) { return _defaultValue is not null; }

    ///<inheritdoc/>
    public override object? GetValue(object? component)
    {
        ArgumentNullException.ThrowIfNull(component);
        return _getter(component);
    }

    ///<inheritdoc/>
    public override void ResetValue(object component)
    {
        if ((_defaultValue is not null) && (_setter is not null))
        {
            SetValue(component, _defaultValue);
        }
    }

    ///<inheritdoc/>
    public override void SetValue(object? component, object? value)
    {
        ArgumentNullException.ThrowIfNull(component);

        if (_setter is null)
        {
            throw new InvalidOperationException($"Property '{Name}' is read-only.");
        }

        _setter(component, value);
        OnValueChanged(component, EventArgs.Empty);
    }

    ///<inheritdoc/>
    public override bool ShouldSerializeValue(object component)
    {
        if (_defaultValue is null)
        {
            return true;
        }

        object? current = GetValue(component);
        return !Equals(current, _defaultValue);
    }

    ///<summary>
    ///Creates a new <see cref="DynamicPropertyDescriptor"/> with the specified attributes merged onto the existing
    ///attribute set.
    ///</summary>
    ///<param name="additionalAttributes">The attributes to merge.</param>
    ///<returns>A new descriptor with the merged attributes.</returns>
    public DynamicPropertyDescriptor WithMergedAttributes(params Attribute[] additionalAttributes)
    {
        AttributeCollectionBuilder builder = new AttributeCollectionBuilder(Attributes)
            .Merge(additionalAttributes);

        return new DynamicPropertyDescriptor(Name, _propertyType, _componentType, _getter, _setter, _defaultValue, ToAttributeArray(builder.Build()));
    }
    #endregion

    #region Public properties
    ///<inheritdoc/>
    public override Type ComponentType => _componentType;

    ///<inheritdoc/>
    public override bool IsReadOnly => _setter is null;

    ///<inheritdoc/>
    public override Type PropertyType => _propertyType;
    #endregion
}
