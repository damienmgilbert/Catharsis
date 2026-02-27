namespace Catharsis.Extensions.UnitTests;

[TestClass]
public class ListExtensionsTests
{
    [TestMethod]
    public void ReplaceRange_ReplacesElementsAtIndex()
    {
        var source = new List<int> { 1, 2, 3, 4, 5 };
        source.ReplaceRange(1, 2, new[] { 20, 30, 40 });
        CollectionAssert.AreEqual(new[] { 1, 20, 30, 40, 4, 5 }, source);
    }

    [TestMethod]
    public void ReplaceRange_EmptyReplacement_JustRemoves()
    {
        var source = new List<int> { 1, 2, 3, 4, 5 };
        source.ReplaceRange(1, 2, Array.Empty<int>());
        CollectionAssert.AreEqual(new[] { 1, 4, 5 }, source);
    }

    [TestMethod]
    public void ReplaceRange_InvalidRange_ThrowsArgumentOutOfRange()
    {
        var source = new List<int> { 1, 2, 3 };
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => source.ReplaceRange(2, 5, new[] { 10 }));
    }

    [TestMethod]
    public void MoveItem_MovesElementForward()
    {
        var source = new List<int> { 1, 2, 3, 4, 5 };
        source.MoveItem(0, 3);
        CollectionAssert.AreEqual(new[] { 2, 3, 4, 1, 5 }, source);
    }

    [TestMethod]
    public void MoveItem_MovesElementBackward()
    {
        var source = new List<int> { 1, 2, 3, 4, 5 };
        source.MoveItem(3, 1);
        CollectionAssert.AreEqual(new[] { 1, 4, 2, 3, 5 }, source);
    }

    [TestMethod]
    public void MoveItem_SameIndex_NoChange()
    {
        var source = new List<int> { 1, 2, 3 };
        source.MoveItem(1, 1);
        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, source);
    }

    [TestMethod]
    public void MoveItem_InvalidIndex_ThrowsArgumentOutOfRange()
    {
        var source = new List<int> { 1, 2, 3 };
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => source.MoveItem(-1, 1));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => source.MoveItem(0, 5));
    }

    [TestMethod]
    public void LinkedList_AddRange_AddsAllToEnd()
    {
        var source = new LinkedList<int>(new[] { 1, 2 });
        source.AddRange(new[] { 3, 4, 5 });
        CollectionAssert.AreEqual(new[] { 1, 2, 3, 4, 5 }, source.ToList());
    }

    [TestMethod]
    public void LinkedList_RemoveWhere_RemovesMatchingNodes()
    {
        var source = new LinkedList<int>(new[] { 1, 2, 3, 4, 5, 6 });
        int removed = source.RemoveWhere(x => x % 2 == 0);
        Assert.AreEqual(3, removed);
        CollectionAssert.AreEqual(new[] { 1, 3, 5 }, source.ToList());
    }

    [TestMethod]
    public void LinkedList_ModifyAll_TransformsAllNodes()
    {
        var source = new LinkedList<int>(new[] { 1, 2, 3 });
        source.ModifyAll(x => x * 10);
        CollectionAssert.AreEqual(new[] { 10, 20, 30 }, source.ToList());
    }
}
