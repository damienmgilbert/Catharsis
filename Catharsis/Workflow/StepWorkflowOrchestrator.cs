using System.Collections.Concurrent;
using Catharsis.ComponentModel;
using Catharsis.ComponentModel.Lifecycle;
using Catharsis.Events;
using Catharsis.Resilience;

namespace Catharsis.Workflow;

///<summary>
///Executes a set of named async steps in dependency order, reusing <see cref="ComponentGraph"/>/ ///<see
///cref="ComponentGraphBuilder"/> for the dependency graph and topological ordering rather than implementing a new one.
///Steps with no dependency relationship between them run concurrently, grouped into successive waves by dependency
///depth. Each step's lifecycle is tracked through its <see cref="ComponentGraphNode.State"/> (<see
///cref="ComponentState.Activating"/> → <see cref="ComponentState.Active"/> or ///<see cref="ComponentState.Faulted"/>),
///optionally retried through a caller-supplied <see cref="IAsyncPolicy"/>, and published as a <see
///cref="WorkflowStepEvent"/> on an <see cref="EventBus"/>. A faulted step fails only the steps that (transitively)
///depend on it; unrelated branches still run.
///</summary>
public sealed class StepWorkflowOrchestrator
{
    #region Fields
    private readonly ComponentGraphBuilder _builder = new();
    private readonly EventBus _eventBus;
    private readonly Dictionary<string, WorkflowStepComponent> _steps = new(StringComparer.Ordinal);
    #endregion

    #region Constructors
    ///<summary>
    ///Creates an empty workflow.
    ///</summary>
    ///<param name="eventBus">The event bus that <see cref="WorkflowStepEvent"/> notifications are published to.</param>
    ///<exception cref="ArgumentNullException"><paramref name="eventBus"/> is <c>null</c>.</exception>
    public StepWorkflowOrchestrator(EventBus eventBus)
    {
        ArgumentNullException.ThrowIfNull(eventBus);
        _eventBus = eventBus;
    }
    #endregion

    #region Private methods
    private static int ComputeWave(ComponentGraphNode node, IReadOnlyDictionary<ComponentGraphNode, int> waveByNode) { return (node.Dependencies.Count == 0) ? 0 : (node.Dependencies.Max(dependency => waveByNode[dependency]) + 1); }

    private async Task ExecuteNodeAsync(ComponentGraphNode node, ConcurrentDictionary<ComponentGraphNode, bool> faulted, CancellationToken cancellationToken)
    {
        WorkflowStepComponent step = (WorkflowStepComponent)node.Component;

        if(node.Dependencies.Any(dependency => faulted.ContainsKey(dependency)))
        {
            node.State = ComponentState.Faulted;
            faulted[node] = true;
            await _eventBus.PublishAsync(new WorkflowStepEvent(step.Name, ComponentState.Faulted, new InvalidOperationException($"Step '{step.Name}' was skipped because a dependency failed."), DateTimeOffset.UtcNow), cancellationToken).ConfigureAwait(false);
            return;
        }

        node.State = ComponentState.Activating;
        await _eventBus.PublishAsync(new WorkflowStepEvent(step.Name, ComponentState.Activating, null, DateTimeOffset.UtcNow), cancellationToken).ConfigureAwait(false);

        try
        {
            if(step.RetryPolicy is not null)
            {
                await step.RetryPolicy
                    .ExecuteAsync(
                      async ct =>
                      {
                          await step.Action(ct).ConfigureAwait(false);
                          return true;
                      },
                      cancellationToken)
                    .ConfigureAwait(false);
            } else
            {
                await step.Action(cancellationToken).ConfigureAwait(false);
            }

            node.State = ComponentState.Active;
            await _eventBus.PublishAsync(new WorkflowStepEvent(step.Name, ComponentState.Active, null, DateTimeOffset.UtcNow), cancellationToken).ConfigureAwait(false);
        } catch(Exception ex) when(ex is not OperationCanceledException)
        {
            node.State = ComponentState.Faulted;
            faulted[node] = true;
            await _eventBus.PublishAsync(new WorkflowStepEvent(step.Name, ComponentState.Faulted, UnwrapSingleFailure(ex), DateTimeOffset.UtcNow), cancellationToken).ConfigureAwait(false);
        }
    }

    ///<summary>
    ///Unwraps an <see cref="AggregateException"/> (as thrown by <see cref="RetryPolicy"/> on exhaustion) down to its
    ///single inner exception, when there is exactly one distinct cause. <see cref="RetryPolicy"/> records one entry per
    ///attempt, so a persistent failure that throws the same exception instance on every attempt still produces one
    ///entry per attempt; comparing by reference (rather than by count) correctly collapses that common case. A
    ///genuinely multi-cause aggregate is returned as-is, since collapsing it to one exception would lose information.
    ///</summary>
    private static Exception UnwrapSingleFailure(Exception exception)
    {
        if(exception is AggregateException aggregate)
        {
            Exception[] distinctCauses = [ .. aggregate.Flatten().InnerExceptions.Distinct() ];

            if(distinctCauses.Length == 1)
            {
                return distinctCauses[0];
            }
        }

        return exception;
    }
    #endregion

