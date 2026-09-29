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
    public void ToString_WithFormat_AppliesToWholePercentValue()
    {
        Percentage value = Percentage.FromFraction(0.125m);

        Assert.AreEqual("12.5%", value.ToString("%", null));
        Assert.AreEqual("12.5%", value.ToString("F1", null));
        Assert.AreEqual("13%", value.ToString("F0", null));
        Assert.AreEqual("12.50%", value.ToString("F2", System.Globalization.CultureInfo.InvariantCulture));
    }

    [TestMethod]
    public void ToString_PFormat_UsesCultureFormatting() { Assert.AreEqual(0.125m.ToString("P", System.Globalization.CultureInfo.InvariantCulture), Percentage.FromFraction(0.125m).ToString("P", System.Globalization.CultureInfo.InvariantCulture)); }

    [TestMethod]
    public void Interpolation_UsesFormatter() { Assert.AreEqual("rate 12.5%", string.Create(System.Globalization.CultureInfo.InvariantCulture, $"rate {Percentage.FromFraction(0.125m):F1}")); }

    [TestMethod]
    public void ToString_AppendsPercentSign() { Assert.AreEqual("12.5%", Percentage.FromPercent(12.5m).ToString()); }
}
