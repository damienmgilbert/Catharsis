using CommunityToolkit.Diagnostics;
using System.Collections;

namespace Catharsis.Diagnostics;

///<summary>
///A capacity-checked wrapper around <see cref="Dictionary{TKey, TValue}"/> that uses CommunityToolkit <see cref="Guard"/>
///for key and capacity validation, analogous to how <see cref="CheckedSpan{T}"/> bounds-checks index access.
///</summary>
///<typeparam name="TKey">The type of the keys.</typeparam>
///<typeparam name="TValue">The type of the values.</typeparam>
public sealed class CheckedDictionary<TKey, TValue> : IDictionary<TKey, TValue> where TKey : notnull
{
    #region Fields
    readonly Dictionary<TKey, TValue> _inner;
    #endregion

    #region Constructors
    ///<summary>
    ///Initializes a new <see cref="CheckedDictionary{TKey, TValue}"/>.
    ///</summary>
    ///<param name="maxCapacity">The maximum number of entries allowed, or <c>null</c> for no limit.</param>
    ///<param name="mode">The validation mode to apply.</param>
    ///<param name="comparer">The equality comparer used to match keys, or <c>null</c> to use the default comparer.</param>
    ///<exception cref="ArgumentOutOfRangeException"><paramref name="maxCapacity"/> is not positive.</exception>
    public CheckedDictionary(int? maxCapacity = null, ValidationMode mode = ValidationMode.Full, IEqualityComparer<TKey>? comparer = null)
    {
        if(maxCapacity.HasValue)
        {
            Guard.IsGreaterThan(maxCapacity.Value, 0);
        }

        MaxCapacity = maxCapacity;
        Mode = mode;
        _inner = new Dictionary<TKey, TValue>(comparer);
    }
    #endregion

    #region Private methods
    void CheckCapacityForNewKey()
    {
        if((Mode == ValidationMode.Full) && MaxCapacity.HasValue && (_inner.Count >= MaxCapacity.Value))
        {
            throw new InvalidOperationException($"Adding this key would exceed the maximum capacity of {MaxCapacity.Value}.");
        }
    }
    #endregion

    #region Explicit interface implementations
    ///<inheritdoc/>
    bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => false;

    ///<inheritdoc/>
    void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);

    ///<inheritdoc/>
    bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item) => _inner.Contains(item);

    ///<inheritdoc/>
    void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) => ((ICollection<KeyValuePair<TKey, TValue>>)_inner).CopyTo(array, arrayIndex);

    ///<inheritdoc/>
    bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item) => ((ICollection<KeyValuePair<TKey, TValue>>)_inner).Remove(item);

    ///<inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    #endregion

    #region Public methods
    ///<summary>
    ///Adds the specified key and value, validating the key and the configured capacity limit first.
    ///</summary>
    ///<param name="key">The key of the entry to add.</param>
    ///<param name="value">The value of the entry to add.</param>
    ///<exception cref="ArgumentNullException"><paramref name="key"/> is <c>null</c>.</exception>
    ///<exception cref="ArgumentException">An entry with the same key already exists.</exception>
    ///<exception cref="InvalidOperationException">Adding the entry would exceed <see cref="MaxCapacity"/>.</exception>
    public void Add(TKey key, TValue value)
    {
        if(Mode >= ValidationMode.BoundsOnly)
        {
            Guard.IsNotNull(key);
        }

        CheckCapacityForNewKey();
        _inner.Add(key, value);
    }

    ///<summary>
    ///Removes all entries from the dictionary.
    ///</summary>
    public void Clear() => _inner.Clear();

    ///<summary>
    ///Determines whether the dictionary contains the specified key.
    ///</summary>
    ///<param name="key">The key to look for.</param>
    ///<returns><c>true</c> if the key exists; otherwise <c>false</c>.</returns>
    public bool ContainsKey(TKey key) => _inner.ContainsKey(key);

    ///<inheritdoc/>
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _inner.GetEnumerator();

    ///<summary>
    ///Removes the entry with the specified key.
    ///</summary>
    ///<param name="key">The key to remove.</param>
    ///<returns><c>true</c> if the entry was found and removed; otherwise <c>false</c>.</returns>
    public bool Remove(TKey key) => _inner.Remove(key);

    ///<summary>
    ///Attempts to retrieve the value associated with the specified key.
    ///</summary>
    ///<param name="key">The key to look up.</param>
    ///<param name="value">The associated value, if found.</param>
    ///<returns><c>true</c> if the key was found; otherwise <c>false</c>.</returns>
    public bool TryGetValue(TKey key, out TValue value) => _inner.TryGetValue(key, out value!);
    #endregion

    #region Indexers
    ///<summary>
    ///Gets or sets the value associated with the specified key. Setting a new key validates the configured capacity
    ///limit first.
    ///</summary>
    ///<param name="key">The key of the value to get or set.</param>
    ///<exception cref="ArgumentNullException"><paramref name="key"/> is <c>null</c>.</exception>
    ///<exception cref="KeyNotFoundException">The key was not found when getting the value.</exception>
    ///<exception cref="InvalidOperationException">Setting a new key would exceed <see cref="MaxCapacity"/>.</exception>
    public TValue this[TKey key]
    {
        get
        {
            if(Mode >= ValidationMode.BoundsOnly)
            {
                Guard.IsNotNull(key);
            }

            return _inner[key];
        }
        set
        {
            if(Mode >= ValidationMode.BoundsOnly)
            {
                Guard.IsNotNull(key);
            }

            if(!_inner.ContainsKey(key))
            {
                CheckCapacityForNewKey();
            }

            _inner[key] = value;
        }
    }
    #endregion

    #region Public properties
    ///<summary>
    ///Gets the number of entries in the dictionary.
    ///</summary>
    public int Count => _inner.Count;

    ///<summary>
    ///Gets the keys in the dictionary.
    ///</summary>
    public ICollection<TKey> Keys => _inner.Keys;

    ///<summary>
    ///Gets the maximum number of entries allowed, or <c>null</c> if unbounded.
    ///</summary>
    public int? MaxCapacity { get; }

    ///<summary>
    ///Gets the validation mode for this dictionary.
    ///</summary>
    public ValidationMode Mode { get; }

    ///<summary>
    ///Gets the values in the dictionary.
    ///</summary>
    public ICollection<TValue> Values => _inner.Values;
    #endregion
}
