using Catharsis.Concurrency;

namespace Catharsis.UnitTests.Concurrency;

///<summary>
///Unit tests for the <see cref="KeyedAsyncLock{TKey}"/> class.
///</summary>
[TestClass]
public class KeyedAsyncLockTests
{
    #region Lock / LockAsync

    [TestMethod]
    public void Lock_AcquiresAndReleases()
    {
        KeyedAsyncLock<string> locks = new();

        using (locks.Lock("a"))
        {
            Assert.AreEqual(1, locks.ActiveKeyCount);
        }

        Assert.AreEqual(0, locks.ActiveKeyCount);
    }

    [TestMethod]
    public async Task LockAsync_AcquiresAndReleases()
    {
        KeyedAsyncLock<string> locks = new();

        using (await locks.LockAsync("a"))
        {
            Assert.AreEqual(1, locks.ActiveKeyCount);
        }

        Assert.AreEqual(0, locks.ActiveKeyCount);
    }

    [TestMethod]
    public void LockAsync_NullKey_Throws()
    {
        KeyedAsyncLock<string> locks = new();
        Assert.ThrowsExactly<ArgumentNullException>(() => locks.Lock(null!));
    }

    [TestMethod]
    public async Task LockAsync_DifferentKeys_DoNotBlockEachOther()
    {
        KeyedAsyncLock<string> locks = new();

        using IDisposable handleA = await locks.LockAsync("a");
        using IDisposable handleB = await locks.LockAsync("b");

        Assert.AreEqual(2, locks.ActiveKeyCount);
    }

    [TestMethod]
    public async Task LockAsync_SameKey_SerializesAccess()
    {
        KeyedAsyncLock<string> locks = new();
        int concurrent = 0;
        int maxObserved = 0;
        object gate = new();

        async Task RunAsync()
        {
            using (await locks.LockAsync("shared"))
            {
                lock (gate)
                {
                    concurrent++;
                    maxObserved = Math.Max(maxObserved, concurrent);
                }

                await Task.Delay(20);

                lock (gate)
                {
                    concurrent--;
                }
            }
        }

        await Task.WhenAll(RunAsync(), RunAsync(), RunAsync());

        Assert.AreEqual(1, maxObserved);
    }

    [TestMethod]
    public async Task LockAsync_SecondWaiterProceedsAfterFirstReleases()
    {
        KeyedAsyncLock<int> locks = new();

        IDisposable first = await locks.LockAsync(1);
        Task<IDisposable> secondTask = locks.LockAsync(1);

        await Task.Delay(20);
        Assert.IsFalse(secondTask.IsCompleted);

        first.Dispose();

        IDisposable second = await secondTask;
        Assert.AreEqual(1, locks.ActiveKeyCount);
        second.Dispose();
        Assert.AreEqual(0, locks.ActiveKeyCount);
    }

    [TestMethod]
    public void ReleaseHandle_DisposedTwice_DoesNotThrow()
    {
        KeyedAsyncLock<string> locks = new();
        IDisposable handle = locks.Lock("a");
        handle.Dispose();
        handle.Dispose();
        Assert.AreEqual(0, locks.ActiveKeyCount);
    }

    #endregion
}
