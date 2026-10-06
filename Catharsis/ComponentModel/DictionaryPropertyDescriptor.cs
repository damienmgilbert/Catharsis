using System.ComponentModel;

namespace Catharsis.ComponentModel;

///<summary>
///A simple <see cref="PropertyDescriptor"/> backed by a dictionary of values, intended for use with <see
///cref="DynamicTypeDescriptor"/>.
///</summary>
///<remarks>
///Initializes a new instance of <see cref="DictionaryPropertyDescriptor"/>.
///</remarks>
///<param name="name">The property name.</param>
///<param name="propertyType">The type of the property value.</param>
///<param name="store">The backing dictionary for get/set operations.</param>
///<param name="attributes">Optional attributes for the property.</param>
///<exception cref="ArgumentNullException">
public sealed class DictionaryPropertyDescriptor(string name, Type propertyType, IDictionary<string, object?> store, params Attribute[] attributes) : PropertyDescriptor(name, attributes)
{
    #region Fields
    private readonly Type _propertyType = propertyType ?? throw new ArgumentNullException(nameof(propertyType));
    private readonly IDictionary<string, object?> _store = store ?? throw new ArgumentNullException(nameof(store));
    #endregion

    #region Public methods
    ///<inheritdoc/>
    public override bool CanResetValue(object component) => true;

    ///<inheritdoc/>
    public override object? GetValue(object? component) => _store.TryGetValue(Name, out object? value) ? value : null;

    ///<inheritdoc/>
    public override void ResetValue(object component) => _store.Remove(Name);

    ///<inheritdoc/>
    public override void SetValue(object? component, object? value) => _store[Name] = value;

    ///<inheritdoc/>
    public override bool ShouldSerializeValue(object component) => _store.ContainsKey(Name);
    #endregion

    #region Public properties
    ///<inheritdoc/>
    public override Type ComponentType => typeof(DynamicTypeDescriptor);

    ///<inheritdoc/>
    public override bool IsReadOnly => false;

    ///<inheritdoc/>
    public override Type PropertyType => _propertyType;
    #endregion
}
