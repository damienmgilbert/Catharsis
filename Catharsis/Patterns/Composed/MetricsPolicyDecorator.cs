using Catharsis.Resilience;

namespace Catharsis.Patterns.Composed;

///<summary>
///Decorates an <see cref="IAsyncPolicy"/> with counters for executions, failures, cancellations and running time, read
///with <see cref="GetSnapshot"/>. Counting is lock-free and safe for concurrent executions.
///</summary>
public sealed class MetricsPolicyDecorator : IAsyncPolicy
{
    #region Fields
    readonly IAsyncPolicy _inner;
    readonly TimeProvider _timeProvider;
    long _executions;
    long _failures;
    long _cancellations;
    long _totalTicks;
    #endregion

    #region Constructors
    ///<summary>
    ///Initializes a new <see cref="MetricsPolicyDecorator"/>.
    ///</summary>
    ///<param name="inner">The policy to decorate.</param>
    ///<param name="timeProvider">The clock used to time executions. Defaults to <see cref="TimeProvider.System"/>.</param>
    ///<exception cref="ArgumentNullException"><paramref name="inner"/> is <c>null</c>.</exception>
    public MetricsPolicyDecorator(IAsyncPolicy inner, TimeProvider? timeProvider = null)
    {
        ArgumentNullException.ThrowIfNull(inner);

        _inner = inner;
        _timeProvider = timeProvider ?? TimeProvider.System;
    }
    #endregion

    #region Public methods
    ///<inheritdoc/>
    public async Task<TResult> ExecuteAsync<TResult>(Func<CancellationToken, Task<TResult>> operation, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);

        long start = _timeProvider.GetTimestamp();

        try
        {
            return await _inner.ExecuteAsync(operation, cancellationToken).ConfigureAwait(false);
        }
        catch(OperationCanceledException)
        {
            Interlocked.Increment(ref _cancellations);
            throw;
        }
        catch(Exception)
        {
            Interlocked.Increment(ref _failures);
            throw;
        }
        finally
        {
            Interlocked.Add(ref _totalTicks, _timeProvider.GetElapsedTime(start).Ticks);
            Interlocked.Increment(ref _executions);
        }
    }

    ///<summary>
    ///Reads the current counters. Under concurrent execution the values may be a few operations out of step with each
    ///other.
    ///</summary>
    public PolicyMetrics GetSnapshot() => new(
        Interlocked.Read(ref _executions),
        Interlocked.Read(ref _failures),
        Interlocked.Read(ref _cancellations),
        TimeSpan.FromTicks(Interlocked.Read(ref _totalTicks)));
    #endregion
}
