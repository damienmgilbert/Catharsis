using Catharsis.DesignPatterns.Structural;

namespace Catharsis.UnitTests.DesignPatterns.Structural;

///<summary>
///Unit tests for the <see cref="FacadePattern"/> class.
///</summary>
[TestClass]
public class FacadePatternTests
{
    #region Public methods
    [TestMethod]
    public void Facade_ExecutesSimplifiedOperation()
    {
        FacadePattern facade = new();
        string result = FacadePattern.Facade("hello", static s => s.ToUpper());
        Assert.AreEqual("HELLO", result);
    }

    [TestMethod]
    public void Facade_NullOperation_Throws()
    {
        FacadePattern facade = new();
        Assert.ThrowsExactly<ArgumentNullException>(() => FacadePattern.Facade(1, (Func<int, int>)null!));
    }
    #endregion
}
