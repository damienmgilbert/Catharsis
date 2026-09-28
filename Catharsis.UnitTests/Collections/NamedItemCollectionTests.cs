using Catharsis.Collections;

namespace Catharsis.UnitTests.Collections;

///<summary>
///Unit tests for the <see cref="NamedItemCollection{T}"/> class.
///</summary>
[TestClass]
public class NamedItemCollectionTests
{
    #region Add / lookup

    [TestMethod]
    public void Add_Item_CanBeRetrievedByName()
    {
        NamedItemCollection<Widget> collection = [];
        Widget widget = new("alpha");

        collection.Add(widget);

        Assert.AreSame(widget, collection["alpha"]);
    }

    [TestMethod]
    public void Add_DuplicateName_Throws()
    {
        NamedItemCollection<Widget> collection = [new Widget("alpha")];
        Assert.ThrowsExactly<ArgumentException>(() => collection.Add(new Widget("alpha")));
    }

    [TestMethod]
    public void Contains_ByName_ReturnsTrueWhenPresent()
    {
        NamedItemCollection<Widget> collection = [new Widget("alpha")];
        Assert.IsTrue(collection.Contains("alpha"));
    }

    [TestMethod]
    public void Contains_ByName_ReturnsFalseWhenAbsent()
    {
        NamedItemCollection<Widget> collection = [];
        Assert.IsFalse(collection.Contains("alpha"));
    }

    [TestMethod]
    public void TryGetValue_PresentName_ReturnsTrueAndItem()
    {
        NamedItemCollection<Widget> collection = [];
        Widget widget = new("alpha");
        collection.Add(widget);

        bool found = collection.TryGetValue("alpha", out Widget? result);

        Assert.IsTrue(found);
        Assert.AreSame(widget, result);
    }

    [TestMethod]
    public void TryGetValue_AbsentName_ReturnsFalse()
    {
        NamedItemCollection<Widget> collection = [];
        Assert.IsFalse(collection.TryGetValue("alpha", out _));
    }

    #endregion

    #region Remove

    [TestMethod]
    public void Remove_ByName_RemovesItem()
    {
        NamedItemCollection<Widget> collection = [new Widget("alpha")];

        bool removed = collection.Remove("alpha");

        Assert.IsTrue(removed);
        Assert.IsFalse(collection.Contains("alpha"));
    }

    #endregion

    #region Comparer

    [TestMethod]
    public void Constructor_CaseInsensitiveComparer_TreatsNamesAsEquivalent()
    {
        NamedItemCollection<Widget> collection = new(StringComparer.OrdinalIgnoreCase) { new Widget("Alpha") };
        Assert.IsTrue(collection.Contains("alpha"));
    }

    #endregion

    #region Test types
    sealed class Widget(string name) : INamedItem
    {
        public string Name { get; } = name;
    }
    #endregion
}
