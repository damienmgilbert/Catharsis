using Catharsis.Linq;

namespace Catharsis.UnitTests.Linq;

[TestClass]
public class SequencePartitionTests
{
    [TestMethod]
    public void ChunkBy_GroupsConsecutiveByKey()
    {
        List<IGrouping<char, string>> result = new[] { "apple", "avocado", "banana", "blueberry", "cherry" }
            .ChunkBy(s => s[0])
            .ToList();
        Assert.AreEqual(3, result.Count);
        Assert.AreEqual('a', result[0].Key);
        Assert.AreEqual(2, result[0].Count());
        Assert.AreEqual('b', result[1].Key);
        Assert.AreEqual('c', result[2].Key);
    }

    [TestMethod]
    public void ChunkBy_NullSource_ThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => ((IEnumerable<int>)null!).ChunkBy(x => x).ToList());
    }

    [TestMethod]
    public void Partition_SplitsByPredicate()
    {
        (List<int> matched, List<int> unmatched) = new[] { 1, 2, 3, 4, 5 }.Partition(x => x % 2 == 0);
        CollectionAssert.AreEqual(new[] { 2, 4 }, matched);
        CollectionAssert.AreEqual(new[] { 1, 3, 5 }, unmatched);
    }

    [TestMethod]
    public void Partition_NullSource_ThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => ((IEnumerable<int>)null!).Partition(x => true));
    }

    [TestMethod]
    public void PartitionEvenly_DistributesRoundRobin()
    {
        List<List<int>> result = new[] { 1, 2, 3, 4, 5 }.PartitionEvenly(3);
        Assert.AreEqual(3, result.Count);
        CollectionAssert.AreEqual(new[] { 1, 4 }, result[0]);
        CollectionAssert.AreEqual(new[] { 2, 5 }, result[1]);
        CollectionAssert.AreEqual(new[] { 3 }, result[2]);
    }

    [TestMethod]
    public void PartitionEvenly_GroupCountLessThan1_ThrowsArgumentOutOfRangeException()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new[] { 1 }.PartitionEvenly(0));
    }

    [TestMethod]
    public void PartitionGroups_SplitsGroupsByKeyPredicate()
    {
        IGrouping<int, string>[] groups =
        [
            SequenceFactory.Grouping(1, "a"),
            SequenceFactory.Grouping(2, "b"),
            SequenceFactory.Grouping(3, "c")
        ];

        var (matched, unmatched) = groups.AsEnumerable().PartitionGroups(k => k % 2 == 0);
        Assert.AreEqual(1, matched.Count);
        Assert.AreEqual(2, matched[0].Key);
        Assert.AreEqual(2, unmatched.Count);
    }

    [TestMethod]
    public void Span_SplitsAtPredicateFailure()
    {
        (List<int> prefix, List<int> suffix) = new[] { 1, 2, 3, 4, 5 }.Span(x => x < 4);
        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, prefix);
        CollectionAssert.AreEqual(new[] { 4, 5 }, suffix);
    }

    [TestMethod]
    public void SplitAt_SplitsAtIndex()
    {
        (List<int> before, List<int> after) = new[] { 10, 20, 30, 40, 50 }.SplitAt(2);
        CollectionAssert.AreEqual(new[] { 10, 20 }, before);
        CollectionAssert.AreEqual(new[] { 30, 40, 50 }, after);
    }

    [TestMethod]
    public void SplitAt_NegativeIndex_ThrowsArgumentOutOfRangeException()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new[] { 1 }.SplitAt(-1));
    }

    [TestMethod]
    public void SplitBy_SplitsAtSeparator()
    {
        List<IReadOnlyList<int>> result = new[] { 1, 2, 0, 3, 4, 0, 5 }.SplitBy(0).ToList();
        Assert.AreEqual(3, result.Count);
        CollectionAssert.AreEqual(new[] { 1, 2 }, result[0].ToList());
        CollectionAssert.AreEqual(new[] { 3, 4 }, result[1].ToList());
        CollectionAssert.AreEqual(new[] { 5 }, result[2].ToList());
    }

    [TestMethod]
    public void SplitBy_NullSource_ThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => ((IEnumerable<int>)null!).SplitBy(0).ToList());
    }

    [TestMethod]
    public void SplitWhen_SplitsAtPredicateTrigger()
    {
        List<IReadOnlyList<int>> result = new[] { 1, 2, 10, 3, 20, 4 }.SplitWhen(x => x >= 10).ToList();
        Assert.AreEqual(3, result.Count);
        CollectionAssert.AreEqual(new[] { 1, 2 }, result[0].ToList());
        CollectionAssert.AreEqual(new[] { 10, 3 }, result[1].ToList());
        CollectionAssert.AreEqual(new[] { 20, 4 }, result[2].ToList());
    }

    [TestMethod]
    public void SplitWhen_NullSource_ThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => ((IEnumerable<int>)null!).SplitWhen(x => true).ToList());
    }
}
