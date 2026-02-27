using Catharsis.ComponentModel.Validation;

namespace Catharsis.UnitTests.ComponentModel.Validation;

[TestClass]
public sealed class ErrorInfoTests
{
    [TestMethod]
    public void Constructor_Defaults_SeverityIsErrorAndPropertyNameIsNull()
    {
        var error = new ErrorInfo("Something failed.");

        Assert.AreEqual("Something failed.", error.Message);
        Assert.AreEqual(ValidationSeverity.Error, error.Severity);
        Assert.IsNull(error.PropertyName);
    }

    [TestMethod]
    public void Constructor_AllParameters_SetsProperties()
    {
        var error = new ErrorInfo("Bad value.", ValidationSeverity.Warning, "Age");

        Assert.AreEqual("Bad value.", error.Message);
        Assert.AreEqual(ValidationSeverity.Warning, error.Severity);
        Assert.AreEqual("Age", error.PropertyName);
    }

    [TestMethod]
    public void ToString_WithPropertyName_IncludesPropertyName()
    {
        var error = new ErrorInfo("Required.", ValidationSeverity.Error, "Name");

        Assert.AreEqual("[Error] Name: Required.", error.ToString());
    }

    [TestMethod]
    public void ToString_WithoutPropertyName_OmitsPropertyName()
    {
        var error = new ErrorInfo("Object invalid.", ValidationSeverity.Info);

        Assert.AreEqual("[Info] Object invalid.", error.ToString());
    }

    [TestMethod]
    public void Equality_SameValues_AreEqual()
    {
        var a = new ErrorInfo("msg", ValidationSeverity.Warning, "Prop");
        var b = new ErrorInfo("msg", ValidationSeverity.Warning, "Prop");

        Assert.AreEqual(a, b);
    }

    [TestMethod]
    public void Equality_DifferentMessage_AreNotEqual()
    {
        var a = new ErrorInfo("msg1");
        var b = new ErrorInfo("msg2");

        Assert.AreNotEqual(a, b);
    }
}

[TestClass]
public sealed class ValidationSeverityTests
{
    [TestMethod]
    public void EnumValues_AreDefined()
    {
        Assert.AreEqual(0, (int)ValidationSeverity.Info);
        Assert.AreEqual(1, (int)ValidationSeverity.Warning);
        Assert.AreEqual(2, (int)ValidationSeverity.Error);
    }
}

[TestClass]
public sealed class ValidationScopeTests
{
    [TestMethod]
    public void EnumValues_AreDefined()
    {
        Assert.AreEqual(0, (int)ValidationScope.Property);
        Assert.AreEqual(1, (int)ValidationScope.Object);
        Assert.AreEqual(2, (int)ValidationScope.CrossProperty);
    }
}
