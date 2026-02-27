using System.ComponentModel;

using Catharsis.ComponentModel.Licensing;

namespace Catharsis.UnitTests.ComponentModel.Licensing;

[TestClass]
public sealed class ComponentActionListTests
{
    [TestMethod]
    public void Constructor_NullContext_ThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(
            () => new TestActionList(null!));
    }

    [TestMethod]
    public void Count_Initial_ReturnsZero()
    {
        var list = CreateActionList();

        Assert.AreEqual(0, list.Count);
    }

    [TestMethod]
    public void AddVerb_IncreasesCount()
    {
        var list = CreateActionList();

        list.ExposeAddVerb(new ComponentVerb("Do", () => { }));

        Assert.AreEqual(1, list.Count);
    }

    [TestMethod]
    public void AddVerb_TextAndAction_AddsVerb()
    {
        var list = CreateActionList();

        list.ExposeAddVerb("Reset", () => { }, "Reset everything");

        Assert.AreEqual(1, list.Count);
        Assert.AreEqual("Reset", list[0].Text);
        Assert.AreEqual("Reset everything", list[0].Description);
    }

    [TestMethod]
    public void Indexer_ReturnsCorrectVerb()
    {
        var list = CreateActionList();
        list.ExposeAddVerb(new ComponentVerb("First", () => { }));
        list.ExposeAddVerb(new ComponentVerb("Second", () => { }));

        Assert.AreEqual("First", list[0].Text);
        Assert.AreEqual("Second", list[1].Text);
    }

    [TestMethod]
    public void ClearVerbs_RemovesAll()
    {
        var list = CreateActionList();
        list.ExposeAddVerb(new ComponentVerb("Do", () => { }));

        list.ExposeClearVerbs();

        Assert.AreEqual(0, list.Count);
    }

    [TestMethod]
    public void Enumeration_ReturnsAllVerbs()
    {
        var list = CreateActionList();
        list.ExposeAddVerb(new ComponentVerb("A", () => { }));
        list.ExposeAddVerb(new ComponentVerb("B", () => { }));

        var names = list.Select(v => v.Text).ToList();

        Assert.AreEqual(2, names.Count);
        CollectionAssert.Contains(names, "A");
        CollectionAssert.Contains(names, "B");
    }

    private static TestActionList CreateActionList()
    {
        var context = new ComponentDesignContext(new StubComponent());
        return new TestActionList(context);
    }

    private sealed class TestActionList(ComponentDesignContext context)
        : ComponentActionList(context)
    {
        public void ExposeAddVerb(ComponentVerb verb) => AddVerb(verb);
        public void ExposeAddVerb(string text, Action action, string? description = null) =>
            AddVerb(text, action, description);
        public void ExposeClearVerbs() => ClearVerbs();
    }

    private sealed class StubComponent : IComponent
    {
        public ISite? Site { get; set; }
        public event EventHandler? Disposed;
        public void Dispose() => Disposed?.Invoke(this, EventArgs.Empty);
    }
}
