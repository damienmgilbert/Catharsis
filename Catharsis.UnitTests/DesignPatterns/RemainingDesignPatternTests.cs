using Catharsis.DesignPatterns.Behavioral;
using Catharsis.DesignPatterns.Structural;
using System.Collections.Concurrent;

namespace Catharsis.DesignPatterns.UnitTests;

[TestClass]
public class ChainOfResponsibilityTests
{
    [TestMethod]
    public void Chain_FirstHandlerHandles_StopsChain()
    {
        var cor = new ChainOfResponsibility();
        int callCount = 0;

        cor.Chain(10,
            x => { callCount++; return true; },
            x => { callCount++; return true; });

        Assert.AreEqual(1, callCount);
    }

    [TestMethod]
    public void Chain_NoHandlerHandles_AllCalled()
    {
        var cor = new ChainOfResponsibility();
        int callCount = 0;

        cor.Chain("test",
            x => { callCount++; return false; },
            x => { callCount++; return false; });

        Assert.AreEqual(2, callCount);
    }

    [TestMethod]
    public void Chain_ReturnsOriginalObject()
    {
        var cor = new ChainOfResponsibility();
        int result = cor.Chain(42, x => false);
        Assert.AreEqual(42, result);
    }

    [TestMethod]
    public void Chain_NullHandlers_Throws()
    {
        var cor = new ChainOfResponsibility();
        Assert.ThrowsExactly<ArgumentNullException>(() => cor.Chain(1, null!));
    }
}

[TestClass]
public class StatePatternTests
{
    [TestMethod]
    public void State_AppliesBehaviorForState()
    {
        var sp = new StatePattern();
        var list = new List<int>();

        sp.State(list, "add", state => state switch
        {
            "add" => l => l.Add(42),
            _ => _ => { }
        });

        Assert.AreEqual(1, list.Count);
        Assert.AreEqual(42, list[0]);
    }

    [TestMethod]
    public void State_WithResult_ReturnsBehaviorResult()
    {
        var sp = new StatePattern();

        int result = sp.State(10, "double", state => state switch
        {
            "double" => (Func<int, int>)(x => x * 2),
            _ => x => x
        });

        Assert.AreEqual(20, result);
    }

    [TestMethod]
    public void State_NullBehaviorSelector_Throws()
    {
        var sp = new StatePattern();
        Assert.ThrowsExactly<ArgumentNullException>(
            () => sp.State(1, "s", (Func<string, Action<int>>)null!));
    }
}

[TestClass]
public class ProxyPatternTests
{
    [TestMethod]
    public void Proxy_ExecutesOperation()
    {
        var proxy = new ProxyPattern();
        var result = proxy.Proxy(5, x => x * 2);
        Assert.AreEqual(10, result);
    }

    [TestMethod]
    public void Proxy_ExecutesBeforeAndAfter()
    {
        var proxy = new ProxyPattern();
        var log = new List<string>();

        proxy.Proxy("test",
            x => x.Length,
            x => log.Add("before"),
            x => log.Add("after"));

        CollectionAssert.AreEqual(new[] { "before", "after" }, log);
    }

    [TestMethod]
    public void Proxy_NullOperation_Throws()
    {
        var proxy = new ProxyPattern();
        Assert.ThrowsExactly<ArgumentNullException>(
            () => proxy.Proxy(1, (Func<int, int>)null!));
    }
}

[TestClass]
public class FacadePatternTests
{
    [TestMethod]
    public void Facade_ExecutesSimplifiedOperation()
    {
        var facade = new FacadePattern();
        var result = facade.Facade("hello", s => s.ToUpper());
        Assert.AreEqual("HELLO", result);
    }

    [TestMethod]
    public void Facade_NullOperation_Throws()
    {
        var facade = new FacadePattern();
        Assert.ThrowsExactly<ArgumentNullException>(
            () => facade.Facade(1, (Func<int, int>)null!));
    }
}

[TestClass]
public class FlyweightPatternTests
{
    [TestMethod]
    public void Flyweight_ReturnsSharedInstance()
    {
        var fw = new FlyweightPattern();
        var cache = new ConcurrentDictionary<string, string>();

        var r1 = fw.Flyweight("key", cache, k => $"value_{k}");
        var r2 = fw.Flyweight("key", cache, k => $"new_value_{k}");

        Assert.AreEqual("value_key", r1);
        Assert.AreSame(r1, r2);
    }

    [TestMethod]
    public void Flyweight_NullCache_Throws()
    {
        var fw = new FlyweightPattern();
        Assert.ThrowsExactly<ArgumentNullException>(
            () => fw.Flyweight("k", null!, k => "v"));
    }

    [TestMethod]
    public void Flyweight_NullFactory_Throws()
    {
        var fw = new FlyweightPattern();
        Assert.ThrowsExactly<ArgumentNullException>(
            () => fw.Flyweight("k", new ConcurrentDictionary<string, string>(), null!));
    }
}
