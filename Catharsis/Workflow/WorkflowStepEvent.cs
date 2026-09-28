using Catharsis.ComponentModel.Lifecycle;

namespace Catharsis.Workflow;

///<summary>
///Published on the <see cref="Catharsis.Events.EventBus"/> by <see cref="StepWorkflowOrchestrator"/> whenever a
///step's lifecycle state changes during a run.
///</summary>
///<param name="StepName">The name of the step whose state changed.</param>
///<param name="State">
///The step's new state: <see cref="ComponentState.Activating"/> when the step starts, <see cref="ComponentState.Active"/>
///when it completes successfully, or <see cref="ComponentState.Faulted"/> if it (or one of its dependencies) failed.
///</param>
///<param name="Error">The exception that faulted the step, or the dependency failure that caused it to be skipped. <c>null</c> otherwise.</param>
///<param name="AtUtc">The UTC timestamp at which the state change occurred.</param>
public sealed record WorkflowStepEvent(string StepName, ComponentState State, Exception? Error, DateTimeOffset AtUtc);
