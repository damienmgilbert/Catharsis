using Catharsis.Randomization;

namespace Catharsis.UnitTests.Randomization;

///<summary>
///Unit tests for the <see cref="ShuffleExtensions"/> class.
///</summary>
[TestClass]
public class ShuffleExtensionsTests
{
    #region IList<T>.Shuffle

    [TestMethod]
    public void Shuffle_List_NullList_Throws()
    {
        List<int> list = null!;
        Assert.ThrowsExactly<ArgumentNullException>(() => list.Shuffle());
    }

    [TestMethod]
    public void Shuffle_List_PreservesAllElements()
    {
        List<int> list = [1, 2, 3, 4, 5];
        list.Shuffle(new Random(1));
        CollectionAssert.AreEquivalent(new[] { 1, 2, 3, 4, 5 }, list);
    }

    [TestMethod]
    public void Shuffle_List_EmptyList_DoesNotThrow()
    {
        List<int> list = [];
        list.Shuffle();
        Assert.IsEmpty(list);
    }

    [TestMethod]
    public void Shuffle_List_SingleElement_Unchanged()
    {
        List<int> list = [42];
        list.Shuffle();
        Assert.HasCount(1, list);
        Assert.AreEqual(42, list[0]);
    }

    [TestMethod]
    public void Shuffle_List_ChangesOrderForNonTrivialInput()
    {
        List<int> original = Enumerable.Range(0, 50).ToList();
        List<int> shuffled = [.. original];

        shuffled.Shuffle(new Random(1));

        CollectionAssert.AreNotEqual(original, shuffled);
        CollectionAssert.AreEquivalent(original, shuffled);
    }

    #endregion

    #region Span<T>.Shuffle

    [TestMethod]
    public void Shuffle_Span_PreservesAllElements()
    {
        int[] array = [1, 2, 3, 4, 5];
        array.AsSpan().Shuffle(new Random(1));
        CollectionAssert.AreEquivalent(new[] { 1, 2, 3, 4, 5 }, array);
    }

    [TestMethod]
    public void Shuffle_Span_EmptySpan_DoesNotThrow()
    {
        Span<int> span = [];
        span.Shuffle();
    }

    #endregion
}
