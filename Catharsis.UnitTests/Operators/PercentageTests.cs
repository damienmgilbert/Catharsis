using Catharsis.Operators;

namespace Catharsis.UnitTests.Operators;

///<summary>
///Unit tests for the <see cref="Percentage"/> struct.
///</summary>
[TestClass]
public class PercentageTests
{
    [TestMethod]
    public void FromPercent_And_FromFraction_AreEquivalent() { Assert.AreEqual(Percentage.FromFraction(0.25m), Percentage.FromPercent(25m)); }

    [TestMethod]
    public void Value_ReturnsWholePercentUnits() { Assert.AreEqual(25m, Percentage.FromFraction(0.25m).Value); }

    [TestMethod]
    public void Multiply_Decimal_AppliesPercentage()
    {
        Assert.AreEqual(50m, 200m * Percentage.FromPercent(25m));
        Assert.AreEqual(50m, Percentage.FromPercent(25m) * 200m);
    }

    [TestMethod]
    public void Multiply_Money_AppliesPercentage() { Assert.AreEqual(new Money(15m, "USD"), new Money(60m, "USD") * Percentage.FromPercent(25m)); }

    [TestMethod]
    public void AddSubtract_CombinesFractions()
    {
        Assert.AreEqual(Percentage.FromPercent(30m), Percentage.FromPercent(10m) + Percentage.FromPercent(20m));
        Assert.AreEqual(Percentage.FromPercent(-10m), Percentage.FromPercent(10m) - Percentage.FromPercent(20m));
    }

    [TestMethod]
    public void Compare_Orders() { Assert.IsTrue(Percentage.FromPercent(10m) < Percentage.FromPercent(20m)); }

    [TestMethod]
    public void ToString_AppendsPercentSign() { Assert.AreEqual("12.5%", Percentage.FromPercent(12.5m).ToString()); }
}
