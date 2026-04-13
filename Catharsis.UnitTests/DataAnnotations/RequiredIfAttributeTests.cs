using System.ComponentModel.DataAnnotations;
using Catharsis.DataAnnotations;

namespace Catharsis.UnitTests.DataAnnotations;

///<summary>
///Unit tests for the <see cref="RequiredIfAttribute"/> class.
///</summary>
[TestClass]
public class RequiredIfAttributeTests
{
    #region Public methods
    [TestMethod]
    public void ConditionMet_EmptyString_ReturnsFailure()
    {
        RequiredIfAttribute attribute = new(nameof(TestModel.IsActive), true);
        TestModel model = new() { IsActive = true, Details = string.Empty };
        ValidationContext context = new(model) { MemberName = nameof(TestModel.Details) };

        ValidationResult? result = attribute.GetValidationResult(string.Empty, context);

        Assert.AreNotEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void ConditionMet_EmptyStringAllowed_ReturnsSuccess()
    {
        RequiredIfAttribute attribute = new(nameof(TestModel.IsActive), true) { DisallowEmptyStrings = false };
        TestModel model = new() { IsActive = true, Details = string.Empty };
        ValidationContext context = new(model) { MemberName = nameof(TestModel.Details) };

        ValidationResult? result = attribute.GetValidationResult(string.Empty, context);

        Assert.AreEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void ConditionMet_ValueNull_ReturnsFailure()
    {
        RequiredIfAttribute attribute = new(nameof(TestModel.IsActive), true);
        TestModel model = new() { IsActive = true, Details = null };
        ValidationContext context = new(model) { MemberName = nameof(TestModel.Details) };

        ValidationResult? result = attribute.GetValidationResult(null, context);

        Assert.AreNotEqual(ValidationResult.Success, result);
        Assert.Contains("required", result!.ErrorMessage!);
    }

    [TestMethod]
    public void ConditionMet_ValuePresent_ReturnsSuccess()
    {
        RequiredIfAttribute attribute = new(nameof(TestModel.IsActive), true);
        TestModel model = new() { IsActive = true, Details = "provided" };
        ValidationContext context = new(model) { MemberName = nameof(TestModel.Details) };

        ValidationResult? result = attribute.GetValidationResult("provided", context);

        Assert.AreEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void ConditionNotMet_ValueNull_ReturnsSuccess()
    {
        RequiredIfAttribute attribute = new(nameof(TestModel.IsActive), true);
        TestModel model = new() { IsActive = false, Details = null };
        ValidationContext context = new(model) { MemberName = nameof(TestModel.Details) };

        ValidationResult? result = attribute.GetValidationResult(null, context);

        Assert.AreEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void Constructor_NullDependentProperty_ThrowsArgumentNullException() { Assert.ThrowsExactly<ArgumentNullException>(static () => new RequiredIfAttribute(null!, true)); }
    [TestMethod]
    public void TargetValueNull_ConditionMet_ReturnsFailure()
    {
        RequiredIfAttribute attribute = new(nameof(TestModel.Details), null);
        TestModel model = new() { Details = null, IsActive = false };
        ValidationContext context = new(model) { MemberName = nameof(TestModel.IsActive) };

        ValidationResult? result = attribute.GetValidationResult(null, context);

        Assert.AreNotEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void UnknownDependentProperty_ReturnsFailure()
    {
        RequiredIfAttribute attribute = new("NonExistent", true);
        TestModel model = new() { IsActive = true };
        ValidationContext context = new(model) { MemberName = nameof(TestModel.Details) };

        ValidationResult? result = attribute.GetValidationResult(null, context);

        Assert.AreNotEqual(ValidationResult.Success, result);
        Assert.Contains("Unknown property", result!.ErrorMessage!);
    }
    #endregion

    sealed class TestModel
    {
        #region Public properties
        public string? Details { get; set; }

        public bool IsActive { get; set; }
        #endregion
    }
}
