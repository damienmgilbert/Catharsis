using Catharsis.ComponentModel.Validation;

namespace Catharsis.UnitTests.ComponentModel.Validation;

///<summary>
///Unit tests for the <see cref="ValidationSeverity"/> class.
///</summary>
[TestClass]
public sealed class ValidationSeverityTests
{
    #region Public methods
    [TestMethod]
    public void EnumValues_AreDefined()
    {
#pragma warning disable MSTEST0032 // Enum value guardrail assertions are intentionally constant
        Assert.AreEqual(0, (int)ValidationSeverity.Info);
        Assert.AreEqual(1, (int)ValidationSeverity.Warning);
        Assert.AreEqual(2, (int)ValidationSeverity.Error);
#pragma warning restore MSTEST0032
    }
    #endregion
}
