using System.ComponentModel.DataAnnotations;
using Catharsis.ComponentModel.DTO;

namespace Catharsis.UnitTests.ComponentModel.DTO;

///<summary>
///Unit tests for the <see cref="BindableValidatedRecord"/> class.
///</summary>
[TestClass]
public sealed class BindableValidatedRecordTests
{
    #region Public methods
    [TestMethod]
    public void AcceptChanges_ResetsIsChanged()
    {
        BindableValidatedRecord<PersonDto> record = new BindableValidatedRecord<PersonDto>(new PersonDto { Name = "Alice", Age = 30 });
        record.Value = new PersonDto { Name = "Bob", Age = 25 };

        record.AcceptChanges();

        Assert.IsFalse(record.IsChanged);
    }

    [TestMethod]
    public void BeginEdit_CancelEdit_RevertsValue()
    {
        PersonDto original = new PersonDto { Name = "Alice", Age = 30 };
        BindableValidatedRecord<PersonDto> record = new BindableValidatedRecord<PersonDto>(original);
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
        BindableValidatedRecord<PersonDto> record = new BindableValidatedRecord<PersonDto>(new PersonDto { Name = "Alice", Age = 30 });
        record.BeginEdit();
        PersonDto newVal = new PersonDto { Name = "Bob", Age = 25 };
        record.Value = newVal;
        record.EndEdit();

        Assert.AreSame(newVal, record.Value);
        Assert.IsFalse(record.IsEditing);
    }

    [TestMethod]
    public void Constructor_NullValue_ThrowsArgumentNullException() { Assert.ThrowsExactly<ArgumentNullException>(static () => new BindableValidatedRecord<PersonDto>(null!)); }
    [TestMethod]
    public void CurrentErrors_ReturnsReadOnlyDictionary()
    {
        BindableValidatedRecord<PersonDto> record = new BindableValidatedRecord<PersonDto>(new PersonDto { Name = null, Age = 30 });
        record.Validate();

        Assert.IsTrue(record.CurrentErrors.ContainsKey("Name"));
    }

    [TestMethod]
    public void ErrorsChanged_Raised()
    {
        BindableValidatedRecord<PersonDto> record = new BindableValidatedRecord<PersonDto>(new PersonDto { Name = "Alice", Age = 30 });
        bool raised = false;
        record.ErrorsChanged += (s, e) => raised = true;

        record.Value = new PersonDto { Name = null, Age = 30 };

        Assert.IsTrue(raised);
    }

    [TestMethod]
    public void GetErrors_NullOrEmpty_ReturnsAll()
    {
        BindableValidatedRecord<PersonDto> record = new BindableValidatedRecord<PersonDto>(new PersonDto { Name = null, Age = 200 });
        record.Validate();

        List<string> all = [.. record.GetErrors(null).Cast<string>()];

        Assert.IsGreaterThanOrEqualTo(2, all.Count);
    }

    [TestMethod]
    public void GetErrors_ReturnsErrorsForProperty()
    {
        BindableValidatedRecord<PersonDto> record = new BindableValidatedRecord<PersonDto>(new PersonDto { Name = null, Age = 30 });
        record.Validate();

        List<string> errors = [.. record.GetErrors("Name").Cast<string>()];

        Assert.HasCount(1, errors);
    }

    [TestMethod]
    public void IsChanged_AfterValueChange_ReturnsTrue()
    {
        BindableValidatedRecord<PersonDto> record = new BindableValidatedRecord<PersonDto>(new PersonDto { Name = "Alice", Age = 30 });

        record.Value = new PersonDto { Name = "Bob", Age = 25 };

        Assert.IsTrue(record.IsChanged);
    }

    [TestMethod]
    public void IsChanged_InitiallyFalse()
    {
        BindableValidatedRecord<PersonDto> record = new BindableValidatedRecord<PersonDto>(new PersonDto { Name = "Alice", Age = 30 });

        Assert.IsFalse(record.IsChanged);
    }

    [TestMethod]
    public void RejectChanges_RevertsToAcceptedValue()
    {
        PersonDto original = new PersonDto { Name = "Alice", Age = 30 };
        BindableValidatedRecord<PersonDto> record = new BindableValidatedRecord<PersonDto>(original);
        record.Value = new PersonDto { Name = "Bob", Age = 25 };

        record.RejectChanges();

        Assert.AreSame(original, record.Value);
    }

    [TestMethod]
    public void SettingValue_TriggersAutoValidation()
    {
        BindableValidatedRecord<PersonDto> record = new BindableValidatedRecord<PersonDto>(new PersonDto { Name = "Alice", Age = 30 });

        record.Value = new PersonDto { Name = null, Age = 200 };

        Assert.IsTrue(record.HasErrors);
    }

    [TestMethod]
    public void Validate_InvalidObject_ReturnsFalse()
    {
        BindableValidatedRecord<PersonDto> record = new BindableValidatedRecord<PersonDto>(new PersonDto { Name = null, Age = 200 });

        Assert.IsFalse(record.Validate());
        Assert.IsTrue(record.HasErrors);
    }

    [TestMethod]
    public void Validate_ValidObject_ReturnsTrue()
    {
        BindableValidatedRecord<PersonDto> record = new BindableValidatedRecord<PersonDto>(new PersonDto { Name = "Alice", Age = 30 });

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
