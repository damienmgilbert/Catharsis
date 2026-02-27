using System.ComponentModel;

using Catharsis.ComponentModel.Validation;

namespace Catharsis.UnitTests.ComponentModel.Validation;

[TestClass]
public sealed class ErrorDictionaryTests
{
    [TestMethod]
    public void HasErrors_NoErrors_ReturnsFalse()
    {
        var dict = new ErrorDictionary();

        Assert.IsFalse(dict.HasErrors);
    }

    [TestMethod]
    public void AddError_SingleError_HasErrorsReturnsTrue()
    {
        var dict = new ErrorDictionary();

        dict.AddError("Name", new ErrorInfo("Required."));

        Assert.IsTrue(dict.HasErrors);
    }

    [TestMethod]
    public void AddError_NullPropertyName_ThrowsArgumentNullException()
    {
        var dict = new ErrorDictionary();

        Assert.ThrowsExactly<ArgumentNullException>(
            () => dict.AddError(null!, new ErrorInfo("msg")));
    }

    [TestMethod]
    public void AddError_NullError_ThrowsArgumentNullException()
    {
        var dict = new ErrorDictionary();

        Assert.ThrowsExactly<ArgumentNullException>(
            () => dict.AddError("Name", null!));
    }

    [TestMethod]
    public void GetErrors_ByPropertyName_ReturnsOnlyThatProperty()
    {
        var dict = new ErrorDictionary();
        dict.AddError("Name", new ErrorInfo("Required."));
        dict.AddError("Age", new ErrorInfo("Out of range."));

        var errors = dict.GetErrors("Name").Cast<ErrorInfo>().ToList();

        Assert.AreEqual(1, errors.Count);
        Assert.AreEqual("Required.", errors[0].Message);
    }

    [TestMethod]
    public void GetErrors_NullOrEmpty_ReturnsAllErrors()
    {
        var dict = new ErrorDictionary();
        dict.AddError("Name", new ErrorInfo("err1"));
        dict.AddError("Age", new ErrorInfo("err2"));

        var errors = dict.GetErrors(null).Cast<ErrorInfo>().ToList();

        Assert.AreEqual(2, errors.Count);
    }

    [TestMethod]
    public void GetErrors_UnknownProperty_ReturnsEmptyList()
    {
        var dict = new ErrorDictionary();

        var errors = dict.GetErrors("Unknown").Cast<ErrorInfo>().ToList();

        Assert.AreEqual(0, errors.Count);
    }

    [TestMethod]
    public void GetErrorInfos_ReturnsTypedList()
    {
        var dict = new ErrorDictionary();
        dict.AddError("Name", new ErrorInfo("Required."));

        IReadOnlyList<ErrorInfo> errors = dict.GetErrorInfos("Name");

        Assert.AreEqual(1, errors.Count);
        Assert.AreEqual("Required.", errors[0].Message);
    }

    [TestMethod]
    public void SetErrors_ReplacesExistingErrors()
    {
        var dict = new ErrorDictionary();
        dict.AddError("Name", new ErrorInfo("old"));

        dict.SetErrors("Name", [new ErrorInfo("new1"), new ErrorInfo("new2")]);

        var errors = dict.GetErrorInfos("Name");
        Assert.AreEqual(2, errors.Count);
        Assert.AreEqual("new1", errors[0].Message);
    }

    [TestMethod]
    public void SetErrors_EmptyList_ClearsErrors()
    {
        var dict = new ErrorDictionary();
        dict.AddError("Name", new ErrorInfo("err"));

        dict.SetErrors("Name", []);

        Assert.IsFalse(dict.HasErrors);
    }

    [TestMethod]
    public void SetErrors_NullPropertyName_ThrowsArgumentNullException()
    {
        var dict = new ErrorDictionary();

        Assert.ThrowsExactly<ArgumentNullException>(
            () => dict.SetErrors(null!, [new ErrorInfo("msg")]));
    }

    [TestMethod]
    public void ClearErrors_RemovesErrorsForProperty()
    {
        var dict = new ErrorDictionary();
        dict.AddError("Name", new ErrorInfo("err"));
        dict.AddError("Age", new ErrorInfo("err"));

        dict.ClearErrors("Name");

        Assert.AreEqual(0, dict.GetErrorInfos("Name").Count);
        Assert.AreEqual(1, dict.GetErrorInfos("Age").Count);
    }

    [TestMethod]
    public void ClearAll_RemovesAllErrors()
    {
        var dict = new ErrorDictionary();
        dict.AddError("Name", new ErrorInfo("err"));
        dict.AddError("Age", new ErrorInfo("err"));

        dict.ClearAll();

        Assert.IsFalse(dict.HasErrors);
        Assert.AreEqual(0, dict.PropertyErrorCount);
        Assert.AreEqual(0, dict.TotalErrorCount);
    }

    [TestMethod]
    public void PropertyErrorCount_ReturnsDistinctPropertyCount()
    {
        var dict = new ErrorDictionary();
        dict.AddError("Name", new ErrorInfo("err1"));
        dict.AddError("Name", new ErrorInfo("err2"));
        dict.AddError("Age", new ErrorInfo("err3"));

        Assert.AreEqual(2, dict.PropertyErrorCount);
    }

    [TestMethod]
    public void TotalErrorCount_ReturnsSumOfAllErrors()
    {
        var dict = new ErrorDictionary();
        dict.AddError("Name", new ErrorInfo("err1"));
        dict.AddError("Name", new ErrorInfo("err2"));
        dict.AddError("Age", new ErrorInfo("err3"));

        Assert.AreEqual(3, dict.TotalErrorCount);
    }

    [TestMethod]
    public void ErrorsChanged_RaisedOnAddError()
    {
        var dict = new ErrorDictionary();
        string? changedProperty = null;
        dict.ErrorsChanged += (s, e) => changedProperty = e.PropertyName;

        dict.AddError("Name", new ErrorInfo("err"));

        Assert.AreEqual("Name", changedProperty);
    }

    [TestMethod]
    public void ErrorsChanged_RaisedOnClearErrors()
    {
        var dict = new ErrorDictionary();
        dict.AddError("Name", new ErrorInfo("err"));

        string? changedProperty = null;
        dict.ErrorsChanged += (s, e) => changedProperty = e.PropertyName;

        dict.ClearErrors("Name");

        Assert.AreEqual("Name", changedProperty);
    }

    [TestMethod]
    public void ErrorsChanged_RaisedForEachPropertyOnClearAll()
    {
        var dict = new ErrorDictionary();
        dict.AddError("Name", new ErrorInfo("err"));
        dict.AddError("Age", new ErrorInfo("err"));

        var changedProperties = new List<string>();
        dict.ErrorsChanged += (s, e) => changedProperties.Add(e.PropertyName!);

        dict.ClearAll();

        Assert.AreEqual(2, changedProperties.Count);
        CollectionAssert.Contains(changedProperties, "Name");
        CollectionAssert.Contains(changedProperties, "Age");
    }

    [TestMethod]
    public void ErrorsChanged_NotRaisedWhenClearingEmptyProperty()
    {
        var dict = new ErrorDictionary();
        var raised = false;
        dict.ErrorsChanged += (s, e) => raised = true;

        dict.ClearErrors("NonExistent");

        Assert.IsFalse(raised);
    }
}
