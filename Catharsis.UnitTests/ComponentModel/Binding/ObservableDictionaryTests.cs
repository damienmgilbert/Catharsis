using Catharsis.ComponentModel.Binding;
using System.Collections.Specialized;

namespace Catharsis.UnitTests.ComponentModel.Binding;

[TestClass]
public sealed class ObservableDictionaryTests
{
    [TestMethod]
    public void Constructor_Default_IsEmpty()
    {
        var dict = new ObservableDictionary<string, int>();

        Assert.AreEqual(0, dict.Count);
    }

    [TestMethod]
    public void Constructor_WithComparer_UsesComparer()
    {
        var dict = new ObservableDictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        dict.Add("key", 1);
        Assert.IsTrue(dict.ContainsKey("KEY"));
    }

    [TestMethod]
    public void Constructor_WithDictionary_CopiesEntries()
    {
        var source = new Dictionary<string, int> { ["a"] = 1, ["b"] = 2 };
        var dict = new ObservableDictionary<string, int>(source);

        Assert.AreEqual(2, dict.Count);
        Assert.AreEqual(1, dict["a"]);
    }

    [TestMethod]
    public void Constructor_NullDictionary_ThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(
            () => new ObservableDictionary<string, int>((IDictionary<string, int>)null!));
    }

    [TestMethod]
    public void Add_RaisesCollectionChanged_Add()
    {
        var dict = new ObservableDictionary<string, int>();
        NotifyCollectionChangedAction? action = null;
        dict.CollectionChanged += (s, e) => action = e.Action;

        dict.Add("key", 42);

        Assert.AreEqual(NotifyCollectionChangedAction.Add, action);
    }

    [TestMethod]
    public void Add_RaisesPropertyChanged_Count()
    {
        var dict = new ObservableDictionary<string, int>();
        var changedProperties = new List<string>();
        dict.PropertyChanged += (s, e) => changedProperties.Add(e.PropertyName!);

        dict.Add("key", 42);

        CollectionAssert.Contains(changedProperties, "Count");
    }

    [TestMethod]
    public void Remove_ExistingKey_RaisesCollectionChanged_Remove()
    {
        var dict = new ObservableDictionary<string, int> { { "key", 42 } };
        NotifyCollectionChangedAction? action = null;
        dict.CollectionChanged += (s, e) => action = e.Action;

        var removed = dict.Remove("key");

        Assert.IsTrue(removed);
        Assert.AreEqual(NotifyCollectionChangedAction.Remove, action);
    }

    [TestMethod]
    public void Remove_NonExistentKey_ReturnsFalse()
    {
        var dict = new ObservableDictionary<string, int>();

        Assert.IsFalse(dict.Remove("missing"));
    }

    [TestMethod]
    public void Indexer_Set_NewKey_RaisesAdd()
    {
        var dict = new ObservableDictionary<string, int>();
        NotifyCollectionChangedAction? action = null;
        dict.CollectionChanged += (s, e) => action = e.Action;

        dict["key"] = 42;

        Assert.AreEqual(NotifyCollectionChangedAction.Add, action);
    }

    [TestMethod]
    public void Indexer_Set_ExistingKey_RaisesReplace()
    {
        var dict = new ObservableDictionary<string, int> { { "key", 1 } };
        NotifyCollectionChangedAction? action = null;
        dict.CollectionChanged += (s, e) => action = e.Action;

        dict["key"] = 2;

        Assert.AreEqual(NotifyCollectionChangedAction.Replace, action);
    }

    [TestMethod]
    public void Indexer_Set_SameValue_NoNotification()
    {
        var dict = new ObservableDictionary<string, int> { { "key", 42 } };
        var raised = false;
        dict.CollectionChanged += (s, e) => raised = true;

        dict["key"] = 42;

        Assert.IsFalse(raised);
    }

    [TestMethod]
    public void Clear_RaisesReset()
    {
        var dict = new ObservableDictionary<string, int> { { "a", 1 }, { "b", 2 } };
        NotifyCollectionChangedAction? action = null;
        dict.CollectionChanged += (s, e) => action = e.Action;

        dict.Clear();

        Assert.AreEqual(NotifyCollectionChangedAction.Reset, action);
        Assert.AreEqual(0, dict.Count);
    }

    [TestMethod]
    public void Clear_EmptyDictionary_NoNotification()
    {
        var dict = new ObservableDictionary<string, int>();
        var raised = false;
        dict.CollectionChanged += (s, e) => raised = true;

        dict.Clear();

        Assert.IsFalse(raised);
    }

    [TestMethod]
    public void ContainsKey_ReturnsCorrectResult()
    {
        var dict = new ObservableDictionary<string, int> { { "key", 42 } };

        Assert.IsTrue(dict.ContainsKey("key"));
        Assert.IsFalse(dict.ContainsKey("missing"));
    }

    [TestMethod]
    public void TryGetValue_ExistingKey_ReturnsTrueAndValue()
    {
        var dict = new ObservableDictionary<string, int> { { "key", 42 } };

        var found = dict.TryGetValue("key", out var value);

        Assert.IsTrue(found);
        Assert.AreEqual(42, value);
    }

    [TestMethod]
    public void TryGetValue_MissingKey_ReturnsFalse()
    {
        var dict = new ObservableDictionary<string, int>();

        Assert.IsFalse(dict.TryGetValue("missing", out _));
    }

    [TestMethod]
    public void SuppressNotifications_SuppressesDuringScope()
    {
        var dict = new ObservableDictionary<string, int>();
        var collectionChangedCount = 0;
        dict.CollectionChanged += (s, e) => collectionChangedCount++;

        using (dict.SuppressNotifications())
        {
            dict.Add("a", 1);
            dict.Add("b", 2);
            Assert.AreEqual(0, collectionChangedCount);
        }

        // Reset raised on scope disposal
        Assert.AreEqual(1, collectionChangedCount);
    }

    [TestMethod]
    public void SuppressNotifications_Nested_OnlyRaisesOnOuterDispose()
    {
        var dict = new ObservableDictionary<string, int>();
        var resetCount = 0;
        dict.CollectionChanged += (s, e) =>
        {
            if (e.Action == NotifyCollectionChangedAction.Reset)
                resetCount++;
        };

        using (dict.SuppressNotifications())
        {
            using (dict.SuppressNotifications())
            {
                dict.Add("a", 1);
            }
            Assert.AreEqual(0, resetCount);
        }

        Assert.AreEqual(1, resetCount);
    }

    [TestMethod]
    public void Enumeration_ReturnsAllPairs()
    {
        var dict = new ObservableDictionary<string, int> { { "a", 1 }, { "b", 2 } };

        var pairs = dict.ToList();

        Assert.AreEqual(2, pairs.Count);
    }

    [TestMethod]
    public void Keys_ReturnsAllKeys()
    {
        var dict = new ObservableDictionary<string, int> { { "a", 1 }, { "b", 2 } };

        Assert.AreEqual(2, dict.Keys.Count);
    }

    [TestMethod]
    public void Values_ReturnsAllValues()
    {
        var dict = new ObservableDictionary<string, int> { { "a", 1 }, { "b", 2 } };

        Assert.AreEqual(2, dict.Values.Count);
    }
}
