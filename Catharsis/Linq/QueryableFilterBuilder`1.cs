using Catharsis.Linq.Expressions;
using System.Linq.Expressions;

namespace Catharsis.Linq;

///<summary>
///A mutable builder that accumulates predicate expressions for <see cref="IQueryable{T}"/> filtering. Predicates are
///combined at the expression tree level using <see cref="ExpressionComposer"/>, producing a single ///<see
///cref="Expression{TDelegate}"/> that is provider-translatable (e.g. to SQL).
///</summary>
///<typeparam name="T">The element type being filtered.</typeparam>
public sealed class QueryableFilterBuilder<T>
{
    #region Fields
    private FilterCombineMode _defaultMode = FilterCombineMode.And;
    private readonly List<Expression<Func<T, bool>>> _predicates = [];
    #endregion

    #region Public methods
    ///<summary>
    ///Builds the accumulated predicates and applies them to the given queryable source.
    ///</summary>
    ///<param name="source">The queryable source to filter.</param>
    ///<returns>The filtered query.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
    public IQueryable<T> Apply(IQueryable<T> source)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        return source.Where(Build());
    }

    ///<summary>
    ///Builds the accumulated predicates into a single combined expression.
    ///</summary>
    ///<returns>A combined predicate expression.</returns>
    public Expression<Func<T, bool>> Build()
    {
        return _defaultMode switch
        {
            FilterCombineMode.And => ExpressionComposer.AndAll(_predicates),
            FilterCombineMode.Or => ExpressionComposer.OrAny(_predicates),
            _ => ExpressionComposer.AndAll(_predicates)
        };
    }

    ///<summary>
    ///Removes all accumulated predicates.
    ///</summary>
    ///<returns>The current builder for fluent chaining.</returns>
    public QueryableFilterBuilder<T> Clear()
    {
        _predicates.Clear();
        return this;
    }

    ///<summary>
    ///Adds a predicate expression to the builder.
    ///</summary>
    ///<param name="predicate">The predicate to add.</param>
    ///<returns>The current builder for fluent chaining.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="predicate"/> is <c>null</c>.</exception>
    public QueryableFilterBuilder<T> Where(Expression<Func<T, bool>> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate, nameof(predicate));
        _predicates.Add(predicate);
        return this;
    }

    ///<summary>
    ///Adds a comparison predicate for a property: <c>selector(x) op value</c>.
    ///</summary>
    ///<typeparam name="TProperty">The property type.</typeparam>
    ///<param name="selector">An expression selecting the property.</param>
    ///<param name="comparison">The comparison operator.</param>
    ///<param name="value">The value to compare against.</param>
    ///<returns>The current builder for fluent chaining.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="selector"/> is <c>null</c>.</exception>
    public QueryableFilterBuilder<T> WhereCompare<TProperty>(Expression<Func<T, TProperty>> selector, FilterComparison comparison, TProperty value)
    {
        ArgumentNullException.ThrowIfNull(selector, nameof(selector));

        ConstantExpression valueExpr = Expression.Constant(value, typeof(TProperty));

        BinaryExpression body = comparison switch
        {
            FilterComparison.Equal => Expression.Equal(selector.Body, valueExpr),
            FilterComparison.NotEqual => Expression.NotEqual(selector.Body, valueExpr),
            FilterComparison.GreaterThan => Expression.GreaterThan(selector.Body, valueExpr),
            FilterComparison.GreaterThanOrEqual => Expression.GreaterThanOrEqual(selector.Body, valueExpr),
            FilterComparison.LessThan => Expression.LessThan(selector.Body, valueExpr),
            FilterComparison.LessThanOrEqual => Expression.LessThanOrEqual(selector.Body, valueExpr),
            _ => throw new ArgumentOutOfRangeException(nameof(comparison), comparison, "Unsupported comparison operator.")
        };

        _predicates.Add(Expression.Lambda<Func<T, bool>>(body, selector.Parameters));
        return this;
    }

    ///<summary>
    ///Adds a predicate that tests whether a string property contains <paramref name="substring"/>. Builds a ///<see
    ///cref="string.Contains(string)"/> call expression.
    ///</summary>
    ///<param name="selector">An expression selecting the string property.</param>
    ///<param name="substring">The substring to search for.</param>
    ///<returns>The current builder for fluent chaining.</returns>
    ///<exception cref="ArgumentNullException">Any argument is <c>null</c>.</exception>
    public QueryableFilterBuilder<T> WhereContains(Expression<Func<T, string>> selector, string substring)
    {
        ArgumentNullException.ThrowIfNull(selector, nameof(selector));
        ArgumentNullException.ThrowIfNull(substring, nameof(substring));

        ConstantExpression substringConstant = Expression.Constant(substring, typeof(string));

        MethodCallExpression containsCall = Expression.Call(selector.Body, typeof(string).GetMethod(nameof(string.Contains), [typeof(string)])!, substringConstant);

        _predicates.Add(Expression.Lambda<Func<T, bool>>(containsCall, selector.Parameters));
        return this;
    }

    ///<summary>
    ///Adds a predicate only when <paramref name="condition"/> is <c>true</c>.
    ///</summary>
    ///<param name="condition">Whether to add the predicate.</param>
    ///<param name="predicate">The predicate to add.</param>
    ///<returns>The current builder for fluent chaining.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="predicate"/> is <c>null</c>.</exception>
    public QueryableFilterBuilder<T> WhereIf(bool condition, Expression<Func<T, bool>> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate, nameof(predicate));

        if (condition)
        {
            _predicates.Add(predicate);
        }

        return this;
    }

    ///<summary>
    ///Adds a predicate only when <paramref name="value"/> is not <c>null</c>.
    ///</summary>
    ///<typeparam name="TValue">The filter value type.</typeparam>
    ///<param name="value">The optional filter value.</param>
    ///<param name="predicateFactory">A function that creates the predicate from the non-null value.</param>
    ///<returns>The current builder for fluent chaining.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="predicateFactory"/> is <c>null</c>.</exception>
    public QueryableFilterBuilder<T> WhereIfNotNull<TValue>(TValue? value, Func<TValue, Expression<Func<T, bool>>> predicateFactory) where TValue : class
    {
        ArgumentNullException.ThrowIfNull(predicateFactory, nameof(predicateFactory));

        if (value is not null)
        {
            _predicates.Add(predicateFactory(value));
        }

        return this;
    }

    ///<summary>
    ///Adds a predicate only when the nullable value type has a value.
    ///</summary>
    ///<typeparam name="TValue">The filter value type.</typeparam>
    ///<param name="value">The optional filter value.</param>
    ///<param name="predicateFactory">A function that creates the predicate from the value.</param>
    ///<returns>The current builder for fluent chaining.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="predicateFactory"/> is <c>null</c>.</exception>
    public QueryableFilterBuilder<T> WhereIfNotNull<TValue>(TValue? value, Func<TValue, Expression<Func<T, bool>>> predicateFactory) where TValue : struct
    {
        ArgumentNullException.ThrowIfNull(predicateFactory, nameof(predicateFactory));

        if (value.HasValue)
        {
            _predicates.Add(predicateFactory(value.Value));
        }

        return this;
    }

    ///<summary>
    ///Adds a predicate that filters by a property extracted with <paramref name="selector"/> being contained in
    ///<paramref name="allowedValues"/>. Builds a <c>Contains</c> call expression.
    ///</summary>
    ///<typeparam name="TProperty">The property type.</typeparam>
    ///<param name="selector">An expression selecting the property.</param>
    ///<param name="allowedValues">The set of allowed values.</param>
    ///<returns>The current builder for fluent chaining.</returns>
    ///<exception cref="ArgumentNullException">Any argument is <c>null</c>.</exception>
    public QueryableFilterBuilder<T> WhereIn<TProperty>(Expression<Func<T, TProperty>> selector, IEnumerable<TProperty> allowedValues)
    {
        ArgumentNullException.ThrowIfNull(selector, nameof(selector));
        ArgumentNullException.ThrowIfNull(allowedValues, nameof(allowedValues));

        List<TProperty> values = allowedValues as List<TProperty> ?? allowedValues.ToList();
        ConstantExpression valuesConstant = Expression.Constant(values, typeof(List<TProperty>));

        MethodCallExpression containsCall = Expression.Call(typeof(Enumerable), nameof(Enumerable.Contains), [typeof(TProperty)], valuesConstant, selector.Body);

        _predicates.Add(Expression.Lambda<Func<T, bool>>(containsCall, selector.Parameters));
        return this;
    }

    ///<summary>
    ///Adds a negated predicate: <c>!(predicate(x))</c>.
    ///</summary>
    ///<param name="predicate">The predicate to negate and add.</param>
    ///<returns>The current builder for fluent chaining.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="predicate"/> is <c>null</c>.</exception>
    public QueryableFilterBuilder<T> WhereNot(Expression<Func<T, bool>> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate, nameof(predicate));
        _predicates.Add(ExpressionComposer.Not(predicate));
        return this;
    }

    ///<summary>
    ///Sets the default combination mode for predicates added via <see cref="Where"/>. Defaults to ///<see
    ///cref="FilterCombineMode.And"/>.
    ///</summary>
    ///<param name="mode">The combination mode.</param>
    ///<returns>The current builder for fluent chaining.</returns>
    public QueryableFilterBuilder<T> WithDefaultMode(FilterCombineMode mode)
    {
        _defaultMode = mode;
        return this;
    }
    #endregion

    #region Public properties
    ///<summary>
    ///Returns the number of predicates currently in the builder.
    ///</summary>
    public int Count => _predicates.Count;
    #endregion
}
