using System.ComponentModel.DataAnnotations;
using Catharsis.ComponentModel.Validation;

namespace Catharsis.UnitTests.ComponentModel.Validation;

[TestClass]
public sealed class RequiredIfAttributeTests
{
    #region Public methods
    [TestMethod]
    public void Constructor_NullDependentProperty_ThrowsArgumentNullException() { Assert.ThrowsExactly<ArgumentNullException>(() => new RequiredIfAttribute(null!, true)); }
    [TestMethod]
    public void Constructor_SetsProperties()
    {
        RequiredIfAttribute attr = new RequiredIfAttribute("IsActive", true);

        Assert.AreEqual("IsActive", attr.DependentProperty);
        Assert.IsTrue((bool?)attr.TargetValue);
        Assert.IsTrue(attr.DisallowEmptyStrings);
    }

    [TestMethod]
    public void FormatErrorMessage_ContainsDependentPropertyAndTargetValue()
    {
        RequiredIfAttribute attr = new RequiredIfAttribute("IsActive", true);

        string msg = attr.FormatErrorMessage("Name");

        Assert.Contains("Name", msg);
        Assert.Contains("IsActive", msg);
        Assert.Contains("True", msg);
    }

    [TestMethod]
    public void Validate_ConditionMet_EmptyString_Fails()
    {
        TestModel model = new TestModel { IsActive = true, Name = string.Empty };
        ValidationContext context = new ValidationContext(model) { MemberName = nameof(TestModel.Name) };
        List<ValidationResult> results = new List<ValidationResult>();

        bool isValid = Validator.TryValidateProperty(model.Name, context, results);

        Assert.IsFalse(isValid);
        Assert.HasCount(1, results);
    }

    [TestMethod]
    public void Validate_ConditionMet_NullValue_Fails()
    {
        TestModel model = new TestModel { IsActive = true, Name = null };
        ValidationContext context = new ValidationContext(model) { MemberName = nameof(TestModel.Name) };
        List<ValidationResult> results = new List<ValidationResult>();

        bool isValid = Validator.TryValidateProperty(model.Name, context, results);

        Assert.IsFalse(isValid);
        Assert.HasCount(1, results);
    }

    [TestMethod]
    public void Validate_ConditionMet_ValidValue_Passes()
    {
        TestModel model = new TestModel { IsActive = true, Name = "John" };
        ValidationContext context = new ValidationContext(model) { MemberName = nameof(TestModel.Name) };

        bool result = Validator.TryValidateProperty(model.Name, context, null);

        Assert.IsTrue(result);
    }

    [TestMethod]
    public void Validate_ConditionNotMet_NullValue_Passes()
    {
        TestModel model = new TestModel { IsActive = false, Name = null };
        ValidationContext context = new ValidationContext(model) { MemberName = nameof(TestModel.Name) };

        bool result = Validator.TryValidateProperty(model.Name, context, null);

        Assert.IsTrue(result);
    }

    [TestMethod]
    public void Validate_DisallowEmptyStringsFalse_EmptyStringPasses()
    {
        RequiredIfAttribute attr = new RequiredIfAttribute("IsActive", true) { DisallowEmptyStrings = false };
        TestModel model = new TestModel { IsActive = true, Name = string.Empty };
        ValidationContext context = new ValidationContext(model) { MemberName = nameof(TestModel.Name) };

        ValidationResult? result = attr.GetValidationResult(model.Name, context);

        Assert.AreSame(ValidationResult.Success, result);
    }

    [TestMethod]
    public void Validate_NonExistentDependentProperty_Passes()
    {
        RequiredIfAttribute attr = new RequiredIfAttribute("NonExistent", true);
        TestModel model = new TestModel { IsActive = true, Name = null };
        ValidationContext context = new ValidationContext(model) { MemberName = nameof(TestModel.Name) };

        ValidationResult? result = attr.GetValidationResult(model.Name, context);

        Assert.AreSame(ValidationResult.Success, result);
    }
    #endregion

    sealed class TestModel
    {
        #region Public properties
        public bool IsActive { get; set; }

        [RequiredIf(nameof(IsActive), true)]
        public string? Name { get; set; }
        #endregion
    }
}
