using System.ComponentModel;

namespace Catharsis.ComponentModel.Licensing;

/// <summary>
/// Encapsulates the design-time context for a component, providing access
/// to the component instance, its container, and the service provider.
/// </summary>
/// <remarks>
/// This class serves as a simplified, non-designer-dependency context
/// object that can be used in design-time support scenarios without
/// requiring a full <c>System.ComponentModel.Design</c> designer host.
/// </remarks>
public sealed class ComponentDesignContext
{
    /// <summary>
    /// Initializes a new instance of <see cref="ComponentDesignContext"/>.
    /// </summary>
    /// <param name="component">The component being designed.</param>
    /// <param name="container">
    /// The container hosting the component, or <c>null</c>.
    /// </param>
    /// <param name="serviceProvider">
    /// The service provider for resolving design-time services, or <c>null</c>.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="component"/> is <c>null</c>.
    /// </exception>
    public ComponentDesignContext(
        IComponent component,
        IContainer? container = null,
        IServiceProvider? serviceProvider = null)
    {
        ArgumentNullException.ThrowIfNull(component);

        Component = component;
        Container = container ?? component.Site?.Container;
        ServiceProvider = serviceProvider;
    }

    /// <summary>
    /// Gets the component being designed.
    /// </summary>
    public IComponent Component { get; }

    /// <summary>
    /// Gets the container hosting the component.
    /// </summary>
    public IContainer? Container { get; }

    /// <summary>
    /// Gets the service provider for design-time service resolution.
    /// </summary>
    public IServiceProvider? ServiceProvider { get; }

    /// <summary>
    /// Gets a value indicating whether the component is currently in design mode.
    /// </summary>
    public bool IsDesignMode => Component.Site?.DesignMode ?? false;

    /// <summary>
    /// Resolves a service of the specified type from the service provider.
    /// </summary>
    /// <typeparam name="T">The service type.</typeparam>
    /// <returns>The service instance, or <c>null</c> if not available.</returns>
    public T? GetService<T>() where T : class =>
        ServiceProvider?.GetService(typeof(T)) as T;
}
