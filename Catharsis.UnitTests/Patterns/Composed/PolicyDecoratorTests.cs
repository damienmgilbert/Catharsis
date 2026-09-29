using Catharsis.Patterns.Composed;
using Catharsis.Resilience;
using Microsoft.Extensions.Logging;

namespace Catharsis.UnitTests.Patterns.Composed;

///<summary>
///Unit tests for <see cref="LoggingPolicyDecorator"/> and <see cref="MetricsPolicyDecorator"/>.
///</summary>
[TestClass]
public class PolicyDecoratorTests
{
    sealed class ListLogger : ILogger
    {
        public List<(LogLevel Level, string Message, Exception? Exception)> Entries { get; } = [];

        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) => Entries.Add((logLevel, formatter(state, exception), exception));
    }

    sealed class ManualTimeProvider : TimeProvider
    {
        long _ticks;

        public override long TimestampFrequency => TimeSpan.TicksPerSecond;

        public override long GetTimestamp() => _ticks;

        public void Advance(TimeSpan by) => _ticks += by.Ticks;
    }

    sealed class PassThrough : IAsyncPolicy
    {
        public Task<TResult> ExecuteAsync<TResult>(Func<CancellationToken, Task<TResult>> operation, CancellationToken cancellationToken = default) => operation(cancellationToken);
    }

    #region LoggingPolicyDecorator

    [TestMethod]
    public void Logging_Constructor_RejectsBadArguments()
    {
        Assert.ThrowsExactly<ArgumentNullException>(static () => new LoggingPolicyDecorator(null!, new ListLogger()));
        Assert.ThrowsExactly<ArgumentNullException>(static () => new LoggingPolicyDecorator(new PassThrough(), null!));
        Assert.ThrowsExactly<ArgumentException>(static () => new LoggingPolicyDecorator(new PassThrough(), new ListLogger(), " "));
    }

    [TestMethod]
    public async Task Logging_Success_LogsStartAndSuccessAndReturnsResult()
    {
        ListLogger logger = new();
        LoggingPolicyDecorator policy = new(new PassThrough(), logger, "db");

        int result = await policy.ExecuteAsync(static _ => Task.FromResult(42));

        Assert.AreEqual(42, result);
        Assert.AreEqual(2, logger.Entries.Count);
        StringAssert.Contains(logger.Entries[0].Message, "'db' starting");
        StringAssert.Contains(logger.Entries[1].Message, "'db' succeeded");
        Assert.AreEqual(LogLevel.Debug, logger.Entries[1].Level);
    }

    [TestMethod]
    public async Task Logging_Failure_LogsWarningWithExceptionAndRethrows()
    {
        ListLogger logger = new();
        LoggingPolicyDecorator policy = new(new PassThrough(), logger);
        InvalidOperationException boom = new("boom");

        InvalidOperationException caught = await Assert.ThrowsExactlyAsync<InvalidOperationException>(async () => await policy.ExecuteAsync<int>(_ => throw boom));

        Assert.AreSame(boom, caught);
        (LogLevel level, string message, Exception? exception) = logger.Entries[^1];
        Assert.AreEqual(LogLevel.Warning, level);
        StringAssert.Contains(message, "failed");
        Assert.AreSame(boom, exception);
    }

    [TestMethod]
    public async Task Logging_Cancellation_IsNotLoggedAsFailure()
    {
        ListLogger logger = new();
        LoggingPolicyDecorator policy = new(new PassThrough(), logger);

        await Assert.ThrowsExactlyAsync<OperationCanceledException>(async () => await policy.ExecuteAsync<int>(static _ => throw new OperationCanceledException()));

        Assert.IsFalse(logger.Entries.Any(static e => e.Level == LogLevel.Warning));
    }

    [TestMethod]
    public async Task Logging_NullOperation_Throws()
    {
        LoggingPolicyDecorator policy = new(new PassThrough(), new ListLogger());
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(async () => await policy.ExecuteAsync<int>(null!));
    }

    [TestMethod]
    public async Task Logging_ExecuteValueAsync_RunsOperationThroughPolicy()
    {
        ListLogger logger = new();
        LoggingPolicyDecorator policy = new(new PassThrough(), logger);

        int result = await policy.ExecuteValueAsync(static _ => ValueTask.FromResult(9));

        Assert.AreEqual(9, result);
        Assert.AreEqual(2, logger.Entries.Count);
    }

    [TestMethod]
    public async Task Logging_ExecuteValueAsync_NullOperation_Throws()
    {
        LoggingPolicyDecorator policy = new(new PassThrough(), new ListLogger());

        await Assert.ThrowsExactlyAsync<ArgumentNullException>(async () => await policy.ExecuteValueAsync<int>(null!));
    }

    #endregion

    #region MetricsPolicyDecorator

    [TestMethod]
    public void Metrics_Constructor_NullInner_Throws() { Assert.ThrowsExactly<ArgumentNullException>(static () => new MetricsPolicyDecorator(null!)); }

    [TestMethod]
    public void Metrics_Initially_Zero()
    {
        PolicyMetrics metrics = new MetricsPolicyDecorator(new PassThrough()).GetSnapshot();

        Assert.AreEqual(0, metrics.Executions);
        Assert.AreEqual(TimeSpan.Zero, metrics.AverageDuration);
    }

    [TestMethod]
    public async Task Metrics_CountsSuccessFailureAndCancellationSeparately()
    {
        MetricsPolicyDecorator policy = new(new PassThrough());

        await policy.ExecuteAsync(static _ => Task.FromResult(1));
        await Assert.ThrowsExactlyAsync<InvalidOperationException>(async () => await policy.ExecuteAsync<int>(static _ => throw new InvalidOperationException()));
        await Assert.ThrowsExactlyAsync<OperationCanceledException>(async () => await policy.ExecuteAsync<int>(static _ => throw new OperationCanceledException()));

        PolicyMetrics metrics = policy.GetSnapshot();
        Assert.AreEqual(3, metrics.Executions);
        Assert.AreEqual(1, metrics.Failures);
        Assert.AreEqual(1, metrics.Cancellations);
        Assert.AreEqual(1, metrics.Successes);
    }

    [TestMethod]
    public async Task Metrics_MeasuresDurationWithTimeProvider()
    {
        ManualTimeProvider clock = new();
        MetricsPolicyDecorator policy = new(new PassThrough(), clock);

        await policy.ExecuteAsync(_ => { clock.Advance(TimeSpan.FromSeconds(2)); return Task.FromResult(0); });
        await policy.ExecuteAsync(_ => { clock.Advance(TimeSpan.FromSeconds(4)); return Task.FromResult(0); });

        PolicyMetrics metrics = policy.GetSnapshot();
        Assert.AreEqual(TimeSpan.FromSeconds(6), metrics.TotalDuration);
        Assert.AreEqual(TimeSpan.FromSeconds(3), metrics.AverageDuration);
    }

    [TestMethod]
    public async Task Metrics_ExecuteValueAsync_IsCounted()
    {
        MetricsPolicyDecorator policy = new(new PassThrough());

        Assert.AreEqual(3, await policy.ExecuteValueAsync(static _ => ValueTask.FromResult(3)));
        await Assert.ThrowsExactlyAsync<InvalidOperationException>(async () => await policy.ExecuteValueAsync<int>(static _ => throw new InvalidOperationException()));

        PolicyMetrics metrics = policy.GetSnapshot();
        Assert.AreEqual(2, metrics.Executions);
        Assert.AreEqual(1, metrics.Failures);
    }

    [TestMethod]
    public async Task Metrics_ConcurrentExecutions_AreAllCounted()
    {
        MetricsPolicyDecorator policy = new(new PassThrough());

        await Task.WhenAll(Enumerable.Range(0, 200).Select(_ => Task.Run(() => policy.ExecuteAsync(static _ => Task.FromResult(1)))));

        Assert.AreEqual(200, policy.GetSnapshot().Executions);
    }

    #endregion
}
