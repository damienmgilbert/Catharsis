using System.Linq.Expressions;

namespace Catharsis.Linq.Expressions;

///<summary>
///Provides methods for quoting, unquoting, and analyzing quoted <see cref="Expression"/> trees. Quoting wraps a <see
///cref="LambdaExpression"/> in an <see cref="ExpressionType.Quote"/> node so that it is preserved as data rather than
///compiled.
///</summary>
public static class ExpressionQuoter
{
    #region Public methods

    ///<summary>
    ///Recursively removes all <see cref="ExpressionType.Quote"/> wrappers from the expression tree, returning the
    ///innermost non-quote expression.
    ///</summary>
    ///<param name="expression">The expression to deeply unquote.</param>
    ///<returns>The innermost non-quote expression.</returns>
    public static Expression DeepUnquote(Expression expression)
    {
        ArgumentNullException.ThrowIfNull(expression, nameof(expression));

        while(expression is UnaryExpression { NodeType: ExpressionType.Quote } unary)
        {
            expression = unary.Operand;
        }

        return expression;
    }

    ///<summary>
    ///Finds all <see cref="ExpressionType.Quote"/> nodes within the given expression tree.
    ///</summary>
    ///<param name="expression">The root expression to search.</param>
    ///<returns>A read-only list of all <see cref="UnaryExpression"/> quote nodes found.</returns>
    public static IReadOnlyList<UnaryExpression> FindQuotedExpressions(Expression expression)
    {
        ArgumentNullException.ThrowIfNull(expression, nameof(expression));

        QuoteFindingVisitor visitor = new();
        visitor.Visit(expression);
        return visitor.Quotes;
    }

    ///<summary>
    ///Determines whether the specified expression is a <see cref="ExpressionType.Quote"/> node.
    ///</summary>
    ///<param name="expression">The expression to test.</param>
    ///<returns><c>true</c> if the expression is a quote; otherwise <c>false</c>.</returns>
    public static bool IsQuoted(Expression? expression) => expression is UnaryExpression { NodeType: ExpressionType.Quote };

    ///<summary>
    ///Wraps the specified <see cref="LambdaExpression"/> in a <see cref="UnaryExpression"/> of type ///<see
    ///cref="ExpressionType.Quote"/>.
    ///</summary>
    ///<param name="lambda">The lambda expression to quote.</param>
    ///<returns>A <see cref="UnaryExpression"/> that quotes the lambda.</returns>
    public static UnaryExpression Quote(LambdaExpression lambda)
    {
        ArgumentNullException.ThrowIfNull(lambda, nameof(lambda));
        return Expression.Quote(lambda);
    }

    ///<summary>
    ///Wraps a strongly-typed <see cref="Expression{TDelegate}"/> in a <see cref="UnaryExpression"/> of type ///<see
    ///cref="ExpressionType.Quote"/>.
    ///</summary>
    ///<typeparam name="TDelegate">The delegate type of the lambda.</typeparam>
    ///<param name="lambda">The lambda expression to quote.</param>
    ///<returns>A <see cref="UnaryExpression"/> that quotes the lambda.</returns>
    public static UnaryExpression Quote<TDelegate>(Expression<TDelegate> lambda)
    {
        ArgumentNullException.ThrowIfNull(lambda, nameof(lambda));
        return Expression.Quote(lambda);
    }

    ///<summary>
    ///Counts the nesting depth of <see cref="ExpressionType.Quote"/> wrappers around the expression.
    ///</summary>
    ///<param name="expression">The expression to inspect.</param>
    ///<returns>The number of nested quote layers; <c>0</c> if not quoted.</returns>
    public static int QuoteDepth(Expression? expression)
    {
        int depth = 0;

        while(expression is UnaryExpression { NodeType: ExpressionType.Quote } unary)
        {
            depth++;
            expression = unary.Operand;
        }

        return depth;
    }

    ///<summary>
    ///Quotes the specified lambda and rebinds its parameters to the supplied replacement expressions. This is useful
    ///when constructing expression trees that reference parameters from an outer scope.
    ///</summary>
    ///<param name="lambda">The lambda to quote.</param>
    ///<param name="replacements">
    ///A dictionary mapping original parameters to the replacement expressions that should appear in the quoted tree.
    ///</param>
    ///<returns>A <see cref="UnaryExpression"/> quote with parameters replaced.</returns>
    public static UnaryExpression QuoteWithCapture(LambdaExpression lambda, IReadOnlyDictionary<ParameterExpression, Expression> replacements)
    {
        ArgumentNullException.ThrowIfNull(lambda, nameof(lambda));
        ArgumentNullException.ThrowIfNull(replacements, nameof(replacements));

        Expression body = lambda.Body;

        foreach(KeyValuePair<ParameterExpression, Expression> kvp in replacements)
        {
            body = new ReplacingVisitor(kvp.Key, kvp.Value).Visit(body);
        }

        ParameterExpression[] remainingParameters = [ .. lambda.Parameters.Where(p => !replacements.ContainsKey(p)) ];

        LambdaExpression rewritten = remainingParameters.Length > 0 ? Expression.Lambda(body, remainingParameters) : Expression.Lambda(body);

        return Expression.Quote(rewritten);
    }

