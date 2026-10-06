using Catharsis.Patterns.Composed;

namespace Catharsis.UnitTests.Patterns.Composed;

///<summary>
///Unit tests for <see cref="WindowedIterator"/>.
///</summary>
[TestClass]
public class WindowedIteratorTests
{
    static string Render(IEnumerable<IReadOnlyList<int>> windows) => string.Join("|", windows.Select(static w => string.Join(",", w)));

    [TestMethod]
    public void Windows_StepOne_SlidesOverlapping() { Assert.AreEqual("1,2,3|2,3,4|3,4,5", Render(WindowedIterator.Windows(Enumerable.Range(1, 5), 3))); }

    [TestMethod]
    public void Windows_StepEqualsSize_ChunksWithoutOverlap() { Assert.AreEqual("1,2|3,4", Render(WindowedIterator.Windows(Enumerable.Range(1, 5), 2, 2))); }

    [TestMethod]
    public void Windows_IncludePartial_AddsTrailingShortWindow() { Assert.AreEqual("1,2|3,4|5", Render(WindowedIterator.Windows(Enumerable.Range(1, 5), 2, 2, includePartial: true))); }

    [TestMethod]
    public void Windows_IncludePartial_WithSlidingStep_AddsShrinkingTail() { Assert.AreEqual("1,2,3|2,3,4|3,4|4", Render(WindowedIterator.Windows(Enumerable.Range(1, 4), 3, 1, includePartial: true))); }

    [TestMethod]
    public void Windows_StepLargerThanSize_SkipsBetweenWindows() { Assert.AreEqual("1,2|5,6", Render(WindowedIterator.Windows(Enumerable.Range(1, 7), 2, 4))); }

    [TestMethod]
    public void Windows_SourceShorterThanSize_IsEmptyUnlessPartialRequested()
    {
        Assert.AreEqual(string.Empty, Render(WindowedIterator.Windows([1, 2], 3)));
        Assert.AreEqual("1,2", Render(WindowedIterator.Windows([1, 2], 3, 1, includePartial: true).Take(1)));
    }

    [TestMethod]
    public void Windows_EmptySource_IsEmpty() { Assert.AreEqual(0, WindowedIterator.Windows(Array.Empty<int>(), 2, 1, includePartial: true).Count()); }

    [TestMethod]
    public void Windows_ReturnedListsAreIndependentCopies()
    {
        List<IReadOnlyList<int>> windows = [.. WindowedIterator.Windows([1, 2, 3], 2)];

        Assert.AreEqual(2, windows.Count);
        Assert.AreEqual(2, windows[0][1]);
        Assert.AreEqual(2, windows[1][0]);
    }

    [TestMethod]
    public void Windows_IsLazy_ReadsOnlyWhatIsNeeded()
    {
        int read = 0;

        IEnumerable<int> Source()
        {
            for (int i = 1; i <= 100; i++)
            {
                read++;
                yield return i;
            }
        }

        _ = WindowedIterator.Windows(Source(), 3).First();

        Assert.AreEqual(3, read);
    }

    [TestMethod]
    public void Windows_InvalidArguments_ThrowEagerly()
    {
        Assert.ThrowsExactly<ArgumentNullException>(static () => WindowedIterator.Windows<int>(null!, 2));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(static () => WindowedIterator.Windows([1], 0));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(static () => WindowedIterator.Windows([1], 1, 0));
    }

    [TestMethod]
    public void Pairwise_PairsAdjacentElements() { CollectionAssert.AreEqual(new[] { (1, 2), (2, 3) }, WindowedIterator.Pairwise([1, 2, 3]).ToArray()); }

    [TestMethod]
    public void Pairwise_FewerThanTwo_IsEmpty()
    {
        Assert.AreEqual(0, WindowedIterator.Pairwise([1]).Count());
        Assert.AreEqual(0, WindowedIterator.Pairwise(Array.Empty<int>()).Count());
    }

    [TestMethod]
    public void Pairwise_Null_Throws() { Assert.ThrowsExactly<ArgumentNullException>(static () => WindowedIterator.Pairwise<int>(null!)); }
}
