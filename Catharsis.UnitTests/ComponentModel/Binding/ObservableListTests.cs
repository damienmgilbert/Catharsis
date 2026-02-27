using System.Collections.Specialized;
using System.ComponentModel;

using Catharsis.ComponentModel.Binding;

namespace Catharsis.UnitTests.ComponentModel.Binding;

[TestClass]
public sealed class ObservableListTests
{
    [TestMethod]
    public void Constructor_Default_IsEmpty()
    {
        var list = new ObservableList<int>();

        Assert.AreEqual(0, list.Count);
    }

    [TestMethod]
    public void Constructor_WithCollection_CopiesItems()
    {
        var list = new ObservableList<int>([1, 2, 3]);

        Assert.AreEqual(3, list.Count);
    }

    [TestMethod]
    public void Add_RaisesCollectionChanged()
    {
        var list = new ObservableList<int>();
        NotifyCollectionChangedAction? action = null;
        list.CollectionChanged += (s, e) => action = e.Action;

        list.Add(42);

        Assert.AreEqual(NotifyCollectionChangedAction.Add, action);
    }

    [TestMethod]
    public void Remove_RaisesCollectionChanged()
    {
        var list = new ObservableList<int>([1, 2, 3]);
        NotifyCollectionChangedAction? action = null;
        list.CollectionChanged += (s, e) => action = e.Action;

        list.Remove(2);

        Assert.AreEqual(NotifyCollectionChangedAction.Remove, action);
    }

    [TestMethod]
    public void AddRange_AddsAllItems()
    {
        var list = new ObservableList<int>();

        list.AddRange([1, 2, 3, 4, 5]);

        Assert.AreEqual(5, list.Count);
    }

    [TestMethod]
    public void AddRange_NullItems_ThrowsArgumentNullException()
    {
        var list = new ObservableList<int>();

        Assert.ThrowsExactly<ArgumentNullException>(() => list.AddRange(null!));
    }

    [TestMethod]
    public void AddRange_RaisesSingleResetNotification()
    {
        var list = new ObservableList<int>();
        var collectionChangedCount = 0;
        list.CollectionChanged += (s, e) => collectionChangedCount++;

        list.AddRange([1, 2, 3]);

        Assert.AreEqual(1, collectionChangedCount);
    }

    [TestMethod]
    public void RemoveAll_RemovesMatchingItems()
    {
        var list = new ObservableList<int>([1, 2, 3, 4, 5]);

        var removed = list.RemoveAll(x => x > 3);

        Assert.AreEqual(2, removed);
        Assert.AreEqual(3, list.Count);
    }

    [TestMethod]
    public void RemoveAll_NullPredicate_ThrowsArgumentNullException()
    {
        var list = new ObservableList<int>();

        Assert.ThrowsExactly<ArgumentNullException>(() => list.RemoveAll(null!));
    }

    [TestMethod]
    public void RemoveAll_NoMatches_ReturnsZero()
    {
        var list = new ObservableList<int>([1, 2, 3]);

        var removed = list.RemoveAll(x => x > 10);

        Assert.AreEqual(0, removed);
    }

    [TestMethod]
    public void ReplaceAll_ReplacesContent()
    {
        var list = new ObservableList<int>([1, 2, 3]);

        list.ReplaceAll([10, 20]);

        Assert.AreEqual(2, list.Count);
        Assert.AreEqual(10, list[0]);
        Assert.AreEqual(20, list[1]);
    }

    [TestMethod]
    public void ReplaceAll_NullItems_ThrowsArgumentNullException()
    {
        var list = new ObservableList<int>();

        Assert.ThrowsExactly<ArgumentNullException>(() => list.ReplaceAll(null!));
    }

    [TestMethod]
    public void SuppressNotifications_SuppressesDuringScope()
    {
        var list = new ObservableList<int>();
        var collectionChangedCount = 0;
        list.CollectionChanged += (s, e) => collectionChangedCount++;

        using (list.SuppressNotifications())
        {
            list.Add(1);
            list.Add(2);
            Assert.AreEqual(0, collectionChangedCount);
        }

        // Reset raised on scope disposal
        Assert.AreEqual(1, collectionChangedCount);
    }

    [TestMethod]
    public void IsNotificationSuppressed_ReturnsTrueDuringScope()
    {
        var list = new ObservableList<int>();

        using (list.SuppressNotifications())
        {
            Assert.IsTrue(list.IsNotificationSuppressed);
        }

        Assert.IsFalse(list.IsNotificationSuppressed);
    }

    [TestMethod]
    public void SuppressNotifications_Nested_OnlyRaisesOnOuterDispose()
    {
        var list = new ObservableList<int>();
        var resetCount = 0;
        list.CollectionChanged += (s, e) =>
        {
            if (e.Action == NotifyCollectionChangedAction.Reset)
                resetCount++;
        };

        using (list.SuppressNotifications())
        {
            using (list.SuppressNotifications())
            {
                list.Add(1);
            }
            Assert.AreEqual(0, resetCount);
        }

        Assert.AreEqual(1, resetCount);
    }
}
