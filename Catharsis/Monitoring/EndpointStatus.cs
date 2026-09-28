namespace Catharsis.Monitoring;

///<summary>
///The aggregate health status <see cref="NetworkMonitor"/> assigns to an endpoint, derived from that endpoint's
///<see cref="Catharsis.Resilience.CircuitBreaker"/> state and recent success ratio (see
///<see cref="NetworkMonitor.DetermineStatus"/>).
///</summary>
public enum EndpointStatus
{
    ///<summary>The endpoint's circuit is not open and every recent sweep has succeeded.</summary>
    Healthy,

    ///<summary>The endpoint's circuit is not open, but at least one recent sweep has failed.</summary>
    Degraded,

    ///<summary>The endpoint's circuit is open: sweeps are currently being rejected without being attempted.</summary>
    Down
}
