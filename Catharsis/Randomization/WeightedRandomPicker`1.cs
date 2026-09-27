namespace Catharsis.Randomization;

///<summary>
///Selects a random item from a weighted set, where items with a higher weight are proportionally more likely to be
///picked.
///</summary>
///<typeparam name="T">The type of item to pick.</typeparam>
///<example>
///<code>
///WeightedRandomPicker&lt;string&gt; loot = new();
///loot.Add("common", 70).Add("rare", 25).Add("legendary", 5);
///string drop = loot.Pick();
///</code>
///</example>
public sealed class WeightedRandomPicker<T>
{
    #region Fields
    readonly List<(T Item, double CumulativeWeight)> _entries = [];
    double _totalWeight;
    #endregion

    #region Public methods
    ///<summary>
    ///Adds an item with the specified weight.
    ///</summary>
    ///<param name="item">The item to add.</param>
    ///<param name="weight">The item's weight. Must be greater than zero.</param>
    ///<returns>The current picker, for fluent chaining.</returns>
    ///<exception cref="ArgumentOutOfRangeException"><paramref name="weight"/> is not greater than zero.</exception>
    public WeightedRandomPicker<T> Add(T item, double weight)
    {
        if(weight <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(weight), "Weight must be greater than zero.");
        }

        _totalWeight += weight;
        _entries.Add((item, _totalWeight));
        return this;
    }

    ///<summary>
    ///Picks a random item, proportionally weighted, in O(log n) via a binary search over the cumulative weights
    ///recorded at <see cref="Add"/> time.
    ///</summary>
    ///<param name="random">The random source to use, or <c>null</c> to use <see cref="Random.Shared"/>.</param>
    ///<returns>The picked item.</returns>
    ///<exception cref="InvalidOperationException">No items have been added.</exception>
    public T Pick(Random? random = null)
    {
        if(_entries.Count == 0)
        {
            throw new InvalidOperationException("No items have been added.");
        }

        double roll = (random ?? Random.Shared).NextDouble() * _totalWeight;

        int low = 0;
        int high = _entries.Count - 1;

        while(low < high)
        {
            int mid = low + ((high - low) / 2);

            if(_entries[mid].CumulativeWeight <= roll)
            {
                low = mid + 1;
            } else
            {
                high = mid;
            }
        }

        return _entries[low].Item;
    }
    #endregion

    #region Public properties
    ///<summary>
    ///Gets the number of items that have been added.
    ///</summary>
    public int Count => _entries.Count;
    #endregion
}
