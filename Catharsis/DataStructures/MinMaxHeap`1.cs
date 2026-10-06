namespace Catharsis.DataStructures;

///<summary>
///A double-ended priority queue supporting O(1) access and O(log n) removal of both the minimum and the maximum
///element, using the min-max heap structure (Atkinson, Sack, Santoro &amp; Strong, 1986). A single array-backed binary
///tree whose levels alternate between enforcing "smaller than every descendant" (even levels, starting with the root)
///and "larger than every descendant" (odd levels).
///</summary>
///<typeparam name="T">The type of element stored in the heap.</typeparam>
///<example>
public sealed class MinMaxHeap<T>
{
    #region Fields
    private readonly IComparer<T> _comparer;
    private readonly List<T> _items = [];
    #endregion

    #region Constructors
    ///<summary>
    ///Creates an empty heap using the default comparer for <typeparamref name="T"/>.
    ///</summary>
    public MinMaxHeap() : this((IComparer<T>?)null)
    {
    }
    ///<summary>
    ///Creates an empty heap using the specified comparer.
    ///</summary>
    ///<param name="comparer">The comparer used to order elements, or <c>null</c> to use <see cref="Comparer{T}.Default"/>.</param>
    public MinMaxHeap(IComparer<T>? comparer) { _comparer = comparer ?? Comparer<T>.Default; }

    ///<summary>
    ///Creates a heap containing the specified items, using the default comparer for <typeparamref name="T"/>.
    ///</summary>
    ///<param name="items">The items to add.</param>
    ///<exception cref="ArgumentNullException"><paramref name="items"/> is <c>null</c>.</exception>
    public MinMaxHeap(IEnumerable<T> items) : this(items, null)
    {
    }

    ///<summary>
    ///Creates a heap containing the specified items, using the specified comparer.
    ///</summary>
    ///<param name="items">The items to add.</param>
    ///<param name="comparer">The comparer used to order elements, or <c>null</c> to use <see cref="Comparer{T}.Default"/>.</param>
    ///<exception cref="ArgumentNullException"><paramref name="items"/> is <c>null</c>.</exception>
    public MinMaxHeap(IEnumerable<T> items, IComparer<T>? comparer)
    {
        ArgumentNullException.ThrowIfNull(items);
        _comparer = comparer ?? Comparer<T>.Default;

        foreach(T item in items)
        {
            Add(item);
        }
    }
    #endregion

    #region Private methods
    private void EnsureNotEmpty()
    {
        if(_items.Count == 0)
        {
            throw new InvalidOperationException("The heap is empty.");
        }
    }

    private int GetChildrenAndGrandchildren(int i, Span<int> buffer)
    {
        int found = 0;
        int count = _items.Count;
        int left = LeftChild(i);
        int right = RightChild(i);

        if(left < count)
        {
            buffer[found++] = left;
        }

        if(right < count)
        {
            buffer[found++] = right;
        }

        if(left < count)
        {
            int leftLeft = LeftChild(left);
            int leftRight = RightChild(left);

            if(leftLeft < count)
            {
                buffer[found++] = leftLeft;
            }

            if(leftRight < count)
            {
                buffer[found++] = leftRight;
            }
        }

        if(right < count)
        {
            int rightLeft = LeftChild(right);
            int rightRight = RightChild(right);

            if(rightLeft < count)
            {
                buffer[found++] = rightLeft;
            }

            if(rightRight < count)
            {
                buffer[found++] = rightRight;
            }
        }

        return found;
    }

    private static bool HasGrandparent(int i) => Parent(i) > 0;

    private static bool IsGrandchild(int i, int m) => m != LeftChild(i) && m != RightChild(i);

    private static bool IsMinLevel(int index)
    {
        int level = 0;
        long value = index + 1;

        while(value > 1)
        {
            value >>= 1;
            level++;
        }

        return level % 2 == 0;
    }

    private static int LeftChild(int i) => (2 * i) + 1;

    private int MaxIndex() => _items.Count switch
    {
        1 => 0,
        2 => 1,
        _ => _comparer.Compare(_items[1], _items[2]) >= 0 ? 1 : 2
    };

    private static int Parent(int i) => (i - 1) / 2;

    private void PushDownMax(int i)
    {
        Span<int> candidates = stackalloc int[6];
        int count = GetChildrenAndGrandchildren(i, candidates);

        if(count == 0)
        {
            return;
        }

        int m = candidates[0];

        for(int k = 1; k < count; k++)
        {
            if(_comparer.Compare(_items[candidates[k]], _items[m]) > 0)
            {
                m = candidates[k];
            }
        }

        if(IsGrandchild(i, m))
        {
            if(_comparer.Compare(_items[m], _items[i]) > 0)
            {
                Swap(m, i);
                int parentOfM = Parent(m);

                if(_comparer.Compare(_items[m], _items[parentOfM]) < 0)
                {
                    Swap(m, parentOfM);
                }

                PushDownMax(m);
            }
        } else if(_comparer.Compare(_items[m], _items[i]) > 0)
        {
            Swap(m, i);
        }
    }

    private void PushDownMin(int i)
    {
        Span<int> candidates = stackalloc int[6];
        int count = GetChildrenAndGrandchildren(i, candidates);

        if(count == 0)
        {
            return;
        }

        int m = candidates[0];

        for(int k = 1; k < count; k++)
        {
            if(_comparer.Compare(_items[candidates[k]], _items[m]) < 0)
            {
                m = candidates[k];
            }
        }

        if(IsGrandchild(i, m))
        {
            if(_comparer.Compare(_items[m], _items[i]) < 0)
            {
                Swap(m, i);
                int parentOfM = Parent(m);

                if(_comparer.Compare(_items[m], _items[parentOfM]) > 0)
                {
                    Swap(m, parentOfM);
                }

                PushDownMin(m);
            }
        } else if(_comparer.Compare(_items[m], _items[i]) < 0)
        {
            Swap(m, i);
        }
    }

    private void PushUp(int i)
    {
        if(i == 0)
        {
            return;
        }

        int parent = Parent(i);

        if(IsMinLevel(i))
        {
            if(_comparer.Compare(_items[i], _items[parent]) > 0)
            {
                Swap(i, parent);
                PushUpMax(parent);
            } else
            {
                PushUpMin(i);
            }
        } else
        {
            if(_comparer.Compare(_items[i], _items[parent]) < 0)
            {
                Swap(i, parent);
                PushUpMin(parent);
            } else
            {
                PushUpMax(i);
            }
        }
    }

    private void PushUpMax(int i)
    {
        if(!HasGrandparent(i))
        {
            return;
        }

        int grandparent = Parent(Parent(i));

        if(_comparer.Compare(_items[i], _items[grandparent]) > 0)
        {
            Swap(i, grandparent);
            PushUpMax(grandparent);
        }
    }

    private void PushUpMin(int i)
    {
        if(!HasGrandparent(i))
        {
            return;
        }

        int grandparent = Parent(Parent(i));

        if(_comparer.Compare(_items[i], _items[grandparent]) < 0)
        {
            Swap(i, grandparent);
            PushUpMin(grandparent);
        }
    }

    private void RemoveAt(int index)
    {
        int lastIndex = _items.Count - 1;
        _items[index] = _items[lastIndex];
        _items.RemoveAt(lastIndex);

        if(index >= _items.Count)
        {
            return;
        }

        if(IsMinLevel(index))
        {
            PushDownMin(index);
        } else
        {
            PushDownMax(index);
        }
    }

    private static int RightChild(int i) => (2 * i) + 2;

    private void Swap(int i, int j) => (_items[i], _items[j]) = (_items[j], _items[i]);
    #endregion

    #region Public methods
    ///<summary>
    ///Adds an item to the heap.
    ///</summary>
    ///<param name="item">The item to add.</param>
    public void Add(T item)
    {
        _items.Add(item);
        PushUp(_items.Count - 1);
    }

    ///<summary>
    ///Removes all items from the heap.
    ///</summary>
    public void Clear() => _items.Clear();

    ///<summary>
    ///Removes and returns the maximum item.
    ///</summary>
    ///<returns>The maximum item.</returns>
    ///<exception cref="InvalidOperationException">The heap is empty.</exception>
    public T ExtractMax()
    {
        EnsureNotEmpty();
        int index = MaxIndex();
        T result = _items[index];
        RemoveAt(index);
        return result;
    }

    ///<summary>
    ///Removes and returns the minimum item.
    ///</summary>
    ///<returns>The minimum item.</returns>
    ///<exception cref="InvalidOperationException">The heap is empty.</exception>
    public T ExtractMin()
    {
        EnsureNotEmpty();
        T result = _items[0];
        RemoveAt(0);
        return result;
    }

    ///<summary>
    ///Returns the maximum item without removing it.
    ///</summary>
    ///<returns>The maximum item.</returns>
    ///<exception cref="InvalidOperationException">The heap is empty.</exception>
    public T PeekMax()
    {
        EnsureNotEmpty();
        return _items[MaxIndex()];
    }

    ///<summary>
    ///Returns the minimum item without removing it.
    ///</summary>
    ///<returns>The minimum item.</returns>
    ///<exception cref="InvalidOperationException">The heap is empty.</exception>
    public T PeekMin()
    {
        EnsureNotEmpty();
        return _items[0];
    }
    #endregion

    #region Public properties
    ///<summary>
    ///Gets the number of items in the heap.
    ///</summary>
    public int Count => _items.Count;
    #endregion
}
