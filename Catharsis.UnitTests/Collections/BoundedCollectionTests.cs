using Catharsis;
using Catharsis.Collections;

namespace Catharsis.UnitTests.Collections;

///<summary>
///Unit tests for the <see cref="BoundedCollection"/> class.
///</summary>
[TestClass]
public class BoundedCollectionTests
{
    [TestMethod]
    public void Constructor_ValidCapacity_Creates()
    {
        BoundedCollection<int> c = new(5);
        Assert.AreEqual(5, c.MaxCapacity);
        Assert.IsEmpty(c);
    }

    [TestMethod]
    public void Constructor_ZeroCapacity_Throws()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(static () => new BoundedCollection<int>(0));
    }

    [TestMethod]
    public void Add_BelowCapacity_Succeeds()
    {
        BoundedCollection<int> c = new(2)
        {
            1
        };
        Assert.HasCount(1, c);
        Assert.IsFalse(c.IsFull);
    }

    [TestMethod]
    public void Add_AtCapacity_Throws()
    {
        BoundedCollection<int> c = new(1)
        {
            1
        };
        Assert.ThrowsExactly<InvalidOperationException>(() => c.Add(2));
    }

    [TestMethod]
    public void TryAdd_AtCapacity_ReturnsFalse()
    {
        BoundedCollection<int> c = new(1)
        {
            1
        };
        Assert.IsFalse(c.TryAdd(2));
    }

    [TestMethod]
    public void TryAdd_BelowCapacity_ReturnsTrue()
    {
        BoundedCollection<int> c = new(2);
        Assert.IsTrue(c.TryAdd(1));
        Assert.HasCount(1, c);
    }

    [TestMethod]
    public void Remove_ExistingItem_ReturnsTrue()
    {
        BoundedCollection<int> c = new(3)
        {
            1
        };
        Assert.IsTrue(c.Remove(1));
        Assert.IsEmpty(c);
    }

    [TestMethod]
    public void Contains_ExistingItem_ReturnsTrue()
    {
        BoundedCollection<string> c = new(3)
        {
            "hello"
        };
        Assert.IsTrue(c.Contains("hello"));
        Assert.IsFalse(c.Contains("world"));
    }

    [TestMethod]
    public void Clear_ResetsCollection()
    {
        BoundedCollection<int> c = new(3)
        {
            1,
            2
        };
        c.Clear();
        Assert.IsEmpty(c);
        Assert.IsFalse(c.IsFull);
    }
}
