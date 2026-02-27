using Catharsis.ComponentModel;
using System.Collections;
using System.ComponentModel;

namespace Catharsis.UnitTests.ComponentModel;

[TestClass]
public class DataErrorInfoAdapterTests
{
    private sealed class TestValidatable : INotifyDataErrorInfo
    {
        private readonly Dictionary<string, List<string>> _errors = [];

        public bool HasErrors => _errors.Count > 0;

        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

        public IEnumerable GetErrors(string? propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                return _errors.SelectMany(kvp => kvp.Value);
            }

            return _errors.TryGetValue(propertyName, out var list) ? list : [];
        }

        public void AddError(string propertyName, string error)
        {
            if (!_errors.TryGetValue(propertyName, out var list))
            {
                list = [];
                _errors[propertyName] = list;
            }

            list.Add(error);
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }

        public void ClearErrors()
        {
            _errors.Clear();
        }
    }

    [TestMethod]
    public void Indexer_ReturnsErrorForProperty()
    {
        var source = new TestValidatable();
        source.AddError("Name", "Name is required.");
        var adapter = new DataErrorInfoAdapter(source);

        Assert.AreEqual("Name is required.", adapter["Name"]);
    }

    [TestMethod]
    public void Indexer_NoErrors_ReturnsEmpty()
    {
        var source = new TestValidatable();
        var adapter = new DataErrorInfoAdapter(source);

        Assert.AreEqual(string.Empty, adapter["Name"]);
    }

    [TestMethod]
    public void Error_AggregatesAllErrors()
    {
        var source = new TestValidatable();
        source.AddError("Name", "Name is required.");
        source.AddError("Age", "Age must be positive.");
        var adapter = new DataErrorInfoAdapter(source);

        var error = adapter.Error;

        Assert.IsTrue(error.Contains("Name is required."));
        Assert.IsTrue(error.Contains("Age must be positive."));
    }

    [TestMethod]
    public void Error_NoErrors_ReturnsEmpty()
    {
        var source = new TestValidatable();
        var adapter = new DataErrorInfoAdapter(source);

        Assert.AreEqual(string.Empty, adapter.Error);
    }

    [TestMethod]
    public void Indexer_MultipleErrors_JoinsThem()
    {
        var source = new TestValidatable();
        source.AddError("Name", "Too short.");
        source.AddError("Name", "Contains invalid characters.");
        var adapter = new DataErrorInfoAdapter(source);

        var result = adapter["Name"];

        Assert.IsTrue(result.Contains("Too short."));
        Assert.IsTrue(result.Contains("Contains invalid characters."));
    }

    [TestMethod]
    public void Constructor_NullSource_Throws()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => new DataErrorInfoAdapter(null!));
    }
}
