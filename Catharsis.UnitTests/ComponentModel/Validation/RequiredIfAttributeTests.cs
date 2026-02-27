using Catharsis.ComponentModel.Validation;
using System.ComponentModel.DataAnnotations;

namespace Catharsis.UnitTests.ComponentModel.Validation;

[TestClass]
public sealed class RequiredIfAttributeTests
{
    private sealed class TestModel
    {
        public bool IsActive { get; set; }

        [RequiredIf(nameof(IsActive), true)]
        public string? Name { get; set; }
    }

    [TestMethod]
    public void Constructor_NullDependentProperty_ThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(
            () => new RequiredIfAttribute(null!, true));
    }

    [TestMethod]
    public void Constructor_SetsProperties()
    {
        var attr = new RequiredIfAttribute("IsActive", true);

        Assert.AreEqual("IsActive", attr.DependentProperty);
        Assert.AreEqual(true, attr.TargetValue);
        Assert.IsTrue(attr.DisallowEmptyStrings);
    }

    [TestMethod]
    public void Validate_ConditionNotMet_NullValue_Passes()
    {
        var model = new TestModel { IsActive = false, Name = null };
        var context = new ValidationContext(model) { MemberName = nameof(TestModel.Name) };

        var result = Validator.TryValidateProperty(model.Name, context, null);

        Assert.IsTrue(result);
    }

    [TestMethod]
    public void Validate_ConditionMet_NullValue_Fails()
    {
        var model = new TestModel { IsActive = true, Name = null };
        var context = new ValidationContext(model) { MemberName = nameof(TestModel.Name) };
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateProperty(model.Name, context, results);

        Assert.IsFalse(isValid);
        Assert.AreEqual(1, results.Count);
    }

    [TestMethod]
    public void Validate_ConditionMet_EmptyString_Fails()
    {
        var model = new TestModel { IsActive = true, Name = "" };
        var context = new ValidationContext(model) { MemberName = nameof(TestModel.Name) };
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateProperty(model.Name, context, results);

        Assert.IsFalse(isValid);
        Assert.AreEqual(1, results.Count);
    }

    [TestMethod]
    public void Validate_ConditionMet_ValidValue_Passes()
    {
        var model = new TestModel { IsActive = true, Name = "John" };
        var context = new ValidationContext(model) { MemberName = nameof(TestModel.Name) };

        var result = Validator.TryValidateProperty(model.Name, context, null);

        Assert.IsTrue(result);
    }

    [TestMethod]
    public void Validate_DisallowEmptyStringsFalse_EmptyStringPasses()
    {
        var attr = new RequiredIfAttribute("IsActive", true) { DisallowEmptyStrings = false };
        var model = new TestModel { IsActive = true, Name = "" };
        var context = new ValidationContext(model) { MemberName = nameof(TestModel.Name) };

        var result = attr.GetValidationResult(model.Name, context);

        Assert.AreSame(ValidationResult.Success, result);
    }

    [TestMethod]
    public void FormatErrorMessage_ContainsDependentPropertyAndTargetValue()
    {
        var attr = new RequiredIfAttribute("IsActive", true);

        var msg = attr.FormatErrorMessage("Name");

        Assert.IsTrue(msg.Contains("Name"));
        Assert.IsTrue(msg.Contains("IsActive"));
        Assert.IsTrue(msg.Contains("True"));
    }

    [TestMethod]
    public void Validate_NonExistentDependentProperty_Passes()
    {
        var attr = new RequiredIfAttribute("NonExistent", true);
        var model = new TestModel { IsActive = true, Name = null };
        var context = new ValidationContext(model) { MemberName = nameof(TestModel.Name) };

        var result = attr.GetValidationResult(model.Name, context);

        Assert.AreSame(ValidationResult.Success, result);
    }
}
