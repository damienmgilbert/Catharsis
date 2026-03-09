using Catharsis.ComponentModel.Validation;

namespace Catharsis.UnitTests.ComponentModel.Validation;

///<summary>
///Unit tests for the <see cref="ErrorInfo"/> class.
///</summary>
[TestClass]
public sealed class ErrorInfoTests
{
    #region Public methods
    [TestMethod]
    public void Constructor_AllParameters_SetsProperties()
    {
        ErrorInfo error = new ErrorInfo("Bad value.", ValidationSeverity.Warning, "Age");

        Assert.AreEqual("Bad value.", error.Message);
        Assert.AreEqual(ValidationSeverity.Warning, error.Severity);
        Assert.AreEqual("Age", error.PropertyName);
    }

    [TestMethod]
    public void Constructor_Defaults_SeverityIsErrorAndPropertyNameIsNull()
    {
        ErrorInfo error = new ErrorInfo("Something failed.");

        Assert.AreEqual("Something failed.", error.Message);
        Assert.AreEqual(ValidationSeverity.Error, error.Severity);
        Assert.IsNull(error.PropertyName);
    }

    [TestMethod]
    public void Equality_DifferentMessage_AreNotEqual()
    {
        ErrorInfo a = new ErrorInfo("msg1");
        ErrorInfo b = new ErrorInfo("msg2");

        Assert.AreNotEqual(a, b);
    }

    [TestMethod]
    public void Equality_SameValues_AreEqual()
    {
        ErrorInfo a = new ErrorInfo("msg", ValidationSeverity.Warning, "Prop");
        ErrorInfo b = new ErrorInfo("msg", ValidationSeverity.Warning, "Prop");

        Assert.AreEqual(a, b);
    }

    [TestMethod]
    public void ToString_WithoutPropertyName_OmitsPropertyName()
    {
        ErrorInfo error = new ErrorInfo("Object invalid.", ValidationSeverity.Info);

        Assert.AreEqual("[Info] Object invalid.", error.ToString());
    }

    [TestMethod]
    public void ToString_WithPropertyName_IncludesPropertyName()
    {
        ErrorInfo error = new ErrorInfo("Required.", ValidationSeverity.Error, "Name");

        Assert.AreEqual("[Error] Name: Required.", error.ToString());
    }
    #endregion
}
