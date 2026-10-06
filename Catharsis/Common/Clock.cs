namespace Catharsis.Common;

///<summary>
///The default <see cref="ISystemClock"/> implementation, adapting a <see cref="TimeProvider"/> so callers can depend on
///the narrower <see cref="ISystemClock"/> abstraction instead of the full <see cref="TimeProvider"/> surface.
///</summary>
///<param name="timeProvider">The time provider to adapt, or <c>null</c> to use <see cref="TimeProvider.System"/>.</param>
public sealed class Clock(TimeProvider? timeProvider = null) : ISystemClock
{
    #region Fields
    private readonly TimeProvider _timeProvider = timeProvider ?? TimeProvider.System;
    #endregion

    #region Public properties
    ///<inheritdoc/>
    public DateTimeOffset UtcNow => _timeProvider.GetUtcNow();
    #endregion
}
