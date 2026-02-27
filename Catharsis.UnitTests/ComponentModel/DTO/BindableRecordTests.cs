using Catharsis.ComponentModel.DTO;

namespace Catharsis.UnitTests.ComponentModel.DTO;

[TestClass]
public sealed class BindableRecordTests
{
    private sealed record PersonRecord(string Name, int Age);

    [TestMethod]
    public void Constructor_NullValue_ThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(
            () => new BindableRecord<PersonRecord>(null!));
    }

    [TestMethod]
    public void Constructor_SetsValue()
    {
        var record = new PersonRecord("Alice", 30);
        var bindable = new BindableRecord<PersonRecord>(record);

        Assert.AreSame(record, bindable.Value);
    }

    [TestMethod]
    public void Value_Set_NullValue_ThrowsArgumentNullException()
    {
        var bindable = new BindableRecord<PersonRecord>(new PersonRecord("Alice", 30));

        Assert.ThrowsExactly<ArgumentNullException>(() => bindable.Value = null!);
    }

    [TestMethod]
    public void Value_Set_SameReference_NoNotification()
    {
        var record = new PersonRecord("Alice", 30);
        var bindable = new BindableRecord<PersonRecord>(record);
        var raised = false;
        bindable.PropertyChanged += (s, e) => raised = true;

        bindable.Value = record;

        Assert.IsFalse(raised);
    }

    [TestMethod]
    public void Value_Set_DifferentValue_RaisesPropertyChanged()
    {
        var bindable = new BindableRecord<PersonRecord>(new PersonRecord("Alice", 30));
        string? changedProp = null;
        bindable.PropertyChanged += (s, e) => changedProp = e.PropertyName;

        bindable.Value = new PersonRecord("Bob", 25);

        Assert.AreEqual("Value", changedProp);
    }

    [TestMethod]
    public void Value_Set_DifferentValue_RaisesPropertyChanging()
    {
        var bindable = new BindableRecord<PersonRecord>(new PersonRecord("Alice", 30));
        string? changingProp = null;
        bindable.PropertyChanging += (s, e) => changingProp = e.PropertyName;

        bindable.Value = new PersonRecord("Bob", 25);

        Assert.AreEqual("Value", changingProp);
    }

    [TestMethod]
    public void Update_NullTransform_ThrowsArgumentNullException()
    {
        var bindable = new BindableRecord<PersonRecord>(new PersonRecord("Alice", 30));

        Assert.ThrowsExactly<ArgumentNullException>(() => bindable.Update(null!));
    }

    [TestMethod]
    public void Update_TransformsAndSetsValue()
    {
        var bindable = new BindableRecord<PersonRecord>(new PersonRecord("Alice", 30));

        var result = bindable.Update(r => r with { Age = 31 });

        Assert.AreEqual(31, result.Age);
        Assert.AreEqual(31, bindable.Value.Age);
    }

    [TestMethod]
    public void Update_RaisesPropertyChanged()
    {
        var bindable = new BindableRecord<PersonRecord>(new PersonRecord("Alice", 30));
        var raised = false;
        bindable.PropertyChanged += (s, e) => raised = true;

        bindable.Update(r => r with { Name = "Bob" });

        Assert.IsTrue(raised);
    }

    [TestMethod]
    public void ToString_ReturnsValueToString()
    {
        var record = new PersonRecord("Alice", 30);
        var bindable = new BindableRecord<PersonRecord>(record);

        Assert.AreEqual(record.ToString(), bindable.ToString());
    }
}
