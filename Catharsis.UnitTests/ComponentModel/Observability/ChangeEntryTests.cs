using Catharsis.ComponentModel.Observability;

namespace Catharsis.UnitTests.ComponentModel.Observability;

[TestClass]
public sealed class ChangeEntryTests
{
    [TestMethod]
    public void Constructor_SetsProperties()
    {
        var entry = new ChangeEntry("Name", "old", "new");

        Assert.AreEqual("Name", entry.PropertyName);
        Assert.AreEqual("old", entry.OldValue);
        Assert.AreEqual("new", entry.NewValue);
        Assert.IsNotNull(entry.Timestamp);
    }

    [TestMethod]
    public void Constructor_ExplicitTimestamp_UsesProvided()
    {
        var ts = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var entry = new ChangeEntry("X", null, null, ts);

        Assert.AreEqual(ts, entry.Timestamp);
    }

    [TestMethod]
    public void Constructor_NullTimestamp_DefaultsToUtcNow()
    {
        var before = DateTime.UtcNow;
        var entry = new ChangeEntry("Prop", "a", "b");
        var after = DateTime.UtcNow;

        Assert.IsTrue(entry.Timestamp >= before && entry.Timestamp <= after);
    }

    [TestMethod]
    public void ToString_ContainsPropertyNameAndValues()
    {
        var entry = new ChangeEntry("Name", "old", "new");
        var result = entry.ToString();

        Assert.IsTrue(result.Contains("Name"));
        Assert.IsTrue(result.Contains("old"));
        Assert.IsTrue(result.Contains("new"));
    }

    [TestMethod]
    public void Equality_SamePropertyNameAndValues_DifferentTimestamp_AreNotEqual()
    {
        var ts1 = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var ts2 = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var a = new ChangeEntry("P", "old", "new", ts1);
        var b = new ChangeEntry("P", "old", "new", ts2);

        Assert.AreNotEqual(a, b);
    }
}

[TestClass]
public sealed class ChangeSetTests
{
    [TestMethod]
    public void Initial_State_IsEmpty()
    {
        var set = new ChangeSet();

        Assert.AreEqual(0, set.Count);
        Assert.IsFalse(set.HasChanges);
        Assert.IsFalse(set.CanUndo);
        Assert.IsFalse(set.CanRedo);
    }

    [TestMethod]
    public void Record_Entry_IncreasesCount()
    {
        var set = new ChangeSet();

        set.Record(new ChangeEntry("Name", "old", "new"));

        Assert.AreEqual(1, set.Count);
        Assert.IsTrue(set.HasChanges);
        Assert.IsTrue(set.CanUndo);
    }

    [TestMethod]
    public void Record_NullEntry_ThrowsArgumentNullException()
    {
        var set = new ChangeSet();

        Assert.ThrowsExactly<ArgumentNullException>(() => set.Record((ChangeEntry)null!));
    }

    [TestMethod]
    public void Record_ByNameAndValues_NullPropertyName_ThrowsArgumentNullException()
    {
        var set = new ChangeSet();

        Assert.ThrowsExactly<ArgumentNullException>(() => set.Record(null!, "old", "new"));
    }

    [TestMethod]
    public void Record_ClearsRedoStack()
    {
        var set = new ChangeSet();
        set.Record("A", "1", "2");
        set.Undo();
        Assert.IsTrue(set.CanRedo);

        set.Record("B", "3", "4");

        Assert.IsFalse(set.CanRedo);
    }

    [TestMethod]
    public void Undo_ReturnsLastEntry_MovesToRedoStack()
    {
        var set = new ChangeSet();
        set.Record("Name", "old", "new");

        var entry = set.Undo();

        Assert.IsNotNull(entry);
        Assert.AreEqual("Name", entry.PropertyName);
        Assert.AreEqual(0, set.Count);
        Assert.IsTrue(set.CanRedo);
    }

    [TestMethod]
    public void Undo_EmptyStack_ReturnsNull()
    {
        var set = new ChangeSet();

        Assert.IsNull(set.Undo());
    }

    [TestMethod]
    public void Redo_ReturnsLastUndone_MovesToUndoStack()
    {
        var set = new ChangeSet();
        set.Record("Name", "old", "new");
        set.Undo();

        var entry = set.Redo();

        Assert.IsNotNull(entry);
        Assert.AreEqual("Name", entry.PropertyName);
        Assert.AreEqual(1, set.Count);
        Assert.IsFalse(set.CanRedo);
    }

    [TestMethod]
    public void Redo_EmptyStack_ReturnsNull()
    {
        var set = new ChangeSet();

        Assert.IsNull(set.Redo());
    }

    [TestMethod]
    public void GetAll_ReturnsChronologicalOrder()
    {
        var set = new ChangeSet();
        set.Record("A", "1", "2");
        set.Record("B", "3", "4");

        var all = set.GetAll();

        Assert.AreEqual(2, all.Count);
        Assert.AreEqual("A", all[0].PropertyName);
        Assert.AreEqual("B", all[1].PropertyName);
    }

    [TestMethod]
    public void GetByProperty_FiltersCorrectly()
    {
        var set = new ChangeSet();
        set.Record("A", "1", "2");
        set.Record("B", "3", "4");
        set.Record("A", "2", "5");

        var aChanges = set.GetByProperty("A");

        Assert.AreEqual(2, aChanges.Count);
    }

    [TestMethod]
    public void GetByProperty_NullPropertyName_ThrowsArgumentNullException()
    {
        var set = new ChangeSet();

        Assert.ThrowsExactly<ArgumentNullException>(() => set.GetByProperty(null!));
    }

    [TestMethod]
    public void AcceptAll_ClearsBothStacks()
    {
        var set = new ChangeSet();
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
        var set = new ChangeSet();
        set.Record("A", "1", "2");

        set.Clear();

        Assert.AreEqual(0, set.Count);
        Assert.IsFalse(set.HasChanges);
    }
}
