using Catharsis.Linq;

namespace Catharsis.UnitTests.Linq;

[TestClass]
public class SequenceFactoryTests
{
    [TestMethod]
    public void Create_GeneratesElementsByIndex()
    {
        List<int> result = SequenceFactory.Create(4, i => i * 10).ToList();
        CollectionAssert.AreEqual(new[] { 0, 10, 20, 30 }, result);
    }

    [TestMethod]
    public void Create_ZeroCount_ReturnsEmpty()
    {
        Assert.AreEqual(0, SequenceFactory.Create(0, i => i).Count());
    }

    [TestMethod]
    public void Create_NegativeCount_ThrowsArgumentOutOfRangeException()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => SequenceFactory.Create(-1, i => i).ToList());
    }

    [TestMethod]
    public void Create_NullFactory_ThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => SequenceFactory.Create<int>(3, null!).ToList());
    }

    [TestMethod]
    public void Cycle_RepeatsElementsCyclically()
    {
        List<int> result = SequenceFactory.Cycle([1, 2, 3]).Take(7).ToList();
        CollectionAssert.AreEqual(new[] { 1, 2, 3, 1, 2, 3, 1 }, result);
    }

    [TestMethod]
    public void Cycle_EmptySource_ThrowsInvalidOperationException()
    {
        Assert.ThrowsExactly<InvalidOperationException>(() => SequenceFactory.Cycle(Array.Empty<int>()).Take(1).ToList());
    }

    [TestMethod]
    public void Cycle_NullSource_ThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => SequenceFactory.Cycle<int>(null!).ToList());
    }

    [TestMethod]
    public void Empty_ReturnsEmptySequence()
    {
        Assert.AreEqual(0, SequenceFactory.Empty<string>().Count());
    }

    [TestMethod]
    public void Generate_ProducesSequenceWhilePredicateHolds()
    {
        List<int> result = SequenceFactory.Generate(1, x => x < 16, x => x * 2).ToList();
        CollectionAssert.AreEqual(new[] { 1, 2, 4, 8 }, result);
    }

    [TestMethod]
    public void Generate_WithResultSelector_ProjectsState()
    {
        List<string> result = SequenceFactory.Generate(1, x => x <= 3, x => x + 1, x => $"v{x}").ToList();
        CollectionAssert.AreEqual(new[] { "v1", "v2", "v3" }, result);
    }

    [TestMethod]
    public void Generate_NullPredicate_ThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => SequenceFactory.Generate(0, null!, x => x + 1).ToList());
    }

    [TestMethod]
    public void Grouping_CreatesGroupWithKeyAndElements()
    {
        IGrouping<string, int> group = SequenceFactory.Grouping("key", new[] { 1, 2, 3 });
        Assert.AreEqual("key", group.Key);
        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, group.ToList());
    }

    [TestMethod]
    public void Grouping_Params_CreatesGroup()
    {
        IGrouping<int, string> group = SequenceFactory.Grouping(42, "a", "b");
        Assert.AreEqual(42, group.Key);
        Assert.AreEqual(2, group.Count());
    }

    [TestMethod]
    public void Groupings_GroupsByKey()
    {
        List<IGrouping<int, int>> result = SequenceFactory.Groupings(new[] { 1, 2, 3, 4 }, x => x % 2).ToList();
        Assert.AreEqual(2, result.Count);
    }

    [TestMethod]
    public void Infinite_GeneratesUnboundedSequence()
    {
        List<int> result = SequenceFactory.Infinite(1, x => x * 2).Take(5).ToList();
        CollectionAssert.AreEqual(new[] { 1, 2, 4, 8, 16 }, result);
    }

    [TestMethod]
    public void Ordered_SortsSequence()
    {
        IOrderedEnumerable<int> result = SequenceFactory.Ordered(new[] { 3, 1, 2 });
        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, result.ToList());
    }

    [TestMethod]
    public void OrderedBy_SortsByKey()
    {
        IOrderedEnumerable<string> result = SequenceFactory.OrderedBy(new[] { "bbb", "a", "cc" }, s => s.Length);
        CollectionAssert.AreEqual(new[] { "a", "cc", "bbb" }, result.ToList());
    }

    [TestMethod]
    public void OrderedByDescending_SortsByKeyDescending()
    {
        IOrderedEnumerable<int> result = SequenceFactory.OrderedByDescending(new[] { 1, 3, 2 }, x => x);
        CollectionAssert.AreEqual(new[] { 3, 2, 1 }, result.ToList());
    }

    [TestMethod]
    public void Random_GeneratesElements()
    {
        List<int> result = SequenceFactory.Random(r => r.Next(0, 100), new Random(42)).Take(5).ToList();
        Assert.AreEqual(5, result.Count);
        Assert.IsTrue(result.All(x => x >= 0 && x < 100));
    }

    [TestMethod]
    public void Range_GeneratesConsecutiveIntegers()
    {
        CollectionAssert.AreEqual(new[] { 5, 6, 7 }, SequenceFactory.Range(5, 3).ToList());
    }

    [TestMethod]
    public void Range_NegativeCount_ThrowsArgumentOutOfRangeException()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => SequenceFactory.Range(0, -1).ToList());
    }

    [TestMethod]
    public void Repeat_RepeatsElement()
    {
        CollectionAssert.AreEqual(new[] { "x", "x", "x" }, SequenceFactory.Repeat("x", 3).ToList());
    }

    [TestMethod]
    public void Singleton_ReturnsSingleElement()
    {
        CollectionAssert.AreEqual(new[] { 42 }, SequenceFactory.Singleton(42).ToList());
    }
}
