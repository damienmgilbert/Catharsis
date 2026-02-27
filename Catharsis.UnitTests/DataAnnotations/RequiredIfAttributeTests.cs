using System.ComponentModel.DataAnnotations;
using Catharsis.DataAnnotations;

namespace Catharsis.UnitTests.DataAnnotations;

[TestClass]
public class RequiredIfAttributeTests
{
    #region Public methods
    [TestMethod]
    public void ConditionMet_EmptyString_ReturnsFailure()
    {
        RequiredIfAttribute attribute = new RequiredIfAttribute(nameof(TestModel.IsActive), true);
        TestModel model = new TestModel { IsActive = true, Details = string.Empty };
        ValidationContext context = new ValidationContext(model) { MemberName = nameof(TestModel.Details) };

        ValidationResult? result = attribute.GetValidationResult(string.Empty, context);

        Assert.AreNotEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void ConditionMet_EmptyStringAllowed_ReturnsSuccess()
    {
        RequiredIfAttribute attribute = new RequiredIfAttribute(nameof(TestModel.IsActive), true) { DisallowEmptyStrings = false };
        TestModel model = new TestModel { IsActive = true, Details = string.Empty };
        ValidationContext context = new ValidationContext(model) { MemberName = nameof(TestModel.Details) };

        ValidationResult? result = attribute.GetValidationResult(string.Empty, context);

        Assert.AreEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void ConditionMet_ValueNull_ReturnsFailure()
    {
        RequiredIfAttribute attribute = new RequiredIfAttribute(nameof(TestModel.IsActive), true);
        TestModel model = new TestModel { IsActive = true, Details = null };
        ValidationContext context = new ValidationContext(model) { MemberName = nameof(TestModel.Details) };

        ValidationResult? result = attribute.GetValidationResult(null, context);

        Assert.AreNotEqual(ValidationResult.Success, result);
        Assert.IsTrue(result!.ErrorMessage!.Contains("required"));
    }

    [TestMethod]
    public void ConditionMet_ValuePresent_ReturnsSuccess()
    {
        RequiredIfAttribute attribute = new RequiredIfAttribute(nameof(TestModel.IsActive), true);
        TestModel model = new TestModel { IsActive = true, Details = "provided" };
        ValidationContext context = new ValidationContext(model) { MemberName = nameof(TestModel.Details) };

        ValidationResult? result = attribute.GetValidationResult("provided", context);

        Assert.AreEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void ConditionNotMet_ValueNull_ReturnsSuccess()
    {
        RequiredIfAttribute attribute = new RequiredIfAttribute(nameof(TestModel.IsActive), true);
        TestModel model = new TestModel { IsActive = false, Details = null };
        ValidationContext context = new ValidationContext(model) { MemberName = nameof(TestModel.Details) };

        ValidationResult? result = attribute.GetValidationResult(null, context);

        Assert.AreEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void Constructor_NullDependentProperty_ThrowsArgumentNullException() { Assert.ThrowsExactly<ArgumentNullException>(() => new RequiredIfAttribute(null!, true)); }
    [TestMethod]
    public void TargetValueNull_ConditionMet_ReturnsFailure()
    {
        RequiredIfAttribute attribute = new RequiredIfAttribute(nameof(TestModel.Details), null);
        TestModel model = new TestModel { Details = null, IsActive = false };
        ValidationContext context = new ValidationContext(model) { MemberName = nameof(TestModel.IsActive) };

        ValidationResult? result = attribute.GetValidationResult(null, context);

        Assert.AreNotEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void UnknownDependentProperty_ReturnsFailure()
    {
        RequiredIfAttribute attribute = new RequiredIfAttribute("NonExistent", true);
        TestModel model = new TestModel { IsActive = true };
        ValidationContext context = new ValidationContext(model) { MemberName = nameof(TestModel.Details) };

        ValidationResult? result = attribute.GetValidationResult(null, context);

        Assert.AreNotEqual(ValidationResult.Success, result);
        Assert.IsTrue(result!.ErrorMessage!.Contains("Unknown property"));
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
