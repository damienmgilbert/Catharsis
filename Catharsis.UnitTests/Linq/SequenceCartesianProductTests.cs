using Catharsis.Linq;

namespace Catharsis.UnitTests.Linq;

///<summary>
///Unit tests for the <see cref="SequenceCartesianProduct"/> class.
///</summary>
[TestClass]
public class SequenceCartesianProductTests
{
    #region CartesianProduct

    [TestMethod]
    public void CartesianProduct_NullSequences_Throws()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => ((IEnumerable<IEnumerable<int>>)null!).CartesianProduct().ToList());
    }

    [TestMethod]
    public void CartesianProduct_NoSequences_ReturnsEmpty()
    {
        Assert.IsEmpty(Array.Empty<IEnumerable<int>>().CartesianProduct().ToList());
    }

    [TestMethod]
    public void CartesianProduct_OneSequenceIsEmpty_ReturnsEmpty()
    {
        int[][] sequences = [[1, 2], [], [3, 4]];
        Assert.IsEmpty(sequences.CartesianProduct().ToList());
    }

    [TestMethod]
    public void CartesianProduct_SingleSequence_ReturnsEachElementAsSingletonCombination()
    {
        int[][] sequences = [[1, 2, 3]];
        List<IReadOnlyList<int>> result = [.. sequences.CartesianProduct()];

        Assert.HasCount(3, result);
        CollectionAssert.AreEqual(new[] { 1 }, result[0].ToList());
        CollectionAssert.AreEqual(new[] { 2 }, result[1].ToList());
        CollectionAssert.AreEqual(new[] { 3 }, result[2].ToList());
    }

    [TestMethod]
    public void CartesianProduct_TwoSequences_ReturnsAllCombinationsInOrder()
    {
        int[][] sequences = [[1, 2], [10, 20]];
        List<List<int>> result = [.. sequences.CartesianProduct().Select(static c => c.ToList())];

        List<List<int>> expected =
        [
            [1, 10],
            [1, 20],
            [2, 10],
            [2, 20],
        ];

        Assert.HasCount(4, result);

        for(int i = 0; i < expected.Count; i++)
        {
            CollectionAssert.AreEqual(expected[i], result[i]);
        }
    }

    [TestMethod]
    public void CartesianProduct_ThreeSequences_ProducesCorrectCount()
    {
        int[][] sequences = [[1, 2], [10, 20, 30], [100, 200]];
        List<IReadOnlyList<int>> result = [.. sequences.CartesianProduct()];

        Assert.HasCount(2 * 3 * 2, result);
        CollectionAssert.AreEqual(new[] { 1, 10, 100 }, result[0].ToList());
        CollectionAssert.AreEqual(new[] { 2, 30, 200 }, result[^1].ToList());
    }

    #endregion
}
