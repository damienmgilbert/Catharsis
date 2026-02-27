using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

using Catharsis.ComponentModel.DTO;

namespace Catharsis.UnitTests.ComponentModel.DTO;

[TestClass]
public sealed class ValidatedRecordTests
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
            () => new ValidatedRecord<PersonDto>(null!));
    }

    [TestMethod]
    public void Validate_ValidObject_ReturnsTrue()
    {
        var record = new ValidatedRecord<PersonDto>(new PersonDto { Name = "Alice", Age = 30 });

        Assert.IsTrue(record.Validate());
        Assert.IsFalse(record.HasErrors);
    }

    [TestMethod]
    public void Validate_InvalidObject_ReturnsFalse()
    {
        var record = new ValidatedRecord<PersonDto>(new PersonDto { Name = null, Age = 200 });

        Assert.IsFalse(record.Validate());
        Assert.IsTrue(record.HasErrors);
    }

    [TestMethod]
    public void Validate_PopulatesErrors()
    {
        var record = new ValidatedRecord<PersonDto>(new PersonDto { Name = null, Age = 30 });

        record.Validate();

        var errors = record.GetErrors("Name").Cast<string>().ToList();
        Assert.AreEqual(1, errors.Count);
        Assert.AreEqual("Name is required.", errors[0]);
    }

    [TestMethod]
    public void Validate_ClearsPreviousErrors()
    {
        var record = new ValidatedRecord<PersonDto>(new PersonDto { Name = null, Age = 30 });
        record.Validate();
        Assert.IsTrue(record.HasErrors);

        record.Value = new PersonDto { Name = "Alice", Age = 30 };
        // Setting Value triggers validation via OnPropertyChanged
        Assert.IsFalse(record.HasErrors);
    }

    [TestMethod]
    public void GetErrors_NullOrEmpty_ReturnsAll()
    {
        var record = new ValidatedRecord<PersonDto>(new PersonDto { Name = null, Age = 200 });
        record.Validate();

        var allErrors = record.GetErrors(null).Cast<string>().ToList();

        Assert.IsTrue(allErrors.Count >= 2);
    }

    [TestMethod]
    public void GetErrors_UnknownProperty_ReturnsEmpty()
    {
        var record = new ValidatedRecord<PersonDto>(new PersonDto { Name = "Alice", Age = 30 });
        record.Validate();

        var errors = record.GetErrors("Unknown").Cast<string>().ToList();

        Assert.AreEqual(0, errors.Count);
    }

    [TestMethod]
    public void CurrentErrors_ReturnsReadOnlyDictionary()
    {
        var record = new ValidatedRecord<PersonDto>(new PersonDto { Name = null, Age = 30 });
        record.Validate();

        var errors = record.CurrentErrors;

        Assert.IsTrue(errors.ContainsKey("Name"));
    }

    [TestMethod]
    public void ErrorsChanged_RaisedOnValidation()
    {
        var record = new ValidatedRecord<PersonDto>(new PersonDto { Name = "Alice", Age = 30 });
        var raised = false;
        record.ErrorsChanged += (s, e) => raised = true;

        record.Value = new PersonDto { Name = null, Age = 30 };

        Assert.IsTrue(raised);
    }

    [TestMethod]
    public void SettingValue_TriggersAutoValidation()
    {
        var record = new ValidatedRecord<PersonDto>(new PersonDto { Name = "Alice", Age = 30 });

        record.Value = new PersonDto { Name = null, Age = 200 };

        Assert.IsTrue(record.HasErrors);
    }
}
