namespace Catharsis.DataStructures;

///<summary>
///An augmented binary search tree indexing a set of <see cref="Interval{T}"/> values by their start bound, enabling
///overlap queries faster than a linear scan on typical (non-adversarial) insertion orders. This structure is add-only;
///to remove intervals, build a new tree from a filtered sequence.
///</summary>
///<typeparam name="T">The type of the interval bounds. Must implement <see cref="IComparable{T}"/>.</typeparam>
///<example>
public sealed class IntervalTree<T> where T : IComparable<T>
{
    #region Fields
    private int _count;
    private Node? _root;
    #endregion

    #region Private methods
    private static T ComputeMaxEnd(Node node)
    {
        T max = node.Interval.End;

        if(node.Left is not null && node.Left.MaxEnd.CompareTo(max) > 0)
        {
            max = node.Left.MaxEnd;
        }

        if(node.Right is not null && node.Right.MaxEnd.CompareTo(max) > 0)
        {
            max = node.Right.MaxEnd;
        }

        return max;
    }

    private static void FindOverlapping(Node? node, Interval<T> query, List<Interval<T>> results)
    {
        if(node is null || node.MaxEnd.CompareTo(query.Start) < 0)
        {
            return;
        }

        FindOverlapping(node.Left, query, results);

        if(node.Interval.Overlaps(query))
        {
            results.Add(node.Interval);
        }

        if(node.Interval.Start.CompareTo(query.End) <= 0)
        {
            FindOverlapping(node.Right, query, results);
        }
    }

    private static Node Insert(Node? node, Interval<T> interval)
    {
        if(node is null)
        {
            return new Node(interval);
        }

        if(interval.Start.CompareTo(node.Interval.Start) < 0)
        {
            node.Left = Insert(node.Left, interval);
        } else
        {
            node.Right = Insert(node.Right, interval);
        }

        node.MaxEnd = ComputeMaxEnd(node);
        return node;
    }
    #endregion

    #region Public methods
    ///<summary>
    ///Adds an interval to the tree.
    ///</summary>
    ///<param name="interval">The interval to add.</param>
    public void Add(Interval<T> interval)
    {
        _root = Insert(_root, interval);
        _count++;
    }

    ///<summary>
    ///Removes every interval from the tree.
    ///</summary>
    public void Clear()
    {
        _root = null;
        _count = 0;
    }

    ///<summary>
    ///Finds every stored interval that overlaps the specified query interval.
    ///</summary>
    ///<param name="query">The interval to test for overlap.</param>
    ///<returns>The overlapping intervals, in no particular order.</returns>
    public IReadOnlyList<Interval<T>> FindOverlapping(Interval<T> query)
    {
        List<Interval<T>> results = [];
        FindOverlapping(_root, query, results);
        return results;
    }
    #endregion

    #region Public properties
    ///<summary>
    ///Gets the number of intervals in the tree.
    ///</summary>
    public int Count => _count;
    #endregion

    private sealed class Node(Interval<T> interval)
    {
        #region Public properties
        public Interval<T> Interval { get; } = interval;

        public Node? Left { get; set; }

        public T MaxEnd { get; set; } = interval.End;

        public Node? Right { get; set; }
        #endregion
    }
}
