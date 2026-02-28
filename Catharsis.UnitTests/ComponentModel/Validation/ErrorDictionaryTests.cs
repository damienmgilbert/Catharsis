using Catharsis.ComponentModel.Validation;

namespace Catharsis.UnitTests.ComponentModel.Validation;

[TestClass]
public sealed class ErrorDictionaryTests
{
    #region Public methods
    [TestMethod]
    public void AddError_NullError_ThrowsArgumentNullException()
    {
        ErrorDictionary dict = new ErrorDictionary();

        Assert.ThrowsExactly<ArgumentNullException>(() => dict.AddError("Name", null!));
    }

    [TestMethod]
    public void AddError_NullPropertyName_ThrowsArgumentNullException()
    {
        ErrorDictionary dict = new ErrorDictionary();

        Assert.ThrowsExactly<ArgumentNullException>(() => dict.AddError(null!, new ErrorInfo("msg")));
    }

    [TestMethod]
    public void AddError_SingleError_HasErrorsReturnsTrue()
    {
        ErrorDictionary dict = new ErrorDictionary();

        dict.AddError("Name", new ErrorInfo("Required."));

        Assert.IsTrue(dict.HasErrors);
    }

    [TestMethod]
    public void ClearAll_RemovesAllErrors()
    {
        ErrorDictionary dict = new ErrorDictionary();
        dict.AddError("Name", new ErrorInfo("err"));
        dict.AddError("Age", new ErrorInfo("err"));

        dict.ClearAll();

        Assert.IsFalse(dict.HasErrors);
        Assert.AreEqual(0, dict.PropertyErrorCount);
        Assert.AreEqual(0, dict.TotalErrorCount);
    }

    [TestMethod]
    public void ClearErrors_RemovesErrorsForProperty()
    {
        ErrorDictionary dict = new ErrorDictionary();
        dict.AddError("Name", new ErrorInfo("err"));
        dict.AddError("Age", new ErrorInfo("err"));

        dict.ClearErrors("Name");

        Assert.IsEmpty(dict.GetErrorInfos("Name"));
        Assert.HasCount(1, dict.GetErrorInfos("Age"));
    }

    [TestMethod]
    public void ErrorsChanged_NotRaisedWhenClearingEmptyProperty()
    {
        ErrorDictionary dict = new ErrorDictionary();
        bool raised = false;
        dict.ErrorsChanged += (s, e) => raised = true;

        dict.ClearErrors("NonExistent");

        Assert.IsFalse(raised);
    }

    [TestMethod]
    public void ErrorsChanged_RaisedForEachPropertyOnClearAll()
    {
        ErrorDictionary dict = new ErrorDictionary();
        dict.AddError("Name", new ErrorInfo("err"));
        dict.AddError("Age", new ErrorInfo("err"));

        List<string> changedProperties = new List<string>();
        dict.ErrorsChanged += (s, e) => changedProperties.Add(e.PropertyName!);

        dict.ClearAll();

        Assert.HasCount(2, changedProperties);
        CollectionAssert.Contains(changedProperties, "Name");
        CollectionAssert.Contains(changedProperties, "Age");
    }

    [TestMethod]
    public void ErrorsChanged_RaisedOnAddError()
    {
        ErrorDictionary dict = new ErrorDictionary();
        string? changedProperty = null;
        dict.ErrorsChanged += (s, e) => changedProperty = e.PropertyName;

        dict.AddError("Name", new ErrorInfo("err"));

        Assert.AreEqual("Name", changedProperty);
    }

    [TestMethod]
    public void ErrorsChanged_RaisedOnClearErrors()
    {
        ErrorDictionary dict = new ErrorDictionary();
        dict.AddError("Name", new ErrorInfo("err"));

        string? changedProperty = null;
        dict.ErrorsChanged += (s, e) => changedProperty = e.PropertyName;

        dict.ClearErrors("Name");

        Assert.AreEqual("Name", changedProperty);
    }

