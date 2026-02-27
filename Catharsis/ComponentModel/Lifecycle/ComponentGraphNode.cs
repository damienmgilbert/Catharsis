using System.ComponentModel;

namespace Catharsis.ComponentModel.Lifecycle;

/// <summary>
/// Represents a single node in a <see cref="ComponentGraph"/>, wrapping an
/// <see cref="IComponent"/> with its dependencies and lifecycle state.
/// </summary>
/// <remarks>
/// Nodes track their direct dependencies (other nodes this node depends on)
/// and dependents (other nodes that depend on this node). The
/// <see cref="ComponentGraphBuilder"/> constructs the graph and resolves
/// these relationships.
/// </remarks>
public sealed class ComponentGraphNode
{
    private readonly List<ComponentGraphNode> _dependencies = [];
    private readonly List<ComponentGraphNode> _dependents = [];

    /// <summary>
    /// Initializes a new instance of <see cref="ComponentGraphNode"/>.
    /// </summary>
    /// <param name="component">The component this node represents.</param>
    /// <param name="name">An optional name for the node.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="component"/> is <c>null</c>.
    /// </exception>
    public ComponentGraphNode(IComponent component, string? name = null)
    {
        ArgumentNullException.ThrowIfNull(component);
        Component = component;
        Name = name ?? component.GetType().Name;
    }

    /// <summary>
    /// Gets the component this node represents.
    /// </summary>
    public IComponent Component { get; }

    /// <summary>
    /// Gets the human-readable name for this node.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets or sets the current lifecycle state of the component.
    /// </summary>
    public ComponentState State { get; set; } = ComponentState.Created;

    /// <summary>
    /// Gets the nodes that this node depends on (must be activated first).
    /// </summary>
    public IReadOnlyList<ComponentGraphNode> Dependencies => _dependencies;

    /// <summary>
    /// Gets the nodes that depend on this node (must be deactivated first).
    /// </summary>
    public IReadOnlyList<ComponentGraphNode> Dependents => _dependents;

    /// <summary>
    /// Gets a value indicating whether all dependencies are in an active state.
    /// </summary>
    public bool AreDependenciesSatisfied =>
        _dependencies.TrueForAll(static d => d.State == ComponentState.Active);

    /// <summary>
    /// Adds a dependency to this node.
    /// </summary>
    /// <param name="dependency">The node this node depends on.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="dependency"/> is <c>null</c>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Adding this dependency would create a circular reference.
    /// </exception>
    internal void AddDependency(ComponentGraphNode dependency)
    {
        ArgumentNullException.ThrowIfNull(dependency);

        if (ReferenceEquals(this, dependency))
            throw new InvalidOperationException("A node cannot depend on itself.");

        if (WouldCreateCycle(dependency))
        {
            throw new InvalidOperationException(
                $"Adding dependency '{dependency.Name}' to '{Name}' would create a circular reference.");
        }

        if (!_dependencies.Contains(dependency))
        {
            _dependencies.Add(dependency);
            dependency._dependents.Add(this);
        }
    }

    /// <summary>
    /// Removes a dependency from this node.
    /// </summary>
    /// <param name="dependency">The dependency to remove.</param>
    /// <returns><c>true</c> if the dependency was found and removed; otherwise, <c>false</c>.</returns>
    internal bool RemoveDependency(ComponentGraphNode dependency)
    {
        ArgumentNullException.ThrowIfNull(dependency);

        if (_dependencies.Remove(dependency))
        {
            dependency._dependents.Remove(this);
            return true;
        }

        return false;
    }

    private bool WouldCreateCycle(ComponentGraphNode target)
    {
        var visited = new HashSet<ComponentGraphNode>();
        return HasPathTo(target, this, visited);
    }

    private static bool HasPathTo(
        ComponentGraphNode from,
        ComponentGraphNode to,
        HashSet<ComponentGraphNode> visited)
    {
        if (ReferenceEquals(from, to))
            return true;

        if (!visited.Add(from))
            return false;

        foreach (var dep in from._dependencies)
        {
            if (HasPathTo(dep, to, visited))
                return true;
        }

        return false;
    }

    /// <inheritdoc />
    public override string ToString() => $"{Name} [{State}]";
}
