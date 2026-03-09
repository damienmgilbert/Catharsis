using Catharsis.Linq;

namespace Catharsis.UnitTests.Linq;

[TestClass]
public class PartitionBuilderTests
{
    [TestMethod]
    public void AddPartition_And_Apply_ClassifiesElements()
    {
        PartitionBuilder<int> builder = new PartitionBuilder<int>()
            .AddPartition("even", x => x % 2 == 0)
            .AddPartition("odd", x => x % 2 != 0);

        ILookup<string, int> result = builder.Apply([1, 2, 3, 4, 5]);

        CollectionAssert.AreEqual(new[] { 2, 4 }, result["even"].ToList());
        CollectionAssert.AreEqual(new[] { 1, 3, 5 }, result["odd"].ToList());
    }

    [TestMethod]
    public void Apply_UnmatchedGoToDefaultPartition()
    {
        PartitionBuilder<int> builder = new PartitionBuilder<int>()
            .AddPartition("big", x => x > 100);

        ILookup<string, int> result = builder.Apply([1, 2, 200]);

        CollectionAssert.AreEqual(new[] { 200 }, result["big"].ToList());
        CollectionAssert.AreEqual(new[] { 1, 2 }, result["__unmatched__"].ToList());
    }

    [TestMethod]
    public void WithDefaultPartition_ChangesDefaultName()
    {
        PartitionBuilder<int> builder = new PartitionBuilder<int>()
            .WithDefaultPartition("other")
            .AddPartition("even", x => x % 2 == 0);

        ILookup<string, int> result = builder.Apply([1, 2]);

        Assert.IsTrue(result.Contains("other"));
        CollectionAssert.AreEqual(new[] { 1 }, result["other"].ToList());
    }

    [TestMethod]
    public void WithFirstMatchOnly_False_AllowsMultipleMatches()
    {
        PartitionBuilder<int> builder = new PartitionBuilder<int>()
            .WithFirstMatchOnly(false)
            .AddPartition("positive", x => x > 0)
            .AddPartition("even", x => x % 2 == 0);

        ILookup<string, int> result = builder.Apply([2]);

        Assert.IsTrue(result["positive"].Contains(2));
        Assert.IsTrue(result["even"].Contains(2));
    }

    [TestMethod]
    public void AddPartitionIf_ConditionTrue_AddsRule()
    {
        PartitionBuilder<int> builder = new PartitionBuilder<int>()
            .AddPartitionIf(true, "big", x => x > 10);

        Assert.AreEqual(1, builder.Count);
    }

    [TestMethod]
    public void AddPartitionIf_ConditionFalse_DoesNotAddRule()
    {
        PartitionBuilder<int> builder = new PartitionBuilder<int>()
            .AddPartitionIf(false, "big", x => x > 10);

        Assert.AreEqual(0, builder.Count);
    }

    [TestMethod]
    public void ApplyAsGroupings_ReturnsGroupings()
    {
        PartitionBuilder<int> builder = new PartitionBuilder<int>()
            .AddPartition("even", x => x % 2 == 0);

        List<IGrouping<string, int>> groups = builder.ApplyAsGroupings([1, 2, 3]).ToList();

        Assert.IsTrue(groups.Count >= 2);
    }

    [TestMethod]
    public void Clear_RemovesAllRules()
    {
        PartitionBuilder<int> builder = new PartitionBuilder<int>()
            .AddPartition("a", _ => true)
            .AddPartition("b", _ => true)
            .Clear();

        Assert.AreEqual(0, builder.Count);
    }

    [TestMethod]
    public void Apply_NullSource_ThrowsArgumentNullException()
    {
        PartitionBuilder<int> builder = new();
        Assert.ThrowsExactly<ArgumentNullException>(() => builder.Apply(null!));
    }
}
