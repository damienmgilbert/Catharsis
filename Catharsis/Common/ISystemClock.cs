namespace Catharsis.Common;

///<summary>
///Abstracts access to the current time so code that depends on "now" can be unit tested with a fixed or controllable
///clock instead of the real system clock.
///</summary>
public interface ISystemClock
{
    #region Public properties

    ///<summary>
    ///Gets the current date and time, expressed in UTC.
    ///</summary>
    DateTimeOffset UtcNow { get; }
    #endregion
}
