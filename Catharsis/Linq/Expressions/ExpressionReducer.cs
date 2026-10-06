using System.Linq.Expressions;

namespace Catharsis.Linq.Expressions;

///<summary>
///Provides methods for reducing and simplifying <see cref="Expression"/> trees. Reduction replaces complex expression
///nodes with simpler equivalents using <see cref="Expression.Reduce"/> and custom visitor-based strategies.
///</summary>
public static class ExpressionReducer
{
    #region Public methods

    ///<summary>
    ///Determines whether the specified expression can be reduced.
    ///</summary>
    ///<param name="expression">The expression to test.</param>
    ///<returns><c>true</c> if the expression can be reduced; otherwise <c>false</c>.</returns>
    public static bool CanReduce(Expression? expression) { return expression is not null && expression.CanReduce; }

    ///<summary>
    ///Determines whether any node in the expression tree can be reduced.
    ///</summary>
    ///<param name="expression">The root expression to inspect.</param>
    ///<returns><c>true</c> if at least one node in the tree is reducible; otherwise <c>false</c>.</returns>
    public static bool CanReduceAny(Expression expression)
    {
        ArgumentNullException.ThrowIfNull(expression, nameof(expression));

        ReducibilityVisitor visitor = new();
        visitor.Visit(expression);
        return visitor.FoundReducible;
    }

    ///<summary>
    ///Collects all reducible nodes from the expression tree.
    ///</summary>
    ///<param name="expression">The root expression to search.</param>
    ///<returns>A read-only list of all reducible <see cref="Expression"/> nodes found in the tree.</returns>
    public static IReadOnlyList<Expression> CollectReducible(Expression expression)
    {
        ArgumentNullException.ThrowIfNull(expression, nameof(expression));

        ReducibleCollectorVisitor visitor = new();
        visitor.Visit(expression);
        return visitor.ReducibleNodes;
    }

    ///<summary>
    ///Counts the number of reducible nodes in the expression tree.
    ///</summary>
    ///<param name="expression">The root expression to inspect.</param>
    ///<returns>The total number of reducible nodes.</returns>
    public static int CountReducible(Expression expression)
    {
        ArgumentNullException.ThrowIfNull(expression, nameof(expression));

        ReducibleCountVisitor visitor = new();
        visitor.Visit(expression);
        return visitor.Count;
    }

    ///<summary>
    ///Recursively reduces all reducible nodes throughout the entire expression tree until no further reduction is
    ///possible. Each node is visited bottom-up and reduced if <see cref="Expression.CanReduce"/> is <c>true</c>.
    ///</summary>
    ///<param name="expression">The root expression to deeply reduce.</param>
    ///<returns>A fully reduced expression tree.</returns>
    public static Expression DeepReduce(Expression expression)
    {
        ArgumentNullException.ThrowIfNull(expression, nameof(expression));
        return new DeepReducingVisitor().Visit(expression);
    }

    ///<summary>
    ///Recursively reduces all reducible nodes in a strongly-typed lambda expression tree.
    ///</summary>
    ///<typeparam name="TDelegate">The delegate type of the lambda.</typeparam>
    ///<param name="expression">The lambda expression to deeply reduce.</param>
    ///<returns>A new lambda expression with a fully reduced body.</returns>
    public static Expression<TDelegate> DeepReduce<TDelegate>(Expression<TDelegate> expression)
    {
        ArgumentNullException.ThrowIfNull(expression, nameof(expression));

        Expression reducedBody = new DeepReducingVisitor().Visit(expression.Body);
        return Expression.Lambda<TDelegate>(reducedBody, expression.Parameters);
    }

    ///<summary>
    ///Recursively reduces all reducible nodes in a <see cref="LambdaExpression"/>.
    ///</summary>
    ///<param name="lambda">The lambda expression to deeply reduce.</param>
    ///<returns>A new lambda expression with a fully reduced body.</returns>
    public static LambdaExpression DeepReduce(LambdaExpression lambda)
    {
        ArgumentNullException.ThrowIfNull(lambda, nameof(lambda));

        Expression reducedBody = new DeepReducingVisitor().Visit(lambda.Body);
        return Expression.Lambda(lambda.Type, reducedBody, lambda.Parameters);
    }

