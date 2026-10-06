namespace Catharsis.Monitoring;

///<summary>
///Published on the <see cref="Catharsis.Events.EventBus"/> by <see cref="NetworkMonitor"/> whenever a sweep changes an
///endpoint's aggregate <see cref="EndpointStatus"/>.
///</summary>
///<param name="Endpoint">The endpoint whose status changed.</param>
///<param name="Previous">The endpoint's status before this sweep.</param>
///<param name="Current">The endpoint's status after this sweep.</param>
///<param name="ChangedAtUtc">The UTC timestamp at which the change was detected.</param>
public sealed record EndpointStatusChangedEvent(Uri Endpoint, EndpointStatus Previous, EndpointStatus Current, DateTimeOffset ChangedAtUtc);
