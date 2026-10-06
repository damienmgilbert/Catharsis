using System.ComponentModel;

namespace Catharsis.ComponentModel.Lifecycle;

///<summary>
///Represents a directed acyclic graph of <see cref="ComponentGraphNode"/> instances, modeling component dependencies
///and enabling topologically-ordered lifecycle operations (activation, deactivation, disposal).
///</summary>
///<remarks>
public sealed class ComponentGraph
{
    #region Fields
    private readonly Dictionary<IComponent, ComponentGraphNode> _nodes = [];
    #endregion

    #region Private methods
    private List<ComponentGraphNode> TopologicalSort(bool reverse)
    {
        List<ComponentGraphNode> sorted = [ with(_nodes.Count) ];
        HashSet<ComponentGraphNode> visited = [];
        HashSet<ComponentGraphNode> visiting = [];

        foreach(ComponentGraphNode node in _nodes.Values)
        {
            if(!visited.Contains(node))
            {
                Visit(node, visited, visiting, sorted);
            }
        }

        if(reverse)
        {
            sorted.Reverse();
        }

        return sorted;
    }

    private static void Visit(ComponentGraphNode node, HashSet<ComponentGraphNode> visited, HashSet<ComponentGraphNode> visiting, List<ComponentGraphNode> sorted)
    {
        if(visiting.Contains(node))
        {
            throw new InvalidOperationException($"Cycle detected involving node '{node.Name}'.");
        }

        if(visited.Contains(node))
        {
            return;
        }

        visiting.Add(node);

        foreach(ComponentGraphNode dependency in node.Dependencies)
        {
            Visit(dependency, visited, visiting, sorted);
        }

        visiting.Remove(node);
        visited.Add(node);
        sorted.Add(node);
    }
    #endregion

    #region Internal methods
    ///<summary>
    ///Adds a node to the graph. Called by <see cref="ComponentGraphBuilder"/>.
    ///</summary>
    internal void AddNode(ComponentGraphNode node)
    {
        ArgumentNullException.ThrowIfNull(node);
        _nodes[node.Component] = node;
    }
    #endregion

    #region Public methods
    ///<summary>
    ///Determines whether the graph contains the specified component.
    ///</summary>
    ///<param name="component">The component to look for.</param>
    ///<returns><c>true</c> if the component is in the graph; otherwise, <c>false</c>.</returns>
    public bool Contains(IComponent component)
    {
        ArgumentNullException.ThrowIfNull(component);
        return _nodes.ContainsKey(component);
    }

    ///<summary>
    ///Returns the nodes in topological order suitable for activation (dependencies before dependents).
    ///</summary>
    ///<returns>An ordered list of nodes.</returns>
    ///<exception cref="InvalidOperationException">
    ///The graph contains a cycle.
    ///</exception>
    public IReadOnlyList<ComponentGraphNode> GetActivationOrder() => TopologicalSort(reverse: false);

    ///<summary>
    ///Returns the nodes in reverse topological order suitable for deactivation (dependents before dependencies).
    ///</summary>
    ///<returns>An ordered list of nodes.</returns>
    ///<exception cref="InvalidOperationException">
    ///The graph contains a cycle.
    ///</exception>
    public IReadOnlyList<ComponentGraphNode> GetDeactivationOrder() => TopologicalSort(reverse: true);

    ///<summary>
    ///Returns all leaf nodes (nodes with no dependents).
    ///</summary>
    public IReadOnlyList<ComponentGraphNode> GetLeaves()
    {
        List<ComponentGraphNode> leaves = [];

        foreach(ComponentGraphNode node in _nodes.Values)
        {
            if(node.Dependents.Count == 0)
            {
                leaves.Add(node);
            }
        }

        return leaves;
    }

    ///<summary>
    ///Gets the node for the specified component.
    ///</summary>
    ///<param name="component">The component to look up.</param>
    ///<returns>The graph node, or <c>null</c> if the component is not in the graph.</returns>
    public ComponentGraphNode? GetNode(IComponent component)
    {
        ArgumentNullException.ThrowIfNull(component);
        return _nodes.GetValueOrDefault(component);
    }

    ///<summary>
    ///Returns all root nodes (nodes with no dependencies).
    ///</summary>
    public IReadOnlyList<ComponentGraphNode> GetRoots()
    {
        List<ComponentGraphNode> roots = [];

        foreach(ComponentGraphNode node in _nodes.Values)
        {
            if(node.Dependencies.Count == 0)
            {
                roots.Add(node);
            }
        }

        return roots;
    }
    #endregion

    #region Public properties
    ///<summary>
    ///Gets the number of nodes in the graph.
    ///</summary>
    public int Count => _nodes.Count;

    ///<summary>
    ///Gets all nodes in the graph.
    ///</summary>
    public IReadOnlyCollection<ComponentGraphNode> Nodes => _nodes.Values;
    #endregion
}