    ///<summary>
    ///Performs a deep reduce on the expression tree up to the given maximum number of full-tree passes.
    ///</summary>
    ///<param name="expression">The root expression to reduce.</param>
    ///<param name="maxPasses">The maximum number of full-tree reduction passes.</param>
    ///<returns>The reduced expression tree.</returns>
    ///<exception cref="ArgumentOutOfRangeException"><paramref name="maxPasses"/> is less than 1.</exception>
    public static Expression DeepReduceBounded(Expression expression, int maxPasses)
    {
        ArgumentNullException.ThrowIfNull(expression, nameof(expression));
        ArgumentOutOfRangeException.ThrowIfLessThan(maxPasses, 1, nameof(maxPasses));

        for(int i = 0; i < maxPasses; i++)
        {
            Expression reduced = new DeepReducingVisitor().Visit(expression);

            if(ReferenceEquals(reduced, expression))
            {
                break;
            }

            expression = reduced;
        }

        return expression;
    }

    ///<summary>
    ///Reduces the specified expression if it is reducible; otherwise returns it unchanged.
    ///</summary>
    ///<param name="expression">The expression to reduce.</param>
    ///<returns>The reduced expression, or the original if it cannot be reduced.</returns>
    public static Expression Reduce(Expression expression)
    {
        ArgumentNullException.ThrowIfNull(expression, nameof(expression));
        return expression.CanReduce ? expression.Reduce() : expression;
    }

    ///<summary>
    ///Reduces the specified expression, verifying that the result is valid via ///<see
    ///cref="Expression.ReduceAndCheck"/>.
    ///</summary>
    ///<param name="expression">The expression to reduce and check.</param>
    ///<returns>The reduced expression after validation.</returns>
    ///<exception cref="InvalidOperationException">The expression is not reducible.</exception>
    public static Expression ReduceAndCheck(Expression expression)
    {
        ArgumentNullException.ThrowIfNull(expression, nameof(expression));

        if(!expression.CanReduce)
        {
            throw new InvalidOperationException($"Expression of NodeType '{expression.NodeType}' is not reducible.");
        }

        return expression.ReduceAndCheck();
    }

    ///<summary>
    ///Reduces the specified expression up to the given maximum number of iterations. This prevents infinite reduction
    ///loops when custom expression nodes continuously produce reducible output.
    ///</summary>
    ///<param name="expression">The expression to reduce.</param>
    ///<param name="maxIterations">The maximum number of reduction passes.</param>
    ///<returns>The reduced expression, or the last reducible state if the limit is reached.</returns>
    ///<exception cref="ArgumentOutOfRangeException"><paramref name="maxIterations"/> is less than 1.</exception>
    public static Expression ReduceBounded(Expression expression, int maxIterations)
    {
        ArgumentNullException.ThrowIfNull(expression, nameof(expression));
        ArgumentOutOfRangeException.ThrowIfLessThan(maxIterations, 1, nameof(maxIterations));

        for(int i = 0; i < maxIterations && expression.CanReduce; i++)
        {
            expression = expression.Reduce();
        }

        return expression;
    }

    ///<summary>
    ///Reduces only expression nodes that match the specified <see cref="ExpressionType"/>. All other nodes are left
    ///unchanged.
    ///</summary>
    ///<param name="expression">The root expression to process.</param>
    ///<param name="nodeType">The <see cref="ExpressionType"/> of nodes to reduce.</param>
    ///<returns>An expression tree with matching reducible nodes replaced by their reduced forms.</returns>
    public static Expression ReduceByNodeType(Expression expression, ExpressionType nodeType)
    {
        ArgumentNullException.ThrowIfNull(expression, nameof(expression));
        return new NodeTypeReducingVisitor(nodeType).Visit(expression);
    }

    ///<summary>
    ///Reduces only expression nodes whose <see cref="Expression.NodeType"/> is contained in the specified set.
    ///</summary>
    ///<param name="expression">The root expression to process.</param>
    ///<param name="nodeTypes">The set of <see cref="ExpressionType"/> values to reduce.</param>
    ///<returns>An expression tree with matching reducible nodes replaced by their reduced forms.</returns>
    public static Expression ReduceByNodeTypes(Expression expression, IReadOnlySet<ExpressionType> nodeTypes)
    {
        ArgumentNullException.ThrowIfNull(expression, nameof(expression));
        ArgumentNullException.ThrowIfNull(nodeTypes, nameof(nodeTypes));
        return new NodeTypeSetReducingVisitor(nodeTypes).Visit(expression);
    }

