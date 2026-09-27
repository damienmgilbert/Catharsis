using Catharsis.DataAnnotations;
using System.ComponentModel.DataAnnotations;

namespace Catharsis.UnitTests.DataAnnotations;

///<summary>
///Unit tests for the <see cref="RequiredWhenAttribute"/> class.
///</summary>
[TestClass]
public class RequiredWhenAttributeTests
{
    #region Private methods
    private static ValidationContext CreateContext(TestModel model, string memberName) => new(model) { MemberName = memberName, DisplayName = memberName };
    #endregion

    #region Public methods

    [TestMethod]
    public void Constructor_NullDependentProperty_Throws() { Assert.ThrowsExactly<ArgumentNullException>(static () => new RequiredWhenAttribute(null!, "a")); }

    [TestMethod]
    public void Constructor_NullTargetValues_Throws() { Assert.ThrowsExactly<ArgumentNullException>(static () => new RequiredWhenAttribute("Status", (object?[])null!)); }

    [TestMethod]
    public void Constructor_EmptyTargetValues_Throws() { Assert.ThrowsExactly<ArgumentException>(static () => new RequiredWhenAttribute("Status")); }

    [TestMethod]
    public void IsValid_UnknownDependentProperty_ReturnsFailure()
    {
        RequiredWhenAttribute attribute = new("DoesNotExist", "Approved");
        TestModel model = new() { Status = "Approved", Notes = null };

        ValidationResult? result = attribute.GetValidationResult(model.Notes, CreateContext(model, nameof(TestModel.Notes)));

        Assert.AreNotEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void IsValid_DependentNotAmongTargets_ReturnsSuccess()
    {
        RequiredWhenAttribute attribute = new(nameof(TestModel.Status), "Approved", "Shipped");
        TestModel model = new() { Status = "Draft", Notes = null };

        ValidationResult? result = attribute.GetValidationResult(model.Notes, CreateContext(model, nameof(TestModel.Notes)));

        Assert.AreEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void IsValid_DependentMatchesOneTarget_ValueMissing_ReturnsFailure()
    {
        RequiredWhenAttribute attribute = new(nameof(TestModel.Status), "Approved", "Shipped");
        TestModel model = new() { Status = "Shipped", Notes = null };

        ValidationResult? result = attribute.GetValidationResult(model.Notes, CreateContext(model, nameof(TestModel.Notes)));

        Assert.AreNotEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void IsValid_DependentMatchesOneTarget_ValuePresent_ReturnsSuccess()
    {
        RequiredWhenAttribute attribute = new(nameof(TestModel.Status), "Approved", "Shipped");
        TestModel model = new() { Status = "Approved", Notes = "signed off" };

        ValidationResult? result = attribute.GetValidationResult(model.Notes, CreateContext(model, nameof(TestModel.Notes)));

        Assert.AreEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void IsValid_EmptyStringWithDisallowEmptyStrings_ReturnsFailure()
    {
        RequiredWhenAttribute attribute = new(nameof(TestModel.Status), "Approved");
        TestModel model = new() { Status = "Approved", Notes = "" };

        ValidationResult? result = attribute.GetValidationResult(model.Notes, CreateContext(model, nameof(TestModel.Notes)));

        Assert.AreNotEqual(ValidationResult.Success, result);
    }

    #endregion

    private sealed class TestModel
    {
        #region Public properties
        public string? Status { get; set; }
        public string? Notes { get; set; }
        #endregion
    }
}
