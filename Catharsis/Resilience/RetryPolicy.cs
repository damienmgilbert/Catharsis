using System.Diagnostics;

namespace Catharsis.Resilience;

///<summary>
///A fluent, composable retry policy that executes an operation with configurable retry logic including exponential
///backoff, jitter, and predicate-based retry decisions. Use the fluent builder methods to configure the policy, then
///call one of the <c>Execute</c> overloads to run an operation under the policy.
///</summary>
///<example>
///<code>
///var policy = new RetryPolicy()
///    .MaxAttempts(5)
///    .InitialDelay(TimeSpan.FromMilliseconds(200))
///    .ExponentialBackoff(2.0)
///    .WithJitter()
///    .RetryOn&lt;HttpRequestException&gt;();
///
///string result = await policy.ExecuteAsync(
///    async ct => await httpClient.GetStringAsync(url, ct),
///    cancellationToken);
///</code>
///</example>
public sealed class RetryPolicy : IAsyncPolicy
{
    #region Fields
    TimeSpan _initialDelay = TimeSpan.FromMilliseconds(200);
    double _backoffMultiplier = 2.0;
    int _maxAttempts = 3;
    TimeSpan _maxDelay = TimeSpan.FromSeconds(30);
    Func<Exception, bool> _retryPredicate = static _ => true;
    bool _useJitter;
    #endregion

    #region Private methods
    TimeSpan ComputeDelay(int attempt)
    {
        double delayMs = _initialDelay.TotalMilliseconds * Math.Pow(_backoffMultiplier, attempt);
        delayMs = Math.Min(delayMs, _maxDelay.TotalMilliseconds);

        if (_useJitter)
        {
            delayMs *= 0.5 + (Random.Shared.NextDouble() * 0.5);
        }

        return TimeSpan.FromMilliseconds(delayMs);
    }
    #endregion

    #region Public methods
    ///<summary>
    ///Executes the specified operation, retrying on failure according to the configured policy.
    ///</summary>
    ///<typeparam name="TResult">The return type of the operation.</typeparam>
    ///<param name="operation">The operation to execute.</param>
    ///<returns>The result of the successful operation.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="operation"/> is <c>null</c>.</exception>
    ///<exception cref="AggregateException">
    ///Thrown when all retry attempts are exhausted. Contains all exceptions from each failed attempt.
    ///</exception>
    public TResult Execute<TResult>(Func<TResult> operation)
    {
        if (operation is null)
        {
            throw new ArgumentNullException(nameof(operation), "Operation must not be null.");
        }

        List<Exception>? exceptions = null;

        for (int attempt = 0; attempt < _maxAttempts; attempt++)
        {
            try
            {
                return operation();
            }
            catch (Exception ex) when (attempt < _maxAttempts - 1 && _retryPredicate(ex))
            {
                exceptions ??= [with(_maxAttempts)];
                exceptions.Add(ex);

                Thread.Sleep(ComputeDelay(attempt));
            }
            catch (Exception ex) when (exceptions is not null && _retryPredicate(ex))
            {
                exceptions.Add(ex);
                throw new AggregateException("All retry attempts have been exhausted.", exceptions);
            }
        }

        throw new UnreachableException();
    }

    ///<summary>
    ///Executes the specified operation, retrying on failure according to the configured policy.
    ///</summary>
    ///<param name="operation">The operation to execute.</param>
    ///<exception cref="ArgumentNullException"><paramref name="operation"/> is <c>null</c>.</exception>
    ///<exception cref="AggregateException">
    ///Thrown when all retry attempts are exhausted. Contains all exceptions from each failed attempt.
    ///</exception>
    public void Execute(Action operation)
    {
        if (operation is null)
        {
            throw new ArgumentNullException(nameof(operation), "Operation must not be null.");
        }

        Execute<object?>(() =>
        {
            operation();
            return null;
        });
    }

