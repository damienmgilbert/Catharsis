using Catharsis.Linq;

namespace Catharsis.UnitTests.Linq;

[TestClass]
public class GroupingAdapterTests
{
    private static IGrouping<string, int>[] SampleGroupings =>
    [
        SequenceFactory.Grouping("a", 1, 2, 3),
        SequenceFactory.Grouping("b", 4, 5)
    ];

    [TestMethod]
    public void AggregatePerGroup_Groupings_AggregatesEachGroup()
    {
        List<string> result = SampleGroupings.AsEnumerable()
            .AggregatePerGroup((key, elements) => $"{key}:{elements.Sum()}")
            .ToList();

        CollectionAssert.AreEqual(new[] { "a:6", "b:9" }, result);
    }

    [TestMethod]
    public void AggregatePerGroup_Groupings_NullSource_ThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(
            () => ((IEnumerable<IGrouping<string, int>>)null!).AggregatePerGroup((k, e) => 0).ToList());
    }

    [TestMethod]
    public void AggregatePerGroup_Lookup_AggregatesEachGroup()
    {
        ILookup<string, int> lookup = new[] { ("a", 1), ("a", 2), ("b", 3) }
            .ToLookup(t => t.Item1, t => t.Item2);

        List<int> result = lookup.AggregatePerGroup((key, elements) => elements.Sum()).ToList();

        Assert.AreEqual(2, result.Count);
    }

    [TestMethod]
    public void ProjectElements_Groupings_ProjectsElements()
    {
        List<IGrouping<string, string>> result = SampleGroupings.AsEnumerable()
            .ProjectElements(x => x.ToString())
            .ToList();

        Assert.AreEqual("a", result[0].Key);
        CollectionAssert.AreEqual(new[] { "1", "2", "3" }, result[0].ToList());
    }

    [TestMethod]
    public void ProjectElements_Lookup_ProjectsElements()
    {
        ILookup<string, int> lookup = new[] { ("x", 10), ("x", 20) }
            .ToLookup(t => t.Item1, t => t.Item2);

        ILookup<string, string> result = lookup.ProjectElements(v => $"v{v}");

        CollectionAssert.AreEqual(new[] { "v10", "v20" }, result["x"].ToList());
    }

    [TestMethod]
    public void ReKey_Groupings_TransformsKeys()
    {
        List<IGrouping<string, int>> result = SampleGroupings.AsEnumerable()
            .ReKey(k => k.ToUpperInvariant())
            .ToList();

        Assert.AreEqual("A", result[0].Key);
        Assert.AreEqual("B", result[1].Key);
    }

    [TestMethod]
    public void ReKey_Lookup_TransformsKeys()
    {
        ILookup<string, int> lookup = new[] { ("a", 1), ("b", 2) }
            .ToLookup(t => t.Item1, t => t.Item2);

        ILookup<string, int> result = lookup.ReKey(k => k.ToUpperInvariant());

        Assert.IsTrue(result.Contains("A"));
        Assert.IsTrue(result.Contains("B"));
    }

    [TestMethod]
    public void ToDictionary_Lookup_ConvertsToDictionary()
    {
        ILookup<string, int> lookup = new[] { ("a", 1), ("a", 2), ("b", 3) }
            .ToLookup(t => t.Item1, t => t.Item2);

        Dictionary<string, List<int>> dict = lookup.ToDictionary();

        CollectionAssert.AreEqual(new[] { 1, 2 }, dict["a"]);
        CollectionAssert.AreEqual(new[] { 3 }, dict["b"]);
    }

    [TestMethod]
    public void ToDictionary_Groupings_ConvertsToDictionary()
    {
        Dictionary<string, List<int>> dict = SampleGroupings.AsEnumerable()
            .ToDictionary();

        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, dict["a"]);
        CollectionAssert.AreEqual(new[] { 4, 5 }, dict["b"]);
    }

    [TestMethod]
    public void ToGroupings_Lookup_ReturnsGroupings()
    {
        ILookup<string, int> lookup = new[] { ("a", 1) }
            .ToLookup(t => t.Item1, t => t.Item2);

        List<IGrouping<string, int>> result = lookup.ToGroupings().ToList();

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("a", result[0].Key);
    }

    [TestMethod]
    public void ToKeyValuePair_ConvertsGrouping()
    {
        IGrouping<string, int> grouping = SequenceFactory.Grouping("k", 1, 2);
        KeyValuePair<string, List<int>> kvp = grouping.ToKeyValuePair();

        Assert.AreEqual("k", kvp.Key);
        CollectionAssert.AreEqual(new[] { 1, 2 }, kvp.Value);
    }

    [TestMethod]
    public void ToLookup_Groupings_ConvertsToLookup()
    {
        ILookup<string, int> lookup = SampleGroupings.AsEnumerable().ToLookup();

        Assert.AreEqual(2, lookup.Count);
        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, lookup["a"].ToList());
    }

    [TestMethod]
    public void WhereCountAtLeast_FiltersSmallGroups()
    {
        List<IGrouping<string, int>> result = SampleGroupings.AsEnumerable()
            .WhereCountAtLeast(3)
            .ToList();

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("a", result[0].Key);
    }

    [TestMethod]
    public void WhereKey_FiltersByKeyPredicate()
    {
        List<IGrouping<string, int>> result = SampleGroupings.AsEnumerable()
            .WhereKey(k => k == "b")
            .ToList();

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("b", result[0].Key);
    }
}
