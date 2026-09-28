namespace Catharsis.Monitoring;

///<summary>
///The aggregate health status <see cref="NetworkMonitor"/> assigns to an endpoint, derived from that endpoint's
///<see cref="Catharsis.Resilience.CircuitBreaker"/> state.
///</summary>
public enum EndpointStatus
{
    ///<summary>The endpoint's circuit is closed: recent sweeps have been succeeding.</summary>
    Healthy,

    ///<summary>The endpoint's circuit is half-open: it is being trialled again after a break.</summary>
    Degraded,

    ///<summary>The endpoint's circuit is open: sweeps are currently being rejected without being attempted.</summary>
    Down
}
