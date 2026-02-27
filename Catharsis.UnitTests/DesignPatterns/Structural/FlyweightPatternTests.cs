using System.Collections.Concurrent;
using Catharsis.DesignPatterns.Structural;

namespace Catharsis.UnitTests.DesignPatterns.Structural;

[TestClass]
public class FlyweightPatternTests
{
    #region Public methods
    [TestMethod]
    public void Flyweight_NullCache_Throws()
    {
        FlyweightPattern fw = new FlyweightPattern();
        Assert.ThrowsExactly<ArgumentNullException>(() => fw.Flyweight("k", null!, k => "v"));
    }

    [TestMethod]
    public void Flyweight_NullFactory_Throws()
    {
        FlyweightPattern fw = new FlyweightPattern();
        Assert.ThrowsExactly<ArgumentNullException>(() => fw.Flyweight("k", new ConcurrentDictionary<string, string>(), null!));
    }

    [TestMethod]
    public void Flyweight_ReturnsSharedInstance()
    {
        FlyweightPattern fw = new FlyweightPattern();
        ConcurrentDictionary<string, string> cache = new ConcurrentDictionary<string, string>();

        string r1 = fw.Flyweight("key", cache, k => $"value_{k}");
        string r2 = fw.Flyweight("key", cache, k => $"new_value_{k}");

        Assert.AreEqual("value_key", r1);
        Assert.AreSame(r1, r2);
    }
    #endregion
}
