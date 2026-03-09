using System.ComponentModel.DataAnnotations;
using Catharsis.ComponentModel.DTO;

namespace Catharsis.UnitTests.ComponentModel.DTO;

///<summary>
///Unit tests for the <see cref="ValidatedRecord"/> class.
///</summary>
[TestClass]
public sealed class ValidatedRecordTests
{
    #region Public methods
    [TestMethod]
    public void Constructor_NullValue_ThrowsArgumentNullException() { Assert.ThrowsExactly<ArgumentNullException>(static () => new ValidatedRecord<PersonDto>(null!)); }
    [TestMethod]
    public void CurrentErrors_ReturnsReadOnlyDictionary()
    {
        ValidatedRecord<PersonDto> record = new ValidatedRecord<PersonDto>(new PersonDto { Name = null, Age = 30 });
        record.Validate();

        IReadOnlyDictionary<string, IReadOnlyList<string>> errors = record.CurrentErrors;

        Assert.IsTrue(errors.ContainsKey("Name"));
    }

    [TestMethod]
    public void ErrorsChanged_RaisedOnValidation()
    {
        ValidatedRecord<PersonDto> record = new ValidatedRecord<PersonDto>(new PersonDto { Name = "Alice", Age = 30 });
        bool raised = false;
        record.ErrorsChanged += (s, e) => raised = true;

        record.Value = new PersonDto { Name = null, Age = 30 };

        Assert.IsTrue(raised);
    }

    [TestMethod]
    public void GetErrors_NullOrEmpty_ReturnsAll()
    {
        ValidatedRecord<PersonDto> record = new ValidatedRecord<PersonDto>(new PersonDto { Name = null, Age = 200 });
        record.Validate();

        List<string> allErrors = [.. record.GetErrors(null).Cast<string>()];

        Assert.IsGreaterThanOrEqualTo(2, allErrors.Count);
    }

    [TestMethod]
    public void GetErrors_UnknownProperty_ReturnsEmpty()
    {
        ValidatedRecord<PersonDto> record = new ValidatedRecord<PersonDto>(new PersonDto { Name = "Alice", Age = 30 });
        record.Validate();

        List<string> errors = [.. record.GetErrors("Unknown").Cast<string>()];

        Assert.IsEmpty(errors);
    }

    [TestMethod]
    public void SettingValue_TriggersAutoValidation()
    {
        ValidatedRecord<PersonDto> record = new ValidatedRecord<PersonDto>(new PersonDto { Name = "Alice", Age = 30 });

        record.Value = new PersonDto { Name = null, Age = 200 };

        Assert.IsTrue(record.HasErrors);
    }

    [TestMethod]
    public void Validate_ClearsPreviousErrors()
    {
        ValidatedRecord<PersonDto> record = new ValidatedRecord<PersonDto>(new PersonDto { Name = null, Age = 30 });
        record.Validate();
        Assert.IsTrue(record.HasErrors);

        record.Value = new PersonDto { Name = "Alice", Age = 30 };
        // Setting Value triggers validation via OnPropertyChanged
        Assert.IsFalse(record.HasErrors);
    }

    [TestMethod]
    public void Validate_InvalidObject_ReturnsFalse()
    {
        ValidatedRecord<PersonDto> record = new ValidatedRecord<PersonDto>(new PersonDto { Name = null, Age = 200 });

        Assert.IsFalse(record.Validate());
        Assert.IsTrue(record.HasErrors);
    }

    [TestMethod]
    public void Validate_PopulatesErrors()
    {
        ValidatedRecord<PersonDto> record = new ValidatedRecord<PersonDto>(new PersonDto { Name = null, Age = 30 });

        record.Validate();

        List<string> errors = [.. record.GetErrors("Name").Cast<string>()];
        Assert.HasCount(1, errors);
        Assert.AreEqual("Name is required.", errors[0]);
    }

    [TestMethod]
    public void Validate_ValidObject_ReturnsTrue()
    {
        ValidatedRecord<PersonDto> record = new ValidatedRecord<PersonDto>(new PersonDto { Name = "Alice", Age = 30 });

        Assert.IsTrue(record.Validate());
        Assert.IsFalse(record.HasErrors);
    }
    #endregion

    sealed class PersonDto
    {
        #region Public properties
        [Range(0, 150, ErrorMessage = "Age must be between 0 and 150.")]
        public int Age { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        public string? Name { get; set; }
        #endregion
    }
}
