using Catharsis.Immutable;
using Microsoft.Extensions.Primitives;

namespace Catharsis.UnitTests.Immutable;

[TestClass]
public class ChangeTokenBufferWatcherTests
{
    #region Public methods
    [TestMethod]
    public void Change_IncrementsChangeCount()
    {
        TestNotifier notifier = new TestNotifier();
        int callbackCount = 0;
        using ChangeTokenBufferWatcher watcher = new ChangeTokenBufferWatcher(notifier, () => callbackCount++);
        watcher.Start();

        notifier.TriggerChange();

        // Give the change token a moment to propagate
        Thread.Sleep(50);
        Assert.IsTrue(watcher.ChangeCount >= 1);
        Assert.IsTrue(callbackCount >= 1);
    }

    [TestMethod]
    public void Dispose_IsIdempotent()
    {
        TestNotifier notifier = new TestNotifier();
        ChangeTokenBufferWatcher watcher = new ChangeTokenBufferWatcher(
                                           notifier,
                                           () =>
        {
        });
        watcher.Dispose();
        watcher.Dispose();
    }

    [TestMethod]
    public void Start_SetsIsWatchingTrue()
    {
        TestNotifier notifier = new TestNotifier();
        using ChangeTokenBufferWatcher watcher = new ChangeTokenBufferWatcher(
                                                 notifier,
                                                 () =>
        {
        });
        watcher.Start();
        Assert.IsTrue(watcher.IsWatching);
    }

    [TestMethod]
    public void Stop_SetsIsWatchingFalse()
    {
        TestNotifier notifier = new TestNotifier();
        using ChangeTokenBufferWatcher watcher = new ChangeTokenBufferWatcher(
                                                 notifier,
                                                 () =>
        {
        });
        watcher.Start();
        watcher.Stop();
        Assert.IsFalse(watcher.IsWatching);
    }
    #endregion

    sealed class TestNotifier : IBufferChangeNotifier
    {
        #region Fields
        CancellationTokenSource _cts = new();
        #endregion

        #region Public methods
        public IChangeToken GetChangeToken() { return new CancellationChangeToken(_cts.Token); }

        public void TriggerChange()
        {
            CancellationTokenSource old = _cts;
            _cts = new CancellationTokenSource();
            old.Cancel();
        }
        #endregion

        #region Public properties
        public bool HasChanged => _cts.IsCancellationRequested;
        #endregion
    }
}
