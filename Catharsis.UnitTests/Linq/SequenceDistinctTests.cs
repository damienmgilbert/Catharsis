using Catharsis.Linq;

namespace Catharsis.UnitTests.Linq;

///<summary>
///Unit tests for the <see cref="SequenceDistinct"/> class.
///</summary>
[TestClass]
public class SequenceDistinctTests
{
    [TestMethod]
    public void CountByKey_CountsOccurrences()
    {
        Dictionary<int, int> result = new[] { 1, 2, 2, 3, 3, 3 }.CountByKey(static x => x);
        Assert.AreEqual(1, result[1]);
        Assert.AreEqual(2, result[2]);
        Assert.AreEqual(3, result[3]);
    }

    [TestMethod]
    public void CountByKey_NullSource_ThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(static () => ((IEnumerable<int>)null!).CountByKey(static x => x));
    }

    [TestMethod]
    public void DistinctByKey_ReturnsFirstOfEachKey()
    {
        List<(int Id, string Name)> source = [(1, "a"), (2, "b"), (1, "c"), (3, "d")];

        List<(int, string)> result = source.DistinctByKey(static x => x.Id).ToList();

        Assert.AreEqual(3, result.Count);
        Assert.AreEqual("a", result.First(static x => x.Item1 == 1).Item2);
    }

    [TestMethod]
    public void DistinctByKey_WithComparer_UsesComparer()
    {
        List<string> result = new[] { "abc", "ABC", "def" }
            .DistinctByKey(static x => x, StringComparer.OrdinalIgnoreCase)
            .ToList();
        Assert.AreEqual(2, result.Count);
    }

    [TestMethod]
    public void DistinctByKey_Ordered_ReturnsDistinctElements()
    {
        IOrderedEnumerable<int> ordered = new[] { 1, 1, 2, 3, 3 }.Order();
        List<int> result = ordered.DistinctByKey(static x => x).ToList();
        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, result);
    }

    [TestMethod]
    public void DistinctByKey_NullSource_ThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(static () => ((IEnumerable<int>)null!).DistinctByKey(static x => x).ToList());
    }

    [TestMethod]
    public void DistinctByKey_WithResolver_KeepsResolvedElement()
    {
        List<(int Id, int Value)> source = [(1, 10), (1, 20), (2, 5)];

        // Keep the one with higher Value
        List<(int, int)> result = source
            .DistinctByKey(static x => x.Id, static (existing, dup) => existing.Value >= dup.Value ? existing : dup)
            .ToList();

        Assert.AreEqual(2, result.Count);
        Assert.AreEqual(20, result.First(static x => x.Item1 == 1).Item2);
    }

    [TestMethod]
    public void DistinctByKeyPerGroup_DistinctWithinEachGroup()
    {
        IGrouping<string, int>[] groups =
        [
            SequenceFactory.Grouping("a", 1, 2, 1, 3),
            SequenceFactory.Grouping("b", 4, 4, 5)
        ];

        List<IGrouping<string, int>> result = groups.AsEnumerable()
            .DistinctByKeyPerGroup(static x => x)
            .ToList();

        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, result[0].ToList());
        CollectionAssert.AreEqual(new[] { 4, 5 }, result[1].ToList());
    }

    [TestMethod]
    public void DistinctByKeyPerGroup_NullSource_ThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(
            static () => ((IEnumerable<IGrouping<string, int>>)null!).DistinctByKeyPerGroup(static x => x).ToList());
    }
}
