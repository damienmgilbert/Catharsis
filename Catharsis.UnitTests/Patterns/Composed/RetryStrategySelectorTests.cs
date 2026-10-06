using Catharsis.Patterns.Composed;
using Catharsis.Scheduling;

namespace Catharsis.UnitTests.Patterns.Composed;

///<summary>
///Unit tests for <see cref="RetryStrategySelector"/>.
///</summary>
[TestClass]
public class RetryStrategySelectorTests
{
    static RetryStrategySelector Selector() => new(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(10), random: new Random(1));

    [TestMethod]
    public void Constructor_InvalidDelays_Throw() { Assert.ThrowsExactly<ArgumentOutOfRangeException>(static () => new RetryStrategySelector(TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(1))); }

    [TestMethod]
    public void Select_None_IsDeterministicExponential()
    {
        JitteredDelayCalculator none = Selector().Select(JitterStrategy.None);

        Assert.AreEqual(TimeSpan.FromMilliseconds(100), none.ComputeDelay(0));
        Assert.AreEqual(TimeSpan.FromMilliseconds(400), none.ComputeDelay(2));
    }

    [TestMethod]
    public void Select_Full_StaysWithinZeroAndExponentialCeiling()
    {
        JitteredDelayCalculator full = Selector().Select(JitterStrategy.Full);

        for (int i = 0; i < 50; i++)
        {
            TimeSpan delay = full.ComputeDelay(2);
            Assert.IsTrue(delay >= TimeSpan.Zero && delay <= TimeSpan.FromMilliseconds(400));
        }
    }

    [TestMethod]
    public void Select_Equal_KeepsAtLeastHalfTheCeiling()
    {
        JitteredDelayCalculator equal = Selector().Select(JitterStrategy.Equal);

        for (int i = 0; i < 50; i++)
        {
            TimeSpan delay = equal.ComputeDelay(2);
            Assert.IsTrue(delay >= TimeSpan.FromMilliseconds(200) && delay <= TimeSpan.FromMilliseconds(400));
        }
    }

    [TestMethod]
    public void Select_SameStrategy_ReturnsSameCalculator()
    {
        RetryStrategySelector selector = Selector();

        Assert.AreSame(selector.Select(JitterStrategy.Full), selector.Select(JitterStrategy.Full));
        Assert.AreNotSame(selector.Select(JitterStrategy.Full), selector.Select(JitterStrategy.Equal));
    }

    [TestMethod]
    public void Select_UndefinedStrategy_Throws() { Assert.ThrowsExactly<ArgumentOutOfRangeException>(static () => Selector().Select((JitterStrategy)99)); }

    [TestMethod]
    [DataRow(1, JitterStrategy.None)]
    [DataRow(2, JitterStrategy.Equal)]
    [DataRow(10, JitterStrategy.Equal)]
    [DataRow(11, JitterStrategy.Full)]
    [DataRow(1000, JitterStrategy.Full)]
    public void Recommend_ScalesJitterWithClientCount(int clients, JitterStrategy expected) { Assert.AreEqual(expected, RetryStrategySelector.Recommend(clients)); }

    [TestMethod]
    public void Recommend_NonPositive_Throws() { Assert.ThrowsExactly<ArgumentOutOfRangeException>(static () => RetryStrategySelector.Recommend(0)); }

    [TestMethod]
    public void ForClients_ReturnsRecommendedCalculator()
    {
        RetryStrategySelector selector = Selector();

        Assert.AreSame(selector.Select(JitterStrategy.None), selector.ForClients(1));
        Assert.AreSame(selector.Select(JitterStrategy.Full), selector.ForClients(50));
    }
}
