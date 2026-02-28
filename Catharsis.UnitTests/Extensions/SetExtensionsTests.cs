using Catharsis.Extensions;

namespace Catharsis.UnitTests.Extensions;

[TestClass]
public class SetExtensionsTests
{
    #region Public methods
    [TestMethod]
    public void AddRange_AddsNewElements_ReturnsAddedCount()
    {
        ISet<int> source = new HashSet<int> { 1, 2, 3 };
        int added = source.AddRange(new[] { 3, 4, 5 });
        Assert.AreEqual(2, added);
        Assert.HasCount(5, source);
    }

    [TestMethod]
    public void AddRange_AllDuplicates_ReturnsZero()
    {
        ISet<int> source = new HashSet<int> { 1, 2, 3 };
        int added = source.AddRange(new[] { 1, 2, 3 });
        Assert.AreEqual(0, added);
    }

    [TestMethod]
    public void HashSet_RemoveWhere_RemovesMatchingElements()
    {
        HashSet<int> source = new HashSet<int> { 1, 2, 3, 4, 5, 6 };
        int removed = source.RemoveWhere(x => x % 2 == 0);
        Assert.AreEqual(3, removed);
        Assert.HasCount(3, source);
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

    [TestMethod]
    public void RemoveRange_RemovesExistingItems_ReturnsCount()
    {
        ISet<int> source = new HashSet<int> { 1, 2, 3, 4, 5 };
        int removed = source.RemoveRange(new[] { 2, 4, 99 });
        Assert.AreEqual(2, removed);
        Assert.HasCount(3, source);
    }

    [TestMethod]
    public void ReplaceWith_EmptyItems_ClearsSet()
    {
        ISet<int> source = new HashSet<int> { 1, 2, 3 };
        source.ReplaceWith(Array.Empty<int>());
        Assert.IsEmpty(source);
    }

    [TestMethod]
    public void ReplaceWith_ReplacesAllContents()
    {
        ISet<int> source = new HashSet<int> { 1, 2, 3 };
        source.ReplaceWith(new[] { 10, 20 });
        Assert.HasCount(2, source);
        Assert.Contains(10, source);
        Assert.Contains(20, source);
        Assert.DoesNotContain(1, source);
    }

    [TestMethod]
    public void SortedSet_RemoveWhere_RemovesMatchingElements()
    {
        SortedSet<int> source = new SortedSet<int> { 1, 2, 3, 4, 5, 6 };
        int removed = source.RemoveWhere(x => x > 4);
        Assert.AreEqual(2, removed);
        Assert.HasCount(4, source);
    }

    [TestMethod]
    public void Toggle_AddsMissingItem_ReturnsTrue()
    {
        ISet<int> source = new HashSet<int> { 1, 2, 3 };
        bool wasAdded = source.Toggle(4);
        Assert.IsTrue(wasAdded);
        Assert.Contains(4, source);
    }

    [TestMethod]
    public void Toggle_RemovesExistingItem_ReturnsFalse()
    {
        ISet<int> source = new HashSet<int> { 1, 2, 3 };
        bool wasAdded = source.Toggle(2);
        Assert.IsFalse(wasAdded);
        Assert.DoesNotContain(2, source);
    }
    #endregion
}
