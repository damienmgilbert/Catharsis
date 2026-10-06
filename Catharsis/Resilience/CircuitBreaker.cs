namespace Catharsis.Resilience;

///<summary>
///A circuit breaker that stops calling a failing operation once its consecutive-failure count reaches a threshold,
///giving the downstream dependency time to recover before allowing a single trial call through. See ///<see
///cref="CircuitState"/> for the state machine (closed, open, half-open).
///</summary>
///<example>
public sealed class CircuitBreaker : IAsyncPolicy
{
    #region Fields
    private readonly TimeSpan _breakDuration;
    private int _consecutiveFailures;
    private readonly int _failureThreshold;
    private readonly Lock _gate = new();
    private long _openedAtTimestamp;
    private CircuitState _state = CircuitState.Closed;
    private readonly TimeProvider _timeProvider;
    #endregion

    #region Constructors
    ///<summary>
    ///Creates a circuit breaker with the specified failure threshold and break duration.
    ///</summary>
    ///<param name="failureThreshold">The number of consecutive failures that opens the circuit.</param>
    ///<param name="breakDuration">How long the circuit stays open before allowing a trial call.</param>
    ///<param name="timeProvider">The time source used to measure the break duration, or <c>null</c> to use <see cref="TimeProvider.System"/>.</param>
    ///<exception cref="ArgumentOutOfRangeException">
    public CircuitBreaker(int failureThreshold, TimeSpan breakDuration, TimeProvider? timeProvider = null)
    {
        if(failureThreshold < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(failureThreshold), "Failure threshold must be at least 1.");
        }

        if(breakDuration < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(breakDuration), "Break duration must not be negative.");
        }

        _failureThreshold = failureThreshold;
        _breakDuration = breakDuration;
        _timeProvider = timeProvider ?? TimeProvider.System;
    }
    #endregion

    #region Private methods
    private void OnBeforeExecute()
    {
        lock(_gate)
        {
            if(_state != CircuitState.Open)
            {
                return;
            }

            if(_timeProvider.GetElapsedTime(_openedAtTimestamp).CompareTo(_breakDuration) >= 0)
            {
                _state = CircuitState.HalfOpen;
                return;
            }

            throw new CircuitBreakerOpenException();
        }
    }

    private void OnFailure()
    {
        lock(_gate)
        {
            if(_state == CircuitState.HalfOpen)
            {
                Open();
                return;
            }

            _consecutiveFailures++;

            if(_consecutiveFailures >= _failureThreshold)
            {
                Open();
            }
        }
    }

    private void OnSuccess()
    {
        lock(_gate)
        {
            _consecutiveFailures = 0;
            _state = CircuitState.Closed;
        }
    }

    private void Open()
    {
        _state = CircuitState.Open;
        _openedAtTimestamp = _timeProvider.GetTimestamp();
        _consecutiveFailures = 0;
    }
    #endregion

    #region Public methods
    ///<summary>
    ///Executes the specified operation, subject to the circuit breaker's current state.
    ///</summary>
    ///<typeparam name="TResult">The return type of the operation.</typeparam>
    ///<param name="operation">The operation to execute.</param>
    ///<returns>The result of the successful operation.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="operation"/> is <c>null</c>.</exception>
    ///<exception cref="CircuitBreakerOpenException">The circuit is open.</exception>
    public TResult Execute<TResult>(Func<TResult> operation)
    {
        ArgumentNullException.ThrowIfNull(operation);
        OnBeforeExecute();

        try
        {
            TResult result = operation();
            OnSuccess();
            return result;
        } catch(Exception ex) when(ex is not OperationCanceledException)
        {
            OnFailure();
            throw;
        }
    }

    ///<summary>
    ///Executes the specified operation, subject to the circuit breaker's current state.
    ///</summary>
    ///<param name="operation">The operation to execute.</param>
    ///<exception cref="ArgumentNullException"><paramref name="operation"/> is <c>null</c>.</exception>
    ///<exception cref="CircuitBreakerOpenException">The circuit is open.</exception>
    public void Execute(Action operation)
    {
        ArgumentNullException.ThrowIfNull(operation);

        Execute<object?>(
        () =>
        {
            operation();
            return null;
        });
    }

    ///<summary>
    ///Asynchronously executes the specified operation, subject to the circuit breaker's current state.
    ///</summary>
    ///<typeparam name="TResult">The return type of the operation.</typeparam>
    ///<param name="operation">The async operation to execute. Receives a <see cref="CancellationToken"/>.</param>
    ///<param name="cancellationToken">A cancellation token passed to the operation.</param>
    ///<returns>The result of the successful operation.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="operation"/> is <c>null</c>.</exception>
    ///<exception cref="CircuitBreakerOpenException">The circuit is open.</exception>
    public async Task<TResult> ExecuteAsync<TResult>(Func<CancellationToken, Task<TResult>> operation, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);
        OnBeforeExecute();

        try
        {
            TResult result = await operation(cancellationToken).ConfigureAwait(false);
            OnSuccess();
            return result;
        } catch(Exception ex) when(ex is not OperationCanceledException)
        {
            OnFailure();
            throw;
        }
    }

    ///<summary>
    ///Asynchronously executes the specified operation, subject to the circuit breaker's current state.
    ///</summary>
    ///<param name="operation">The async operation to execute. Receives a <see cref="CancellationToken"/>.</param>
    ///<param name="cancellationToken">A cancellation token passed to the operation.</param>
    ///<exception cref="ArgumentNullException"><paramref name="operation"/> is <c>null</c>.</exception>
    ///<exception cref="CircuitBreakerOpenException">The circuit is open.</exception>
    public async Task ExecuteAsync(Func<CancellationToken, Task> operation, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);

        await ExecuteAsync<object?>(
              async ct =>
              {
                  await operation(ct).ConfigureAwait(false);
                  return null;
              },
              cancellationToken)
            .ConfigureAwait(false);
    }

    ///<summary>
    ///Forces the circuit back to <see cref="CircuitState.Closed"/> and clears the failure count.
    ///</summary>
    public void Reset()
    {
        lock(_gate)
        {
            _state = CircuitState.Closed;
            _consecutiveFailures = 0;
        }
    }
    #endregion

    #region Public properties
    ///<summary>
    ///Gets the current state of the circuit.
    ///</summary>
    public CircuitState State
    {
        get
        {
            lock(_gate)
            {
                return _state;
            }
        }
    }
    #endregion
}
