using Catharsis;
using Catharsis.Collections;

namespace Catharsis.UnitTests.Collections;

///<summary>
///Unit tests for the <see cref="OrderedSet"/> class.
///</summary>
[TestClass]
public class OrderedSetTests
{
    [TestMethod]
    public void TryAdd_UniqueItems_ReturnsTrue()
    {
        OrderedSet<int> set = new OrderedSet<int>();
        Assert.IsTrue(set.TryAdd(1));
        Assert.IsTrue(set.TryAdd(2));
        Assert.HasCount(2, set);
    }

    [TestMethod]
    public void TryAdd_Duplicate_ReturnsFalse()
    {
        OrderedSet<int> set = new OrderedSet<int>();
        set.TryAdd(1);
        Assert.IsFalse(set.TryAdd(1));
        Assert.HasCount(1, set);
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
        Assert.HasCount(1, set);
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
        Assert.IsEmpty(set);
    }

    [TestMethod]
    public void Constructor_NullComparer_Throws()
    {
        Assert.ThrowsExactly<ArgumentNullException>(static () => new OrderedSet<int>(null!));
    }
}
