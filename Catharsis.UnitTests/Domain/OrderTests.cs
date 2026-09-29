using Catharsis.Domain;
using Catharsis.Events;
using Catharsis.Generics;
using Catharsis.Operators;

namespace Catharsis.UnitTests.Domain;

///<summary>
///Unit tests for <see cref="Order"/>, <see cref="OrderLine"/> and <see cref="Inventory"/>.
///</summary>
[TestClass]
public class OrderTests
{
    static Money Usd(decimal amount) => new(amount, "USD");

    static Order NewOrder() => new(Guid.NewGuid(), "usd");

    #region OrderLine

    [TestMethod]
    public void OrderLine_ComputesTotal() { Assert.AreEqual(Usd(30m), new OrderLine("A", 3, Usd(10m)).Total); }

    [TestMethod]
    public void OrderLine_SameParts_AreEqual()
    {
        Assert.AreEqual(new OrderLine("A", 2, Usd(5m)), new OrderLine("A", 2, Usd(5m)));
        Assert.AreNotEqual(new OrderLine("A", 2, Usd(5m)), new OrderLine("A", 3, Usd(5m)));
    }

    [TestMethod]
    public void OrderLine_InvalidArguments_Throw()
    {
        Assert.ThrowsExactly<ArgumentException>(static () => new OrderLine(" ", 1, Usd(1m)));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(static () => new OrderLine("A", 0, Usd(1m)));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(static () => new OrderLine("A", 1, Usd(-1m)));
    }

    #endregion

    #region Order

    [TestMethod]
    public void NewOrder_IsEmptyDraftInUpperCaseCurrency()
    {
        Order order = NewOrder();

        Assert.AreEqual(OrderStatus.Draft, order.Status);
        Assert.AreEqual("USD", order.Currency);
        Assert.AreEqual(Usd(0m), order.Total);
        Assert.AreEqual(0, order.Lines.Count);
    }

    [TestMethod]
    public void AddLine_AccumulatesTotal()
    {
        Order order = NewOrder().AddLine("A", 2, Usd(10m)).AddLine("B", 1, Usd(5.5m));

        Assert.AreEqual(Usd(25.5m), order.Total);
        Assert.AreEqual(2, order.Lines.Count);
    }

    [TestMethod]
    public void AddLine_WrongCurrency_Throws() { Assert.ThrowsExactly<InvalidOperationException>(static () => NewOrder().AddLine("A", 1, new Money(1m, "EUR"))); }

    [TestMethod]
    public void AddLine_Null_Throws() { Assert.ThrowsExactly<ArgumentNullException>(static () => NewOrder().AddLine(null!)); }

    [TestMethod]
    public void AddLine_AfterConfirm_Throws()
    {
        Order order = NewOrder().AddLine("A", 1, Usd(1m));
        order.Confirm();

        Assert.ThrowsExactly<InvalidOperationException>(() => order.AddLine("B", 1, Usd(1m)));
    }

    [TestMethod]
    public void Confirm_LocksOrderAndRaisesEvent()
    {
        Order order = NewOrder().AddLine("A", 2, Usd(10m));

        order.Confirm();

        Assert.AreEqual(OrderStatus.Confirmed, order.Status);
        OrderConfirmed raised = (OrderConfirmed)order.DomainEvents.Single();
        Assert.AreEqual(order.Id, raised.OrderId);
        Assert.AreEqual(Usd(20m), raised.Total);
    }

    [TestMethod]
    public void Confirm_EmptyOrder_Throws() { Assert.ThrowsExactly<InvalidOperationException>(static () => NewOrder().Confirm()); }

    [TestMethod]
    public void Confirm_Twice_Throws()
    {
        Order order = NewOrder().AddLine("A", 1, Usd(1m));
        order.Confirm();

        Assert.ThrowsExactly<InvalidOperationException>(order.Confirm);
    }

    [TestMethod]
    public void Cancel_Draft_RaisesEventWithoutConfirmedFlag()
    {
        Order order = NewOrder();

        order.Cancel("changed my mind");

        Assert.AreEqual(OrderStatus.Cancelled, order.Status);
        OrderCancelled raised = (OrderCancelled)order.DomainEvents.Single();
        Assert.AreEqual("changed my mind", raised.Reason);
        Assert.IsFalse(raised.WasConfirmed);
    }

