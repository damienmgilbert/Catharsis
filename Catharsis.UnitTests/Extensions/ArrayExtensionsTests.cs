namespace Catharsis.Extensions.UnitTests;

[TestClass]
public class ArrayExtensionsTests
{
    [TestMethod]
    public void Add_AppendsElement_ReturnsNewArrayWithElement()
    {
        var source = new[] { 1, 2, 3 };
        var result = source.Add(4);
        CollectionAssert.AreEqual(new[] { 1, 2, 3, 4 }, result);
        Assert.AreEqual(3, source.Length);
    }

    [TestMethod]
    public void Add_EmptyArray_ReturnsSingleElementArray()
    {
        var source = Array.Empty<int>();
        var result = source.Add(42);
        CollectionAssert.AreEqual(new[] { 42 }, result);
    }

    [TestMethod]
    public void Add_NullSource_ThrowsArgumentNullException()
    {
        int[]? source = null;
        Assert.ThrowsExactly<ArgumentNullException>(() => source!.Add(1));
    }

    [TestMethod]
    public void AddRange_AppendsMultipleElements_ReturnsNewArray()
    {
        var source = new[] { 1, 2 };
        var result = source.AddRange(new[] { 3, 4, 5 });
        CollectionAssert.AreEqual(new[] { 1, 2, 3, 4, 5 }, result);
    }

    [TestMethod]
    public void AddRange_EmptyItems_ReturnsCopy()
    {
        var source = new[] { 1, 2 };
        var result = source.AddRange(Array.Empty<int>());
        CollectionAssert.AreEqual(new[] { 1, 2 }, result);
        Assert.AreNotSame(source, result);
    }

    [TestMethod]
    public void RemoveAt_MiddleIndex_RemovesElement()
    {
        var source = new[] { 1, 2, 3, 4, 5 };
        var result = source.RemoveAt(2);
        CollectionAssert.AreEqual(new[] { 1, 2, 4, 5 }, result);
    }

    [TestMethod]
    public void RemoveAt_FirstIndex_RemovesFirstElement()
    {
        var source = new[] { 10, 20, 30 };
        var result = source.RemoveAt(0);
        CollectionAssert.AreEqual(new[] { 20, 30 }, result);
    }

    [TestMethod]
    public void RemoveAt_LastIndex_RemovesLastElement()
    {
        var source = new[] { 10, 20, 30 };
        var result = source.RemoveAt(2);
        CollectionAssert.AreEqual(new[] { 10, 20 }, result);
    }

    [TestMethod]
    public void RemoveAt_InvalidIndex_ThrowsArgumentOutOfRangeException()
    {
        var source = new[] { 1, 2, 3 };
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => source.RemoveAt(5));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => source.RemoveAt(-1));
    }

    [TestMethod]
    public void RemoveAll_MatchingPredicate_RemovesMatchingElements()
    {
        var source = new[] { 1, 2, 3, 4, 5, 6 };
        var result = source.RemoveAll(x => x % 2 == 0);
        CollectionAssert.AreEqual(new[] { 1, 3, 5 }, result);
    }

    [TestMethod]
    public void RemoveAll_NoMatch_ReturnsCopy()
    {
        var source = new[] { 1, 3, 5 };
        var result = source.RemoveAll(x => x % 2 == 0);
        CollectionAssert.AreEqual(new[] { 1, 3, 5 }, result);
    }

    [TestMethod]
    public void Remove_ExistingItem_RemovesFirstOccurrence()
    {
        var source = new[] { 1, 2, 3, 2, 4 };
        var result = source.Remove(2);
        CollectionAssert.AreEqual(new[] { 1, 3, 2, 4 }, result);
    }

    [TestMethod]
    public void Remove_NonExistentItem_ReturnsCopy()
    {
        var source = new[] { 1, 2, 3 };
        var result = source.Remove(99);
        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, result);
        Assert.AreNotSame(source, result);
    }

    [TestMethod]
    public void SetAt_ValidIndex_ReplacesElement()
    {
        var source = new[] { 1, 2, 3 };
        var result = source.SetAt(1, 99);
        CollectionAssert.AreEqual(new[] { 1, 99, 3 }, result);
        Assert.AreEqual(2, source[1]);
    }

    [TestMethod]
    public void SetAt_InvalidIndex_ThrowsArgumentOutOfRangeException()
    {
        var source = new[] { 1, 2, 3 };
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => source.SetAt(5, 99));
    }

    [TestMethod]
    public void SetRange_ValidRange_ReplacesElements()
    {
        var source = new[] { 1, 2, 3, 4, 5 };
        var result = source.SetRange(1, new[] { 20, 30 });
        CollectionAssert.AreEqual(new[] { 1, 20, 30, 4, 5 }, result);
    }

    [TestMethod]
    public void SetRange_RangeExceedsBounds_ThrowsArgumentOutOfRangeException()
    {
        var source = new[] { 1, 2, 3 };
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => source.SetRange(2, new[] { 10, 20, 30 }));
    }

    [TestMethod]
    public void InsertAt_MiddleIndex_InsertsElement()
    {
        var source = new[] { 1, 2, 4, 5 };
        var result = source.InsertAt(2, 3);
        CollectionAssert.AreEqual(new[] { 1, 2, 3, 4, 5 }, result);
    }

    [TestMethod]
    public void InsertAt_Beginning_PrependElement()
    {
        var source = new[] { 2, 3 };
        var result = source.InsertAt(0, 1);
        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, result);
    }

    [TestMethod]
    public void InsertAt_End_AppendsElement()
    {
        var source = new[] { 1, 2 };
        var result = source.InsertAt(2, 3);
        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, result);
    }

    [TestMethod]
    public void InsertRange_MiddleIndex_InsertsElements()
    {
        var source = new[] { 1, 5 };
        var result = source.InsertRange(1, new[] { 2, 3, 4 });
        CollectionAssert.AreEqual(new[] { 1, 2, 3, 4, 5 }, result);
    }

    [TestMethod]
    public void ModifyAll_TransformsAllElements_ReturnsNewArray()
    {
        var source = new[] { 1, 2, 3 };
        var result = source.ModifyAll(x => x * 10);
        CollectionAssert.AreEqual(new[] { 10, 20, 30 }, result);
        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, source);
    }

    [TestMethod]
    public void ModifyWhere_TransformsMatchingElements_ReturnsNewArray()
    {
        var source = new[] { 1, 2, 3, 4, 5 };
        var result = source.ModifyWhere(x => x % 2 == 0, x => x * 10);
        CollectionAssert.AreEqual(new[] { 1, 20, 3, 40, 5 }, result);
    }
}
