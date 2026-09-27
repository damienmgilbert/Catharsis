using System.Collections;

namespace Catharsis.Collections;

///<summary>
///Provides a read-only view over an existing <see cref="IList{T}"/>. Implements both the generic
///<see cref="IReadOnlyList{T}"/> and the non-generic <see cref="ICollection"/> interfaces, bridging
///the generic and non-generic <see cref="System.Collections"/> worlds.
///</summary>
///<typeparam name="T">The type of elements in the underlying list.</typeparam>
public sealed class ReadOnlyListAdapter<T> : IReadOnlyList<T>, ICollection
{
    #region Fields
    readonly IList<T> _source;
    #endregion

    #region Constructors
    ///<summary>
    ///Initializes a new <see cref="ReadOnlyListAdapter{T}"/> that wraps the specified list.
    ///</summary>
    ///<param name="source">The list to wrap as read-only.</param>
    ///<exception cref="ArgumentNullException">Thrown when <paramref name="source"/> is <c>null</c>.</exception>
    public ReadOnlyListAdapter(IList<T> source)
    {
        if(source is null)
        {
            throw new ArgumentNullException(nameof(source), "Source list must not be null.");
        }

        _source = source;
    }
    #endregion

    #region Indexers
    ///<summary>
    ///Gets the element at the specified index.
    ///</summary>
    ///<param name="index">The zero-based index.</param>
    ///<returns>The element at <paramref name="index"/>.</returns>
    public T this[int index] => _source[index];
    #endregion

    #region Explicit interface implementations
    ///<inheritdoc/>
    void ICollection.CopyTo(Array array, int index)
    {
        if(array is null)
        {
            throw new ArgumentNullException(nameof(array), "Destination array must not be null.");
        }

        if(index < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(index), "Index must not be negative.");
        }

        if(array.Length - index < _source.Count)
        {
            throw new ArgumentException("The destination array does not have enough space.", nameof(array));
        }

        for(int i = 0; i < _source.Count; i++)
        {
            array.SetValue(_source[i], index + i);
        }
    }

    ///<inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
    #endregion

    #region Public methods
    ///<summary>
    ///Determines whether the adapter contains the specified element.
    ///</summary>
    ///<param name="item">The element to locate.</param>
    ///<returns><c>true</c> if <paramref name="item"/> is found; otherwise <c>false</c>.</returns>
    public bool Contains(T item) { return _source.Contains(item); }

    ///<inheritdoc/>
    public IEnumerator<T> GetEnumerator() { return _source.GetEnumerator(); }
    #endregion

    #region Public properties
    ///<inheritdoc/>
    public int Count => _source.Count;

    ///<inheritdoc/>
    bool ICollection.IsSynchronized => false;

    ///<inheritdoc/>
    object ICollection.SyncRoot => this;
    #endregion
}
