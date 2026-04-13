using Catharsis.DesignPatterns.Structural;

namespace Catharsis.UnitTests.DesignPatterns.Structural;

///<summary>
///Unit tests for the <see cref="ProxyPattern"/> class.
///</summary>
[TestClass]
public class ProxyPatternTests
{
    #region Public methods
    [TestMethod]
    public void Proxy_ExecutesBeforeAndAfter()
    {
        ProxyPattern proxy = new();
        List<string> log = [];

        ProxyPattern.Proxy("test", x => x.Length, x => log.Add("before"), x => log.Add("after"));

        CollectionAssert.AreEqual(new[] { "before", "after" }, log);
    }

    [TestMethod]
    public void Proxy_ExecutesOperation()
    {
        ProxyPattern proxy = new();
        int result = ProxyPattern.Proxy(5, static x => x * 2);
        Assert.AreEqual(10, result);
    }

    [TestMethod]
    public void Proxy_NullOperation_Throws()
    {
        ProxyPattern proxy = new();
        Assert.ThrowsExactly<ArgumentNullException>(() => ProxyPattern.Proxy(1, (Func<int, int>)null!));
    }
    #endregion
}
