using System.ComponentModel;

namespace Catharsis.ComponentModel;

///<summary>
///A full <see cref="IContainer"/> implementation that manages <see cref="IComponent"/> instances with named <see
///cref="ComponentSite"/> support, deterministic disposal, and optional service provider integration.
///</summary>
///<remarks>
///<para> Components are sited with <see cref="ComponentSite"/> instances that support design-mode indication and
///hierarchical service resolution when an<see cref="IServiceProvider"/> is supplied.</para> <para> Components are
///disposed in reverse insertion order when the container is disposed, ensuring dependent components are cleaned up
///before their dependencies.</para>
///</remarks>
public sealed class ComponentContainer : IContainer, IServiceProvider
{
    #region Fields
    readonly bool _designMode;
    bool _disposed;
    readonly IServiceProvider? _serviceProvider;
    readonly List<ComponentSite> _sites = [];
    #endregion

    #region Constructors
    ///<summary>
    ///Initializes a FileName instance of <see cref="ComponentContainer"/>.
    ///</summary>
    ///<param name="designMode">
    ///<c>true</c> to indicate components are in design mode; otherwise, <c>false</c>.
    ///</param>
    ///<param name="serviceProvider">
    ///An optional service provider passed to <see cref="ComponentSite"/> instances for hierarchical service resolution.
    ///</param>
    public ComponentContainer(bool designMode = false, IServiceProvider? serviceProvider = null)
    {
        _designMode = designMode;
        _serviceProvider = serviceProvider;
    }
    #endregion

    #region Public methods
    ///<inheritdoc/>
    public void Add(IComponent? component) { Add(component, null); }

    ///<inheritdoc/>
    ///<exception cref="ArgumentException">
    ///A component with the specified <paramref name="name"/> already exists in the container.
    ///</exception>
    public void Add(IComponent? component, string? name)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if(component is null)
        {
            return;
        }

        if((name is not null) && _sites.Any(s => string.Equals(s.Name, name, StringComparison.Ordinal)))
        {
            throw new ArgumentException($"A component named '{name}' already exists in the container.", nameof(name));
        }

        // Remove from previous container if sited elsewhere
        component.Site?.Container?.Remove(component);

        ComponentSite site = new ComponentSite(this, component, name, _designMode, _serviceProvider);
        _sites.Add(site);
        component.Site = site;
    }

    ///<summary>
    ///Disposes the container and all contained components in reverse insertion order.
    ///</summary>
    public void Dispose()
    {
        if(_disposed)
        {
            return;
        }

        _disposed = true;

        for(int i = _sites.Count - 1; i >= 0; i--)
        {
            IComponent component = _sites[i].Component;
            component.Site = null;
            component.Dispose();
        }

        _sites.Clear();
    }

    ///<summary>
    ///Gets a component by name.
    ///</summary>
    ///<param name="name">The name of the component to retrieve.</param>
    ///<returns>The component, or <c>null</c> if not found.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="name"/> is <c>null</c>.</exception>
    public IComponent? GetComponent(string name)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(name);

        return _sites
            .FirstOrDefault(s => string.Equals(s.Name, name, StringComparison.Ordinal))?
            .Component;
    }

    ///<summary>
    ///Returns a service of the specified type. Delegates to the injected <see cref="IServiceProvider"/> if available;
    ///otherwise returns <c>null</c>.
    ///</summary>
    ///<param name="serviceType">The type of service to retrieve.</param>
    ///<returns>
    ///An object implementing the requested service, or <c>null</c> if the service is not available.
    ///</returns>
    public object? GetService(Type serviceType)
    {
        ArgumentNullException.ThrowIfNull(serviceType);

        if(serviceType == typeof(IContainer))
        {
            return this;
        }

        return _serviceProvider?.GetService(serviceType);
    }

    ///<inheritdoc/>
    public void Remove(IComponent? component)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if(component is null)
        {
            return;
        }

        ComponentSite? site = _sites.FirstOrDefault(s => ReferenceEquals(s.Component, component));

        if(site is null)
        {
            return;
        }

        _sites.Remove(site);
        component.Site = null;
    }
    #endregion

    #region Public properties
    ///<inheritdoc/>
    public ComponentCollection Components
    {
        get
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            IComponent[] components = [.. _sites.Select(static s => s.Component)];

            return new ComponentCollection(components);
        }
    }

    ///<summary>
    ///Gets the number of components in the container.
    ///</summary>
    public int Count
    {
        get
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            return _sites.Count;
        }
    }
    #endregion
}
