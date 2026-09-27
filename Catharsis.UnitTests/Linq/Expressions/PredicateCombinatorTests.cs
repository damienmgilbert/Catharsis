using System.Linq.Expressions;
using Catharsis.Linq.Expressions;

namespace Catharsis.UnitTests.Linq.Expressions;

///<summary>
///Unit tests for the <see cref="PredicateCombinator"/> class.
///</summary>
[TestClass]
public class PredicateCombinatorTests
{
    #region And

    [TestMethod]
    public void And_NullLeft_Throws()
    {
        Expression<Func<int, bool>> right = static x => x > 0;
        Assert.ThrowsExactly<ArgumentNullException>(() => ((Expression<Func<int, bool>>)null!).And(right));
    }

    [TestMethod]
    public void And_NullRight_Throws()
    {
        Expression<Func<int, bool>> left = static x => x > 0;
        Assert.ThrowsExactly<ArgumentNullException>(() => left.And(null!));
    }

    [TestMethod]
    public void And_BothTrue_ReturnsTrue()
    {
        Expression<Func<int, bool>> isPositive = static x => x > 0;
        Expression<Func<int, bool>> isEven = static x => x % 2 == 0;

        Func<int, bool> combined = isPositive.And(isEven).Compile();

        Assert.IsTrue(combined(4));
    }

    [TestMethod]
    public void And_OneFalse_ReturnsFalse()
    {
        Expression<Func<int, bool>> isPositive = static x => x > 0;
        Expression<Func<int, bool>> isEven = static x => x % 2 == 0;

        Func<int, bool> combined = isPositive.And(isEven).Compile();

        Assert.IsFalse(combined(3));
        Assert.IsFalse(combined(-4));
    }

    #endregion

    #region Or

    [TestMethod]
    public void Or_NullArguments_Throws()
    {
        Expression<Func<int, bool>> predicate = static x => x > 0;
        Assert.ThrowsExactly<ArgumentNullException>(() => ((Expression<Func<int, bool>>)null!).Or(predicate));
        Assert.ThrowsExactly<ArgumentNullException>(() => predicate.Or(null!));
    }

    [TestMethod]
    public void Or_EitherTrue_ReturnsTrue()
    {
        Expression<Func<int, bool>> isNegative = static x => x < 0;
        Expression<Func<int, bool>> isEven = static x => x % 2 == 0;

        Func<int, bool> combined = isNegative.Or(isEven).Compile();

        Assert.IsTrue(combined(-3));
        Assert.IsTrue(combined(4));
    }

    [TestMethod]
    public void Or_BothFalse_ReturnsFalse()
    {
        Expression<Func<int, bool>> isNegative = static x => x < 0;
        Expression<Func<int, bool>> isEven = static x => x % 2 == 0;

        Func<int, bool> combined = isNegative.Or(isEven).Compile();

        Assert.IsFalse(combined(3));
    }

    #endregion

    #region Not

    [TestMethod]
    public void Not_NullExpression_Throws()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => ((Expression<Func<int, bool>>)null!).Not());
    }

    [TestMethod]
    public void Not_InvertsResult()
    {
        Expression<Func<int, bool>> isPositive = static x => x > 0;
        Func<int, bool> negated = isPositive.Not().Compile();

        Assert.IsFalse(negated(5));
        Assert.IsTrue(negated(-5));
    }

    #endregion

    #region Chained combinations

    [TestMethod]
    public void ChainedCombinators_ProduceCorrectResult()
    {
        Expression<Func<int, bool>> isPositive = static x => x > 0;
        Expression<Func<int, bool>> isEven = static x => x % 2 == 0;
        Expression<Func<int, bool>> isSmall = static x => x < 100;

        Func<int, bool> combined = isPositive.And(isEven).And(isSmall.Not()).Compile();

        // Positive, even, and NOT small (i.e. >= 100): only large positive even numbers qualify.
        Assert.IsTrue(combined(200));
        Assert.IsFalse(combined(50));
        Assert.IsFalse(combined(201));
    }

    #endregion
}
