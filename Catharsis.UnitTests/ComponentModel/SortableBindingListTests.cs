using Catharsis.ComponentModel;
using System.ComponentModel;

namespace Catharsis.UnitTests.ComponentModel;

[TestClass]
public class SortableBindingListTests
{
    private sealed class Item
    {
        public string Name { get; set; } = string.Empty;
        public int Value { get; set; }
    }

    [TestMethod]
    public void SupportsSorting_ReturnsTrue()
    {
        var list = new SortableBindingList<Item>();
        IBindingList bindingList = list;

        Assert.IsTrue(bindingList.SupportsSorting);
    }

    [TestMethod]
    public void SupportsSearching_ReturnsTrue()
    {
        var list = new SortableBindingList<Item>();
        IBindingList bindingList = list;

        Assert.IsTrue(bindingList.SupportsSearching);
    }

    [TestMethod]
    public void ApplySort_Ascending_SortsItemsByProperty()
    {
        var list = new SortableBindingList<Item>
        {
            new() { Name = "Charlie", Value = 3 },
            new() { Name = "Alice", Value = 1 },
            new() { Name = "Bob", Value = 2 }
        };

        IBindingList bindingList = list;
        var prop = TypeDescriptor.GetProperties(typeof(Item))[nameof(Item.Name)]!;
        bindingList.ApplySort(prop, ListSortDirection.Ascending);

        Assert.AreEqual("Alice", list[0].Name);
        Assert.AreEqual("Bob", list[1].Name);
        Assert.AreEqual("Charlie", list[2].Name);
        Assert.IsTrue(bindingList.IsSorted);
    }

    [TestMethod]
    public void ApplySort_Descending_SortsItemsInReverse()
    {
        var list = new SortableBindingList<Item>
        {
            new() { Name = "Alice", Value = 1 },
            new() { Name = "Charlie", Value = 3 },
            new() { Name = "Bob", Value = 2 }
        };

        IBindingList bindingList = list;
        var prop = TypeDescriptor.GetProperties(typeof(Item))[nameof(Item.Value)]!;
        bindingList.ApplySort(prop, ListSortDirection.Descending);

        Assert.AreEqual(3, list[0].Value);
        Assert.AreEqual(2, list[1].Value);
        Assert.AreEqual(1, list[2].Value);
    }

    [TestMethod]
    public void RemoveSort_ClearsSortState()
    {
        var list = new SortableBindingList<Item>
        {
            new() { Name = "B" },
            new() { Name = "A" }
        };

        IBindingList bindingList = list;
        var prop = TypeDescriptor.GetProperties(typeof(Item))[nameof(Item.Name)]!;
        bindingList.ApplySort(prop, ListSortDirection.Ascending);

        Assert.IsTrue(bindingList.IsSorted);

        bindingList.RemoveSort();

        Assert.IsFalse(bindingList.IsSorted);
    }

    [TestMethod]
    public void Find_ExistingItem_ReturnsCorrectIndex()
    {
        var list = new SortableBindingList<Item>
        {
            new() { Name = "Alice" },
            new() { Name = "Bob" },
            new() { Name = "Charlie" }
        };

        IBindingList bindingList = list;
        var prop = TypeDescriptor.GetProperties(typeof(Item))[nameof(Item.Name)]!;

        Assert.AreEqual(1, bindingList.Find(prop, "Bob"));
    }

    [TestMethod]
    public void Find_NonExistingItem_ReturnsNegativeOne()
    {
        var list = new SortableBindingList<Item>
        {
            new() { Name = "Alice" }
        };

        IBindingList bindingList = list;
        var prop = TypeDescriptor.GetProperties(typeof(Item))[nameof(Item.Name)]!;

        Assert.AreEqual(-1, bindingList.Find(prop, "Zoe"));
    }

    [TestMethod]
    public void Constructor_WithList_WrapsExistingItems()
    {
        var items = new List<Item>
        {
            new() { Name = "One" },
            new() { Name = "Two" }
        };

        var list = new SortableBindingList<Item>(items);

        Assert.AreEqual(2, list.Count);
        Assert.AreEqual("One", list[0].Name);
    }

    [TestMethod]
    public void ApplySort_NullValues_HandlesGracefully()
    {
        var list = new SortableBindingList<Item>
        {
            new() { Name = "Bob" },
            new() { Name = null! },
            new() { Name = "Alice" }
        };

        IBindingList bindingList = list;
        var prop = TypeDescriptor.GetProperties(typeof(Item))[nameof(Item.Name)]!;
        bindingList.ApplySort(prop, ListSortDirection.Ascending);

        // null sorts first
        Assert.IsNull(list[0].Name);
        Assert.AreEqual("Alice", list[1].Name);
        Assert.AreEqual("Bob", list[2].Name);
    }
}
