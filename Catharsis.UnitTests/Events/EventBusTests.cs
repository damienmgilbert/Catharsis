using Catharsis.Events;

namespace Catharsis.UnitTests.Events;

///<summary>
///Unit tests for the <see cref="EventBus"/> class.
///</summary>
[TestClass]
public class EventBusTests
{
    #region Subscribe

    [TestMethod]
    public void Subscribe_NullHandler_Throws()
    {
        EventBus bus = new();
        Assert.ThrowsExactly<ArgumentNullException>(() => bus.Subscribe<string>(null!));
    }

    #endregion

    #region PublishAsync

    [TestMethod]
    public async Task PublishAsync_NullEvent_Throws()
    {
        EventBus bus = new();
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => bus.PublishAsync<string>(null!));
    }

    [TestMethod]
    public async Task PublishAsync_NoSubscribers_DoesNotThrow()
    {
        EventBus bus = new();
        await bus.PublishAsync("hello");
    }

    [TestMethod]
    public async Task PublishAsync_SubscribedHandler_ReceivesEvent()
    {
        EventBus bus = new();
        string? received = null;
        bus.Subscribe<string>(value => { received = value; return Task.CompletedTask; });

        await bus.PublishAsync("hello");

        Assert.AreEqual("hello", received);
    }

    [TestMethod]
    public async Task PublishAsync_MultipleSubscribers_AllReceiveEventInOrder()
    {
        EventBus bus = new();
        List<int> order = [];

        bus.Subscribe<string>(_ => { order.Add(1); return Task.CompletedTask; });
        bus.Subscribe<string>(_ => { order.Add(2); return Task.CompletedTask; });

        await bus.PublishAsync("hello");

        CollectionAssert.AreEqual(new[] { 1, 2 }, order);
    }

    [TestMethod]
    public async Task PublishAsync_DifferentEventType_DoesNotInvokeHandler()
    {
        EventBus bus = new();
        bool invoked = false;
        bus.Subscribe<string>(_ => { invoked = true; return Task.CompletedTask; });

        await bus.PublishAsync(42);

        Assert.IsFalse(invoked);
    }

    [TestMethod]
    public async Task PublishAsync_AwaitsHandlerBeforeCompleting()
    {
        EventBus bus = new();
        bool completed = false;
        bus.Subscribe<string>(async _ =>
        {
            await Task.Delay(20);
            completed = true;
        });

        await bus.PublishAsync("hello");

        Assert.IsTrue(completed);
    }

    #endregion

    #region Unsubscribe (via disposal)

    [TestMethod]
    public async Task Dispose_UnsubscribesHandler()
    {
        EventBus bus = new();
        bool invoked = false;
        IDisposable subscription = bus.Subscribe<string>(_ => { invoked = true; return Task.CompletedTask; });

        subscription.Dispose();
        await bus.PublishAsync("hello");

        Assert.IsFalse(invoked);
    }

    [TestMethod]
    public async Task Dispose_OnlyUnsubscribesThatHandler()
    {
        EventBus bus = new();
        List<string> received = [];
        IDisposable first = bus.Subscribe<string>(value => { received.Add(value); return Task.CompletedTask; });
        bus.Subscribe<string>(value => { received.Add(value); return Task.CompletedTask; });

        first.Dispose();
        await bus.PublishAsync("hello");

        CollectionAssert.AreEqual(new[] { "hello" }, received);
    }

    #endregion
}