    [TestMethod]
    public void Cancel_Confirmed_FlagsThatItWasConfirmed()
    {
        Order order = NewOrder().AddLine("A", 1, Usd(1m));
        order.Confirm();

        order.Cancel("late");

        Assert.IsTrue(((OrderCancelled)order.DomainEvents[^1]).WasConfirmed);
    }

    [TestMethod]
    public void Cancel_Twice_Or_BlankReason_Throws()
    {
        Order order = NewOrder();

        Assert.ThrowsExactly<ArgumentException>(() => order.Cancel(" "));
        order.Cancel("x");
        Assert.ThrowsExactly<InvalidOperationException>(() => order.Cancel("y"));
    }

    [TestMethod]
    public async Task Order_EventsReachDispatcherSubscribers()
    {
        DomainEventDispatcher dispatcher = new();
        Money? seen = null;
        dispatcher.Subscribe<OrderConfirmed>(e => { seen = e.Total; return Task.CompletedTask; });
        Order order = NewOrder().AddLine("A", 1, Usd(9m));
        order.Confirm();

        await order.DispatchEventsAsync(dispatcher);

        Assert.AreEqual(Usd(9m), seen);
    }

    #endregion

    #region Inventory

    [TestMethod]
    public void Restock_AccumulatesAndIsCaseInsensitive()
    {
        Inventory inventory = new();
        inventory.Restock("abc", 5);

        Assert.AreEqual(8, inventory.Restock("ABC", 3));
        Assert.AreEqual(8, inventory.StockOf("Abc"));
        Assert.AreEqual(0, inventory.StockOf("unknown"));
    }

    [TestMethod]
    public void Restock_InvalidArguments_Throw()
    {
        Inventory inventory = new();

        Assert.ThrowsExactly<ArgumentException>(() => inventory.Restock(" ", 1));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => inventory.Restock("A", 0));
    }

    [TestMethod]
    public void Reserve_EnoughStock_DecrementsAndReportsUnits()
    {
        Inventory inventory = new();
        inventory.Restock("A", 10);
        inventory.Restock("B", 5);
        Order order = NewOrder().AddLine("A", 4, Usd(1m)).AddLine("B", 2, Usd(1m));

        Result<int, string> result = inventory.Reserve(order);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(6, result.Value);
        Assert.AreEqual(6, inventory.StockOf("A"));
        Assert.AreEqual(3, inventory.StockOf("B"));
    }

    [TestMethod]
    public void Reserve_ShortOnAnyLine_ReservesNothing()
    {
        Inventory inventory = new();
        inventory.Restock("A", 10);
        inventory.Restock("B", 1);
        Order order = NewOrder().AddLine("A", 4, Usd(1m)).AddLine("B", 2, Usd(1m));

        Result<int, string> result = inventory.Reserve(order);

        Assert.IsTrue(result.IsFailure);
        StringAssert.Contains(result.Error, "'B'");
        Assert.AreEqual(10, inventory.StockOf("A"));
        Assert.AreEqual(1, inventory.StockOf("B"));
    }

    [TestMethod]
    public void Reserve_RepeatedSkuAcrossLines_IsSummed()
    {
        Inventory inventory = new();
        inventory.Restock("A", 5);
        Order order = NewOrder().AddLine("A", 3, Usd(1m)).AddLine("a", 3, Usd(1m));

        Assert.IsTrue(inventory.Reserve(order).IsFailure);
        Assert.AreEqual(5, inventory.StockOf("A"));
    }

    [TestMethod]
    public void Reserve_NullOrder_Throws() { Assert.ThrowsExactly<ArgumentNullException>(static () => new Inventory().Reserve(null!)); }

    [TestMethod]
    public async Task Reserve_Concurrently_NeverOversells()
    {
        Inventory inventory = new();
        inventory.Restock("A", 50);

        Result<int, string>[] results = await Task.WhenAll(Enumerable.Range(0, 200).Select(_ => Task.Run(() => inventory.Reserve(NewOrder().AddLine("A", 1, Usd(1m))))));

        Assert.AreEqual(50, results.Count(static r => r.IsSuccess));
        Assert.AreEqual(0, inventory.StockOf("A"));
    }

    #endregion
}
