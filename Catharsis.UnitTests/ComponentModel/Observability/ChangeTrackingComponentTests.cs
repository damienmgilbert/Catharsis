using System.ComponentModel;

using Catharsis.ComponentModel.Observability;

namespace Catharsis.UnitTests.ComponentModel.Observability;

[TestClass]
public sealed class ChangeTrackingComponentTests
{
    private sealed class TestComponent : ChangeTrackingComponent
    {
        private string? _name;
        private int _age;

        public string? Name
        {
            get => _name;
            set => SetTrackedProperty(ref _name, value);
        }

        public int Age
        {
            get => _age;
            set => SetTrackedProperty(ref _age, value);
        }
    }

    [TestMethod]
    public void Initial_State_NoChanges()
    {
        var c = new TestComponent();

        Assert.IsFalse(c.IsChanged);
        Assert.IsFalse(c.CanUndo);
        Assert.IsFalse(c.CanRedo);
    }

    [TestMethod]
    public void SetTrackedProperty_RecordsChange()
    {
        var c = new TestComponent();

        c.Name = "Alice";

        Assert.IsTrue(c.IsChanged);
        Assert.AreEqual(1, c.Changes.Count);
    }

    [TestMethod]
    public void SetTrackedProperty_SameValue_NoChange()
    {
        var c = new TestComponent { Name = "Alice" };
        c.AcceptChanges();

        c.Name = "Alice";

        Assert.IsFalse(c.IsChanged);
    }

    [TestMethod]
    public void SetTrackedProperty_RaisesPropertyChanged()
    {
        var c = new TestComponent();
        string? changedProp = null;
        c.PropertyChanged += (s, e) => changedProp = e.PropertyName;

        c.Name = "Bob";

        Assert.AreEqual("Name", changedProp);
    }

    [TestMethod]
    public void SetTrackedProperty_RaisesPropertyChanging()
    {
        var c = new TestComponent();
        string? changingProp = null;
        c.PropertyChanging += (s, e) => changingProp = e.PropertyName;

        c.Name = "Bob";

        Assert.AreEqual("Name", changingProp);
    }

    [TestMethod]
    public void Undo_RevertsToPreviousValue()
    {
        var c = new TestComponent();
        c.Name = "Alice";

        var entry = c.Undo();

        Assert.IsNotNull(entry);
        Assert.IsNull(c.Name);
        Assert.AreEqual("Name", entry.PropertyName);
    }

    [TestMethod]
    public void Undo_NoChanges_ReturnsNull()
    {
        var c = new TestComponent();

        Assert.IsNull(c.Undo());
    }

    [TestMethod]
    public void Redo_RestoresUndoneValue()
    {
        var c = new TestComponent();
        c.Name = "Alice";
        c.Undo();

        var entry = c.Redo();

        Assert.IsNotNull(entry);
        Assert.AreEqual("Alice", c.Name);
    }

    [TestMethod]
    public void Redo_NoUndone_ReturnsNull()
    {
        var c = new TestComponent();

        Assert.IsNull(c.Redo());
    }

    [TestMethod]
    public void AcceptChanges_ClearsHistory()
    {
        var c = new TestComponent();
        c.Name = "Alice";
        c.Age = 30;

        c.AcceptChanges();

        Assert.IsFalse(c.IsChanged);
        Assert.IsFalse(c.CanUndo);
    }

    [TestMethod]
    public void RejectChanges_RevertsAllChanges()
    {
        var c = new TestComponent();
        c.Name = "Alice";
        c.Age = 30;

        c.RejectChanges();

        Assert.IsNull(c.Name);
        Assert.AreEqual(0, c.Age);
        Assert.IsFalse(c.IsChanged);
    }

    [TestMethod]
    public void MultipleChanges_UndoRedo_PreservesOrder()
    {
        var c = new TestComponent();
        c.Name = "Alice";
        c.Age = 25;

        c.Undo(); // undo Age
        Assert.AreEqual(0, c.Age);
        Assert.AreEqual("Alice", c.Name);

        c.Undo(); // undo Name
        Assert.IsNull(c.Name);

        c.Redo(); // redo Name
        Assert.AreEqual("Alice", c.Name);
    }

    [TestMethod]
    public void Changes_GetAll_ReturnsChronologicalOrder()
    {
        var c = new TestComponent();
        c.Name = "Alice";
        c.Age = 30;

        var all = c.Changes.GetAll();

        Assert.AreEqual(2, all.Count);
        Assert.AreEqual("Name", all[0].PropertyName);
        Assert.AreEqual("Age", all[1].PropertyName);
    }
}
