using Catharsis.Common;

namespace Catharsis.Patterns.Composed;

///<summary>
///Adapts an <see cref="ISystemClock"/> to <see cref="TimeProvider"/>, the reverse of
///<see cref="TimeProviderSystemClock"/>. Only the wall-clock time comes from the adapted clock; timestamps and timers
///use the system implementation.
///</summary>
///<param name="clock">The clock supplying <see cref="GetUtcNow"/>.</param>
public sealed class SystemClockTimeProvider(ISystemClock clock) : TimeProvider
{
    #region Fields
    readonly ISystemClock _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    #endregion

    #region Public methods
    ///<inheritdoc/>
    public override DateTimeOffset GetUtcNow() => _clock.UtcNow;
    #endregion
}
