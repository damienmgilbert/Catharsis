using System.ComponentModel;

namespace Catharsis.ComponentModel.Lifecycle;

///<summary>
///Fluent builder for constructing <see cref="ComponentGraph"/> instances by registering components and declaring
///dependencies between them.
///</summary>
///<remarks>
///<para> Use <see cref="AddComponent"/> to register components, <see cref="AddDependency"/> to declare edges, and <see
///cref="Build"/> to produce the final graph. The builder validates the graph for cycles during <see
///cref="Build"/>.</para>
///</remarks>
public sealed class ComponentGraphBuilder
{
    #region Fields
    readonly List<(IComponent Dependent, IComponent Dependency)> _edges = [];
    readonly Dictionary<IComponent, ComponentGraphNode> _nodes = [];
    #endregion

    #region Public methods
    ///<summary>
    ///Registers a component in the graph.
    ///</summary>
    ///<param name="component">The component to register.</param>
    ///<param name="name">An optional display name for the node.</param>
    ///<returns>This builder, for fluent chaining.</returns>
    ///<exception cref="ArgumentNullException">
    ///<paramref name="component"/> is <c>null</c>.
    ///</exception>
    ///<exception cref="InvalidOperationException">
    ///The component has already been registered.
    ///</exception>
    public ComponentGraphBuilder AddComponent(IComponent component, string? name = null)
    {
        ArgumentNullException.ThrowIfNull(component);

        if(_nodes.ContainsKey(component))
        {
            throw new InvalidOperationException($"Component '{name ?? component.GetType().Name}' is already registered.");
        }

        _nodes[component] = new ComponentGraphNode(component, name);
        return this;
    }

    ///<summary>
    ///Declares that <paramref name="dependent"/> depends on <paramref name="dependency"/> (dependency must be activated
    ///first).
    ///</summary>
    ///<param name="dependent">The component that has the dependency.</param>
    ///<param name="dependency">The component being depended upon.</param>
    ///<returns>This builder, for fluent chaining.</returns>
    ///<exception cref="ArgumentNullException">
    ///Either argument is <c>null</c>.
    ///</exception>
    public ComponentGraphBuilder AddDependency(IComponent dependent, IComponent dependency)
    {
        ArgumentNullException.ThrowIfNull(dependent);
        ArgumentNullException.ThrowIfNull(dependency);

        _edges.Add((dependent, dependency));
        return this;
    }

    ///<summary>
    ///Builds the <see cref="ComponentGraph"/> from the registered components and dependencies.
    ///</summary>
    ///<returns>A constructed and validated <see cref="ComponentGraph"/>.</returns>
    ///<exception cref="InvalidOperationException">
    ///A dependency references an unregistered component, or the graph contains a cycle.
    ///</exception>
    public ComponentGraph Build()
    {
        ComponentGraph graph = new ComponentGraph();

        foreach(ComponentGraphNode node in _nodes.Values)
        {
            graph.AddNode(node);
        }

        foreach (var (dependent, dependency) in _edges)
        {
            if(!_nodes.TryGetValue(dependent, out ComponentGraphNode dependentNode))
            {
                throw new InvalidOperationException($"Dependent component '{dependent.GetType().Name}' is not registered in the graph.");
            }

            if(!_nodes.TryGetValue(dependency, out ComponentGraphNode dependencyNode))
            {
                throw new InvalidOperationException($"Dependency component '{dependency.GetType().Name}' is not registered in the graph.");
            }

            dependentNode.AddDependency(dependencyNode);
        }

        // Validate no cycles by attempting topological sort.
        graph.GetActivationOrder();

        return graph;
    }

    ///<summary>
    ///Resets the builder to its initial state.
    ///</summary>
    ///<returns>This builder, for fluent chaining.</returns>
    public ComponentGraphBuilder Clear()
    {
        _nodes.Clear();
        _edges.Clear();
        return this;
    }
    #endregion
}
