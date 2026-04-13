using Catharsis.Linq;

namespace Catharsis.UnitTests.Linq;

///<summary>
///Unit tests for the <see cref="SlidingLookup"/> class.
///</summary>
[TestClass]
public class SlidingLookupTests
{
    private static readonly int[] Source = [1, 2, 3, 4, 5];

    [TestMethod]
    public void ToSlidingLookup_CreatesWindowedLookup()
    {
        ILookup<int, int> lookup = Source.ToSlidingLookup(3);

        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, lookup[0].ToList());
        CollectionAssert.AreEqual(new[] { 2, 3, 4 }, lookup[1].ToList());
        CollectionAssert.AreEqual(new[] { 3, 4, 5 }, lookup[2].ToList());
    }

    [TestMethod]
    public void ToSlidingLookup_NullSource_ThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(static () => ((IEnumerable<int>)null!).ToSlidingLookup(2));
    }

    [TestMethod]
    public void ToSlidingLookup_WindowSizeLessThan1_ThrowsArgumentOutOfRangeException()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(static () => Source.ToSlidingLookup(0));
    }

    [TestMethod]
    public void ToSlidingGroupings_CreatesGroupingsPerWindow()
    {
        List<IGrouping<int, int>> groups = Source.ToSlidingGroupings(2).ToList();

        Assert.HasCount(4, groups);
        Assert.AreEqual(0, groups[0].Key);
        CollectionAssert.AreEqual(new[] { 1, 2 }, groups[0].ToList());
    }

    [TestMethod]
    public void ToSlidingGroupingsBy_KeysByFirstElement()
    {
        List<IGrouping<int, int>> groups = Source.ToSlidingGroupingsBy(2, static x => x * 10).ToList();

        Assert.HasCount(4, groups);
        Assert.AreEqual(10, groups[0].Key);
        CollectionAssert.AreEqual(new[] { 1, 2 }, groups[0].ToList());
    }

    [TestMethod]
    public void ToSlidingLookupBy_KeysByFirstElement()
    {
        ILookup<int, int> lookup = Source.ToSlidingLookupBy(2, static x => x * 10);

        CollectionAssert.AreEqual(new[] { 1, 2 }, lookup[10].ToList());
        CollectionAssert.AreEqual(new[] { 2, 3 }, lookup[20].ToList());
    }

    [TestMethod]
    public void ToTumblingGroupings_CreatesNonOverlappingGroupings()
    {
        List<IGrouping<int, int>> groups = Source.ToTumblingGroupings(2).ToList();

        Assert.HasCount(3, groups);
        CollectionAssert.AreEqual(new[] { 1, 2 }, groups[0].ToList());
        CollectionAssert.AreEqual(new[] { 3, 4 }, groups[1].ToList());
        CollectionAssert.AreEqual(new[] { 5 }, groups[2].ToList());
    }

    [TestMethod]
    public void ToTumblingLookup_CreatesNonOverlappingLookup()
    {
        ILookup<int, int> lookup = Source.ToTumblingLookup(2);

        CollectionAssert.AreEqual(new[] { 1, 2 }, lookup[0].ToList());
        CollectionAssert.AreEqual(new[] { 3, 4 }, lookup[1].ToList());
        CollectionAssert.AreEqual(new[] { 5 }, lookup[2].ToList());
    }

    [TestMethod]
    public void ToProgressiveGroupings_CreatesAccumulatingGroupings()
    {
        List<IGrouping<int, int>> groups = new[] { 10, 20, 30 }.ToProgressiveGroupings().ToList();

        Assert.HasCount(3, groups);
        CollectionAssert.AreEqual(new[] { 10 }, groups[0].ToList());
        CollectionAssert.AreEqual(new[] { 10, 20 }, groups[1].ToList());
        CollectionAssert.AreEqual(new[] { 10, 20, 30 }, groups[2].ToList());
    }

    [TestMethod]
    public void ToProgressiveLookup_CreatesAccumulatingLookup()
    {
        ILookup<int, int> lookup = new[] { 10, 20, 30 }.ToProgressiveLookup();

        CollectionAssert.AreEqual(new[] { 10 }, lookup[0].ToList());
        CollectionAssert.AreEqual(new[] { 10, 20 }, lookup[1].ToList());
        CollectionAssert.AreEqual(new[] { 10, 20, 30 }, lookup[2].ToList());
    }
}
