using Catharsis.ComponentModel.Observability;

namespace Catharsis.UnitTests.ComponentModel;

///<summary>
///Unit tests for the <see cref="ChangeSet"/> class.
///</summary>
[TestClass]
public class ChangeSetTests
{
    #region Public methods
    [TestMethod]
    public void AcceptAll_ClearsBothStacks()
    {
        ChangeSet set = new();
        set.Record("P", null, 1);
        set.Undo();
        set.AcceptAll();
        Assert.IsFalse(set.HasChanges);
        Assert.IsFalse(set.CanRedo);
    }

    [TestMethod]
    public void Clear_ResetsBothStacks()
    {
        ChangeSet set = new();
        set.Record("P", null, 1);
        set.Clear();
        Assert.AreEqual(0, set.Count);
    }

    [TestMethod]
    public void GetAll_ReturnsChronologicalOrder()
    {
        ChangeSet set = new();
        set.Record("A", null, 1);
        set.Record("B", null, 2);
        IReadOnlyList<ChangeEntry> all = set.GetAll();
        Assert.HasCount(2, all);
        Assert.AreEqual("A", all[0].PropertyName);
        Assert.AreEqual("B", all[1].PropertyName);
    }

    [TestMethod]
    public void GetByProperty_FiltersCorrectly()
    {
        ChangeSet set = new();
        set.Record("A", null, 1);
        set.Record("B", null, 2);
        set.Record("A", 1, 3);
        IReadOnlyList<ChangeEntry> aChanges = set.GetByProperty("A");
        Assert.HasCount(2, aChanges);
    }

    [TestMethod]
    public void Record_AddsEntry()
    {
        ChangeSet set = new();
        set.Record("Prop", "old", "new");
        Assert.AreEqual(1, set.Count);
        Assert.IsTrue(set.HasChanges);
        Assert.IsTrue(set.CanUndo);
    }

    [TestMethod]
    public void Record_ClearsRedoStack()
    {
        ChangeSet set = new();
        set.Record("P", "a", "b");
        set.Undo();
        Assert.IsTrue(set.CanRedo);
        set.Record("P", "c", "d");
        Assert.IsFalse(set.CanRedo);
    }

    [TestMethod]
    public void Redo_EmptyStack_ReturnsNull()
    {
        ChangeSet set = new();
        Assert.IsNull(set.Redo());
    }

    [TestMethod]
    public void Redo_RestoresEntry()
    {
        ChangeSet set = new();
        set.Record("Prop", "a", "b");
        set.Undo();
        ChangeEntry? entry = set.Redo();
        Assert.IsNotNull(entry);
        Assert.AreEqual(1, set.Count);
        Assert.IsFalse(set.CanRedo);
    }

    [TestMethod]
    public void Undo_EmptyStack_ReturnsNull()
    {
        ChangeSet set = new();
        Assert.IsNull(set.Undo());
    }

    [TestMethod]
    public void Undo_MovesToRedoStack()
    {
        ChangeSet set = new();
        set.Record("Prop", "a", "b");
        ChangeEntry? entry = set.Undo();
        Assert.IsNotNull(entry);
        Assert.AreEqual("Prop", entry.PropertyName);
        Assert.AreEqual(0, set.Count);
        Assert.IsTrue(set.CanRedo);
    }
    #endregion
}
