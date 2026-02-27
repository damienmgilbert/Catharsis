using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Catharsis.ComponentModel.Validation;

namespace Catharsis.UnitTests.ComponentModel.Validation;

[TestClass]
public sealed class RegexIfAttributeTests
{
    #region Public methods
    [TestMethod]
    public void Constructor_NullDependentProperty_ThrowsArgumentNullException() { Assert.ThrowsExactly<ArgumentNullException>(() => new RegexIfAttribute(null!, true, @"\d+")); }
    [TestMethod]
    public void Constructor_NullPattern_ThrowsArgumentNullException() { Assert.ThrowsExactly<ArgumentNullException>(() => new RegexIfAttribute("Prop", true, null!)); }
    [TestMethod]
    public void Constructor_SetsProperties()
    {
        RegexIfAttribute attr = new RegexIfAttribute("Prop", "yes", @"\d+", RegexOptions.IgnoreCase);

        Assert.AreEqual("Prop", attr.DependentProperty);
        Assert.AreEqual("yes", attr.TargetValue);
        Assert.AreEqual(@"\d+", attr.Pattern);
    }

    [TestMethod]
    public void FormatErrorMessage_ContainsPatternAndDependentInfo()
    {
        RegexIfAttribute attr = new RegexIfAttribute("Format", "email", @"^\d+$");

        string msg = attr.FormatErrorMessage("Contact");

        Assert.IsTrue(msg.Contains("Contact"));
        Assert.IsTrue(msg.Contains(@"^\d+$"));
        Assert.IsTrue(msg.Contains("Format"));
        Assert.IsTrue(msg.Contains("email"));
    }

    [TestMethod]
    public void Validate_ConditionMet_MatchingValue_Passes()
    {
        TestModel model = new TestModel { Format = "email", Contact = "user@example.com" };
        ValidationContext context = new ValidationContext(model) { MemberName = nameof(TestModel.Contact) };

        Assert.IsTrue(Validator.TryValidateProperty(model.Contact, context, null));
    }

    [TestMethod]
    public void Validate_ConditionMet_NonMatchingValue_Fails()
    {
        TestModel model = new TestModel { Format = "email", Contact = "not-an-email" };
        ValidationContext context = new ValidationContext(model) { MemberName = nameof(TestModel.Contact) };
        List<ValidationResult> results = new List<ValidationResult>();

        Assert.IsFalse(Validator.TryValidateProperty(model.Contact, context, results));
        Assert.AreEqual(1, results.Count);
    }

    [TestMethod]
    public void Validate_ConditionMet_NullValue_Passes()
    {
        TestModel model = new TestModel { Format = "email", Contact = null };
        ValidationContext context = new ValidationContext(model) { MemberName = nameof(TestModel.Contact) };

        Assert.IsTrue(Validator.TryValidateProperty(model.Contact, context, null));
    }

    [TestMethod]
    public void Validate_ConditionNotMet_InvalidValue_Passes()
    {
        TestModel model = new TestModel { Format = "phone", Contact = "not-an-email" };
        ValidationContext context = new ValidationContext(model) { MemberName = nameof(TestModel.Contact) };

        Assert.IsTrue(Validator.TryValidateProperty(model.Contact, context, null));
    }

    [TestMethod]
    public void Validate_NonExistentDependentProperty_Passes()
    {
        RegexIfAttribute attr = new RegexIfAttribute("NonExistent", true, @"\d+");
        TestModel model = new TestModel { Format = "email", Contact = "abc" };
        ValidationContext context = new ValidationContext(model) { MemberName = nameof(TestModel.Contact) };

        ValidationResult? result = attr.GetValidationResult(model.Contact, context);

        Assert.AreSame(ValidationResult.Success, result);
    }
    #endregion

    sealed class TestModel
    {
        #region Public properties
        [RegexIf(nameof(Format), "email", @"^[^@]+@[^@]+\.[^@]+$")]
        public string? Contact { get; set; }

        public string? Format { get; set; }
        #endregion
    }
}
