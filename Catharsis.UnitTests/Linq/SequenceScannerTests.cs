using Catharsis.Linq;

namespace Catharsis.UnitTests.Linq;

[TestClass]
public class SequenceScannerTests
{
    [TestMethod]
    public void PairwiseIndexed_ReturnsIndexedPairs()
    {
        List<(int Index, int Previous, int Current)> result = new[] { 10, 20, 30 }.PairwiseIndexed().ToList();
        Assert.AreEqual(2, result.Count);
        Assert.AreEqual((0, 10, 20), result[0]);
        Assert.AreEqual((1, 20, 30), result[1]);
    }

    [TestMethod]
    public void PairwiseIndexed_SingleElement_ReturnsEmpty()
    {
        Assert.AreEqual(0, new[] { 1 }.PairwiseIndexed().Count());
    }

    [TestMethod]
    public void PairwiseIndexed_NullSource_ThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => ((IEnumerable<int>)null!).PairwiseIndexed().ToList());
    }

    [TestMethod]
    public void PairwiseWhere_FiltersConsecutivePairs()
    {
        List<(int, int)> result = new[] { 1, 5, 3, 8, 2 }
            .PairwiseWhere((prev, curr) => curr > prev)
            .ToList();
        // Pairs where current > previous: (1,5), (3,8)
        Assert.AreEqual(2, result.Count);
        Assert.AreEqual((1, 5), result[0]);
        Assert.AreEqual((3, 8), result[1]);
    }

    [TestMethod]
    public void PairwiseWhere_NullSource_ThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => ((IEnumerable<int>)null!).PairwiseWhere((a, b) => true).ToList());
    }

    [TestMethod]
    public void ScanSeedless_ProducesRunningAggregatesFromFirstElement()
    {
        List<int> result = new[] { 1, 2, 3, 4 }.ScanSeedless((acc, x) => acc + x).ToList();
        CollectionAssert.AreEqual(new[] { 1, 3, 6, 10 }, result);
    }

    [TestMethod]
    public void ScanSeedless_EmptySource_ThrowsInvalidOperationException()
    {
        Assert.ThrowsExactly<InvalidOperationException>(() => Array.Empty<int>().ScanSeedless((a, b) => a + b).ToList());
    }

    [TestMethod]
    public void ScanSeedless_NullSource_ThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => ((IEnumerable<int>)null!).ScanSeedless((a, b) => a + b).ToList());
    }

    [TestMethod]
    public void ScanSelect_ProjectsIntermediateResults()
    {
        List<string> result = new[] { 1, 2, 3 }
            .ScanSelect(0, (acc, x) => acc + x, acc => $"sum={acc}")
            .ToList();
        CollectionAssert.AreEqual(new[] { "sum=0", "sum=1", "sum=3", "sum=6" }, result);
    }

    [TestMethod]
    public void ScanSelect_NullSource_ThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => ((IEnumerable<int>)null!).ScanSelect(0, (a, b) => a + b, x => x).ToList());
    }

    [TestMethod]
    public void ScanWhile_StopsWhenPredicateFails()
    {
        List<int> result = new[] { 1, 2, 3, 4, 5 }
            .ScanWhile(0, (acc, x) => acc + x, acc => acc < 7)
            .ToList();
        // 0, 1, 3, 6 (next would be 10, fails predicate)
        CollectionAssert.AreEqual(new[] { 0, 1, 3, 6 }, result);
    }

    [TestMethod]
    public void ScanWhile_NullSource_ThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => ((IEnumerable<int>)null!).ScanWhile(0, (a, b) => a + b, _ => true).ToList());
    }

    [TestMethod]
    public void Triplewise_ProducesOverlappingTriples()
    {
        List<(int, int, int)> result = new[] { 1, 2, 3, 4, 5 }.Triplewise().ToList();
        Assert.AreEqual(3, result.Count);
        Assert.AreEqual((1, 2, 3), result[0]);
        Assert.AreEqual((2, 3, 4), result[1]);
        Assert.AreEqual((3, 4, 5), result[2]);
    }

    [TestMethod]
    public void Triplewise_LessThanThreeElements_ReturnsEmpty()
    {
        Assert.AreEqual(0, new[] { 1, 2 }.Triplewise().Count());
    }

    [TestMethod]
    public void Triplewise_NullSource_ThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => ((IEnumerable<int>)null!).Triplewise().ToList());
    }
}
