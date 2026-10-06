using Catharsis.Linq.Expressions;
using System.Linq.Expressions;

namespace Catharsis.Linq;

///<summary>
///Provides extension methods for composing <see cref="IQueryable{T}"/> pipelines using expression trees. Supports
///conditional application, provider-safe predicate combination, and expression-based pipeline transformations.
///</summary>
public static class QueryableComposer
{
    #region Public methods

    ///<summary>
    ///Adds an additional <c>Where</c> clause that is combined with any existing filter using logical AND at the
    ///expression tree level via <see cref="ExpressionComposer.AndAlso{T}"/>.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="source">The queryable source.</param>
    ///<param name="predicate">The additional predicate to AND with existing filters.</param>
    ///<returns>The query with the combined filter.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="predicate"/> is <c>null</c>.</exception>
    public static IQueryable<T> AndWhere<T>(this IQueryable<T> source, Expression<Func<T, bool>> predicate)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(predicate, nameof(predicate));
        return source.Where(predicate);
    }

    ///<summary>
    ///Returns the underlying <see cref="Expression"/> tree for the queryable source.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="source">The queryable source.</param>
    ///<returns>The expression tree representing the query.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
    public static Expression GetExpression<T>(this IQueryable<T> source)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        return source.Expression;
    }

    ///<summary>
    ///Returns the <see cref="IQueryProvider"/> associated with the queryable source.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="source">The queryable source.</param>
    ///<returns>The query provider.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
    public static IQueryProvider GetProvider<T>(this IQueryable<T> source)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        return source.Provider;
    }

    ///<summary>
    ///Applies skip/take pagination to the query.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="source">The queryable source.</param>
    ///<param name="pageIndex">The zero-based page index.</param>
    ///<param name="pageSize">The number of elements per page. Must be at least 1.</param>
    ///<returns>The paginated query.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
    ///<exception cref="ArgumentOutOfRangeException"><paramref name="pageIndex"/> is negative or <paramref name="pageSize"/> is less than 1.</exception>
    public static IQueryable<T> Page<T>(this IQueryable<T> source, int pageIndex, int pageSize)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentOutOfRangeException.ThrowIfNegative(pageIndex, nameof(pageIndex));
        ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1, nameof(pageSize));
        return source.Skip(pageIndex * pageSize).Take(pageSize);
    }

    ///<summary>
    ///Applies an arbitrary <see cref="IQueryable{T}"/>-to-<see cref="IQueryable{T}"/> transformation inline within a
    ///fluent pipeline.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="source">The queryable source.</param>
    ///<param name="transform">A function that transforms the query.</param>
    ///<returns>The transformed query.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="transform"/> is <c>null</c>.</exception>
    public static IQueryable<T> Pipe<T>(this IQueryable<T> source, Func<IQueryable<T>, IQueryable<T>> transform)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(transform, nameof(transform));
        return transform(source);
    }

    ///<summary>
    ///Applies an arbitrary transformation that changes the element type.
    ///</summary>
    ///<typeparam name="T">The source element type.</typeparam>
    ///<typeparam name="TResult">The result element type.</typeparam>
    ///<param name="source">The queryable source.</param>
    ///<param name="transform">A function that transforms the query.</param>
    ///<returns>The transformed query.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="transform"/> is <c>null</c>.</exception>
    public static IQueryable<TResult> Pipe<T, TResult>(this IQueryable<T> source, Func<IQueryable<T>, IQueryable<TResult>> transform)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(transform, nameof(transform));
        return transform(source);
    }

    ///<summary>
    ///Terminates a queryable pipeline by applying a function that produces a scalar result.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<typeparam name="TResult">The result type.</typeparam>
    ///<param name="source">The queryable source.</param>
    ///<param name="terminator">A function that produces a result from the query.</param>
    ///<returns>The result of applying <paramref name="terminator"/>.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="terminator"/> is <c>null</c>.</exception>
    public static TResult PipeResult<T, TResult>(this IQueryable<T> source, Func<IQueryable<T>, TResult> terminator)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(terminator, nameof(terminator));
        return terminator(source);
    }

    ///<summary>
    ///Applies a combined AND predicate built from multiple predicate expressions.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="source">The queryable source.</param>
    ///<param name="predicates">The predicates to combine with AND.</param>
    ///<returns>The query filtered by the combined predicate, or the original query if the collection is empty.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="predicates"/> is <c>null</c>.</exception>
    public static IQueryable<T> WhereAll<T>(this IQueryable<T> source, IEnumerable<Expression<Func<T, bool>>> predicates)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(predicates, nameof(predicates));

        Expression<Func<T, bool>> combined = ExpressionComposer.AndAll(predicates);
        return source.Where(combined);
    }

    ///<summary>
    ///Applies a combined OR predicate built from multiple predicate expressions.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="source">The queryable source.</param>
    ///<param name="predicates">The predicates to combine with OR.</param>
    ///<returns>The query filtered by the combined predicate, or an empty result if the collection is empty.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="predicates"/> is <c>null</c>.</exception>
    public static IQueryable<T> WhereAny<T>(this IQueryable<T> source, IEnumerable<Expression<Func<T, bool>>> predicates)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(predicates, nameof(predicates));

        Expression<Func<T, bool>> combined = ExpressionComposer.OrAny(predicates);
        return source.Where(combined);
    }

    ///<summary>
    ///Applies a <c>Where</c> filter only when <paramref name="condition"/> is <c>true</c>; otherwise returns the query
    ///unchanged. Useful for optional search criteria.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="source">The queryable source.</param>
    ///<param name="condition">Whether to apply the filter.</param>
    ///<param name="predicate">The filter predicate.</param>
    ///<returns>The filtered query, or the original query if <paramref name="condition"/> is <c>false</c>.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="predicate"/> is <c>null</c>.</exception>
    public static IQueryable<T> WhereIf<T>(this IQueryable<T> source, bool condition, Expression<Func<T, bool>> predicate)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(predicate, nameof(predicate));
        return condition ? source.Where(predicate) : source;
    }

    ///<summary>
    ///Applies a <c>Where</c> filter only when <paramref name="value"/> is not <c>null</c>. The predicate factory
    ///receives the non-null value.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<typeparam name="TValue">The filter value type.</typeparam>
    ///<param name="source">The queryable source.</param>
    ///<param name="value">The optional filter value.</param>
    ///<param name="predicateFactory">A function that creates the predicate from the non-null value.</param>
    ///<returns>The filtered query, or the original query if <paramref name="value"/> is <c>null</c>.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="predicateFactory"/> is <c>null</c>.</exception>
    public static IQueryable<T> WhereIfNotNull<T, TValue>(this IQueryable<T> source, TValue? value, Func<TValue, Expression<Func<T, bool>>> predicateFactory) where TValue : class
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(predicateFactory, nameof(predicateFactory));
        return value is not null ? source.Where(predicateFactory(value)) : source;
    }

    ///<summary>
    ///Applies a <c>Where</c> filter only when the nullable value type has a value.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<typeparam name="TValue">The filter value type.</typeparam>
    ///<param name="source">The queryable source.</param>
    ///<param name="value">The optional filter value.</param>
    ///<param name="predicateFactory">A function that creates the predicate from the value.</param>
    ///<returns>The filtered query, or the original query if <paramref name="value"/> has no value.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="predicateFactory"/> is <c>null</c>.</exception>
    public static IQueryable<T> WhereIfNotNull<T, TValue>(this IQueryable<T> source, TValue? value, Func<TValue, Expression<Func<T, bool>>> predicateFactory) where TValue : struct
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(predicateFactory, nameof(predicateFactory));
        return value.HasValue ? source.Where(predicateFactory(value.Value)) : source;
    }

    ///<summary>
    ///Combines two predicate expressions with logical OR and applies the result as a <c>Where</c> clause.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="source">The queryable source.</param>
    ///<param name="first">The first predicate.</param>
    ///<param name="second">The second predicate.</param>
    ///<returns>The query filtered by <c>first(x) || second(x)</c>.</returns>
    ///<exception cref="ArgumentNullException">Any argument is <c>null</c>.</exception>
    public static IQueryable<T> WhereOr<T>(this IQueryable<T> source, Expression<Func<T, bool>> first, Expression<Func<T, bool>> second)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(first, nameof(first));
        ArgumentNullException.ThrowIfNull(second, nameof(second));
        return source.Where(ExpressionComposer.OrElse(first, second));
    }
    #endregion
}
