using System.Linq.Expressions;
using Catharsis.Linq.Expressions;

namespace Catharsis.Linq;

///<summary>
///Provides extension methods for building dynamic <see cref="IQueryable{T}"/> projections using expression trees.
///Includes single-property selection, multi-property dictionary projections, and composed selector pipelines that
///remain translatable by query providers such as Entity Framework.
///</summary>
public static class QueryableProjector
{
    #region Public methods

    ///<summary>
    ///Projects each element to a single property or computed value using the supplied expression. This is a convenience
    ///alias for <see cref="Queryable.Select{TSource,TResult}(IQueryable{TSource},Expression{Func{TSource,TResult}})"/>.
    ///
    ///</summary>
    ///<typeparam name="T">The source element type.</typeparam>
    ///<typeparam name="TResult">The projected type.</typeparam>
    ///<param name="source">The queryable source.</param>
    ///<param name="selector">The projection expression.</param>
    ///<returns>A query of projected values.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="selector"/> is <c>null</c>.</exception>
    public static IQueryable<TResult> Project<T, TResult>(this IQueryable<T> source, Expression<Func<T, TResult>> selector)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(selector, nameof(selector));
        return source.Select(selector);
    }

    ///<summary>
    ///Projects elements and then applies <see cref="Queryable.Distinct{TSource}(IQueryable{TSource})"/> to the result.
    ///</summary>
    ///<typeparam name="T">The source element type.</typeparam>
    ///<typeparam name="TResult">The projected type.</typeparam>
    ///<param name="source">The queryable source.</param>
    ///<param name="selector">The projection expression.</param>
    ///<returns>A query of distinct projected values.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="selector"/> is <c>null</c>.</exception>
    public static IQueryable<TResult> ProjectDistinct<T, TResult>(this IQueryable<T> source, Expression<Func<T, TResult>> selector)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(selector, nameof(selector));
        return source.Select(selector).Distinct();
    }

    ///<summary>
    ///Projects elements using <paramref name="selector"/> only when <paramref name="condition"/> is <c>true</c>;
    ///otherwise returns the source cast to the result type.
    ///</summary>
    ///<typeparam name="T">The source element type.</typeparam>
    ///<typeparam name="TResult">The projected type.</typeparam>
    ///<param name="source">The queryable source.</param>
    ///<param name="condition">Whether to apply the projection.</param>
    ///<param name="selector">The projection expression.</param>
    ///<returns>The projected query, or the original query cast when the condition is <c>false</c>.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="selector"/> is <c>null</c>.</exception>
    ///<exception cref="InvalidCastException">
    public static IQueryable<TResult> ProjectIf<T, TResult>(this IQueryable<T> source, bool condition, Expression<Func<T, TResult>> selector)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(selector, nameof(selector));
        return condition ? source.Select(selector) : source.Cast<TResult>();
    }

    ///<summary>
    ///Projects each element to a collection and flattens the results using ///<see
    ///cref="Queryable.SelectMany{TSource,TResult}(IQueryable{TSource},Expression{Func{TSource,IEnumerable{TResult}}})"/>.
    ///
    ///</summary>
    ///<typeparam name="T">The source element type.</typeparam>
    ///<typeparam name="TResult">The result element type.</typeparam>
    ///<param name="source">The queryable source.</param>
    ///<param name="collectionSelector">An expression that projects each element to a collection.</param>
    ///<returns>A flattened query of results.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="collectionSelector"/> is <c>null</c>.</exception>
    public static IQueryable<TResult> ProjectMany<T, TResult>(this IQueryable<T> source, Expression<Func<T, IEnumerable<TResult>>> collectionSelector)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(collectionSelector, nameof(collectionSelector));
        return source.SelectMany(collectionSelector);
    }

    ///<summary>
    ///Chains a second projection onto an existing one via expression composition. The resulting expression
    public static IQueryable<TResult> ProjectThrough<T, TMiddle, TResult>(this IQueryable<T> source, Expression<Func<T, TMiddle>> first, Expression<Func<TMiddle, TResult>> second)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(first, nameof(first));
        ArgumentNullException.ThrowIfNull(second, nameof(second));

        Expression<Func<T, TResult>> composed = ExpressionComposer.Compose(first, second);
        return source.Select(composed);
    }

    ///<summary>
    ///Projects each element into a <see cref="Dictionary{TKey,TValue}"/> of <c>string</c>/<c>object?</c> pairs using
    ///the supplied property selectors. Useful for dynamic column selection without anonymous types.
    ///</summary>
    ///<typeparam name="T">The source element type.</typeparam>
    ///<param name="source">The queryable source.</param>
    ///<param name="projections">A dictionary mapping column names to selector expressions.</param>
    ///<returns>An enumerable of dictionaries, each representing one projected row.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="projections"/> is <c>null</c>.</exception>
    public static IEnumerable<Dictionary<string, object?>> ProjectToDictionaries<T>(this IQueryable<T> source, IReadOnlyDictionary<string, Expression<Func<T, object?>>> projections)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(projections, nameof(projections));

        List<KeyValuePair<string, Func<T, object?>>> compiled = [ .. projections.Select(static kvp => KeyValuePair.Create(kvp.Key, kvp.Value.Compile())) ];

        foreach(T item in source)
        {
            Dictionary<string, object?> row = [ with(compiled.Count) ];

            foreach(KeyValuePair<string, Func<T, object?>> kvp in compiled)
            {
                row[kvp.Key] = kvp.Value(item);
            }

            yield return row;
        }
    }

    ///<summary>
    ///Projects each element along with its zero-based index.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="source">The queryable source.</param>
    ///<returns>A query of <c>(Index, Element)</c> tuples.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
    public static IQueryable<(int Index, T Element)> ProjectWithIndex<T>(this IQueryable<T> source)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        return source.Select(static(item, index) => new ValueTuple<int, T>(index, item));
    }
    #endregion
}
