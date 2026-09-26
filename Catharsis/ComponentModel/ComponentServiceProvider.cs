namespace Catharsis.ComponentModel;

///<summary>
///An <see cref="IServiceProvider"/> implementation that supports hierarchical (chained) service resolution by combining
///explicit registrations with an optional parent provider fallback.
///</summary>
///<remarks>
///<remarks>
///Initializes a FileName instance of <see cref="ComponentServiceProvider"/> with an optional parent provider for
///fallback resolution.
///</remarks>
///<param name="parent">
///An optional parent <see cref="IServiceProvider"/> to delegate to when a service cannot be resolved locally.
///</param>
public sealed class ComponentServiceProvider(IServiceProvider? parent = null) : IServiceProvider
{
    #region Fields
    private readonly Dictionary<Type, Func<object>> _factories = [];
    private readonly Dictionary<Type, object> _instances = [];
    private readonly IServiceProvider? _parent = parent;

    #endregion
    #region Constructors
    #endregion

    #region Public methods
    ///<inheritdoc/>
    public object? GetService(Type serviceType)
    {
        ArgumentNullException.ThrowIfNull(serviceType);

        if (serviceType == typeof(IServiceProvider))
        {
            return this;
        }

        if (_instances.TryGetValue(serviceType, out object? instance))
        {
            return instance;
        }

        if (_factories.TryGetValue(serviceType, out Func<object>? factory))
        {
            return factory();
        }

        return _parent?.GetService(serviceType);
    }

    ///<summary>
    ///Gets a value indicating whether a service of the specified type is registered locally (not counting the parent
    ///provider).
    ///</summary>
    ///<typeparam name="TService">The service type to check.</typeparam>
    ///<returns>
    public bool IsRegistered<TService>() where TService : class { return _instances.ContainsKey(typeof(TService)) || _factories.ContainsKey(typeof(TService)); }

    ///<summary>
    ///Registers a singleton service instance for the specified service type.
    ///</summary>
    ///<typeparam name="TService">The service type.</typeparam>
    ///<param name="instance">The service instance to register.</param>
    ///<returns>This instance, for fluent chaining.</returns>
    ///<exception cref="ArgumentNullException">
    public ComponentServiceProvider Register<TService>(TService instance) where TService : class
    {
        ArgumentNullException.ThrowIfNull(instance);

        _instances[typeof(TService)] = instance;
        _factories.Remove(typeof(TService));
        return this;
    }

    ///<summary>
    ///Registers a factory function for the specified service type. The factory is invoked each time the service is
    ///requested.
    ///</summary>
    ///<typeparam name="TService">The service type.</typeparam>
    ///<param name="factory">The factory function that creates the service.</param>
    ///<returns>This instance, for fluent chaining.</returns>
    ///<exception cref="ArgumentNullException">
    public ComponentServiceProvider Register<TService>(Func<TService> factory) where TService : class
    {
        ArgumentNullException.ThrowIfNull(factory);

        _factories[typeof(TService)] = () => factory();
        _instances.Remove(typeof(TService));
        return this;
    }

    ///<summary>
    ///Removes a previously registered service of the specified type.
    ///</summary>
    ///<typeparam name="TService">The service type to unregister.</typeparam>
    ///<returns>
    public bool Unregister<TService>() where TService : class
    {
        bool removed = _instances.Remove(typeof(TService));
        removed |= _factories.Remove(typeof(TService));
        return removed;
    }
    #endregion
}
