using System.ComponentModel;

namespace Catharsis.ComponentModel;

/// <summary>
/// A dynamic <see cref="ICustomTypeDescriptor"/> that allows adding, removing,
/// and replacing <see cref="PropertyDescriptor"/> entries at runtime.
/// Useful for binding scenarios where the property shape of an object
/// needs to be determined at runtime.
/// </summary>
public class DynamicTypeDescriptor : CustomTypeDescriptor
{
    private readonly List<PropertyDescriptor> _properties = [];

    /// <summary>
    /// Initializes a new instance of <see cref="DynamicTypeDescriptor"/>.
    /// </summary>
    public DynamicTypeDescriptor() { }

    /// <summary>
    /// Initializes a new instance of <see cref="DynamicTypeDescriptor"/>
    /// that delegates to the specified parent descriptor for defaults.
    /// </summary>
    /// <param name="parent">The parent type descriptor.</param>
    public DynamicTypeDescriptor(ICustomTypeDescriptor? parent)
        : base(parent) { }

    /// <summary>
    /// Adds a property descriptor to this type descriptor.
    /// </summary>
    /// <param name="property">The property descriptor to add.</param>
    /// <exception cref="ArgumentNullException"><paramref name="property"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">A property with the same name already exists.</exception>
    public void AddProperty(PropertyDescriptor property)
    {
        ArgumentNullException.ThrowIfNull(property);

        if (_properties.Any(p => p.Name == property.Name))
            throw new ArgumentException($"A property named '{property.Name}' already exists.", nameof(property));

        _properties.Add(property);
    }

    /// <summary>
    /// Removes a property descriptor by name.
    /// </summary>
    /// <param name="propertyName">The name of the property to remove.</param>
    /// <returns><c>true</c> if a property was removed; otherwise <c>false</c>.</returns>
    public bool RemoveProperty(string propertyName)
    {
        var index = _properties.FindIndex(p => p.Name == propertyName);

        if (index < 0)
            return false;

        _properties.RemoveAt(index);
        return true;
    }

    /// <summary>
    /// Gets the number of dynamic properties currently registered.
    /// </summary>
    public int PropertyCount => _properties.Count;

    /// <inheritdoc />
    public override PropertyDescriptorCollection GetProperties()
    {
        var baseProperties = base.GetProperties();
        var merged = new PropertyDescriptor[baseProperties.Count + _properties.Count];

        baseProperties.CopyTo(merged, 0);
        _properties.CopyTo(merged, baseProperties.Count);

        return new PropertyDescriptorCollection(merged);
    }

    /// <inheritdoc />
    public override PropertyDescriptorCollection GetProperties(Attribute[]? attributes)
    {
        var baseProperties = base.GetProperties(attributes);
        var filtered = _properties.Where(p => MatchesAttributes(p, attributes)).ToArray();
        var merged = new PropertyDescriptor[baseProperties.Count + filtered.Length];

        baseProperties.CopyTo(merged, 0);
        filtered.CopyTo(merged, baseProperties.Count);

        return new PropertyDescriptorCollection(merged);
    }

    private static bool MatchesAttributes(PropertyDescriptor property, Attribute[]? attributes)
    {
        if (attributes is null || attributes.Length == 0)
            return true;

        foreach (var attribute in attributes)
        {
            if (!property.Attributes.Contains(attribute))
                return false;
        }

        return true;
    }
}

/// <summary>
/// A simple <see cref="PropertyDescriptor"/> backed by a dictionary of values,
/// intended for use with <see cref="DynamicTypeDescriptor"/>.
/// </summary>
public sealed class DictionaryPropertyDescriptor : PropertyDescriptor
{
    private readonly IDictionary<string, object?> _store;
    private readonly Type _propertyType;

    /// <summary>
    /// Initializes a new instance of <see cref="DictionaryPropertyDescriptor"/>.
    /// </summary>
    /// <param name="name">The property name.</param>
    /// <param name="propertyType">The type of the property value.</param>
    /// <param name="store">The backing dictionary for get/set operations.</param>
    /// <param name="attributes">Optional attributes for the property.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="store"/> or <paramref name="propertyType"/> is <c>null</c>.
    /// </exception>
    public DictionaryPropertyDescriptor(
        string name,
        Type propertyType,
        IDictionary<string, object?> store,
        params Attribute[] attributes)
        : base(name, attributes)
    {
        _propertyType = propertyType ?? throw new ArgumentNullException(nameof(propertyType));
        _store = store ?? throw new ArgumentNullException(nameof(store));
    }

    /// <inheritdoc />
    public override Type ComponentType => typeof(DynamicTypeDescriptor);

    /// <inheritdoc />
    public override bool IsReadOnly => false;

    /// <inheritdoc />
    public override Type PropertyType => _propertyType;

    /// <inheritdoc />
    public override bool CanResetValue(object component) => true;

    /// <inheritdoc />
    public override object? GetValue(object? component) =>
        _store.TryGetValue(Name, out var value) ? value : null;

    /// <inheritdoc />
    public override void SetValue(object? component, object? value) =>
        _store[Name] = value;

    /// <inheritdoc />
    public override void ResetValue(object component) =>
        _store.Remove(Name);

    /// <inheritdoc />
    public override bool ShouldSerializeValue(object component) =>
        _store.ContainsKey(Name);
}
