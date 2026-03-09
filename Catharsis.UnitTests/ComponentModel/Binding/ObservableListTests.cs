using System.Collections.Specialized;
using Catharsis.ComponentModel.Binding;

namespace Catharsis.UnitTests.ComponentModel.Binding;

///<summary>
///Unit tests for the <see cref="ObservableList"/> class.
///</summary>
[TestClass]
public sealed class ObservableListTests
{
    #region Public methods
    [TestMethod]
    public void Add_RaisesCollectionChanged()
    {
        ObservableList<int> list = new ObservableList<int>();
        NotifyCollectionChangedAction? action = null;
        list.CollectionChanged += (s, e) => action = e.Action;

        list.Add(42);

        Assert.AreEqual(NotifyCollectionChangedAction.Add, action);
    }

    [TestMethod]
    public void AddRange_AddsAllItems()
    {
        ObservableList<int> list = new ObservableList<int>();

        list.AddRange([ 1, 2, 3, 4, 5 ]);

        Assert.HasCount(5, list);
    }

    [TestMethod]
    public void AddRange_NullItems_ThrowsArgumentNullException()
    {
        ObservableList<int> list = new ObservableList<int>();

        Assert.ThrowsExactly<ArgumentNullException>(() => list.AddRange(null!));
    }

    [TestMethod]
    public void AddRange_RaisesSingleResetNotification()
    {
        ObservableList<int> list = new ObservableList<int>();
        int collectionChangedCount = 0;
        list.CollectionChanged += (s, e) => collectionChangedCount++;

        list.AddRange([ 1, 2, 3 ]);

        Assert.AreEqual(1, collectionChangedCount);
    }

    [TestMethod]
    public void Constructor_Default_IsEmpty()
    {
        ObservableList<int> list = new ObservableList<int>();

        Assert.IsEmpty(list);
    }

    [TestMethod]
    public void Constructor_WithCollection_CopiesItems()
    {
        ObservableList<int> list = new ObservableList<int>([ 1, 2, 3 ]);

        Assert.HasCount(3, list);
    }

    [TestMethod]
    public void IsNotificationSuppressed_ReturnsTrueDuringScope()
    {
        ObservableList<int> list = new ObservableList<int>();

        using(list.SuppressNotifications())
        {
            Assert.IsTrue(list.IsNotificationSuppressed);
        }

        Assert.IsFalse(list.IsNotificationSuppressed);
    }

    [TestMethod]
    public void Remove_RaisesCollectionChanged()
    {
        ObservableList<int> list = new ObservableList<int>([ 1, 2, 3 ]);
        NotifyCollectionChangedAction? action = null;
        list.CollectionChanged += (s, e) => action = e.Action;

        list.Remove(2);

        Assert.AreEqual(NotifyCollectionChangedAction.Remove, action);
    }

    [TestMethod]
    public void RemoveAll_NoMatches_ReturnsZero()
    {
        ObservableList<int> list = new ObservableList<int>([ 1, 2, 3 ]);

        int removed = list.RemoveAll(static x => x > 10);

        Assert.AreEqual(0, removed);
    }

    [TestMethod]
    public void RemoveAll_NullPredicate_ThrowsArgumentNullException()
    {
        ObservableList<int> list = new ObservableList<int>();

        Assert.ThrowsExactly<ArgumentNullException>(() => list.RemoveAll(null!));
    }

    [TestMethod]
    public void RemoveAll_RemovesMatchingItems()
    {
        ObservableList<int> list = new ObservableList<int>([ 1, 2, 3, 4, 5 ]);

        int removed = list.RemoveAll(static x => x > 3);

        Assert.AreEqual(2, removed);
        Assert.HasCount(3, list);
    }

    [TestMethod]
    public void ReplaceAll_NullItems_ThrowsArgumentNullException()
    {
        ObservableList<int> list = new ObservableList<int>();

        Assert.ThrowsExactly<ArgumentNullException>(() => list.ReplaceAll(null!));
    }

    [TestMethod]
    public void ReplaceAll_ReplacesContent()
    {
        ObservableList<int> list = new ObservableList<int>([ 1, 2, 3 ]);

        list.ReplaceAll([ 10, 20 ]);

        Assert.HasCount(2, list);
        Assert.AreEqual(10, list[0]);
        Assert.AreEqual(20, list[1]);
    }

    [TestMethod]
    public void SuppressNotifications_Nested_OnlyRaisesOnOuterDispose()
    {
        ObservableList<int> list = new ObservableList<int>();
        int resetCount = 0;
        list.CollectionChanged += (s, e) =>
        {
            if(e.Action == NotifyCollectionChangedAction.Reset)
            {
                resetCount++;
            }
        };

        using(list.SuppressNotifications())
        {
            using(list.SuppressNotifications())
            {
                list.Add(1);
            }
            Assert.AreEqual(0, resetCount);
        }

        Assert.AreEqual(1, resetCount);
    }

    [TestMethod]
    public void SuppressNotifications_SuppressesDuringScope()
    {
        ObservableList<int> list = new ObservableList<int>();
        int collectionChangedCount = 0;
        list.CollectionChanged += (s, e) => collectionChangedCount++;

        using(list.SuppressNotifications())
        {
            list.Add(1);
            list.Add(2);
            Assert.AreEqual(0, collectionChangedCount);
        }

        // Reset raised on scope disposal
        Assert.AreEqual(1, collectionChangedCount);
    }
    #endregion
}
