using System.Collections;

namespace Catharsis.Collections;

///<summary>
///A dictionary that maps each key to a collection of values rather than a single value, adding values to a key's
///collection instead of overwriting it.
///</summary>
///<typeparam name="TKey">The type of the keys.</typeparam>
///<typeparam name="TValue">The type of the values associated with each key.</typeparam>
///<param name="comparer">The equality comparer used to match keys, or <c>null</c> to use the default comparer.</param>
public sealed class MultiValueDictionary<TKey, TValue>(IEqualityComparer<TKey>? comparer = null) : IEnumerable<KeyValuePair<TKey, IReadOnlyCollection<TValue>>> where TKey : notnull
{
    #region Fields
    private readonly Dictionary<TKey, List<TValue>> _map = new(comparer);
    #endregion

    #region Indexers
    ///<summary>
    ///Gets the values associated with the specified key.
    ///</summary>
    ///<param name="key">The key to look up.</param>
    ///<returns>The associated values.</returns>
    ///<exception cref="KeyNotFoundException"><paramref name="key"/> was not found.</exception>
    public IReadOnlyCollection<TValue> this[TKey key] => TryGetValues(key, out IReadOnlyCollection<TValue> values) ? values : throw new KeyNotFoundException($"Key '{key}' was not found.");
    #endregion

    #region Explicit interface implementations
    ///<inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    #endregion

    #region Public methods
    ///<summary>
    ///Adds a value to the collection associated with the specified key, creating the collection if this is the key's
    ///first value.
    ///</summary>
    ///<param name="key">The key of the entry.</param>
    ///<param name="value">The value to add.</param>
    ///<exception cref="ArgumentNullException"><paramref name="key"/> is <c>null</c>.</exception>
    public void Add(TKey key, TValue value)
    {
        ArgumentNullException.ThrowIfNull(key);

        if(!_map.TryGetValue(key, out List<TValue>? values))
        {
            values = [];
            _map[key] = values;
        }

        values.Add(value);
    }

    ///<summary>
    ///Removes all entries from the dictionary.
    ///</summary>
    public void Clear() => _map.Clear();

    ///<summary>
    ///Determines whether the dictionary contains the specified key.
    ///</summary>
    ///<param name="key">The key to look for.</param>
    ///<returns><c>true</c> if the key exists; otherwise <c>false</c>.</returns>
    public bool ContainsKey(TKey key) => _map.ContainsKey(key);

    ///<inheritdoc/>
    public IEnumerator<KeyValuePair<TKey, IReadOnlyCollection<TValue>>> GetEnumerator()
    {
        foreach(KeyValuePair<TKey, List<TValue>> entry in _map)
        {
            yield return new KeyValuePair<TKey, IReadOnlyCollection<TValue>>(entry.Key, entry.Value);
        }
    }

    ///<summary>
    ///Removes the first occurrence of the specified value from the specified key's collection. If the collection
    ///becomes empty, the key is removed entirely.
    ///</summary>
    ///<param name="key">The key of the entry.</param>
    ///<param name="value">The value to remove.</param>
    ///<returns><c>true</c> if the value was found and removed; otherwise <c>false</c>.</returns>
    public bool Remove(TKey key, TValue value)
    {
        ArgumentNullException.ThrowIfNull(key);

        if(!_map.TryGetValue(key, out List<TValue>? values))
        {
            return false;
        }

        bool removed = values.Remove(value);

        if(removed && values.Count == 0)
        {
            _map.Remove(key);
        }

        return removed;
    }

    ///<summary>
    ///Removes the specified key and all of its associated values.
    ///</summary>
    ///<param name="key">The key to remove.</param>
    ///<returns><c>true</c> if the key was found and removed; otherwise <c>false</c>.</returns>
    public bool RemoveKey(TKey key) => _map.Remove(key);

    ///<summary>
    ///Attempts to retrieve the values associated with the specified key.
    ///</summary>
    ///<param name="key">The key to look up.</param>
    ///<param name="values">The associated values, if the key was found.</param>
    ///<returns><c>true</c> if the key was found; otherwise <c>false</c>.</returns>
    public bool TryGetValues(TKey key, out IReadOnlyCollection<TValue> values)
    {
        if(_map.TryGetValue(key, out List<TValue>? list))
        {
            values = list;
            return true;
        }

        values = [];
        return false;
    }
    #endregion

    #region Public properties
    ///<summary>
    ///Gets the number of distinct keys in the dictionary.
    ///</summary>
    public int KeyCount => _map.Count;

        ///<summary>
///Gets the keys currently in the dictionary.
///</summary>
    public IReadOnlyCollection<TKey> Keys => _map.Keys;

    ///<summary>
    ///Gets the total number of values across every key.
    ///</summary>
    public int ValueCount
    {
        get
        {
            int total = 0;

            foreach(List<TValue> values in _map.Values)
            {
                total += values.Count;
            }

            return total;
        }
    }
    #endregion
}
