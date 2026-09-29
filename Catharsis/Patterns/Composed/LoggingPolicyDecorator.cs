using Catharsis.Resilience;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Catharsis.Patterns.Composed;

///<summary>
///Decorates an <see cref="IAsyncPolicy"/> with logging: it records when an execution starts and how long it took to
///succeed or fail. Cancellation is not logged as a failure. Log messages are generated at compile time
///(<see cref="LoggerMessageAttribute"/>), so a disabled level costs almost nothing.
///</summary>
public sealed partial class LoggingPolicyDecorator : IAsyncPolicy
{
    #region Fields
    readonly IAsyncPolicy _inner;
    readonly ILogger _logger;
    readonly string _name;
    #endregion

    #region Constructors
    ///<summary>
    ///Initializes a new <see cref="LoggingPolicyDecorator"/>.
    ///</summary>
    ///<param name="inner">The policy to decorate.</param>
    ///<param name="logger">Receives the log entries.</param>
    ///<param name="name">A label identifying the policy in log entries.</param>
    ///<exception cref="ArgumentNullException"><paramref name="inner"/> or <paramref name="logger"/> is <c>null</c>.</exception>
    ///<exception cref="ArgumentException"><paramref name="name"/> is empty.</exception>
    public LoggingPolicyDecorator(IAsyncPolicy inner, ILogger logger, string name = "policy")
    {
        ArgumentNullException.ThrowIfNull(inner);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        _inner = inner;
        _logger = logger;
        _name = name;
    }
    #endregion

    #region Public methods
    ///<inheritdoc/>
    public async Task<TResult> ExecuteAsync<TResult>(Func<CancellationToken, Task<TResult>> operation, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);

        long start = Stopwatch.GetTimestamp();
        LogStarting(_logger, _name);

        try
        {
            TResult result = await _inner.ExecuteAsync(operation, cancellationToken).ConfigureAwait(false);
            double elapsedMs = Stopwatch.GetElapsedTime(start).TotalMilliseconds;
            LogSucceeded(_logger, _name, elapsedMs);
            return result;
        }
        catch(Exception ex) when(ex is not OperationCanceledException)
        {
            double elapsedMs = Stopwatch.GetElapsedTime(start).TotalMilliseconds;
            LogFailed(_logger, ex, _name, elapsedMs);
            throw;
        }
    }
    #endregion

    #region Private methods
    [LoggerMessage(Level = LogLevel.Debug, Message = "Policy '{PolicyName}' starting")]
    static partial void LogStarting(ILogger logger, string policyName);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Policy '{PolicyName}' succeeded in {ElapsedMs:F1} ms")]
    static partial void LogSucceeded(ILogger logger, string policyName, double elapsedMs);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Policy '{PolicyName}' failed after {ElapsedMs:F1} ms")]
    static partial void LogFailed(ILogger logger, Exception exception, string policyName, double elapsedMs);
    #endregion
}
