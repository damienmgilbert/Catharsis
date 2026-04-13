using Catharsis.Linq;

namespace Catharsis.UnitTests.Linq;

///<summary>
///Unit tests for the <see cref="GroupJoinBuilder"/> class.
///</summary>
[TestClass]
public class GroupJoinBuilderTests
{
    private sealed record Person(int Id, string Name);
    private sealed record Order(int PersonId, string Product);

    private static readonly Person[] People = [new(1, "Alice"), new(2, "Bob"), new(3, "Carol")];
    private static readonly Order[] Orders = [new(1, "Book"), new(1, "Pen"), new(2, "Laptop")];

    [TestMethod]
    public void Select_PerformsGroupJoin()
    {
        List<string> result = People.GroupJoin()
            .With(Orders)
            .On(static p => p.Id, static o => o.PersonId)
            .Select(static (p, orders) => $"{p.Name}:{orders.Count()}")
            .ToList();

        CollectionAssert.AreEqual(new[] { "Alice:2", "Bob:1", "Carol:0" }, result);
    }

    [TestMethod]
    public void AsTuples_ReturnsTuples()
    {
        List<(Person, IEnumerable<Order>)> result = People.GroupJoin()
            .With(Orders)
            .On(static p => p.Id, static o => o.PersonId)
            .AsTuples()
            .ToList();

        Assert.HasCount(3, result);
        Assert.AreEqual("Alice", result[0].Item1.Name);
        Assert.AreEqual(2, result[0].Item2.Count());
    }

    [TestMethod]
    public void AsGroupings_ReturnsGroupings()
    {
        List<IGrouping<Person, Order>> result = People.GroupJoin()
            .With(Orders)
            .On(static p => p.Id, static o => o.PersonId)
            .AsGroupings()
            .ToList();

        Assert.HasCount(3, result);
        Assert.AreEqual("Alice", result[0].Key.Name);
    }

    [TestMethod]
    public void LeftJoin_IncludesAllOuterElements()
    {
        List<string> result = People.GroupJoin()
            .With(Orders)
            .On(static p => p.Id, static o => o.PersonId)
            .LeftJoin(static (p, o) => $"{p.Name}:{o?.Product ?? "none"}")
            .ToList();

        Assert.Contains("Carol:none", result);
        Assert.HasCount(4, result); // Alice:Book, Alice:Pen, Bob:Laptop, Carol:none
    }

    [TestMethod]
    public void LeftJoinTuples_IncludesAllOuterElements()
    {
        List<(Person, Order?)> result = People.GroupJoin()
            .With(Orders)
            .On(static p => p.Id, static o => o.PersonId)
            .LeftJoinTuples()
            .ToList();

        Assert.HasCount(4, result);
    }

    [TestMethod]
    public void GroupJoin_NullOuter_ThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(
            static () => ((IEnumerable<Person>)null!).GroupJoin());
    }

    [TestMethod]
    public void With_NullInner_ThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(
            static () => People.GroupJoin().With<Order>(null!));
    }

    [TestMethod]
    public void On_NullKeySelector_ThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(
            static () => People.GroupJoin().With(Orders).On<int>(null!, static o => o.PersonId));
    }
}
