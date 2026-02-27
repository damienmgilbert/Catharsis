using System.Collections;

namespace Catharsis.DataStructures;

/// <summary>
/// An adjacency-list directed graph that supports adding/removing vertices and edges,
/// neighbor queries, and breadth-first / depth-first traversals.
/// </summary>
/// <typeparam name="T">The type of the vertex value. Must be non-null and equatable.</typeparam>
public class Graph<T> : IEnumerable<T> where T : notnull
{
    private readonly Dictionary<T, HashSet<T>> _adjacency;

    /// <summary>
    /// Initializes a new empty <see cref="Graph{T}"/> using the default equality comparer.
    /// </summary>
    public Graph()
        : this(EqualityComparer<T>.Default)
    {
    }

    /// <summary>
    /// Initializes a new empty <see cref="Graph{T}"/> with the specified equality comparer.
    /// </summary>
    /// <param name="comparer">The comparer used to determine vertex equality.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="comparer"/> is <c>null</c>.</exception>
    public Graph(IEqualityComparer<T> comparer)
    {
        if (comparer is null)
            throw new ArgumentNullException(nameof(comparer), "Equality comparer must not be null.");

        _adjacency = new Dictionary<T, HashSet<T>>(comparer);
    }

    /// <summary>Gets the number of vertices in the graph.</summary>
    public int VertexCount => _adjacency.Count;

    /// <summary>Gets the total number of directed edges in the graph.</summary>
    public int EdgeCount
    {
        get
        {
            int count = 0;

            foreach (var edges in _adjacency.Values)
            {
                count += edges.Count;
            }

            return count;
        }
    }

    /// <summary>Gets all vertices in the graph.</summary>
    public IEnumerable<T> Vertices => _adjacency.Keys;

    /// <summary>
    /// Adds a vertex to the graph. Does nothing if the vertex already exists.
    /// </summary>
    /// <param name="vertex">The vertex to add.</param>
    /// <returns><c>true</c> if the vertex was added; <c>false</c> if it already existed.</returns>
    public bool AddVertex(T vertex)
    {
        if (_adjacency.ContainsKey(vertex))
            return false;

        _adjacency[vertex] = new HashSet<T>(_adjacency.Comparer);
        return true;
    }

    /// <summary>
    /// Removes a vertex and all edges connected to it.
    /// </summary>
    /// <param name="vertex">The vertex to remove.</param>
    /// <returns><c>true</c> if the vertex was found and removed; otherwise <c>false</c>.</returns>
    public bool RemoveVertex(T vertex)
    {
        if (!_adjacency.Remove(vertex))
            return false;

        foreach (var edges in _adjacency.Values)
        {
            edges.Remove(vertex);
        }

        return true;
    }

    /// <summary>
    /// Adds a directed edge from <paramref name="from"/> to <paramref name="to"/>.
    /// Both vertices are added automatically if they do not already exist.
    /// </summary>
    /// <param name="from">The source vertex.</param>
    /// <param name="to">The destination vertex.</param>
    /// <returns><c>true</c> if the edge was added; <c>false</c> if it already existed.</returns>
    public bool AddEdge(T from, T to)
    {
        AddVertex(from);
        AddVertex(to);
        return _adjacency[from].Add(to);
    }

    /// <summary>
    /// Removes the directed edge from <paramref name="from"/> to <paramref name="to"/>.
    /// </summary>
    /// <param name="from">The source vertex.</param>
    /// <param name="to">The destination vertex.</param>
    /// <returns><c>true</c> if the edge was found and removed; otherwise <c>false</c>.</returns>
    public bool RemoveEdge(T from, T to)
    {
        if (!_adjacency.TryGetValue(from, out var edges))
            return false;

        return edges.Remove(to);
    }

    /// <summary>
    /// Determines whether an edge exists from <paramref name="from"/> to <paramref name="to"/>.
    /// </summary>
    /// <param name="from">The source vertex.</param>
    /// <param name="to">The destination vertex.</param>
    /// <returns><c>true</c> if the edge exists; otherwise <c>false</c>.</returns>
    public bool HasEdge(T from, T to)
    {
        return _adjacency.TryGetValue(from, out var edges) && edges.Contains(to);
    }

    /// <summary>
    /// Determines whether the graph contains the specified vertex.
    /// </summary>
    /// <param name="vertex">The vertex to look for.</param>
    /// <returns><c>true</c> if the vertex exists; otherwise <c>false</c>.</returns>
    public bool ContainsVertex(T vertex) => _adjacency.ContainsKey(vertex);

    /// <summary>
    /// Returns the direct neighbors (successors) of the specified vertex.
    /// </summary>
    /// <param name="vertex">The vertex whose neighbors to retrieve.</param>
    /// <returns>A read-only sequence of neighbor vertices.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when <paramref name="vertex"/> is not in the graph.</exception>
    public IEnumerable<T> Neighbors(T vertex)
    {
        if (!_adjacency.TryGetValue(vertex, out var edges))
            throw new KeyNotFoundException($"Vertex '{vertex}' is not in the graph.");

        return edges;
    }

    /// <summary>
    /// Enumerates vertices reachable from <paramref name="start"/> in depth-first order.
    /// </summary>
    /// <param name="start">The starting vertex.</param>
    /// <returns>A sequence of vertices in depth-first order.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when <paramref name="start"/> is not in the graph.</exception>
    public IEnumerable<T> DepthFirst(T start)
    {
        if (!_adjacency.ContainsKey(start))
            throw new KeyNotFoundException($"Vertex '{start}' is not in the graph.");

        var visited = new HashSet<T>(_adjacency.Comparer);
        var stack = new Stack<T>();
        stack.Push(start);

        while (stack.Count > 0)
        {
            var current = stack.Pop();

            if (!visited.Add(current))
                continue;

            yield return current;

            foreach (var neighbor in _adjacency[current])
            {
                if (!visited.Contains(neighbor))
                    stack.Push(neighbor);
            }
        }
    }

    /// <summary>
    /// Enumerates vertices reachable from <paramref name="start"/> in breadth-first order.
    /// </summary>
    /// <param name="start">The starting vertex.</param>
    /// <returns>A sequence of vertices in breadth-first order.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when <paramref name="start"/> is not in the graph.</exception>
    public IEnumerable<T> BreadthFirst(T start)
    {
        if (!_adjacency.ContainsKey(start))
            throw new KeyNotFoundException($"Vertex '{start}' is not in the graph.");

        var visited = new HashSet<T>(_adjacency.Comparer);
        var queue = new Queue<T>();
        queue.Enqueue(start);
        visited.Add(start);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            yield return current;

            foreach (var neighbor in _adjacency[current])
            {
                if (visited.Add(neighbor))
                    queue.Enqueue(neighbor);
            }
        }
    }

    /// <summary>Removes all vertices and edges from the graph.</summary>
    public void Clear() => _adjacency.Clear();

    /// <summary>
    /// Default enumeration returns all vertices.
    /// </summary>
    /// <inheritdoc />
    public IEnumerator<T> GetEnumerator() => _adjacency.Keys.GetEnumerator();

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
