using Catharsis.ComponentModel.Validation;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Catharsis.UnitTests.ComponentModel.Validation;

[TestClass]
public sealed class RegexIfAttributeTests
{
    private sealed class TestModel
    {
        public string? Format { get; set; }

        [RegexIf(nameof(Format), "email", @"^[^@]+@[^@]+\.[^@]+$")]
        public string? Contact { get; set; }
    }

    [TestMethod]
    public void Constructor_NullDependentProperty_ThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(
            () => new RegexIfAttribute(null!, true, @"\d+"));
    }

    [TestMethod]
    public void Constructor_NullPattern_ThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(
            () => new RegexIfAttribute("Prop", true, null!));
    }

    [TestMethod]
    public void Constructor_SetsProperties()
    {
        var attr = new RegexIfAttribute("Prop", "yes", @"\d+", RegexOptions.IgnoreCase);

        Assert.AreEqual("Prop", attr.DependentProperty);
        Assert.AreEqual("yes", attr.TargetValue);
        Assert.AreEqual(@"\d+", attr.Pattern);
    }

    [TestMethod]
    public void Validate_ConditionNotMet_InvalidValue_Passes()
    {
        var model = new TestModel { Format = "phone", Contact = "not-an-email" };
        var context = new ValidationContext(model) { MemberName = nameof(TestModel.Contact) };

        Assert.IsTrue(Validator.TryValidateProperty(model.Contact, context, null));
    }

    [TestMethod]
    public void Validate_ConditionMet_MatchingValue_Passes()
    {
        var model = new TestModel { Format = "email", Contact = "user@example.com" };
        var context = new ValidationContext(model) { MemberName = nameof(TestModel.Contact) };

        Assert.IsTrue(Validator.TryValidateProperty(model.Contact, context, null));
    }

    [TestMethod]
    public void Validate_ConditionMet_NonMatchingValue_Fails()
    {
        var model = new TestModel { Format = "email", Contact = "not-an-email" };
        var context = new ValidationContext(model) { MemberName = nameof(TestModel.Contact) };
        var results = new List<ValidationResult>();

        Assert.IsFalse(Validator.TryValidateProperty(model.Contact, context, results));
        Assert.AreEqual(1, results.Count);
    }

    [TestMethod]
    public void Validate_ConditionMet_NullValue_Passes()
    {
        var model = new TestModel { Format = "email", Contact = null };
        var context = new ValidationContext(model) { MemberName = nameof(TestModel.Contact) };

        Assert.IsTrue(Validator.TryValidateProperty(model.Contact, context, null));
    }

    [TestMethod]
    public void FormatErrorMessage_ContainsPatternAndDependentInfo()
    {
        var attr = new RegexIfAttribute("Format", "email", @"^\d+$");

        var msg = attr.FormatErrorMessage("Contact");

        Assert.IsTrue(msg.Contains("Contact"));
        Assert.IsTrue(msg.Contains(@"^\d+$"));
        Assert.IsTrue(msg.Contains("Format"));
        Assert.IsTrue(msg.Contains("email"));
    }

    [TestMethod]
    public void Validate_NonExistentDependentProperty_Passes()
    {
        var attr = new RegexIfAttribute("NonExistent", true, @"\d+");
        var model = new TestModel { Format = "email", Contact = "abc" };
        var context = new ValidationContext(model) { MemberName = nameof(TestModel.Contact) };

        var result = attr.GetValidationResult(model.Contact, context);

        Assert.AreSame(ValidationResult.Success, result);
    }
}
