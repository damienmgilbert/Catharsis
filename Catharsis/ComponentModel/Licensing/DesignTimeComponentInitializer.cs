using System.ComponentModel;

namespace Catharsis.ComponentModel.Licensing;

///<summary>
///Provides design-time initialization for components, applying default property values and running initialization
///callbacks when a component is added to a designer surface.
///</summary>
///<remarks>
///Use <see cref="Initialize"/> to apply registered defaults and invoke the initialization callback for a component
///type. Register defaults via <see cref="RegisterDefault{T}"/>.
///</remarks>
public sealed class DesignTimeComponentInitializer
{
    #region Fields
    readonly Dictionary<Type, Dictionary<string, object?>> _defaults = [];
    readonly Dictionary<Type, Action<IComponent>> _initializers = [];
    #endregion

    #region Public methods
    ///<summary>
    ///Gets a value indicating whether the specified component type has registered defaults or an initializer.
    ///</summary>
    ///<typeparam name="T">The component type.</typeparam>
    ///<returns><c>true</c> if registration exists; otherwise, <c>false</c>.</returns>
    public bool HasRegistration<T>() where T : IComponent { return _defaults.ContainsKey(typeof(T)) || _initializers.ContainsKey(typeof(T)); }

    ///<summary>
    ///Initializes the specified component by applying registered default values and invoking the initialization
    ///callback.
    ///</summary>
    ///<param name="component">The component to initialize.</param>
    ///<exception cref="ArgumentNullException">
    ///<paramref name="component"/> is <c>null</c>.
    ///</exception>
    public void Initialize(IComponent component)
    {
        ArgumentNullException.ThrowIfNull(component);

        Type type = component.GetType();

        if(_defaults.TryGetValue(type, out Dictionary<string, object?>? props))
        {
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(component);

            foreach (var (name, value) in props)
            {
                PropertyDescriptor? prop = properties[name];
                if((prop is not null) && !prop.IsReadOnly)
                {
                    prop.SetValue(component, value);
                }
            }
        }

        if(_initializers.TryGetValue(type, out Action<IComponent>? initializer))
        {
            initializer(component);
        }
    }

    ///<summary>
    ///Registers a default property value for the specified component type.
    ///</summary>
    ///<typeparam name="T">The component type.</typeparam>
    ///<param name="propertyName">The property to set.</param>
    ///<param name="value">The default value.</param>
    ///<returns>This instance, for fluent chaining.</returns>
    ///<exception cref="ArgumentNullException">
    ///<paramref name="propertyName"/> is <c>null</c>.
    ///</exception>
    public DesignTimeComponentInitializer RegisterDefault<T>(string propertyName, object? value) where T : IComponent
    {
        ArgumentNullException.ThrowIfNull(propertyName);

        Type type = typeof(T);

        if(!_defaults.TryGetValue(type, out Dictionary<string, object?> props))
        {
            props = new Dictionary<string, object?>(StringComparer.Ordinal);
            _defaults[type] = props;
        }

        props[propertyName] = value;
        return this;
    }

    ///<summary>
    ///Registers an initialization callback for the specified component type.
    ///</summary>
    ///<typeparam name="T">The component type.</typeparam>
    ///<param name="initializer">The callback to invoke during initialization.</param>
    ///<returns>This instance, for fluent chaining.</returns>
    ///<exception cref="ArgumentNullException">
    ///<paramref name="initializer"/> is <c>null</c>.
    ///</exception>
    public DesignTimeComponentInitializer RegisterInitializer<T>(Action<T> initializer) where T : IComponent
    {
        ArgumentNullException.ThrowIfNull(initializer);
        _initializers[typeof(T)] = c => initializer((T)c);
        return this;
    }
    #endregion
}
