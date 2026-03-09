using Catharsis.ComponentModel.DTO;

namespace Catharsis.UnitTests.ComponentModel.DTO;

///<summary>
///Unit tests for the <see cref="EditableRecord"/> class.
///</summary>
[TestClass]
public sealed class EditableRecordTests
{
    #region Public methods
    [TestMethod]
    public void AcceptChanges_RaisesPropertyChanged_IsChanged()
    {
        EditableRecord<PersonRecord> record = new EditableRecord<PersonRecord>(new PersonRecord("Alice", 30));
        record.Value = new PersonRecord("Bob", 25);
        string? changedProp = null;
        record.PropertyChanged += (s, e) => changedProp = e.PropertyName;

        record.AcceptChanges();

        Assert.AreEqual("IsChanged", changedProp);
    }

    [TestMethod]
    public void AcceptChanges_ResetsIsChanged()
    {
        EditableRecord<PersonRecord> record = new EditableRecord<PersonRecord>(new PersonRecord("Alice", 30));
        record.Value = new PersonRecord("Bob", 25);
        Assert.IsTrue(record.IsChanged);

        record.AcceptChanges();

        Assert.IsFalse(record.IsChanged);
    }

    [TestMethod]
    public void BeginEdit_CalledTwice_DoesNotThrow()
    {
        EditableRecord<PersonRecord> record = new EditableRecord<PersonRecord>(new PersonRecord("Alice", 30));

        record.BeginEdit();
        record.BeginEdit();

        Assert.IsTrue(record.IsEditing);
    }

    [TestMethod]
    public void BeginEdit_SetsIsEditingTrue()
    {
        EditableRecord<PersonRecord> record = new EditableRecord<PersonRecord>(new PersonRecord("Alice", 30));

        record.BeginEdit();

        Assert.IsTrue(record.IsEditing);
    }

    [TestMethod]
    public void CancelEdit_RevertsToSnapshot()
    {
        PersonRecord original = new PersonRecord("Alice", 30);
        EditableRecord<PersonRecord> record = new EditableRecord<PersonRecord>(original);
        record.BeginEdit();
        record.Value = new PersonRecord("Bob", 25);

        record.CancelEdit();

        Assert.AreSame(original, record.Value);
        Assert.IsFalse(record.IsEditing);
    }

    [TestMethod]
    public void CancelEdit_WithoutBeginEdit_DoesNothing()
    {
        PersonRecord original = new PersonRecord("Alice", 30);
        EditableRecord<PersonRecord> record = new EditableRecord<PersonRecord>(original);

        record.CancelEdit();

        Assert.AreSame(original, record.Value);
        Assert.IsFalse(record.IsEditing);
    }

    [TestMethod]
    public void Constructor_NullValue_ThrowsArgumentNullException() { Assert.ThrowsExactly<ArgumentNullException>(static () => new EditableRecord<PersonRecord>(null!)); }
    [TestMethod]
    public void EndEdit_CommitsChanges()
    {
        EditableRecord<PersonRecord> record = new EditableRecord<PersonRecord>(new PersonRecord("Alice", 30));
        record.BeginEdit();
        PersonRecord newValue = new PersonRecord("Bob", 25);
        record.Value = newValue;

        record.EndEdit();

        Assert.AreSame(newValue, record.Value);
        Assert.IsFalse(record.IsEditing);
    }

    [TestMethod]
    public void EndEdit_WithoutBeginEdit_DoesNothing()
    {
        EditableRecord<PersonRecord> record = new EditableRecord<PersonRecord>(new PersonRecord("Alice", 30));

        record.EndEdit();

        Assert.IsFalse(record.IsEditing);
    }

    [TestMethod]
    public void FullEditCycle_BeginEditCancelEdit()
    {
        PersonRecord original = new PersonRecord("Alice", 30);
        EditableRecord<PersonRecord> record = new EditableRecord<PersonRecord>(original);

        record.BeginEdit();
        record.Value = new PersonRecord("Bob", 25);
        record.CancelEdit();

        Assert.AreSame(original, record.Value);
        Assert.IsFalse(record.IsEditing);
    }

    [TestMethod]
    public void FullEditCycle_BeginEditEndEdit()
    {
        PersonRecord original = new PersonRecord("Alice", 30);
        EditableRecord<PersonRecord> record = new EditableRecord<PersonRecord>(original);

        record.BeginEdit();
        record.Value = new PersonRecord("Bob", 25);
        record.EndEdit();

        Assert.AreEqual("Bob", record.Value.Name);
        Assert.IsFalse(record.IsEditing);
    }

    [TestMethod]
    public void IsChanged_AfterValueChange_ReturnsTrue()
    {
        EditableRecord<PersonRecord> record = new EditableRecord<PersonRecord>(new PersonRecord("Alice", 30));

        record.Value = new PersonRecord("Bob", 25);

        Assert.IsTrue(record.IsChanged);
    }

    [TestMethod]
    public void IsChanged_InitiallyFalse()
    {
        EditableRecord<PersonRecord> record = new EditableRecord<PersonRecord>(new PersonRecord("Alice", 30));

        Assert.IsFalse(record.IsChanged);
    }

    [TestMethod]
    public void IsEditing_InitiallyFalse()
    {
        EditableRecord<PersonRecord> record = new EditableRecord<PersonRecord>(new PersonRecord("Alice", 30));

        Assert.IsFalse(record.IsEditing);
    }

    [TestMethod]
    public void RejectChanges_RaisesPropertyChanged_IsChanged()
    {
        EditableRecord<PersonRecord> record = new EditableRecord<PersonRecord>(new PersonRecord("Alice", 30));
        record.Value = new PersonRecord("Bob", 25);
        List<string> changedProps = new List<string>();
        record.PropertyChanged += (s, e) => changedProps.Add(e.PropertyName!);

        record.RejectChanges();

        CollectionAssert.Contains(changedProps, "IsChanged");
    }

    [TestMethod]
    public void RejectChanges_RevertsToAcceptedValue()
    {
        PersonRecord original = new PersonRecord("Alice", 30);
        EditableRecord<PersonRecord> record = new EditableRecord<PersonRecord>(original);
        record.Value = new PersonRecord("Bob", 25);

        record.RejectChanges();

        Assert.AreSame(original, record.Value);
    }
    #endregion

    sealed record PersonRecord(string Name, int Age);
}
