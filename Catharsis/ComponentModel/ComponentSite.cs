using System.ComponentModel;

namespace Catharsis.ComponentModel;

///<summary>
///A full <see cref="ISite"/> implementation that binds a component to its container, provides naming, design-mode
///indication, and hierarchical service resolution.
///</summary>
///<remarks>
///When resolving services via <see cref="GetService"/>, the site first checks for well-known types (<see
///cref="ISite"/>, <see cref="IContainer"/>), then delegates to an optional <see cref="IServiceProvider"/>, and finally
///falls back to the container if it implements <see cref="IServiceProvider"/>.
///</remarks>
public sealed class ComponentSite : ISite
{
    #region Fields
    readonly IServiceProvider? _serviceProvider;
    #endregion

    #region Constructors
    ///<summary>
    ///Initializes a new instance of <see cref="ComponentSite"/> binding the specified component to the given
    ///container.
    ///</summary>
    ///<param name="container">The container hosting the component.</param>
    ///<param name="component">The component being sited.</param>
    ///<param name="name">An optional name for the component.</param>
    ///<param name="designMode">
    ///<c>true</c> if the component is in design mode; otherwise, <c>false</c>.
    ///</param>
    ///<param name="serviceProvider">
    ///An optional service provider for resolving additional services.
    ///</param>
    ///<exception cref="ArgumentNullException">
    ///<paramref name="container"/> or <paramref name="component"/> is <c>null</c>.
    ///</exception>
    public ComponentSite(IContainer container, IComponent component, string? name = null, bool designMode = false, IServiceProvider? serviceProvider = null)
    {
        ArgumentNullException.ThrowIfNull(container);
        ArgumentNullException.ThrowIfNull(component);

        Container = container;
        Component = component;
        Name = name;
        DesignMode = designMode;
        _serviceProvider = serviceProvider;
    }
    #endregion

    #region Public methods
    ///<summary>
    ///Returns a service of the specified type.
    ///</summary>
    ///<param name="serviceType">The type of service to retrieve.</param>
    ///<returns>
    ///An object implementing the requested service, or <c>null</c> if the service is not available.
    ///</returns>
    ///<remarks>
    ///Resolution order: <list type="number"><item><description><see cref="ISite"/> — returns this
    ///instance.</description></item><item><description><see cref="IContainer"/> — returns the <see
    ///cref="Container"/>.</description></item><item><description>The injected <see cref="IServiceProvider"/>, if
    ///any.</description></item><item><description>The <see cref="Container"/> if it implements <see
    ///cref="IServiceProvider"/>.</description></item></list>
    ///</remarks>
    public object? GetService(Type serviceType)
    {
        ArgumentNullException.ThrowIfNull(serviceType);

        if(serviceType == typeof(ISite))
        {
            return this;
        }

        if(serviceType == typeof(IContainer))
        {
            return Container;
        }

        if(serviceType == typeof(IComponent))
        {
            return Component;
        }

        object? service = _serviceProvider?.GetService(serviceType);

        if(service is not null)
        {
            return service;
        }

        if(Container is IServiceProvider containerProvider)
        {
            return containerProvider.GetService(serviceType);
        }

        return null;
    }
    #endregion

    #region Public properties
    ///<inheritdoc/>
    public IComponent Component { get; }

    ///<inheritdoc/>
    public IContainer Container { get; }

    ///<inheritdoc/>
    public bool DesignMode { get; }

    ///<inheritdoc/>
    public string? Name { get; set; }
    #endregion
}
