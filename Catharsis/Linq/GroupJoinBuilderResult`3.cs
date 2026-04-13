namespace Catharsis.Linq;

///<summary>
///Builder stage awaiting the result selector and optional configuration.
///</summary>
///<typeparam name="TOuter">The outer element type.</typeparam>
///<typeparam name="TInner">The inner element type.</typeparam>
///<typeparam name="TKey">The key type.</typeparam>
public sealed class GroupJoinBuilderResult<TOuter, TInner, TKey>(IEnumerable<TOuter> outer, IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector)
{
    #region Fields
    private IEqualityComparer<TKey>? _comparer;
    #endregion

    #region Public methods
    ///<summary>
    ///Executes the group join and returns the results as <see cref="IGrouping{TKey,TElement}"/> instances keyed by the
    ///outer element.
    ///</summary>
    ///<returns>A sequence of groupings where each key is an outer element and elements are the matched inners.</returns>
    public IEnumerable<IGrouping<TOuter, TInner>> AsGroupings() { return outer.GroupJoin(inner, outerKeySelector, innerKeySelector, (o, inners) => SequenceFactory.Grouping(o, inners), _comparer); }
    ///<summary>
    ///Executes the group join and returns a sequence of tuples pairing each outer element with its matching inner
    ///elements.
    ///</summary>
    ///<returns>A sequence of <c>(Outer, InnerGroup)</c> tuples.</returns>
    public IEnumerable<(TOuter Outer, IEnumerable<TInner> InnerGroup)> AsTuples() { return outer.GroupJoin(inner, outerKeySelector, innerKeySelector, (o, inners) => (o, inners), _comparer); }

    ///<summary>
    ///Performs a left outer join: for each outer element, if no inner matches exist the ///<paramref
    ///name="defaultInner"/> value is used instead.
    ///</summary>
    ///<typeparam name="TResult">The result type.</typeparam>
    ///<param name="resultSelector">A function that receives each outer element and a single inner element.</param>
    ///<param name="defaultInner">The fallback inner value when no match exists.</param>
    ///<returns>A flat sequence of results where every outer element appears at least once.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="resultSelector"/> is <c>null</c>.</exception>
    public IEnumerable<TResult> LeftJoin<TResult>(Func<TOuter, TInner, TResult> resultSelector, TInner? defaultInner = default)
    {
        ArgumentNullException.ThrowIfNull(resultSelector, nameof(resultSelector));
        return outer.GroupJoin(inner, outerKeySelector, innerKeySelector, (o, inners) => (o, inners), _comparer).SelectMany(pair => pair.inners.DefaultIfEmpty(defaultInner!), (pair, innerItem) => resultSelector(pair.o, innerItem));
    }

    ///<summary>
    ///Performs a left outer join and returns <c>(Outer, Inner)</c> tuples, using <paramref name="defaultInner"/> when
    ///no inner match exists.
    ///</summary>
    ///<param name="defaultInner">The fallback inner value when no match exists.</param>
    ///<returns>A flat sequence of tuples.</returns>
    public IEnumerable<(TOuter Outer, TInner? Inner)> LeftJoinTuples(TInner? defaultInner = default)
    { return outer.GroupJoin(inner, outerKeySelector, innerKeySelector, (o, inners) => (o, inners), _comparer).SelectMany(pair => pair.inners.DefaultIfEmpty(defaultInner!), (pair, innerItem) => (pair.o, (TInner?)innerItem)); }

    ///<summary>
    ///Projects each outer element and its matched inner group into a result and executes the group join.
    ///</summary>
    ///<typeparam name="TResult">The result type.</typeparam>
    ///<param name="resultSelector">
    ///A function that receives the outer element and its matching inner elements and produces a result.
    ///</param>
    ///<returns>A sequence of projected results from the group join.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="resultSelector"/> is <c>null</c>.</exception>
    public IEnumerable<TResult> Select<TResult>(Func<TOuter, IEnumerable<TInner>, TResult> resultSelector)
    {
        ArgumentNullException.ThrowIfNull(resultSelector, nameof(resultSelector));
        return outer.GroupJoin(inner, outerKeySelector, innerKeySelector, resultSelector, _comparer);
    }

    ///<summary>
    ///Performs the group join and additionally chains another inner sequence. The result is a builder that can be used
    ///to add a second join.
    ///</summary>
    ///<typeparam name="TInner2">The second inner element type.</typeparam>
    ///<param name="inner2">The second inner sequence.</param>
    ///<returns>A builder for the chained group join.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="inner2"/> is <c>null</c>.</exception>
    public GroupJoinBuilderKeys<(TOuter Outer, IEnumerable<TInner> InnerGroup), TInner2> ThenWith<TInner2>(IEnumerable<TInner2> inner2)
    {
        ArgumentNullException.ThrowIfNull(inner2, nameof(inner2));

        IEnumerable<(TOuter Outer, IEnumerable<TInner> InnerGroup)> firstJoin = AsTuples();
        return new GroupJoinBuilderKeys<(TOuter Outer, IEnumerable<TInner> InnerGroup), TInner2>(firstJoin, inner2);
    }

    ///<summary>
    ///Specifies a custom equality comparer for the join keys.
    ///</summary>
    ///<param name="comparer">The equality comparer to use.</param>
    ///<returns>The current builder for fluent chaining.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="comparer"/> is <c>null</c>.</exception>
    public GroupJoinBuilderResult<TOuter, TInner, TKey> WithComparer(IEqualityComparer<TKey> comparer)
    {
        ArgumentNullException.ThrowIfNull(comparer, nameof(comparer));
        _comparer = comparer;
        return this;
    }
    #endregion
}
