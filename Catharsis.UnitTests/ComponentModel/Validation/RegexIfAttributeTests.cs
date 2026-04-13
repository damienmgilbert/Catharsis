using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Catharsis.ComponentModel.Validation;

namespace Catharsis.UnitTests.ComponentModel.Validation;

///<summary>
///Unit tests for the <see cref="RegexIfAttribute"/> class.
///</summary>
[TestClass]
public sealed class RegexIfAttributeTests
{
    #region Public methods
    [TestMethod]
    public void Constructor_NullDependentProperty_ThrowsArgumentNullException() { Assert.ThrowsExactly<ArgumentNullException>(static () => new RegexIfAttribute(null!, true, @"\d+")); }
    [TestMethod]
    public void Constructor_NullPattern_ThrowsArgumentNullException() { Assert.ThrowsExactly<ArgumentNullException>(static () => new RegexIfAttribute("Prop", true, null!)); }
    [TestMethod]
    public void Constructor_SetsProperties()
    {
        RegexIfAttribute attr = new("Prop", "yes", @"\d+", RegexOptions.IgnoreCase);

        Assert.AreEqual("Prop", attr.DependentProperty);
        Assert.AreEqual("yes", attr.TargetValue);
        Assert.AreEqual(@"\d+", attr.Pattern);
    }

    [TestMethod]
    public void FormatErrorMessage_ContainsPatternAndDependentInfo()
    {
        RegexIfAttribute attr = new("Format", "email", @"^\d+$");

        string msg = attr.FormatErrorMessage("Contact");

        Assert.Contains("Contact", msg);
        Assert.Contains(@"^\d+$", msg);
        Assert.Contains("Format", msg);
        Assert.Contains("email", msg);
    }

    [TestMethod]
    public void Validate_ConditionMet_MatchingValue_Passes()
    {
        TestModel model = new() { Format = "email", Contact = "user@example.com" };
        ValidationContext context = new(model) { MemberName = nameof(TestModel.Contact) };

        Assert.IsTrue(Validator.TryValidateProperty(model.Contact, context, null));
    }

    [TestMethod]
    public void Validate_ConditionMet_NonMatchingValue_Fails()
    {
        TestModel model = new() { Format = "email", Contact = "not-an-email" };
        ValidationContext context = new(model) { MemberName = nameof(TestModel.Contact) };
        List<ValidationResult> results = [];

        Assert.IsFalse(Validator.TryValidateProperty(model.Contact, context, results));
        Assert.HasCount(1, results);
    }

    [TestMethod]
    public void Validate_ConditionMet_NullValue_Passes()
    {
        TestModel model = new() { Format = "email", Contact = null };
        ValidationContext context = new(model) { MemberName = nameof(TestModel.Contact) };

        Assert.IsTrue(Validator.TryValidateProperty(model.Contact, context, null));
    }

    [TestMethod]
    public void Validate_ConditionNotMet_InvalidValue_Passes()
    {
        TestModel model = new() { Format = "phone", Contact = "not-an-email" };
        ValidationContext context = new(model) { MemberName = nameof(TestModel.Contact) };

        Assert.IsTrue(Validator.TryValidateProperty(model.Contact, context, null));
    }

    [TestMethod]
    public void Validate_NonExistentDependentProperty_Passes()
    {
        RegexIfAttribute attr = new("NonExistent", true, @"\d+");
        TestModel model = new() { Format = "email", Contact = "abc" };
        ValidationContext context = new(model) { MemberName = nameof(TestModel.Contact) };

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
