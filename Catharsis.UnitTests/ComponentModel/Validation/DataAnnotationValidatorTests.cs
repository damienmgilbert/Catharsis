using System.ComponentModel.DataAnnotations;
using Catharsis.ComponentModel.Validation;

namespace Catharsis.UnitTests.ComponentModel.Validation;

///<summary>
///Unit tests for the <see cref="DataAnnotationValidator"/> class.
///</summary>
[TestClass]
public sealed class DataAnnotationValidatorTests
{
    #region Public methods
    [TestMethod]
    public void ClearAll_ClearsAllErrors()
    {
        DataAnnotationValidator validator = new();
        validator.ValidateObject(new PersonModel { Name = null, Age = 200 });
        Assert.IsTrue(validator.HasErrors);

        validator.ClearAll();

        Assert.IsFalse(validator.HasErrors);
    }

    [TestMethod]
    public void Constructor_WithCustomContextFactory_UsesFactory()
    {
        ValidationContextFactory factory = new();
        DataAnnotationValidator validator = new(factory);

        Assert.IsNotNull(validator);
    }

    [TestMethod]
    public void ErrorsChanged_RaisedWhenErrorsChange()
    {
        DataAnnotationValidator validator = new();
        List<string> changedProperties = [];
        validator.ErrorsChanged += (s, e) => changedProperties.Add(e.PropertyName!);

        validator.ValidateObject(new PersonModel { Name = null, Age = 200 });

        Assert.IsNotEmpty(changedProperties);
    }

    [TestMethod]
    public void GetErrors_ReturnsErrorsFromDictionary()
    {
        DataAnnotationValidator validator = new();
        validator.ValidateObject(new PersonModel { Name = null, Age = 30 });

        List<ErrorInfo> errors = [.. validator.GetErrors("Name").Cast<ErrorInfo>()];

        Assert.HasCount(1, errors);
    }

    [TestMethod]
    public void Initial_State_HasNoErrors()
    {
        DataAnnotationValidator validator = new();

        Assert.IsFalse(validator.HasErrors);
        Assert.AreEqual(0, validator.Errors.TotalErrorCount);
    }

    [TestMethod]
    public void ValidateObject_ClearsPreviousErrors()
    {
        DataAnnotationValidator validator = new();

        validator.ValidateObject(new PersonModel { Name = null, Age = 30 });
        Assert.IsTrue(validator.HasErrors);

        validator.ValidateObject(new PersonModel { Name = "Alice", Age = 30 });
        Assert.IsFalse(validator.HasErrors);
    }

    [TestMethod]
    public void ValidateObject_InvalidModel_ReturnsFalse()
    {
        DataAnnotationValidator validator = new();
        PersonModel model = new() { Name = null, Age = 200 };

        bool result = validator.ValidateObject(model);

        Assert.IsFalse(result);
        Assert.IsTrue(validator.HasErrors);
    }

    [TestMethod]
    public void ValidateObject_NullInstance_ThrowsArgumentNullException()
    {
        DataAnnotationValidator validator = new();

        Assert.ThrowsExactly<ArgumentNullException>(() => validator.ValidateObject(null!));
    }

    [TestMethod]
    public void ValidateObject_PopulatesErrorDictionary()
    {
        DataAnnotationValidator validator = new();
        PersonModel model = new() { Name = null, Age = 30 };

        validator.ValidateObject(model);

        IReadOnlyList<ErrorInfo> nameErrors = validator.Errors.GetErrorInfos("Name");
        Assert.HasCount(1, nameErrors);
        Assert.AreEqual("Name is required.", nameErrors[0].Message);
    }

    [TestMethod]
    public void ValidateObject_ValidModel_ReturnsTrue()
    {
        DataAnnotationValidator validator = new();
        PersonModel model = new() { Name = "Alice", Age = 30 };

        bool result = validator.ValidateObject(model);

        Assert.IsTrue(result);
        Assert.IsFalse(validator.HasErrors);
    }

    [TestMethod]
    public void ValidateProperty_ClearsPreviousErrorsForThatProperty()
    {
        DataAnnotationValidator validator = new();
        PersonModel model = new() { Name = "Alice", Age = 30 };

        validator.ValidateProperty(model, "Name", null);
        Assert.IsTrue(validator.HasErrors);

        validator.ValidateProperty(model, "Name", "Alice");
        Assert.IsFalse(validator.HasErrors);
    }

    [TestMethod]
    public void ValidateProperty_InvalidProperty_ReturnsFalse()
    {
        DataAnnotationValidator validator = new();
        PersonModel model = new() { Name = null, Age = 30 };

        bool result = validator.ValidateProperty(model, "Name", null);

        Assert.IsFalse(result);
    }

    [TestMethod]
    public void ValidateProperty_NullInstance_ThrowsArgumentNullException()
    {
        DataAnnotationValidator validator = new();

        Assert.ThrowsExactly<ArgumentNullException>(() => validator.ValidateProperty(null!, "Name", "Alice"));
    }

    [TestMethod]
    public void ValidateProperty_NullPropertyName_ThrowsArgumentNullException()
    {
        DataAnnotationValidator validator = new();

        Assert.ThrowsExactly<ArgumentNullException>(() => validator.ValidateProperty(new PersonModel(), null!, "Alice"));
    }

    [TestMethod]
    public void ValidateProperty_ValidProperty_ReturnsTrue()
    {
        DataAnnotationValidator validator = new();
        PersonModel model = new() { Name = "Alice", Age = 30 };

        bool result = validator.ValidateProperty(model, "Name", "Alice");

        Assert.IsTrue(result);
    }
    #endregion

    sealed class PersonModel
    {
        #region Public properties
        [Range(0, 150, ErrorMessage = "Age must be between 0 and 150.")]
        public int Age { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        [StringLength(50, ErrorMessage = "Name must be 50 chars or fewer.")]
        public string? Name { get; set; }
        #endregion
    }
}
