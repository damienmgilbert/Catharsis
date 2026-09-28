using Catharsis.Numerics;

namespace Catharsis.UnitTests.Numerics;

///<summary>
///Unit tests for the <see cref="GenericMathExtensions"/> class.
///</summary>
[TestClass]
public class GenericMathExtensionsTests
{
    #region IsBetween

    [TestMethod]
    public void IsBetween_ValueWithinRange_ReturnsTrue() { Assert.IsTrue(5.IsBetween(1, 10)); }

    [TestMethod]
    public void IsBetween_ValueEqualToBound_ReturnsTrue()
    {
        Assert.IsTrue(1.IsBetween(1, 10));
        Assert.IsTrue(10.IsBetween(1, 10));
    }

    [TestMethod]
    public void IsBetween_ValueOutsideRange_ReturnsFalse()
    {
        Assert.IsFalse(0.IsBetween(1, 10));
        Assert.IsFalse(11.IsBetween(1, 10));
    }

    [TestMethod]
    public void IsBetween_DoubleValues_Works() { Assert.IsTrue(2.5.IsBetween(1.0, 3.0)); }

    #endregion

    #region Lerp

    [TestMethod]
    public void Lerp_HalfwayFactor_ReturnsMidpoint() { Assert.AreEqual(5.0, GenericMathExtensions.Lerp(0.0, 10.0, 0.5)); }

    [TestMethod]
    public void Lerp_ZeroFactor_ReturnsStart() { Assert.AreEqual(0.0, GenericMathExtensions.Lerp(0.0, 10.0, 0.0)); }

    [TestMethod]
    public void Lerp_OneFactor_ReturnsEnd() { Assert.AreEqual(10.0, GenericMathExtensions.Lerp(0.0, 10.0, 1.0)); }

    [TestMethod]
    public void Lerp_FactorBeyondOne_Extrapolates() { Assert.AreEqual(20.0, GenericMathExtensions.Lerp(0.0, 10.0, 2.0)); }

    [TestMethod]
    public void Lerp_IntegerType_RoundsToNearestInt() { Assert.AreEqual(5, GenericMathExtensions.Lerp(0, 10, 0.5)); }

    #endregion

    #region NormalizeTo

    [TestMethod]
    public void NormalizeTo_MinEqualsMax_Throws() { Assert.ThrowsExactly<ArgumentException>(static () => 5.NormalizeTo(3, 3)); }

    [TestMethod]
    public void NormalizeTo_ValueAtMin_ReturnsZero() { Assert.AreEqual(0.0, 0.NormalizeTo(0, 10)); }

    [TestMethod]
    public void NormalizeTo_ValueAtMax_ReturnsOne() { Assert.AreEqual(1.0, 10.NormalizeTo(0, 10)); }

    [TestMethod]
    public void NormalizeTo_ValueAtMidpoint_ReturnsHalf() { Assert.AreEqual(0.5, 5.NormalizeTo(0, 10)); }

    [TestMethod]
    public void NormalizeTo_ValueBelowMin_ReturnsNegative() { Assert.AreEqual(-0.5, (-5).NormalizeTo(0, 10)); }

    #endregion
}
