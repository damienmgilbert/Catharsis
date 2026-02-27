using System.ComponentModel;

namespace Catharsis.ComponentModel;

/// <summary>
/// A lightweight <see cref="IContainer"/> implementation that manages
/// <see cref="IComponent"/> instances with named site support and
/// deterministic disposal.
/// </summary>
public sealed class ComponentContainerSlim : IContainer
{
    private readonly List<ISite> _sites = [];
    private bool _disposed;

    /// <inheritdoc />
    public ComponentCollection Components
    {
        get
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            var components = _sites
                .Select(s => s.Component)
                .Where(c => c is not null)
                .ToArray();

            return new ComponentCollection(components!);
        }
    }

    /// <summary>
    /// Adds a component to the container without a name.
    /// </summary>
    /// <param name="component">The component to add.</param>
    public void Add(IComponent? component) => Add(component, null);

    /// <summary>
    /// Adds a component to the container with an optional name.
    /// </summary>
    /// <param name="component">The component to add.</param>
    /// <param name="name">The name to assign, or <c>null</c> for unnamed.</param>
    /// <exception cref="ArgumentException">
    /// A component with the specified <paramref name="name"/> already exists.
    /// </exception>
    public void Add(IComponent? component, string? name)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (component is null)
            return;

        if (name is not null && _sites.Any(s => string.Equals(s.Name, name, StringComparison.Ordinal)))
            throw new ArgumentException($"A component named '{name}' already exists in the container.", nameof(name));

        var site = new SlimSite(this, component, name);
        _sites.Add(site);
        component.Site = site;
    }

    /// <summary>
    /// Removes a component from the container.
    /// </summary>
    /// <param name="component">The component to remove.</param>
    public void Remove(IComponent? component)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (component is null)
            return;

        var site = _sites.FirstOrDefault(s => ReferenceEquals(s.Component, component));

        if (site is null)
            return;

        _sites.Remove(site);
        component.Site = null;
    }

    /// <summary>
    /// Gets a component by name.
    /// </summary>
    /// <param name="name">The name of the component to retrieve.</param>
    /// <returns>The component, or <c>null</c> if not found.</returns>
    public IComponent? GetComponent(string name)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(name);

        return _sites
            .FirstOrDefault(s => string.Equals(s.Name, name, StringComparison.Ordinal))?
            .Component;
    }

    /// <summary>
    /// Gets the number of components in the container.
    /// </summary>
    public int Count => _sites.Count;

    /// <summary>
    /// Disposes the container and all contained components.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        // Dispose in reverse order (last added first)
        for (int i = _sites.Count - 1; i >= 0; i--)
        {
            var component = _sites[i].Component;
            component.Site = null;
            component.Dispose();
        }

        _sites.Clear();
    }

    private sealed class SlimSite(IContainer container, IComponent component, string? name) : ISite
    {
        public IComponent Component { get; } = component;
        public IContainer Container { get; } = container;
        public bool DesignMode => false;
        public string? Name { get; set; } = name;

        public object? GetService(Type serviceType)
        {
            if (serviceType == typeof(ISite))
                return this;

            if (serviceType == typeof(IContainer))
                return Container;

            return null;
        }
    }
}
