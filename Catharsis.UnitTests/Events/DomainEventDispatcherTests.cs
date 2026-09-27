using Catharsis.Events;

namespace Catharsis.UnitTests.Events;

///<summary>
///Unit tests for the <see cref="DomainEventDispatcher"/> class.
///</summary>
[TestClass]
public class DomainEventDispatcherTests
{
    #region Subscribe

    [TestMethod]
    public void Subscribe_NullHandler_Throws()
    {
        DomainEventDispatcher dispatcher = new();
        Assert.ThrowsExactly<ArgumentNullException>(() => dispatcher.Subscribe<OrderPlaced>(null!));
    }

    #endregion

    #region DispatchAsync

    [TestMethod]
    public async Task DispatchAsync_NullEvent_Throws()
    {
        DomainEventDispatcher dispatcher = new();
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => dispatcher.DispatchAsync(null!));
    }

    [TestMethod]
    public async Task DispatchAsync_NoSubscribers_DoesNotThrow()
    {
        DomainEventDispatcher dispatcher = new();
        await dispatcher.DispatchAsync(new OrderPlaced("order-1"));
    }

    [TestMethod]
    public async Task DispatchAsync_SubscribedHandler_ReceivesEvent()
    {
        DomainEventDispatcher dispatcher = new();
        string? receivedOrderId = null;
        dispatcher.Subscribe<OrderPlaced>(e => { receivedOrderId = e.OrderId; return Task.CompletedTask; });

        await dispatcher.DispatchAsync(new OrderPlaced("order-1"));

        Assert.AreEqual("order-1", receivedOrderId);
    }

    [TestMethod]
    public async Task DispatchAsync_DifferentEventType_DoesNotInvokeHandler()
    {
        DomainEventDispatcher dispatcher = new();
        bool invoked = false;
        dispatcher.Subscribe<OrderPlaced>(_ => { invoked = true; return Task.CompletedTask; });

        await dispatcher.DispatchAsync(new OrderCancelled("order-1"));

        Assert.IsFalse(invoked);
    }

    [TestMethod]
    public async Task DispatchAsync_MultipleSubscribers_AllReceiveEvent()
    {
        DomainEventDispatcher dispatcher = new();
        List<string> receivedOrderIds = [];

        dispatcher.Subscribe<OrderPlaced>(e => { receivedOrderIds.Add(e.OrderId); return Task.CompletedTask; });
        dispatcher.Subscribe<OrderPlaced>(e => { receivedOrderIds.Add(e.OrderId); return Task.CompletedTask; });

        await dispatcher.DispatchAsync(new OrderPlaced("order-1"));

        CollectionAssert.AreEqual(new[] { "order-1", "order-1" }, receivedOrderIds);
    }

    #endregion

    #region DomainEvent

    [TestMethod]
    public void OccurredAt_DefaultsToNow()
    {
        DateTimeOffset before = DateTimeOffset.UtcNow;
        OrderPlaced placed = new("order-1");
        DateTimeOffset after = DateTimeOffset.UtcNow;

        Assert.IsTrue((placed.OccurredAt >= before) && (placed.OccurredAt <= after));
    }

    #endregion

    private sealed record OrderPlaced(string OrderId) : DomainEvent;

    private sealed record OrderCancelled(string OrderId) : DomainEvent;
}
