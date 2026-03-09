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
        FacadePattern facade = new FacadePattern();
        string result = facade.Facade("hello", static s => s.ToUpper());
        Assert.AreEqual("HELLO", result);
    }

    [TestMethod]
    public void Facade_NullOperation_Throws()
    {
        FacadePattern facade = new FacadePattern();
        Assert.ThrowsExactly<ArgumentNullException>(() => facade.Facade(1, (Func<int, int>)null!));
    }
    #endregion
}
