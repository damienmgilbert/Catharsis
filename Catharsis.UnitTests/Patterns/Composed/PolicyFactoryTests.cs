using Catharsis.Patterns.Composed;
using Catharsis.Resilience;

namespace Catharsis.UnitTests.Patterns.Composed;

///<summary>
///Unit tests for <see cref="PolicyFactory"/>.
///</summary>
[TestClass]
public class PolicyFactoryTests
{
    [TestMethod]
    public void Create_NullOptions_Throws() { Assert.ThrowsExactly<ArgumentNullException>(static () => PolicyFactory.Create(null!)); }

    [TestMethod]
    public void Create_ZeroAttempts_Throws() { Assert.ThrowsExactly<ArgumentOutOfRangeException>(static () => PolicyFactory.Create(new PolicyOptions { MaxAttempts = 0 })); }

    [TestMethod]
    public async Task Create_DefaultOptions_PassesOperationThrough()
    {
        IAsyncPolicy policy = PolicyFactory.Create(new PolicyOptions());

        Assert.AreEqual(7, await policy.ExecuteAsync(static _ => Task.FromResult(7)));
        Assert.IsNotInstanceOfType<PolicyWrap>(policy);
    }

    [TestMethod]
    public async Task Create_PassThrough_PropagatesExceptionsUnchanged()
    {
        IAsyncPolicy policy = PolicyFactory.Create(new PolicyOptions());

        await Assert.ThrowsExactlyAsync<InvalidOperationException>(async () => await policy.ExecuteAsync<int>(static _ => throw new InvalidOperationException()));
    }

    [TestMethod]
    public async Task Create_WithRetry_RetriesUntilSuccess()
    {
        IAsyncPolicy policy = PolicyFactory.Create(new PolicyOptions { MaxAttempts = 3, InitialRetryDelay = TimeSpan.FromMilliseconds(1) });
        int calls = 0;

        int result = await policy.ExecuteAsync(_ => ++calls < 3 ? throw new InvalidOperationException() : Task.FromResult(calls));

        Assert.AreEqual(3, result);
    }

    [TestMethod]
    public async Task Create_WithRetry_GivesUpAfterMaxAttempts()
    {
        IAsyncPolicy policy = PolicyFactory.Create(new PolicyOptions { MaxAttempts = 2, InitialRetryDelay = TimeSpan.FromMilliseconds(1), UseJitter = true });
        int calls = 0;

        await Assert.ThrowsExactlyAsync<AggregateException>(async () => await policy.ExecuteAsync<int>(_ => { calls++; throw new InvalidOperationException(); }));

        Assert.AreEqual(2, calls);
    }

    [TestMethod]
    public async Task Create_WithTimeoutOnly_AbortsSlowOperation()
    {
        IAsyncPolicy policy = PolicyFactory.Create(new PolicyOptions { Timeout = TimeSpan.FromMilliseconds(30) });

        await Assert.ThrowsExactlyAsync<TimeoutException>(async () => await policy.ExecuteAsync(static async ct => { await Task.Delay(TimeSpan.FromSeconds(10), ct); return 1; }));
    }

    [TestMethod]
    public void Create_RetryAndTimeout_ProducesWrappedPipeline() { Assert.AreEqual(2, ((PolicyWrap)PolicyFactory.Create(new PolicyOptions { MaxAttempts = 2, Timeout = TimeSpan.FromSeconds(1) })).PolicyCount); }

    [TestMethod]
    public async Task Create_TimeoutAppliesPerAttempt_SoRetryRecovers()
    {
        IAsyncPolicy policy = PolicyFactory.Create(new PolicyOptions { MaxAttempts = 2, InitialRetryDelay = TimeSpan.FromMilliseconds(1), Timeout = TimeSpan.FromMilliseconds(50) });
        int calls = 0;

        int result = await policy.ExecuteAsync(async ct =>
        {
            if(++calls == 1)
            {
                await Task.Delay(TimeSpan.FromSeconds(10), ct);
            }

            return calls;
        });

        Assert.AreEqual(2, result);
    }
}