    ///<summary>
    ///Asynchronously executes the specified operation, retrying on failure according to the configured policy.
    ///</summary>
    ///<typeparam name="TResult">The return type of the operation.</typeparam>
    ///<param name="operation">The async operation to execute. Receives a <see cref="CancellationToken"/>.</param>
    ///<param name="cancellationToken">A cancellation token that can cancel the retry loop.</param>
    ///<returns>The result of the successful operation.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="operation"/> is <c>null</c>.</exception>
    ///<exception cref="AggregateException">
    ///Thrown when all retry attempts are exhausted. Contains all exceptions from each failed attempt.
    ///</exception>
    public async Task<TResult> ExecuteAsync<TResult>(Func<CancellationToken, Task<TResult>> operation, CancellationToken cancellationToken = default)
    {
        if (operation is null)
        {
            throw new ArgumentNullException(nameof(operation), "Operation must not be null.");
        }

        List<Exception>? exceptions = null;

        for (int attempt = 0; attempt < _maxAttempts; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                return await operation(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is not OperationCanceledException && attempt < _maxAttempts - 1 && _retryPredicate(ex))
            {
                exceptions ??= [with(_maxAttempts)];
                exceptions.Add(ex);

                await Task.Delay(ComputeDelay(attempt), cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is not OperationCanceledException && exceptions is not null && _retryPredicate(ex))
            {
                exceptions.Add(ex);
                throw new AggregateException("All retry attempts have been exhausted.", exceptions);
            }
        }

        throw new UnreachableException();
    }

    ///<summary>
    ///Asynchronously executes the specified operation, retrying on failure according to the configured policy.
    ///</summary>
    ///<param name="operation">The async operation to execute. Receives a <see cref="CancellationToken"/>.</param>
    ///<param name="cancellationToken">A cancellation token that can cancel the retry loop.</param>
    ///<exception cref="ArgumentNullException"><paramref name="operation"/> is <c>null</c>.</exception>
    ///<exception cref="AggregateException">
    ///Thrown when all retry attempts are exhausted. Contains all exceptions from each failed attempt.
    ///</exception>
    public async Task ExecuteAsync(Func<CancellationToken, Task> operation, CancellationToken cancellationToken = default)
    {
        if (operation is null)
        {
            throw new ArgumentNullException(nameof(operation), "Operation must not be null.");
        }

        await ExecuteAsync<object?>(async ct =>
        {
            await operation(ct).ConfigureAwait(false);
            return null;
        }, cancellationToken).ConfigureAwait(false);
    }

    ///<summary>
    ///Asynchronously executes the specified operation using <see cref="ValueTask{TResult}"/>, retrying on failure
    ///according to the configured policy.
    ///</summary>
    ///<typeparam name="TResult">The return type of the operation.</typeparam>
    ///<param name="operation">The async operation to execute. Receives a <see cref="CancellationToken"/>.</param>
    ///<param name="cancellationToken">A cancellation token that can cancel the retry loop.</param>
    ///<returns>The result of the successful operation.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="operation"/> is <c>null</c>.</exception>
    ///<exception cref="AggregateException">
    ///Thrown when all retry attempts are exhausted. Contains all exceptions from each failed attempt.
    ///</exception>
    public ValueTask<TResult> ExecuteValueAsync<TResult>(Func<CancellationToken, ValueTask<TResult>> operation, CancellationToken cancellationToken = default)
    {
        if (operation is null)
        {
            throw new ArgumentNullException(nameof(operation), "Operation must not be null.");
        }

        return Core(operation, cancellationToken);

        async ValueTask<TResult> Core(Func<CancellationToken, ValueTask<TResult>> op, CancellationToken ct)
        {
            List<Exception>? exceptions = null;

            for (int attempt = 0; attempt < _maxAttempts; attempt++)
            {
                ct.ThrowIfCancellationRequested();

                try
                {
                    return await op(ct).ConfigureAwait(false);
                }
                catch (Exception ex) when (ex is not OperationCanceledException && attempt < _maxAttempts - 1 && _retryPredicate(ex))
                {
                    exceptions ??= [with(_maxAttempts)];
                    exceptions.Add(ex);

                    await Task.Delay(ComputeDelay(attempt), ct).ConfigureAwait(false);
                }
                catch (Exception ex) when (ex is not OperationCanceledException && exceptions is not null && _retryPredicate(ex))
                {
                    exceptions.Add(ex);
                    throw new AggregateException("All retry attempts have been exhausted.", exceptions);
                }
            }

            throw new UnreachableException();
        }
    }

    ///<summary>
    ///Asynchronously executes the specified void operation using <see cref="ValueTask"/>, retrying on failure according
    ///to the configured policy.
    ///</summary>
    ///<param name="operation">The async operation to execute. Receives a <see cref="CancellationToken"/>.</param>
    ///<param name="cancellationToken">A cancellation token that can cancel the retry loop.</param>
    ///<exception cref="ArgumentNullException"><paramref name="operation"/> is <c>null</c>.</exception>
    ///<exception cref="AggregateException">
    ///Thrown when all retry attempts are exhausted. Contains all exceptions from each failed attempt.
    ///</exception>
    public ValueTask ExecuteValueAsync(Func<CancellationToken, ValueTask> operation, CancellationToken cancellationToken = default)
    {
        if (operation is null)
        {
            throw new ArgumentNullException(nameof(operation), "Operation must not be null.");
        }

        return Core(operation, cancellationToken);

        async ValueTask Core(Func<CancellationToken, ValueTask> op, CancellationToken ct)
        {
            await ExecuteValueAsync<object?>(async token =>
            {
                await op(token).ConfigureAwait(false);
                return null;
            }, ct).ConfigureAwait(false);
        }
    }

    ///<summary>
    ///Sets the exponential backoff multiplier. Each successive delay is multiplied by this factor.
    ///</summary>
    ///<param name="multiplier">The backoff multiplier. Must be greater than or equal to 1.0.</param>
    ///<returns>The current <see cref="RetryPolicy"/> for fluent chaining.</returns>
    ///<exception cref="ArgumentOutOfRangeException"><paramref name="multiplier"/> is less than 1.0.</exception>
    public RetryPolicy ExponentialBackoff(double multiplier = 2.0)
    {
        if (multiplier < 1.0)
        {
            throw new ArgumentOutOfRangeException(nameof(multiplier), "Backoff multiplier must be at least 1.0.");
        }

        _backoffMultiplier = multiplier;
        return this;
    }

    ///<summary>
    ///Sets the initial delay before the first retry.
    ///</summary>
    ///<param name="delay">The initial delay. Must be non-negative.</param>
    ///<returns>The current <see cref="RetryPolicy"/> for fluent chaining.</returns>
    ///<exception cref="ArgumentOutOfRangeException"><paramref name="delay"/> is negative.</exception>
    public RetryPolicy InitialDelay(TimeSpan delay)
    {
        if (delay < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(delay), "Initial delay must not be negative.");
        }

        _initialDelay = delay;
        return this;
    }

