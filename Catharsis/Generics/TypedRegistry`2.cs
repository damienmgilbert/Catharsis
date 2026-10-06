using System.Collections;
using System.Collections.Concurrent;

namespace Catharsis.Generics;

///<summary>
///A thread-safe lookup of values by key that treats a duplicate registration as a bug: <see cref="Register"/> throws
///rather than silently replacing an earlier entry. Use <see cref="Set"/> when replacement is intended.
///</summary>
///<typeparam name="TKey">The key type. Must not be <c>null</c>.</typeparam>
///<typeparam name="TValue">The value type.</typeparam>
///<param name="comparer">Compares keys, or <c>null</c> for the default comparer.</param>
public sealed class TypedRegistry<TKey, TValue>(IEqualityComparer<TKey>? comparer) : IEnumerable<KeyValuePair<TKey, TValue>> where TKey : notnull
{
    #region Fields
    private readonly ConcurrentDictionary<TKey, TValue> _items = new(comparer);
    #endregion

    #region Constructors
    ///<summary>
    ///Creates an empty registry using the default key comparer.
    ///</summary>
    public TypedRegistry() : this(null)
    {
    }
    #endregion

    #region Indexers
    ///<summary>
    ///Gets the value for <paramref name="key"/>.
    ///</summary>
    ///<exception cref="ArgumentNullException"><paramref name="key"/> is <c>null</c>.</exception>
    ///<exception cref="KeyNotFoundException">The key is not registered.</exception>
    public TValue this[TKey key]
    {
        get
        {
            ArgumentNullException.ThrowIfNull(key);

            return _items.TryGetValue(key, out TValue? value) ? value : throw new KeyNotFoundException($"'{key}' is not registered.");
        }
    }
    #endregion

    #region Explicit interface implementations
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    #endregion

    #region Public methods
    ///<summary>
    ///Determines whether the key is registered.
    ///</summary>
    ///<exception cref="ArgumentNullException"><paramref name="key"/> is <c>null</c>.</exception>
    public bool Contains(TKey key)
    {
        ArgumentNullException.ThrowIfNull(key);

        return _items.ContainsKey(key);
    }

    ///<inheritdoc/>
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _items.GetEnumerator();

    ///<summary>
    ///Gets an entry, creating it with <paramref name="factory"/> if it is missing.
    ///</summary>
    ///<exception cref="ArgumentNullException">An argument is <c>null</c>.</exception>
    public TValue GetOrAdd(TKey key, Func<TKey, TValue> factory)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(factory);

        return _items.GetOrAdd(key, factory);
    }

        ///<summary>
///Adds a new entry.
///</summary>
    ///<param name="key">The key.</param>
    ///<param name="value">The value.</param>
    ///<exception cref="ArgumentNullException"><paramref name="key"/> is <c>null</c>.</exception>
    ///<exception cref="InvalidOperationException">The key is already registered.</exception>
    public void Register(TKey key, TValue value)
    {
        ArgumentNullException.ThrowIfNull(key);

        if(!_items.TryAdd(key, value))
        {
            throw new InvalidOperationException($"'{key}' is already registered.");
        }
    }

    ///<summary>
    ///Removes an entry.
    ///</summary>
    ///<returns><c>true</c> if the key was registered.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="key"/> is <c>null</c>.</exception>
    public bool Remove(TKey key)
    {
        ArgumentNullException.ThrowIfNull(key);

        return _items.TryRemove(key, out _);
    }

    ///<summary>
    ///Adds or replaces an entry.
    ///</summary>
    ///<exception cref="ArgumentNullException"><paramref name="key"/> is <c>null</c>.</exception>
    public void Set(TKey key, TValue value)
    {
        ArgumentNullException.ThrowIfNull(key);

        _items[key] = value;
    }

    ///<summary>
    ///Attempts to read an entry.
    ///</summary>
    ///<returns><c>true</c> if the key is registered.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="key"/> is <c>null</c>.</exception>
    public bool TryGet(TKey key, out TValue value)
    {
        ArgumentNullException.ThrowIfNull(key);

        return _items.TryGetValue(key, out value!);
    }
    #endregion

    #region Public properties
    ///<summary>
    ///Gets the number of entries.
    ///</summary>
    public int Count => _items.Count;

    ///<summary>
    ///Gets a snapshot of the registered keys.
    ///</summary>
    public IReadOnlyCollection<TKey> Keys => [ .. _items.Keys ];
    #endregion
}
