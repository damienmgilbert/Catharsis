using System.ComponentModel;

namespace Catharsis.ComponentModel.Lifecycle;

///<summary>
///Provides contextual information to components during activation, including access to services, the owning container,
///and configuration properties.
///</summary>
///<remarks>
///<para> The <see cref="ComponentLifecycleManager"/> creates an activation context for each component being initialized
///or activated. Components can use the context to resolve services and read configuration parameters.</para>
///</remarks>
public sealed class ComponentActivationContext : IServiceProvider
{
    #region Fields
    readonly Dictionary<string, object?> _properties = new(StringComparer.Ordinal);
    readonly IServiceProvider? _serviceProvider;
    #endregion

    #region Constructors
    ///<summary>
    ///Initializes a new instance of <see cref="ComponentActivationContext"/>.
    ///</summary>
    ///<param name="component">The component being activated.</param>
    ///<param name="serviceProvider">An optional service provider for service resolution.</param>
    ///<exception cref="ArgumentNullException">
    ///<paramref name="component"/> is <c>null</c>.
    ///</exception>
    public ComponentActivationContext(IComponent component, IServiceProvider? serviceProvider = null)
    {
        ArgumentNullException.ThrowIfNull(component);
        Component = component;
        _serviceProvider = serviceProvider;
    }
    #endregion

    #region Public methods
    ///<summary>
    ///Gets a typed property from the activation context.
    ///</summary>
    ///<typeparam name="T">The expected type of the property value.</typeparam>
    ///<param name="key">The property key.</param>
    ///<returns>The typed value, or the default if not found.</returns>
    public T? GetProperty<T>(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        return (_properties.TryGetValue(key, out object? value) && (value is T typed)) ? typed : default;
    }

    ///<inheritdoc/>
    public object? GetService(Type serviceType)
    {
        ArgumentNullException.ThrowIfNull(serviceType);
        return _serviceProvider?.GetService(serviceType);
    }

    ///<summary>
    ///Sets a property on the activation context.
    ///</summary>
    ///<param name="key">The property key.</param>
    ///<param name="value">The property value.</param>
    ///<exception cref="ArgumentException">
    ///<paramref name="key"/> is null or whitespace.
    ///</exception>
    public void SetProperty(string key, object? value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        _properties[key] = value;
    }

    ///<inheritdoc/>
    public override string ToString() { return $"ActivationContext [{Component.GetType().Name}]: {CurrentState} -> {TargetState}"; }
    #endregion

    #region Public properties
    ///<summary>
    ///Gets or sets a cancellation token for cooperative cancellation of the activation process.
    ///</summary>
    public CancellationToken CancellationToken { get; init; }

    ///<summary>
    ///Gets the component being activated.
    ///</summary>
    public IComponent Component { get; }

    ///<summary>
    ///Gets the container hosting the component, if available.
    ///</summary>
    public IContainer? Container => Component.Site?.Container;

    ///<summary>
    ///Gets or sets the current lifecycle state of the component at the time this context was created.
    ///</summary>
    public ComponentState CurrentState { get; init; }

    ///<summary>
    ///Gets the activation properties as a read-only dictionary.
    ///</summary>
    public IReadOnlyDictionary<string, object?> Properties => _properties;

    ///<summary>
    ///Gets or sets the target state the component is transitioning to.
    ///</summary>
    public ComponentState TargetState { get; init; }
    #endregion
}
