using Catharsis.ComponentModel.Validation;

namespace Catharsis.UnitTests.ComponentModel.Validation;

///<summary>
///Unit tests for the <see cref="ValidationScope"/> class.
///</summary>
[TestClass]
public sealed class ValidationScopeTests
{
    #region Public methods
    [TestMethod]
    public void EnumValues_AreDefined()
    {
#pragma warning disable MSTEST0032 // Enum value guardrail assertions are intentionally constant
        Assert.AreEqual(0, (int)ValidationScope.Property);
        Assert.AreEqual(1, (int)ValidationScope.Object);
        Assert.AreEqual(2, (int)ValidationScope.CrossProperty);
#pragma warning restore MSTEST0032
    }
    #endregion
}
