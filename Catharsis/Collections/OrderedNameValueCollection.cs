using System.Collections;
using System.Collections.Specialized;

namespace Catharsis.Collections;

///<summary>
///A strongly-typed, insertion-order-preserving string-to-string collection, backed by <see cref="OrderedDictionary"/>
///rather than <see cref="Dictionary{TKey,TValue}"/> (whose enumeration order is not guaranteed) or ///<see
///cref="NameValueCollection"/> (which has no indexed access by position).
///</summary>
public sealed class OrderedNameValueCollection : IEnumerable<KeyValuePair<string, string>>
{
    #region Fields
    private readonly OrderedDictionary _inner = new();
    #endregion

    #region Indexers
    ///<summary>
    ///Gets or sets the value associated with the specified key. Setting a key that does not exist yet appends it at the
    ///end, in insertion order.
    ///</summary>
    ///<exception cref="ArgumentNullException"><paramref name="key"/> or, on set, the value is <c>null</c>.</exception>
    ///<exception cref="KeyNotFoundException">On get, the key was not found.</exception>
    public string this[string key]
    {
        get
        {
            ArgumentNullException.ThrowIfNull(key);

            if(!_inner.Contains(key))
            {
                throw new KeyNotFoundException($"The key '{key}' was not found.");
            }

            return (string)_inner[key]!;
        }
        set
        {
            ArgumentNullException.ThrowIfNull(key);
            ArgumentNullException.ThrowIfNull(value);
            _inner[key] = value;
        }
    }
    #endregion

    #region Explicit interface implementations
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    #endregion

    #region Public methods
    ///<summary>
    ///Adds a new entry. The key must not already be present.
    ///</summary>
    ///<param name="key">The entry's key.</param>
    ///<param name="value">The entry's value.</param>
    ///<exception cref="ArgumentNullException"><paramref name="key"/> or <paramref name="value"/> is <c>null</c>.</exception>
    ///<exception cref="ArgumentException">An entry with <paramref name="key"/> already exists.</exception>
    public void Add(string key, string value)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(value);
        _inner.Add(key, value);
    }

    ///<summary>
    ///Removes every entry.
    ///</summary>
    public void Clear() => _inner.Clear();

    ///<summary>
    ///Determines whether an entry with the specified key exists.
    ///</summary>
    ///<param name="key">The key to look for.</param>
    ///<exception cref="ArgumentNullException"><paramref name="key"/> is <c>null</c>.</exception>
    public bool ContainsKey(string key)
    {
        ArgumentNullException.ThrowIfNull(key);
        return _inner.Contains(key);
    }

    ///<summary>
    ///Gets the key/value pair at the specified position in insertion order.
    ///</summary>
    ///<param name="index">The zero-based position.</param>
    ///<exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is out of range.</exception>
    public KeyValuePair<string, string> GetAt(int index)
    {
        DictionaryEntry entry = _inner.Cast<DictionaryEntry>().ElementAt(index);
        return new KeyValuePair<string, string>((string)entry.Key, (string)entry.Value!);
    }

    ///<inheritdoc/>
    public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
    {
        foreach(DictionaryEntry entry in _inner)
        {
            yield return new KeyValuePair<string, string>((string)entry.Key, (string)entry.Value!);
        }
    }

    ///<summary>
    ///Removes the entry with the specified key, if present.
    ///</summary>
    ///<param name="key">The key to remove.</param>
    ///<returns><c>true</c> if an entry was removed; otherwise <c>false</c>.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="key"/> is <c>null</c>.</exception>
    public bool Remove(string key)
    {
        ArgumentNullException.ThrowIfNull(key);

        if(!_inner.Contains(key))
        {
            return false;
        }

        _inner.Remove(key);
        return true;
    }

    ///<summary>
    ///Removes the entry at the specified position in insertion order.
    ///</summary>
    ///<param name="index">The zero-based position.</param>
    ///<exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is out of range.</exception>
    public void RemoveAt(int index) => _inner.RemoveAt(index);
    #endregion

    #region Public properties
    ///<summary>
    ///The number of entries.
    ///</summary>
    public int Count => _inner.Count;
    #endregion
}