    ///<summary>
    ///Returns a new expression tree with all <see cref="ExpressionType.Quote"/> nodes replaced by their operands
    ///throughout the entire tree.
    ///</summary>
    ///<param name="expression">The expression to strip quotes from.</param>
    ///<returns>An expression tree with all quotes removed.</returns>
    public static Expression StripAllQuotes(Expression expression)
    {
        ArgumentNullException.ThrowIfNull(expression, nameof(expression));
        return new QuoteStrippingVisitor().Visit(expression);
    }

    ///<summary>
    ///Returns a new lambda expression tree with all <see cref="ExpressionType.Quote"/> nodes replaced by their
    ///operands.
    ///</summary>
    ///<typeparam name="TDelegate">The delegate type of the lambda.</typeparam>
    ///<param name="expression">The lambda expression to strip quotes from.</param>
    ///<returns>A lambda expression with all quotes removed from the body.</returns>
    public static Expression<TDelegate> StripAllQuotes<TDelegate>(Expression<TDelegate> expression)
    {
        ArgumentNullException.ThrowIfNull(expression, nameof(expression));

        Expression newBody = new QuoteStrippingVisitor().Visit(expression.Body);
        return Expression.Lambda<TDelegate>(newBody, expression.Parameters);
    }

    ///<summary>
    ///Extracts the <see cref="LambdaExpression"/> operand from a quoted (<see cref="ExpressionType.Quote"/>) ///<see
    ///cref="UnaryExpression"/>.
    ///</summary>
    ///<param name="expression">The potentially quoted expression.</param>
    ///<returns>
    ///The inner <see cref="LambdaExpression"/> if <paramref name="expression"/> is a quote; otherwise the original
    ///expression.
    ///</returns>
    public static Expression Unquote(Expression expression)
    {
        ArgumentNullException.ThrowIfNull(expression, nameof(expression));

        return expression is UnaryExpression { NodeType: ExpressionType.Quote } unary ? unary.Operand : expression;
    }

    ///<summary>
    ///Extracts the <see cref="LambdaExpression"/> from a quoted expression, casting to the specified delegate type.
    ///</summary>
    ///<typeparam name="TDelegate">The expected delegate type of the inner lambda.</typeparam>
    ///<param name="expression">The quoted expression.</param>
    ///<returns>The inner <see cref="Expression{TDelegate}"/>.</returns>
    ///<exception cref="InvalidOperationException">
    ///The expression is not a <see cref="ExpressionType.Quote"/> node, or the operand is not of the expected type.
    ///</exception>
    public static Expression<TDelegate> Unquote<TDelegate>(Expression expression)
    {
        ArgumentNullException.ThrowIfNull(expression, nameof(expression));

        if(expression is not UnaryExpression { NodeType: ExpressionType.Quote } unary)
        {
            throw new InvalidOperationException($"Expression of NodeType '{expression.NodeType}' is not a Quote expression.");
        }

        if(unary.Operand is not Expression<TDelegate> typed)
        {
            throw new InvalidOperationException($"Quoted operand is of type '{unary.Operand.GetType().Name}', expected 'Expression<{typeof(TDelegate).Name}>'.");
        }

        return typed;
    }
    #endregion

    private sealed class ReplacingVisitor(Expression searchFor, Expression replaceWith) : ExpressionVisitor
    {
        #region Public methods
        public override Expression Visit(Expression? node) => node is not null && node == searchFor ? replaceWith : base.Visit(node)!;
        #endregion
    }

    private sealed class QuoteFindingVisitor : ExpressionVisitor
    {
        #region Fields
        private readonly List<UnaryExpression> _quotes = [];
        #endregion

        #region Protected methods
        protected override Expression VisitUnary(UnaryExpression node)
        {
            if(node.NodeType is ExpressionType.Quote)
            {
                _quotes.Add(node);
            }

            return base.VisitUnary(node);
        }
        #endregion

        #region Public properties
        public IReadOnlyList<UnaryExpression> Quotes => _quotes;
        #endregion
    }

    private sealed class QuoteStrippingVisitor : ExpressionVisitor
    {
        #region Protected methods
        protected override Expression VisitUnary(UnaryExpression node)
        {
            if(node.NodeType is ExpressionType.Quote)
            {
                return Visit(node.Operand);
            }

            return base.VisitUnary(node);
        }
        #endregion
    }
}
