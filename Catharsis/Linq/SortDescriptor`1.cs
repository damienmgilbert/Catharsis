using System.Linq.Expressions;

namespace Catharsis.Linq;

///<summary>
///Describes a single sort key and direction for use with <see
///cref="QueryableSorter.ApplySort{T}(IQueryable{T},IEnumerable{SortDescriptor{T}})"/>.
///</summary>
///<typeparam name="T">The element type being sorted.</typeparam>
public sealed class SortDescriptor<T>
{
    #region Public methods
    ///<summary>
    ///Creates a <see cref="SortDescriptor{T}"/> from a strongly-typed key selector.
    ///</summary>
    ///<typeparam name="TKey">The key type.</typeparam>
    ///<param name="keySelector">The key selector expression.</param>
    ///<param name="direction">The sort direction.</param>
    ///<returns>A new sort descriptor.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="keySelector"/> is <c>null</c>.</exception>
    public static SortDescriptor<T> Create<TKey>(Expression<Func<T, TKey>> keySelector, SortDirection direction = SortDirection.Ascending)
    {
        ArgumentNullException.ThrowIfNull(keySelector, nameof(keySelector));
        return new SortDescriptor<T> { KeySelector = keySelector, Direction = direction, KeyType = typeof(TKey) };
    }

    ///<summary>
    ///Creates a <see cref="SortDescriptor{T}"/> from a property name resolved at runtime.
    ///</summary>
    ///<param name="propertyName">The name of the property to sort by.</param>
    ///<param name="direction">The sort direction.</param>
    ///<returns>A new sort descriptor.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="propertyName"/> is <c>null</c>.</exception>
    ///<exception cref="ArgumentException"><paramref name="propertyName"/> does not correspond to a property on <typeparamref name="T"/>.</exception>
    public static SortDescriptor<T> Create(string propertyName, SortDirection direction = SortDirection.Ascending)
    {
        ArgumentNullException.ThrowIfNull(propertyName, nameof(propertyName));

        Type type = typeof(T);
        System.Reflection.PropertyInfo property = type.GetProperty(propertyName) ?? throw new ArgumentException($"Property '{propertyName}' not found on type '{type.FullName}'.", nameof(propertyName));

        ParameterExpression parameter = Expression.Parameter(type, "x");
        MemberExpression access = Expression.Property(parameter, property);
        LambdaExpression keySelector = Expression.Lambda(access, parameter);

        return new SortDescriptor<T> { KeySelector = keySelector, Direction = direction, KeyType = property.PropertyType };
    }
    #endregion

    #region Public properties
    ///<summary>
    ///The sort direction.
    ///</summary>
    public required SortDirection Direction { get; init; }

    ///<summary>
    ///The key selector expression.
    ///</summary>
    public required LambdaExpression KeySelector { get; init; }

    ///<summary>
    ///The runtime <see cref="Type"/> of the key, derived from <see cref="KeySelector"/>.
    ///</summary>
    public required Type KeyType { get; init; }
    #endregion
}
