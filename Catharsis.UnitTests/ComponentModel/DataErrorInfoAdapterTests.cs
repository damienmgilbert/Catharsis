using System.Collections;
using System.ComponentModel;
using Catharsis.ComponentModel;

namespace Catharsis.UnitTests.ComponentModel;

///<summary>
///Unit tests for the <see cref="DataErrorInfoAdapter"/> class.
///</summary>
[TestClass]
public class DataErrorInfoAdapterTests
{
    #region Public methods
    [TestMethod]
    public void Constructor_NullSource_Throws() { Assert.ThrowsExactly<ArgumentNullException>(static () => new DataErrorInfoAdapter(null!)); }
    [TestMethod]
    public void Error_AggregatesAllErrors()
    {
        TestValidatable source = new TestValidatable();
        source.AddError("Name", "Name is required.");
        source.AddError("Age", "Age must be positive.");
        DataErrorInfoAdapter adapter = new DataErrorInfoAdapter(source);

        string error = adapter.Error;

        Assert.Contains("Name is required.", error);
        Assert.Contains("Age must be positive.", error);
    }

    [TestMethod]
    public void Error_NoErrors_ReturnsEmpty()
    {
        TestValidatable source = new TestValidatable();
        DataErrorInfoAdapter adapter = new DataErrorInfoAdapter(source);

        Assert.AreEqual(string.Empty, adapter.Error);
    }

    [TestMethod]
    public void Indexer_MultipleErrors_JoinsThem()
    {
        TestValidatable source = new TestValidatable();
        source.AddError("Name", "Too short.");
        source.AddError("Name", "Contains invalid characters.");
        DataErrorInfoAdapter adapter = new DataErrorInfoAdapter(source);

        string result = adapter["Name"];

        Assert.Contains("Too short.", result);
        Assert.Contains("Contains invalid characters.", result);
    }

    [TestMethod]
    public void Indexer_NoErrors_ReturnsEmpty()
    {
        TestValidatable source = new TestValidatable();
        DataErrorInfoAdapter adapter = new DataErrorInfoAdapter(source);

        Assert.AreEqual(string.Empty, adapter["Name"]);
    }

    [TestMethod]
    public void Indexer_ReturnsErrorForProperty()
    {
        TestValidatable source = new TestValidatable();
        source.AddError("Name", "Name is required.");
        DataErrorInfoAdapter adapter = new DataErrorInfoAdapter(source);

        Assert.AreEqual("Name is required.", adapter["Name"]);
    }
    #endregion

    sealed class TestValidatable : INotifyDataErrorInfo
    {
        #region Fields
        readonly Dictionary<string, List<string>> _errors = [];
        #endregion

        #region Events
        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;
        #endregion

        #region Public methods
        public void AddError(string propertyName, string error)
        {
            if(!_errors.TryGetValue(propertyName, out List<string>? list))
            {
                list = [];
                _errors[propertyName] = list;
            }

            list.Add(error);
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }

        public void ClearErrors() { _errors.Clear(); }

        public IEnumerable GetErrors(string? propertyName)
        {
            if(string.IsNullOrEmpty(propertyName))
            {
                return _errors.SelectMany(static kvp => kvp.Value);
            }

            return _errors.TryGetValue(propertyName, out List<string>? list) ? list : [];
        }
        #endregion

        #region Public properties
        public bool HasErrors => _errors.Count > 0;
        #endregion
    }
}
