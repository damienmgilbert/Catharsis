using System.Linq.Expressions;

namespace Catharsis.Linq;

///<summary>
///Provides extension methods and a fluent builder for sorting <see cref="IQueryable{T}"/> sources using expression
///trees. Supports dynamic multi-key sorting, conditional ordering, and direction-agnostic sort descriptors.
///</summary>
public static class QueryableSorter
{

    ///<summary>
    ///Applies <see cref="Queryable.OrderBy{TSource,TKey}(IQueryable{TSource},Expression{Func{TSource,TKey}})"/>
    ///only when <paramref name="condition"/> is <c>true</c>.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<typeparam name="TKey">The sort key type.</typeparam>
    ///<param name="source">The queryable source.</param>
    ///<param name="condition">Whether to apply the ordering.</param>
    ///<param name="keySelector">The key selector expression.</param>
    ///<returns>The ordered query, or the original query if <paramref name="condition"/> is <c>false</c>.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="keySelector"/> is <c>null</c>.</exception>
    public static IQueryable<T> OrderByIf<T, TKey>(
        this IQueryable<T> source,
        bool condition,
        Expression<Func<T, TKey>> keySelector)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(keySelector, nameof(keySelector));
        return condition ? source.OrderBy(keySelector) : source;
    }

    ///<summary>
    ///Applies <see cref="Queryable.OrderByDescending{TSource,TKey}(IQueryable{TSource},Expression{Func{TSource,TKey}})"/>
    ///only when <paramref name="condition"/> is <c>true</c>.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<typeparam name="TKey">The sort key type.</typeparam>
    ///<param name="source">The queryable source.</param>
    ///<param name="condition">Whether to apply the ordering.</param>
    ///<param name="keySelector">The key selector expression.</param>
    ///<returns>The ordered query, or the original query if <paramref name="condition"/> is <c>false</c>.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="keySelector"/> is <c>null</c>.</exception>
    public static IQueryable<T> OrderByDescendingIf<T, TKey>(
        this IQueryable<T> source,
        bool condition,
        Expression<Func<T, TKey>> keySelector)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(keySelector, nameof(keySelector));
        return condition ? source.OrderByDescending(keySelector) : source;
    }



    ///<summary>
    ///Orders the query by the specified key using the given <see cref="SortDirection"/>.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<typeparam name="TKey">The sort key type.</typeparam>
    ///<param name="source">The queryable source.</param>
    ///<param name="keySelector">The key selector expression.</param>
    ///<param name="direction">The sort direction.</param>
    ///<returns>The ordered query.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="keySelector"/> is <c>null</c>.</exception>
    public static IOrderedQueryable<T> OrderByDirection<T, TKey>(
        this IQueryable<T> source,
        Expression<Func<T, TKey>> keySelector,
        SortDirection direction)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(keySelector, nameof(keySelector));
        return direction == SortDirection.Descending
            ? source.OrderByDescending(keySelector)
            : source.OrderBy(keySelector);
    }

    ///<summary>
    ///Adds a secondary sort to an already-ordered query using the given <see cref="SortDirection"/>.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<typeparam name="TKey">The sort key type.</typeparam>
    ///<param name="source">The ordered queryable source.</param>
    ///<param name="keySelector">The key selector expression.</param>
    ///<param name="direction">The sort direction.</param>
    ///<returns>The ordered query with the secondary sort applied.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="keySelector"/> is <c>null</c>.</exception>
    public static IOrderedQueryable<T> ThenByDirection<T, TKey>(
        this IOrderedQueryable<T> source,
        Expression<Func<T, TKey>> keySelector,
        SortDirection direction)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(keySelector, nameof(keySelector));
        return direction == SortDirection.Descending
            ? source.ThenByDescending(keySelector)
            : source.ThenBy(keySelector);
    }



    ///<summary>
    ///Applies a sequence of <see cref="SortDescriptor{T}"/> to the query. The first descriptor uses
    ///<c>OrderBy</c>/<c>OrderByDescending</c>; subsequent descriptors use <c>ThenBy</c>/<c>ThenByDescending</c>.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="source">The queryable source.</param>
    ///<param name="descriptors">The sort descriptors to apply in order.</param>
    ///<returns>The ordered query, or the original query if no descriptors are supplied.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="descriptors"/> is <c>null</c>.</exception>
    public static IQueryable<T> ApplySort<T>(
        this IQueryable<T> source,
        IEnumerable<SortDescriptor<T>> descriptors)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(descriptors, nameof(descriptors));

        IOrderedQueryable<T>? ordered = null;

        foreach (SortDescriptor<T> descriptor in descriptors)
        {
            ordered = ordered is null
                ? ApplyOrderBy(source, descriptor)
                : ApplyThenBy(ordered, descriptor);
        }

        return ordered ?? source;
    }

    ///<summary>
    ///Applies a sequence of <see cref="SortDescriptor{T}"/> built from a fluent <see cref="QueryableSortBuilder{T}"/>.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="source">The queryable source.</param>
    ///<param name="builder">The sort builder containing descriptors.</param>
    ///<returns>The ordered query.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="builder"/> is <c>null</c>.</exception>
    public static IQueryable<T> ApplySort<T>(
        this IQueryable<T> source,
        QueryableSortBuilder<T> builder)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(builder, nameof(builder));
        return source.ApplySort(builder.Build());
    }



    ///<summary>
    ///Orders the query by a property identified by name at runtime using expression trees. The property must
    ///be a direct member of <typeparamref name="T"/>.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="source">The queryable source.</param>
    ///<param name="propertyName">The name of the property to order by.</param>
    ///<param name="direction">The sort direction.</param>
    ///<returns>The ordered query.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="propertyName"/> is <c>null</c>.</exception>
    ///<exception cref="ArgumentException"><paramref name="propertyName"/> does not correspond to a property on <typeparamref name="T"/>.</exception>
    public static IOrderedQueryable<T> OrderByProperty<T>(
        this IQueryable<T> source,
        string propertyName,
        SortDirection direction = SortDirection.Ascending)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(propertyName, nameof(propertyName));

        (LambdaExpression keySelector, Type keyType) = BuildPropertySelector<T>(propertyName);

        string methodName = direction == SortDirection.Descending
            ? nameof(Queryable.OrderByDescending)
            : nameof(Queryable.OrderBy);

        return ApplyOrderMethod(source, methodName, keySelector, keyType);
    }

    ///<summary>
    ///Adds a secondary sort by a property identified by name at runtime.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="source">The ordered queryable source.</param>
    ///<param name="propertyName">The name of the property to sort by.</param>
    ///<param name="direction">The sort direction.</param>
    ///<returns>The ordered query with the secondary sort.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="propertyName"/> is <c>null</c>.</exception>
    ///<exception cref="ArgumentException"><paramref name="propertyName"/> does not correspond to a property on <typeparamref name="T"/>.</exception>
    public static IOrderedQueryable<T> ThenByProperty<T>(
        this IOrderedQueryable<T> source,
        string propertyName,
        SortDirection direction = SortDirection.Ascending)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(propertyName, nameof(propertyName));

        (LambdaExpression keySelector, Type keyType) = BuildPropertySelector<T>(propertyName);

        string methodName = direction == SortDirection.Descending
            ? nameof(Queryable.ThenByDescending)
            : nameof(Queryable.ThenBy);

        return ApplyOrderMethod(source, methodName, keySelector, keyType);
    }



    private static IOrderedQueryable<T> ApplyOrderBy<T>(IQueryable<T> source, SortDescriptor<T> descriptor)
    {
        return descriptor.Direction == SortDirection.Descending
                    ? ApplyOrderMethod(source, nameof(Queryable.OrderByDescending), descriptor.KeySelector, descriptor.KeyType)
                    : ApplyOrderMethod(source, nameof(Queryable.OrderBy), descriptor.KeySelector, descriptor.KeyType);
    }

    private static IOrderedQueryable<T> ApplyThenBy<T>(IOrderedQueryable<T> source, SortDescriptor<T> descriptor)
    {
        return descriptor.Direction == SortDirection.Descending
                    ? ApplyOrderMethod(source, nameof(Queryable.ThenByDescending), descriptor.KeySelector, descriptor.KeyType)
                    : ApplyOrderMethod(source, nameof(Queryable.ThenBy), descriptor.KeySelector, descriptor.KeyType);
    }

    private static IOrderedQueryable<T> ApplyOrderMethod<T>(
        IQueryable<T> source,
        string methodName,
        LambdaExpression keySelector,
        Type keyType)
    {
        MethodCallExpression call = Expression.Call(
            typeof(Queryable),
            methodName,
            [typeof(T), keyType],
            source.Expression,
            Expression.Quote(keySelector));

        return (IOrderedQueryable<T>)source.Provider.CreateQuery<T>(call);
    }

    private static (LambdaExpression KeySelector, Type KeyType) BuildPropertySelector<T>(string propertyName)
    {
        Type type = typeof(T);
        System.Reflection.PropertyInfo property = type.GetProperty(propertyName)
            ?? throw new ArgumentException($"Property '{propertyName}' not found on type '{type.FullName}'.", nameof(propertyName));

        ParameterExpression parameter = Expression.Parameter(type, "x");
        MemberExpression access = Expression.Property(parameter, property);
        LambdaExpression keySelector = Expression.Lambda(access, parameter);

        return (keySelector, property.PropertyType);
    }

}

