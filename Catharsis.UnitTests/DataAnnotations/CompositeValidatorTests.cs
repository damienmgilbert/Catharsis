using Catharsis.DataAnnotations;
using System.ComponentModel.DataAnnotations;

namespace Catharsis.UnitTests.DataAnnotations;

[TestClass]
public class CompositeValidatorTests
{
    [TestMethod]
    public void ValidObject_ReturnsIsValidTrue()
    {
        var model = new ValidModel { Name = "Alice", Age = 25 };

        var result = CompositeValidator.ValidateObject(model);

        Assert.IsTrue(result.IsValid);
        Assert.AreEqual(0, result.Results.Count);
    }

    [TestMethod]
    public void InvalidObject_ReturnsIsValidFalse()
    {
        var model = new ValidModel { Name = null!, Age = 200 };

        var result = CompositeValidator.ValidateObject(model);

        Assert.IsFalse(result.IsValid);
        Assert.IsTrue(result.Results.Count > 0);
    }

    [TestMethod]
    public void InvalidObject_ErrorMessagesPopulated()
    {
        var model = new ValidModel { Name = null!, Age = 25 };

        var result = CompositeValidator.ValidateObject(model);

        Assert.IsFalse(result.IsValid);
        Assert.IsTrue(result.ErrorMessages.Any());
    }

    [TestMethod]
    public void InvalidObject_MemberNamesPopulated()
    {
        var model = new ValidModel { Name = null!, Age = 25 };

        var result = CompositeValidator.ValidateObject(model);

        Assert.IsFalse(result.IsValid);
        Assert.IsTrue(result.MemberNames.Any());
    }

    [TestMethod]
    public void ValidateProperty_ValidProperty_ReturnsSuccess()
    {
        var model = new ValidModel { Name = "Alice", Age = 25 };

        var result = CompositeValidator.ValidateProperty(model, nameof(ValidModel.Name));

        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public void ValidateProperty_InvalidProperty_ReturnsFailure()
    {
        var model = new ValidModel { Name = null!, Age = 25 };

        var result = CompositeValidator.ValidateProperty(model, nameof(ValidModel.Name));

        Assert.IsFalse(result.IsValid);
    }

    [TestMethod]
    public void ValidateProperty_UnknownProperty_ThrowsArgumentException()
    {
        var model = new ValidModel { Name = "Alice", Age = 25 };

        Assert.ThrowsExactly<ArgumentException>(() =>
            CompositeValidator.ValidateProperty(model, "NonExistent"));
    }

    [TestMethod]
    public void ValidateValue_ValidValue_ReturnsSuccess()
    {
        var attributes = new ValidationAttribute[] { new RequiredAttribute(), new StringLengthAttribute(50) };

        var result = CompositeValidator.ValidateValue("hello", "TestField", attributes);

        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public void ValidateValue_InvalidValue_ReturnsFailure()
    {
        var attributes = new ValidationAttribute[] { new RequiredAttribute() };

        var result = CompositeValidator.ValidateValue(null, "TestField", attributes);

        Assert.IsFalse(result.IsValid);
    }

    [TestMethod]
    public void ThrowIfInvalid_ValidObject_DoesNotThrow()
    {
        var model = new ValidModel { Name = "Alice", Age = 25 };

        var result = CompositeValidator.ValidateObject(model);

        result.ThrowIfInvalid();
    }

    [TestMethod]
    public void ThrowIfInvalid_InvalidObject_ThrowsValidationException()
    {
        var model = new ValidModel { Name = null!, Age = 25 };

        var result = CompositeValidator.ValidateObject(model);

        Assert.ThrowsExactly<ValidationException>(() => result.ThrowIfInvalid());
    }

    [TestMethod]
    public void ValidateObject_NullInstance_ThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() =>
            CompositeValidator.ValidateObject<ValidModel>(null!));
    }

    [TestMethod]
    public void ValidateObject_WithIValidatableObject_RunsValidation()
    {
        var model = new SelfValidatingModel { Start = 10, End = 5 };

        var result = CompositeValidator.ValidateObject(model);

        Assert.IsFalse(result.IsValid);
        Assert.IsTrue(result.ErrorMessages.Any(m => m!.Contains("End must be greater")));
    }

    [TestMethod]
    public void ValidateObject_WithValidIValidatableObject_ReturnsSuccess()
    {
        var model = new SelfValidatingModel { Start = 5, End = 10 };

        var result = CompositeValidator.ValidateObject(model);

        Assert.IsTrue(result.IsValid);
    }

    private sealed class ValidModel
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = default!;

        [Range(0, 150)]
        public int Age { get; set; }
    }

    private sealed class SelfValidatingModel : IValidatableObject
    {
        public int Start { get; set; }
        public int End { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (End <= Start)
            {
                yield return new ValidationResult(
                    "End must be greater than Start.",
                    [nameof(End)]);
            }
        }
    }
}
