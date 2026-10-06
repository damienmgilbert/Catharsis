using System.Linq.Expressions;

namespace Catharsis.Linq.Expressions;

///<summary>
///Provides <c>And</c>, <c>Or</c>, and <c>Not</c> combinators for composing <see cref="Expression{TDelegate}"/>
///predicates into a single expression tree that a provider such as EF Core can translate, rather than composing the
///compiled delegates (which a provider cannot see into).
///</summary>
///<example>
public static class PredicateCombinator
{
    #region Private methods
    private static Expression<Func<T, bool>> Combine<T>(Expression<Func<T, bool>> left, Expression<Func<T, bool>> right, Func<Expression, Expression, BinaryExpression> combinator)
    {
        ParameterExpression parameter = left.Parameters[0];
        Expression rightBody = new ParameterRebinder(right.Parameters[0], parameter).Visit(right.Body);
        return Expression.Lambda<Func<T, bool>>(combinator(left.Body, rightBody), parameter);
    }
    #endregion

    #region Public methods
    ///<summary>
    ///Combines two predicates with a logical AND.
    ///</summary>
    ///<typeparam name="T">The predicate's parameter type.</typeparam>
    ///<param name="left">The left predicate.</param>
    ///<param name="right">The right predicate.</param>
    ///<returns>A predicate equivalent to <c>left(x) &amp;&amp; right(x)</c>.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="left"/> or <paramref name="right"/> is <c>null</c>.</exception>
    public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> left, Expression<Func<T, bool>> right)
    {
        ArgumentNullException.ThrowIfNull(left, nameof(left));
        ArgumentNullException.ThrowIfNull(right, nameof(right));
        return Combine(left, right, Expression.AndAlso);
    }

    ///<summary>
    ///Negates a predicate.
    ///</summary>
    ///<typeparam name="T">The predicate's parameter type.</typeparam>
    ///<param name="expression">The predicate to negate.</param>
    ///<returns>A predicate equivalent to <c>!expression(x)</c>.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="expression"/> is <c>null</c>.</exception>
    public static Expression<Func<T, bool>> Not<T>(this Expression<Func<T, bool>> expression)
    {
        ArgumentNullException.ThrowIfNull(expression, nameof(expression));
        return Expression.Lambda<Func<T, bool>>(Expression.Not(expression.Body), expression.Parameters);
    }

    ///<summary>
    ///Combines two predicates with a logical OR.
    ///</summary>
    ///<typeparam name="T">The predicate's parameter type.</typeparam>
    ///<param name="left">The left predicate.</param>
    ///<param name="right">The right predicate.</param>
    ///<returns>A predicate equivalent to <c>left(x) || right(x)</c>.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="left"/> or <paramref name="right"/> is <c>null</c>.</exception>
    public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> left, Expression<Func<T, bool>> right)
    {
        ArgumentNullException.ThrowIfNull(left, nameof(left));
        ArgumentNullException.ThrowIfNull(right, nameof(right));
        return Combine(left, right, Expression.OrElse);
    }
    #endregion

    private sealed class ParameterRebinder(ParameterExpression from, ParameterExpression to) : ExpressionVisitor
    {
        #region Protected methods
        protected override Expression VisitParameter(ParameterExpression node) => node == from ? to : base.VisitParameter(node);
        #endregion
    }
}
