using Catharsis.Numerics;
using System.Numerics;

namespace Catharsis.UnitTests.Numerics;

///<summary>
///Unit tests for the <see cref="Rational"/> struct.
///</summary>
[TestClass]
public class RationalTests
{
    #region Constructor

    [TestMethod]
    public void Constructor_ZeroDenominator_Throws() { Assert.ThrowsExactly<DivideByZeroException>(static () => new Rational(1, 0)); }

    [TestMethod]
    public void Constructor_ReducesToLowestTerms()
    {
        Rational value = new(4, 8);
        Assert.AreEqual(new BigInteger(1), value.Numerator);
        Assert.AreEqual(new BigInteger(2), value.Denominator);
    }

    [TestMethod]
    public void Constructor_NegativeDenominator_NormalizesSignToNumerator()
    {
        Rational value = new(1, -2);
        Assert.AreEqual(new BigInteger(-1), value.Numerator);
        Assert.AreEqual(new BigInteger(2), value.Denominator);
    }

    [TestMethod]
    public void Constructor_ZeroNumerator_ReducesToZeroOverOne()
    {
        Rational value = new(0, 5);
        Assert.AreEqual(BigInteger.Zero, value.Numerator);
        Assert.AreEqual(BigInteger.One, value.Denominator);
    }

    [TestMethod]
    public void Constructor_WholeNumber_HasDenominatorOne()
    {
        Rational value = new(5);
        Assert.AreEqual(new BigInteger(5), value.Numerator);
        Assert.AreEqual(BigInteger.One, value.Denominator);
    }

    #endregion

    #region Arithmetic operators

    [TestMethod]
    public void Add_CombinesFractionsCorrectly()
    {
        Rational result = new Rational(1, 2) + new Rational(1, 3);
        Assert.AreEqual(new Rational(5, 6), result);
    }

    [TestMethod]
    public void Subtract_CombinesFractionsCorrectly()
    {
        Rational result = new Rational(1, 2) - new Rational(1, 3);
        Assert.AreEqual(new Rational(1, 6), result);
    }

    [TestMethod]
    public void Negate_FlipsSign()
    {
        Rational result = -new Rational(1, 2);
        Assert.AreEqual(new Rational(-1, 2), result);
    }

    [TestMethod]
    public void Multiply_MultipliesNumeratorsAndDenominators()
    {
        Rational result = new Rational(2, 3) * new Rational(3, 4);
        Assert.AreEqual(new Rational(1, 2), result);
    }

    [TestMethod]
    public void Divide_MultipliesByReciprocal()
    {
        Rational result = new Rational(1, 2) / new Rational(1, 4);
        Assert.AreEqual(new Rational(2, 1), result);
    }

    [TestMethod]
    public void Divide_ByZero_Throws() { Assert.ThrowsExactly<DivideByZeroException>(static () => new Rational(1, 2) / new Rational(0, 1)); }

    #endregion

    #region Comparison

    [TestMethod]
    public void Equals_EquivalentFractions_AreEqual()
    {
        Assert.AreEqual(new Rational(1, 2), new Rational(2, 4));
        Assert.IsTrue(new Rational(1, 2) == new Rational(2, 4));
    }

    [TestMethod]
    public void NotEquals_DifferentFractions_AreNotEqual() { Assert.IsTrue(new Rational(1, 2) != new Rational(1, 3)); }

    [TestMethod]
    public void LessThan_ComparesAcrossDenominators() { Assert.IsTrue(new Rational(1, 3) < new Rational(1, 2)); }

    [TestMethod]
    public void GreaterThanOrEqual_HoldsForEqualValues() { Assert.IsTrue(new Rational(1, 2) >= new Rational(2, 4)); }

    [TestMethod]
    public void GetHashCode_EquivalentFractions_ProduceSameHash()
    {
        Assert.AreEqual(new Rational(1, 2).GetHashCode(), new Rational(2, 4).GetHashCode());
    }

    #endregion

    #region Conversions

    [TestMethod]
    public void ImplicitConversion_FromBigInteger_CreatesWholeNumberRational()
    {
        Rational value = new BigInteger(7);
        Assert.AreEqual(new Rational(7, 1), value);
    }

    [TestMethod]
    public void ImplicitConversion_FromLong_CreatesWholeNumberRational()
    {
        Rational value = 7L;
        Assert.AreEqual(new Rational(7, 1), value);
    }

    [TestMethod]
    public void ToDouble_ReturnsApproximateValue() { Assert.AreEqual(0.5, new Rational(1, 2).ToDouble()); }

    #endregion

    #region ToString

    [TestMethod]
    public void ToString_NonWholeNumber_ReturnsFractionFormat() { Assert.AreEqual("1/2", new Rational(1, 2).ToString()); }

    [TestMethod]
    public void ToString_WholeNumber_ReturnsPlainNumber() { Assert.AreEqual("5", new Rational(5, 1).ToString()); }

    #endregion
}
