using System.Diagnostics;

namespace Catharsis.Common;

///<summary>
///A lightweight, allocation-free stopwatch based on <see cref="Stopwatch.GetTimestamp()"/> for high-precision
///performance measurements.
///</summary>
public readonly struct ValueStopwatch
{
    #region Struct fields
    readonly long _startTimestamp;
    #endregion

    #region Constructors
    ValueStopwatch(long startTimestamp) { _startTimestamp = startTimestamp; }
    #endregion

    #region Public methods
    ///<summary>
    ///Gets the elapsed time in microseconds since the stopwatch was started.
    ///</summary>
    ///<returns>Elapsed microseconds as a double.</returns>
    public double GetElapsedMicroseconds() { return GetElapsedTime().TotalMicroseconds; }
    ///<summary>
    ///Gets the elapsed time in milliseconds since the stopwatch was started.
    ///</summary>
    ///<returns>Elapsed milliseconds as a double.</returns>
    public double GetElapsedMilliseconds() { return GetElapsedTime().TotalMilliseconds; }

    ///<summary>
    ///Gets the elapsed time since the stopwatch was started.
    ///</summary>
    ///<returns>The elapsed <see cref="TimeSpan"/>.</returns>
    ///<exception cref="InvalidOperationException">The stopwatch was not started.</exception>
    public TimeSpan GetElapsedTime()
    {
        if(!IsActive)
        {
            throw new InvalidOperationException("The stopwatch has not been started. Call StartNew() first.");
        }

        return Stopwatch.GetElapsedTime(_startTimestamp);
    }

    ///<summary>
    ///Starts a new <see cref="ValueStopwatch"/>.
    ///</summary>
    ///<returns>A running stopwatch.</returns>
    public static ValueStopwatch StartNew() { return new(Stopwatch.GetTimestamp()); }
    #endregion

    #region Public properties
    ///<summary>
    ///Gets whether this stopwatch has been started.
    ///</summary>
    public bool IsActive => _startTimestamp != 0;
    #endregion
}
