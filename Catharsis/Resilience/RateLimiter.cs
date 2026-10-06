namespace Catharsis.Resilience;

///<summary>
///A token-bucket rate limiter that permits up to <see cref="Capacity"/> operations in a burst, then refills tokens
///continuously at a configured rate, either rejecting or asynchronously waiting for callers once the bucket is
///empty.
///</summary>
///<example>
///<code>
///RateLimiter limiter = new(capacity: 10, tokensPerSecond: 2);
///
///if(limiter.TryAcquire())
///{
///    // Proceed immediately.
///}
///</code>
///</example>
public sealed class RateLimiter
{
    #region Fields
    readonly Lock _gate = new();
    readonly double _capacity;
    readonly double _tokensPerSecond;
    readonly TimeProvider _timeProvider;
    double _availableTokens;
    long _lastRefillTimestamp;
    #endregion

    #region Public methods
    ///<summary>
    ///Creates a rate limiter with the specified bucket capacity and refill rate.
    ///</summary>
    ///<param name="capacity">The maximum number of tokens the bucket can hold, and the largest burst allowed.</param>
    ///<param name="tokensPerSecond">The number of tokens added to the bucket per second.</param>
    ///<param name="timeProvider">The time source used to compute refills, or <c>null</c> to use <see cref="TimeProvider.System"/>.</param>
    ///<exception cref="ArgumentOutOfRangeException">
    ///<paramref name="capacity"/> or <paramref name="tokensPerSecond"/> is not greater than zero.
    ///</exception>
    public RateLimiter(double capacity, double tokensPerSecond, TimeProvider? timeProvider = null)
    {
        if (capacity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity must be greater than zero.");
        }

        if (tokensPerSecond <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(tokensPerSecond), "Token refill rate must be greater than zero.");
        }

        _capacity = capacity;
        _tokensPerSecond = tokensPerSecond;
        _timeProvider = timeProvider ?? TimeProvider.System;
        _availableTokens = capacity;
        _lastRefillTimestamp = _timeProvider.GetTimestamp();
    }

    ///<summary>
    ///Attempts to acquire the specified number of tokens without waiting.
    ///</summary>
    ///<param name="tokens">The number of tokens to acquire. Defaults to 1.</param>
    ///<returns><c>true</c> if the tokens were acquired; otherwise <c>false</c>.</returns>
    ///<exception cref="ArgumentOutOfRangeException"><paramref name="tokens"/> is not greater than zero.</exception>
    public bool TryAcquire(double tokens = 1)
    {
        if (tokens <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(tokens), "Token count must be greater than zero.");
        }

        lock (_gate)
        {
            Refill();

            if (_availableTokens < tokens)
            {
                return false;
            }

            _availableTokens -= tokens;
            return true;
        }
    }

    ///<summary>
    ///Asynchronously acquires the specified number of tokens, waiting for them to refill if necessary.
    ///</summary>
    ///<param name="tokens">The number of tokens to acquire. Defaults to 1.</param>
    ///<param name="cancellationToken">A cancellation token that can abandon the wait.</param>
    ///<returns>A task that completes once the tokens have been acquired.</returns>
    ///<exception cref="ArgumentOutOfRangeException">
    ///<paramref name="tokens"/> is not greater than zero, or exceeds the bucket capacity.
    ///</exception>
    public async Task AcquireAsync(double tokens = 1, CancellationToken cancellationToken = default)
    {
        if (tokens <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(tokens), "Token count must be greater than zero.");
        }

        if (tokens > _capacity)
        {
            throw new ArgumentOutOfRangeException(nameof(tokens), "Token count must not exceed the bucket capacity.");
        }

        while (true)
        {
            TimeSpan waitTime;

            lock (_gate)
            {
                Refill();

                if (_availableTokens >= tokens)
                {
                    _availableTokens -= tokens;
                    return;
                }

                double deficit = tokens - _availableTokens;
                waitTime = TimeSpan.FromSeconds(deficit / _tokensPerSecond);
            }

            await Task.Delay(waitTime, cancellationToken).ConfigureAwait(false);
        }
    }

    void Refill()
    {
        long now = _timeProvider.GetTimestamp();
        double elapsedSeconds = _timeProvider.GetElapsedTime(_lastRefillTimestamp, now).TotalSeconds;

        if (elapsedSeconds <= 0)
        {
            return;
        }

        _availableTokens = Math.Min(_capacity, _availableTokens + (elapsedSeconds * _tokensPerSecond));
        _lastRefillTimestamp = now;
    }
    #endregion

    #region Public properties
    ///<summary>
    ///Gets the bucket capacity.
    ///</summary>
    public double Capacity => _capacity;

    ///<summary>
    ///Gets the number of tokens currently available, after applying any pending refill.
    ///</summary>
    public double AvailableTokens
    {
        get
        {
            lock (_gate)
            {
                Refill();
                return _availableTokens;
            }
        }
    }
    #endregion
}
