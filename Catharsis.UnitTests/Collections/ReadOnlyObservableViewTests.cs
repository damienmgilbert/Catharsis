using Catharsis.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Catharsis.UnitTests.Collections;

///<summary>
///Unit tests for the <see cref="ReadOnlyObservableView{T}"/> class.
///</summary>
[TestClass]
public class ReadOnlyObservableViewTests
{
    #region Constructor

    [TestMethod]
    public void Constructor_NullSource_Throws() { Assert.ThrowsExactly<ArgumentNullException>(static () => new ReadOnlyObservableView<int>(null!)); }

    #endregion

    #region Live view

    [TestMethod]
    public void Indexer_ReflectsSourceContents()
    {
        ObservableCollection<int> source = [1, 2, 3];
        ReadOnlyObservableView<int> view = new(source);

        Assert.AreEqual(2, view[1]);
    }

    [TestMethod]
    public void Count_ReflectsSourceContents() { Assert.AreEqual(3, new ReadOnlyObservableView<int>([1, 2, 3]).Count); }

    [TestMethod]
    public void Enumeration_ReflectsSourceContents()
    {
        ObservableCollection<int> source = [1, 2, 3];
        ReadOnlyObservableView<int> view = new(source);

        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, view.ToList());
    }

    [TestMethod]
    public void Count_AfterSourceMutation_ReflectsChange()
    {
        ObservableCollection<int> source = [1, 2];
        ReadOnlyObservableView<int> view = new(source);

        source.Add(3);

        Assert.AreEqual(3, view.Count);
    }

    [TestMethod]
    public void CollectionChanged_WhenSourceMutated_IsForwarded()
    {
        ObservableCollection<int> source = [];
        ReadOnlyObservableView<int> view = new(source);

        NotifyCollectionChangedEventArgs? received = null;
        view.CollectionChanged += (_, e) => received = e;

        source.Add(1);

        Assert.IsNotNull(received);
        Assert.AreEqual(NotifyCollectionChangedAction.Add, received!.Action);
    }

    #endregion

    #region Freeze

    [TestMethod]
    public void IsFrozen_BeforeFreeze_IsFalse() { Assert.IsFalse(new ReadOnlyObservableView<int>([1]).IsFrozen); }

    [TestMethod]
    public void Freeze_SetsIsFrozen()
    {
        ReadOnlyObservableView<int> view = new([1]);
        view.Freeze();

        Assert.IsTrue(view.IsFrozen);
    }

    [TestMethod]
    public void Freeze_CalledTwice_DoesNotThrow()
    {
        ReadOnlyObservableView<int> view = new([1]);
        view.Freeze();
        view.Freeze();
    }

    [TestMethod]
    public void Count_AfterFreeze_IgnoresSubsequentMutations()
    {
        ObservableCollection<int> source = [1, 2];
        ReadOnlyObservableView<int> view = new(source);
        view.Freeze();

        source.Add(3);

        Assert.AreEqual(2, view.Count);
    }

    [TestMethod]
    public void Enumeration_AfterFreeze_ReflectsSnapshotNotLiveSource()
    {
        ObservableCollection<int> source = [1, 2];
        ReadOnlyObservableView<int> view = new(source);
        view.Freeze();

        source.Add(3);

        CollectionAssert.AreEqual(new[] { 1, 2 }, view.ToList());
    }

    [TestMethod]
    public void CollectionChanged_AfterFreeze_IsNoLongerRaised()
    {
        ObservableCollection<int> source = [1];
        ReadOnlyObservableView<int> view = new(source);
        view.Freeze();

        bool raised = false;
        view.CollectionChanged += (_, _) => raised = true;

        source.Add(2);

        Assert.IsFalse(raised);
    }

    #endregion
}