    [TestMethod]
    public void GetErrorInfos_ReturnsTypedList()
    {
        ErrorDictionary dict = new ErrorDictionary();
        dict.AddError("Name", new ErrorInfo("Required."));

        IReadOnlyList<ErrorInfo> errors = dict.GetErrorInfos("Name");

        Assert.HasCount(1, errors);
        Assert.AreEqual("Required.", errors[0].Message);
    }

    [TestMethod]
    public void GetErrors_ByPropertyName_ReturnsOnlyThatProperty()
    {
        ErrorDictionary dict = new ErrorDictionary();
        dict.AddError("Name", new ErrorInfo("Required."));
        dict.AddError("Age", new ErrorInfo("Out of range."));

        List<ErrorInfo> errors = dict.GetErrors("Name").Cast<ErrorInfo>().ToList();

        Assert.HasCount(1, errors);
        Assert.AreEqual("Required.", errors[0].Message);
    }

    [TestMethod]
    public void GetErrors_NullOrEmpty_ReturnsAllErrors()
    {
        ErrorDictionary dict = new ErrorDictionary();
        dict.AddError("Name", new ErrorInfo("err1"));
        dict.AddError("Age", new ErrorInfo("err2"));

        List<ErrorInfo> errors = dict.GetErrors(null).Cast<ErrorInfo>().ToList();

        Assert.HasCount(2, errors);
    }

    [TestMethod]
    public void GetErrors_UnknownProperty_ReturnsEmptyList()
    {
        ErrorDictionary dict = new ErrorDictionary();

        List<ErrorInfo> errors = dict.GetErrors("Unknown").Cast<ErrorInfo>().ToList();

        Assert.IsEmpty(errors);
    }

    [TestMethod]
    public void HasErrors_NoErrors_ReturnsFalse()
    {
        ErrorDictionary dict = new ErrorDictionary();

        Assert.IsFalse(dict.HasErrors);
    }

    [TestMethod]
    public void PropertyErrorCount_ReturnsDistinctPropertyCount()
    {
        ErrorDictionary dict = new ErrorDictionary();
        dict.AddError("Name", new ErrorInfo("err1"));
        dict.AddError("Name", new ErrorInfo("err2"));
        dict.AddError("Age", new ErrorInfo("err3"));

        Assert.AreEqual(2, dict.PropertyErrorCount);
    }

    [TestMethod]
    public void SetErrors_EmptyList_ClearsErrors()
    {
        ErrorDictionary dict = new ErrorDictionary();
        dict.AddError("Name", new ErrorInfo("err"));

        dict.SetErrors("Name", []);

        Assert.IsFalse(dict.HasErrors);
    }

    [TestMethod]
    public void SetErrors_NullPropertyName_ThrowsArgumentNullException()
    {
        ErrorDictionary dict = new ErrorDictionary();

        Assert.ThrowsExactly<ArgumentNullException>(() => dict.SetErrors(null!, [ new ErrorInfo("msg") ]));
    }

    [TestMethod]
    public void SetErrors_ReplacesExistingErrors()
    {
        ErrorDictionary dict = new ErrorDictionary();
        dict.AddError("Name", new ErrorInfo("old"));

        dict.SetErrors("Name", [ new ErrorInfo("new1"), new ErrorInfo("new2") ]);

        IReadOnlyList<ErrorInfo> errors = dict.GetErrorInfos("Name");
        Assert.HasCount(2, errors);
        Assert.AreEqual("new1", errors[0].Message);
    }

    [TestMethod]
    public void TotalErrorCount_ReturnsSumOfAllErrors()
    {
        ErrorDictionary dict = new ErrorDictionary();
        dict.AddError("Name", new ErrorInfo("err1"));
        dict.AddError("Name", new ErrorInfo("err2"));
        dict.AddError("Age", new ErrorInfo("err3"));

        Assert.AreEqual(3, dict.TotalErrorCount);
    }
    #endregion
}
