using Catharsis.ComponentModel.DTO;

namespace Catharsis.UnitTests.ComponentModel.DTO;

///<summary>
///Unit tests for the <see cref="BindableRecord"/> class.
///</summary>
[TestClass]
public sealed class BindableRecordTests
{
    #region Public methods
    [TestMethod]
    public void Constructor_NullValue_ThrowsArgumentNullException() { Assert.ThrowsExactly<ArgumentNullException>(static () => new BindableRecord<PersonRecord>(null!)); }
    [TestMethod]
    public void Constructor_SetsValue()
    {
        PersonRecord record = new PersonRecord("Alice", 30);
        BindableRecord<PersonRecord> bindable = new BindableRecord<PersonRecord>(record);

        Assert.AreSame(record, bindable.Value);
    }

    [TestMethod]
    public void ToString_ReturnsValueToString()
    {
        PersonRecord record = new PersonRecord("Alice", 30);
        BindableRecord<PersonRecord> bindable = new BindableRecord<PersonRecord>(record);

        Assert.AreEqual(record.ToString(), bindable.ToString());
    }

    [TestMethod]
    public void Update_NullTransform_ThrowsArgumentNullException()
    {
        BindableRecord<PersonRecord> bindable = new BindableRecord<PersonRecord>(new PersonRecord("Alice", 30));

        Assert.ThrowsExactly<ArgumentNullException>(() => bindable.Update(null!));
    }

    [TestMethod]
    public void Update_RaisesPropertyChanged()
    {
        BindableRecord<PersonRecord> bindable = new BindableRecord<PersonRecord>(new PersonRecord("Alice", 30));
        bool raised = false;
        bindable.PropertyChanged += (s, e) => raised = true;

        bindable.Update(r => r with { Name = "Bob" });

        Assert.IsTrue(raised);
    }

    [TestMethod]
    public void Update_TransformsAndSetsValue()
    {
        BindableRecord<PersonRecord> bindable = new BindableRecord<PersonRecord>(new PersonRecord("Alice", 30));

        PersonRecord result = bindable.Update(static r => r with { Age = 31 });

        Assert.AreEqual(31, result.Age);
        Assert.AreEqual(31, bindable.Value.Age);
    }

    [TestMethod]
    public void Value_Set_DifferentValue_RaisesPropertyChanged()
    {
        BindableRecord<PersonRecord> bindable = new BindableRecord<PersonRecord>(new PersonRecord("Alice", 30));
        string? changedProp = null;
        bindable.PropertyChanged += (s, e) => changedProp = e.PropertyName;

        bindable.Value = new PersonRecord("Bob", 25);

        Assert.AreEqual("Value", changedProp);
    }

    [TestMethod]
    public void Value_Set_DifferentValue_RaisesPropertyChanging()
    {
        BindableRecord<PersonRecord> bindable = new BindableRecord<PersonRecord>(new PersonRecord("Alice", 30));
        string? changingProp = null;
        bindable.PropertyChanging += (s, e) => changingProp = e.PropertyName;

        bindable.Value = new PersonRecord("Bob", 25);

        Assert.AreEqual("Value", changingProp);
    }

    [TestMethod]
    public void Value_Set_NullValue_ThrowsArgumentNullException()
    {
        BindableRecord<PersonRecord> bindable = new BindableRecord<PersonRecord>(new PersonRecord("Alice", 30));

        Assert.ThrowsExactly<ArgumentNullException>(() => bindable.Value = null!);
    }

    [TestMethod]
    public void Value_Set_SameReference_NoNotification()
    {
        PersonRecord record = new PersonRecord("Alice", 30);
        BindableRecord<PersonRecord> bindable = new BindableRecord<PersonRecord>(record);
        bool raised = false;
        bindable.PropertyChanged += (s, e) => raised = true;

        bindable.Value = record;

        Assert.IsFalse(raised);
    }
    #endregion

    sealed record PersonRecord(string Name, int Age);
}
