namespace Catharsis.Resilience;

///<summary>
///The operating state of a <see cref="CircuitBreaker"/>.
///</summary>
public enum CircuitState
{
    ///<summary>
    ///The circuit is closed: calls are allowed through and failures are being counted toward the threshold.
    ///</summary>
    Closed,

    ///<summary>
    ///The circuit is open: calls are rejected immediately without invoking the operation.
    ///</summary>
    Open,

    ///<summary>
    ///The break duration has elapsed and a single trial call is being allowed through to test recovery.
    ///</summary>
    HalfOpen
}
