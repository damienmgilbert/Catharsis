using System.Linq.Expressions;

using Catharsis.Linq;

namespace Catharsis.UnitTests.Linq;

[TestClass]
public class QueryableSorterTests
{
    private sealed record Item(int Id, string Name, int Price);

    private static IQueryable<Item> Source => new Item[]
    {
        new(3, "Cherry", 8),
        new(1, "Apple", 5),
        new(2, "Banana", 3),
        new(4, "Date", 12),
        new(5, "Elderberry", 15)
    }.AsQueryable();

    [TestMethod]
    public void OrderByIf_ConditionTrue_AppliesOrdering()
    {
        List<Item> result = Source.OrderByIf(true, x => x.Id).ToList();

        Assert.AreEqual(1, result[0].Id);
        Assert.AreEqual(5, result[4].Id);
    }

    [TestMethod]
    public void OrderByIf_ConditionFalse_RetainsOriginalOrder()
    {
        List<Item> result = Source.OrderByIf(false, x => x.Id).ToList();

        Assert.AreEqual(3, result[0].Id);
    }

    [TestMethod]
    public void OrderByDescendingIf_ConditionTrue_AppliesDescending()
    {
        List<Item> result = Source.OrderByDescendingIf(true, x => x.Id).ToList();

        Assert.AreEqual(5, result[0].Id);
        Assert.AreEqual(1, result[4].Id);
    }

    [TestMethod]
    public void OrderByDirection_Ascending_SortsAscending()
    {
        List<Item> result = Source.OrderByDirection(x => x.Price, SortDirection.Ascending).ToList();

        Assert.AreEqual(3, result[0].Price);
        Assert.AreEqual(15, result[4].Price);
    }

    [TestMethod]
    public void OrderByDirection_Descending_SortsDescending()
    {
        List<Item> result = Source.OrderByDirection(x => x.Price, SortDirection.Descending).ToList();

        Assert.AreEqual(15, result[0].Price);
        Assert.AreEqual(3, result[4].Price);
    }

    [TestMethod]
    public void ThenByDirection_AddsSortLevel()
    {
        IQueryable<Item> withDupes = new Item[]
        {
            new(1, "A", 5),
            new(2, "B", 5),
            new(3, "C", 3)
        }.AsQueryable();

        List<Item> result = withDupes
            .OrderByDirection(x => x.Price, SortDirection.Ascending)
            .ThenByDirection(x => x.Name, SortDirection.Descending)
            .ToList();

        Assert.AreEqual("C", result[0].Name);
        Assert.AreEqual("B", result[1].Name);
        Assert.AreEqual("A", result[2].Name);
    }

    [TestMethod]
    public void OrderByProperty_SortsByNamedProperty()
    {
        List<Item> result = Source.OrderByProperty("Name").ToList();

        Assert.AreEqual("Apple", result[0].Name);
        Assert.AreEqual("Elderberry", result[4].Name);
    }

    [TestMethod]
    public void OrderByProperty_Descending_SortsDescending()
    {
        List<Item> result = Source.OrderByProperty("Price", SortDirection.Descending).ToList();

        Assert.AreEqual(15, result[0].Price);
    }

    [TestMethod]
    public void OrderByProperty_InvalidProperty_ThrowsArgumentException()
    {
        Assert.ThrowsExactly<ArgumentException>(
            () => Source.OrderByProperty("NonExistent").ToList());
    }

    [TestMethod]
    public void ThenByProperty_SortsBySecondaryProperty()
    {
        IQueryable<Item> data = new Item[]
        {
            new(1, "A", 5),
            new(2, "B", 5),
            new(3, "C", 3)
        }.AsQueryable();

        List<Item> result = data.OrderByProperty("Price")
            .ThenByProperty("Name", SortDirection.Descending)
            .ToList();

        Assert.AreEqual("C", result[0].Name);
        Assert.AreEqual("B", result[1].Name);
    }

