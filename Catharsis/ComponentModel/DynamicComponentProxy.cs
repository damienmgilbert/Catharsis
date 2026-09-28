using System.Dynamic;
using System.Reflection;

namespace Catharsis.ComponentModel;

///<summary>
///A <see cref="DynamicObject"/> proxy that exposes a wrapped component's public instance properties through
///dynamic member access (<c>((dynamic)proxy).PropertyName</c>), resolved via reflection on each access. Distinct
///from <see cref="DynamicTypeDescriptor"/>, which augments static <see cref="System.ComponentModel.ICustomTypeDescriptor"/>
///reflection rather than providing runtime dynamic dispatch.
///</summary>
///<param name="component">The object whose properties are exposed dynamically.</param>
///<exception cref="ArgumentNullException"><paramref name="component"/> is <c>null</c>.</exception>
public sealed class DynamicComponentProxy(object component) : DynamicObject
{
    #region Fields
    readonly object _component = component ?? throw new ArgumentNullException(nameof(component));
    #endregion

    #region Public methods
    ///<inheritdoc/>
    public override IEnumerable<string> GetDynamicMemberNames()
    {
        return _component.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).Select(static property => property.Name);
    }

    ///<inheritdoc/>
    public override bool TryGetMember(GetMemberBinder binder, out object? result)
    {
        ArgumentNullException.ThrowIfNull(binder);

        PropertyInfo? property = _component.GetType().GetProperty(binder.Name, BindingFlags.Public | BindingFlags.Instance);

        if((property is null) || !property.CanRead)
        {
            result = null;
            return false;
        }

        result = property.GetValue(_component);
        return true;
    }

    ///<inheritdoc/>
    public override bool TrySetMember(SetMemberBinder binder, object? value)
    {
        ArgumentNullException.ThrowIfNull(binder);

        PropertyInfo? property = _component.GetType().GetProperty(binder.Name, BindingFlags.Public | BindingFlags.Instance);

        if((property is null) || !property.CanWrite)
        {
            return false;
        }

        property.SetValue(_component, value);
        return true;
    }
    #endregion

    #region Public properties
    ///<summary>
    ///The wrapped component.
    ///</summary>
    public object Component => _component;
    #endregion
}
