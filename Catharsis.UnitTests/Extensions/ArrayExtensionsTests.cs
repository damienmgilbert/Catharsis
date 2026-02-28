using Catharsis.Extensions;

namespace Catharsis.UnitTests.Extensions;

[TestClass]
public class ArrayExtensionsTests
{
    #region Public methods
    [TestMethod]
    public void Add_AppendsElement_ReturnsNewArrayWithElement()
    {
        int[] source = new[] { 1, 2, 3 };
        int[] result = source.Add(4);
        CollectionAssert.AreEqual(new[] { 1, 2, 3, 4 }, result);
        Assert.HasCount(3, source);
    }

    [TestMethod]
    public void Add_EmptyArray_ReturnsSingleElementArray()
    {
        int[] source = Array.Empty<int>();
        int[] result = source.Add(42);
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
        int[] source = new[] { 1, 2 };
        int[] result = source.AddRange(new[] { 3, 4, 5 });
        CollectionAssert.AreEqual(new[] { 1, 2, 3, 4, 5 }, result);
    }

    [TestMethod]
    public void AddRange_EmptyItems_ReturnsCopy()
    {
        int[] source = new[] { 1, 2 };
        int[] result = source.AddRange(Array.Empty<int>());
        CollectionAssert.AreEqual(new[] { 1, 2 }, result);
        Assert.AreNotSame(source, result);
    }

    [TestMethod]
    public void InsertAt_Beginning_PrependElement()
    {
        int[] source = new[] { 2, 3 };
        int[] result = source.InsertAt(0, 1);
        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, result);
    }

    [TestMethod]
    public void InsertAt_End_AppendsElement()
    {
        int[] source = new[] { 1, 2 };
        int[] result = source.InsertAt(2, 3);
        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, result);
    }

    [TestMethod]
    public void InsertAt_MiddleIndex_InsertsElement()
    {
        int[] source = new[] { 1, 2, 4, 5 };
        int[] result = source.InsertAt(2, 3);
        CollectionAssert.AreEqual(new[] { 1, 2, 3, 4, 5 }, result);
    }

    [TestMethod]
    public void InsertRange_MiddleIndex_InsertsElements()
    {
        int[] source = new[] { 1, 5 };
        int[] result = source.InsertRange(1, new[] { 2, 3, 4 });
        CollectionAssert.AreEqual(new[] { 1, 2, 3, 4, 5 }, result);
    }

    [TestMethod]
    public void ModifyAll_TransformsAllElements_ReturnsNewArray()
    {
        int[] source = new[] { 1, 2, 3 };
        int[] result = source.ModifyAll(x => x * 10);
        CollectionAssert.AreEqual(new[] { 10, 20, 30 }, result);
        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, source);
    }

    [TestMethod]
    public void ModifyWhere_TransformsMatchingElements_ReturnsNewArray()
    {
        int[] source = new[] { 1, 2, 3, 4, 5 };
        int[] result = source.ModifyWhere(x => x % 2 == 0, x => x * 10);
        CollectionAssert.AreEqual(new[] { 1, 20, 3, 40, 5 }, result);
    }

    [TestMethod]
    public void Remove_ExistingItem_RemovesFirstOccurrence()
    {
        int[] source = new[] { 1, 2, 3, 2, 4 };
        int[] result = source.Remove(2);
        CollectionAssert.AreEqual(new[] { 1, 3, 2, 4 }, result);
    }

    [TestMethod]
    public void Remove_NonExistentItem_ReturnsCopy()
    {
        int[] source = new[] { 1, 2, 3 };
        int[] result = source.Remove(99);
        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, result);
        Assert.AreNotSame(source, result);
    }

    [TestMethod]
    public void RemoveAll_MatchingPredicate_RemovesMatchingElements()
    {
        int[] source = new[] { 1, 2, 3, 4, 5, 6 };
        int[] result = source.RemoveAll(x => x % 2 == 0);
        CollectionAssert.AreEqual(new[] { 1, 3, 5 }, result);
    }

    [TestMethod]
    public void RemoveAll_NoMatch_ReturnsCopy()
    {
        int[] source = new[] { 1, 3, 5 };
        int[] result = source.RemoveAll(x => x % 2 == 0);
        CollectionAssert.AreEqual(new[] { 1, 3, 5 }, result);
    }

    [TestMethod]
    public void RemoveAt_FirstIndex_RemovesFirstElement()
    {
        int[] source = new[] { 10, 20, 30 };
        int[] result = source.RemoveAt(0);
        CollectionAssert.AreEqual(new[] { 20, 30 }, result);
    }

    [TestMethod]
    public void RemoveAt_InvalidIndex_ThrowsArgumentOutOfRangeException()
    {
        int[] source = new[] { 1, 2, 3 };
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => source.RemoveAt(5));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => source.RemoveAt(-1));
    }

    [TestMethod]
    public void RemoveAt_LastIndex_RemovesLastElement()
    {
        int[] source = new[] { 10, 20, 30 };
        int[] result = source.RemoveAt(2);
        CollectionAssert.AreEqual(new[] { 10, 20 }, result);
    }

    [TestMethod]
    public void RemoveAt_MiddleIndex_RemovesElement()
    {
        int[] source = new[] { 1, 2, 3, 4, 5 };
        int[] result = source.RemoveAt(2);
        CollectionAssert.AreEqual(new[] { 1, 2, 4, 5 }, result);
    }

    [TestMethod]
    public void SetAt_InvalidIndex_ThrowsArgumentOutOfRangeException()
    {
        int[] source = new[] { 1, 2, 3 };
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => source.SetAt(5, 99));
    }

    [TestMethod]
    public void SetAt_ValidIndex_ReplacesElement()
    {
        int[] source = new[] { 1, 2, 3 };
        int[] result = source.SetAt(1, 99);
        CollectionAssert.AreEqual(new[] { 1, 99, 3 }, result);
        Assert.AreEqual(2, source[1]);
    }

    [TestMethod]
    public void SetRange_RangeExceedsBounds_ThrowsArgumentOutOfRangeException()
    {
        int[] source = new[] { 1, 2, 3 };
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => source.SetRange(2, new[] { 10, 20, 30 }));
    }

    [TestMethod]
    public void SetRange_ValidRange_ReplacesElements()
    {
        int[] source = new[] { 1, 2, 3, 4, 5 };
        int[] result = source.SetRange(1, new[] { 20, 30 });
        CollectionAssert.AreEqual(new[] { 1, 20, 30, 4, 5 }, result);
    }
    #endregion
}
