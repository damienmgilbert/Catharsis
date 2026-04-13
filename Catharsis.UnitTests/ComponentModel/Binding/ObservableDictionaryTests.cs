using System.Collections.Specialized;
using Catharsis.ComponentModel.Binding;

namespace Catharsis.UnitTests.ComponentModel.Binding;

///<summary>
///Unit tests for the <see cref="ObservableDictionary"/> class.
///</summary>
[TestClass]
public sealed class ObservableDictionaryTests
{
    #region Public methods
    [TestMethod]
    public void Add_RaisesCollectionChanged_Add()
    {
        ObservableDictionary<string, int> dict = [];
        NotifyCollectionChangedAction? action = null;
        dict.CollectionChanged += (s, e) => action = e.Action;

        dict.Add("key", 42);

        Assert.AreEqual(NotifyCollectionChangedAction.Add, action);
    }

    [TestMethod]
    public void Add_RaisesPropertyChanged_Count()
    {
        ObservableDictionary<string, int> dict = [];
        List<string> changedProperties = [];
        dict.PropertyChanged += (s, e) => changedProperties.Add(e.PropertyName!);

        dict.Add("key", 42);

        CollectionAssert.Contains(changedProperties, "Count");
    }

    [TestMethod]
    public void Clear_EmptyDictionary_NoNotification()
    {
        ObservableDictionary<string, int> dict = [];
        bool raised = false;
        dict.CollectionChanged += (s, e) => raised = true;

        dict.Clear();

        Assert.IsFalse(raised);
    }

    [TestMethod]
    public void Clear_RaisesReset()
    {
        ObservableDictionary<string, int> dict = new() { { "a", 1 }, { "b", 2 } };
        NotifyCollectionChangedAction? action = null;
        dict.CollectionChanged += (s, e) => action = e.Action;

        dict.Clear();

        Assert.AreEqual(NotifyCollectionChangedAction.Reset, action);
        Assert.IsEmpty(dict);
    }

    [TestMethod]
    public void Constructor_Default_IsEmpty()
    {
        ObservableDictionary<string, int> dict = [];

        Assert.IsEmpty(dict);
    }

    [TestMethod]
    public void Constructor_NullDictionary_ThrowsArgumentNullException() { Assert.ThrowsExactly<ArgumentNullException>(static () => new ObservableDictionary<string, int>((IDictionary<string, int>)null!)); }
    [TestMethod]
    public void Constructor_WithComparer_UsesComparer()
    {
        ObservableDictionary<string, int> dict = new(StringComparer.OrdinalIgnoreCase);

        dict.Add("key", 1);
        Assert.IsTrue(dict.ContainsKey("KEY"));
    }

    [TestMethod]
    public void Constructor_WithDictionary_CopiesEntries()
    {
        Dictionary<string, int> source = new() { ["a"] = 1, ["b"] = 2 };
        ObservableDictionary<string, int> dict = new(source);

        Assert.HasCount(2, dict);
        Assert.AreEqual(1, dict["a"]);
    }

    [TestMethod]
    public void ContainsKey_ReturnsCorrectResult()
    {
        ObservableDictionary<string, int> dict = new() { { "key", 42 } };

        Assert.IsTrue(dict.ContainsKey("key"));
        Assert.IsFalse(dict.ContainsKey("missing"));
    }

    [TestMethod]
    public void Enumeration_ReturnsAllPairs()
    {
        ObservableDictionary<string, int> dict = new() { { "a", 1 }, { "b", 2 } };

        List<KeyValuePair<string, int>> pairs = [.. dict];

        Assert.HasCount(2, pairs);
    }

    [TestMethod]
    public void Indexer_Set_ExistingKey_RaisesReplace()
    {
        ObservableDictionary<string, int> dict = new() { { "key", 1 } };
        NotifyCollectionChangedAction? action = null;
        dict.CollectionChanged += (s, e) => action = e.Action;

        dict["key"] = 2;

        Assert.AreEqual(NotifyCollectionChangedAction.Replace, action);
    }

    [TestMethod]
    public void Indexer_Set_NewKey_RaisesAdd()
    {
        ObservableDictionary<string, int> dict = [];
        NotifyCollectionChangedAction? action = null;
        dict.CollectionChanged += (s, e) => action = e.Action;

        dict["key"] = 42;

        Assert.AreEqual(NotifyCollectionChangedAction.Add, action);
    }

    [TestMethod]
    public void Indexer_Set_SameValue_NoNotification()
    {
        ObservableDictionary<string, int> dict = new() { { "key", 42 } };
        bool raised = false;
        dict.CollectionChanged += (s, e) => raised = true;

        dict["key"] = 42;

        Assert.IsFalse(raised);
    }

    [TestMethod]
    public void Keys_ReturnsAllKeys()
    {
        ObservableDictionary<string, int> dict = new() { { "a", 1 }, { "b", 2 } };

        Assert.HasCount(2, dict.Keys);
    }

    [TestMethod]
    public void Remove_ExistingKey_RaisesCollectionChanged_Remove()
    {
        ObservableDictionary<string, int> dict = new() { { "key", 42 } };
        NotifyCollectionChangedAction? action = null;
        dict.CollectionChanged += (s, e) => action = e.Action;

        bool removed = dict.Remove("key");

        Assert.IsTrue(removed);
        Assert.AreEqual(NotifyCollectionChangedAction.Remove, action);
    }

    [TestMethod]
    public void Remove_NonExistentKey_ReturnsFalse()
    {
        ObservableDictionary<string, int> dict = [];

        Assert.IsFalse(dict.Remove("missing"));
    }

    [TestMethod]
    public void SuppressNotifications_Nested_OnlyRaisesOnOuterDispose()
    {
        ObservableDictionary<string, int> dict = [];
        int resetCount = 0;
        dict.CollectionChanged += (s, e) =>
        {
            if(e.Action == NotifyCollectionChangedAction.Reset)
            {
                resetCount++;
            }
        };

        using(dict.SuppressNotifications())
        {
            using(dict.SuppressNotifications())
            {
                dict.Add("a", 1);
            }
            Assert.AreEqual(0, resetCount);
        }

        Assert.AreEqual(1, resetCount);
    }

    [TestMethod]
    public void SuppressNotifications_SuppressesDuringScope()
    {
        ObservableDictionary<string, int> dict = [];
        int collectionChangedCount = 0;
        dict.CollectionChanged += (s, e) => collectionChangedCount++;

        using(dict.SuppressNotifications())
        {
            dict.Add("a", 1);
            dict.Add("b", 2);
            Assert.AreEqual(0, collectionChangedCount);
        }

        // Reset raised on scope disposal
        Assert.AreEqual(1, collectionChangedCount);
    }

    [TestMethod]
    public void TryGetValue_ExistingKey_ReturnsTrueAndValue()
    {
        ObservableDictionary<string, int> dict = new() { { "key", 42 } };

        bool found = dict.TryGetValue("key", out int value);

        Assert.IsTrue(found);
        Assert.AreEqual(42, value);
    }

    [TestMethod]
    public void TryGetValue_MissingKey_ReturnsFalse()
    {
        ObservableDictionary<string, int> dict = [];

        Assert.IsFalse(dict.TryGetValue("missing", out _));
    }

    [TestMethod]
    public void Values_ReturnsAllValues()
    {
        ObservableDictionary<string, int> dict = new() { { "a", 1 }, { "b", 2 } };

        Assert.HasCount(2, dict.Values);
    }
    #endregion
}