    [TestMethod]
    public void ApplySort_WithDescriptors_AppliesMultipleOrders()
    {
        SortDescriptor<Item>[] descriptors =
        [
            SortDescriptor<Item>.Create(x => x.Price, SortDirection.Ascending),
            SortDescriptor<Item>.Create(x => x.Name, SortDirection.Descending)
        ];

        List<Item> result = Source.ApplySort(descriptors).ToList();

        Assert.AreEqual(3, result[0].Price);
    }

    [TestMethod]
    public void ApplySort_EmptyDescriptors_RetainsOriginalOrder()
    {
        List<Item> result = Source.ApplySort(Array.Empty<SortDescriptor<Item>>()).ToList();

        Assert.AreEqual(3, result[0].Id);
    }

    [TestMethod]
    public void ApplySort_WithBuilder_AppliesBuiltDescriptors()
    {
        QueryableSortBuilder<Item> builder = new QueryableSortBuilder<Item>()
            .OrderBy(x => x.Price)
            .OrderByDescending(x => x.Id);

        List<Item> result = Source.ApplySort(builder).ToList();

        Assert.AreEqual(3, result[0].Price);
    }
}

[TestClass]
public class QueryableSortBuilderTests
{
    private sealed record Item(int Id, string Name);

    [TestMethod]
    public void OrderBy_AddsAscendingDescriptor()
    {
        QueryableSortBuilder<Item> builder = new QueryableSortBuilder<Item>()
            .OrderBy(x => x.Id);

        Assert.AreEqual(1, builder.Count);
        Assert.AreEqual(SortDirection.Ascending, builder.Build()[0].Direction);
    }

    [TestMethod]
    public void OrderByDescending_AddsDescendingDescriptor()
    {
        QueryableSortBuilder<Item> builder = new QueryableSortBuilder<Item>()
            .OrderByDescending(x => x.Name);

        Assert.AreEqual(SortDirection.Descending, builder.Build()[0].Direction);
    }

    [TestMethod]
    public void OrderByIf_ConditionTrue_AddsDescriptor()
    {
        QueryableSortBuilder<Item> builder = new QueryableSortBuilder<Item>()
            .OrderByIf(true, x => x.Id);

        Assert.AreEqual(1, builder.Count);
    }

    [TestMethod]
    public void OrderByIf_ConditionFalse_SkipsDescriptor()
    {
        QueryableSortBuilder<Item> builder = new QueryableSortBuilder<Item>()
            .OrderByIf(false, x => x.Id);

        Assert.AreEqual(0, builder.Count);
    }

    [TestMethod]
    public void OrderByProperty_AddsByName()
    {
        QueryableSortBuilder<Item> builder = new QueryableSortBuilder<Item>()
            .OrderByProperty("Name");

        Assert.AreEqual(1, builder.Count);
    }

    [TestMethod]
    public void Clear_RemovesAllDescriptors()
    {
        QueryableSortBuilder<Item> builder = new QueryableSortBuilder<Item>()
            .OrderBy(x => x.Id)
            .Clear();

        Assert.AreEqual(0, builder.Count);
    }
}

[TestClass]
public class SortDescriptorTests
{
    private sealed record Item(int Id, string Name);

    [TestMethod]
    public void Create_WithExpression_SetsProperties()
    {
        SortDescriptor<Item> desc = SortDescriptor<Item>.Create(x => x.Id, SortDirection.Descending);

        Assert.AreEqual(SortDirection.Descending, desc.Direction);
        Assert.AreEqual(typeof(int), desc.KeyType);
    }

    [TestMethod]
    public void Create_WithPropertyName_SetsProperties()
    {
        SortDescriptor<Item> desc = SortDescriptor<Item>.Create("Name", SortDirection.Ascending);

        Assert.AreEqual(SortDirection.Ascending, desc.Direction);
        Assert.AreEqual(typeof(string), desc.KeyType);
    }

    [TestMethod]
    public void Create_WithInvalidPropertyName_ThrowsArgumentException()
    {
        Assert.ThrowsExactly<ArgumentException>(
            () => SortDescriptor<Item>.Create("NonExistent"));
    }
}
