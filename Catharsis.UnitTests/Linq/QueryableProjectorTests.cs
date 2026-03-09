using System.Linq.Expressions;

using Catharsis.Linq;

namespace Catharsis.UnitTests.Linq;

[TestClass]
public class QueryableProjectorTests
{
    private sealed record Item(int Id, string Name, string[] Tags);

    private static IQueryable<Item> Source => new Item[]
    {
        new(1, "Alpha", ["a", "b"]),
        new(2, "Beta", ["c"]),
        new(3, "Gamma", ["d", "e", "f"])
    }.AsQueryable();

    [TestMethod]
    public void Project_SelectsProperty()
    {
        List<string> result = Source.Project(x => x.Name).ToList();

        CollectionAssert.AreEqual(new[] { "Alpha", "Beta", "Gamma" }, result);
    }

    [TestMethod]
    public void ProjectDistinct_RemovesDuplicates()
    {
        IQueryable<Item> dupes = new Item[]
        {
            new(1, "A", []),
            new(2, "B", []),
            new(3, "A", [])
        }.AsQueryable();

        List<string> result = dupes.ProjectDistinct(x => x.Name).ToList();

        Assert.AreEqual(2, result.Count);
    }

    [TestMethod]
    public void ProjectIf_ConditionTrue_AppliesProjection()
    {
        List<string> result = Source.ProjectIf(true, x => x.Name).ToList();

        CollectionAssert.AreEqual(new[] { "Alpha", "Beta", "Gamma" }, result);
    }

    [TestMethod]
    public void ProjectIf_ConditionFalse_ReturnsCast()
    {
        IQueryable<int> source = new[] { 1, 2, 3 }.AsQueryable();
        List<int> result = source.ProjectIf<int, int>(false, x => x * 2).ToList();

        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, result);
    }

    [TestMethod]
    public void ProjectMany_FlattensCollections()
    {
        List<string> result = Source.ProjectMany(x => x.Tags).ToList();

        CollectionAssert.AreEqual(new[] { "a", "b", "c", "d", "e", "f" }, result);
    }

    [TestMethod]
    public void ProjectThrough_ComposesProjections()
    {
        Expression<Func<Item, string>> getName = x => x.Name;
        Expression<Func<string, int>> getLength = s => s.Length;

        List<int> result = Source.ProjectThrough(getName, getLength).ToList();

        CollectionAssert.AreEqual(new[] { 5, 4, 5 }, result);
    }

    [TestMethod]
    public void ProjectToDictionaries_CreatesDictionaryPerRow()
    {
        Dictionary<string, Expression<Func<Item, object?>>> projections = new()
        {
            ["id"] = x => x.Id,
            ["name"] = x => x.Name
        };

        List<Dictionary<string, object?>> result = Source.ProjectToDictionaries(projections).ToList();

        Assert.AreEqual(3, result.Count);
        Assert.AreEqual(1, result[0]["id"]);
        Assert.AreEqual("Alpha", result[0]["name"]);
    }

    [TestMethod]
    public void ProjectWithIndex_AddsIndex()
    {
        List<(int Index, Item Element)> result = Source.ProjectWithIndex().ToList();

        Assert.AreEqual(0, result[0].Index);
        Assert.AreEqual("Alpha", result[0].Element.Name);
        Assert.AreEqual(2, result[2].Index);
    }

    [TestMethod]
    public void Project_NullSource_ThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(
            () => ((IQueryable<Item>)null!).Project(x => x.Name));
    }

    [TestMethod]
    public void Project_NullSelector_ThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(
            () => Source.Project<Item, string>(null!));
    }
}
