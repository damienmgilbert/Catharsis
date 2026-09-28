using Catharsis.ComponentModel.Lifecycle;
using Catharsis.Events;
using Catharsis.Resilience;
using Catharsis.Workflow;
using System.Collections.Concurrent;

namespace Catharsis.UnitTests.Workflow;

///<summary>
///Unit tests for the <see cref="StepWorkflowOrchestrator"/> class.
///</summary>
[TestClass]
public class StepWorkflowOrchestratorTests
{
    #region Constructor

    [TestMethod]
    public void Constructor_NullEventBus_Throws() { Assert.ThrowsExactly<ArgumentNullException>(() => new StepWorkflowOrchestrator(null!)); }

    #endregion

    #region AddStep

    [TestMethod]
    public void AddStep_NullOrWhitespaceName_Throws()
    {
        StepWorkflowOrchestrator workflow = new(new EventBus());
        Assert.ThrowsExactly<ArgumentException>(() => workflow.AddStep(" ", static _ => Task.CompletedTask));
    }

    [TestMethod]
    public void AddStep_NullAction_Throws()
    {
        StepWorkflowOrchestrator workflow = new(new EventBus());
        Assert.ThrowsExactly<ArgumentNullException>(() => workflow.AddStep("step", null!));
    }

    [TestMethod]
    public void AddStep_DuplicateName_Throws()
    {
        StepWorkflowOrchestrator workflow = new(new EventBus());
        workflow.AddStep("step", static _ => Task.CompletedTask);

        Assert.ThrowsExactly<InvalidOperationException>(() => workflow.AddStep("step", static _ => Task.CompletedTask));
    }

    [TestMethod]
    public void AddStep_UnregisteredDependency_Throws()
    {
        StepWorkflowOrchestrator workflow = new(new EventBus());
        Assert.ThrowsExactly<InvalidOperationException>(() => workflow.AddStep("b", static _ => Task.CompletedTask, dependsOn: ["a"]));
    }

    [TestMethod]
    public void AddStep_NewStep_AddsToStepNames()
    {
        StepWorkflowOrchestrator workflow = new(new EventBus());
        workflow.AddStep("step", static _ => Task.CompletedTask);

        CollectionAssert.Contains(workflow.StepNames.ToList(), "step");
    }

    #endregion

    #region RunAsync

    [TestMethod]
    public async Task RunAsync_SingleSuccessfulStep_PublishesActivatingThenActiveEvents()
    {
        EventBus eventBus = new();
        StepWorkflowOrchestrator workflow = new(eventBus);
        workflow.AddStep("step", static _ => Task.CompletedTask);

        List<WorkflowStepEvent> published = [];
        using IDisposable subscription = eventBus.Subscribe<WorkflowStepEvent>(evt =>
        {
            published.Add(evt);
            return Task.CompletedTask;
        });

        await workflow.RunAsync();

        Assert.HasCount(2, published);
        Assert.AreEqual(ComponentState.Activating, published[0].State);
        Assert.AreEqual(ComponentState.Active, published[1].State);
        Assert.IsTrue(published.All(static e => e.StepName == "step"));
    }

    [TestMethod]
    public async Task RunAsync_StepThrows_PublishesFaultedEventWithError()
    {
        EventBus eventBus = new();
        StepWorkflowOrchestrator workflow = new(eventBus);
        InvalidOperationException failure = new("boom");
        workflow.AddStep("step", _ => throw failure);

        List<WorkflowStepEvent> published = [];
        using IDisposable subscription = eventBus.Subscribe<WorkflowStepEvent>(evt =>
        {
            published.Add(evt);
            return Task.CompletedTask;
        });

        await workflow.RunAsync();

        WorkflowStepEvent faultedEvent = published.Single(static e => e.State == ComponentState.Faulted);
        Assert.AreSame(failure, faultedEvent.Error);
    }

    [TestMethod]
    public async Task RunAsync_DependentStep_RunsAfterItsDependency()
    {
        StepWorkflowOrchestrator workflow = new(new EventBus());
        List<string> executionOrder = [];

        workflow.AddStep("a", _ =>
        {
            executionOrder.Add("a");
            return Task.CompletedTask;
        });

        workflow.AddStep("b", _ =>
        {
            executionOrder.Add("b");
            return Task.CompletedTask;
        }, dependsOn: ["a"]);

        await workflow.RunAsync();

        CollectionAssert.AreEqual(new[] { "a", "b" }, executionOrder);
    }

    [TestMethod]
    public async Task RunAsync_FaultedDependency_SkipsDependentAndMarksItFaulted()
    {
        EventBus eventBus = new();
        StepWorkflowOrchestrator workflow = new(eventBus);
        bool dependentRan = false;

        workflow.AddStep("a", static _ => throw new InvalidOperationException("dependency failed"));
        workflow.AddStep("b", _ =>
        {
            dependentRan = true;
            return Task.CompletedTask;
        }, dependsOn: ["a"]);

        List<WorkflowStepEvent> published = [];
        using IDisposable subscription = eventBus.Subscribe<WorkflowStepEvent>(evt =>
        {
            published.Add(evt);
            return Task.CompletedTask;
        });

        await workflow.RunAsync();

        Assert.IsFalse(dependentRan);
        Assert.IsTrue(published.Any(static e => (e.StepName == "b") && (e.State == ComponentState.Faulted)));
    }

    [TestMethod]
    public async Task RunAsync_IndependentSteps_BothExecute()
    {
        StepWorkflowOrchestrator workflow = new(new EventBus());
        ConcurrentBag<string> executed = [];

        workflow.AddStep("a", _ =>
        {
            executed.Add("a");
            return Task.CompletedTask;
        });

        workflow.AddStep("b", _ =>
        {
            executed.Add("b");
            return Task.CompletedTask;
        });

        await workflow.RunAsync();

        CollectionAssert.AreEquivalent(new[] { "a", "b" }, executed.ToList());
    }

    [TestMethod]
    public async Task RunAsync_WithRetryPolicy_RetriesUntilSuccess()
    {
        StepWorkflowOrchestrator workflow = new(new EventBus());
        int attempts = 0;

        RetryPolicy retry = new RetryPolicy().MaxAttempts(3).InitialDelay(TimeSpan.FromMilliseconds(1));

        workflow.AddStep("flaky", _ =>
        {
            attempts++;

            if(attempts < 2)
            {
                throw new InvalidOperationException("transient failure");
            }

            return Task.CompletedTask;
        }, retryPolicy: retry);

        await workflow.RunAsync();

        Assert.AreEqual(2, attempts);
    }

    #endregion
}
