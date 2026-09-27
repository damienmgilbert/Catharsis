using Catharsis.DataAnnotations;
using System.ComponentModel.DataAnnotations;

namespace Catharsis.UnitTests.DataAnnotations;

///<summary>
///Unit tests for the <see cref="EnumRangeAttribute"/> class.
///</summary>
[TestClass]
public class EnumRangeAttributeTests
{
    #region Private methods
    private static ValidationContext CreateContext(string memberName) => new(new object()) { MemberName = memberName, DisplayName = memberName };
    #endregion

    #region Public methods

    [TestMethod]
    public void Constructor_NullEnumType_Throws() { Assert.ThrowsExactly<ArgumentNullException>(static () => new EnumRangeAttribute(null!)); }

    [TestMethod]
    public void Constructor_NonEnumType_Throws() { Assert.ThrowsExactly<ArgumentException>(static () => new EnumRangeAttribute(typeof(int))); }

    [TestMethod]
    public void IsValid_Null_ReturnsSuccess()
    {
        EnumRangeAttribute attribute = new(typeof(Status));
        Assert.AreEqual(ValidationResult.Success, attribute.GetValidationResult(null, CreateContext("Value")));
    }

    [TestMethod]
    public void IsValid_DefinedEnumValue_ReturnsSuccess()
    {
        EnumRangeAttribute attribute = new(typeof(Status));
        Assert.AreEqual(ValidationResult.Success, attribute.GetValidationResult(Status.Approved, CreateContext("Value")));
    }

    [TestMethod]
    public void IsValid_DefinedUnderlyingIntValue_ReturnsSuccess()
    {
        EnumRangeAttribute attribute = new(typeof(Status));
        Assert.AreEqual(ValidationResult.Success, attribute.GetValidationResult(1, CreateContext("Value")));
    }

    [TestMethod]
    public void IsValid_UndefinedIntValue_ReturnsFailure()
    {
        EnumRangeAttribute attribute = new(typeof(Status));
        Assert.AreNotEqual(ValidationResult.Success, attribute.GetValidationResult(999, CreateContext("Value")));
    }

    [TestMethod]
    public void IsValid_FlagsCombinationOfDefinedBits_ReturnsSuccess()
    {
        EnumRangeAttribute attribute = new(typeof(Permissions));
        Assert.AreEqual(ValidationResult.Success, attribute.GetValidationResult(Permissions.Read | Permissions.Write, CreateContext("Value")));
    }

    [TestMethod]
    public void IsValid_FlagsCombinationWithUndefinedBit_ReturnsFailure()
    {
        EnumRangeAttribute attribute = new(typeof(Permissions));
        Assert.AreNotEqual(ValidationResult.Success, attribute.GetValidationResult((Permissions)16, CreateContext("Value")));
    }

    #endregion

    private enum Status
    {
        Draft = 0,
        Approved = 1,
        Shipped = 2
    }

    [Flags]
    private enum Permissions
    {
        None = 0,
        Read = 1,
        Write = 2,
        Execute = 4
    }
}
