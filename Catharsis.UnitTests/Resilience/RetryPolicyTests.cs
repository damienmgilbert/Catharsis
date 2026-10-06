using Catharsis.Resilience;

namespace Catharsis.UnitTests.Resilience;

///<summary>
///Unit tests for the <see cref="RetryPolicy"/> class.
///</summary>
[TestClass]
public class RetryPolicyTests
{
    #region Defaults

    [TestMethod]
    public void Defaults_MaxAttempts_IsThree()
    {
        RetryPolicy policy = new();
        Assert.AreEqual(3, policy.ConfiguredMaxAttempts);
    }

    [TestMethod]
    public void Defaults_InitialDelay_Is200Ms()
    {
        RetryPolicy policy = new();
        Assert.AreEqual(TimeSpan.FromMilliseconds(200), policy.ConfiguredInitialDelay);
    }

    #endregion

    #region Fluent configuration

    [TestMethod]
    public void MaxAttempts_ValidValue_ConfiguresAndChains()
    {
        RetryPolicy policy = new RetryPolicy().MaxAttempts(5);
        Assert.AreEqual(5, policy.ConfiguredMaxAttempts);
    }

    [TestMethod]
    public void MaxAttempts_ZeroOrNegative_Throws()
    {
        RetryPolicy policy = new();
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => policy.MaxAttempts(0));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => policy.MaxAttempts(-1));
    }

    [TestMethod]
    public void InitialDelay_ValidValue_ConfiguresAndChains()
    {
        RetryPolicy policy = new RetryPolicy().InitialDelay(TimeSpan.FromSeconds(1));
        Assert.AreEqual(TimeSpan.FromSeconds(1), policy.ConfiguredInitialDelay);
    }

    [TestMethod]
    public void InitialDelay_Negative_Throws()
    {
        RetryPolicy policy = new();
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => policy.InitialDelay(TimeSpan.FromMilliseconds(-1)));
    }

    [TestMethod]
    public void ExponentialBackoff_BelowOne_Throws()
    {
        RetryPolicy policy = new();
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => policy.ExponentialBackoff(0.5));
    }

    [TestMethod]
    public void ExponentialBackoff_ValidValue_Chains()
    {
        RetryPolicy result = new RetryPolicy().ExponentialBackoff(3.0);
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public void MaxDelay_Negative_Throws()
    {
        RetryPolicy policy = new();
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => policy.MaxDelay(TimeSpan.FromMilliseconds(-1)));
    }

    [TestMethod]
    public void MaxDelay_ValidValue_Chains()
    {
        RetryPolicy result = new RetryPolicy().MaxDelay(TimeSpan.FromSeconds(10));
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public void RetryWhen_Null_Throws()
    {
        RetryPolicy policy = new();
        Assert.ThrowsExactly<ArgumentNullException>(() => policy.RetryWhen(null!));
    }

    [TestMethod]
    public void FluentChaining_AllMethods_ReturnsSameInstance()
    {
        RetryPolicy policy = new();
        RetryPolicy chained = policy
            .MaxAttempts(5)
            .InitialDelay(TimeSpan.FromMilliseconds(100))
            .ExponentialBackoff(2.0)
            .MaxDelay(TimeSpan.FromSeconds(5))
            .WithJitter()
            .RetryOn<InvalidOperationException>();

        Assert.AreSame(policy, chained);
    }

    #endregion

    #region Execute<TResult> (synchronous)

    [TestMethod]
    public void Execute_SuccessOnFirstAttempt_ReturnsResult()
    {
        RetryPolicy policy = new RetryPolicy().InitialDelay(TimeSpan.Zero);
        int result = policy.Execute(static () => 42);
        Assert.AreEqual(42, result);
    }

    [TestMethod]
    public void Execute_SucceedsAfterTransientFailures_ReturnsResult()
    {
        int callCount = 0;
        RetryPolicy policy = new RetryPolicy().MaxAttempts(3).InitialDelay(TimeSpan.Zero);

        int result = policy.Execute(() =>
        {
            callCount++;
            if (callCount < 3)
            {
                throw new InvalidOperationException("transient");
            }
            return 99;
        });

        Assert.AreEqual(99, result);
        Assert.AreEqual(3, callCount);
    }

    [TestMethod]
    public void Execute_AllAttemptsFail_ThrowsAggregateException()
    {
        RetryPolicy policy = new RetryPolicy().MaxAttempts(3).InitialDelay(TimeSpan.Zero);

        AggregateException ex = Assert.ThrowsExactly<AggregateException>(() =>
            policy.Execute<int>(static () => throw new InvalidOperationException("fail")));

        Assert.HasCount(3, ex.InnerExceptions);
    }

    [TestMethod]
    public void Execute_NullOperation_Throws()
    {
        RetryPolicy policy = new();
        Assert.ThrowsExactly<ArgumentNullException>(() => policy.Execute<int>(null!));
    }

    #endregion

    #region Execute (void, synchronous)

    [TestMethod]
    public void ExecuteVoid_SuccessOnFirstAttempt_Completes()
    {
        RetryPolicy policy = new RetryPolicy().InitialDelay(TimeSpan.Zero);
        bool executed = false;
        policy.Execute(() => executed = true);
        Assert.IsTrue(executed);
    }

    [TestMethod]
    public void ExecuteVoid_NullOperation_Throws()
    {
        RetryPolicy policy = new();
        Assert.ThrowsExactly<ArgumentNullException>(() => policy.Execute((Action)null!));
    }

    [TestMethod]
    public void ExecuteVoid_AllAttemptsFail_ThrowsAggregateException()
    {
        RetryPolicy policy = new RetryPolicy().MaxAttempts(2).InitialDelay(TimeSpan.Zero);

        AggregateException ex = Assert.ThrowsExactly<AggregateException>(() =>
            policy.Execute(static () => throw new InvalidOperationException("fail")));

        Assert.HasCount(2, ex.InnerExceptions);
    }

    #endregion

    #region ExecuteAsync<TResult> (Task)

    [TestMethod]
    public async Task ExecuteAsync_SuccessOnFirstAttempt_ReturnsResult()
    {
        RetryPolicy policy = new RetryPolicy().InitialDelay(TimeSpan.Zero);
        int result = await policy.ExecuteAsync(static ct => Task.FromResult(42));
        Assert.AreEqual(42, result);
    }

    [TestMethod]
    public async Task ExecuteAsync_SucceedsAfterTransientFailures_ReturnsResult()
    {
        int callCount = 0;
        RetryPolicy policy = new RetryPolicy().MaxAttempts(3).InitialDelay(TimeSpan.Zero);

        int result = await policy.ExecuteAsync(ct =>
        {
            callCount++;
            if (callCount < 3)
            {
                throw new InvalidOperationException("transient");
            }
            return Task.FromResult(99);
        });

        Assert.AreEqual(99, result);
        Assert.AreEqual(3, callCount);
    }

    [TestMethod]
    public async Task ExecuteAsync_AllAttemptsFail_ThrowsAggregateException()
    {
        RetryPolicy policy = new RetryPolicy().MaxAttempts(2).InitialDelay(TimeSpan.Zero);

        AggregateException ex = await Assert.ThrowsExactlyAsync<AggregateException>(async () =>
            await policy.ExecuteAsync<int>(static ct => throw new InvalidOperationException("fail")));

        Assert.HasCount(2, ex.InnerExceptions);
    }

    [TestMethod]
    public async Task ExecuteAsync_NullOperation_Throws()
    {
        RetryPolicy policy = new();
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(async () =>
            await policy.ExecuteAsync<int>(null!));
    }

    [TestMethod]
    public async Task ExecuteAsync_CancellationRequested_ThrowsOperationCanceledException()
    {
        using CancellationTokenSource cts = new();
        await cts.CancelAsync();

        RetryPolicy policy = new RetryPolicy().InitialDelay(TimeSpan.Zero);

        await Assert.ThrowsExactlyAsync<OperationCanceledException>(async () =>
            await policy.ExecuteAsync(static ct => Task.FromResult(42), cts.Token));
    }

    #endregion

    #region ExecuteAsync (void, Task)

    [TestMethod]
    public async Task ExecuteAsyncVoid_SuccessOnFirstAttempt_Completes()
    {
        RetryPolicy policy = new RetryPolicy().InitialDelay(TimeSpan.Zero);
        bool executed = false;
        await policy.ExecuteAsync(ct =>
        {
            executed = true;
            return Task.CompletedTask;
        });
        Assert.IsTrue(executed);
    }

    [TestMethod]
    public async Task ExecuteAsyncVoid_NullOperation_Throws()
    {
        RetryPolicy policy = new();
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(async () =>
            await policy.ExecuteAsync((Func<CancellationToken, Task>)null!));
    }

    #endregion

    #region ExecuteValueAsync<TResult> (ValueTask)

    [TestMethod]
    public async Task ExecuteValueAsync_SuccessOnFirstAttempt_ReturnsResult()
    {
        RetryPolicy policy = new RetryPolicy().InitialDelay(TimeSpan.Zero);
        int result = await policy.ExecuteValueAsync(static ct => ValueTask.FromResult(42));
        Assert.AreEqual(42, result);
    }

    [TestMethod]
    public async Task ExecuteValueAsync_SucceedsAfterTransientFailures_ReturnsResult()
    {
        int callCount = 0;
        RetryPolicy policy = new RetryPolicy().MaxAttempts(3).InitialDelay(TimeSpan.Zero);

        int result = await policy.ExecuteValueAsync(ct =>
        {
            callCount++;
            if (callCount < 3)
            {
                throw new InvalidOperationException("transient");
            }
            return ValueTask.FromResult(99);
        });

        Assert.AreEqual(99, result);
        Assert.AreEqual(3, callCount);
    }

    [TestMethod]
    public async Task ExecuteValueAsync_AllAttemptsFail_ThrowsAggregateException()
    {
        RetryPolicy policy = new RetryPolicy().MaxAttempts(2).InitialDelay(TimeSpan.Zero);

        AggregateException ex = await Assert.ThrowsExactlyAsync<AggregateException>(async () =>
            await policy.ExecuteValueAsync<int>(static ct => throw new InvalidOperationException("fail")));

        Assert.HasCount(2, ex.InnerExceptions);
    }

    [TestMethod]
    public async Task ExecuteValueAsync_NullOperation_Throws()
    {
        RetryPolicy policy = new();
        Assert.ThrowsExactly<ArgumentNullException>(() =>
            policy.ExecuteValueAsync<int>(null!));
    }

    [TestMethod]
    public async Task ExecuteValueAsync_CancellationRequested_ThrowsOperationCanceledException()
    {
        using CancellationTokenSource cts = new();
        await cts.CancelAsync();

        RetryPolicy policy = new RetryPolicy().InitialDelay(TimeSpan.Zero);

        await Assert.ThrowsExactlyAsync<OperationCanceledException>(async () =>
            await policy.ExecuteValueAsync(static ct => ValueTask.FromResult(42), cts.Token));
    }

    #endregion

    #region ExecuteValueAsync (void, ValueTask)

    [TestMethod]
    public async Task ExecuteValueAsyncVoid_SuccessOnFirstAttempt_Completes()
    {
        RetryPolicy policy = new RetryPolicy().InitialDelay(TimeSpan.Zero);
        bool executed = false;
        await policy.ExecuteValueAsync(ct =>
        {
            executed = true;
            return ValueTask.CompletedTask;
        });
        Assert.IsTrue(executed);
    }

    [TestMethod]
    public async Task ExecuteValueAsyncVoid_NullOperation_Throws()
    {
        RetryPolicy policy = new();
        Assert.ThrowsExactly<ArgumentNullException>(() =>
            policy.ExecuteValueAsync((Func<CancellationToken, ValueTask>)null!));
    }

    #endregion

    #region RetryOn<TException> filtering

    [TestMethod]
    public void RetryOn_MatchingException_Retries()
    {
        int callCount = 0;
        RetryPolicy policy = new RetryPolicy()
            .MaxAttempts(3)
            .InitialDelay(TimeSpan.Zero)
            .RetryOn<InvalidOperationException>();

        int result = policy.Execute(() =>
        {
            callCount++;
            if (callCount < 2)
            {
                throw new InvalidOperationException("transient");
            }
            return 1;
        });

        Assert.AreEqual(1, result);
        Assert.AreEqual(2, callCount);
    }

    [TestMethod]
    public void RetryOn_NonMatchingException_DoesNotRetry()
    {
        int callCount = 0;
        RetryPolicy policy = new RetryPolicy()
            .MaxAttempts(3)
            .InitialDelay(TimeSpan.Zero)
            .RetryOn<InvalidOperationException>();

        Assert.ThrowsExactly<ArgumentException>(() =>
            policy.Execute<int>(() =>
            {
                callCount++;
                throw new ArgumentException("non-retriable");
            }));

        Assert.AreEqual(1, callCount);
    }

    #endregion

    #region RetryWhen predicate

    [TestMethod]
    public void RetryWhen_PredicateTrue_Retries()
    {
        int callCount = 0;
        RetryPolicy policy = new RetryPolicy()
            .MaxAttempts(3)
            .InitialDelay(TimeSpan.Zero)
            .RetryWhen(static ex => ex.Message.Contains("transient"));

        int result = policy.Execute(() =>
        {
            callCount++;
            if (callCount < 2)
            {
                throw new Exception("transient error");
            }
            return 1;
        });

        Assert.AreEqual(1, result);
        Assert.AreEqual(2, callCount);
    }

    [TestMethod]
    public void RetryWhen_PredicateFalse_DoesNotRetry()
    {
        int callCount = 0;
        RetryPolicy policy = new RetryPolicy()
            .MaxAttempts(3)
            .InitialDelay(TimeSpan.Zero)
            .RetryWhen(static ex => ex.Message.Contains("transient"));

        Assert.ThrowsExactly<Exception>(() =>
            policy.Execute<int>(() =>
            {
                callCount++;
                throw new Exception("permanent error");
            }));

        Assert.AreEqual(1, callCount);
    }

    #endregion

    #region Single attempt (MaxAttempts=1)

    [TestMethod]
    public void Execute_SingleAttempt_Success_ReturnsResult()
    {
        RetryPolicy policy = new RetryPolicy().MaxAttempts(1);
        int result = policy.Execute(static () => 42);
        Assert.AreEqual(42, result);
    }

    [TestMethod]
    public void Execute_SingleAttempt_Failure_ThrowsDirectly()
    {
        RetryPolicy policy = new RetryPolicy().MaxAttempts(1);

        Assert.ThrowsExactly<InvalidOperationException>(() =>
            policy.Execute<int>(static () => throw new InvalidOperationException("no retry")));
    }

    #endregion

    #region AggregateException contents

    [TestMethod]
    public void Execute_AggregateException_ContainsAllInnerExceptions()
    {
        int callCount = 0;
        RetryPolicy policy = new RetryPolicy().MaxAttempts(3).InitialDelay(TimeSpan.Zero);

        AggregateException ex = Assert.ThrowsExactly<AggregateException>(() =>
            policy.Execute<int>(() =>
            {
                callCount++;
                throw new InvalidOperationException($"failure {callCount}");
            }));

        Assert.HasCount(3, ex.InnerExceptions);
        Assert.AreEqual("failure 1", ex.InnerExceptions[0].Message);
        Assert.AreEqual("failure 2", ex.InnerExceptions[1].Message);
        Assert.AreEqual("failure 3", ex.InnerExceptions[2].Message);
    }

    #endregion

    #region Async cancellation does not retry

    [TestMethod]
    public async Task ExecuteAsync_OperationCanceledException_NotRetried()
    {
        int callCount = 0;
        RetryPolicy policy = new RetryPolicy().MaxAttempts(3).InitialDelay(TimeSpan.Zero);

        await Assert.ThrowsExactlyAsync<OperationCanceledException>(async () =>
            await policy.ExecuteAsync<int>(ct =>
            {
                callCount++;
                throw new OperationCanceledException("cancelled");
            }));

        Assert.AreEqual(1, callCount);
    }

    #endregion

    #region WithJitter

    [TestMethod]
    public void WithJitter_Chains()
    {
        RetryPolicy policy = new();
        RetryPolicy result = policy.WithJitter();
        Assert.AreSame(policy, result);
    }

    #endregion
}