    ///<summary>
    ///Sets the maximum number of attempts (including the initial attempt).
    ///</summary>
    ///<param name="attempts">The maximum number of attempts. Must be at least 1.</param>
    ///<returns>The current <see cref="RetryPolicy"/> for fluent chaining.</returns>
    ///<exception cref="ArgumentOutOfRangeException"><paramref name="attempts"/> is less than 1.</exception>
    public RetryPolicy MaxAttempts(int attempts)
    {
        if (attempts < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(attempts), "Maximum attempts must be at least 1.");
        }

        _maxAttempts = attempts;
        return this;
    }

    ///<summary>
    ///Sets the maximum delay cap. No individual delay will exceed this value regardless of the backoff calculation.
    ///</summary>
    ///<param name="maxDelay">The maximum delay. Must be non-negative.</param>
    ///<returns>The current <see cref="RetryPolicy"/> for fluent chaining.</returns>
    ///<exception cref="ArgumentOutOfRangeException"><paramref name="maxDelay"/> is negative.</exception>
    public RetryPolicy MaxDelay(TimeSpan maxDelay)
    {
        if (maxDelay < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(maxDelay), "Maximum delay must not be negative.");
        }

        _maxDelay = maxDelay;
        return this;
    }

    ///<summary>
    ///Restricts retries to exceptions of type <typeparamref name="TException"/>. This replaces any previously
    ///configured retry predicate.
    ///</summary>
    ///<typeparam name="TException">The exception type that triggers a retry.</typeparam>
    ///<returns>The current <see cref="RetryPolicy"/> for fluent chaining.</returns>
    public RetryPolicy RetryOn<TException>() where TException : Exception
    {
        _retryPredicate = static ex => ex is TException;
        return this;
    }

    ///<summary>
    ///Sets a custom predicate that determines whether a failed attempt should be retried. This replaces any previously
    ///configured retry predicate.
    ///</summary>
    ///<param name="predicate">A function that returns <c>true</c> if the exception is retriable.</param>
    ///<returns>The current <see cref="RetryPolicy"/> for fluent chaining.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="predicate"/> is <c>null</c>.</exception>
    public RetryPolicy RetryWhen(Func<Exception, bool> predicate)
    {
        if (predicate is null)
        {
            throw new ArgumentNullException(nameof(predicate), "Retry predicate must not be null.");
        }

        _retryPredicate = predicate;
        return this;
    }

    ///<summary>
    ///Enables jitter on retry delays. Each delay is randomly scaled between 50% and 100% of the computed value to
    ///prevent thundering-herd effects when multiple callers retry simultaneously.
    ///</summary>
    ///<returns>The current <see cref="RetryPolicy"/> for fluent chaining.</returns>
    public RetryPolicy WithJitter()
    {
        _useJitter = true;
        return this;
    }
    #endregion

    #region Public properties
    ///<summary>
    ///Gets the configured maximum number of attempts.
    ///</summary>
    public int ConfiguredMaxAttempts => _maxAttempts;

    ///<summary>
    ///Gets the configured initial delay.
    ///</summary>
    public TimeSpan ConfiguredInitialDelay => _initialDelay;
    #endregion
}
