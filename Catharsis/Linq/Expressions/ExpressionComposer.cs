using System.Linq.Expressions;

namespace Catharsis.Linq.Expressions;

///<summary>
///Provides methods for composing, combining, and transforming <see cref="Expression"/> trees. Includes predicate
///combination, function composition, parameter rebinding, and expression visitor–based rewriting.
///</summary>
public static class ExpressionComposer
{
    #region Public methods

    ///<summary>
    ///Combines multiple predicates with short-circuit logical AND. All predicates must return <c>true</c> for the
    ///combined expression to return <c>true</c>.
    ///</summary>
    ///<typeparam name="T">The input type of the predicates.</typeparam>
    ///<param name="predicates">The predicates to combine.</param>
    ///<returns>
    ///A single predicate that is the logical AND of all inputs, or a constant <c>true</c> predicate if the sequence is
    ///empty.
    ///</returns>
    public static Expression<Func<T, bool>> AndAll<T>(IEnumerable<Expression<Func<T, bool>>> predicates)
    {
        ArgumentNullException.ThrowIfNull(predicates, nameof(predicates));

        Expression<Func<T, bool>>? result = null;

        foreach (Expression<Func<T, bool>> predicate in predicates)
        {
            result = result is null ? predicate : AndAlso(result, predicate);
        }

        return result ?? (static _ => true);
    }

    ///<summary>
    ///Combines two predicate expressions with a short-circuit logical AND (<c>&amp;&amp;</c>). Parameters from
    public static Expression<Func<T, bool>> AndAlso<T>(Expression<Func<T, bool>> first, Expression<Func<T, bool>> second)
    {
        ArgumentNullException.ThrowIfNull(first, nameof(first));
        ArgumentNullException.ThrowIfNull(second, nameof(second));

        Expression reboundBody = RebindParameters(second.Body, second.Parameters, first.Parameters);
        return Expression.Lambda<Func<T, bool>>(Expression.AndAlso(first.Body, reboundBody), first.Parameters);
    }

    ///<summary>
    ///Combines two expressions using the specified <see cref="ExpressionType"/> binary operator. Parameters from
    public static Expression<Func<T, TResult>> Combine<T, TResult>(ExpressionType binaryType, Expression<Func<T, TResult>> first, Expression<Func<T, TResult>> second)
    {
        ArgumentNullException.ThrowIfNull(first, nameof(first));
        ArgumentNullException.ThrowIfNull(second, nameof(second));

        Expression reboundBody = RebindParameters(second.Body, second.Parameters, first.Parameters);
        return Expression.Lambda<Func<T, TResult>>(Expression.MakeBinary(binaryType, first.Body, reboundBody), first.Parameters);
    }

    ///<summary>
    ///Composes two functions such that the output of <paramref name="first"/> is passed as the input to ///<paramref
    ///name="second"/>: <c>second(first(x))</c>.
    ///</summary>
    ///<typeparam name="TInput">The input type of the first function.</typeparam>
    ///<typeparam name="TMiddle">The output type of the first function and input type of the second.</typeparam>
    ///<typeparam name="TOutput">The output type of the second function.</typeparam>
    ///<param name="first">The first (inner) function.</param>
    ///<param name="second">The second (outer) function.</param>
    ///<returns>A new <see cref="Expression{TDelegate}"/> representing <c>second(first(x))</c>.</returns>
    public static Expression<Func<TInput, TOutput>> Compose<TInput, TMiddle, TOutput>(Expression<Func<TInput, TMiddle>> first, Expression<Func<TMiddle, TOutput>> second)
    {
        ArgumentNullException.ThrowIfNull(first, nameof(first));
        ArgumentNullException.ThrowIfNull(second, nameof(second));

        Expression inlined = RebindParameters(second.Body, second.Parameters, [first.Body]);
        return Expression.Lambda<Func<TInput, TOutput>>(inlined, first.Parameters);
    }

    ///<summary>
    ///Applies a selector to the input before evaluating a predicate: <c>predicate(selector(x))</c>.
    ///</summary>
    ///<typeparam name="TOuter">The original input type.</typeparam>
    ///<typeparam name="TInner">The type returned by the selector and expected by the predicate.</typeparam>
    ///<param name="selector">The selector to apply first.</param>
    ///<param name="predicate">The predicate to evaluate on the selector result.</param>
    ///<returns>A new predicate that applies <paramref name="selector"/> then <paramref name="predicate"/>.</returns>
    public static Expression<Func<TOuter, bool>> ComposeWith<TOuter, TInner>(Expression<Func<TOuter, TInner>> selector, Expression<Func<TInner, bool>> predicate)
    {
        ArgumentNullException.ThrowIfNull(selector, nameof(selector));
        ArgumentNullException.ThrowIfNull(predicate, nameof(predicate));

        return Compose<TOuter, TInner, bool>(selector, predicate);
    }

