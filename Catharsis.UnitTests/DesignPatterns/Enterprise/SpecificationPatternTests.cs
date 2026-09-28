using Catharsis.DesignPatterns.Enterprise;

namespace Catharsis.UnitTests.DesignPatterns.Enterprise;

///<summary>
///Unit tests for the <see cref="SpecificationPattern"/> class.
///</summary>
[TestClass]
public class SpecificationPatternTests
{
    #region And

    [TestMethod]
    public void And_NullLeft_Throws() { Assert.ThrowsExactly<ArgumentNullException>(static () => SpecificationPattern.And<int>(null!, static _ => true)); }

    [TestMethod]
    public void And_NullRight_Throws() { Assert.ThrowsExactly<ArgumentNullException>(static () => SpecificationPattern.And<int>(static _ => true, null!)); }

    [TestMethod]
    public void And_BothTrue_ReturnsTrue()
    {
        Func<int, bool> combined = SpecificationPattern.And<int>(static x => x > 0, static x => x < 10);
        Assert.IsTrue(combined(5));
    }

    [TestMethod]
    public void And_OneFalse_ReturnsFalse()
    {
        Func<int, bool> combined = SpecificationPattern.And<int>(static x => x > 0, static x => x < 10);
        Assert.IsFalse(combined(15));
    }

    #endregion

    #region Or

    [TestMethod]
    public void Or_NullLeft_Throws() { Assert.ThrowsExactly<ArgumentNullException>(static () => SpecificationPattern.Or<int>(null!, static _ => true)); }

    [TestMethod]
    public void Or_NullRight_Throws() { Assert.ThrowsExactly<ArgumentNullException>(static () => SpecificationPattern.Or<int>(static _ => true, null!)); }

    [TestMethod]
    public void Or_EitherTrue_ReturnsTrue()
    {
        Func<int, bool> combined = SpecificationPattern.Or<int>(static x => x < 0, static x => x > 10);
        Assert.IsTrue(combined(15));
    }

    [TestMethod]
    public void Or_BothFalse_ReturnsFalse()
    {
        Func<int, bool> combined = SpecificationPattern.Or<int>(static x => x < 0, static x => x > 10);
        Assert.IsFalse(combined(5));
    }

    #endregion

    #region Not

    [TestMethod]
    public void Not_NullSpecification_Throws() { Assert.ThrowsExactly<ArgumentNullException>(static () => SpecificationPattern.Not<int>(null!)); }

    [TestMethod]
    public void Not_NegatesResult()
    {
        Func<int, bool> negated = SpecificationPattern.Not<int>(static x => x > 0);
        Assert.IsTrue(negated(-1));
        Assert.IsFalse(negated(1));
    }

    #endregion
}
