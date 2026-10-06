using System.ComponentModel.DataAnnotations;
using Catharsis.DataAnnotations;

namespace Catharsis.UnitTests.DataAnnotations;

///<summary>
///Unit tests for the <see cref="CompositeValidator"/> class.
///</summary>
[TestClass]
public class CompositeValidatorTests
{
    #region Public methods
    [TestMethod]
    public void InvalidObject_ErrorMessagesPopulated()
    {
        ValidModel model = new() { Name = null!, Age = 25 };

        CompositeValidationResult result = CompositeValidator.ValidateObject(model);

        Assert.IsFalse(result.IsValid);
        Assert.IsTrue(result.ErrorMessages.Any());
    }

    [TestMethod]
    public void InvalidObject_MemberNamesPopulated()
    {
        ValidModel model = new() { Name = null!, Age = 25 };

        CompositeValidationResult result = CompositeValidator.ValidateObject(model);

        Assert.IsFalse(result.IsValid);
        Assert.IsTrue(result.MemberNames.Any());
    }

    [TestMethod]
    public void InvalidObject_ReturnsIsValidFalse()
    {
        ValidModel model = new() { Name = null!, Age = 200 };

        CompositeValidationResult result = CompositeValidator.ValidateObject(model);

        Assert.IsFalse(result.IsValid);
        Assert.IsNotEmpty(result.Results);
    }

    [TestMethod]
    public void ThrowIfInvalid_InvalidObject_ThrowsValidationException()
    {
        ValidModel model = new() { Name = null!, Age = 25 };

        CompositeValidationResult result = CompositeValidator.ValidateObject(model);

        Assert.ThrowsExactly<ValidationException>(() => result.ThrowIfInvalid());
    }

    [TestMethod]
    public void ThrowIfInvalid_ValidObject_DoesNotThrow()
    {
        ValidModel model = new() { Name = "Alice", Age = 25 };

        CompositeValidationResult result = CompositeValidator.ValidateObject(model);

        result.ThrowIfInvalid();
    }

    [TestMethod]
    public void ValidateObject_NullInstance_ThrowsArgumentNullException() { Assert.ThrowsExactly<ArgumentNullException>(static () => CompositeValidator.ValidateObject<ValidModel>(null!)); }
    [TestMethod]
    public void ValidateObject_WithIValidatableObject_RunsValidation()
    {
        SelfValidatingModel model = new() { Start = 10, End = 5 };

        CompositeValidationResult result = CompositeValidator.ValidateObject(model);

        Assert.IsFalse(result.IsValid);
        Assert.Contains(static m => m!.Contains("End must be greater"), result.ErrorMessages);
    }

    [TestMethod]
    public void ValidateObject_WithValidIValidatableObject_ReturnsSuccess()
    {
        SelfValidatingModel model = new() { Start = 5, End = 10 };

        CompositeValidationResult result = CompositeValidator.ValidateObject(model);

        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public void ValidateProperty_InvalidProperty_ReturnsFailure()
    {
        ValidModel model = new() { Name = null!, Age = 25 };

        CompositeValidationResult result = CompositeValidator.ValidateProperty(model, nameof(ValidModel.Name));

        Assert.IsFalse(result.IsValid);
    }

    [TestMethod]
    public void ValidateProperty_UnknownProperty_ThrowsArgumentException()
    {
        ValidModel model = new() { Name = "Alice", Age = 25 };

        Assert.ThrowsExactly<ArgumentException>(() => CompositeValidator.ValidateProperty(model, "NonExistent"));
    }

    [TestMethod]
    public void ValidateProperty_ValidProperty_ReturnsSuccess()
    {
        ValidModel model = new() { Name = "Alice", Age = 25 };

        CompositeValidationResult result = CompositeValidator.ValidateProperty(model, nameof(ValidModel.Name));

        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public void ValidateValue_InvalidValue_ReturnsFailure()
    {
        ValidationAttribute[] attributes = [new RequiredAttribute()];

        CompositeValidationResult result = CompositeValidator.ValidateValue(null, "TestField", attributes);

        Assert.IsFalse(result.IsValid);
    }

    [TestMethod]
    public void ValidateValue_ValidValue_ReturnsSuccess()
    {
        ValidationAttribute[] attributes = [new RequiredAttribute(), new StringLengthAttribute(50)];

        CompositeValidationResult result = CompositeValidator.ValidateValue("hello", "TestField", attributes);

        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public void ValidObject_ReturnsIsValidTrue()
    {
        ValidModel model = new() { Name = "Alice", Age = 25 };

        CompositeValidationResult result = CompositeValidator.ValidateObject(model);

        Assert.IsTrue(result.IsValid);
        Assert.IsEmpty(result.Results);
    }
    #endregion

    sealed class ValidModel
    {
        #region Public properties
        [Range(0, 150)]
        public int Age { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = default!;
        #endregion
    }

    sealed class SelfValidatingModel : IValidatableObject
    {
        #region Public methods
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (End <= Start)
            {
                yield return new ValidationResult("End must be greater than Start.", [nameof(End)]);
            }
        }
        #endregion

        #region Public properties
        public int End { get; set; }

        public int Start { get; set; }
        #endregion
    }
}
