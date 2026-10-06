using System.Collections.Concurrent;
using Catharsis.Caching;
using Catharsis.Events;
using Catharsis.Resilience;

namespace Catharsis.Reliability;

///<summary>
///A name-keyed registry of <see cref="IAsyncPolicy"/> pipelines (typically a caller-built <see cref="PolicyWrap"/>),
///such as one tuned differently for "the payments API call" versus "the inventory API call" without re-wiring call
///sites. Every execution's last successful result is cached — including a legitimately <c>null</c> result — so that if
///a named pipeline's policies are ultimately exhausted, <see cref="ExecuteAsync{TResult}"/> serves that stale-but-last-
///known-good result instead of throwing, when one is available. If a <see cref="CircuitBreaker"/> is supplied alongside
///a pipeline, its state transitions are published as <see cref="CircuitStateChangedEvent"/> notifications on an <see
///cref="EventBus"/> — including when the call that observed the transition was itself canceled. Publishing is
///deduplicated per pipeline so concurrent calls that observe the same transition publish it only once. This does not,
///however, prevent a <see cref="CircuitBreaker"/> whose trial (half-open) call is itself canceled from remaining stuck
///at <see cref="CircuitState.HalfOpen"/>: that is a limitation of ///<see cref="CircuitBreaker"/> itself, which only
///resolves half-open back to closed or open from within a non-canceled <c>Execute</c>/<c>ExecuteAsync</c> call.
///</summary>
public sealed class ResiliencyPipelineRegistry
{
    #region Fields
    private readonly EventBus _eventBus;
    private readonly TtlCache<string, object?> _lastKnownGood;
    private readonly ConcurrentDictionary<string, PipelineRegistration> _pipelines = new(StringComparer.Ordinal);
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
    ///<summary>
    ///Compares a pipeline's circuit breaker state against the last state published for it and, if it changed, publishes
    ///exactly one <see cref="CircuitStateChangedEvent"/> — even under concurrent callers racing to observe the same
    ///transition, since the compare-and-swap against <see cref="PipelineRegistration.LastPublishedState"/> is
    ///serialized per registration.
    ///</summary>
    private async Task PublishIfChangedAsync(string name, PipelineRegistration registration, CancellationToken cancellationToken)
    {
        if(registration.CircuitBreaker is null)
        {
            return;
        }

        CircuitState currentState = registration.CircuitBreaker.State;
        CircuitState previousState;

        lock(registration.Gate)
        {
            previousState = registration.LastPublishedState;

            if(currentState == previousState)
            {
                return;
            }

            registration.LastPublishedState = currentState;
        }

        await _eventBus.PublishAsync(new CircuitStateChangedEvent(name, previousState, currentState), cancellationToken).ConfigureAwait(false);
    }

    ///<summary>
    ///Attempts to interpret a cached raw value as a <typeparamref name="TResult"/>, correctly distinguishing "no value
    ///cached" from "a legitimately cached <c>null</c>" — the latter can't be detected via <c>is TResult</c> alone,
    ///since that pattern never matches a <c>null</c> operand even when <typeparamref name="TResult"/> is a reference
    ///type.
    ///</summary>
    private static bool TryCastCached<TResult>(object? cached, out TResult result)
    {
        if(cached is TResult typed)
        {
            result = typed;
            return true;
        }

        if((cached is null) && (default(TResult) is null))
        {
            result = default!;
            return true;
        }

        result = default!;
        return false;
    }
    #endregion

    #region Public methods
    ///<summary>
    ///Executes <paramref name="operation"/> through the pipeline registered under <paramref name="name"/>. On success,
    ///the result — including <c>null</c> — is cached as that pipeline's last known good value. On total failure (every
    ///policy in the pipeline exhausted), the last known good value is returned instead, if one is still cached;
    ///otherwise the exception thrown by the pipeline propagates. Any resulting circuit-breaker state transition is
    ///published (see <see cref="PublishIfChangedAsync"/>) regardless of whether the call that observed it succeeded,
    ///failed, or was canceled.
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

        try
        {
            TResult result = await registration.Pipeline.ExecuteAsync(operation, cancellationToken).ConfigureAwait(false);
            _lastKnownGood.Set(name, result);
            return result;
        } catch(Exception ex) when(ex is not OperationCanceledException)
        {
            if(_lastKnownGood.TryGetValue(name, out object? cached) && TryCastCached(cached, out TResult staleResult))
            {
                return staleResult;
            }

            throw;
        } finally
        {
            // Uses CancellationToken.None: the transition already happened (or didn't) regardless of whether
            // cancellationToken itself was signaled, and publishing must not be defeated by that same cancellation.
            await PublishIfChangedAsync(name, registration, CancellationToken.None).ConfigureAwait(false);
        }
    }

    ///<summary>
    ///Registers a pipeline under the specified name, overwriting any pipeline already registered under that name.
    ///</summary>
    ///<param name="name">The name to register the pipeline under.</param>
    ///<param name="pipeline">The policy pipeline to execute operations through.</param>
    ///<param name="circuitBreaker">
    ///An optional circuit breaker participating in <paramref name="pipeline"/>, whose state transitions are published
    ///as <see cref="CircuitStateChangedEvent"/> notifications. Pass <c>null</c> if the pipeline has none, or if its
    ///state changes should not be published.
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
    public IReadOnlyList<string> RegisteredPipelineNames => [ .. _pipelines.Keys ];
    #endregion

    ///<summary>
    ///Tracks a registered pipeline's policy, optional circuit breaker, and the last circuit state published for it (so
    ///concurrent callers can deduplicate publishing a single real transition via <see cref="Gate"/>).
    ///</summary>
    private sealed class PipelineRegistration(IAsyncPolicy pipeline, CircuitBreaker? circuitBreaker)
    {
        #region Public properties
        public CircuitBreaker? CircuitBreaker { get; } = circuitBreaker;

        public Lock Gate { get; } = new();

        public CircuitState LastPublishedState { get; set; } = circuitBreaker?.State ?? default;

        public IAsyncPolicy Pipeline { get; } = pipeline;
        #endregion
    }
}
