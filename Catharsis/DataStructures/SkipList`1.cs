using System.Collections;

namespace Catharsis.DataStructures;

///<summary>
///A probabilistic ordered collection backed by multiple linked-list levels, giving expected O(log n) search, insertion,
///and removal without the rebalancing logic a balanced tree requires. Duplicate values are allowed. Enumeration yields
///items in ascending order.
///</summary>
///<typeparam name="T">The type of element stored in the list.</typeparam>
///<param name="comparer">The comparer used to order elements, or <c>null</c> to use <see cref="Comparer{T}.Default"/>.</param>
///<param name="random">The random source used to assign each new node's level, or <c>null</c> to use <see cref="Random.Shared"/>.</param>
///<example>
public sealed class SkipList<T>(IComparer<T>? comparer = null, Random? random = null) : IEnumerable<T>, IReadOnlyCollection<T>
{
    #region Constants
    private const double LevelProbability = 0.5;
    private const int MaxLevel = 32;
    #endregion

    #region Fields
    private readonly IComparer<T> _comparer = comparer ?? Comparer<T>.Default;
    private int _count;
    private readonly Node _head = new(default!, MaxLevel);
    private readonly Random _random = random ?? Random.Shared;
    private int _topLevel;
    #endregion

    #region Explicit interface implementations
    ///<inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    #endregion

    #region Private methods
    private int RandomLevel()
    {
        int level = 0;

        while(level < MaxLevel - 1 && _random.NextDouble() < LevelProbability)
        {
            level++;
        }

        return level;
    }
    #endregion

    #region Public methods
    ///<summary>
    ///Adds a value to the list.
    ///</summary>
    ///<param name="value">The value to add.</param>
    public void Add(T value)
    {
        Node[] update = new Node[MaxLevel];
        Node current = _head;

        for(int i = _topLevel; i >= 0; i--)
        {
            while(current.Next[i] is not null && _comparer.Compare(current.Next[i]!.Value, value) < 0)
            {
                current = current.Next[i]!;
            }

            update[i] = current;
        }

        int newLevel = RandomLevel();

        if(newLevel > _topLevel)
        {
            for(int i = _topLevel + 1; i <= newLevel; i++)
            {
                update[i] = _head;
            }

            _topLevel = newLevel;
        }

        Node newNode = new(value, newLevel + 1);

        for(int i = 0; i <= newLevel; i++)
        {
            newNode.Next[i] = update[i].Next[i];
            update[i].Next[i] = newNode;
        }

        _count++;
    }

    ///<summary>
    ///Removes all values from the list.
    ///</summary>
    public void Clear()
    {
        Array.Clear(_head.Next);
        _topLevel = 0;
        _count = 0;
    }

    ///<summary>
    ///Determines whether the list contains the specified value.
    ///</summary>
    ///<param name="value">The value to look for.</param>
    ///<returns><c>true</c> if the value is present; otherwise <c>false</c>.</returns>
    public bool Contains(T value)
    {
        Node current = _head;

        for(int i = _topLevel; i >= 0; i--)
        {
            while(current.Next[i] is not null && _comparer.Compare(current.Next[i]!.Value, value) < 0)
            {
                current = current.Next[i]!;
            }
        }

        Node? candidate = current.Next[0];
        return candidate is not null && _comparer.Compare(candidate.Value, value) == 0;
    }

    ///<inheritdoc/>
    public IEnumerator<T> GetEnumerator()
    {
        Node? current = _head.Next[0];

        while(current is not null)
        {
            yield return current.Value;
            current = current.Next[0];
        }
    }

    ///<summary>
    ///Removes a single occurrence of the specified value.
    ///</summary>
    ///<param name="value">The value to remove.</param>
    ///<returns><c>true</c> if a matching value was found and removed; otherwise <c>false</c>.</returns>
    public bool Remove(T value)
    {
        Node[] update = new Node[MaxLevel];
        Node current = _head;

        for(int i = _topLevel; i >= 0; i--)
        {
            while(current.Next[i] is not null && _comparer.Compare(current.Next[i]!.Value, value) < 0)
            {
                current = current.Next[i]!;
            }

            update[i] = current;
        }

        Node? target = update[0].Next[0];

        if(target is null || _comparer.Compare(target.Value, value) != 0)
        {
            return false;
        }

        for(int i = 0; i <= _topLevel; i++)
        {
            if(update[i].Next[i] != target)
            {
                break;
            }

            update[i].Next[i] = target.Next[i];
        }

        while(_topLevel > 0 && _head.Next[_topLevel] is null)
        {
            _topLevel--;
        }

        _count--;
        return true;
    }
    #endregion

    #region Public properties
    ///<summary>
    ///Gets the number of values in the list.
    ///</summary>
    public int Count => _count;
    #endregion

    private sealed class Node(T value, int levels)
    {
        #region Public properties
        public Node?[] Next { get; } = new Node?[levels];

        public T Value { get; } = value;
        #endregion
    }
}
