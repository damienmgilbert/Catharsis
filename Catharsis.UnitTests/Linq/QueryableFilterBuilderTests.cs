using System.Linq.Expressions;

using Catharsis.Linq;

namespace Catharsis.UnitTests.Linq;

[TestClass]
public class QueryableFilterBuilderTests
{
    private sealed record Item(int Id, string Name, int Price);

    private static IQueryable<Item> Source => new Item[]
    {
        new(1, "Apple", 5),
        new(2, "Banana", 3),
        new(3, "Cherry", 8),
        new(4, "Date", 12),
        new(5, "Elderberry", 15)
    }.AsQueryable();

    [TestMethod]
    public void Where_And_Apply_FiltersWithCombinedAnd()
    {
        QueryableFilterBuilder<Item> builder = new QueryableFilterBuilder<Item>()
            .Where(x => x.Price > 4)
            .Where(x => x.Price < 13);

        List<Item> result = builder.Apply(Source).ToList();

        Assert.AreEqual(2, result.Count);
        Assert.IsTrue(result.All(x => x.Price > 4 && x.Price < 13));
    }

    [TestMethod]
    public void WhereCompare_Equal_FiltersCorrectly()
    {
        QueryableFilterBuilder<Item> builder = new QueryableFilterBuilder<Item>()
            .WhereCompare(x => x.Id, FilterComparison.Equal, 3);

        List<Item> result = builder.Apply(Source).ToList();

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("Cherry", result[0].Name);
    }

    [TestMethod]
    public void WhereCompare_GreaterThan_FiltersCorrectly()
    {
        QueryableFilterBuilder<Item> builder = new QueryableFilterBuilder<Item>()
            .WhereCompare(x => x.Price, FilterComparison.GreaterThan, 10);

        List<Item> result = builder.Apply(Source).ToList();

        Assert.AreEqual(2, result.Count);
        Assert.IsTrue(result.All(x => x.Price > 10));
    }

    [TestMethod]
    public void WhereContains_FiltersBySubstring()
    {
        QueryableFilterBuilder<Item> builder = new QueryableFilterBuilder<Item>()
            .WhereContains(x => x.Name, "an");

        List<Item> result = builder.Apply(Source).ToList();

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("Banana", result[0].Name);
    }

    [TestMethod]
    public void WhereIf_ConditionTrue_AddsFilter()
    {
        QueryableFilterBuilder<Item> builder = new QueryableFilterBuilder<Item>()
            .WhereIf(true, x => x.Price > 10);

        List<Item> result = builder.Apply(Source).ToList();

        Assert.AreEqual(2, result.Count);
    }

    [TestMethod]
    public void WhereIf_ConditionFalse_SkipsFilter()
    {
        QueryableFilterBuilder<Item> builder = new QueryableFilterBuilder<Item>()
            .WhereIf(false, x => x.Price > 10);

        List<Item> result = builder.Apply(Source).ToList();

        Assert.AreEqual(5, result.Count);
    }

    [TestMethod]
    public void WhereIn_FiltersByAllowedValues()
    {
        QueryableFilterBuilder<Item> builder = new QueryableFilterBuilder<Item>()
            .WhereIn(x => x.Id, new[] { 1, 3, 5 });

        List<Item> result = builder.Apply(Source).ToList();

        Assert.AreEqual(3, result.Count);
    }

    [TestMethod]
    public void WhereIfNotNull_ClassValue_NotNull_AddsFilter()
    {
        string? search = "Cherry";
        QueryableFilterBuilder<Item> builder = new QueryableFilterBuilder<Item>()
            .WhereIfNotNull(search, v => x => x.Name == v);

        List<Item> result = builder.Apply(Source).ToList();

        Assert.AreEqual(1, result.Count);
    }

    [TestMethod]
    public void WhereIfNotNull_ClassValue_Null_SkipsFilter()
    {
        string? search = null;
        QueryableFilterBuilder<Item> builder = new QueryableFilterBuilder<Item>()
            .WhereIfNotNull(search, v => x => x.Name == v);

        List<Item> result = builder.Apply(Source).ToList();

        Assert.AreEqual(5, result.Count);
    }

    [TestMethod]
    public void WhereIfNotNull_StructValue_HasValue_AddsFilter()
    {
        int? minPrice = 10;
        QueryableFilterBuilder<Item> builder = new QueryableFilterBuilder<Item>()
            .WhereIfNotNull(minPrice, v => x => x.Price >= v);

        List<Item> result = builder.Apply(Source).ToList();

        Assert.AreEqual(2, result.Count);
    }

    [TestMethod]
    public void WhereIfNotNull_StructValue_Null_SkipsFilter()
    {
        int? minPrice = null;
        QueryableFilterBuilder<Item> builder = new QueryableFilterBuilder<Item>()
            .WhereIfNotNull(minPrice, v => x => x.Price >= v);

        List<Item> result = builder.Apply(Source).ToList();

        Assert.AreEqual(5, result.Count);
    }

    [TestMethod]
    public void Build_ReturnsExpression()
    {
        QueryableFilterBuilder<Item> builder = new QueryableFilterBuilder<Item>()
            .Where(x => x.Price > 5);

        Expression<Func<Item, bool>> expr = builder.Build();

        Assert.IsNotNull(expr);
        Func<Item, bool> compiled = expr.Compile();
        Assert.IsTrue(compiled(new Item(1, "test", 10)));
        Assert.IsFalse(compiled(new Item(1, "test", 3)));
    }

    [TestMethod]
    public void Clear_RemovesAllPredicates()
    {
        QueryableFilterBuilder<Item> builder = new QueryableFilterBuilder<Item>()
            .Where(x => x.Price > 100)
            .Clear();

        List<Item> result = builder.Apply(Source).ToList();

        Assert.AreEqual(5, result.Count);
    }

    [TestMethod]
    public void Apply_NullSource_ThrowsArgumentNullException()
    {
        QueryableFilterBuilder<Item> builder = new();
        Assert.ThrowsExactly<ArgumentNullException>(() => builder.Apply(null!));
    }
}
