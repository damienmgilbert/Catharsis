using Catharsis.ComponentModel.DTO;

namespace Catharsis.UnitTests.ComponentModel.DTO;

[TestClass]
public sealed class EditableRecordTests
{
    private sealed record PersonRecord(string Name, int Age);

    [TestMethod]
    public void Constructor_NullValue_ThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(
            () => new EditableRecord<PersonRecord>(null!));
    }

    [TestMethod]
    public void IsChanged_InitiallyFalse()
    {
        var record = new EditableRecord<PersonRecord>(new PersonRecord("Alice", 30));

        Assert.IsFalse(record.IsChanged);
    }

    [TestMethod]
    public void IsChanged_AfterValueChange_ReturnsTrue()
    {
        var record = new EditableRecord<PersonRecord>(new PersonRecord("Alice", 30));

        record.Value = new PersonRecord("Bob", 25);

        Assert.IsTrue(record.IsChanged);
    }

    [TestMethod]
    public void IsEditing_InitiallyFalse()
    {
        var record = new EditableRecord<PersonRecord>(new PersonRecord("Alice", 30));

        Assert.IsFalse(record.IsEditing);
    }

    [TestMethod]
    public void BeginEdit_SetsIsEditingTrue()
    {
        var record = new EditableRecord<PersonRecord>(new PersonRecord("Alice", 30));

        record.BeginEdit();

        Assert.IsTrue(record.IsEditing);
    }

    [TestMethod]
    public void BeginEdit_CalledTwice_DoesNotThrow()
    {
        var record = new EditableRecord<PersonRecord>(new PersonRecord("Alice", 30));

        record.BeginEdit();
        record.BeginEdit();

        Assert.IsTrue(record.IsEditing);
    }

    [TestMethod]
    public void CancelEdit_RevertsToSnapshot()
    {
        var original = new PersonRecord("Alice", 30);
        var record = new EditableRecord<PersonRecord>(original);
        record.BeginEdit();
        record.Value = new PersonRecord("Bob", 25);

        record.CancelEdit();

        Assert.AreSame(original, record.Value);
        Assert.IsFalse(record.IsEditing);
    }

    [TestMethod]
    public void CancelEdit_WithoutBeginEdit_DoesNothing()
    {
        var original = new PersonRecord("Alice", 30);
        var record = new EditableRecord<PersonRecord>(original);

        record.CancelEdit();

        Assert.AreSame(original, record.Value);
        Assert.IsFalse(record.IsEditing);
    }

    [TestMethod]
    public void EndEdit_CommitsChanges()
    {
        var record = new EditableRecord<PersonRecord>(new PersonRecord("Alice", 30));
        record.BeginEdit();
        var newValue = new PersonRecord("Bob", 25);
        record.Value = newValue;

        record.EndEdit();

        Assert.AreSame(newValue, record.Value);
        Assert.IsFalse(record.IsEditing);
    }

    [TestMethod]
    public void EndEdit_WithoutBeginEdit_DoesNothing()
    {
        var record = new EditableRecord<PersonRecord>(new PersonRecord("Alice", 30));

        record.EndEdit();

        Assert.IsFalse(record.IsEditing);
    }

    [TestMethod]
    public void AcceptChanges_ResetsIsChanged()
    {
        var record = new EditableRecord<PersonRecord>(new PersonRecord("Alice", 30));
        record.Value = new PersonRecord("Bob", 25);
        Assert.IsTrue(record.IsChanged);

        record.AcceptChanges();

        Assert.IsFalse(record.IsChanged);
    }

    [TestMethod]
    public void RejectChanges_RevertsToAcceptedValue()
    {
        var original = new PersonRecord("Alice", 30);
        var record = new EditableRecord<PersonRecord>(original);
        record.Value = new PersonRecord("Bob", 25);

        record.RejectChanges();

        Assert.AreSame(original, record.Value);
    }

    [TestMethod]
    public void AcceptChanges_RaisesPropertyChanged_IsChanged()
    {
        var record = new EditableRecord<PersonRecord>(new PersonRecord("Alice", 30));
        record.Value = new PersonRecord("Bob", 25);
        string? changedProp = null;
        record.PropertyChanged += (s, e) => changedProp = e.PropertyName;

        record.AcceptChanges();

        Assert.AreEqual("IsChanged", changedProp);
    }

    [TestMethod]
    public void RejectChanges_RaisesPropertyChanged_IsChanged()
    {
        var record = new EditableRecord<PersonRecord>(new PersonRecord("Alice", 30));
        record.Value = new PersonRecord("Bob", 25);
        var changedProps = new List<string>();
        record.PropertyChanged += (s, e) => changedProps.Add(e.PropertyName!);

        record.RejectChanges();

        CollectionAssert.Contains(changedProps, "IsChanged");
    }

    [TestMethod]
    public void FullEditCycle_BeginEditEndEdit()
    {
        var original = new PersonRecord("Alice", 30);
        var record = new EditableRecord<PersonRecord>(original);

        record.BeginEdit();
        record.Value = new PersonRecord("Bob", 25);
        record.EndEdit();

        Assert.AreEqual("Bob", record.Value.Name);
        Assert.IsFalse(record.IsEditing);
    }

    [TestMethod]
    public void FullEditCycle_BeginEditCancelEdit()
    {
        var original = new PersonRecord("Alice", 30);
        var record = new EditableRecord<PersonRecord>(original);

        record.BeginEdit();
        record.Value = new PersonRecord("Bob", 25);
        record.CancelEdit();

        Assert.AreSame(original, record.Value);
        Assert.IsFalse(record.IsEditing);
    }
}