    ///<summary>
    ///Expands <see cref="InvocationExpression"/> nodes that invoke lambda expressions by inlining the lambda body and
    ///rebinding parameters. This is useful for query providers that do not support <see cref="Expression.Invoke"/>.
    ///</summary>
    ///<typeparam name="TDelegate">The delegate type of the outer expression.</typeparam>
    ///<param name="expression">The expression to expand.</param>
    ///<returns>An equivalent expression with invocations of lambdas inlined.</returns>
    public static Expression<TDelegate> Expand<TDelegate>(Expression<TDelegate> expression)
    {
        ArgumentNullException.ThrowIfNull(expression, nameof(expression));

        Expression expanded = new InvocationExpandingVisitor().Visit(expression.Body);
        return Expression.Lambda<TDelegate>(expanded, expression.Parameters);
    }

    ///<summary>
    ///Expands <see cref="InvocationExpression"/> nodes in a non-generic <see cref="LambdaExpression"/>.
    ///</summary>
    ///<param name="expression">The lambda expression to expand.</param>
    ///<returns>An equivalent lambda expression with invocations of lambdas inlined.</returns>
    public static LambdaExpression Expand(LambdaExpression expression)
    {
        ArgumentNullException.ThrowIfNull(expression, nameof(expression));

        Expression expanded = new InvocationExpandingVisitor().Visit(expression.Body);
        return Expression.Lambda(expression.Type, expanded, expression.Parameters);
    }

    ///<summary>
    ///Negates a predicate expression. Returns a new expression representing <c>!predicate(x)</c>.
    ///</summary>
    ///<typeparam name="T">The input type of the predicate.</typeparam>
    ///<param name="predicate">The predicate to negate.</param>
    ///<returns>A new <see cref="Expression{TDelegate}"/> representing the negated predicate.</returns>
    public static Expression<Func<T, bool>> Not<T>(Expression<Func<T, bool>> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate, nameof(predicate));

