using System.ComponentModel;
using Catharsis.ComponentModel.Licensing;

namespace Catharsis.UnitTests.ComponentModel.Licensing;

[TestClass]
public sealed class ComponentActionListTests
{
    #region Private methods
    static TestActionList CreateActionList()
    {
        ComponentDesignContext context = new ComponentDesignContext(new StubComponent());
        return new TestActionList(context);
    }
    #endregion

    #region Public methods
    [TestMethod]
    public void AddVerb_IncreasesCount()
    {
        TestActionList list = CreateActionList();

        list.ExposeAddVerb(
        new ComponentVerb(
        "Do",
        () =>
        {
        }));

        Assert.HasCount(1, list);
    }

    [TestMethod]
    public void AddVerb_TextAndAction_AddsVerb()
    {
        TestActionList list = CreateActionList();

        list.ExposeAddVerb(
        "Reset",
        () =>
        {
        },
        "Reset everything");

        Assert.HasCount(1, list);
        Assert.AreEqual("Reset", list[0].Text);
        Assert.AreEqual("Reset everything", list[0].Description);
    }

    [TestMethod]
    public void ClearVerbs_RemovesAll()
    {
        TestActionList list = CreateActionList();
        list.ExposeAddVerb(
        new ComponentVerb(
        "Do",
        () =>
        {
        }));

        list.ExposeClearVerbs();

        Assert.IsEmpty(list);
    }

    [TestMethod]
    public void Constructor_NullContext_ThrowsArgumentNullException() { Assert.ThrowsExactly<ArgumentNullException>(() => new TestActionList(null!)); }
    [TestMethod]
    public void Count_Initial_ReturnsZero()
    {
        TestActionList list = CreateActionList();

        Assert.IsEmpty(list);
    }

    [TestMethod]
    public void Enumeration_ReturnsAllVerbs()
    {
        TestActionList list = CreateActionList();
        list.ExposeAddVerb(
        new ComponentVerb(
        "A",
        () =>
        {
        }));
        list.ExposeAddVerb(
        new ComponentVerb(
        "B",
        () =>
        {
        }));

        List<string> names = list.Select(v => v.Text).ToList();

        Assert.HasCount(2, names);
        CollectionAssert.Contains(names, "A");
        CollectionAssert.Contains(names, "B");
    }

    [TestMethod]
    public void Indexer_ReturnsCorrectVerb()
    {
        TestActionList list = CreateActionList();
        list.ExposeAddVerb(
        new ComponentVerb(
        "First",
        () =>
        {
        }));
        list.ExposeAddVerb(
        new ComponentVerb(
        "Second",
        () =>
        {
        }));

        Assert.AreEqual("First", list[0].Text);
        Assert.AreEqual("Second", list[1].Text);
    }
    #endregion

    sealed class TestActionList(ComponentDesignContext context) : ComponentActionList(context)
    {
        #region Public methods
        public void ExposeAddVerb(ComponentVerb verb) { AddVerb(verb); }
        public void ExposeAddVerb(string text, Action action, string? description = null) { AddVerb(text, action, description); }
        public void ExposeClearVerbs() { ClearVerbs(); }
        #endregion
    }

    sealed class StubComponent : IComponent
    {
        #region Events
        public event EventHandler? Disposed;
        #endregion

        #region Public methods
        public void Dispose() { Disposed?.Invoke(this, EventArgs.Empty); }
        #endregion

        #region Public properties
        public ISite? Site { get; set; }
        #endregion
    }
}
