using Catharsis.Linq;

namespace Catharsis.UnitTests.Linq;

///<summary>
///Unit tests for the <see cref="SequenceComposer"/> class.
///</summary>
[TestClass]
public class SequenceComposerTests
{
    [TestMethod]
    public void AppendMany_AppendsElements()
    {
        CollectionAssert.AreEqual(new[] { 1, 2, 3, 4 }, new[] { 1, 2 }.AppendMany(3, 4).ToList());
    }

    [TestMethod]
    public void AppendMany_NullSource_ThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(static () => ((IEnumerable<int>)null!).AppendMany(1).ToList());
    }

    [TestMethod]
    public void CartesianProduct_ProducesAllPairs()
    {
        List<(int, string)> result = new[] { 1, 2 }.CartesianProduct(new[] { "a", "b" }, static (x, y) => (x, y)).ToList();
        Assert.AreEqual(4, result.Count);
        Assert.IsTrue(result.Contains((1, "a")));
        Assert.IsTrue(result.Contains((2, "b")));
    }

    [TestMethod]
    public void CartesianProduct_NullSource_ThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(static () => ((IEnumerable<int>)null!).CartesianProduct(new[] { 1 }, static (a, b) => a + b).ToList());
    }

    [TestMethod]
    public void ConcatWith_ConcatenatesTwoSequences()
    {
        CollectionAssert.AreEqual(new[] { 1, 2, 3, 4 }, new[] { 1, 2 }.ConcatWith([3, 4]).ToList());
    }

    [TestMethod]
    public void ConcatWith_NullSource_ThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(static () => ((IEnumerable<int>)null!).ConcatWith([1]).ToList());
    }

    [TestMethod]
    public void Interleave_AlternatesElements()
    {
        CollectionAssert.AreEqual(new[] { 1, 4, 2, 5, 3, 6 }, new[] { 1, 2, 3 }.Interleave([4, 5, 6]).ToList());
    }

    [TestMethod]
    public void Interleave_UnequalLengths_AppendsRemainder()
    {
        CollectionAssert.AreEqual(new[] { 1, 3, 2, 4, 5 }, new[] { 1, 2 }.Interleave([3, 4, 5]).ToList());
    }

    [TestMethod]
    public void Interleave_NullSource_ThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(static () => ((IEnumerable<int>)null!).Interleave([1]).ToList());
    }

    [TestMethod]
    public void InterleaveMany_InterleavesMultipleSequences()
    {
        // With reverse-indexed round-robin, the interleave goes from end to start
        List<int> result = new[] { 1, 2 }.InterleaveMany([10, 20], [100, 200]).ToList();
        Assert.AreEqual(6, result.Count);
    }

    [TestMethod]
    public void MergeOrdered_MergesTwoSortedSequences()
    {
        CollectionAssert.AreEqual(
            new[] { 1, 2, 3, 4, 5, 6 },
            new[] { 1, 3, 5 }.MergeOrdered([2, 4, 6]).ToList());
    }

    [TestMethod]
    public void MergeOrdered_NullSource_ThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(static () => ((IEnumerable<int>)null!).MergeOrdered([1]).ToList());
    }

    [TestMethod]
    public void MergeGroupings_CombinesGroupsByKey()
    {
        IGrouping<string, int>[] g1 = [SequenceFactory.Grouping("a", 1, 2)];
        IGrouping<string, int>[] g2 = [SequenceFactory.Grouping("a", 3)];

        List<IGrouping<string, int>> result = g1.AsEnumerable().MergeGroupings(g2).ToList();
        Assert.AreEqual(1, result.Count);
        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, result[0].ToList());
    }

    [TestMethod]
    public void PrependMany_PrependsElements()
    {
        CollectionAssert.AreEqual(new[] { 0, 1, 2 }, new[] { 2 }.PrependMany(0, 1).ToList());
    }

    [TestMethod]
    public void Scan_ProducesRunningAggregates()
    {
        List<int> result = new[] { 1, 2, 3 }.Scan(0, static (acc, x) => acc + x).ToList();
        CollectionAssert.AreEqual(new[] { 0, 1, 3, 6 }, result);
    }

    [TestMethod]
    public void Scan_NullSource_ThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(static () => ((IEnumerable<int>)null!).Scan(0, static (a, b) => a + b).ToList());
    }

    [TestMethod]
    public void ZipLongest_PadsShorterSequence()
    {
        var result = new[] { 1, 2, 3 }.ZipLongest(new[] { "a" }).ToList();
        Assert.AreEqual(3, result.Count);
        Assert.AreEqual(1, result[0].First);
        Assert.AreEqual("a", result[0].Second);
        Assert.AreEqual(2, result[1].First);
        Assert.IsNull(result[1].Second);
    }

    [TestMethod]
    public void ZipWith_ZipsToShorterLength()
    {
        List<(int, string)> result = new[] { 1, 2, 3 }.ZipWith(new[] { "a", "b" }).ToList();
        Assert.AreEqual(2, result.Count);
        Assert.AreEqual((1, "a"), result[0]);
        Assert.AreEqual((2, "b"), result[1]);
    }

    [TestMethod]
    public void ZipWith_Selector_CombinesElements()
    {
        List<string> result = new[] { 1, 2 }.ZipWith(new[] { "a", "b" }, static (x, y) => $"{x}{y}").ToList();
        CollectionAssert.AreEqual(new[] { "1a", "2b" }, result);
    }

    [TestMethod]
    public void AppendToGroup_AddsElementsToExistingGroup()
    {
        IGrouping<string, int>[] source = [SequenceFactory.Grouping("g", 1, 2)];
        List<IGrouping<string, int>> result = source.AsEnumerable().AppendToGroup("g", new[] { 3 }).ToList();
        Assert.AreEqual(1, result.Count);
        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, result[0].ToList());
    }
}
