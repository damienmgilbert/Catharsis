using System.Linq.Expressions;

using Catharsis.Linq.Expressions;

namespace Catharsis.UnitTests.Linq.Expressions;

[TestClass]
public class ExpressionReducerTests
{
    #region CanReduce

    [TestMethod]
    public void CanReduce_NonReducible_ReturnsFalse()
    {
        Assert.IsFalse(ExpressionReducer.CanReduce(Expression.Constant(42)));
    }

    [TestMethod]
    public void CanReduce_Null_ReturnsFalse()
    {
        Assert.IsFalse(ExpressionReducer.CanReduce(null));
    }

    #endregion

    #region CanReduceAny

    [TestMethod]
    public void CanReduceAny_SimpleConstant_ReturnsFalse()
    {
        Assert.IsFalse(ExpressionReducer.CanReduceAny(Expression.Constant(5)));
    }

    [TestMethod]
    public void CanReduceAny_NullExpression_ThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(
            () => ExpressionReducer.CanReduceAny(null!));
    }

    #endregion

    #region Reduce

    [TestMethod]
    public void Reduce_NonReducible_ReturnsSameExpression()
    {
        ConstantExpression constant = Expression.Constant(42);

        Expression result = ExpressionReducer.Reduce(constant);

        Assert.AreSame(constant, result);
    }

    [TestMethod]
    public void Reduce_NullExpression_ThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(
            () => ExpressionReducer.Reduce(null!));
    }

    #endregion

    #region ReduceAndCheck

    [TestMethod]
    public void ReduceAndCheck_NonReducible_ThrowsInvalidOperationException()
    {
        Assert.ThrowsExactly<InvalidOperationException>(
            () => ExpressionReducer.ReduceAndCheck(Expression.Constant(42)));
    }

    #endregion

    #region ReduceBounded

    [TestMethod]
    public void ReduceBounded_NonReducible_ReturnsSame()
    {
        ConstantExpression constant = Expression.Constant(42);

        Expression result = ExpressionReducer.ReduceBounded(constant, 5);

        Assert.AreSame(constant, result);
    }

    [TestMethod]
    public void ReduceBounded_MaxIterationsLessThan1_ThrowsArgumentOutOfRangeException()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            () => ExpressionReducer.ReduceBounded(Expression.Constant(1), 0));
    }

    #endregion

    #region DeepReduce

    [TestMethod]
    public void DeepReduce_SimpleExpression_ReturnsEquivalent()
    {
        BinaryExpression add = Expression.Add(Expression.Constant(1), Expression.Constant(2));

        Expression result = ExpressionReducer.DeepReduce(add);

        Assert.IsNotNull(result);
    }

    [TestMethod]
    public void DeepReduce_Lambda_ReducesBody()
    {
        Expression<Func<int, int>> expr = x => x + 1;

        Expression<Func<int, int>> result = ExpressionReducer.DeepReduce(expr);

        Assert.IsNotNull(result);
        Assert.AreEqual(6, result.Compile()(5));
    }

    [TestMethod]
    public void DeepReduce_LambdaExpression_ReducesBody()
    {
        ParameterExpression p = Expression.Parameter(typeof(int), "x");
        LambdaExpression lambda = Expression.Lambda(Expression.Add(p, Expression.Constant(1)), p);

        LambdaExpression result = ExpressionReducer.DeepReduce(lambda);

        Assert.IsNotNull(result);
    }

    #endregion

    #region DeepReduceBounded

    [TestMethod]
    public void DeepReduceBounded_ReturnsReducedExpression()
    {
        BinaryExpression add = Expression.Add(Expression.Constant(1), Expression.Constant(2));

        Expression result = ExpressionReducer.DeepReduceBounded(add, 3);

        Assert.IsNotNull(result);
    }

    [TestMethod]
    public void DeepReduceBounded_MaxPassesLessThan1_ThrowsArgumentOutOfRangeException()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            () => ExpressionReducer.DeepReduceBounded(Expression.Constant(1), 0));
    }

    #endregion

    #region CountReducible / CollectReducible

    [TestMethod]
    public void CountReducible_SimpleTree_ReturnsCount()
    {
        ConstantExpression constant = Expression.Constant(42);

        int count = ExpressionReducer.CountReducible(constant);

        Assert.AreEqual(0, count);
    }

    [TestMethod]
    public void CollectReducible_SimpleTree_ReturnsEmptyList()
    {
        ConstantExpression constant = Expression.Constant(42);

        IReadOnlyList<Expression> nodes = ExpressionReducer.CollectReducible(constant);

        Assert.AreEqual(0, nodes.Count);
    }

    #endregion

    #region ReduceByNodeType / ReduceByNodeTypes

    [TestMethod]
    public void ReduceByNodeType_NonReducibleType_ReturnsSame()
    {
        ConstantExpression constant = Expression.Constant(42);

        Expression result = ExpressionReducer.ReduceByNodeType(constant, ExpressionType.Add);

        Assert.IsNotNull(result);
    }

    [TestMethod]
    public void ReduceByNodeTypes_EmptySet_ReturnsSame()
    {
        ConstantExpression constant = Expression.Constant(42);
        HashSet<ExpressionType> nodeTypes = [];

        Expression result = ExpressionReducer.ReduceByNodeTypes(constant, nodeTypes);

        Assert.IsNotNull(result);
    }

    #endregion

    #region ReduceWith

    [TestMethod]
    public void ReduceWith_CustomVisitor_AppliesVisitor()
    {
        ConstantExpression constant = Expression.Constant(42);
        IdentityVisitor visitor = new();

        Expression result = ExpressionReducer.ReduceWith(constant, visitor);

        Assert.AreSame(constant, result);
    }

    [TestMethod]
    public void ReduceWith_Lambda_CustomVisitor_AppliesVisitor()
    {
        Expression<Func<int, int>> expr = x => x + 1;
        IdentityVisitor visitor = new();

        Expression<Func<int, int>> result = ExpressionReducer.ReduceWith(expr, visitor);

        Assert.IsNotNull(result);
        Assert.AreEqual(6, result.Compile()(5));
    }

    [TestMethod]
    public void ReduceWith_NullExpression_ThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(
            () => ExpressionReducer.ReduceWith(null!, new IdentityVisitor()));
    }

    [TestMethod]
    public void ReduceWith_NullVisitor_ThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(
            () => ExpressionReducer.ReduceWith(Expression.Constant(1), null!));
    }

    #endregion

    private sealed class IdentityVisitor : ExpressionVisitor;
}
