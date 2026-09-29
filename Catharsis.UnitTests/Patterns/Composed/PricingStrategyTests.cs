using Catharsis.Operators;
using Catharsis.Patterns.Composed;

namespace Catharsis.UnitTests.Patterns.Composed;

///<summary>
///Unit tests for the pricing strategies and <see cref="PricingContext{TItem}"/>.
///</summary>
[TestClass]
public class PricingStrategyTests
{
    sealed record Line(string Sku, int Quantity);

    static readonly Line Ten = new("A", 10);

    static TieredPricingStrategy<Line> Tiers() => new(static l => l.Quantity, "USD", [(1, 5m), (10, 4m), (100, 3m)]);

    #region Flat

    [TestMethod]
    public void Flat_ReturnsSamePriceForAnyItem() { Assert.AreEqual(new Money(9m, "USD"), new FlatPricingStrategy<Line>(new Money(9m, "USD")).Price(Ten)); }

    #endregion

    #region Tiered

    [TestMethod]
    public void Tiered_UsesHighestReachedTierForAllUnits()
    {
        TieredPricingStrategy<Line> strategy = Tiers();

        Assert.AreEqual(new Money(15m, "USD"), strategy.Price(new Line("A", 3)));
        Assert.AreEqual(new Money(40m, "USD"), strategy.Price(new Line("A", 10)));
        Assert.AreEqual(new Money(396m, "USD"), strategy.Price(new Line("A", 99)));
        Assert.AreEqual(new Money(300m, "USD"), strategy.Price(new Line("A", 100)));
    }

    [TestMethod]
    public void Tiered_ZeroQuantity_IsFree() { Assert.AreEqual(new Money(0m, "USD"), Tiers().Price(new Line("A", 0))); }

    [TestMethod]
    public void Tiered_NegativeQuantity_Throws() { Assert.ThrowsExactly<ArgumentOutOfRangeException>(static () => Tiers().Price(new Line("A", -1))); }

    [TestMethod]
    public void Tiered_InvalidTiers_Throw()
    {
        Assert.ThrowsExactly<ArgumentException>(static () => new TieredPricingStrategy<Line>(static l => l.Quantity, "USD", []));
        Assert.ThrowsExactly<ArgumentException>(static () => new TieredPricingStrategy<Line>(static l => l.Quantity, "USD", [(5, 1m)]));
        Assert.ThrowsExactly<ArgumentException>(static () => new TieredPricingStrategy<Line>(static l => l.Quantity, "USD", [(1, 1m), (1, 2m)]));
    }

    [TestMethod]
    public void Tiered_NullArguments_Throw()
    {
        Assert.ThrowsExactly<ArgumentNullException>(static () => new TieredPricingStrategy<Line>(null!, "USD", [(1, 1m)]));
        Assert.ThrowsExactly<ArgumentNullException>(static () => new TieredPricingStrategy<Line>(static l => l.Quantity, null!, [(1, 1m)]));
    }

    #endregion

    #region Discount

    [TestMethod]
    public void Discount_TakesPercentageOffInnerPrice()
    {
        DiscountPricingStrategy<Line> strategy = new(new FlatPricingStrategy<Line>(new Money(80m, "USD")), Percentage.FromPercent(25m));

        Assert.AreEqual(new Money(60m, "USD"), strategy.Price(Ten));
    }

    [TestMethod]
    public void Discount_Stacks()
    {
        IPricingStrategy<Line> flat = new FlatPricingStrategy<Line>(new Money(100m, "USD"));
        IPricingStrategy<Line> stacked = new DiscountPricingStrategy<Line>(new DiscountPricingStrategy<Line>(flat, Percentage.FromPercent(10m)), Percentage.FromPercent(10m));

        Assert.AreEqual(new Money(81m, "USD"), stacked.Price(Ten));
    }

    [TestMethod]
    public void Discount_RoundsToCents()
    {
        DiscountPricingStrategy<Line> strategy = new(new FlatPricingStrategy<Line>(new Money(10m, "USD")), Percentage.FromFraction(1m / 3m));

        Assert.AreEqual(new Money(6.67m, "USD"), strategy.Price(Ten));
    }

    [TestMethod]
    public void Discount_OutOfRange_Throws()
    {
        IPricingStrategy<Line> flat = new FlatPricingStrategy<Line>(new Money(1m, "USD"));

        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new DiscountPricingStrategy<Line>(flat, Percentage.FromPercent(-1m)));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new DiscountPricingStrategy<Line>(flat, Percentage.FromPercent(101m)));
    }

    [TestMethod]
    public void Discount_NullInner_Throws() { Assert.ThrowsExactly<ArgumentNullException>(static () => new DiscountPricingStrategy<Line>(null!, Percentage.FromPercent(1m))); }

    #endregion

    #region Context

    [TestMethod]
    public void Context_PricesWithNamedStrategy_CaseInsensitive()
    {
        PricingContext<Line> context = new PricingContext<Line>()
            .Register("standard", Tiers())
            .Register("clearance", new FlatPricingStrategy<Line>(new Money(1m, "USD")));

        Assert.AreEqual(new Money(40m, "USD"), context.Price("STANDARD", Ten));
        Assert.AreEqual(new Money(1m, "USD"), context.Price("clearance", Ten));
        Assert.AreEqual(2, context.Names.Count);
    }

    [TestMethod]
    public void Context_UnknownName_Throws() { Assert.ThrowsExactly<KeyNotFoundException>(static () => new PricingContext<Line>().Price("nope", Ten)); }

    [TestMethod]
    public void Context_DuplicateName_Throws()
    {
        PricingContext<Line> context = new PricingContext<Line>().Register("a", Tiers());

        Assert.ThrowsExactly<InvalidOperationException>(() => context.Register("A", Tiers()));
    }

    [TestMethod]
    public void Context_NullArguments_Throw()
    {
        PricingContext<Line> context = new();
        Assert.ThrowsExactly<ArgumentNullException>(() => context.Register(null!, Tiers()));
        Assert.ThrowsExactly<ArgumentNullException>(() => context.Register("x", null!));
    }

    #endregion
}