        return Expression.Lambda<Func<T, bool>>(Expression.Not(predicate.Body), predicate.Parameters);
    }

    ///<summary>
    ///Combines multiple predicates with short-circuit logical OR. Any predicate returning <c>true</c> causes the
    ///combined expression to return <c>true</c>.
    ///</summary>
    ///<typeparam name="T">The input type of the predicates.</typeparam>
    ///<param name="predicates">The predicates to combine.</param>
    ///<returns>
    ///A single predicate that is the logical OR of all inputs, or a constant <c>false</c> predicate if the sequence is
    ///empty.
    ///</returns>
    public static Expression<Func<T, bool>> OrAny<T>(IEnumerable<Expression<Func<T, bool>>> predicates)
    {
        ArgumentNullException.ThrowIfNull(predicates, nameof(predicates));

        Expression<Func<T, bool>>? result = null;

        foreach (Expression<Func<T, bool>> predicate in predicates)
        {
            result = result is null ? predicate : OrElse(result, predicate);
        }

        return result ?? (static _ => false);
    }

    ///<summary>
    ///Combines two predicate expressions with a short-circuit logical OR (<c>||</c>). Parameters from ///<paramref
    ///name="second"/> are rebound to match <paramref name="first"/>.
    ///</summary>
    ///<typeparam name="T">The input type of the predicates.</typeparam>
    ///<param name="first">The first predicate.</param>
    ///<param name="second">The second predicate.</param>
    ///<returns>A new <see cref="Expression{TDelegate}"/> representing <c>first(x) || second(x)</c>.</returns>
    public static Expression<Func<T, bool>> OrElse<T>(Expression<Func<T, bool>> first, Expression<Func<T, bool>> second)
    {
        ArgumentNullException.ThrowIfNull(first, nameof(first));
        ArgumentNullException.ThrowIfNull(second, nameof(second));

        Expression reboundBody = RebindParameters(second.Body, second.Parameters, first.Parameters);
        return Expression.Lambda<Func<T, bool>>(Expression.OrElse(first.Body, reboundBody), first.Parameters);
    }

    ///<summary>
    ///Rebinds parameters in <paramref name="lambda"/> using the provided mapping dictionary.
    ///</summary>
    ///<param name="lambda">The lambda expression to rebind.</param>
    ///<param name="parameterMap">A dictionary mapping original parameters to replacement parameters.</param>
    ///<returns>A new lambda expression with rebound parameters.</returns>
    public static LambdaExpression RebindParameters(LambdaExpression lambda, IReadOnlyDictionary<ParameterExpression, ParameterExpression> parameterMap)
    {
        ArgumentNullException.ThrowIfNull(lambda, nameof(lambda));
        ArgumentNullException.ThrowIfNull(parameterMap, nameof(parameterMap));

        Expression body = lambda.Body;

        foreach (KeyValuePair<ParameterExpression, ParameterExpression> kvp in parameterMap)
        {
            body = new ReplacingVisitor(kvp.Key, kvp.Value).Visit(body);
        }

        ParameterExpression[] newParameters = [.. lambda.Parameters.Select(p => parameterMap.TryGetValue(p, out ParameterExpression? replacement) ? replacement : p)];

        return Expression.Lambda(lambda.Type, body, newParameters);
    }

    ///<summary>
    ///Rebinds the parameters in <paramref name="expression"/> from <paramref name="source"/> to ///<paramref
    ///name="target"/> using the specified parameter mapping.
    ///</summary>
    ///<param name="expression">The expression whose parameters to rebind.</param>
    ///<param name="source">The original parameters to replace.</param>
    ///<param name="target">The replacement expressions (must be the same count as <paramref name="source"/>).</param>
    ///<returns>The expression with rebound parameters.</returns>
    public static Expression RebindParameters(Expression expression, IReadOnlyList<ParameterExpression> source, IReadOnlyList<Expression> target)
    {
        ArgumentNullException.ThrowIfNull(expression, nameof(expression));
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(target, nameof(target));

        if (source.Count != target.Count)
        {
            throw new ArgumentException($"Source parameter count ({source.Count}) must match target expression count ({target.Count}).", nameof(target));
        }

        Expression result = expression;

        for (int i = 0; i < source.Count; i++)
        {
            result = new ReplacingVisitor(source[i], target[i]).Visit(result);
        }

        return result;
    }

    ///<summary>
    ///Replaces all occurrences of <paramref name="searchFor"/> in <paramref name="expression"/> with ///<paramref
    ///name="replaceWith"/>.
    ///</summary>
    ///<param name="expression">The expression tree to search within.</param>
    ///<param name="searchFor">The expression to find.</param>
    ///<param name="replaceWith">The expression to substitute.</param>
    ///<returns>A new expression with all occurrences replaced.</returns>
    public static Expression Replace(Expression expression, Expression searchFor, Expression replaceWith)
    {
        ArgumentNullException.ThrowIfNull(expression, nameof(expression));
        ArgumentNullException.ThrowIfNull(searchFor, nameof(searchFor));
        ArgumentNullException.ThrowIfNull(replaceWith, nameof(replaceWith));

        return new ReplacingVisitor(searchFor, replaceWith).Visit(expression);
    }

    ///<summary>
    ///Replaces all occurrences of <paramref name="searchFor"/> in <paramref name="expression"/> with ///<paramref
    ///name="replaceWith"/> and returns a lambda of the specified delegate type.
    ///</summary>
    ///<typeparam name="TDelegate">The delegate type of the lambda.</typeparam>
    ///<param name="expression">The lambda expression tree to search within.</param>
    ///<param name="searchFor">The expression to find.</param>
    ///<param name="replaceWith">The expression to substitute.</param>
    ///<returns>A new lambda expression with all occurrences replaced.</returns>
    public static Expression<TDelegate> Replace<TDelegate>(Expression<TDelegate> expression, Expression searchFor, Expression replaceWith)
    {
        ArgumentNullException.ThrowIfNull(expression, nameof(expression));
        ArgumentNullException.ThrowIfNull(searchFor, nameof(searchFor));
        ArgumentNullException.ThrowIfNull(replaceWith, nameof(replaceWith));

        Expression newBody = Replace(expression.Body, searchFor, replaceWith);
        return Expression.Lambda<TDelegate>(newBody, expression.Parameters);
    }
    #endregion

    sealed class ReplacingVisitor(Expression searchFor, Expression replaceWith) : ExpressionVisitor
    {
        #region Public methods
        public override Expression Visit(Expression? node) { return node is not null && node == searchFor ? replaceWith : base.Visit(node)!; }
        #endregion
    }

    sealed class InvocationExpandingVisitor : ExpressionVisitor
    {
        #region Protected methods
        protected override Expression VisitInvocation(InvocationExpression node)
        {
            if (node.Expression is LambdaExpression lambda)
            {
                Expression body = lambda.Body;

                for (int i = 0; i < lambda.Parameters.Count; i++)
                {
                    body = new ReplacingVisitor(lambda.Parameters[i], Visit(node.Arguments[i])).Visit(body);
                }

                return Visit(body);
            }

            return base.VisitInvocation(node);
        }
        #endregion
    }
}
