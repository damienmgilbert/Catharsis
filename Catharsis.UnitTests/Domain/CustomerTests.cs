using Catharsis.Domain;

namespace Catharsis.UnitTests.Domain;

///<summary>
///Unit tests for the <see cref="Customer"/> entity, whose two partial halves are exercised together.
///</summary>
[TestClass]
public class CustomerTests
{
    static Customer New() => new(Guid.NewGuid(), " Ada ", " ada@example.com ");

    [TestMethod]
    public void Constructor_TrimsAndStoresValues()
    {
        Customer customer = New();

        Assert.AreEqual("Ada", customer.Name);
        Assert.AreEqual("ada@example.com", customer.Email);
        Assert.AreEqual(0, customer.LoyaltyPoints);
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("  ")]
    public void Constructor_BlankName_Throws(string name) { Assert.ThrowsExactly<ArgumentException>(() => new Customer(Guid.NewGuid(), name, "a@b.c")); }

    [TestMethod]
    [DataRow("")]
    [DataRow("nope")]
    [DataRow("@b.c")]
    [DataRow("a@")]
    [DataRow("a@@b.c")]
    [DataRow("a b@c.d")]
    public void Constructor_InvalidEmail_Throws(string email) { Assert.ThrowsExactly<ArgumentException>(() => new Customer(Guid.NewGuid(), "Ada", email)); }

    [TestMethod]
    public void Rename_And_ChangeEmail_UpdateState()
    {
        Customer customer = New();

        customer.Rename("Grace");
        customer.ChangeEmail("grace@example.com");

        Assert.AreEqual("Grace", customer.Name);
        Assert.AreEqual("grace@example.com", customer.Email);
    }

    [TestMethod]
    public void Rename_Invalid_LeavesNameUnchanged()
    {
        Customer customer = New();

        Assert.ThrowsExactly<ArgumentException>(() => customer.Rename(" "));
        Assert.ThrowsExactly<ArgumentException>(() => customer.ChangeEmail("bad"));

        Assert.AreEqual("Ada", customer.Name);
        Assert.AreEqual("ada@example.com", customer.Email);
    }

    [TestMethod]
    public void AddPoints_Accumulates_AndRejectsNonPositive()
    {
        Customer customer = New();

        Assert.AreEqual(10, customer.AddPoints(10));
        Assert.AreEqual(15, customer.AddPoints(5));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => customer.AddPoints(0));
    }

    [TestMethod]
    public void AddPoints_Overflow_Throws_AndKeepsBalance()
    {
        Customer customer = New();
        customer.AddPoints(int.MaxValue);

        Assert.ThrowsExactly<OverflowException>(() => customer.AddPoints(1));
        Assert.AreEqual(int.MaxValue, customer.LoyaltyPoints);
    }

    [TestMethod]
    public void RedeemPoints_Enough_ReturnsRemainingBalance()
    {
        Customer customer = New();
        customer.AddPoints(10);

        Assert.AreEqual(4, customer.RedeemPoints(6).Value);
        Assert.AreEqual(4, customer.LoyaltyPoints);
    }

    [TestMethod]
    public void RedeemPoints_TooMany_FailsWithoutChangingBalance()
    {
        Customer customer = New();
        customer.AddPoints(3);

        Catharsis.Generics.Result<int, string> result = customer.RedeemPoints(5);

        Assert.IsTrue(result.IsFailure);
        StringAssert.Contains(result.Error, "only 3");
        Assert.AreEqual(3, customer.LoyaltyPoints);
    }

    [TestMethod]
    public void RedeemPoints_NonPositive_Throws() { Assert.ThrowsExactly<ArgumentOutOfRangeException>(static () => New().RedeemPoints(0)); }

    [TestMethod]
    public void Equality_IsByIdentity()
    {
        Guid id = Guid.NewGuid();

        Assert.AreEqual(new Customer(id, "A", "a@b.c"), new Customer(id, "Different", "d@e.f"));
    }
}
