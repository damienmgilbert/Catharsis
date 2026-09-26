using Catharsis.Resilience;

namespace Catharsis.UnitTests.Resilience;

///<summary>
///Unit tests for the <see cref="RateLimiter"/> class.
///</summary>
[TestClass]
public class RateLimiterTests
{
    #region Construction

    [TestMethod]
    public void Constructor_ZeroOrNegativeCapacity_Throws()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new RateLimiter(0, 1));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new RateLimiter(-1, 1));
    }

    [TestMethod]
    public void Constructor_ZeroOrNegativeTokensPerSecond_Throws()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new RateLimiter(1, 0));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new RateLimiter(1, -1));
    }

    [TestMethod]
    public void Constructor_StartsFull()
    {
        RateLimiter limiter = new(5, 1);
        Assert.AreEqual(5, limiter.Capacity);
        Assert.AreEqual(5, limiter.AvailableTokens);
    }

    #endregion

    #region TryAcquire

    [TestMethod]
    public void TryAcquire_TokensAvailable_ReturnsTrueAndConsumes()
    {
        RateLimiter limiter = new(5, 1);
        Assert.IsTrue(limiter.TryAcquire());
        Assert.IsTrue(limiter.AvailableTokens is >= 3.9 and <= 4.1);
    }

    [TestMethod]
    public void TryAcquire_MoreThanAvailable_ReturnsFalse()
    {
        RateLimiter limiter = new(1, 1);
        Assert.IsTrue(limiter.TryAcquire());
        Assert.IsFalse(limiter.TryAcquire());
    }

    [TestMethod]
    public void TryAcquire_ZeroOrNegativeTokens_Throws()
    {
        RateLimiter limiter = new(1, 1);
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => limiter.TryAcquire(0));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => limiter.TryAcquire(-1));
    }

    [TestMethod]
    public async Task TryAcquire_AfterRefillWindow_TokensReplenish()
    {
        RateLimiter limiter = new(1, tokensPerSecond: 50);
        Assert.IsTrue(limiter.TryAcquire());
        Assert.IsFalse(limiter.TryAcquire());

        await Task.Delay(40);

        Assert.IsTrue(limiter.TryAcquire());
    }

    [TestMethod]
    public void TryAcquire_RefillNeverExceedsCapacity()
    {
        RateLimiter limiter = new(3, tokensPerSecond: 1000);
        Assert.AreEqual(3, limiter.AvailableTokens);
    }

    #endregion

    #region AcquireAsync

    [TestMethod]
    public async Task AcquireAsync_TokensAvailable_CompletesImmediately()
    {
        RateLimiter limiter = new(5, 1);
        await limiter.AcquireAsync();
        Assert.IsTrue(limiter.AvailableTokens is >= 3.9 and <= 4.1);
    }

    [TestMethod]
    public async Task AcquireAsync_NoTokensAvailable_WaitsForRefill()
    {
        RateLimiter limiter = new(1, tokensPerSecond: 50);
        await limiter.AcquireAsync();

        DateTime start = DateTime.UtcNow;
        await limiter.AcquireAsync();
        TimeSpan elapsed = DateTime.UtcNow - start;

        Assert.IsTrue(elapsed >= TimeSpan.FromMilliseconds(5));
    }

    [TestMethod]
    public async Task AcquireAsync_TokensExceedCapacity_Throws()
    {
        RateLimiter limiter = new(1, 1);
        await Assert.ThrowsExactlyAsync<ArgumentOutOfRangeException>(async () => await limiter.AcquireAsync(2));
    }

    [TestMethod]
    public async Task AcquireAsync_ZeroOrNegativeTokens_Throws()
    {
        RateLimiter limiter = new(1, 1);
        await Assert.ThrowsExactlyAsync<ArgumentOutOfRangeException>(async () => await limiter.AcquireAsync(0));
    }

    #endregion
}
