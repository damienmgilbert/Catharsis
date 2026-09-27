using Catharsis.Services;

namespace Catharsis.UnitTests.Services;

///<summary>
///Unit tests for the <see cref="ThrottledService{TResult}"/> class.
///</summary>
[TestClass]
public class ThrottledServiceTests
{
    #region Constructor

    [TestMethod]
    public void Constructor_NullAction_Throws() { Assert.ThrowsExactly<ArgumentNullException>(static () => new ThrottledService<int>(null!, 1, 1)); }

    [TestMethod]
    public void Constructor_NonPositiveCapacity_Throws() { Assert.ThrowsExactly<ArgumentOutOfRangeException>(static () => new ThrottledService<int>(static _ => Task.FromResult(1), 0, 1)); }

    [TestMethod]
    public void Constructor_NonPositiveCallsPerSecond_Throws() { Assert.ThrowsExactly<ArgumentOutOfRangeException>(static () => new ThrottledService<int>(static _ => Task.FromResult(1), 1, 0)); }

    #endregion

    #region TryInvoke

    [TestMethod]
    public void TryInvoke_TokenAvailable_InvokesActionAndReturnsTrue()
    {
        ThrottledService<int> service = new(static _ => Task.FromResult(42), capacity: 1, callsPerSecond: 1);

        bool acquired = service.TryInvoke(CancellationToken.None, out Task<int>? result);

        Assert.IsTrue(acquired);
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public void TryInvoke_NoTokenAvailable_ReturnsFalseWithoutInvoking()
    {
        int calls = 0;
        ThrottledService<int> service = new(_ => { calls++; return Task.FromResult(1); }, capacity: 1, callsPerSecond: 0.001);

        service.TryInvoke(CancellationToken.None, out _);
        bool secondAcquired = service.TryInvoke(CancellationToken.None, out Task<int>? secondResult);

        Assert.IsFalse(secondAcquired);
        Assert.IsNull(secondResult);
        Assert.AreEqual(1, calls);
    }

    #endregion

    #region InvokeAsync

    [TestMethod]
    public async Task InvokeAsync_TokenAvailable_ReturnsActionResult()
    {
        ThrottledService<int> service = new(static _ => Task.FromResult(99), capacity: 1, callsPerSecond: 1);
        int result = await service.InvokeAsync();
        Assert.AreEqual(99, result);
    }

    [TestMethod]
    public async Task InvokeAsync_NoTokenAvailable_WaitsForRefill()
    {
        ThrottledService<int> service = new(static _ => Task.FromResult(1), capacity: 1, callsPerSecond: 20);

        await service.InvokeAsync();
        int second = await service.InvokeAsync();

        Assert.AreEqual(1, second);
    }

    #endregion
}
