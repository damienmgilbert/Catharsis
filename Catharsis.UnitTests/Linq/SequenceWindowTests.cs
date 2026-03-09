using Catharsis.Linq;

namespace Catharsis.UnitTests.Linq;

///<summary>
///Unit tests for the <see cref="SequenceWindow"/> class.
///</summary>
[TestClass]
public class SequenceWindowTests
{
    [TestMethod]
    public void Buffer_SplitsIntoFixedSizeChunks()
    {
        List<List<int>> result = new[] { 1, 2, 3, 4, 5 }.Buffer(2).ToList();
        Assert.AreEqual(3, result.Count);
        CollectionAssert.AreEqual(new[] { 1, 2 }, result[0]);
        CollectionAssert.AreEqual(new[] { 3, 4 }, result[1]);
        CollectionAssert.AreEqual(new[] { 5 }, result[2]);
    }

    [TestMethod]
    public void Buffer_NullSource_ThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(static () => ((IEnumerable<int>)null!).Buffer(2).ToList());
    }

    [TestMethod]
    public void Buffer_SizeLessThan1_ThrowsArgumentOutOfRangeException()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(static () => new[] { 1 }.Buffer(0).ToList());
    }

    [TestMethod]
    public void BufferBy_GroupsConsecutiveByKey()
    {
        List<IGrouping<bool, int>> result = new[] { 2, 4, 1, 3, 6 }
            .BufferBy(static x => x % 2 == 0)
            .ToList();
        Assert.AreEqual(3, result.Count);
        Assert.IsTrue(result[0].Key);  // 2, 4
        Assert.IsFalse(result[1].Key); // 1, 3
        Assert.IsTrue(result[2].Key);  // 6
    }

    [TestMethod]
    public void BufferBy_NullSource_ThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(static () => ((IEnumerable<int>)null!).BufferBy(static x => x).ToList());
    }

    [TestMethod]
    public void Pairwise_Tuple_ProducesConsecutivePairs()
    {
        List<(int, int)> result = new[] { 1, 2, 3, 4 }.Pairwise().ToList();
        Assert.AreEqual(3, result.Count);
        Assert.AreEqual((1, 2), result[0]);
        Assert.AreEqual((2, 3), result[1]);
        Assert.AreEqual((3, 4), result[2]);
    }

    [TestMethod]
    public void Pairwise_SingleElement_ReturnsEmpty()
    {
        Assert.AreEqual(0, new[] { 1 }.Pairwise().Count());
    }

    [TestMethod]
    public void Pairwise_Selector_ProjectsPairs()
    {
        List<int> result = new[] { 1, 2, 3 }.Pairwise(static (a, b) => a + b).ToList();
        CollectionAssert.AreEqual(new[] { 3, 5 }, result);
    }

    [TestMethod]
    public void Pairwise_NullSource_ThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(static () => ((IEnumerable<int>)null!).Pairwise().ToList());
    }

    [TestMethod]
    public void Sliding_ProducesSlidingWindows()
    {
        List<IReadOnlyList<int>> result = new[] { 1, 2, 3, 4, 5 }.Sliding(3).ToList();
        Assert.AreEqual(3, result.Count);
        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, result[0].ToList());
        CollectionAssert.AreEqual(new[] { 2, 3, 4 }, result[1].ToList());
        CollectionAssert.AreEqual(new[] { 3, 4, 5 }, result[2].ToList());
    }

    [TestMethod]
    public void Sliding_SizeLessThan1_ThrowsArgumentOutOfRangeException()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(static () => new[] { 1 }.Sliding(0).ToList());
    }

    [TestMethod]
    public void Sliding_WithStep_AdvancesByStep()
    {
        List<IReadOnlyList<int>> result = new[] { 1, 2, 3, 4, 5, 6 }.Sliding(2, 3).ToList();
        Assert.AreEqual(2, result.Count);
        CollectionAssert.AreEqual(new[] { 1, 2 }, result[0].ToList());
        CollectionAssert.AreEqual(new[] { 4, 5 }, result[1].ToList());
    }

    [TestMethod]
    public void Tumbling_ProducesNonOverlappingWindows()
    {
        List<IReadOnlyList<int>> result = new[] { 1, 2, 3, 4, 5 }.Tumbling(2).ToList();
        Assert.AreEqual(3, result.Count);
        CollectionAssert.AreEqual(new[] { 1, 2 }, result[0].ToList());
        CollectionAssert.AreEqual(new[] { 3, 4 }, result[1].ToList());
        CollectionAssert.AreEqual(new[] { 5 }, result[2].ToList());
    }

    [TestMethod]
    public void Tumbling_SizeLessThan1_ThrowsArgumentOutOfRangeException()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(static () => new[] { 1 }.Tumbling(0).ToList());
    }

    [TestMethod]
    public void SlidingPerGroup_AppliesWindowPerGroup()
    {
        IGrouping<string, int>[] groups =
        [
            SequenceFactory.Grouping("a", 1, 2, 3),
            SequenceFactory.Grouping("b", 10, 20)
        ];

        List<IGrouping<string, IReadOnlyList<int>>> result = groups.AsEnumerable().SlidingPerGroup(2).ToList();
        Assert.AreEqual(2, result.Count);
        Assert.AreEqual("a", result[0].Key);
        Assert.AreEqual(2, result[0].Count()); // windows: [1,2], [2,3]
    }

    [TestMethod]
    public void Tumbling_NullSource_ThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(static () => ((IEnumerable<int>)null!).Tumbling(2).ToList());
    }
}
