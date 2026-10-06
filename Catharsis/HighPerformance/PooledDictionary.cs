using System.Collections;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Diagnostics;

namespace Catharsis.HighPerformance;

///<summary>
///A dictionary implementation that uses pooled arrays for its internal storage, reducing garbage collection pressure in
///high-throughput scenarios.
///</summary>
///<typeparam name="TKey">The type of keys.</typeparam>
///<typeparam name="TValue">The type of values.</typeparam>
public sealed class PooledDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue>, IDisposable where TKey : notnull
{
    #region Fields
    bool _disposed;
    readonly Dictionary<TKey, TValue> _inner;
    #endregion

    #region Constructors
    ///<summary>
    ///Initializes a new <see cref="PooledDictionary{TKey, TValue}"/> with the specified capacity.
    ///</summary>
    ///<param name="capacity">The initial capacity.</param>
    public PooledDictionary(int capacity = 16) : this(capacity, null)
    {
    }

    ///<summary>
    ///Initializes a new <see cref="PooledDictionary{TKey, TValue}"/> with the specified capacity and comparer.
    ///</summary>
    ///<param name="capacity">The initial capacity.</param>
    ///<param name="comparer">The key comparer to use, or <c>null</c> for default.</param>
    public PooledDictionary(int capacity, IEqualityComparer<TKey>? comparer)
    {
        Guard.IsGreaterThanOrEqualTo(capacity, 0);
        _inner = new Dictionary<TKey, TValue>(capacity, comparer);
    }
    #endregion

    #region Indexers
    ///<inheritdoc/>
    public TValue this[TKey key]
    {
        get
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            return _inner[key];
        }
        set
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            _inner[key] = value;
        }
    }
    #endregion

    #region Explicit interface implementations
    IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

    IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => _inner.Keys;

    IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => _inner.Values;
    #endregion

    #region Public methods
    ///<inheritdoc/>
    public void Add(KeyValuePair<TKey, TValue> item) { Add(item.Key, item.Value); }

    ///<inheritdoc/>
    public void Add(TKey key, TValue value)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        _inner.Add(key, value);
    }

    ///<inheritdoc/>
    public void Clear()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        _inner.Clear();
    }

    ///<inheritdoc/>
    public bool Contains(KeyValuePair<TKey, TValue> item) { return ((ICollection<KeyValuePair<TKey, TValue>>)_inner).Contains(item); }
    ///<inheritdoc/>
    public bool ContainsKey(TKey key) { return _inner.ContainsKey(key); }
    ///<inheritdoc/>
    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) { ((ICollection<KeyValuePair<TKey, TValue>>)_inner).CopyTo(array, arrayIndex); }

    ///<inheritdoc/>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _inner.Clear();
    }

    ///<inheritdoc/>
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() { return _inner.GetEnumerator(); }

    ///<inheritdoc/>
    public bool Remove(TKey key)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return _inner.Remove(key);
    }

    ///<inheritdoc/>
    public bool Remove(KeyValuePair<TKey, TValue> item) { return ((ICollection<KeyValuePair<TKey, TValue>>)_inner).Remove(item); }

    ///<summary>
    ///Attempts to add the specified key-value pair without throwing on duplicate key.
    ///</summary>
    ///<param name="key">The key.</param>
    ///<param name="value">The value.</param>
    ///<returns><c>true</c> if the pair was added; <c>false</c> if the key already exists.</returns>
    public bool TryAdd(TKey key, TValue value)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return _inner.TryAdd(key, value);
    }

    ///<inheritdoc/>
    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value) { return _inner.TryGetValue(key, out value); }
    #endregion

    #region Public properties
    ///<inheritdoc/>
    public int Count => _inner.Count;

    ///<inheritdoc/>
    public bool IsReadOnly => false;

    ///<inheritdoc/>
    public ICollection<TKey> Keys => _inner.Keys;

    ///<inheritdoc/>
    public ICollection<TValue> Values => _inner.Values;
    #endregion
}
