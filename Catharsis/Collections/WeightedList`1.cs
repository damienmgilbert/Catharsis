using System.Collections;

namespace Catharsis.Collections;

///<summary>
///A mutable collection of items paired with a weight, supporting proportionally-weighted random selection. Unlike
///<see cref="Catharsis.Randomization.WeightedRandomPicker{T}"/>, items can be removed after being added; the
///cumulative-weight table used for picking is rebuilt lazily after a mutation.
///</summary>
///<typeparam name="T">The type of item stored in the list.</typeparam>
///<example>
///<code>
///WeightedList&lt;string&gt; loot = new();
///loot.Add("common", 70);
///loot.Add("rare", 25);
///loot.Add("legendary", 5);
///string drop = loot.PickRandom();
///</code>
///</example>
public sealed class WeightedList<T> : IEnumerable<T>, IReadOnlyCollection<T>
{
    #region Fields
    readonly List<(T Item, double Weight)> _items = [];
    double[]? _cumulativeWeights;
    double _totalWeight;
    #endregion

    #region Explicit interface implementations
    ///<inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    #endregion

    #region Public methods
    ///<summary>
    ///Adds an item with the specified weight.
    ///</summary>
    ///<param name="item">The item to add.</param>
    ///<param name="weight">The item's weight. Must be greater than zero.</param>
    ///<exception cref="ArgumentOutOfRangeException"><paramref name="weight"/> is not greater than zero.</exception>
    public void Add(T item, double weight)
    {
        if(weight <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(weight), "Weight must be greater than zero.");
        }

        _items.Add((item, weight));
        _totalWeight += weight;
        _cumulativeWeights = null;
    }

    ///<summary>
    ///Removes all items from the list.
    ///</summary>
    public void Clear()
    {
        _items.Clear();
        _totalWeight = 0;
        _cumulativeWeights = null;
    }

    ///<inheritdoc/>
    public IEnumerator<T> GetEnumerator()
    {
        foreach((T item, double _) in _items)
        {
            yield return item;
        }
    }

    ///<summary>
    ///Picks a random item, proportionally weighted.
    ///</summary>
    ///<param name="random">The random source to use, or <c>null</c> to use <see cref="Random.Shared"/>.</param>
    ///<returns>The picked item.</returns>
    ///<exception cref="InvalidOperationException">The list is empty.</exception>
    public T PickRandom(Random? random = null)
    {
        if(_items.Count == 0)
        {
            throw new InvalidOperationException("The list is empty.");
        }

        double[] cumulative = EnsureCumulativeWeights();
        double roll = (random ?? Random.Shared).NextDouble() * _totalWeight;

        int index = Array.BinarySearch(cumulative, roll);

        if(index < 0)
        {
            index = ~index;
        }

        return _items[Math.Min(index, _items.Count - 1)].Item;
    }

    ///<summary>
    ///Removes the first occurrence of the specified item.
    ///</summary>
    ///<param name="item">The item to remove.</param>
    ///<returns><c>true</c> if the item was found and removed; otherwise <c>false</c>.</returns>
    public bool Remove(T item)
    {
        int index = _items.FindIndex(entry => EqualityComparer<T>.Default.Equals(entry.Item, item));

        if(index < 0)
        {
            return false;
        }

        _totalWeight -= _items[index].Weight;
        _items.RemoveAt(index);
        _cumulativeWeights = null;
        return true;
    }

    double[] EnsureCumulativeWeights()
    {
        if(_cumulativeWeights is not null)
        {
            return _cumulativeWeights;
        }

        double[] cumulative = new double[_items.Count];
        double running = 0;

        for(int i = 0; i < _items.Count; i++)
        {
            running += _items[i].Weight;
            cumulative[i] = running;
        }

        _cumulativeWeights = cumulative;
        return cumulative;
    }
    #endregion

    #region Public properties
    ///<summary>
    ///Gets the number of items in the list.
    ///</summary>
    public int Count => _items.Count;

    ///<summary>
    ///Gets the sum of every item's weight.
    ///</summary>
    public double TotalWeight => _totalWeight;
    #endregion
}
