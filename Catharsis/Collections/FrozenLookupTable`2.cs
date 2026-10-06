using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;

namespace Catharsis.Collections;

///<summary>
///A fluent-builder wrapper over <see cref="FrozenDictionary{TKey,TValue}"/> for build-once reference tables: add
///entries via <see cref="Add"/>, then call <see cref="Build"/> once to switch to fast, immutable lookups.
///</summary>
///<typeparam name="TKey">The type of keys in the table.</typeparam>
///<typeparam name="TValue">The type of values in the table.</typeparam>
///<param name="comparer">The comparer used to compare keys. Defaults to the default comparer for <typeparamref name="TKey"/>.</param>
public sealed class FrozenLookupTable<TKey, TValue>(IEqualityComparer<TKey>? comparer = null) where TKey : notnull
{
    #region Fields
    Dictionary<TKey, TValue>? _building = new(comparer);
    FrozenDictionary<TKey, TValue>? _built;
    #endregion

    #region Public methods
    ///<summary>
    ///Adds or overwrites an entry in the table.
    ///</summary>
    ///<param name="key">The entry's key.</param>
    ///<param name="value">The entry's value.</param>
    ///<returns>This table, for fluent chaining.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="key"/> is <c>null</c>.</exception>
    ///<exception cref="InvalidOperationException"><see cref="Build"/> has already been called.</exception>
    public FrozenLookupTable<TKey, TValue> Add(TKey key, TValue value)
    {
        ArgumentNullException.ThrowIfNull(key);

        if (_building is null)
        {
            throw new InvalidOperationException("The table has already been built.");
        }

        _building[key] = value;
        return this;
    }

    ///<summary>
    ///Freezes the accumulated entries into a <see cref="FrozenDictionary{TKey,TValue}"/> for fast lookups. Idempotent:
    ///calling this more than once has no additional effect.
    ///</summary>
    ///<returns>This table, for fluent chaining.</returns>
    public FrozenLookupTable<TKey, TValue> Build()
    {
        if (_building is not null)
        {
            _built = _building.ToFrozenDictionary(_building.Comparer);
            _building = null;
        }

        return this;
    }

    ///<summary>
    ///Attempts to get the value associated with the specified key.
    ///</summary>
    ///<param name="key">The key to look up.</param>
    ///<param name="value">The value associated with <paramref name="key"/>, if found.</param>
    ///<returns><c>true</c> if the key was found; otherwise <c>false</c>.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="key"/> is <c>null</c>.</exception>
    ///<exception cref="InvalidOperationException"><see cref="Build"/> has not been called yet.</exception>
    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        ArgumentNullException.ThrowIfNull(key);
        return RequireBuilt().TryGetValue(key, out value);
    }
    #endregion

    #region Private methods
    FrozenDictionary<TKey, TValue> RequireBuilt() => _built ?? throw new InvalidOperationException("Call Build() before looking up entries.");
    #endregion

    #region Public properties
    ///<summary>
    ///Whether <see cref="Build"/> has been called.
    ///</summary>
    public bool IsBuilt => _built is not null;

    ///<summary>
    ///The number of entries in the table.
    ///</summary>
    public int Count => _built?.Count ?? _building!.Count;

    ///<summary>
    ///Gets the value associated with the specified key.
    ///</summary>
    ///<exception cref="ArgumentNullException"><paramref name="key"/> is <c>null</c>.</exception>
    ///<exception cref="InvalidOperationException"><see cref="Build"/> has not been called yet.</exception>
    ///<exception cref="KeyNotFoundException">The key was not found.</exception>
    public TValue this[TKey key]
    {
        get
        {
            ArgumentNullException.ThrowIfNull(key);
            return RequireBuilt()[key];
        }
    }
    #endregion
}
