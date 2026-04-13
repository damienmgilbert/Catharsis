using System.ComponentModel;
using Catharsis.ComponentModel;

namespace Catharsis.UnitTests.ComponentModel;

///<summary>
///Unit tests for the <see cref="SortableBindingList"/> class.
///</summary>
[TestClass]
public class SortableBindingListTests
{
    #region Public methods
    [TestMethod]
    public void ApplySort_Ascending_SortsItemsByProperty()
    {
        SortableBindingList<Item> list = [new() { Name = "Charlie", Value = 3 }, new() { Name = "Alice", Value = 1 }, new() { Name = "Bob", Value = 2 }];

        IBindingList bindingList = list;
        PropertyDescriptor prop = TypeDescriptor.GetProperties(typeof(Item))[nameof(Item.Name)]!;
        bindingList.ApplySort(prop, ListSortDirection.Ascending);

        Assert.AreEqual("Alice", list[0].Name);
        Assert.AreEqual("Bob", list[1].Name);
        Assert.AreEqual("Charlie", list[2].Name);
        Assert.IsTrue(bindingList.IsSorted);
    }

    [TestMethod]
    public void ApplySort_Descending_SortsItemsInReverse()
    {
        SortableBindingList<Item> list = [new() { Name = "Alice", Value = 1 }, new() { Name = "Charlie", Value = 3 }, new() { Name = "Bob", Value = 2 }];

        IBindingList bindingList = list;
        PropertyDescriptor prop = TypeDescriptor.GetProperties(typeof(Item))[nameof(Item.Value)]!;
        bindingList.ApplySort(prop, ListSortDirection.Descending);

        Assert.AreEqual(3, list[0].Value);
        Assert.AreEqual(2, list[1].Value);
        Assert.AreEqual(1, list[2].Value);
    }

    [TestMethod]
    public void ApplySort_NullValues_HandlesGracefully()
    {
        SortableBindingList<Item> list = [new() { Name = "Bob" }, new() { Name = null! }, new() { Name = "Alice" }];

        IBindingList bindingList = list;
        PropertyDescriptor prop = TypeDescriptor.GetProperties(typeof(Item))[nameof(Item.Name)]!;
        bindingList.ApplySort(prop, ListSortDirection.Ascending);

        // null sorts first
        Assert.IsNull(list[0].Name);
        Assert.AreEqual("Alice", list[1].Name);
        Assert.AreEqual("Bob", list[2].Name);
    }

    [TestMethod]
    public void Constructor_WithList_WrapsExistingItems()
    {
        List<Item> items = [new() { Name = "One" }, new() { Name = "Two" }];

        SortableBindingList<Item> list = new(items);

        Assert.HasCount(2, list);
        Assert.AreEqual("One", list[0].Name);
    }

    [TestMethod]
    public void Find_ExistingItem_ReturnsCorrectIndex()
    {
        SortableBindingList<Item> list = [new() { Name = "Alice" }, new() { Name = "Bob" }, new() { Name = "Charlie" }];

        IBindingList bindingList = list;
        PropertyDescriptor prop = TypeDescriptor.GetProperties(typeof(Item))[nameof(Item.Name)]!;

        Assert.AreEqual(1, bindingList.Find(prop, "Bob"));
    }

    [TestMethod]
    public void Find_NonExistingItem_ReturnsNegativeOne()
    {
        SortableBindingList<Item> list = [new() { Name = "Alice" }];

        IBindingList bindingList = list;
        PropertyDescriptor prop = TypeDescriptor.GetProperties(typeof(Item))[nameof(Item.Name)]!;

        Assert.AreEqual(-1, bindingList.Find(prop, "Zoe"));
    }

    [TestMethod]
    public void RemoveSort_ClearsSortState()
    {
        SortableBindingList<Item> list = [new() { Name = "B" }, new() { Name = "A" }];

        IBindingList bindingList = list;
        PropertyDescriptor prop = TypeDescriptor.GetProperties(typeof(Item))[nameof(Item.Name)]!;
        bindingList.ApplySort(prop, ListSortDirection.Ascending);

        Assert.IsTrue(bindingList.IsSorted);

        bindingList.RemoveSort();

        Assert.IsFalse(bindingList.IsSorted);
    }

    [TestMethod]
    public void SupportsSearching_ReturnsTrue()
    {
        SortableBindingList<Item> list = [];
        IBindingList bindingList = list;

        Assert.IsTrue(bindingList.SupportsSearching);
    }

    [TestMethod]
    public void SupportsSorting_ReturnsTrue()
    {
        SortableBindingList<Item> list = [];
        IBindingList bindingList = list;

        Assert.IsTrue(bindingList.SupportsSorting);
    }
    #endregion

    sealed class Item
    {
        #region Public properties
        public string Name { get; set; } = string.Empty;

        public int Value { get; set; }
        #endregion
    }
}