    ///<summary>
    ///Reduces the expression tree using a caller-supplied <see cref="ExpressionVisitor"/>. The visitor is applied to
    ///every node, allowing fully custom reduction or transformation logic.
    ///</summary>
    ///<param name="expression">The root expression to process.</param>
    ///<param name="visitor">The visitor that performs custom reduction.</param>
    ///<returns>The expression returned by the visitor.</returns>
    public static Expression ReduceWith(Expression expression, ExpressionVisitor visitor)
    {
        ArgumentNullException.ThrowIfNull(expression, nameof(expression));
        ArgumentNullException.ThrowIfNull(visitor, nameof(visitor));
        return visitor.Visit(expression);
    }

    ///<summary>
    ///Reduces a strongly-typed lambda expression using a caller-supplied <see cref="ExpressionVisitor"/>.
    ///</summary>
    ///<typeparam name="TDelegate">The delegate type of the lambda.</typeparam>
    ///<param name="expression">The lambda expression to process.</param>
    ///<param name="visitor">The visitor that performs custom reduction.</param>
    ///<returns>A new lambda expression with the visited body.</returns>
    public static Expression<TDelegate> ReduceWith<TDelegate>(Expression<TDelegate> expression, ExpressionVisitor visitor)
    {
        ArgumentNullException.ThrowIfNull(expression, nameof(expression));
        ArgumentNullException.ThrowIfNull(visitor, nameof(visitor));

        Expression reducedBody = visitor.Visit(expression.Body);
        return Expression.Lambda<TDelegate>(reducedBody, expression.Parameters);
    }
    #endregion

    sealed class ReducibilityVisitor : ExpressionVisitor
    {
        #region Public methods
        public override Expression Visit(Expression? node)
        {
            if(FoundReducible || node is null)
            {
                return node!;
            }

            if(node.CanReduce)
            {
                FoundReducible = true;
                return node;
            }

            return base.Visit(node);
        }
        #endregion

        #region Public properties
        public bool FoundReducible { get; private set; }
        #endregion
    }

    sealed class ReducibleCountVisitor : ExpressionVisitor
    {
        #region Public methods
        public override Expression Visit(Expression? node)
        {
            if(node is null)
            {
                return null!;
            }

            if(node.CanReduce)
            {
                Count++;
            }

            return base.Visit(node);
        }
        #endregion

        #region Public properties
        public int Count { get; private set; }
        #endregion
    }

    sealed class DeepReducingVisitor : ExpressionVisitor
    {
        #region Public methods
        public override Expression Visit(Expression? node)
        {
            if(node is null)
            {
                return null!;
            }

            Expression visited = base.Visit(node);
            return visited.CanReduce ? visited.Reduce() : visited;
        }
        #endregion
    }

    sealed class ReducibleCollectorVisitor : ExpressionVisitor
    {
        #region Fields
        readonly List<Expression> _reducibleNodes = [];
        #endregion

        #region Public methods
        public override Expression Visit(Expression? node)
        {
            if(node is null)
            {
                return null!;
            }

            if(node.CanReduce)
            {
                _reducibleNodes.Add(node);
            }

            return base.Visit(node);
        }
        #endregion

        #region Public properties
        public IReadOnlyList<Expression> ReducibleNodes => _reducibleNodes;
        #endregion
    }

    sealed class NodeTypeReducingVisitor(ExpressionType targetNodeType) : ExpressionVisitor
    {
        #region Public methods
        public override Expression Visit(Expression? node)
        {
            if(node is null)
            {
                return null!;
            }

            Expression visited = base.Visit(node);
            return visited.CanReduce && visited.NodeType == targetNodeType ? visited.Reduce() : visited;
        }
        #endregion
    }

    sealed class NodeTypeSetReducingVisitor(IReadOnlySet<ExpressionType> targetNodeTypes) : ExpressionVisitor
    {
        #region Public methods
        public override Expression Visit(Expression? node)
        {
            if(node is null)
            {
                return null!;
            }

            Expression visited = base.Visit(node);
            return visited.CanReduce && targetNodeTypes.Contains(visited.NodeType) ? visited.Reduce() : visited;
        }
        #endregion
    }
}
