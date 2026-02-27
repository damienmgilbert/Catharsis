using Catharsis.DataAnnotations;
using System.ComponentModel.DataAnnotations;

namespace Catharsis.UnitTests.DataAnnotations;

[TestClass]
public class RequiredIfAttributeTests
{
    [TestMethod]
    public void ConditionMet_ValuePresent_ReturnsSuccess()
    {
        var attribute = new RequiredIfAttribute(nameof(TestModel.IsActive), true);
        var model = new TestModel { IsActive = true, Details = "provided" };
        var context = new ValidationContext(model) { MemberName = nameof(TestModel.Details) };

        var result = attribute.GetValidationResult("provided", context);

        Assert.AreEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void ConditionMet_ValueNull_ReturnsFailure()
    {
        var attribute = new RequiredIfAttribute(nameof(TestModel.IsActive), true);
        var model = new TestModel { IsActive = true, Details = null };
        var context = new ValidationContext(model) { MemberName = nameof(TestModel.Details) };

        var result = attribute.GetValidationResult(null, context);

        Assert.AreNotEqual(ValidationResult.Success, result);
        Assert.IsTrue(result!.ErrorMessage!.Contains("required"));
    }

    [TestMethod]
    public void ConditionMet_EmptyString_ReturnsFailure()
    {
        var attribute = new RequiredIfAttribute(nameof(TestModel.IsActive), true);
        var model = new TestModel { IsActive = true, Details = "" };
        var context = new ValidationContext(model) { MemberName = nameof(TestModel.Details) };

        var result = attribute.GetValidationResult("", context);

        Assert.AreNotEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void ConditionMet_EmptyStringAllowed_ReturnsSuccess()
    {
        var attribute = new RequiredIfAttribute(nameof(TestModel.IsActive), true)
        {
            DisallowEmptyStrings = false
        };
        var model = new TestModel { IsActive = true, Details = "" };
        var context = new ValidationContext(model) { MemberName = nameof(TestModel.Details) };

        var result = attribute.GetValidationResult("", context);

        Assert.AreEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void ConditionNotMet_ValueNull_ReturnsSuccess()
    {
        var attribute = new RequiredIfAttribute(nameof(TestModel.IsActive), true);
        var model = new TestModel { IsActive = false, Details = null };
        var context = new ValidationContext(model) { MemberName = nameof(TestModel.Details) };

        var result = attribute.GetValidationResult(null, context);

        Assert.AreEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void UnknownDependentProperty_ReturnsFailure()
    {
        var attribute = new RequiredIfAttribute("NonExistent", true);
        var model = new TestModel { IsActive = true };
        var context = new ValidationContext(model) { MemberName = nameof(TestModel.Details) };

        var result = attribute.GetValidationResult(null, context);

        Assert.AreNotEqual(ValidationResult.Success, result);
        Assert.IsTrue(result!.ErrorMessage!.Contains("Unknown property"));
    }

    [TestMethod]
    public void TargetValueNull_ConditionMet_ReturnsFailure()
    {
        var attribute = new RequiredIfAttribute(nameof(TestModel.Details), null);
        var model = new TestModel { Details = null, IsActive = false };
        var context = new ValidationContext(model) { MemberName = nameof(TestModel.IsActive) };

        var result = attribute.GetValidationResult(null, context);

        Assert.AreNotEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void Constructor_NullDependentProperty_ThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() =>
            new RequiredIfAttribute(null!, true));
    }

    private sealed class TestModel
    {
        public bool IsActive { get; set; }
        public string? Details { get; set; }
    }
}
