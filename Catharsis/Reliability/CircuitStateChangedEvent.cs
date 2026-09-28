using Catharsis.Resilience;

namespace Catharsis.Reliability;

///<summary>
///Published on the <see cref="Catharsis.Events.EventBus"/> by <see cref="ResiliencyPipelineRegistry"/> whenever a
///registered pipeline's associated <see cref="CircuitBreaker"/> changes state around a call to
///<see cref="ResiliencyPipelineRegistry.ExecuteAsync{TResult}"/>.
///</summary>
///<param name="PipelineName">The name the pipeline is registered under.</param>
///<param name="Previous">The circuit's state before the call.</param>
///<param name="Current">The circuit's state after the call.</param>
public sealed record CircuitStateChangedEvent(string PipelineName, CircuitState Previous, CircuitState Current);
