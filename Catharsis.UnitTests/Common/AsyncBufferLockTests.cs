using Catharsis.Common;

namespace Catharsis.UnitTests.Common;

///<summary>
///Unit tests for the <see cref="AsyncBufferLock"/> class.
///</summary>
[TestClass]
public class AsyncBufferLockTests
{
    #region Public methods
    [TestMethod]
    public void Dispose_IsIdempotent()
    {
        AsyncBufferLock abl = new();
        abl.Dispose();
        abl.Dispose();
    }

    [TestMethod]
    public void Lock_Synchronous_AcquiresAndReleases()
    {
        using AsyncBufferLock abl = new();

        using (abl.Lock())
        {
            Assert.IsTrue(abl.IsLocked);
        }

        Assert.IsFalse(abl.IsLocked);
    }

    [TestMethod]
    public async Task LockAsync_AcquiresAndReleases()
    {
        using AsyncBufferLock abl = new();
        Assert.IsFalse(abl.IsLocked);

        using (await abl.LockAsync())
        {
            Assert.IsTrue(abl.IsLocked);
        }

        Assert.IsFalse(abl.IsLocked);
    }

    [TestMethod]
    public void LockAsync_AfterDispose_Throws()
    {
        AsyncBufferLock abl = new();
        abl.Dispose();
        Assert.ThrowsExactlyAsync<ObjectDisposedException>(() => abl.LockAsync());
    }

    [TestMethod]
    public async Task LockValueAsync_AcquiresAndReleases()
    {
        using AsyncBufferLock abl = new();

        using (await abl.LockValueAsync())
        {
            Assert.IsTrue(abl.IsLocked);
        }

        Assert.IsFalse(abl.IsLocked);
    }

    [TestMethod]
    public void TryLock_AlreadyLocked_ReturnsFalse()
    {
        using AsyncBufferLock abl = new();
        using IDisposable handle = abl.Lock();
        Assert.IsFalse(abl.TryLock(out IDisposable? h2));
        Assert.IsNull(h2);
    }

    [TestMethod]
    public void TryLock_NotLocked_ReturnsTrue()
    {
        using AsyncBufferLock abl = new();
        Assert.IsTrue(abl.TryLock(out IDisposable? handle));
        Assert.IsNotNull(handle);
        Assert.IsTrue(abl.IsLocked);
        handle.Dispose();
        Assert.IsFalse(abl.IsLocked);
    }
    #endregion
}
