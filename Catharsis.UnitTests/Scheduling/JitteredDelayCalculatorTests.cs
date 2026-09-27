using Catharsis.Scheduling;

namespace Catharsis.UnitTests.Scheduling;

///<summary>
///Unit tests for the <see cref="JitteredDelayCalculator"/> class.
///</summary>
[TestClass]
public class JitteredDelayCalculatorTests
{
    #region Construction

    [TestMethod]
    public void Constructor_NegativeBaseDelay_Throws()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
            new JitteredDelayCalculator(TimeSpan.FromMilliseconds(-1), TimeSpan.FromSeconds(1)));
    }

    [TestMethod]
    public void Constructor_MaxDelayLessThanBaseDelay_Throws()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
            new JitteredDelayCalculator(TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(1)));
    }

    [TestMethod]
    public void Constructor_MultiplierBelowOne_Throws()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
            new JitteredDelayCalculator(TimeSpan.FromMilliseconds(200), TimeSpan.FromSeconds(30), multiplier: 0.5));
    }

    #endregion

    #region ComputeDelay

    [TestMethod]
    public void ComputeDelay_NegativeAttempt_Throws()
    {
        JitteredDelayCalculator calculator = new(TimeSpan.FromMilliseconds(200), TimeSpan.FromSeconds(30));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => calculator.ComputeDelay(-1));
    }

    [TestMethod]
    public void ComputeDelay_NoJitter_ReturnsExactExponentialValue()
    {
        JitteredDelayCalculator calculator = new(
            TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(30), multiplier: 2.0, jitter: JitterStrategy.None);

        Assert.AreEqual(TimeSpan.FromMilliseconds(100), calculator.ComputeDelay(0));
        Assert.AreEqual(TimeSpan.FromMilliseconds(200), calculator.ComputeDelay(1));
        Assert.AreEqual(TimeSpan.FromMilliseconds(400), calculator.ComputeDelay(2));
    }

    [TestMethod]
    public void ComputeDelay_ExceedsMaxDelay_IsCapped()
    {
        JitteredDelayCalculator calculator = new(
            TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(5), multiplier: 10.0, jitter: JitterStrategy.None);

        Assert.AreEqual(TimeSpan.FromSeconds(5), calculator.ComputeDelay(5));
    }

    [TestMethod]
    public void ComputeDelay_FullJitter_IsBetweenZeroAndCappedValue()
    {
        JitteredDelayCalculator calculator = new(
            TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(30), multiplier: 2.0, jitter: JitterStrategy.Full, random: new Random(1));

        for(int attempt = 0; attempt < 5; attempt++)
        {
            TimeSpan delay = calculator.ComputeDelay(attempt);
            Assert.IsTrue(delay >= TimeSpan.Zero);
            Assert.IsTrue(delay <= TimeSpan.FromMilliseconds(100 * Math.Pow(2, attempt)));
        }
    }

    [TestMethod]
    public void ComputeDelay_EqualJitter_IsBetweenHalfAndFullCappedValue()
    {
        JitteredDelayCalculator calculator = new(
            TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(30), multiplier: 2.0, jitter: JitterStrategy.Equal, random: new Random(1));

        double capped = 100 * Math.Pow(2, 3);
        TimeSpan delay = calculator.ComputeDelay(3);

        Assert.IsTrue(delay >= TimeSpan.FromMilliseconds(capped / 2));
        Assert.IsTrue(delay <= TimeSpan.FromMilliseconds(capped));
    }

    [TestMethod]
    public void ComputeDelay_DeterministicRandom_IsReproducible()
    {
        JitteredDelayCalculator calculator1 = new(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(30), random: new Random(42));
        JitteredDelayCalculator calculator2 = new(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(30), random: new Random(42));

        Assert.AreEqual(calculator1.ComputeDelay(2), calculator2.ComputeDelay(2));
    }

    #endregion
}
