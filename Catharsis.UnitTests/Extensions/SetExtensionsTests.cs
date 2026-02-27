using Catharsis.Extensions;

namespace Catharsis.Extensions.UnitTests;

[TestClass]
public class SetExtensionsTests
{
    [TestMethod]
    public void AddRange_AddsNewElements_ReturnsAddedCount()
    {
        ISet<int> source = new HashSet<int> { 1, 2, 3 };
        int added = source.AddRange(new[] { 3, 4, 5 });
        Assert.AreEqual(2, added);
        Assert.AreEqual(5, source.Count);
    }

    [TestMethod]
    public void AddRange_AllDuplicates_ReturnsZero()
    {
        ISet<int> source = new HashSet<int> { 1, 2, 3 };
        int added = source.AddRange(new[] { 1, 2, 3 });
        Assert.AreEqual(0, added);
    }

    [TestMethod]
    public void RemoveRange_RemovesExistingItems_ReturnsCount()
    {
        ISet<int> source = new HashSet<int> { 1, 2, 3, 4, 5 };
        int removed = source.RemoveRange(new[] { 2, 4, 99 });
        Assert.AreEqual(2, removed);
        Assert.AreEqual(3, source.Count);
    }

    [TestMethod]
    public void HashSet_RemoveWhere_RemovesMatchingElements()
    {
        var source = new HashSet<int> { 1, 2, 3, 4, 5, 6 };
        int removed = source.RemoveWhere(x => x % 2 == 0);
        Assert.AreEqual(3, removed);
        Assert.AreEqual(3, source.Count);
    }

    [TestMethod]
    public void SortedSet_RemoveWhere_RemovesMatchingElements()
    {
        var source = new SortedSet<int> { 1, 2, 3, 4, 5, 6 };
        int removed = source.RemoveWhere(x => x > 4);
        Assert.AreEqual(2, removed);
        Assert.AreEqual(4, source.Count);
    }

    [TestMethod]
    public void Toggle_AddsMissingItem_ReturnsTrue()
    {
        ISet<int> source = new HashSet<int> { 1, 2, 3 };
        bool wasAdded = source.Toggle(4);
        Assert.IsTrue(wasAdded);
        Assert.IsTrue(source.Contains(4));
    }

    [TestMethod]
    public void Toggle_RemovesExistingItem_ReturnsFalse()
    {
        ISet<int> source = new HashSet<int> { 1, 2, 3 };
        bool wasAdded = source.Toggle(2);
        Assert.IsFalse(wasAdded);
        Assert.IsFalse(source.Contains(2));
    }

    [TestMethod]
    public void ReplaceWith_ReplacesAllContents()
    {
        ISet<int> source = new HashSet<int> { 1, 2, 3 };
        source.ReplaceWith(new[] { 10, 20 });
        Assert.AreEqual(2, source.Count);
        Assert.IsTrue(source.Contains(10));
        Assert.IsTrue(source.Contains(20));
        Assert.IsFalse(source.Contains(1));
    }

    [TestMethod]
    public void ReplaceWith_EmptyItems_ClearsSet()
    {
        ISet<int> source = new HashSet<int> { 1, 2, 3 };
        source.ReplaceWith(Array.Empty<int>());
        Assert.AreEqual(0, source.Count);
    }

    [TestMethod]
    public void NullSource_ThrowsArgumentNullException()
    {
        ISet<int>? source = null;
        Assert.ThrowsExactly<ArgumentNullException>(() => source!.AddRange(new[] { 1 }));
        Assert.ThrowsExactly<ArgumentNullException>(() => source!.RemoveRange(new[] { 1 }));
        Assert.ThrowsExactly<ArgumentNullException>(() => source!.Toggle(1));
        Assert.ThrowsExactly<ArgumentNullException>(() => source!.ReplaceWith(new[] { 1 }));
    }
}
