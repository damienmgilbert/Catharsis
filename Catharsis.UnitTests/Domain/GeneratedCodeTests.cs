using Catharsis.Domain;

namespace Catharsis.UnitTests.Domain;

///<summary>
///Tests for the code the Catharsis source generators produce inside the library.
///</summary>
[TestClass]
public class GeneratedCodeTests
{
    [TestMethod]
    public void OrderStatus_ToStringFast_MatchesToString()
    {
        foreach(OrderStatus status in Enum.GetValues<OrderStatus>())
        {
            Assert.AreEqual(status.ToString(), status.ToStringFast());
        }
    }

    [TestMethod]
    public void OrderStatus_TryParseFast_And_IsDefinedFast()
    {
        Assert.IsTrue(OrderStatusExtensions.TryParseFast("Confirmed", out OrderStatus parsed));
        Assert.AreEqual(OrderStatus.Confirmed, parsed);
        Assert.IsFalse(OrderStatusExtensions.TryParseFast("nope", out _));
        Assert.IsFalse(((OrderStatus)99).IsDefinedFast());
        Assert.IsTrue(OrderStatus.Draft.IsDefinedFast());
    }

    [TestMethod]
    public void Order_ErrorMessages_UseGeneratedName()
    {
        Order order = new(Guid.NewGuid(), "USD");
        order.Cancel("x");

        InvalidOperationException ex = Assert.ThrowsExactly<InvalidOperationException>(() => order.AddLine("A", 1, new Catharsis.Operators.Money(1m, "USD")));
        StringAssert.Contains(ex.Message, "Cancelled");
    }

    [TestMethod]
    public void OrderDraftViewModel_RaisesPropertyChangedOnlyOnChange()
    {
        OrderDraftViewModel model = new();
        List<string?> raised = [];
        model.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        model.CustomerName = "Ada";
        model.CustomerName = "Ada";
        model.GiftWrap = true;

        CollectionAssert.AreEqual(new[] { "CustomerName", "GiftWrap" }, raised);
        Assert.AreEqual("Ada", model.CustomerName);
        Assert.AreEqual(string.Empty, model.Notes);
    }
}
