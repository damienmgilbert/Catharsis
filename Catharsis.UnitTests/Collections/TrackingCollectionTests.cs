using Catharsis;
using Catharsis.Collections;

namespace Catharsis.UnitTests.Collections;

[TestClass]
public class TrackingCollectionTests
{
    [TestMethod]
    public void Add_InvokesCallback()
    {
        int tracked = -1;
        TrackingCollection<int> c = new TrackingCollection<int>(x => tracked = x);

        c.Add(42);

        Assert.AreEqual(42, tracked);
        Assert.AreEqual(1, c.Count);
    }

    [TestMethod]
    public void Remove_RemovesItem()
    {
        TrackingCollection<int> c = new TrackingCollection<int>(_ => { });
        c.Add(1); c.Add(2);

        Assert.IsTrue(c.Remove(1));
        Assert.AreEqual(1, c.Count);
    }

    [TestMethod]
    public void Contains_ReturnsCorrectResult()
    {
        TrackingCollection<string> c = new TrackingCollection<string>(_ => { });
        c.Add("hello");
        Assert.IsTrue(c.Contains("hello"));
        Assert.IsFalse(c.Contains("world"));
    }

    [TestMethod]
    public void Clear_RemovesAllItems()
    {
        TrackingCollection<int> c = new TrackingCollection<int>(_ => { });
        c.Add(1); c.Add(2);
        c.Clear();
        Assert.AreEqual(0, c.Count);
    }
}
