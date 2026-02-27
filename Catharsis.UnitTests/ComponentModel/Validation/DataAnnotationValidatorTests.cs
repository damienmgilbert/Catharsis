using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

using Catharsis.ComponentModel.Validation;

namespace Catharsis.UnitTests.ComponentModel.Validation;

[TestClass]
public sealed class DataAnnotationValidatorTests
{
    private sealed class PersonModel
    {
        [Required(ErrorMessage = "Name is required.")]
        [StringLength(50, ErrorMessage = "Name must be 50 chars or fewer.")]
        public string? Name { get; set; }

        [Range(0, 150, ErrorMessage = "Age must be between 0 and 150.")]
        public int Age { get; set; }
    }

    [TestMethod]
    public void Initial_State_HasNoErrors()
    {
        var validator = new DataAnnotationValidator();

        Assert.IsFalse(validator.HasErrors);
        Assert.AreEqual(0, validator.Errors.TotalErrorCount);
    }

    [TestMethod]
    public void ValidateObject_ValidModel_ReturnsTrue()
    {
        var validator = new DataAnnotationValidator();
        var model = new PersonModel { Name = "Alice", Age = 30 };

        var result = validator.ValidateObject(model);

        Assert.IsTrue(result);
        Assert.IsFalse(validator.HasErrors);
    }

    [TestMethod]
    public void ValidateObject_InvalidModel_ReturnsFalse()
    {
        var validator = new DataAnnotationValidator();
        var model = new PersonModel { Name = null, Age = 200 };

        var result = validator.ValidateObject(model);

        Assert.IsFalse(result);
        Assert.IsTrue(validator.HasErrors);
    }

    [TestMethod]
    public void ValidateObject_NullInstance_ThrowsArgumentNullException()
    {
        var validator = new DataAnnotationValidator();

        Assert.ThrowsExactly<ArgumentNullException>(
            () => validator.ValidateObject(null!));
    }

    [TestMethod]
    public void ValidateObject_PopulatesErrorDictionary()
    {
        var validator = new DataAnnotationValidator();
        var model = new PersonModel { Name = null, Age = 30 };

        validator.ValidateObject(model);

        var nameErrors = validator.Errors.GetErrorInfos("Name");
        Assert.AreEqual(1, nameErrors.Count);
        Assert.AreEqual("Name is required.", nameErrors[0].Message);
    }

    [TestMethod]
    public void ValidateObject_ClearsPreviousErrors()
    {
        var validator = new DataAnnotationValidator();

        validator.ValidateObject(new PersonModel { Name = null, Age = 30 });
        Assert.IsTrue(validator.HasErrors);

        validator.ValidateObject(new PersonModel { Name = "Alice", Age = 30 });
        Assert.IsFalse(validator.HasErrors);
    }

    [TestMethod]
    public void ValidateProperty_ValidProperty_ReturnsTrue()
    {
        var validator = new DataAnnotationValidator();
        var model = new PersonModel { Name = "Alice", Age = 30 };

        var result = validator.ValidateProperty(model, "Name", "Alice");

        Assert.IsTrue(result);
    }

    [TestMethod]
    public void ValidateProperty_InvalidProperty_ReturnsFalse()
    {
        var validator = new DataAnnotationValidator();
        var model = new PersonModel { Name = null, Age = 30 };

        var result = validator.ValidateProperty(model, "Name", null);

        Assert.IsFalse(result);
    }

    [TestMethod]
    public void ValidateProperty_NullInstance_ThrowsArgumentNullException()
    {
        var validator = new DataAnnotationValidator();

        Assert.ThrowsExactly<ArgumentNullException>(
            () => validator.ValidateProperty(null!, "Name", "Alice"));
    }

    [TestMethod]
    public void ValidateProperty_NullPropertyName_ThrowsArgumentNullException()
    {
        var validator = new DataAnnotationValidator();

        Assert.ThrowsExactly<ArgumentNullException>(
            () => validator.ValidateProperty(new PersonModel(), null!, "Alice"));
    }

    [TestMethod]
    public void ValidateProperty_ClearsPreviousErrorsForThatProperty()
    {
        var validator = new DataAnnotationValidator();
        var model = new PersonModel { Name = "Alice", Age = 30 };

        validator.ValidateProperty(model, "Name", null);
        Assert.IsTrue(validator.HasErrors);

        validator.ValidateProperty(model, "Name", "Alice");
        Assert.IsFalse(validator.HasErrors);
    }

    [TestMethod]
    public void ErrorsChanged_RaisedWhenErrorsChange()
    {
        var validator = new DataAnnotationValidator();
        var changedProperties = new List<string>();
        validator.ErrorsChanged += (s, e) => changedProperties.Add(e.PropertyName!);

        validator.ValidateObject(new PersonModel { Name = null, Age = 200 });

        Assert.IsTrue(changedProperties.Count > 0);
    }

    [TestMethod]
    public void GetErrors_ReturnsErrorsFromDictionary()
    {
        var validator = new DataAnnotationValidator();
        validator.ValidateObject(new PersonModel { Name = null, Age = 30 });

        var errors = validator.GetErrors("Name").Cast<ErrorInfo>().ToList();

        Assert.AreEqual(1, errors.Count);
    }

    [TestMethod]
    public void ClearAll_ClearsAllErrors()
    {
        var validator = new DataAnnotationValidator();
        validator.ValidateObject(new PersonModel { Name = null, Age = 200 });
        Assert.IsTrue(validator.HasErrors);

        validator.ClearAll();

        Assert.IsFalse(validator.HasErrors);
    }

    [TestMethod]
    public void Constructor_WithCustomContextFactory_UsesFactory()
    {
        var factory = new ValidationContextFactory();
        var validator = new DataAnnotationValidator(factory);

        Assert.IsNotNull(validator);
    }
}
