using System.Collections.Concurrent;
using Catharsis.DesignPatterns.Structural;

namespace Catharsis.UnitTests.DesignPatterns.Structural;

///<summary>
///Unit tests for the <see cref="FlyweightPattern"/> class.
///</summary>
[TestClass]
public class FlyweightPatternTests
{
    #region Public methods
    [TestMethod]
    public void Flyweight_NullCache_Throws()
    {
        FlyweightPattern fw = new();
        Assert.ThrowsExactly<ArgumentNullException>(() => FlyweightPattern.Flyweight("k", null!, k => "v"));
    }

    [TestMethod]
    public void Flyweight_NullFactory_Throws()
    {
        FlyweightPattern fw = new();
        Assert.ThrowsExactly<ArgumentNullException>(() => FlyweightPattern.Flyweight("k", new ConcurrentDictionary<string, string>(), null!));
    }

    [TestMethod]
    public void Flyweight_ReturnsSharedInstance()
    {
        FlyweightPattern fw = new();
        ConcurrentDictionary<string, string> cache = new();

        string r1 = FlyweightPattern.Flyweight("key", cache, static k => $"value_{k}");
        string r2 = FlyweightPattern.Flyweight("key", cache, static k => $"new_value_{k}");

        Assert.AreEqual("value_key", r1);
        Assert.AreSame(r1, r2);
    }
    #endregion
}
