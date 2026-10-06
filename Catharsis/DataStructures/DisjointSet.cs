namespace Catharsis.DataStructures;

///<summary>
///A disjoint-set (union-find) structure over a fixed number of elements, indexed <c>0</c> through <c>Count - 1</c>.
///Supports near-constant-time union and connectivity queries via path compression and union by rank. Commonly used
///for Kruskal's algorithm, connected-component detection, and cycle detection in undirected graphs.
///</summary>
///<example>
///<code>
///DisjointSet sets = new(count: vertices.Count);
///
///foreach((int a, int b) in edges)
///{
///    sets.Union(a, b);
///}
///</code>
///</example>
public sealed class DisjointSet
{
    #region Fields
    readonly int[] _parent;
    readonly int[] _rank;
    int _setCount;
    #endregion

    #region Public methods
    ///<summary>
    ///Creates a disjoint-set structure with the specified number of elements, each initially in its own set.
    ///</summary>
    ///<param name="count">The number of elements.</param>
    ///<exception cref="ArgumentOutOfRangeException"><paramref name="count"/> is less than 1.</exception>
    public DisjointSet(int count)
    {
        if (count < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(count), "Count must be at least 1.");
        }

        _parent = new int[count];
        _rank = new int[count];

        for (int i = 0; i < count; i++)
        {
            _parent[i] = i;
        }

        _setCount = count;
    }

    ///<summary>
    ///Determines whether the two elements belong to the same set.
    ///</summary>
    ///<param name="a">The first element.</param>
    ///<param name="b">The second element.</param>
    ///<returns><c>true</c> if the elements are in the same set; otherwise <c>false</c>.</returns>
    ///<exception cref="ArgumentOutOfRangeException"><paramref name="a"/> or <paramref name="b"/> is out of range.</exception>
    public bool AreConnected(int a, int b) => Find(a) == Find(b);

    ///<summary>
    ///Finds the representative element of the set containing <paramref name="item"/>, compressing the path from
    ///<paramref name="item"/> to the root along the way.
    ///</summary>
    ///<param name="item">The element to look up.</param>
    ///<returns>The representative element of the set.</returns>
    ///<exception cref="ArgumentOutOfRangeException"><paramref name="item"/> is out of range.</exception>
    public int Find(int item)
    {
        ValidateIndex(item);

        if (_parent[item] != item)
        {
            _parent[item] = Find(_parent[item]);
        }

        return _parent[item];
    }

    ///<summary>
    ///Merges the sets containing the two elements.
    ///</summary>
    ///<param name="a">The first element.</param>
    ///<param name="b">The second element.</param>
    ///<returns><c>true</c> if the elements were in different sets and were merged; <c>false</c> if they were already connected.</returns>
    ///<exception cref="ArgumentOutOfRangeException"><paramref name="a"/> or <paramref name="b"/> is out of range.</exception>
    public bool Union(int a, int b)
    {
        int rootA = Find(a);
        int rootB = Find(b);

        if (rootA == rootB)
        {
            return false;
        }

        if (_rank[rootA] < _rank[rootB])
        {
            (rootA, rootB) = (rootB, rootA);
        }

        _parent[rootB] = rootA;

        if (_rank[rootA] == _rank[rootB])
        {
            _rank[rootA]++;
        }

        _setCount--;
        return true;
    }

    void ValidateIndex(int item)
    {
        if (item < 0 || item >= _parent.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(item), "Item is out of range.");
        }
    }
    #endregion

    #region Public properties
    ///<summary>
    ///Gets the total number of elements.
    ///</summary>
    public int Count => _parent.Length;

    ///<summary>
    ///Gets the current number of disjoint sets.
    ///</summary>
    public int SetCount => _setCount;
    #endregion
}
