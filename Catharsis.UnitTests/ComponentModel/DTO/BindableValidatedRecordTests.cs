using System.ComponentModel.DataAnnotations;

using Catharsis.ComponentModel.DTO;

namespace Catharsis.UnitTests.ComponentModel.DTO;

[TestClass]
public sealed class BindableValidatedRecordTests
{
    private sealed class PersonDto
    {
        [Required(ErrorMessage = "Name is required.")]
        public string? Name { get; set; }

        [Range(0, 150, ErrorMessage = "Age must be between 0 and 150.")]
        public int Age { get; set; }
    }

    [TestMethod]
    public void Constructor_NullValue_ThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(
            () => new BindableValidatedRecord<PersonDto>(null!));
    }

    [TestMethod]
    public void Validate_ValidObject_ReturnsTrue()
    {
        var record = new BindableValidatedRecord<PersonDto>(
            new PersonDto { Name = "Alice", Age = 30 });

        Assert.IsTrue(record.Validate());
        Assert.IsFalse(record.HasErrors);
    }

    [TestMethod]
    public void Validate_InvalidObject_ReturnsFalse()
    {
        var record = new BindableValidatedRecord<PersonDto>(
            new PersonDto { Name = null, Age = 200 });

        Assert.IsFalse(record.Validate());
        Assert.IsTrue(record.HasErrors);
    }

    [TestMethod]
    public void SettingValue_TriggersAutoValidation()
    {
        var record = new BindableValidatedRecord<PersonDto>(
            new PersonDto { Name = "Alice", Age = 30 });

        record.Value = new PersonDto { Name = null, Age = 200 };

        Assert.IsTrue(record.HasErrors);
    }

    [TestMethod]
    public void GetErrors_ReturnsErrorsForProperty()
    {
        var record = new BindableValidatedRecord<PersonDto>(
            new PersonDto { Name = null, Age = 30 });
        record.Validate();

        var errors = record.GetErrors("Name").Cast<string>().ToList();

        Assert.AreEqual(1, errors.Count);
    }

    [TestMethod]
    public void GetErrors_NullOrEmpty_ReturnsAll()
    {
        var record = new BindableValidatedRecord<PersonDto>(
            new PersonDto { Name = null, Age = 200 });
        record.Validate();

        var all = record.GetErrors(null).Cast<string>().ToList();

        Assert.IsTrue(all.Count >= 2);
    }

    [TestMethod]
    public void CurrentErrors_ReturnsReadOnlyDictionary()
    {
        var record = new BindableValidatedRecord<PersonDto>(
            new PersonDto { Name = null, Age = 30 });
        record.Validate();

        Assert.IsTrue(record.CurrentErrors.ContainsKey("Name"));
    }

    [TestMethod]
    public void ErrorsChanged_Raised()
    {
        var record = new BindableValidatedRecord<PersonDto>(
            new PersonDto { Name = "Alice", Age = 30 });
        var raised = false;
        record.ErrorsChanged += (s, e) => raised = true;

        record.Value = new PersonDto { Name = null, Age = 30 };

        Assert.IsTrue(raised);
    }

    [TestMethod]
    public void BeginEdit_CancelEdit_RevertsValue()
    {
        var original = new PersonDto { Name = "Alice", Age = 30 };
        var record = new BindableValidatedRecord<PersonDto>(original);
        record.BeginEdit();
        Assert.IsTrue(record.IsEditing);

        record.Value = new PersonDto { Name = "Bob", Age = 25 };
        record.CancelEdit();

        Assert.AreSame(original, record.Value);
        Assert.IsFalse(record.IsEditing);
    }

    [TestMethod]
    public void BeginEdit_EndEdit_CommitsValue()
    {
        var record = new BindableValidatedRecord<PersonDto>(
            new PersonDto { Name = "Alice", Age = 30 });
        record.BeginEdit();
        var newVal = new PersonDto { Name = "Bob", Age = 25 };
        record.Value = newVal;
        record.EndEdit();

        Assert.AreSame(newVal, record.Value);
        Assert.IsFalse(record.IsEditing);
    }

    [TestMethod]
    public void IsChanged_InitiallyFalse()
    {
        var record = new BindableValidatedRecord<PersonDto>(
            new PersonDto { Name = "Alice", Age = 30 });

        Assert.IsFalse(record.IsChanged);
    }

    [TestMethod]
    public void IsChanged_AfterValueChange_ReturnsTrue()
    {
        var record = new BindableValidatedRecord<PersonDto>(
            new PersonDto { Name = "Alice", Age = 30 });

        record.Value = new PersonDto { Name = "Bob", Age = 25 };

        Assert.IsTrue(record.IsChanged);
    }

    [TestMethod]
    public void AcceptChanges_ResetsIsChanged()
    {
        var record = new BindableValidatedRecord<PersonDto>(
            new PersonDto { Name = "Alice", Age = 30 });
        record.Value = new PersonDto { Name = "Bob", Age = 25 };

        record.AcceptChanges();

        Assert.IsFalse(record.IsChanged);
    }

    [TestMethod]
    public void RejectChanges_RevertsToAcceptedValue()
    {
        var original = new PersonDto { Name = "Alice", Age = 30 };
        var record = new BindableValidatedRecord<PersonDto>(original);
        record.Value = new PersonDto { Name = "Bob", Age = 25 };

        record.RejectChanges();

        Assert.AreSame(original, record.Value);
    }
}