///<summary>Specifies the direction of a sort operation.</summary>
public enum SortDirection
{
    ///<summary>Sort in ascending order.</summary>
    Ascending,

    ///<summary>Sort in descending order.</summary>
    Descending
}

///<summary>
///Describes a single sort key and direction for use with <see cref="QueryableSorter.ApplySort{T}(IQueryable{T},IEnumerable{SortDescriptor{T}})"/>.
///</summary>
///<typeparam name="T">The element type being sorted.</typeparam>
public sealed class SortDescriptor<T>
{
    ///<summary>The key selector expression.</summary>
    public required LambdaExpression KeySelector { get; init; }

    ///<summary>The sort direction.</summary>
    public required SortDirection Direction { get; init; }

    ///<summary>The runtime <see cref="Type"/> of the key, derived from <see cref="KeySelector"/>.</summary>
    public required Type KeyType { get; init; }

    ///<summary>
    ///Creates a <see cref="SortDescriptor{T}"/> from a strongly-typed key selector.
    ///</summary>
    ///<typeparam name="TKey">The key type.</typeparam>
    ///<param name="keySelector">The key selector expression.</param>
    ///<param name="direction">The sort direction.</param>
    ///<returns>A new sort descriptor.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="keySelector"/> is <c>null</c>.</exception>
    public static SortDescriptor<T> Create<TKey>(
        Expression<Func<T, TKey>> keySelector,
        SortDirection direction = SortDirection.Ascending)
    {
        ArgumentNullException.ThrowIfNull(keySelector, nameof(keySelector));
        return new SortDescriptor<T>
        {
            KeySelector = keySelector,
            Direction = direction,
            KeyType = typeof(TKey)
        };
    }

    ///<summary>
    ///Creates a <see cref="SortDescriptor{T}"/> from a property name resolved at runtime.
    ///</summary>
    ///<param name="propertyName">The name of the property to sort by.</param>
    ///<param name="direction">The sort direction.</param>
    ///<returns>A new sort descriptor.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="propertyName"/> is <c>null</c>.</exception>
    ///<exception cref="ArgumentException"><paramref name="propertyName"/> does not correspond to a property on <typeparamref name="T"/>.</exception>
    public static SortDescriptor<T> Create(
        string propertyName,
        SortDirection direction = SortDirection.Ascending)
    {
        ArgumentNullException.ThrowIfNull(propertyName, nameof(propertyName));

        Type type = typeof(T);
        System.Reflection.PropertyInfo property = type.GetProperty(propertyName)
            ?? throw new ArgumentException($"Property '{propertyName}' not found on type '{type.FullName}'.", nameof(propertyName));

        ParameterExpression parameter = Expression.Parameter(type, "x");
        MemberExpression access = Expression.Property(parameter, property);
        LambdaExpression keySelector = Expression.Lambda(access, parameter);

        return new SortDescriptor<T>
        {
            KeySelector = keySelector,
            Direction = direction,
            KeyType = property.PropertyType
        };
    }
}
