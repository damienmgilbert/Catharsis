using System.ComponentModel;

using Catharsis.ComponentModel.Binding;

namespace Catharsis.UnitTests.ComponentModel.Binding;

[TestClass]
public sealed class PropertyChangeBatcherTests
{
    private sealed class NotifySource : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public void RaisePropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    [TestMethod]
    public void Constructor_NullSource_ThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(
            () => new PropertyChangeBatcher(null!, _ => { }));
    }

    [TestMethod]
    public void Constructor_NullCallback_ThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(
            () => new PropertyChangeBatcher(new NotifySource(), null!));
    }

    [TestMethod]
    public void NoBatch_ForwardsImmediately()
    {
        var source = new NotifySource();
        var raised = new List<string>();
        using var batcher = new PropertyChangeBatcher(source, name => raised.Add(name));

        source.RaisePropertyChanged("Name");

        Assert.AreEqual(1, raised.Count);
        Assert.AreEqual("Name", raised[0]);
    }

    [TestMethod]
    public void BeginBatch_SuppressesForwarding()
    {
        var source = new NotifySource();
        var raised = new List<string>();
        using var batcher = new PropertyChangeBatcher(source, name => raised.Add(name));

        batcher.BeginBatch();
        source.RaisePropertyChanged("Name");

        Assert.AreEqual(0, raised.Count);
        Assert.IsTrue(batcher.IsBatching);
        Assert.AreEqual(1, batcher.PendingCount);

        batcher.EndBatch();
    }

    [TestMethod]
    public void EndBatch_FlushesAccumulatedChanges()
    {
        var source = new NotifySource();
        var raised = new List<string>();
        using var batcher = new PropertyChangeBatcher(source, name => raised.Add(name));

        batcher.BeginBatch();
        source.RaisePropertyChanged("Name");
        source.RaisePropertyChanged("Age");
        batcher.EndBatch();

        Assert.AreEqual(2, raised.Count);
        CollectionAssert.Contains(raised, "Name");
        CollectionAssert.Contains(raised, "Age");
    }

    [TestMethod]
    public void EndBatch_DuplicateProperties_FlushesOncePerProperty()
    {
        var source = new NotifySource();
        var raised = new List<string>();
        using var batcher = new PropertyChangeBatcher(source, name => raised.Add(name));

        batcher.BeginBatch();
        source.RaisePropertyChanged("Name");
        source.RaisePropertyChanged("Name");
        source.RaisePropertyChanged("Name");
        batcher.EndBatch();

        Assert.AreEqual(1, raised.Count);
    }

    [TestMethod]
    public void EndBatch_WithoutBeginBatch_ThrowsInvalidOperationException()
    {
        var source = new NotifySource();
        using var batcher = new PropertyChangeBatcher(source, _ => { });

        Assert.ThrowsExactly<InvalidOperationException>(() => batcher.EndBatch());
    }

    [TestMethod]
    public void NestedBatch_OnlyFlushesOnOutermostEnd()
    {
        var source = new NotifySource();
        var raised = new List<string>();
        using var batcher = new PropertyChangeBatcher(source, name => raised.Add(name));

        batcher.BeginBatch();
        batcher.BeginBatch();
        source.RaisePropertyChanged("Name");
        batcher.EndBatch(); // inner end
        Assert.AreEqual(0, raised.Count);

        batcher.EndBatch(); // outer end
        Assert.AreEqual(1, raised.Count);
    }

    [TestMethod]
    public void Flush_ManualFlush_DrainsPending()
    {
        var source = new NotifySource();
        var raised = new List<string>();
        using var batcher = new PropertyChangeBatcher(source, name => raised.Add(name));

        batcher.BeginBatch();
        source.RaisePropertyChanged("Name");
        batcher.Flush();

        Assert.AreEqual(1, raised.Count);
        Assert.AreEqual(0, batcher.PendingCount);

        batcher.EndBatch();
    }

    [TestMethod]
    public void Flush_NothingPending_DoesNothing()
    {
        var source = new NotifySource();
        var raised = new List<string>();
        using var batcher = new PropertyChangeBatcher(source, name => raised.Add(name));

        batcher.Flush();

        Assert.AreEqual(0, raised.Count);
    }

    [TestMethod]
    public void Dispose_FlushesAndDetaches()
    {
        var source = new NotifySource();
        var raised = new List<string>();
        var batcher = new PropertyChangeBatcher(source, name => raised.Add(name));

        batcher.BeginBatch();
        source.RaisePropertyChanged("Name");
        batcher.Dispose();

        Assert.AreEqual(1, raised.Count);

        // After dispose, source events are no longer forwarded
        raised.Clear();
        source.RaisePropertyChanged("Age");
        Assert.AreEqual(0, raised.Count);
    }

    [TestMethod]
    public void IsBatching_InitiallyFalse()
    {
        var source = new NotifySource();
        using var batcher = new PropertyChangeBatcher(source, _ => { });

        Assert.IsFalse(batcher.IsBatching);
    }

    [TestMethod]
    public void PendingCount_InitiallyZero()
    {
        var source = new NotifySource();
        using var batcher = new PropertyChangeBatcher(source, _ => { });

        Assert.AreEqual(0, batcher.PendingCount);
    }
}
