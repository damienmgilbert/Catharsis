using Catharsis.ComponentModel.Validation;

namespace Catharsis.UnitTests.ComponentModel.Validation;

[TestClass]
public sealed class ValidationSeverityTests
{
    #region Public methods
    [TestMethod]
    public void EnumValues_AreDefined()
    {
        Assert.AreEqual(0, (int)ValidationSeverity.Info);
        Assert.AreEqual(1, (int)ValidationSeverity.Warning);
        Assert.AreEqual(2, (int)ValidationSeverity.Error);
    }
    #endregion
}
