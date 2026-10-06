namespace Catharsis.Collections;

///<summary>
///A multiset that tracks how many times each item has been added, supporting increment/decrement counting and top-N
///frequency queries.
///</summary>
///<typeparam name="T">The type of item to count.</typeparam>
///<param name="comparer">The equality comparer used to match items, or <c>null</c> to use the default comparer.</param>
public sealed class FrequencyCounter<T>(IEqualityComparer<T>? comparer = null) where T : notnull
{
    #region Fields
    private readonly Dictionary<T, int> _counts = new(comparer);
    #endregion

    #region Public methods
    ///<summary>
    ///Increments the count for the specified item.
    ///</summary>
    ///<param name="item">The item to count.</param>
    ///<param name="count">The amount to add. Defaults to 1.</param>
    ///<exception cref="ArgumentNullException"><paramref name="item"/> is <c>null</c>.</exception>
    ///<exception cref="ArgumentOutOfRangeException"><paramref name="count"/> is less than 1.</exception>
    public void Add(T item, int count = 1)
    {
        ArgumentNullException.ThrowIfNull(item);

        if(count < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(count), "Count must be at least 1.");
        }

        _counts[item] = _counts.GetValueOrDefault(item) + count;
    }

    ///<summary>
    ///Removes all items and their counts.
    ///</summary>
    public void Clear() => _counts.Clear();

    ///<summary>
    ///Gets the current count for the specified item.
    ///</summary>
    ///<param name="item">The item to look up.</param>
    ///<returns>The item's count, or 0 if it has never been added.</returns>
    public int GetCount(T item) => _counts.GetValueOrDefault(item);

    ///<summary>
    ///Decrements the count for the specified item, removing it entirely once its count reaches zero.
    ///</summary>
    ///<param name="item">The item to decrement.</param>
    ///<param name="count">The amount to subtract. Defaults to 1.</param>
    ///<returns><c>true</c> if the item was present; otherwise <c>false</c>.</returns>
    ///<exception cref="ArgumentOutOfRangeException"><paramref name="count"/> is less than 1.</exception>
    public bool Remove(T item, int count = 1)
    {
        if(count < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(count), "Count must be at least 1.");
        }

        if(!_counts.TryGetValue(item, out int current))
        {
            return false;
        }

        if(count >= current)
        {
            _counts.Remove(item);
        } else
        {
            _counts[item] = current - count;
        }

        return true;
    }

    ///<summary>
    ///Returns the <paramref name="n"/> most frequently added items, most frequent first.
    ///</summary>
    ///<param name="n">The maximum number of items to return.</param>
    ///<returns>The top items with their counts, most frequent first.</returns>
    ///<exception cref="ArgumentOutOfRangeException"><paramref name="n"/> is less than 1.</exception>
    public IEnumerable<(T Item, int Count)> Top(int n)
    {
        if(n < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(n), "Count must be at least 1.");
        }

        return _counts
            .OrderByDescending(static entry => entry.Value)
            .Take(n)
            .Select(static entry => (entry.Key, entry.Value));
    }
    #endregion

    #region Public properties
    ///<summary>
    ///Gets the number of distinct items being tracked.
    ///</summary>
    public int DistinctCount => _counts.Count;

    ///<summary>
    ///Gets the sum of every item's count.
    ///</summary>
    public int TotalCount => _counts.Values.Sum();
    #endregion
}
