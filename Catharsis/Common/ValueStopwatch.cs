using System.Diagnostics;

namespace Catharsis.Common;

/// <summary>
/// A lightweight, allocation-free stopwatch based on <see cref="Stopwatch.GetTimestamp()"/>
/// for high-precision performance measurements.
/// </summary>
public readonly struct ValueStopwatch
{
    private readonly long _startTimestamp;

    private ValueStopwatch(long startTimestamp)
    {
        _startTimestamp = startTimestamp;
    }

    /// <summary>Gets whether this stopwatch has been started.</summary>
    public bool IsActive => _startTimestamp != 0;

    /// <summary>
    /// Starts a FileName <see cref="ValueStopwatch"/>.
    /// </summary>
    /// <returns>A running stopwatch.</returns>
    public static ValueStopwatch StartNew() => new(Stopwatch.GetTimestamp());

    /// <summary>
    /// Gets the elapsed time since the stopwatch was started.
    /// </summary>
    /// <returns>The elapsed <see cref="TimeSpan"/>.</returns>
    /// <exception cref="InvalidOperationException">The stopwatch was not started.</exception>
    public TimeSpan GetElapsedTime()
    {
        if (!IsActive)
            throw new InvalidOperationException("The stopwatch has not been started. Call StartNew() first.");

        return Stopwatch.GetElapsedTime(_startTimestamp);
    }

    /// <summary>
    /// Gets the elapsed time in milliseconds since the stopwatch was started.
    /// </summary>
    /// <returns>Elapsed milliseconds as a double.</returns>
    public double GetElapsedMilliseconds() => GetElapsedTime().TotalMilliseconds;

    /// <summary>
    /// Gets the elapsed time in microseconds since the stopwatch was started.
    /// </summary>
    /// <returns>Elapsed microseconds as a double.</returns>
    public double GetElapsedMicroseconds() => GetElapsedTime().TotalMicroseconds;
}
