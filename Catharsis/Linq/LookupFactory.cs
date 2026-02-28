using System.Collections;

namespace Catharsis.Linq;

///<summary>
///Provides factory methods for building <see cref="ILookup{TKey,TElement}"/> instances from sequences, dictionaries,
///groupings, and explicit key-element pairs.
///</summary>
public static class LookupFactory
{
    #region Public methods

    ///<summary>
    ///Creates an <see cref="ILookup{TKey,TElement}"/> by applying <paramref name="keySelector"/> to every element in
    ///<paramref name="source"/>.
    ///</summary>
    ///<typeparam name="TKey">The key type.</typeparam>
    ///<typeparam name="TElement">The element type.</typeparam>
    ///<param name="source">The source sequence.</param>
    ///<param name="keySelector">A function that extracts the key from each element.</param>
    ///<param name="comparer">An optional equality comparer for keys.</param>
    ///<returns>An <see cref="ILookup{TKey,TElement}"/>.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="keySelector"/> is <c>null</c>.</exception>
    public static ILookup<TKey, TElement> Create<TKey, TElement>(IEnumerable<TElement> source, Func<TElement, TKey> keySelector, IEqualityComparer<TKey>? comparer = null)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(keySelector, nameof(keySelector));
        return source.ToLookup(keySelector, comparer);
    }

    ///<summary>
    ///Creates an <see cref="ILookup{TKey,TElement}"/> by applying both a key selector and an element selector.
    ///</summary>
    ///<typeparam name="TSource">The source element type.</typeparam>
    ///<typeparam name="TKey">The key type.</typeparam>
    ///<typeparam name="TElement">The element type stored in the lookup.</typeparam>
    ///<param name="source">The source sequence.</param>
    ///<param name="keySelector">A function that extracts the key.</param>
    ///<param name="elementSelector">A function that projects the element.</param>
    ///<param name="comparer">An optional equality comparer for keys.</param>
    ///<returns>An <see cref="ILookup{TKey,TElement}"/>.</returns>
    ///<exception cref="ArgumentNullException">Any required argument is <c>null</c>.</exception>
    public static ILookup<TKey, TElement> Create<TSource, TKey, TElement>(IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey>? comparer = null)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(keySelector, nameof(keySelector));
        ArgumentNullException.ThrowIfNull(elementSelector, nameof(elementSelector));
        return source.ToLookup(keySelector, elementSelector, comparer);
    }

    ///<summary>
    ///Returns an empty <see cref="ILookup{TKey,TElement}"/>.
    ///</summary>
    ///<typeparam name="TKey">The key type.</typeparam>
    ///<typeparam name="TElement">The element type.</typeparam>
    ///<returns>An empty lookup.</returns>
    public static ILookup<TKey, TElement> Empty<TKey, TElement>() { return new EmptyLookup<TKey, TElement>(); }

    ///<summary>
    ///Creates an <see cref="ILookup{TKey,TElement}"/> from a dictionary. Each dictionary entry becomes a single-
    ///element group.
    ///</summary>
    ///<typeparam name="TKey">The key type.</typeparam>
    ///<typeparam name="TValue">The value type.</typeparam>
    ///<param name="dictionary">The source dictionary.</param>
    ///<param name="comparer">An optional equality comparer for keys.</param>
    ///<returns>An <see cref="ILookup{TKey,TValue}"/>.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="dictionary"/> is <c>null</c>.</exception>
    public static ILookup<TKey, TValue> FromDictionary<TKey, TValue>(IEnumerable<KeyValuePair<TKey, TValue>> dictionary, IEqualityComparer<TKey>? comparer = null)
    {
        ArgumentNullException.ThrowIfNull(dictionary, nameof(dictionary));
        return dictionary.ToLookup(kvp => kvp.Key, kvp => kvp.Value, comparer);
    }

    ///<summary>
    ///Creates an <see cref="ILookup{TKey,TElement}"/> from a dictionary of collections. Each entry's collection becomes
    ///the elements for that key.
    ///</summary>
    ///<typeparam name="TKey">The key type.</typeparam>
    ///<typeparam name="TElement">The element type.</typeparam>
    ///<param name="dictionary">A dictionary mapping keys to collections of elements.</param>
    ///<param name="comparer">An optional equality comparer for keys.</param>
    ///<returns>An <see cref="ILookup{TKey,TElement}"/>.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="dictionary"/> is <c>null</c>.</exception>
    public static ILookup<TKey, TElement> FromDictionaryOfCollections<TKey, TElement>(IEnumerable<KeyValuePair<TKey, IEnumerable<TElement>>> dictionary, IEqualityComparer<TKey>? comparer = null)
    {
        ArgumentNullException.ThrowIfNull(dictionary, nameof(dictionary));
        return dictionary
            .SelectMany(kvp => kvp.Value.Select(e => (kvp.Key, Element: e)))
            .ToLookup(pair => pair.Key, pair => pair.Element, comparer);
    }

    ///<summary>
    ///Creates an <see cref="ILookup{TKey,TElement}"/> from a sequence of <see cref="IGrouping{TKey,TElement}"/>
    ///instances. Duplicate keys across groupings are merged.
    ///</summary>
    ///<typeparam name="TKey">The key type.</typeparam>
    ///<typeparam name="TElement">The element type.</typeparam>
    ///<param name="groupings">The groupings to convert.</param>
    ///<param name="comparer">An optional equality comparer for keys.</param>
    ///<returns>An <see cref="ILookup{TKey,TElement}"/>.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="groupings"/> is <c>null</c>.</exception>
    public static ILookup<TKey, TElement> FromGroupings<TKey, TElement>(IEnumerable<IGrouping<TKey, TElement>> groupings, IEqualityComparer<TKey>? comparer = null)
    {
        ArgumentNullException.ThrowIfNull(groupings, nameof(groupings));
        return groupings
            .SelectMany(g => g.Select(e => (g.Key, Element: e)))
            .ToLookup(pair => pair.Key, pair => pair.Element, comparer);
    }

    ///<summary>
    ///Creates an <see cref="ILookup{TKey,TElement}"/> from a sequence of tuples. Multiple tuples with the same key are
    ///merged into a single group.
    ///</summary>
    ///<typeparam name="TKey">The key type.</typeparam>
    ///<typeparam name="TElement">The element type.</typeparam>
    ///<param name="pairs">The key-element pairs.</param>
    ///<param name="comparer">An optional equality comparer for keys.</param>
    ///<returns>An <see cref="ILookup{TKey,TElement}"/>.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="pairs"/> is <c>null</c>.</exception>
    public static ILookup<TKey, TElement> FromPairs<TKey, TElement>(IEnumerable<(TKey Key, TElement Element)> pairs, IEqualityComparer<TKey>? comparer = null)
    {
        ArgumentNullException.ThrowIfNull(pairs, nameof(pairs));
        return pairs.ToLookup(p => p.Key, p => p.Element, comparer);
    }

    ///<summary>
    ///Merges two lookups into a single <see cref="ILookup{TKey,TElement}"/>. Groups with matching keys are
    ///concatenated.
    ///</summary>
    ///<typeparam name="TKey">The key type.</typeparam>
    ///<typeparam name="TElement">The element type.</typeparam>
    ///<param name="first">The first lookup.</param>
    ///<param name="second">The second lookup.</param>
    ///<param name="comparer">An optional equality comparer for keys.</param>
    ///<returns>A merged lookup.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="first"/> or <paramref name="second"/> is <c>null</c>.</exception>
    public static ILookup<TKey, TElement> Merge<TKey, TElement>(ILookup<TKey, TElement> first, ILookup<TKey, TElement> second, IEqualityComparer<TKey>? comparer = null)
    {
        ArgumentNullException.ThrowIfNull(first, nameof(first));
        ArgumentNullException.ThrowIfNull(second, nameof(second));
        return first.Concat(second).SelectMany(g => g.Select(e => (g.Key, Element: e))).ToLookup(p => p.Key, p => p.Element, comparer);
    }
    #endregion

    private sealed class EmptyLookup<TKey, TElement> : ILookup<TKey, TElement>
    {
        #region Indexers
        public IEnumerable<TElement> this[TKey key] => [];
        #endregion

        #region Explicit interface implementations
        IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
        #endregion

        #region Public methods
        public bool Contains(TKey key) { return false; }
        public IEnumerator<IGrouping<TKey, TElement>> GetEnumerator() { return Enumerable.Empty<IGrouping<TKey, TElement>>().GetEnumerator(); }
        #endregion

        #region Public properties
        public int Count => 0;
        #endregion
    }
}
