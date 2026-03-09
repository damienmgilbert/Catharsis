using System.Linq.Expressions;

using Catharsis.Linq.Expressions;

namespace Catharsis.UnitTests.Linq.Expressions;

[TestClass]
public class ExpressionComposerTests
{
    #region AndAlso / AndAll

    [TestMethod]
    public void AndAlso_CombinesTwoPredicatesWithAnd()
    {
        Expression<Func<int, bool>> isPositive = x => x > 0;
        Expression<Func<int, bool>> isEven = x => x % 2 == 0;

        Expression<Func<int, bool>> combined = ExpressionComposer.AndAlso(isPositive, isEven);
        Func<int, bool> compiled = combined.Compile();

        Assert.IsTrue(compiled(4));
        Assert.IsFalse(compiled(3));
        Assert.IsFalse(compiled(-2));
    }

    [TestMethod]
    public void AndAlso_NullFirst_ThrowsArgumentNullException()
    {
        Expression<Func<int, bool>> second = x => x > 0;
        Assert.ThrowsExactly<ArgumentNullException>(
            () => ExpressionComposer.AndAlso<int>(null!, second));
    }

    [TestMethod]
    public void AndAll_CombinesMultiplePredicates()
    {
        Expression<Func<int, bool>>[] predicates =
        [
            x => x > 0,
            x => x < 100,
            x => x % 2 == 0
        ];

        Expression<Func<int, bool>> combined = ExpressionComposer.AndAll<int>(predicates);
        Func<int, bool> compiled = combined.Compile();

        Assert.IsTrue(compiled(50));
        Assert.IsFalse(compiled(51));
        Assert.IsFalse(compiled(200));
    }

    [TestMethod]
    public void AndAll_EmptyPredicates_ReturnsTruePredicate()
    {
        Expression<Func<int, bool>> combined = ExpressionComposer.AndAll<int>([]);
        Func<int, bool> compiled = combined.Compile();

        Assert.IsTrue(compiled(42));
        Assert.IsTrue(compiled(0));
    }

    #endregion

    #region OrElse / OrAny

    [TestMethod]
    public void OrElse_CombinesTwoPredicatesWithOr()
    {
        Expression<Func<int, bool>> isNegative = x => x < 0;
        Expression<Func<int, bool>> isLarge = x => x > 100;

        Expression<Func<int, bool>> combined = ExpressionComposer.OrElse(isNegative, isLarge);
        Func<int, bool> compiled = combined.Compile();

        Assert.IsTrue(compiled(-5));
        Assert.IsTrue(compiled(200));
        Assert.IsFalse(compiled(50));
    }

    [TestMethod]
    public void OrAny_CombinesMultiplePredicates()
    {
        Expression<Func<int, bool>>[] predicates =
        [
            x => x == 1,
            x => x == 5,
            x => x == 10
        ];

        Expression<Func<int, bool>> combined = ExpressionComposer.OrAny<int>(predicates);
        Func<int, bool> compiled = combined.Compile();

        Assert.IsTrue(compiled(1));
        Assert.IsTrue(compiled(5));
        Assert.IsTrue(compiled(10));
        Assert.IsFalse(compiled(7));
    }

    [TestMethod]
    public void OrAny_EmptyPredicates_ReturnsFalsePredicate()
    {
        Expression<Func<int, bool>> combined = ExpressionComposer.OrAny<int>([]);
        Func<int, bool> compiled = combined.Compile();

        Assert.IsFalse(compiled(42));
    }

    #endregion

    #region Not

    [TestMethod]
    public void Not_NegatesPredicate()
    {
        Expression<Func<int, bool>> isPositive = x => x > 0;

        Expression<Func<int, bool>> negated = ExpressionComposer.Not(isPositive);
        Func<int, bool> compiled = negated.Compile();

        Assert.IsFalse(compiled(5));
        Assert.IsTrue(compiled(-5));
    }

