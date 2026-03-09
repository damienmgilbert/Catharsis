using Catharsis.Linq;

namespace Catharsis.UnitTests.Linq;

///<summary>
///Unit tests for the <see cref="SequenceChunkMap"/> class.
///</summary>
[TestClass]
public class SequenceChunkMapTests
{
    [TestMethod]
    public void ChunkAggregate_ReducesEachChunkToSingleValue()
    {
        List<int> result = new[] { 1, 2, 3, 4, 5 }
            .ChunkAggregate(2, static chunk => chunk.Sum())
            .ToList();
        // Chunks: [1,2] -> 3, [3,4] -> 7, [5] -> 5
        CollectionAssert.AreEqual(new[] { 3, 7, 5 }, result);
    }

    [TestMethod]
    public void ChunkAggregate_NullSource_ThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(static () => ((IEnumerable<int>)null!).ChunkAggregate(2, static c => c.Count).ToList());
    }

    [TestMethod]
    public void ChunkAggregate_ChunkSizeLessThan1_ThrowsArgumentOutOfRangeException()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(static () => new[] { 1 }.ChunkAggregate(0, static c => c.Count).ToList());
    }

    [TestMethod]
    public void ChunkMap_MapsAndFlattensChunks()
    {
        List<string> result = new[] { 1, 2, 3, 4, 5 }
            .ChunkMap(3, static chunk => chunk.Select(static x => $"v{x}"))
            .ToList();
        CollectionAssert.AreEqual(new[] { "v1", "v2", "v3", "v4", "v5" }, result);
    }

    [TestMethod]
    public void ChunkMap_NullSource_ThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(static () => ((IEnumerable<int>)null!).ChunkMap(2, static c => c).ToList());
    }

    [TestMethod]
    public void ChunkMapIndexed_IncludesChunkIndex()
    {
        List<string> result = new[] { 10, 20, 30, 40 }
            .ChunkMapIndexed(2, static (idx, chunk) => new[] { $"chunk{idx}:{chunk.Count}" })
            .ToList();
        CollectionAssert.AreEqual(new[] { "chunk0:2", "chunk1:2" }, result);
    }

    [TestMethod]
    public void ChunkMapIndexed_NullSource_ThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(
            static () => ((IEnumerable<int>)null!).ChunkMapIndexed(2, static (i, c) => c).ToList());
    }

    [TestMethod]
    public void ChunkMapByKey_ChunksConsecutiveByKeyAndMaps()
    {
        List<string> result = new[] { 1, 1, 2, 2, 2, 1 }
            .ChunkMapByKey(static x => x, static (key, chunk) => new[] { $"{key}x{chunk.Count}" })
            .ToList();
        // Consecutive runs: [1,1] key=1, [2,2,2] key=2, [1] key=1
        CollectionAssert.AreEqual(new[] { "1x2", "2x3", "1x1" }, result);
    }

    [TestMethod]
    public void ChunkMapByKey_NullSource_ThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(
            static () => ((IEnumerable<int>)null!).ChunkMapByKey(static x => x, static (k, c) => c).ToList());
    }

    [TestMethod]
    public void ChunkMapGroups_ChunksWithinGroups()
    {
        IGrouping<string, int>[] groups =
        [
            SequenceFactory.Grouping("a", 1, 2, 3, 4, 5),
            SequenceFactory.Grouping("b", 10, 20)
        ];

        List<string> result = groups.AsEnumerable()
            .ChunkMapGroups(2, static (key, chunk) => new[] { $"{key}:{chunk.Count}" })
            .ToList();
        // Group "a": [1,2]->a:2, [3,4]->a:2, [5]->a:1; Group "b": [10,20]->b:2
        CollectionAssert.AreEqual(new[] { "a:2", "a:2", "a:1", "b:2" }, result);
    }

    [TestMethod]
    public void ChunkMapGroups_NullSource_ThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(
            static () => ((IEnumerable<IGrouping<string, int>>)null!).ChunkMapGroups(2, static (k, c) => c).ToList());
    }
}