    #region Public methods
    ///<summary>
    ///Registers a step. Dependencies must already be registered before a step that depends on them.
    ///</summary>
    ///<param name="name">A unique name identifying this step.</param>
    ///<param name="action">The async action to execute. Receives a <see cref="CancellationToken"/>.</param>
    ///<param name="retryPolicy">An optional policy the step is executed through, e.g. a <see cref="RetryPolicy"/>. <c>null</c> means no retry.</param>
    ///<param name="dependsOn">The names of steps that must complete successfully before this step runs.</param>
    ///<returns>This orchestrator, for fluent chaining.</returns>
    ///<exception cref="ArgumentException"><paramref name="name"/> is <c>null</c>, empty, or whitespace.</exception>
    ///<exception cref="ArgumentNullException"><paramref name="action"/> or <paramref name="dependsOn"/> is <c>null</c>.</exception>
    ///<exception cref="InvalidOperationException">
    ///A step named <paramref name="name"/> is already registered, or <paramref name="dependsOn"/> names a step that is
    ///not yet registered.
    ///</exception>
    public StepWorkflowOrchestrator AddStep(string name, Func<CancellationToken, Task> action, IAsyncPolicy? retryPolicy = null, params string[] dependsOn)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(action);
        ArgumentNullException.ThrowIfNull(dependsOn);

        if(_steps.ContainsKey(name))
        {
            throw new InvalidOperationException($"A step named '{name}' is already registered.");
        }

        // Resolve every dependency before registering anything, so a failed lookup leaves the orchestrator
        // completely untouched instead of registering the step with only some of its dependency edges.
        WorkflowStepComponent[] dependencySteps = new WorkflowStepComponent[dependsOn.Length];

        for(int index = 0; index < dependsOn.Length; index++)
        {
            if(!_steps.TryGetValue(dependsOn[index], out WorkflowStepComponent? dependencyStep))
            {
                throw new InvalidOperationException($"Step '{name}' depends on '{dependsOn[index]}', which is not registered. Register dependencies before the steps that depend on them.");
            }

            dependencySteps[index] = dependencyStep;
        }

        WorkflowStepComponent step = new(name, action, retryPolicy);
        _steps.Add(name, step);
        _builder.AddComponent(step, name);

        foreach(WorkflowStepComponent dependencyStep in dependencySteps)
        {
            _builder.AddDependency(step, dependencyStep);
        }

        return this;
    }

    ///<summary>
    ///Runs every registered step in dependency order, waiting for each wave of independent steps to finish before
    ///starting the next.
    ///</summary>
    ///<param name="cancellationToken">A token observed between waves and passed to every step.</param>
    ///<exception cref="InvalidOperationException">The registered steps' dependencies form a cycle.</exception>
    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        ComponentGraph graph = _builder.Build();
        IReadOnlyList<ComponentGraphNode> order = graph.GetActivationOrder();

        foreach(ComponentGraphNode node in order)
        {
            node.State = ComponentState.Created;
        }

        Dictionary<ComponentGraphNode, int> waveByNode = new();

        foreach(ComponentGraphNode node in order)
        {
            waveByNode[node] = ComputeWave(node, waveByNode);
        }

        ConcurrentDictionary<ComponentGraphNode, bool> faulted = new();

        foreach(IGrouping<int, ComponentGraphNode> wave in order.GroupBy(node => waveByNode[node]).OrderBy(static group => group.Key))
        {
            cancellationToken.ThrowIfCancellationRequested();
            await Task.WhenAll(wave.Select(node => ExecuteNodeAsync(node, faulted, cancellationToken))).ConfigureAwait(false);
        }
    }
    #endregion

    #region Public properties
    ///<summary>
    ///The names of every currently registered step.
    ///</summary>
    public IReadOnlyList<string> StepNames => [ .. _steps.Keys ];
    #endregion

    ///<summary>
    ///Wraps a step's name, action, and optional retry policy as an <see cref="ComponentModel.ComponentBase"/> so it can
    ///be registered as a node in a <see cref="ComponentGraph"/>.
    ///</summary>
    private sealed class WorkflowStepComponent(string name, Func<CancellationToken, Task> action, IAsyncPolicy? retryPolicy) : ComponentBase
    {
        #region Public properties
        public Func<CancellationToken, Task> Action { get; } = action;

        public string Name { get; } = name;

        public IAsyncPolicy? RetryPolicy { get; } = retryPolicy;
        #endregion
    }
}