    [TestMethod]
    public void Not_NullPredicate_ThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(
            () => ExpressionComposer.Not<int>(null!));
    }

    #endregion

    #region Compose

    [TestMethod]
    public void Compose_ChainsExpressions()
    {
        Expression<Func<string, int>> getLength = s => s.Length;
        Expression<Func<int, bool>> isPositive = n => n > 0;

        Expression<Func<string, bool>> composed = ExpressionComposer.Compose(getLength, isPositive);
        Func<string, bool> compiled = composed.Compile();

        Assert.IsTrue(compiled("hello"));
        Assert.IsFalse(compiled(""));
    }

    [TestMethod]
    public void ComposeWith_AppliesSelectorThenPredicate()
    {
        Expression<Func<string, int>> selector = s => s.Length;
        Expression<Func<int, bool>> predicate = n => n >= 3;

        Expression<Func<string, bool>> composed = ExpressionComposer.ComposeWith(selector, predicate);
        Func<string, bool> compiled = composed.Compile();

        Assert.IsTrue(compiled("abc"));
        Assert.IsFalse(compiled("ab"));
    }

    #endregion

    #region Combine

    [TestMethod]
    public void Combine_AddsTwoExpressions()
    {
        Expression<Func<int, int>> addOne = x => x + 1;
        Expression<Func<int, int>> timesTwo = x => x * 2;

        Expression<Func<int, int>> combined = ExpressionComposer.Combine<int, int>(
            ExpressionType.Add, addOne, timesTwo);
        Func<int, int> compiled = combined.Compile();

        Assert.AreEqual((5 + 1) + (5 * 2), compiled(5));
    }

    #endregion

    #region RebindParameters

    [TestMethod]
    public void RebindParameters_WithDictionary_RebindsLambda()
    {
        ParameterExpression p1 = Expression.Parameter(typeof(int), "x");
        ParameterExpression p2 = Expression.Parameter(typeof(int), "y");
        LambdaExpression lambda = Expression.Lambda(p1, p1);

        Dictionary<ParameterExpression, ParameterExpression> map = new() { [p1] = p2 };

        LambdaExpression rebound = ExpressionComposer.RebindParameters(lambda, map);

        Assert.AreEqual("y", rebound.Parameters[0].Name);
    }

    [TestMethod]
    public void RebindParameters_WithLists_RebindsExpression()
    {
        ParameterExpression p1 = Expression.Parameter(typeof(int), "a");
        ParameterExpression p2 = Expression.Parameter(typeof(int), "b");

        Expression result = ExpressionComposer.RebindParameters(
            p1, [p1], [p2]);

        Assert.AreEqual("b", ((ParameterExpression)result).Name);
    }

    [TestMethod]
    public void RebindParameters_MismatchedCounts_ThrowsArgumentException()
    {
        ParameterExpression p = Expression.Parameter(typeof(int), "x");

        Assert.ThrowsExactly<ArgumentException>(
            () => ExpressionComposer.RebindParameters(p, [p], []));
    }

    #endregion

    #region Replace

    [TestMethod]
    public void Replace_ReplacesExpressionNodes()
    {
        ParameterExpression x = Expression.Parameter(typeof(int), "x");
        ConstantExpression five = Expression.Constant(5);
        ConstantExpression ten = Expression.Constant(10);

        BinaryExpression addFive = Expression.Add(x, five);
        Expression replaced = ExpressionComposer.Replace(addFive, five, ten);

        Func<int, int> compiled = Expression.Lambda<Func<int, int>>(replaced, x).Compile();
        Assert.AreEqual(13, compiled(3));
    }

    [TestMethod]
    public void Replace_Lambda_ReplacesInLambdaBody()
    {
        ConstantExpression five = Expression.Constant(5);
        ConstantExpression ten = Expression.Constant(10);

        Expression<Func<int, int>> expr = x => x + 5;
        Expression<Func<int, int>> replaced = ExpressionComposer.Replace(expr, five, ten);

        Assert.IsNotNull(replaced);
    }

    #endregion

    #region Expand

    [TestMethod]
    public void Expand_InlinesInvocation()
    {
        Expression<Func<int, int>> inner = x => x * 2;
        ParameterExpression param = Expression.Parameter(typeof(int), "y");
        InvocationExpression invocation = Expression.Invoke(inner, param);
        Expression<Func<int, int>> outer = Expression.Lambda<Func<int, int>>(invocation, param);

        Expression<Func<int, int>> expanded = ExpressionComposer.Expand(outer);
        Func<int, int> compiled = expanded.Compile();

        Assert.AreEqual(10, compiled(5));
    }

    [TestMethod]
    public void Expand_NullExpression_ThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(
            () => ExpressionComposer.Expand<Func<int, bool>>(null!));
    }

    #endregion
}
