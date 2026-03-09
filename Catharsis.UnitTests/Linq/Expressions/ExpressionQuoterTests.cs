using System.Linq.Expressions;

using Catharsis.Linq.Expressions;

namespace Catharsis.UnitTests.Linq.Expressions;

[TestClass]
public class ExpressionQuoterTests
{
    #region Quote / IsQuoted

    [TestMethod]
    public void Quote_LambdaExpression_CreatesQuoteNode()
    {
        Expression<Func<int, bool>> lambda = x => x > 0;

        UnaryExpression quoted = ExpressionQuoter.Quote(lambda);

        Assert.AreEqual(ExpressionType.Quote, quoted.NodeType);
    }

    [TestMethod]
    public void Quote_StronglyTyped_CreatesQuoteNode()
    {
        Expression<Func<int, int>> lambda = x => x + 1;

        UnaryExpression quoted = ExpressionQuoter.Quote<Func<int, int>>(lambda);

        Assert.AreEqual(ExpressionType.Quote, quoted.NodeType);
    }

    [TestMethod]
    public void IsQuoted_QuotedExpression_ReturnsTrue()
    {
        Expression<Func<int, bool>> lambda = x => x > 0;
        UnaryExpression quoted = Expression.Quote(lambda);

        Assert.IsTrue(ExpressionQuoter.IsQuoted(quoted));
    }

    [TestMethod]
    public void IsQuoted_NonQuotedExpression_ReturnsFalse()
    {
        Assert.IsFalse(ExpressionQuoter.IsQuoted(Expression.Constant(42)));
    }

    [TestMethod]
    public void IsQuoted_Null_ReturnsFalse()
    {
        Assert.IsFalse(ExpressionQuoter.IsQuoted(null));
    }

    #endregion

    #region Unquote

    [TestMethod]
    public void Unquote_QuotedExpression_ReturnsInnerLambda()
    {
        Expression<Func<int, bool>> lambda = x => x > 0;
        UnaryExpression quoted = Expression.Quote(lambda);

        Expression unquoted = ExpressionQuoter.Unquote(quoted);

        Assert.AreSame(lambda, unquoted);
    }

    [TestMethod]
    public void Unquote_NonQuotedExpression_ReturnsSameExpression()
    {
        ConstantExpression constant = Expression.Constant(42);

        Expression result = ExpressionQuoter.Unquote(constant);

        Assert.AreSame(constant, result);
    }

    [TestMethod]
    public void Unquote_StronglyTyped_ReturnsTypedLambda()
    {
        Expression<Func<int, bool>> lambda = x => x > 0;
        UnaryExpression quoted = Expression.Quote(lambda);

        Expression<Func<int, bool>> unquoted = ExpressionQuoter.Unquote<Func<int, bool>>(quoted);

        Assert.AreSame(lambda, unquoted);
    }

    [TestMethod]
    public void Unquote_StronglyTyped_NotQuoted_ThrowsInvalidOperationException()
    {
        Assert.ThrowsExactly<InvalidOperationException>(
            () => ExpressionQuoter.Unquote<Func<int, bool>>(Expression.Constant(42)));
    }

    #endregion

    #region DeepUnquote

    [TestMethod]
    public void DeepUnquote_SingleQuote_ReturnsInner()
    {
        Expression<Func<int, bool>> lambda = x => x > 0;
        UnaryExpression quoted = Expression.Quote(lambda);

        Expression result = ExpressionQuoter.DeepUnquote(quoted);

        Assert.AreSame(lambda, result);
    }

    [TestMethod]
    public void DeepUnquote_DoubleQuote_ReturnsInnermostLambda()
    {
        Expression<Func<int, bool>> lambda = x => x > 0;
        UnaryExpression singleQuoted = Expression.Quote(lambda);
        UnaryExpression doubleQuoted = Expression.Quote(
            Expression.Lambda(singleQuoted));

        Expression result = ExpressionQuoter.DeepUnquote(doubleQuoted);

        Assert.IsNotInstanceOfType<UnaryExpression>(result, "Should not be a Quote node");
    }

    [TestMethod]
    public void DeepUnquote_NonQuoted_ReturnsSame()
    {
        ConstantExpression constant = Expression.Constant(5);

        Expression result = ExpressionQuoter.DeepUnquote(constant);

        Assert.AreSame(constant, result);
    }

    #endregion

    #region QuoteDepth

    [TestMethod]
    public void QuoteDepth_NonQuoted_ReturnsZero()
    {
        Assert.AreEqual(0, ExpressionQuoter.QuoteDepth(Expression.Constant(1)));
    }

    [TestMethod]
    public void QuoteDepth_SingleQuote_ReturnsOne()
    {
        Expression<Func<int, bool>> lambda = x => x > 0;
        UnaryExpression quoted = Expression.Quote(lambda);

        Assert.AreEqual(1, ExpressionQuoter.QuoteDepth(quoted));
    }

    [TestMethod]
    public void QuoteDepth_Null_ReturnsZero()
    {
        Assert.AreEqual(0, ExpressionQuoter.QuoteDepth(null));
    }

    #endregion

    #region FindQuotedExpressions

    [TestMethod]
    public void FindQuotedExpressions_FindsQuotes()
    {
        Expression<Func<int, bool>> inner = x => x > 0;
        UnaryExpression quoted = Expression.Quote(inner);
        BinaryExpression tree = Expression.Equal(quoted, Expression.Constant(null, quoted.Type));

        IReadOnlyList<UnaryExpression> found = ExpressionQuoter.FindQuotedExpressions(tree);

        Assert.AreEqual(1, found.Count);
    }

    [TestMethod]
    public void FindQuotedExpressions_NoQuotes_ReturnsEmpty()
    {
        ConstantExpression constant = Expression.Constant(42);

        IReadOnlyList<UnaryExpression> found = ExpressionQuoter.FindQuotedExpressions(constant);

        Assert.AreEqual(0, found.Count);
    }

    #endregion

    #region StripAllQuotes

    [TestMethod]
    public void StripAllQuotes_RemovesQuoteNodes()
    {
        Expression<Func<int, bool>> inner = x => x > 0;
        UnaryExpression quoted = Expression.Quote(inner);

        Expression stripped = ExpressionQuoter.StripAllQuotes(quoted);

        Assert.AreNotEqual(ExpressionType.Quote, stripped.NodeType);
    }

    [TestMethod]
    public void StripAllQuotes_StronglyTyped_RemovesQuotes()
    {
        Expression<Func<int, bool>> lambda = x => x > 0;

        Expression<Func<int, bool>> stripped = ExpressionQuoter.StripAllQuotes(lambda);

        Assert.IsNotNull(stripped);
    }

    #endregion

    #region QuoteWithCapture

    [TestMethod]
    public void QuoteWithCapture_ReplacesParameters()
    {
        ParameterExpression p = Expression.Parameter(typeof(int), "x");
        LambdaExpression lambda = Expression.Lambda(Expression.Add(p, Expression.Constant(1)), p);

        ConstantExpression replacement = Expression.Constant(10);
        Dictionary<ParameterExpression, Expression> replacements = new() { [p] = replacement };

        UnaryExpression result = ExpressionQuoter.QuoteWithCapture(lambda, replacements);

        Assert.AreEqual(ExpressionType.Quote, result.NodeType);
    }

    #endregion

    #region Null guards

    [TestMethod]
    public void Quote_NullLambda_ThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(
            () => ExpressionQuoter.Quote((LambdaExpression)null!));
    }

    [TestMethod]
    public void DeepUnquote_NullExpression_ThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(
            () => ExpressionQuoter.DeepUnquote(null!));
    }

    #endregion
}
