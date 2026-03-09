using Catharsis.Linq;

namespace Catharsis.UnitTests.Linq;

[TestClass]
public class RangePartitionerTests
{
    [TestMethod]
    public void PartitionByRange_Int_PartitionsIntoRanges()
    {
        ILookup<int, int> lookup = new[] { 1, 5, 12, 15, 23 }
            .PartitionByRange(x => x, 10);

        CollectionAssert.AreEqual(new[] { 1, 5 }, lookup[0].ToList());
        CollectionAssert.AreEqual(new[] { 12, 15 }, lookup[10].ToList());
        CollectionAssert.AreEqual(new[] { 23 }, lookup[20].ToList());
    }

    [TestMethod]
    public void PartitionByRange_Int_NullSource_ThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(
            () => ((IEnumerable<int>)null!).PartitionByRange(x => x, 10));
    }

    [TestMethod]
    public void PartitionByRange_Int_RangeSizeLessThan1_ThrowsArgumentOutOfRangeException()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            () => new[] { 1 }.PartitionByRange(x => x, 0));
    }

    [TestMethod]
    public void PartitionByRange_Double_PartitionsIntoRanges()
    {
        ILookup<double, double> lookup = new[] { 0.5, 1.5, 2.5, 3.5 }
            .PartitionByRange(x => x, 2.0);

        CollectionAssert.AreEqual(new[] { 0.5, 1.5 }, lookup[0.0].ToList());
        CollectionAssert.AreEqual(new[] { 2.5, 3.5 }, lookup[2.0].ToList());
    }

    [TestMethod]
    public void PartitionByRangeAsGroupings_ReturnsGroupings()
    {
        List<IGrouping<int, int>> groups = new[] { 1, 5, 12, 15 }
            .PartitionByRangeAsGroupings(x => x, 10)
            .ToList();

        Assert.IsTrue(groups.Count >= 2);
    }

    [TestMethod]
    public void PartitionByBoundaries_AssignsBucketsCorrectly()
    {
        int[] boundaries = [0, 10, 20, 50];
        ILookup<int, int> lookup = new[] { 3, 15, 25, 55 }
            .PartitionByBoundaries(x => x, boundaries);

        CollectionAssert.AreEqual(new[] { 3 }, lookup[0].ToList());
        CollectionAssert.AreEqual(new[] { 15 }, lookup[10].ToList());
        CollectionAssert.AreEqual(new[] { 25 }, lookup[20].ToList());
        CollectionAssert.AreEqual(new[] { 55 }, lookup[50].ToList());
    }

    [TestMethod]
    public void PartitionByBoundaries_EmptyBoundaries_ThrowsArgumentException()
    {
        Assert.ThrowsExactly<ArgumentException>(
            () => new[] { 1 }.PartitionByBoundaries(x => x, Array.Empty<int>()));
    }

    [TestMethod]
    public void PartitionByQuantile_DistributesBuckets()
    {
        ILookup<int, int> lookup = Enumerable.Range(1, 10)
            .PartitionByQuantile(x => x, 3);

        Assert.IsTrue(lookup.Count >= 2);
    }

    [TestMethod]
    public void PartitionByQuantileAsGroupings_DistributesBuckets()
    {
        List<IGrouping<int, int>> groups = Enumerable.Range(1, 9)
            .PartitionByQuantileAsGroupings(x => x, 3)
            .ToList();

        Assert.AreEqual(3, groups.Count);
        Assert.AreEqual(3, groups[0].Count());
    }

    [TestMethod]
    public void PartitionToLookup_SplitsByPredicate()
    {
        ILookup<bool, int> lookup = new[] { 1, 2, 3, 4, 5 }
            .PartitionToLookup(x => x % 2 == 0);

        CollectionAssert.AreEqual(new[] { 2, 4 }, lookup[true].ToList());
        CollectionAssert.AreEqual(new[] { 1, 3, 5 }, lookup[false].ToList());
    }
}
