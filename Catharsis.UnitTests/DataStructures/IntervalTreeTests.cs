using Catharsis.DataStructures;

namespace Catharsis.UnitTests.DataStructures;

///<summary>
///Unit tests for the <see cref="IntervalTree{T}"/> class.
///</summary>
[TestClass]
public class IntervalTreeTests
{
    #region Add / Count

    [TestMethod]
    public void Count_StartsAtZero()
    {
        IntervalTree<int> tree = new();
        Assert.AreEqual(0, tree.Count);
    }

    [TestMethod]
    public void Add_IncrementsCount()
    {
        IntervalTree<int> tree = new();
        tree.Add(new Interval<int>(1, 5));
        tree.Add(new Interval<int>(10, 15));
        Assert.AreEqual(2, tree.Count);
    }

    #endregion

    #region FindOverlapping

    [TestMethod]
    public void FindOverlapping_EmptyTree_ReturnsEmpty()
    {
        IntervalTree<int> tree = new();
        Assert.IsEmpty(tree.FindOverlapping(new Interval<int>(0, 10)));
    }

    [TestMethod]
    public void FindOverlapping_NoOverlaps_ReturnsEmpty()
    {
        IntervalTree<int> tree = new();
        tree.Add(new Interval<int>(1, 5));
        tree.Add(new Interval<int>(20, 25));

        Assert.IsEmpty(tree.FindOverlapping(new Interval<int>(10, 15)));
    }

    [TestMethod]
    public void FindOverlapping_SingleOverlap_ReturnsIt()
    {
        IntervalTree<int> tree = new();
        Interval<int> target = new(5, 10);
        tree.Add(target);
        tree.Add(new Interval<int>(20, 25));

        IReadOnlyList<Interval<int>> results = tree.FindOverlapping(new Interval<int>(8, 12));

        Assert.HasCount(1, results);
        Assert.AreEqual(target, results[0]);
    }

    [TestMethod]
    public void FindOverlapping_MultipleOverlaps_ReturnsAllOfThem()
    {
        IntervalTree<int> tree = new();
        tree.Add(new Interval<int>(1, 5));
        tree.Add(new Interval<int>(4, 8));
        tree.Add(new Interval<int>(20, 25));
        tree.Add(new Interval<int>(6, 10));

        IReadOnlyList<Interval<int>> results = tree.FindOverlapping(new Interval<int>(4, 6));

        Assert.HasCount(3, results);
        CollectionAssert.Contains(results.ToList(), new Interval<int>(1, 5));
        CollectionAssert.Contains(results.ToList(), new Interval<int>(4, 8));
        CollectionAssert.Contains(results.ToList(), new Interval<int>(6, 10));
    }

    [TestMethod]
    public void FindOverlapping_TouchingBoundary_CountsAsOverlap()
    {
        IntervalTree<int> tree = new();
        tree.Add(new Interval<int>(1, 5));

        IReadOnlyList<Interval<int>> results = tree.FindOverlapping(new Interval<int>(5, 10));

        Assert.HasCount(1, results);
    }

    [TestMethod]
    public void FindOverlapping_ManyIntervalsInAscendingOrder_FindsCorrectSubset()
    {
        IntervalTree<int> tree = new();

        for(int i = 0; i < 100; i += 10)
        {
            tree.Add(new Interval<int>(i, i + 5));
        }

        IReadOnlyList<Interval<int>> results = tree.FindOverlapping(new Interval<int>(52, 58));

        Assert.HasCount(1, results);
        Assert.AreEqual(new Interval<int>(50, 55), results[0]);
    }

    #endregion

    #region Clear

    [TestMethod]
    public void Clear_RemovesAllIntervals()
    {
        IntervalTree<int> tree = new();
        tree.Add(new Interval<int>(1, 5));
        tree.Clear();

        Assert.AreEqual(0, tree.Count);
        Assert.IsEmpty(tree.FindOverlapping(new Interval<int>(0, 10)));
    }

    #endregion
}
