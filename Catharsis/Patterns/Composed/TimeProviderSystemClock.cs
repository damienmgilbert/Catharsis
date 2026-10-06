using Catharsis.Common;

namespace Catharsis.Patterns.Composed;

///<summary>
///Adapts a <see cref="TimeProvider"/> to the <see cref="ISystemClock"/> interface, so code written against
///<see cref="ISystemClock"/> can run on the platform's <see cref="TimeProvider"/> (including a test double).
///</summary>
///<param name="timeProvider">The time source to adapt. Defaults to <see cref="TimeProvider.System"/>.</param>
public sealed class TimeProviderSystemClock(TimeProvider? timeProvider = null) : ISystemClock
{
    #region Fields
    readonly TimeProvider _timeProvider = timeProvider ?? TimeProvider.System;
    #endregion

    #region Public properties
    ///<inheritdoc/>
    public DateTimeOffset UtcNow => _timeProvider.GetUtcNow();
    #endregion
}
