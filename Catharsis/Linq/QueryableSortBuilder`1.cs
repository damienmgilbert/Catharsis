using System.Linq.Expressions;

namespace Catharsis.Linq;

///<summary>
///A mutable builder that accumulates <see cref="SortDescriptor{T}"/> instances for multi-key sorting of ///<see
///cref="IQueryable{T}"/> sources.
///</summary>
///<typeparam name="T">The element type being sorted.</typeparam>
public sealed class QueryableSortBuilder<T>
{
    #region Fields
    private readonly List<SortDescriptor<T>> _descriptors = [];
    #endregion

    #region Public methods
    ///<summary>
    ///Builds the accumulated sort descriptors into a read-only list.
    ///</summary>
    ///<returns>The list of sort descriptors.</returns>
    public IReadOnlyList<SortDescriptor<T>> Build() => _descriptors.AsReadOnly();

    ///<summary>
    ///Removes all accumulated sort descriptors.
    ///</summary>
    ///<returns>The current builder for fluent chaining.</returns>
    public QueryableSortBuilder<T> Clear()
    {
        _descriptors.Clear();
        return this;
    }

    ///<summary>
    ///Adds a sort descriptor for the specified key in ascending order.
    ///</summary>
    ///<typeparam name="TKey">The key type.</typeparam>
    ///<param name="keySelector">The key selector expression.</param>
    ///<returns>The current builder for fluent chaining.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="keySelector"/> is <c>null</c>.</exception>
    public QueryableSortBuilder<T> OrderBy<TKey>(Expression<Func<T, TKey>> keySelector)
    {
        ArgumentNullException.ThrowIfNull(keySelector, nameof(keySelector));
        _descriptors.Add(SortDescriptor<T>.Create(keySelector, SortDirection.Ascending));
        return this;
    }

    ///<summary>
    ///Adds a sort descriptor for the specified key in descending order.
    ///</summary>
    ///<typeparam name="TKey">The key type.</typeparam>
    ///<param name="keySelector">The key selector expression.</param>
    ///<returns>The current builder for fluent chaining.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="keySelector"/> is <c>null</c>.</exception>
    public QueryableSortBuilder<T> OrderByDescending<TKey>(Expression<Func<T, TKey>> keySelector)
    {
        ArgumentNullException.ThrowIfNull(keySelector, nameof(keySelector));
        _descriptors.Add(SortDescriptor<T>.Create(keySelector, SortDirection.Descending));
        return this;
    }

    ///<summary>
    ///Adds a sort descriptor only when <paramref name="condition"/> is <c>true</c>.
    ///</summary>
    ///<typeparam name="TKey">The key type.</typeparam>
    ///<param name="condition">Whether to add the descriptor.</param>
    ///<param name="keySelector">The key selector expression.</param>
    ///<param name="direction">The sort direction.</param>
    ///<returns>The current builder for fluent chaining.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="keySelector"/> is <c>null</c>.</exception>
    public QueryableSortBuilder<T> OrderByIf<TKey>(bool condition, Expression<Func<T, TKey>> keySelector, SortDirection direction = SortDirection.Ascending)
    {
        ArgumentNullException.ThrowIfNull(keySelector, nameof(keySelector));

        if(condition)
        {
            _descriptors.Add(SortDescriptor<T>.Create(keySelector, direction));
        }

        return this;
    }

    ///<summary>
    ///Adds a sort descriptor from a property name in the specified direction.
    ///</summary>
    ///<param name="propertyName">The name of the property to sort by.</param>
    ///<param name="direction">The sort direction.</param>
    ///<returns>The current builder for fluent chaining.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="propertyName"/> is <c>null</c>.</exception>
    ///<exception cref="ArgumentException"><paramref name="propertyName"/> does not correspond to a property on <typeparamref name="T"/>.</exception>
    public QueryableSortBuilder<T> OrderByProperty(string propertyName, SortDirection direction = SortDirection.Ascending)
    {
        ArgumentNullException.ThrowIfNull(propertyName, nameof(propertyName));
        _descriptors.Add(SortDescriptor<T>.Create(propertyName, direction));
        return this;
    }
    #endregion

    #region Public properties
    ///<summary>
    ///Returns the number of sort descriptors currently in the builder.
    ///</summary>
    public int Count => _descriptors.Count;
    #endregion
}
