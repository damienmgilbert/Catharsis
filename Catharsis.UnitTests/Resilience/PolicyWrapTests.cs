using Catharsis.Resilience;

namespace Catharsis.UnitTests.Resilience;

///<summary>
///Unit tests for the <see cref="PolicyWrap"/> class.
///</summary>
[TestClass]
public class PolicyWrapTests
{
    #region Construction

    [TestMethod]
    public void Constructor_NullPolicies_Throws()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => new PolicyWrap(null!));
    }

    [TestMethod]
    public void Constructor_EmptyPolicies_Throws()
    {
        Assert.ThrowsExactly<ArgumentException>(() => new PolicyWrap());
    }

    [TestMethod]
    public void Constructor_NullPolicyElement_Throws()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => new PolicyWrap(new RecordingPolicy("a", []), null!));
    }

    [TestMethod]
    public void Constructor_SetsPolicyCount()
    {
        PolicyWrap wrap = new(new RecordingPolicy("a", []), new RecordingPolicy("b", []));
        Assert.AreEqual(2, wrap.PolicyCount);
    }

    #endregion

    #region ExecuteAsync

    [TestMethod]
    public async Task ExecuteAsync_Success_ReturnsResult()
    {
        PolicyWrap wrap = new(new RecordingPolicy("a", []));
        int result = await wrap.ExecuteAsync(static ct => Task.FromResult(42));
        Assert.AreEqual(42, result);
    }

    [TestMethod]
    public async Task ExecuteAsync_AppliesPoliciesOutermostFirst()
    {
        List<string> log = [];
        PolicyWrap wrap = new(new RecordingPolicy("outer", log), new RecordingPolicy("inner", log));

        await wrap.ExecuteAsync(ct =>
        {
            log.Add("operation");
            return Task.FromResult(1);
        });

        CollectionAssert.AreEqual(new[] { "outer", "inner", "operation" }, log);
    }

    [TestMethod]
    public async Task ExecuteAsync_ComposesWithRetryPolicy()
    {
        int attempts = 0;
        RetryPolicy retry = new RetryPolicy().MaxAttempts(3).InitialDelay(TimeSpan.Zero);
        PolicyWrap wrap = new(retry);

        int result = await wrap.ExecuteAsync(ct =>
        {
            attempts++;
            if (attempts < 2)
            {
                throw new InvalidOperationException("transient");
            }

            return Task.FromResult(99);
        });

        Assert.AreEqual(99, result);
        Assert.AreEqual(2, attempts);
    }

    [TestMethod]
    public async Task ExecuteAsyncVoid_Success_Completes()
    {
        PolicyWrap wrap = new(new RecordingPolicy("a", []));
        bool executed = false;

        await wrap.ExecuteAsync(ct =>
        {
            executed = true;
            return Task.CompletedTask;
        });

        Assert.IsTrue(executed);
    }

    [TestMethod]
    public async Task ExecuteAsync_NullOperation_Throws()
    {
        PolicyWrap wrap = new(new RecordingPolicy("a", []));
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(async () => await wrap.ExecuteAsync<int>(null!));
    }

    #endregion

    sealed class RecordingPolicy(string name, List<string> log) : IAsyncPolicy
    {
        public async Task<TResult> ExecuteAsync<TResult>(Func<CancellationToken, Task<TResult>> operation, CancellationToken cancellationToken = default)
        {
            log.Add(name);
            return await operation(cancellationToken).ConfigureAwait(false);
        }
    }
}
