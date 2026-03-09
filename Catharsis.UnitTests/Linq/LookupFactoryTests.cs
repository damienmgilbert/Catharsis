using Catharsis.Linq;

namespace Catharsis.UnitTests.Linq;

///<summary>
///Unit tests for the <see cref="LookupFactory"/> class.
///</summary>
[TestClass]
public class LookupFactoryTests
{
    [TestMethod]
    public void Create_WithKeySelector_CreatesLookup()
    {
        ILookup<char, string> lookup = LookupFactory.Create(
            new[] { "apple", "banana", "avocado" },
            static s => s[0]);

        CollectionAssert.AreEqual(new[] { "apple", "avocado" }, lookup['a'].ToList());
        CollectionAssert.AreEqual(new[] { "banana" }, lookup['b'].ToList());
    }

    [TestMethod]
    public void Create_WithKeyAndElementSelector_CreatesLookup()
    {
        ILookup<char, int> lookup = LookupFactory.Create(
            new[] { "apple", "banana", "avocado" },
            static s => s[0],
            static s => s.Length);

        CollectionAssert.AreEqual(new[] { 5, 7 }, lookup['a'].ToList());
    }

    [TestMethod]
    public void Create_NullSource_ThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(
            static () => LookupFactory.Create<int, int>(null!, static x => x));
    }

    [TestMethod]
    public void Empty_ReturnsEmptyLookup()
    {
        ILookup<string, int> lookup = LookupFactory.Empty<string, int>();

        Assert.AreEqual(0, lookup.Count);
        Assert.IsFalse(lookup.Contains("any"));
        Assert.AreEqual(0, lookup["any"].Count());
    }

    [TestMethod]
    public void FromDictionary_CreatesLookupFromKvps()
    {
        Dictionary<string, int> dict = new() { ["a"] = 1, ["b"] = 2 };
        ILookup<string, int> lookup = LookupFactory.FromDictionary(dict);

        Assert.AreEqual(2, lookup.Count);
        Assert.AreEqual(1, lookup["a"].Single());
    }

    [TestMethod]
    public void FromDictionaryOfCollections_CreatesLookup()
    {
        Dictionary<string, IEnumerable<int>> dict = new()
        {
            ["x"] = new[] { 1, 2 },
            ["y"] = new[] { 3 }
        };

        ILookup<string, int> lookup = LookupFactory.FromDictionaryOfCollections(dict);

        CollectionAssert.AreEqual(new[] { 1, 2 }, lookup["x"].ToList());
        Assert.AreEqual(1, lookup["y"].Count());
    }

    [TestMethod]
    public void FromGroupings_CreatesLookup()
    {
        IGrouping<string, int>[] groups =
        [
            SequenceFactory.Grouping("a", 1, 2),
            SequenceFactory.Grouping("b", 3)
        ];

        ILookup<string, int> lookup = LookupFactory.FromGroupings(groups);

        Assert.AreEqual(2, lookup.Count);
        CollectionAssert.AreEqual(new[] { 1, 2 }, lookup["a"].ToList());
    }

    [TestMethod]
    public void FromPairs_CreatesLookup()
    {
        (string, int)[] pairs = [("a", 1), ("a", 2), ("b", 3)];
        ILookup<string, int> lookup = LookupFactory.FromPairs(pairs);

        CollectionAssert.AreEqual(new[] { 1, 2 }, lookup["a"].ToList());
        Assert.AreEqual(1, lookup["b"].Count());
    }

    [TestMethod]
    public void Merge_CombinesTwoLookups()
    {
        ILookup<string, int> first = new[] { ("a", 1) }.ToLookup(static t => t.Item1, static t => t.Item2);
        ILookup<string, int> second = new[] { ("a", 2), ("b", 3) }.ToLookup(static t => t.Item1, static t => t.Item2);

        ILookup<string, int> merged = LookupFactory.Merge(first, second);

        CollectionAssert.AreEqual(new[] { 1, 2 }, merged["a"].ToList());
        Assert.AreEqual(1, merged["b"].Count());
    }

    [TestMethod]
    public void Merge_NullFirst_ThrowsArgumentNullException()
    {
        ILookup<string, int> second = new[] { ("a", 1) }.ToLookup(t => t.Item1, t => t.Item2);
        Assert.ThrowsExactly<ArgumentNullException>(
            () => LookupFactory.Merge<string, int>(null!, second));
    }
}
