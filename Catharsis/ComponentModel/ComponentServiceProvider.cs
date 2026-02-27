namespace Catharsis.ComponentModel;

///<summary>
///An <see cref="IServiceProvider"/> implementation that supports hierarchical (chained) service resolution by combining
///explicit registrations with an optional parent provider fallback.
///</summary>
///<remarks>
///<para> Services are resolved in the following order:<list type="number"><item><description>Explicit registrations
///added via <see cref="Register{TService}(TService)"/>.</description></item><item><description>Factory registrations
///added via <see cref="Register{TService}(Func{TService})"/>.</description></item><item><description>The parent <see
///cref="IServiceProvider"/>, if provided.</description></item></list></para> <para> This type is intentionally
///lightweight and does not manage service lifetimes. For full dependency injection,
///use<c>Microsoft.Extensions.DependencyInjection</c>.</para>
///</remarks>
public sealed class ComponentServiceProvider : IServiceProvider
{
    #region Fields
    readonly Dictionary<Type, Func<object>> _factories = [];
    readonly Dictionary<Type, object> _instances = [];
    readonly IServiceProvider? _parent;
    #endregion

    #region Constructors
    ///<summary>
    ///Initializes a FileName instance of <see cref="ComponentServiceProvider"/> with an optional parent provider for
    ///fallback resolution.
    ///</summary>
    ///<param name="parent">
    ///An optional parent <see cref="IServiceProvider"/> to delegate to when a service cannot be resolved locally.
    ///</param>
    public ComponentServiceProvider(IServiceProvider? parent = null) { _parent = parent; }
    #endregion

    #region Public methods
    ///<inheritdoc/>
    public object? GetService(Type serviceType)
    {
        ArgumentNullException.ThrowIfNull(serviceType);

        if(serviceType == typeof(IServiceProvider))
        {
            return this;
        }

        if(_instances.TryGetValue(serviceType, out object instance))
        {
            return instance;
        }

        if(_factories.TryGetValue(serviceType, out Func<object> factory))
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
    ///<c>true</c> if a registration exists; otherwise, <c>false</c>.
    ///</returns>
    public bool IsRegistered<TService>() where TService : class { return _instances.ContainsKey(typeof(TService)) || _factories.ContainsKey(typeof(TService)); }

    ///<summary>
    ///Registers a singleton service instance for the specified service type.
    ///</summary>
    ///<typeparam name="TService">The service type.</typeparam>
    ///<param name="instance">The service instance to register.</param>
    ///<returns>This instance, for fluent chaining.</returns>
    ///<exception cref="ArgumentNullException">
    ///<paramref name="instance"/> is <c>null</c>.
    ///</exception>
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
    ///<paramref name="factory"/> is <c>null</c>.
    ///</exception>
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
    ///<c>true</c> if a registration was removed; <c>false</c> if no registration existed for the type.
    ///</returns>
    public bool Unregister<TService>() where TService : class
    {
        bool removed = _instances.Remove(typeof(TService));
        removed |= _factories.Remove(typeof(TService));
        return removed;
    }
    #endregion
}
