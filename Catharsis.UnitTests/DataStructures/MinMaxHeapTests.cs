using Catharsis.DataStructures;

namespace Catharsis.UnitTests.DataStructures;

///<summary>
///Unit tests for the <see cref="MinMaxHeap{T}"/> class.
///</summary>
[TestClass]
public class MinMaxHeapTests
{
    #region Construction

    [TestMethod]
    public void Constructor_Default_IsEmpty()
    {
        MinMaxHeap<int> heap = new();
        Assert.AreEqual(0, heap.Count);
    }

    [TestMethod]
    public void Constructor_FromItems_NullItems_Throws()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => new MinMaxHeap<int>((IEnumerable<int>)null!));
    }

    [TestMethod]
    public void Constructor_FromItems_PopulatesHeap()
    {
        MinMaxHeap<int> heap = new([5, 3, 8, 1, 9]);
        Assert.AreEqual(5, heap.Count);
        Assert.AreEqual(1, heap.PeekMin());
        Assert.AreEqual(9, heap.PeekMax());
    }

    [TestMethod]
    public void Constructor_CustomComparer_OrdersAccordingly()
    {
        MinMaxHeap<int> heap = new(Comparer<int>.Create(static (a, b) => b.CompareTo(a)));
        heap.Add(1);
        heap.Add(5);
        heap.Add(3);

        // With a reversed comparer, "min" and "max" swap roles.
        Assert.AreEqual(5, heap.PeekMin());
        Assert.AreEqual(1, heap.PeekMax());
    }

    #endregion

    #region Empty-heap behavior

    [TestMethod]
    public void PeekMin_Empty_Throws()
    {
        MinMaxHeap<int> heap = new();
        Assert.ThrowsExactly<InvalidOperationException>(() => heap.PeekMin());
    }

    [TestMethod]
    public void PeekMax_Empty_Throws()
    {
        MinMaxHeap<int> heap = new();
        Assert.ThrowsExactly<InvalidOperationException>(() => heap.PeekMax());
    }

    [TestMethod]
    public void ExtractMin_Empty_Throws()
    {
        MinMaxHeap<int> heap = new();
        Assert.ThrowsExactly<InvalidOperationException>(() => heap.ExtractMin());
    }

    [TestMethod]
    public void ExtractMax_Empty_Throws()
    {
        MinMaxHeap<int> heap = new();
        Assert.ThrowsExactly<InvalidOperationException>(() => heap.ExtractMax());
    }

    #endregion

    #region Single and double element behavior

    [TestMethod]
    public void SingleElement_MinAndMaxAreTheSame()
    {
        MinMaxHeap<int> heap = new();
        heap.Add(42);
        Assert.AreEqual(42, heap.PeekMin());
        Assert.AreEqual(42, heap.PeekMax());
    }

    [TestMethod]
    public void TwoElements_MinAndMaxAreCorrect()
    {
        MinMaxHeap<int> heap = new();
        heap.Add(5);
        heap.Add(1);
        Assert.AreEqual(1, heap.PeekMin());
        Assert.AreEqual(5, heap.PeekMax());
    }

    #endregion

    #region Basic Add / ExtractMin / ExtractMax

    [TestMethod]
    public void Add_IncrementsCount()
    {
        MinMaxHeap<int> heap = new();
        heap.Add(1);
        heap.Add(2);
        Assert.AreEqual(2, heap.Count);
    }

    [TestMethod]
    public void ExtractMin_DecrementsCountAndReturnsSmallest()
    {
        MinMaxHeap<int> heap = new([5, 3, 8, 1, 9, 2]);
        int result = heap.ExtractMin();
        Assert.AreEqual(1, result);
        Assert.AreEqual(5, heap.Count);
    }

    [TestMethod]
    public void ExtractMax_DecrementsCountAndReturnsLargest()
    {
        MinMaxHeap<int> heap = new([5, 3, 8, 1, 9, 2]);
        int result = heap.ExtractMax();
        Assert.AreEqual(9, result);
        Assert.AreEqual(5, heap.Count);
    }

    [TestMethod]
    public void ExtractMinRepeatedly_ProducesAscendingSequence()
    {
        int[] values = [5, 3, 8, 1, 9, 2, 7, 6, 4, 0];
        MinMaxHeap<int> heap = new(values);
        List<int> extracted = [];

        while(heap.Count > 0)
        {
            extracted.Add(heap.ExtractMin());
        }

        CollectionAssert.AreEqual(values.OrderBy(static v => v).ToList(), extracted);
    }

    [TestMethod]
    public void ExtractMaxRepeatedly_ProducesDescendingSequence()
    {
        int[] values = [5, 3, 8, 1, 9, 2, 7, 6, 4, 0];
        MinMaxHeap<int> heap = new(values);
        List<int> extracted = [];

        while(heap.Count > 0)
        {
            extracted.Add(heap.ExtractMax());
        }

        CollectionAssert.AreEqual(values.OrderByDescending(static v => v).ToList(), extracted);
    }

    [TestMethod]
    public void AlternatingExtractMinAndMax_MeetsInTheMiddleCorrectly()
    {
        int[] values = [.. Enumerable.Range(1, 20)];
        MinMaxHeap<int> heap = new(values);
        List<int> mins = [];
        List<int> maxes = [];

        while(heap.Count > 0)
        {
            mins.Add(heap.ExtractMin());
            if(heap.Count > 0)
            {
                maxes.Add(heap.ExtractMax());
            }
        }

        CollectionAssert.AreEqual(Enumerable.Range(1, 10).ToList(), mins);
        CollectionAssert.AreEqual(Enumerable.Range(11, 10).OrderByDescending(static v => v).ToList(), maxes);
    }

    #endregion

    #region Duplicates

    [TestMethod]
    public void DuplicateValues_AreAllPreservedAndExtractedCorrectly()
    {
        int[] values = [3, 3, 3, 1, 1, 5, 5, 5, 5];
        MinMaxHeap<int> heap = new(values);
        List<int> extracted = [];

        while(heap.Count > 0)
        {
            extracted.Add(heap.ExtractMin());
        }

        CollectionAssert.AreEqual(values.OrderBy(static v => v).ToList(), extracted);
    }

    #endregion

    #region Randomized stress test

    [TestMethod]
    public void RandomizedSequence_ExtractMinAndMaxMatchSortedReference()
    {
        Random random = new(12345);
        int[] values = new int[500];

        for(int i = 0; i < values.Length; i++)
        {
            values[i] = random.Next(-1000, 1000);
        }

        MinMaxHeap<int> minHeap = new(values);
        MinMaxHeap<int> maxHeap = new(values);

        List<int> ascending = [];
        while(minHeap.Count > 0)
        {
            ascending.Add(minHeap.ExtractMin());
        }

        List<int> descending = [];
        while(maxHeap.Count > 0)
        {
            descending.Add(maxHeap.ExtractMax());
        }

        CollectionAssert.AreEqual(values.OrderBy(static v => v).ToList(), ascending);
        CollectionAssert.AreEqual(values.OrderByDescending(static v => v).ToList(), descending);
    }

    [TestMethod]
    public void RandomizedMixedAddAndExtract_NeverViolatesMinMaxInvariant()
    {
        Random random = new(999);
        MinMaxHeap<int> heap = new();
        List<int> reference = [];

        for(int i = 0; i < 2000; i++)
        {
            if(reference.Count == 0 || random.Next(2) == 0)
            {
                int value = random.Next(-10_000, 10_000);
                heap.Add(value);
                reference.Add(value);
            } else if(random.Next(2) == 0)
            {
                Assert.AreEqual(reference.Min(), heap.ExtractMin());
                reference.Remove(reference.Min());
            } else
            {
                Assert.AreEqual(reference.Max(), heap.ExtractMax());
                reference.Remove(reference.Max());
            }

            Assert.AreEqual(reference.Count, heap.Count);
        }
    }

    #endregion

    #region Clear

    [TestMethod]
    public void Clear_RemovesAllItems()
    {
        MinMaxHeap<int> heap = new([1, 2, 3]);
        heap.Clear();
        Assert.AreEqual(0, heap.Count);
    }

    #endregion
}
