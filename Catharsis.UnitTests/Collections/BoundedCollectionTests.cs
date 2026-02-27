using Catharsis;
using Catharsis.Collections;

namespace Catharsis.UnitTests.Collections;

[TestClass]
public class BoundedCollectionTests
{
    [TestMethod]
    public void Constructor_ValidCapacity_Creates()
    {
        BoundedCollection<int> c = new BoundedCollection<int>(5);
        Assert.AreEqual(5, c.MaxCapacity);
        Assert.AreEqual(0, c.Count);
    }

    [TestMethod]
    public void Constructor_ZeroCapacity_Throws()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new BoundedCollection<int>(0));
    }

    [TestMethod]
    public void Add_BelowCapacity_Succeeds()
    {
        BoundedCollection<int> c = new BoundedCollection<int>(2);
        c.Add(1);
        Assert.AreEqual(1, c.Count);
        Assert.IsFalse(c.IsFull);
    }

    [TestMethod]
    public void Add_AtCapacity_Throws()
    {
        BoundedCollection<int> c = new BoundedCollection<int>(1);
        c.Add(1);
        Assert.ThrowsExactly<InvalidOperationException>(() => c.Add(2));
    }

    [TestMethod]
    public void TryAdd_AtCapacity_ReturnsFalse()
    {
        BoundedCollection<int> c = new BoundedCollection<int>(1);
        c.Add(1);
        Assert.IsFalse(c.TryAdd(2));
    }

    [TestMethod]
    public void TryAdd_BelowCapacity_ReturnsTrue()
    {
        BoundedCollection<int> c = new BoundedCollection<int>(2);
        Assert.IsTrue(c.TryAdd(1));
        Assert.AreEqual(1, c.Count);
    }

    [TestMethod]
    public void Remove_ExistingItem_ReturnsTrue()
    {
        BoundedCollection<int> c = new BoundedCollection<int>(3);
        c.Add(1);
        Assert.IsTrue(c.Remove(1));
        Assert.AreEqual(0, c.Count);
    }

    [TestMethod]
    public void Contains_ExistingItem_ReturnsTrue()
    {
        BoundedCollection<string> c = new BoundedCollection<string>(3);
        c.Add("hello");
        Assert.IsTrue(c.Contains("hello"));
        Assert.IsFalse(c.Contains("world"));
    }

    [TestMethod]
    public void Clear_ResetsCollection()
    {
        BoundedCollection<int> c = new BoundedCollection<int>(3);
        c.Add(1); c.Add(2);
        c.Clear();
        Assert.AreEqual(0, c.Count);
        Assert.IsFalse(c.IsFull);
    }
}
