using Catharsis.ComponentModel.Validation;

namespace Catharsis.UnitTests.ComponentModel.Validation;

[TestClass]
public sealed class ValidationScopeTests
{
    #region Public methods
    [TestMethod]
    public void EnumValues_AreDefined()
    {
        Assert.AreEqual(0, (int)ValidationScope.Property);
        Assert.AreEqual(1, (int)ValidationScope.Object);
        Assert.AreEqual(2, (int)ValidationScope.CrossProperty);
    }
    #endregion
}
