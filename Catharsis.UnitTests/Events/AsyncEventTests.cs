using Catharsis.Events;

namespace Catharsis.UnitTests.Events;

///<summary>
///Unit tests for the <see cref="AsyncEvent{TArgs}"/> class.
///</summary>
[TestClass]
public class AsyncEventTests
{
    #region Subscribe / InvokeAsync

    [TestMethod]
    public void Subscribe_NullHandler_Throws() { Assert.ThrowsExactly<ArgumentNullException>(static () => new AsyncEvent<int>().Subscribe(null!)); }

    [TestMethod]
    public async Task InvokeAsync_NoHandlers_Completes()
    {
        await new AsyncEvent<int>().InvokeAsync(null, 1);
    }

    [TestMethod]
    public async Task InvokeAsync_Sequential_RunsInSubscriptionOrder()
    {
        AsyncEvent<int> evt = new();
        List<string> log = [];

        evt.Subscribe(async (_, value, _) => { await Task.Delay(20); log.Add($"a{value}"); });
        evt.Subscribe((_, value, _) => { log.Add($"b{value}"); return Task.CompletedTask; });

        await evt.InvokeAsync(null, 7);

        CollectionAssert.AreEqual(new[] { "a7", "b7" }, log);
    }

    [TestMethod]
    public async Task InvokeAsync_Sequential_StopsAtFirstThrowingHandler()
    {
        AsyncEvent<int> evt = new();
        bool secondRan = false;

        evt.Subscribe((_, _, _) => throw new InvalidOperationException());
        evt.Subscribe((_, _, _) => { secondRan = true; return Task.CompletedTask; });

        await Assert.ThrowsExactlyAsync<InvalidOperationException>(async () => await evt.InvokeAsync(null, 1));
        Assert.IsFalse(secondRan);
    }

    [TestMethod]
    public async Task InvokeAsync_Parallel_StartsEveryHandlerBeforeAnyCompletes()
    {
        AsyncEvent<int> evt = new();
        TaskCompletionSource gate = new(TaskCreationOptions.RunContinuationsAsynchronously);
        int started = 0;

        for (int i = 0; i < 3; i++)
        {
            evt.Subscribe(async (_, _, _) => { Interlocked.Increment(ref started); await gate.Task; });
        }

        Task run = evt.InvokeAsync(null, 1, parallel: true);
        Assert.AreEqual(3, started);
        gate.SetResult();
        await run;
    }

    [TestMethod]
    public async Task InvokeAsync_Cancelled_ThrowsBetweenSequentialHandlers()
    {
        AsyncEvent<int> evt = new();
        using CancellationTokenSource cts = new();

        evt.Subscribe((_, _, _) => { cts.Cancel(); return Task.CompletedTask; });
        evt.Subscribe((_, _, _) => Task.CompletedTask);

        await Assert.ThrowsExactlyAsync<OperationCanceledException>(async () => await evt.InvokeAsync(null, 1, cancellationToken: cts.Token));
    }

    [TestMethod]
    public async Task InvokeAsync_PassesSenderAndArgs()
    {
        AsyncEvent<string> evt = new();
        object sender = new();
        object? seenSender = null;
        string? seenArgs = null;

        evt.Subscribe((s, a, _) => { seenSender = s; seenArgs = a; return Task.CompletedTask; });
        await evt.InvokeAsync(sender, "hi");

        Assert.AreSame(sender, seenSender);
        Assert.AreEqual("hi", seenArgs);
    }

    [TestMethod]
    public async Task InvokeValueAsync_RunsHandlers()
    {
        AsyncEvent<int> evt = new();
        int total = 0;

        evt.Subscribe((_, v, _) => { total += v; return Task.CompletedTask; });
        await evt.InvokeValueAsync(null, 4);

        Assert.AreEqual(4, total);
    }

    #endregion

    #region Unsubscribe

    [TestMethod]
    public async Task Dispose_RemovesHandlerAndIsIdempotent()
    {
        AsyncEvent<int> evt = new();
        int calls = 0;

        IDisposable subscription = evt.Subscribe((_, _, _) => { calls++; return Task.CompletedTask; });
        await evt.InvokeAsync(null, 1);
        subscription.Dispose();
        subscription.Dispose();
        await evt.InvokeAsync(null, 1);

        Assert.AreEqual(1, calls);
        Assert.AreEqual(0, evt.Count);
    }

    #endregion

    #region Operators

    [TestMethod]
    public async Task PlusMinusOperators_AddAndRemoveHandler()
    {
        AsyncEvent<int> evt = new();
        int calls = 0;
        AsyncEventHandler<int> handler = (_, _, _) => { calls++; return Task.CompletedTask; };

        evt += handler;
        await evt.InvokeAsync(null, 1);
        evt -= handler;
        await evt.InvokeAsync(null, 1);

        Assert.AreEqual(1, calls);
    }

    [TestMethod]
    public void PlusOperator_NullHandler_Throws()
    {
        AsyncEvent<int> evt = new();
        Assert.ThrowsExactly<ArgumentNullException>(() => _ = evt + null!);
    }

    #endregion

    #region Filter

    [TestMethod]
    public async Task Subscribe_WithFilter_OnlyDeliversAcceptedEvents()
    {
        AsyncEvent<int> evt = new();
        List<int> seen = [];

        evt.Subscribe((_, v, _) => { seen.Add(v); return Task.CompletedTask; }, static v => v % 2 == 0);
        await evt.InvokeAsync(null, 1);
        await evt.InvokeAsync(null, 2);

        CollectionAssert.AreEqual(new[] { 2 }, seen);
    }

    [TestMethod]
    public void Subscribe_WithFilter_DisposeRemovesFilteredHandler()
    {
        AsyncEvent<int> evt = new();
        IDisposable subscription = evt.Subscribe((_, _, _) => Task.CompletedTask, static _ => true);

        subscription.Dispose();

        Assert.AreEqual(0, evt.Count);
    }

    #endregion
}
