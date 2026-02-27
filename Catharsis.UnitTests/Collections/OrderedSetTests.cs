using Catharsis;
using Catharsis.Collections;

namespace Catharsis.UnitTests.Collections;

[TestClass]
public class OrderedSetTests
{
    [TestMethod]
    public void TryAdd_UniqueItems_ReturnsTrue()
    {
        OrderedSet<int> set = new OrderedSet<int>();
        Assert.IsTrue(set.TryAdd(1));
        Assert.IsTrue(set.TryAdd(2));
        Assert.AreEqual(2, set.Count);
    }

    [TestMethod]
    public void TryAdd_Duplicate_ReturnsFalse()
    {
        OrderedSet<int> set = new OrderedSet<int>();
        set.TryAdd(1);
        Assert.IsFalse(set.TryAdd(1));
        Assert.AreEqual(1, set.Count);
    }

    [TestMethod]
    public void Indexer_ReturnsInsertionOrder()
    {
        OrderedSet<string> set = new OrderedSet<string>();
        set.TryAdd("b"); set.TryAdd("a"); set.TryAdd("c");
        Assert.AreEqual("b", set[0]);
        Assert.AreEqual("a", set[1]);
        Assert.AreEqual("c", set[2]);
    }

    [TestMethod]
    public void Remove_ExistingItem_ReturnsTrue()
    {
        OrderedSet<int> set = new OrderedSet<int>();
        set.TryAdd(1); set.TryAdd(2);
        Assert.IsTrue(set.Remove(1));
        Assert.AreEqual(1, set.Count);
        Assert.IsFalse(set.Contains(1));
    }

    [TestMethod]
    public void Contains_ReturnsCorrectResult()
    {
        OrderedSet<int> set = new OrderedSet<int>();
        set.TryAdd(42);
        Assert.IsTrue(set.Contains(42));
        Assert.IsFalse(set.Contains(99));
    }

    [TestMethod]
    public void Clear_RemovesAllItems()
    {
        OrderedSet<int> set = new OrderedSet<int>();
        set.TryAdd(1); set.TryAdd(2);
        set.Clear();
        Assert.AreEqual(0, set.Count);
    }

    [TestMethod]
    public void Constructor_NullComparer_Throws()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => new OrderedSet<int>(null!));
    }
}
