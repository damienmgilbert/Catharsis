using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;

namespace Catharsis.ComponentModel;

///<summary>
///A strongly-typed façade over an <see cref="ExpandoObject"/> for ad hoc property bags: a typed indexer for
///programmatic access, backed by the same dynamic storage that also supports dynamic member access via
///<see cref="AsDynamic"/> (e.g. <c>((dynamic)bag.AsDynamic).PropertyName</c>).
///</summary>
///<typeparam name="T">The type of values stored in the bag.</typeparam>
public sealed class ExpandoBackedBag<T> : IEnumerable<KeyValuePair<string, T>>
{
    #region Fields
    readonly ExpandoObject _expando = new();
    #endregion

    #region Private methods
    IDictionary<string, object?> Storage => _expando;
    #endregion

    #region Public methods
    ///<summary>
    ///Determines whether an entry with the specified key exists.
    ///</summary>
    ///<param name="key">The key to look for.</param>
    ///<exception cref="ArgumentNullException"><paramref name="key"/> is <c>null</c>.</exception>
    public bool ContainsKey(string key)
    {
        ArgumentNullException.ThrowIfNull(key);
        return Storage.ContainsKey(key);
    }

    ///<inheritdoc/>
    public IEnumerator<KeyValuePair<string, T>> GetEnumerator()
    {
        foreach(KeyValuePair<string, object?> entry in Storage)
        {
            if(entry.Value is T typed)
            {
                yield return new KeyValuePair<string, T>(entry.Key, typed);
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    ///<summary>
    ///Removes the entry with the specified key, if present.
    ///</summary>
    ///<param name="key">The key to remove.</param>
    ///<returns><c>true</c> if an entry was removed; otherwise <c>false</c>.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="key"/> is <c>null</c>.</exception>
    public bool Remove(string key)
    {
        ArgumentNullException.ThrowIfNull(key);
        return Storage.Remove(key);
    }

    ///<summary>
    ///Attempts to get the value associated with the specified key.
    ///</summary>
    ///<param name="key">The key to look up.</param>
    ///<param name="value">The value associated with <paramref name="key"/>, if found and of type <typeparamref name="T"/>.</param>
    ///<returns><c>true</c> if the key was found and its value is of type <typeparamref name="T"/>; otherwise <c>false</c>.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="key"/> is <c>null</c>.</exception>
    public bool TryGetValue(string key, [MaybeNullWhen(false)] out T value)
    {
        ArgumentNullException.ThrowIfNull(key);

        if(Storage.TryGetValue(key, out object? raw) && (raw is T typed))
        {
            value = typed;
            return true;
        }

        value = default;
        return false;
    }
    #endregion

    #region Public properties
    ///<summary>
    ///Exposes this bag's storage as a <c>dynamic</c> value, so its entries can also be accessed as
    ///<c>((dynamic)bag.AsDynamic).PropertyName</c>.
    ///</summary>
    public dynamic AsDynamic => _expando;

    ///<summary>
    ///The number of entries in the bag.
    ///</summary>
    public int Count => Storage.Count;

    ///<summary>
    ///Gets or sets the value associated with the specified key.
    ///</summary>
    ///<exception cref="ArgumentNullException"><paramref name="key"/> is <c>null</c>.</exception>
    ///<exception cref="KeyNotFoundException">On get, the key was not found or its value is not of type <typeparamref name="T"/>.</exception>
    public T this[string key]
    {
        get
        {
            ArgumentNullException.ThrowIfNull(key);

            if(!TryGetValue(key, out T? value))
            {
                throw new KeyNotFoundException($"The key '{key}' was not found.");
            }

            return value;
        }
        set
        {
            ArgumentNullException.ThrowIfNull(key);
            Storage[key] = value;
        }
    }
    #endregion
}
