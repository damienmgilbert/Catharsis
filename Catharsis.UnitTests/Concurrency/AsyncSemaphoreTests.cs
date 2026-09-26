using Catharsis.Concurrency;

namespace Catharsis.UnitTests.Concurrency;

///<summary>
///Unit tests for the <see cref="AsyncSemaphore"/> class.
///</summary>
[TestClass]
public class AsyncSemaphoreTests
{
    #region Construction

    [TestMethod]
    public void Constructor_InitialCount_SetsCurrentCount()
    {
        using AsyncSemaphore semaphore = new(2);
        Assert.AreEqual(2, semaphore.CurrentCount);
    }

    [TestMethod]
    public void Constructor_NegativeInitialCount_Throws()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new AsyncSemaphore(-1));
    }

    #endregion

    #region Wait / release

    [TestMethod]
    public void Wait_AcquiresAndReleases()
    {
        using AsyncSemaphore semaphore = new(1);

        using(semaphore.Wait())
        {
            Assert.AreEqual(0, semaphore.CurrentCount);
        }

        Assert.AreEqual(1, semaphore.CurrentCount);
    }

    [TestMethod]
    public async Task WaitAsync_AcquiresAndReleases()
    {
        using AsyncSemaphore semaphore = new(1);

        using(await semaphore.WaitAsync())
        {
            Assert.AreEqual(0, semaphore.CurrentCount);
        }

        Assert.AreEqual(1, semaphore.CurrentCount);
    }

    [TestMethod]
    public async Task WaitValueAsync_AcquiresAndReleases()
    {
        using AsyncSemaphore semaphore = new(1);

        using(await semaphore.WaitValueAsync())
        {
            Assert.AreEqual(0, semaphore.CurrentCount);
        }

        Assert.AreEqual(1, semaphore.CurrentCount);
    }

    [TestMethod]
    public void TryWait_SlotAvailable_ReturnsTrue()
    {
        using AsyncSemaphore semaphore = new(1);
        Assert.IsTrue(semaphore.TryWait(out IDisposable? handle));
        Assert.IsNotNull(handle);
        handle.Dispose();
        Assert.AreEqual(1, semaphore.CurrentCount);
    }

    [TestMethod]
    public void TryWait_NoSlotsAvailable_ReturnsFalse()
    {
        using AsyncSemaphore semaphore = new(1);
        using IDisposable held = semaphore.Wait();
        Assert.IsFalse(semaphore.TryWait(out IDisposable? handle));
        Assert.IsNull(handle);
    }

    [TestMethod]
    public async Task WaitAsync_LimitsConcurrency()
    {
        using AsyncSemaphore semaphore = new(2);
        int concurrent = 0;
        int maxObserved = 0;
        object gate = new();

        async Task RunAsync()
        {
            using(await semaphore.WaitAsync())
            {
                lock(gate)
                {
                    concurrent++;
                    maxObserved = Math.Max(maxObserved, concurrent);
                }

                await Task.Delay(20);

                lock(gate)
                {
                    concurrent--;
                }
            }
        }

        await Task.WhenAll(RunAsync(), RunAsync(), RunAsync(), RunAsync());

        Assert.AreEqual(2, maxObserved);
    }

    [TestMethod]
    public void ReleaseHandle_DisposedTwice_ReleasesOnce()
    {
        using AsyncSemaphore semaphore = new(1, 1);
        IDisposable handle = semaphore.Wait();
        handle.Dispose();
        handle.Dispose();
        Assert.AreEqual(1, semaphore.CurrentCount);
    }

    #endregion

    #region Disposal

    [TestMethod]
    public void Dispose_IsIdempotent()
    {
        AsyncSemaphore semaphore = new(1);
        semaphore.Dispose();
        semaphore.Dispose();
    }

    [TestMethod]
    public void Wait_AfterDispose_Throws()
    {
        AsyncSemaphore semaphore = new(1);
        semaphore.Dispose();
        Assert.ThrowsExactly<ObjectDisposedException>(() => semaphore.Wait());
    }

    #endregion
}
