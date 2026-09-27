using Catharsis.DesignPatterns.Enterprise;

namespace Catharsis.UnitTests.DesignPatterns.Enterprise;

///<summary>
///Unit tests for the <see cref="NullObjectPattern"/> class.
///</summary>
[TestClass]
public class NullObjectPatternTests
{
    #region Public methods

    [TestMethod]
    public void NullObject_NullFactory_Throws() { Assert.ThrowsExactly<ArgumentNullException>(static () => NullObjectPattern.NullObject<string>("value", null!)); }

    [TestMethod]
    public void NullObject_NonNullValue_ReturnsValue()
    {
        string result = NullObjectPattern.NullObject("value", static () => "fallback");
        Assert.AreEqual("value", result);
    }

    [TestMethod]
    public void NullObject_NullValue_ReturnsFactoryResult()
    {
        string result = NullObjectPattern.NullObject((string?)null, static () => "fallback");
        Assert.AreEqual("fallback", result);
    }

    #endregion
}
