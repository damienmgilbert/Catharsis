namespace Catharsis.Scheduling;

///<summary>
///Computes exponential backoff delays with a configurable jitter strategy, so retry loops avoid a thundering herd of
///synchronized retries. Useful for hand-rolled retry loops, scheduled reconnect logic, or anywhere a delay similar to
///<see cref="Catharsis.Resilience.RetryPolicy"/>'s internal backoff is needed standalone.
///</summary>
///<example>
///<code>
///JitteredDelayCalculator calculator = new(TimeSpan.FromMilliseconds(200), TimeSpan.FromSeconds(30));
///TimeSpan delay = calculator.ComputeDelay(attempt: 2);
///</code>
///</example>
public sealed class JitteredDelayCalculator
{
    #region Fields
    readonly TimeSpan _baseDelay;
    readonly TimeSpan _maxDelay;
    readonly double _multiplier;
    readonly JitterStrategy _jitter;
    readonly Random _random;
    #endregion

    #region Public methods
    ///<summary>
    ///Creates a delay calculator with the specified base delay, maximum delay, backoff multiplier, and jitter strategy.
    ///</summary>
    ///<param name="baseDelay">The delay for the first attempt (attempt 0), before jitter is applied.</param>
    ///<param name="maxDelay">The maximum delay, capping the exponential growth.</param>
    ///<param name="multiplier">The exponential backoff multiplier. Defaults to 2.0.</param>
    ///<param name="jitter">The jitter strategy to apply. Defaults to <see cref="JitterStrategy.Full"/>.</param>
    ///<param name="random">The random source used for jitter, or <c>null</c> to use <see cref="Random.Shared"/>.</param>
    ///<exception cref="ArgumentOutOfRangeException">
    ///<paramref name="baseDelay"/> is negative, <paramref name="maxDelay"/> is less than <paramref name="baseDelay"/>, or
    ///<paramref name="multiplier"/> is less than 1.0.
    ///</exception>
    public JitteredDelayCalculator(TimeSpan baseDelay, TimeSpan maxDelay, double multiplier = 2.0, JitterStrategy jitter = JitterStrategy.Full, Random? random = null)
    {
        if(baseDelay < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(baseDelay), "Base delay must not be negative.");
        }

        if(maxDelay < baseDelay)
        {
            throw new ArgumentOutOfRangeException(nameof(maxDelay), "Maximum delay must not be less than the base delay.");
        }

        if(multiplier < 1.0)
        {
            throw new ArgumentOutOfRangeException(nameof(multiplier), "Multiplier must be at least 1.0.");
        }

        _baseDelay = baseDelay;
        _maxDelay = maxDelay;
        _multiplier = multiplier;
        _jitter = jitter;
        _random = random ?? Random.Shared;
    }

    ///<summary>
    ///Computes the delay for the specified attempt number.
    ///</summary>
    ///<param name="attempt">The zero-based attempt number.</param>
    ///<returns>The computed delay, with jitter applied according to the configured strategy.</returns>
    ///<exception cref="ArgumentOutOfRangeException"><paramref name="attempt"/> is negative.</exception>
    public TimeSpan ComputeDelay(int attempt)
    {
        if(attempt < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(attempt), "Attempt must not be negative.");
        }

        double exponential = _baseDelay.TotalMilliseconds * Math.Pow(_multiplier, attempt);
        double capped = Math.Min(exponential, _maxDelay.TotalMilliseconds);

        double result = _jitter switch
        {
            JitterStrategy.None => capped,
            JitterStrategy.Full => _random.NextDouble() * capped,
            JitterStrategy.Equal => (capped / 2) + (_random.NextDouble() * (capped / 2)),
            _ => capped
        };

        return TimeSpan.FromMilliseconds(result);
    }
    #endregion
}
