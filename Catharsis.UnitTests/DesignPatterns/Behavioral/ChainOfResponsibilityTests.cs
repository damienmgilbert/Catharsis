using Catharsis.DesignPatterns.Behavioral;

namespace Catharsis.UnitTests.DesignPatterns.Behavioral;

///<summary>
///Unit tests for the <see cref="ChainOfResponsibility"/> class.
///</summary>
[TestClass]
public class ChainOfResponsibilityTests
{
    #region Public methods
    [TestMethod]
    public void Chain_FirstHandlerHandles_StopsChain()
    {
        ChainOfResponsibility cor = new ChainOfResponsibility();
        int callCount = 0;

        cor.Chain(
        10,
        x =>
        {
            callCount++;
            return true;
        },
        x =>
        {
            callCount++;
            return true;
        });

        Assert.AreEqual(1, callCount);
    }

    [TestMethod]
    public void Chain_NoHandlerHandles_AllCalled()
    {
        ChainOfResponsibility cor = new ChainOfResponsibility();
        int callCount = 0;

        cor.Chain(
        "test",
        x =>
        {
            callCount++;
            return false;
        },
        x =>
        {
            callCount++;
            return false;
        });

        Assert.AreEqual(2, callCount);
    }

    [TestMethod]
    public void Chain_NullHandlers_Throws()
    {
        ChainOfResponsibility cor = new ChainOfResponsibility();
        Assert.ThrowsExactly<ArgumentNullException>(() => cor.Chain(1, null!));
    }

    [TestMethod]
    public void Chain_ReturnsOriginalObject()
    {
        ChainOfResponsibility cor = new ChainOfResponsibility();
        int result = cor.Chain(42, static x => false);
        Assert.AreEqual(42, result);
    }
    #endregion
}
