using Catharsis.DataStructures;

namespace Catharsis.UnitTests.DataStructures;

///<summary>
///Unit tests for the <see cref="BloomFilter{T}"/> class.
///</summary>
[TestClass]
public class BloomFilterTests
{
    #region Construction

    [TestMethod]
    public void Constructor_ZeroOrNegativeExpectedItemCount_Throws()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new BloomFilter<string>(0));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new BloomFilter<string>(-1));
    }

    [TestMethod]
    public void Constructor_FalsePositiveRateOutOfRange_Throws()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new BloomFilter<string>(100, 0));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new BloomFilter<string>(100, 1));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new BloomFilter<string>(100, -0.1));
    }

    [TestMethod]
    public void Constructor_ProducesAtLeastOneBitAndOneHashFunction()
    {
        BloomFilter<string> filter = new(100);
        Assert.IsGreaterThanOrEqualTo(1, filter.BitCount);
        Assert.IsGreaterThanOrEqualTo(1, filter.HashFunctionCount);
    }

    #endregion

    #region Add / MightContain

    [TestMethod]
    public void MightContain_NeverAdded_ReturnsFalse()
    {
        BloomFilter<string> filter = new(1000);
        Assert.IsFalse(filter.MightContain("never-added"));
    }

    [TestMethod]
    public void MightContain_Added_ReturnsTrue()
    {
        BloomFilter<string> filter = new(1000);
        filter.Add("hello");
        Assert.IsTrue(filter.MightContain("hello"));
    }

    [TestMethod]
    public void MightContain_ManyAddedItems_NoFalseNegatives()
    {
        BloomFilter<int> filter = new(1000, 0.01);

        for (int i = 0; i < 1000; i++)
        {
            filter.Add(i);
        }

        for (int i = 0; i < 1000; i++)
        {
            Assert.IsTrue(filter.MightContain(i));
        }
    }

    [TestMethod]
    public void MightContain_LargeUnaddedSet_FalsePositiveRateIsReasonablyLow()
    {
        BloomFilter<int> filter = new(1000, 0.01);

        for (int i = 0; i < 1000; i++)
        {
            filter.Add(i);
        }

        int falsePositives = 0;

        for (int i = 1_000_000; i < 1_010_000; i++)
        {
            if (filter.MightContain(i))
            {
                falsePositives++;
            }
        }

        // Target is 1%; allow generous headroom to keep the test non-flaky.
        Assert.IsLessThan(1000, falsePositives);
    }

    #endregion

    #region Clear

    [TestMethod]
    public void Clear_RemovesAllAddedItems()
    {
        BloomFilter<string> filter = new(100);
        filter.Add("hello");
        filter.Clear();
        Assert.IsFalse(filter.MightContain("hello"));
    }

    #endregion
}
