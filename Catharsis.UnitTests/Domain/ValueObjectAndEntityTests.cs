using Catharsis.Domain;
using Catharsis.Events;

namespace Catharsis.UnitTests.Domain;

///<summary>
///Unit tests for <see cref="ValueObject"/>, <see cref="Entity{TId}"/> and <see cref="AggregateRoot{TId}"/>.
///</summary>
[TestClass]
public class ValueObjectAndEntityTests
{
    sealed class Point(int x, int y) : ValueObject
    {
        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return x;
            yield return y;
        }
    }

    sealed class OtherPoint(int x, int y) : ValueObject
    {
        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return x;
            yield return y;
        }
    }

    sealed class Nullable(string? name) : ValueObject
    {
        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return name;
        }
    }

    class Customer(int id) : Entity<int>(id)
    {
        public string Name { get; set; } = "";
    }

    sealed class VipCustomer(int id) : Customer(id) { }

    sealed record Something(string What) : DomainEvent;

    sealed class Basket(int id) : AggregateRoot<int>(id)
    {
        public void Do(string what) => Raise(new Something(what));
    }

    #region ValueObject

    [TestMethod]
    public void ValueObject_SameComponents_AreEqualWithSameHash()
    {
        Assert.AreEqual(new Point(1, 2), new Point(1, 2));
        Assert.IsTrue(new Point(1, 2) == new Point(1, 2));
        Assert.AreEqual(new Point(1, 2).GetHashCode(), new Point(1, 2).GetHashCode());
    }

    [TestMethod]
    public void ValueObject_DifferentComponents_AreNotEqual()
    {
        Assert.AreNotEqual(new Point(1, 2), new Point(2, 1));
        Assert.IsTrue(new Point(1, 2) != new Point(1, 3));
    }

    [TestMethod]
    public void ValueObject_DifferentTypesSameComponents_AreNotEqual() { Assert.IsFalse(new Point(1, 2).Equals(new OtherPoint(1, 2))); }

    [TestMethod]
    public void ValueObject_NullComponents_AreHandled()
    {
        Assert.AreEqual(new Nullable(null), new Nullable(null));
        Assert.AreNotEqual(new Nullable(null), new Nullable("x"));
    }

    [TestMethod]
    public void ValueObject_NullComparison_IsSafe()
    {
        Point? none = null;
        Assert.IsTrue(none == null);
        Assert.IsTrue(new Point(1, 1) != null);
        Assert.IsFalse(new Point(1, 1).Equals(null));
    }

    #endregion

    #region Entity

    [TestMethod]
    public void Entity_SameIdSameType_AreEqualEvenIfStateDiffers()
    {
        Assert.AreEqual(new Customer(1) { Name = "a" }, new Customer(1) { Name = "b" });
        Assert.IsTrue(new Customer(1) == new Customer(1));
    }

    [TestMethod]
    public void Entity_DifferentId_AreNotEqual() { Assert.IsTrue(new Customer(1) != new Customer(2)); }

    [TestMethod]
    public void Entity_SameIdDifferentType_AreNotEqual() { Assert.AreNotEqual<Customer>(new Customer(1), new VipCustomer(1)); }

    [TestMethod]
    public void Entity_NullId_Throws() { Assert.ThrowsExactly<ArgumentNullException>(static () => new StringIdEntity(null!)); }

    sealed class StringIdEntity(string id) : Entity<string>(id) { }

    [TestMethod]
    public void Entity_UsableAsDictionaryKey()
    {
        Dictionary<Customer, string> map = new() { [new Customer(7)] = "seven" };

        Assert.AreEqual("seven", map[new Customer(7)]);
    }

    #endregion

    #region AggregateRoot

    [TestMethod]
    public void AggregateRoot_RaiseRecordsEventsInOrder()
    {
        Basket basket = new(1);
        basket.Do("a");
        basket.Do("b");

        CollectionAssert.AreEqual(new[] { "a", "b" }, basket.DomainEvents.Cast<Something>().Select(static e => e.What).ToArray());
    }

    [TestMethod]
    public async Task AggregateRoot_Dispatch_DeliversAndClears()
    {
        DomainEventDispatcher dispatcher = new();
        List<string> seen = [];
        dispatcher.Subscribe<Something>(e => { seen.Add(e.What); return Task.CompletedTask; });
        Basket basket = new(1);
        basket.Do("a");
        basket.Do("b");

        await basket.DispatchEventsAsync(dispatcher);

        CollectionAssert.AreEqual(new[] { "a", "b" }, seen);
        Assert.AreEqual(0, basket.DomainEvents.Count);
    }

    [TestMethod]
    public async Task AggregateRoot_Dispatch_HandlerFailure_KeepsUnhandledEventsForRetry()
    {
        DomainEventDispatcher dispatcher = new();
        int calls = 0;
        dispatcher.Subscribe<Something>(e =>
        {
            calls++;
            return e.What == "b" && calls == 2 ? throw new InvalidOperationException() : Task.CompletedTask;
        });
        Basket basket = new(1);
        basket.Do("a");
        basket.Do("b");

        await Assert.ThrowsExactlyAsync<InvalidOperationException>(async () => await basket.DispatchEventsAsync(dispatcher));
        Assert.AreEqual(1, basket.DomainEvents.Count);

        await basket.DispatchEventsAsync(dispatcher);
        Assert.AreEqual(0, basket.DomainEvents.Count);
    }

    [TestMethod]
    public async Task AggregateRoot_Dispatch_Cancelled_Throws()
    {
        using CancellationTokenSource cts = new();
        cts.Cancel();
        Basket basket = new(1);
        basket.Do("a");

        await Assert.ThrowsExactlyAsync<OperationCanceledException>(async () => await basket.DispatchEventsAsync(new DomainEventDispatcher(), cts.Token));
        Assert.AreEqual(1, basket.DomainEvents.Count);
    }

    [TestMethod]
    public async Task AggregateRoot_NullDispatcher_Throws() { await Assert.ThrowsExactlyAsync<ArgumentNullException>(async () => await new Basket(1).DispatchEventsAsync(null!)); }

    [TestMethod]
    public void AggregateRoot_ClearEvents_Discards()
    {
        Basket basket = new(1);
        basket.Do("a");
        basket.ClearEvents();

        Assert.AreEqual(0, basket.DomainEvents.Count);
    }

    #endregion
}
