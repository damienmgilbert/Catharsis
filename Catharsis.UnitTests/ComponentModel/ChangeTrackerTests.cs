using System.ComponentModel;
using Catharsis.ComponentModel;

namespace Catharsis.UnitTests.ComponentModel;

///<summary>
///Unit tests for the <see cref="ChangeTracker"/> class.
///</summary>
[TestClass]
public class ChangeTrackerTests
{
    #region Public methods
    [TestMethod]
    public void AcceptChanges_UpdatesOriginalAndResetsIsChanged()
    {
        ChangeTracker<string> tracker = new ChangeTracker<string>("hello");

        tracker.Value = "world";
        Assert.IsTrue(tracker.IsChanged);

        tracker.AcceptChanges();

        Assert.IsFalse(tracker.IsChanged);
        Assert.AreEqual("world", tracker.OriginalValue);
        Assert.AreEqual("world", tracker.Value);
    }

    [TestMethod]
    public void AcceptChanges_WhenNotChanged_DoesNothing()
    {
        ChangeTracker<int> tracker = new ChangeTracker<int>(5);
        bool changed = false;
        tracker.PropertyChanged += (_, _) => changed = true;

        tracker.AcceptChanges();

        Assert.IsFalse(changed);
    }

    [TestMethod]
    public void IChangeTracking_AcceptChanges_Works()
    {
        IChangeTracking tracking = new ChangeTracker<int>(5);

        Assert.IsFalse(tracking.IsChanged);
    }

    [TestMethod]
    public void InitialState_IsChangedIsFalse()
    {
        ChangeTracker<int> tracker = new ChangeTracker<int>(42);

        Assert.IsFalse(tracker.IsChanged);
        Assert.AreEqual(42, tracker.Value);
        Assert.AreEqual(42, tracker.OriginalValue);
    }

    [TestMethod]
    public void IRevertibleChangeTracking_RejectChanges_Works()
    {
        ChangeTracker<int> tracker = new ChangeTracker<int>(5);
        tracker.Value = 99;

        IRevertibleChangeTracking revertible = tracker;
        revertible.RejectChanges();

        Assert.AreEqual(5, tracker.Value);
        Assert.IsFalse(tracker.IsChanged);
    }

    [TestMethod]
    public void PropertyChanged_FiredForValueAndIsChanged()
    {
        ChangeTracker<int> tracker = new ChangeTracker<int>(0);
        List<string> changedProperties = new List<string>();
        tracker.PropertyChanged += (_, e) => changedProperties.Add(e.PropertyName!);

        tracker.Value = 10;

        CollectionAssert.Contains(changedProperties, nameof(ChangeTracker<int>.Value));
        CollectionAssert.Contains(changedProperties, nameof(ChangeTracker<int>.IsChanged));
    }

    [TestMethod]
    public void RejectChanges_RevertsValueAndResetsIsChanged()
    {
        ChangeTracker<string> tracker = new ChangeTracker<string>("hello");

        tracker.Value = "world";
        Assert.IsTrue(tracker.IsChanged);

        tracker.RejectChanges();

        Assert.IsFalse(tracker.IsChanged);
        Assert.AreEqual("hello", tracker.Value);
        Assert.AreEqual("hello", tracker.OriginalValue);
    }

    [TestMethod]
    public void RejectChanges_WhenNotChanged_DoesNothing()
    {
        ChangeTracker<int> tracker = new ChangeTracker<int>(5);
        bool changed = false;
        tracker.PropertyChanged += (_, _) => changed = true;

        tracker.RejectChanges();

        Assert.IsFalse(changed);
    }

    [TestMethod]
    public void SettingNewValue_SetsIsChangedToTrue()
    {
        ChangeTracker<int> tracker = new ChangeTracker<int>(42);

        tracker.Value = 100;

        Assert.IsTrue(tracker.IsChanged);
        Assert.AreEqual(100, tracker.Value);
        Assert.AreEqual(42, tracker.OriginalValue);
    }

    [TestMethod]
    public void SettingSameValue_DoesNotTriggerChange()
    {
        ChangeTracker<int> tracker = new ChangeTracker<int>(42);
        bool changed = false;
        tracker.PropertyChanged += (_, _) => changed = true;

        tracker.Value = 42;

        Assert.IsFalse(changed);
        Assert.IsFalse(tracker.IsChanged);
    }
    #endregion
}
