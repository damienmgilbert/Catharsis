using Catharsis.Resilience;

namespace Catharsis.UnitTests.Resilience;

///<summary>
///Unit tests for the <see cref="FallbackPolicy{TResult}"/> class.
///</summary>
[TestClass]
public class FallbackPolicyTests
{
    #region Execute

    [TestMethod]
    public void Execute_Success_ReturnsResult()
    {
        FallbackPolicy<int> policy = new();
        int result = policy.Execute(static () => 42, static ex => -1);
        Assert.AreEqual(42, result);
    }

    [TestMethod]
    public void Execute_Failure_ReturnsFallbackValue()
    {
        FallbackPolicy<int> policy = new();
        int result = policy.Execute(static () => throw new InvalidOperationException(), -1);
        Assert.AreEqual(-1, result);
    }

    [TestMethod]
    public void Execute_Failure_InvokesFallbackDelegateWithException()
    {
        FallbackPolicy<string> policy = new();
        InvalidOperationException thrown = new("boom");

        string result = policy.Execute(() => throw thrown, ex => ex.Message);

        Assert.AreEqual("boom", result);
    }

    [TestMethod]
    public void Execute_PredicateDoesNotMatch_PropagatesException()
    {
        FallbackPolicy<int> policy = new(static ex => ex is TimeoutException);

        Assert.ThrowsExactly<InvalidOperationException>(() =>
            policy.Execute(static () => throw new InvalidOperationException(), -1));
    }

    [TestMethod]
    public void Execute_PredicateMatches_ReturnsFallback()
    {
        FallbackPolicy<int> policy = new(static ex => ex is InvalidOperationException);
        int result = policy.Execute(static () => throw new InvalidOperationException(), -1);
        Assert.AreEqual(-1, result);
    }

    [TestMethod]
    public void Execute_NullOperation_Throws()
    {
        FallbackPolicy<int> policy = new();
        Assert.ThrowsExactly<ArgumentNullException>(() => policy.Execute(null!, -1));
    }

    [TestMethod]
    public void Execute_NullFallback_Throws()
    {
        FallbackPolicy<int> policy = new();
        Assert.ThrowsExactly<ArgumentNullException>(() => policy.Execute(static () => 1, (Func<Exception, int>)null!));
    }

    #endregion

    #region ExecuteAsync

    [TestMethod]
    public async Task ExecuteAsync_Success_ReturnsResult()
    {
        FallbackPolicy<int> policy = new();
        int result = await policy.ExecuteAsync(static ct => Task.FromResult(42), -1);
        Assert.AreEqual(42, result);
    }

    [TestMethod]
    public async Task ExecuteAsync_Failure_ReturnsFallbackValue()
    {
        FallbackPolicy<int> policy = new();
        int result = await policy.ExecuteAsync(static ct => throw new InvalidOperationException(), -1);
        Assert.AreEqual(-1, result);
    }

    [TestMethod]
    public async Task ExecuteAsync_Failure_InvokesFallbackDelegateWithException()
    {
        FallbackPolicy<string> policy = new();
        InvalidOperationException thrown = new("boom");

        string result = await policy.ExecuteAsync(ct => throw thrown, ex => Task.FromResult(ex.Message));

        Assert.AreEqual("boom", result);
    }

    [TestMethod]
    public async Task ExecuteAsync_OperationCanceled_DoesNotFallBack()
    {
        FallbackPolicy<int> policy = new();

        await Assert.ThrowsExactlyAsync<OperationCanceledException>(async () =>
            await policy.ExecuteAsync(static ct => throw new OperationCanceledException(), -1));
    }

    #endregion
}
