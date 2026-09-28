using Catharsis.Common;

namespace Catharsis.UnitTests.Common;

///<summary>
///Unit tests for the <see cref="HashCodeCombiner"/> class.
///</summary>
[TestClass]
public class HashCodeCombinerTests
{
    #region Start / Add / ToHashCode

    [TestMethod]
    public void Start_ReturnsNewInstance() { Assert.IsNotNull(HashCodeCombiner.Start()); }

    [TestMethod]
    public void Add_ReturnsSameInstance_ForChaining()
    {
        HashCodeCombiner combiner = HashCodeCombiner.Start();
        HashCodeCombiner result = combiner.Add(1);
        Assert.AreSame(combiner, result);
    }

    [TestMethod]
    public void ToHashCode_SameValuesInSameOrder_ProduceEqualHashCodes()
    {
        int first = HashCodeCombiner.Start().Add(1).Add("a").Add(true).ToHashCode();
        int second = HashCodeCombiner.Start().Add(1).Add("a").Add(true).ToHashCode();

        Assert.AreEqual(first, second);
    }

    [TestMethod]
    public void ToHashCode_DifferentOrder_ProducesDifferentHashCode()
    {
        int first = HashCodeCombiner.Start().Add(1).Add(2).ToHashCode();
        int second = HashCodeCombiner.Start().Add(2).Add(1).ToHashCode();

        Assert.AreNotEqual(first, second);
    }

    #endregion

    #region Combine

    [TestMethod]
    public void Combine_NullValues_Throws() { Assert.ThrowsExactly<ArgumentNullException>(static () => HashCodeCombiner.Combine(null!)); }

    [TestMethod]
    public void Combine_MatchesEquivalentChainedResult()
    {
        object?[] values = ["a", 1, true];

        int combined = HashCodeCombiner.Combine(values);
        int chained = HashCodeCombiner.Start().Add(values[0]).Add(values[1]).Add(values[2]).ToHashCode();

        Assert.AreEqual(chained, combined);
    }

    #endregion
}
