using Catharsis.Caching;
using Catharsis.Events;
using Catharsis.Resilience;
using System.Collections.Concurrent;

namespace Catharsis.Reliability;

///<summary>
///A name-keyed registry of <see cref="IAsyncPolicy"/> pipelines (typically a caller-built <see cref="PolicyWrap"/>),
///such as one tuned differently for "the payments API call" versus "the inventory API call" without re-wiring call
///sites. Every execution's last successful result is cached; if a named pipeline's policies are ultimately
///exhausted, <see cref="ExecuteAsync{TResult}"/> serves that stale-but-last-known-good result instead of throwing,
///when one is available. If a <see cref="CircuitBreaker"/> is supplied alongside a pipeline, its state transitions
///are published as <see cref="CircuitStateChangedEvent"/> notifications on an <see cref="EventBus"/>.
///</summary>
public sealed class ResiliencyPipelineRegistry
{
    #region Fields
    readonly ConcurrentDictionary<string, PipelineRegistration> _pipelines = new(StringComparer.Ordinal);
    readonly TtlCache<string, object?> _lastKnownGood;
    readonly EventBus _eventBus;
    #endregion

    #region Constructors
    ///<summary>
    ///Creates an empty registry.
    ///</summary>
    ///<param name="eventBus">The event bus that <see cref="CircuitStateChangedEvent"/> notifications are published to.</param>
    ///<param name="staleValueLifetime">
    ///How long a successful result is retained as a fallback for a subsequent total failure. Defaults to 5 minutes.
    ///</param>
    ///<exception cref="ArgumentNullException"><paramref name="eventBus"/> is <c>null</c>.</exception>
    ///<exception cref="ArgumentOutOfRangeException"><paramref name="staleValueLifetime"/> is not greater than zero.</exception>
    public ResiliencyPipelineRegistry(EventBus eventBus, TimeSpan? staleValueLifetime = null)
    {
        ArgumentNullException.ThrowIfNull(eventBus);

        _eventBus = eventBus;
        _lastKnownGood = new TtlCache<string, object?>(staleValueLifetime ?? TimeSpan.FromMinutes(5));
    }
    #endregion

    #region Private methods
    async Task PublishIfChangedAsync(string name, CircuitBreaker? circuitBreaker, CircuitState? previousState, CancellationToken cancellationToken)
    {
        if((circuitBreaker is null) || (previousState is null))
        {
            return;
        }

        CircuitState currentState = circuitBreaker.State;

        if(currentState != previousState.Value)
        {
            await _eventBus.PublishAsync(new CircuitStateChangedEvent(name, previousState.Value, currentState), cancellationToken).ConfigureAwait(false);
        }
    }
    #endregion

    #region Public methods
    ///<summary>
    ///Executes <paramref name="operation"/> through the pipeline registered under <paramref name="name"/>. On
    ///success, the result is cached as that pipeline's last known good value. On total failure (every policy in the
    ///pipeline exhausted), the last known good value is returned instead, if one is still cached; otherwise the
    ///original exception propagates.
    ///</summary>
    ///<typeparam name="TResult">The return type of the operation.</typeparam>
    ///<param name="name">The name the pipeline was registered under.</param>
    ///<param name="operation">The async operation to execute. Receives a <see cref="CancellationToken"/>.</param>
    ///<param name="cancellationToken">A cancellation token observed by the pipeline and passed to the operation.</param>
    ///<returns>The operation's result, or a cached fallback result if every policy in the pipeline failed.</returns>
    ///<exception cref="ArgumentException"><paramref name="name"/> is <c>null</c>, empty, or whitespace.</exception>
    ///<exception cref="ArgumentNullException"><paramref name="operation"/> is <c>null</c>.</exception>
    ///<exception cref="InvalidOperationException">No pipeline is registered under <paramref name="name"/>.</exception>
    public async Task<TResult> ExecuteAsync<TResult>(string name, Func<CancellationToken, Task<TResult>> operation, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(operation);

        if(!_pipelines.TryGetValue(name, out PipelineRegistration? registration))
        {
            throw new InvalidOperationException($"No resiliency pipeline is registered under the name '{name}'.");
        }

        CircuitState? previousState = registration.CircuitBreaker?.State;

        try
        {
            TResult result = await registration.Pipeline.ExecuteAsync(operation, cancellationToken).ConfigureAwait(false);
            _lastKnownGood.Set(name, result);
            await PublishIfChangedAsync(name, registration.CircuitBreaker, previousState, cancellationToken).ConfigureAwait(false);
            return result;
        } catch(Exception ex) when(ex is not OperationCanceledException)
        {
            await PublishIfChangedAsync(name, registration.CircuitBreaker, previousState, cancellationToken).ConfigureAwait(false);

            if(_lastKnownGood.TryGetValue(name, out object? cached) && (cached is TResult staleResult))
            {
                return staleResult;
            }

            throw;
        }
    }

    ///<summary>
    ///Registers a pipeline under the specified name, overwriting any pipeline already registered under that name.
    ///</summary>
    ///<param name="name">The name to register the pipeline under.</param>
    ///<param name="pipeline">The policy pipeline to execute operations through.</param>
    ///<param name="circuitBreaker">
    ///An optional circuit breaker participating in <paramref name="pipeline"/>, whose state transitions are
    ///published as <see cref="CircuitStateChangedEvent"/> notifications. Pass <c>null</c> if the pipeline has none,
    ///or if its state changes should not be published.
    ///</param>
    ///<returns>This registry, for fluent chaining.</returns>
    ///<exception cref="ArgumentException"><paramref name="name"/> is <c>null</c>, empty, or whitespace.</exception>
    ///<exception cref="ArgumentNullException"><paramref name="pipeline"/> is <c>null</c>.</exception>
    public ResiliencyPipelineRegistry Register(string name, IAsyncPolicy pipeline, CircuitBreaker? circuitBreaker = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(pipeline);

        _pipelines[name] = new PipelineRegistration(pipeline, circuitBreaker);
        return this;
    }

    ///<summary>
    ///Removes the pipeline registered under the specified name, if any.
    ///</summary>
    ///<param name="name">The name of the pipeline to remove.</param>
    ///<returns><c>true</c> if a pipeline was removed; otherwise <c>false</c>.</returns>
    ///<exception cref="ArgumentException"><paramref name="name"/> is <c>null</c>, empty, or whitespace.</exception>
    public bool Unregister(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return _pipelines.TryRemove(name, out _);
    }
    #endregion

    #region Public properties
    ///<summary>
    ///The names of every currently registered pipeline.
    ///</summary>
    public IReadOnlyList<string> RegisteredPipelineNames => [.. _pipelines.Keys];
    #endregion

    sealed record PipelineRegistration(IAsyncPolicy Pipeline, CircuitBreaker? CircuitBreaker);
}
