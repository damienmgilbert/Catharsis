using Catharsis;
using Catharsis.Collections;

namespace Catharsis.UnitTests.Collections;

///<summary>
///Unit tests for the <see cref="TrackingCollection"/> class.
///</summary>
[TestClass]
public class TrackingCollectionTests
{
    [TestMethod]
    public void Add_InvokesCallback()
    {
        int tracked = -1;
        TrackingCollection<int> c = new(x => tracked = x)
        {
            42
        };

        Assert.AreEqual(42, tracked);
        Assert.HasCount(1, c);
    }

    [TestMethod]
    public void Remove_RemovesItem()
    {
        TrackingCollection<int> c = new(static _ => { })
        {
            1,
            2
        };

        Assert.IsTrue(c.Remove(1));
        Assert.HasCount(1, c);
    }

    [TestMethod]
    public void Contains_ReturnsCorrectResult()
    {
        TrackingCollection<string> c = new(static _ => { })
        {
            "hello"
        };
        Assert.IsTrue(c.Contains("hello"));
        Assert.IsFalse(c.Contains("world"));
    }

    [TestMethod]
    public void Clear_RemovesAllItems()
    {
        TrackingCollection<int> c = new(static _ => { })
        {
            1,
            2
        };
        c.Clear();
        Assert.IsEmpty(c);
    }
}
