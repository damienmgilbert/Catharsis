using System.Linq.Expressions;

using Catharsis.Linq;

namespace Catharsis.UnitTests.Linq;

///<summary>
///Unit tests for the <see cref="QueryableProjector"/> class.
///</summary>
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
        List<string> result = Source.Project(static x => x.Name).ToList();

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

        List<string> result = dupes.ProjectDistinct(static x => x.Name).ToList();

        Assert.HasCount(2, result);
    }

    [TestMethod]
    public void ProjectIf_ConditionTrue_AppliesProjection()
    {
        List<string> result = Source.ProjectIf(true, static x => x.Name).ToList();

        CollectionAssert.AreEqual(new[] { "Alpha", "Beta", "Gamma" }, result);
    }

    [TestMethod]
    public void ProjectIf_ConditionFalse_ReturnsCast()
    {
        IQueryable<int> source = new[] { 1, 2, 3 }.AsQueryable();
        List<int> result = source.ProjectIf<int, int>(false, static x => x * 2).ToList();

        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, result);
    }

    [TestMethod]
    public void ProjectMany_FlattensCollections()
    {
        List<string> result = Source.ProjectMany(static x => x.Tags).ToList();

        CollectionAssert.AreEqual(new[] { "a", "b", "c", "d", "e", "f" }, result);
    }

    [TestMethod]
    public void ProjectThrough_ComposesProjections()
    {
        Expression<Func<Item, string>> getName = static x => x.Name;
        Expression<Func<string, int>> getLength = static s => s.Length;

        List<int> result = Source.ProjectThrough(getName, getLength).ToList();

        CollectionAssert.AreEqual(new[] { 5, 4, 5 }, result);
    }

    [TestMethod]
    public void ProjectToDictionaries_CreatesDictionaryPerRow()
    {
        Dictionary<string, Expression<Func<Item, object?>>> projections = new()
        {
            ["id"] = static x => x.Id,
            ["name"] = static x => x.Name
        };

        List<Dictionary<string, object?>> result = Source.ProjectToDictionaries(projections).ToList();

        Assert.HasCount(3, result);
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
            static () => ((IQueryable<Item>)null!).Project(static x => x.Name));
    }

    [TestMethod]
    public void Project_NullSelector_ThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(
            static () => Source.Project<Item, string>(null!));
    }
}
