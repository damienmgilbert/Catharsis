using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Catharsis.DataStructures;

///<summary>
///A dictionary that maps each key to a collection of values, supporting one-to-many key-value relationships.
///</summary>
///<typeparam name="TKey">The type of the keys.</typeparam>
///<typeparam name="TValue">The type of the values.</typeparam>
public sealed class Multimap<TKey, TValue> : IEnumerable<KeyValuePair<TKey, IReadOnlyCollection<TValue>>> where TKey : notnull
{
    #region Fields
    readonly Dictionary<TKey, List<TValue>> _map;
    #endregion

    #region Constructors
    ///<summary>
    ///Initializes a new empty <see cref="Multimap{TKey, TValue}"/> using the default key equality comparer.
    ///</summary>
    public Multimap() : this(EqualityComparer<TKey>.Default)
    {
    }

    ///<summary>
    ///Initializes a new empty <see cref="Multimap{TKey, TValue}"/> with the specified key equality comparer.
    ///</summary>
    ///<param name="comparer">The comparer used for key equality.</param>
    ///<exception cref="ArgumentNullException">Thrown when <paramref name="comparer"/> is <c>null</c>.</exception>
    public Multimap(IEqualityComparer<TKey> comparer)
    {
        if(comparer is null)
        {
            throw new ArgumentNullException(nameof(comparer), "Equality comparer must not be null.");
        }

        _map = [with(comparer)];
    }
    #endregion

    #region Indexers
    ///<summary>
    ///Gets the values associated with the specified key.
    ///</summary>
    ///<param name="key">The key to look up.</param>
    ///<returns>A read-only collection of values for the key.</returns>
    ///<exception cref="KeyNotFoundException">Thrown when the key is not found.</exception>
    public IReadOnlyCollection<TValue> this[TKey key] => _map[key];
    #endregion

    #region Explicit interface implementations
    ///<inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
    #endregion

    #region Public methods
    ///<summary>
    ///Adds a value under the specified key.
    ///</summary>
    ///<param name="key">The key to associate the value with.</param>
    ///<param name="value">The value to add.</param>
    public void Add(TKey key, TValue value)
    {
        if(!_map.TryGetValue(key, out List<TValue>? list))
        {
            list = [];
            _map[key] = list;
        }

        list.Add(value);
    }

    ///<summary>
    ///Adds multiple values under the specified key.
    ///</summary>
    ///<param name="key">The key to associate the values with.</param>
    ///<param name="values">The values to add.</param>
    ///<exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is <c>null</c>.</exception>
    public void AddRange(TKey key, IEnumerable<TValue> values)
    {
        if(values is null)
        {
            throw new ArgumentNullException(nameof(values), "Values must not be null.");
        }

        if(!_map.TryGetValue(key, out List<TValue>? list))
        {
            list = [];
            _map[key] = list;
        }

        list.AddRange(values);
    }

    ///<summary>
    ///Removes all keys and values from the multimap.
    ///</summary>
    public void Clear() { _map.Clear(); }
    ///<summary>
    ///Determines whether a specific value exists under the specified key.
    ///</summary>
    ///<param name="key">The key.</param>
    ///<param name="value">The value to look for.</param>
    ///<returns><c>true</c> if the key/value pair exists; otherwise <c>false</c>.</returns>
    public bool Contains(TKey key, TValue value) { return _map.TryGetValue(key, out List<TValue>? list) && list.Contains(value); }
    ///<summary>
    ///Determines whether the multimap contains the specified key.
    ///</summary>
    ///<param name="key">The key to look for.</param>
    ///<returns><c>true</c> if the key exists; otherwise <c>false</c>.</returns>
    public bool ContainsKey(TKey key) { return _map.ContainsKey(key); }

    ///<summary>
    ///Enumerates all key-to-values groupings.
    ///</summary>
    ///<inheritdoc/>
    public IEnumerator<KeyValuePair<TKey, IReadOnlyCollection<TValue>>> GetEnumerator()
    {
        foreach (var (key, list) in _map)
        {
            yield return new KeyValuePair<TKey, IReadOnlyCollection<TValue>>(key, list);
        }
    }

    ///<summary>
    ///Removes a specific value from under the specified key. If the last value for that key is removed, the key is also
    ///removed.
    ///</summary>
    ///<param name="key">The key.</param>
    ///<param name="value">The value to remove.</param>
    ///<returns><c>true</c> if the value was found and removed; otherwise <c>false</c>.</returns>
    public bool Remove(TKey key, TValue value)
    {
        if(!_map.TryGetValue(key, out List<TValue>? list))
        {
            return false;
        }

        if(!list.Remove(value))
        {
            return false;
        }

        if(list.Count == 0)
        {
            _map.Remove(key);
        }

        return true;
    }

    ///<summary>
    ///Removes all values associated with the specified key.
    ///</summary>
    ///<param name="key">The key to remove.</param>
    ///<returns><c>true</c> if the key was found and removed; otherwise <c>false</c>.</returns>
    public bool RemoveAll(TKey key) { return _map.Remove(key); }

    ///<summary>
    ///Attempts to get the values associated with the specified key.
    ///</summary>
    ///<param name="key">The key to look up.</param>
    ///<param name="values">The values, if found.</param>
    ///<returns><c>true</c> if the key exists; otherwise <c>false</c>.</returns>
    public bool TryGetValues(TKey key, [MaybeNullWhen(false)] out IReadOnlyCollection<TValue> values)
    {
        if(_map.TryGetValue(key, out List<TValue>? list))
        {
            values = list;
            return true;
        }

        values = null;
        return false;
    }
    #endregion

    #region Public properties
    ///<summary>
    ///Gets the number of distinct keys in the multimap.
    ///</summary>
    public int KeyCount => _map.Count;

    ///<summary>
    ///Gets all distinct keys in the multimap.
    ///</summary>
    public IEnumerable<TKey> Keys => _map.Keys;

    ///<summary>
    ///Gets the total number of values across all keys.
    ///</summary>
    public int ValueCount
    {
        get
        {
            int count = 0;

            foreach(List<TValue> list in _map.Values)
            {
                count += list.Count;
            }

            return count;
        }
    }
    #endregion
}
