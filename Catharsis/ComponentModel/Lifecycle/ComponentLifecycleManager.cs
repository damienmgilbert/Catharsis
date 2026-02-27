using System.ComponentModel;

namespace Catharsis.ComponentModel.Lifecycle;

/// <summary>
/// Manages the lifecycle of components within a <see cref="ComponentGraph"/>,
/// coordinating initialization, activation, deactivation, and disposal in
/// dependency-aware topological order.
/// </summary>
/// <remarks>
/// <para>
/// Each component is associated with a <see cref="ComponentStateMachine"/>.
/// The manager walks the graph in topological order for activation and in
/// reverse topological order for deactivation and disposal, invoking
/// lifecycle callbacks on components that implement <see cref="ISupportInitialize"/>.
/// </para>
/// </remarks>
public sealed class ComponentLifecycleManager : IDisposable
{
    private readonly ComponentGraph _graph;
    private readonly Dictionary<IComponent, ComponentStateMachine> _machines = [];
    private readonly IServiceProvider? _serviceProvider;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of <see cref="ComponentLifecycleManager"/>.
    /// </summary>
    /// <param name="graph">The component dependency graph.</param>
    /// <param name="serviceProvider">An optional service provider for activation contexts.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graph"/> is <c>null</c>.
    /// </exception>
    public ComponentLifecycleManager(ComponentGraph graph, IServiceProvider? serviceProvider = null)
    {
        ArgumentNullException.ThrowIfNull(graph);
        _graph = graph;
        _serviceProvider = serviceProvider;

        foreach (var node in graph.Nodes)
        {
            var machine = new ComponentStateMachine().ConfigureDefaults();
            _machines[node.Component] = machine;
        }
    }

    /// <summary>
    /// Gets the component graph.
    /// </summary>
    public ComponentGraph Graph => _graph;

    /// <summary>
    /// Gets the state machine for the specified component.
    /// </summary>
    /// <param name="component">The component.</param>
    /// <returns>The state machine, or <c>null</c> if the component is not managed.</returns>
    public ComponentStateMachine? GetStateMachine(IComponent component)
    {
        ArgumentNullException.ThrowIfNull(component);
        return _machines.GetValueOrDefault(component);
    }

    /// <summary>
    /// Initializes all components in the graph in topological order.
    /// Components implementing <see cref="ISupportInitialize"/> have their
    /// <see cref="ISupportInitialize.BeginInit"/> and <see cref="ISupportInitialize.EndInit"/>
    /// methods called.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <exception cref="InvalidOperationException">
    /// A component's state machine does not support the transition.
    /// </exception>
    public void InitializeAll(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        var order = _graph.GetActivationOrder();

        foreach (var node in order)
        {
            cancellationToken.ThrowIfCancellationRequested();
            InitializeNode(node);
        }
    }

    /// <summary>
    /// Activates all components in the graph in topological order.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    public void ActivateAll(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        var order = _graph.GetActivationOrder();

        foreach (var node in order)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ActivateNode(node);
        }
    }

    /// <summary>
    /// Deactivates all components in the graph in reverse topological order.
    /// </summary>
    public void DeactivateAll()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        var order = _graph.GetDeactivationOrder();

        foreach (var node in order)
            DeactivateNode(node);
    }

    /// <summary>
    /// Initializes a single component.
    /// </summary>
    /// <param name="component">The component to initialize.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="component"/> is <c>null</c>.
    /// </exception>
    public void Initialize(IComponent component)
    {
        ArgumentNullException.ThrowIfNull(component);
        ObjectDisposedException.ThrowIf(_disposed, this);

        var node = _graph.GetNode(component)
            ?? throw new InvalidOperationException(
                $"Component '{component.GetType().Name}' is not in the graph.");

        InitializeNode(node);
    }

    /// <summary>
    /// Activates a single component, provided all its dependencies are active.
    /// </summary>
    /// <param name="component">The component to activate.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="component"/> is <c>null</c>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// The component's dependencies are not satisfied.
    /// </exception>
    public void Activate(IComponent component)
    {
        ArgumentNullException.ThrowIfNull(component);
        ObjectDisposedException.ThrowIf(_disposed, this);

        var node = _graph.GetNode(component)
            ?? throw new InvalidOperationException(
                $"Component '{component.GetType().Name}' is not in the graph.");

        if (!node.AreDependenciesSatisfied)
        {
            throw new InvalidOperationException(
                $"Cannot activate '{node.Name}': not all dependencies are active.");
        }

        ActivateNode(node);
    }

    /// <summary>
    /// Deactivates a single component and all its dependents recursively.
    /// </summary>
    /// <param name="component">The component to deactivate.</param>
    public void Deactivate(IComponent component)
    {
        ArgumentNullException.ThrowIfNull(component);
        ObjectDisposedException.ThrowIf(_disposed, this);

        var node = _graph.GetNode(component)
            ?? throw new InvalidOperationException(
                $"Component '{component.GetType().Name}' is not in the graph.");

        // Deactivate dependents first.
        foreach (var dependent in node.Dependents)
        {
            if (dependent.State == ComponentState.Active)
                DeactivateNode(dependent);
        }

        DeactivateNode(node);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        var order = _graph.GetDeactivationOrder();

        foreach (var node in order)
        {
            var machine = _machines[node.Component];

            if (machine.CanTransitionTo(ComponentState.Deactivating))
            {
                machine.TryTransitionTo(ComponentState.Deactivating);
                machine.TryTransitionTo(ComponentState.Deactivated);
            }

            if (machine.CanTransitionTo(ComponentState.Disposing))
            {
                machine.TryTransitionTo(ComponentState.Disposing);
                node.State = ComponentState.Disposing;

                if (node.Component is IDisposable disposable)
                    disposable.Dispose();

                machine.TryTransitionTo(ComponentState.Disposed);
                node.State = ComponentState.Disposed;
            }
        }

        _machines.Clear();
    }

    private void InitializeNode(ComponentGraphNode node)
    {
        var machine = _machines[node.Component];

        machine.TransitionTo(ComponentState.Initializing);
        node.State = ComponentState.Initializing;

        var context = CreateContext(node, ComponentState.Initialized);

        if (node.Component is ISupportInitialize initializable)
        {
            initializable.BeginInit();
            initializable.EndInit();
        }

        machine.TransitionTo(ComponentState.Initialized);
        node.State = ComponentState.Initialized;
    }

    private void ActivateNode(ComponentGraphNode node)
    {
        var machine = _machines[node.Component];

        if (machine.CurrentState == ComponentState.Created)
            InitializeNode(node);

        machine.TransitionTo(ComponentState.Activating);
        node.State = ComponentState.Activating;

        machine.TransitionTo(ComponentState.Active);
        node.State = ComponentState.Active;
    }

    private void DeactivateNode(ComponentGraphNode node)
    {
        var machine = _machines[node.Component];

        if (machine.CurrentState != ComponentState.Active)
            return;

        machine.TransitionTo(ComponentState.Deactivating);
        node.State = ComponentState.Deactivating;

        machine.TransitionTo(ComponentState.Deactivated);
        node.State = ComponentState.Deactivated;
    }

    private ComponentActivationContext CreateContext(
        ComponentGraphNode node,
        ComponentState targetState)
    {
        return new ComponentActivationContext(node.Component, _serviceProvider)
        {
            CurrentState = node.State,
            TargetState = targetState
        };
    }
}
