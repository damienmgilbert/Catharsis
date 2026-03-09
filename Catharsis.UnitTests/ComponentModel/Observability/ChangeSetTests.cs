using Catharsis.ComponentModel.Observability;

namespace Catharsis.UnitTests.ComponentModel.Observability;

///<summary>
///Unit tests for the <see cref="ChangeSet"/> class.
///</summary>
[TestClass]
public sealed class ChangeSetTests
{
    [TestMethod]
    public void AcceptAll_ClearsBothStacks()
    {
        ChangeSet set = new ChangeSet();
        set.Record("A", "1", "2");
        set.Undo();
        Assert.IsTrue(set.CanRedo);

        set.Record("B", "3", "4");
        set.AcceptAll();

        Assert.AreEqual(0, set.Count);
        Assert.IsFalse(set.CanUndo);
        Assert.IsFalse(set.CanRedo);
    }

    [TestMethod]
    public void Clear_ClearsBothStacks()
    {
        ChangeSet set = new ChangeSet();
        set.Record("A", "1", "2");

        set.Clear();

        Assert.AreEqual(0, set.Count);
        Assert.IsFalse(set.HasChanges);
    }

    [TestMethod]
    public void GetAll_ReturnsChronologicalOrder()
    {
        ChangeSet set = new ChangeSet();
        set.Record("A", "1", "2");
        set.Record("B", "3", "4");

        IReadOnlyList<ChangeEntry> all = set.GetAll();

        Assert.HasCount(2, all);
        Assert.AreEqual("A", all[0].PropertyName);
        Assert.AreEqual("B", all[1].PropertyName);
    }

    [TestMethod]
    public void GetByProperty_FiltersCorrectly()
    {
        ChangeSet set = new ChangeSet();
        set.Record("A", "1", "2");
        set.Record("B", "3", "4");
        set.Record("A", "2", "5");

        IReadOnlyList<ChangeEntry> aChanges = set.GetByProperty("A");

        Assert.HasCount(2, aChanges);
    }

    [TestMethod]
    public void GetByProperty_NullPropertyName_ThrowsArgumentNullException()
    {
        ChangeSet set = new ChangeSet();

        Assert.ThrowsExactly<ArgumentNullException>(() => set.GetByProperty(null!));
    }

    [TestMethod]
    public void Initial_State_IsEmpty()
    {
        ChangeSet set = new ChangeSet();

        Assert.AreEqual(0, set.Count);
        Assert.IsFalse(set.HasChanges);
        Assert.IsFalse(set.CanUndo);
        Assert.IsFalse(set.CanRedo);
    }

    [TestMethod]
    public void Record_ByNameAndValues_NullPropertyName_ThrowsArgumentNullException()
    {
        ChangeSet set = new ChangeSet();

        Assert.ThrowsExactly<ArgumentNullException>(() => set.Record(null!, "old", "new"));
    }

    [TestMethod]
    public void Record_ClearsRedoStack()
    {
        ChangeSet set = new ChangeSet();
        set.Record("A", "1", "2");
        set.Undo();
        Assert.IsTrue(set.CanRedo);

        set.Record("B", "3", "4");

        Assert.IsFalse(set.CanRedo);
    }

    [TestMethod]
    public void Record_Entry_IncreasesCount()
    {
        ChangeSet set = new ChangeSet();

        set.Record(new ChangeEntry("Name", "old", "new"));

        Assert.AreEqual(1, set.Count);
        Assert.IsTrue(set.HasChanges);
        Assert.IsTrue(set.CanUndo);
    }

    [TestMethod]
    public void Record_NullEntry_ThrowsArgumentNullException()
    {
        ChangeSet set = new ChangeSet();

        Assert.ThrowsExactly<ArgumentNullException>(() => set.Record(null!));
    }

    [TestMethod]
    public void Redo_EmptyStack_ReturnsNull()
    {
        ChangeSet set = new ChangeSet();

        Assert.IsNull(set.Redo());
    }

    [TestMethod]
    public void Redo_ReturnsLastUndone_MovesToUndoStack()
    {
        ChangeSet set = new ChangeSet();
        set.Record("Name", "old", "new");
        set.Undo();

        ChangeEntry? entry = set.Redo();

        Assert.IsNotNull(entry);
        Assert.AreEqual("Name", entry.PropertyName);
        Assert.AreEqual(1, set.Count);
        Assert.IsFalse(set.CanRedo);
    }

    [TestMethod]
    public void Undo_EmptyStack_ReturnsNull()
    {
        ChangeSet set = new ChangeSet();

        Assert.IsNull(set.Undo());
    }

    [TestMethod]
    public void Undo_ReturnsLastEntry_MovesToRedoStack()
    {
        ChangeSet set = new ChangeSet();
        set.Record("Name", "old", "new");

        ChangeEntry? entry = set.Undo();

        Assert.IsNotNull(entry);
        Assert.AreEqual("Name", entry.PropertyName);
        Assert.AreEqual(0, set.Count);
        Assert.IsTrue(set.CanRedo);
    }
}
