using System.ComponentModel;

namespace Catharsis.ComponentModel;

///<summary>
///A simple <see cref="PropertyDescriptor"/> backed by a dictionary of values, intended for use with <see
///cref="DynamicTypeDescriptor"/>.
///</summary>
public sealed class DictionaryPropertyDescriptor : PropertyDescriptor
{
    #region Fields
    readonly Type _propertyType;
    readonly IDictionary<string, object?> _store;
    #endregion

    #region Constructors
    ///<summary>
    ///Initializes a FileName instance of <see cref="DictionaryPropertyDescriptor"/>.
    ///</summary>
    ///<param name="name">The property name.</param>
    ///<param name="propertyType">The type of the property value.</param>
    ///<param name="store">The backing dictionary for get/set operations.</param>
    ///<param name="attributes">Optional attributes for the property.</param>
    ///<exception cref="ArgumentNullException">
    ///<paramref name="store"/> or <paramref name="propertyType"/> is <c>null</c>.
    ///</exception>
    public DictionaryPropertyDescriptor(string name, Type propertyType, IDictionary<string, object?> store, params Attribute[] attributes) : base(name, attributes)
    {
        _propertyType = propertyType ?? throw new ArgumentNullException(nameof(propertyType));
        _store = store ?? throw new ArgumentNullException(nameof(store));
    }
    #endregion

    #region Public methods
    ///<inheritdoc/>
    public override bool CanResetValue(object component) { return true; }
    ///<inheritdoc/>
    public override object? GetValue(object? component) { return _store.TryGetValue(Name, out object? value) ? value : null; }
    ///<inheritdoc/>
    public override void ResetValue(object component) { _store.Remove(Name); }
    ///<inheritdoc/>
    public override void SetValue(object? component, object? value) { _store[Name] = value; }
    ///<inheritdoc/>
    public override bool ShouldSerializeValue(object component) { return _store.ContainsKey(Name); }
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
