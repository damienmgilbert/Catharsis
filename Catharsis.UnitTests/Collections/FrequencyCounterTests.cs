using Catharsis.Collections;

namespace Catharsis.UnitTests.Collections;

///<summary>
///Unit tests for the <see cref="FrequencyCounter{T}"/> class.
///</summary>
[TestClass]
public class FrequencyCounterTests
{
    #region Add

    [TestMethod]
    public void Add_NullItem_Throws()
    {
        FrequencyCounter<string> counter = new();
        Assert.ThrowsExactly<ArgumentNullException>(() => counter.Add(null!));
    }

    [TestMethod]
    public void Add_ZeroOrNegativeCount_Throws()
    {
        FrequencyCounter<string> counter = new();
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => counter.Add("a", 0));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => counter.Add("a", -1));
    }

    [TestMethod]
    public void Add_NewItem_SetsCountToOne()
    {
        FrequencyCounter<string> counter = new();
        counter.Add("a");
        Assert.AreEqual(1, counter.GetCount("a"));
    }

    [TestMethod]
    public void Add_ExistingItem_AccumulatesCount()
    {
        FrequencyCounter<string> counter = new();
        counter.Add("a");
        counter.Add("a", 2);
        Assert.AreEqual(3, counter.GetCount("a"));
    }

    #endregion

    #region GetCount

    [TestMethod]
    public void GetCount_NeverAddedItem_ReturnsZero()
    {
        FrequencyCounter<string> counter = new();
        Assert.AreEqual(0, counter.GetCount("never-added"));
    }

    #endregion

    #region Remove

    [TestMethod]
    public void Remove_ZeroOrNegativeCount_Throws()
    {
        FrequencyCounter<string> counter = new();
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => counter.Remove("a", 0));
    }

    [TestMethod]
    public void Remove_PartialCount_DecrementsWithoutRemoving()
    {
        FrequencyCounter<string> counter = new();
        counter.Add("a", 5);

        Assert.IsTrue(counter.Remove("a", 2));
        Assert.AreEqual(3, counter.GetCount("a"));
        Assert.AreEqual(1, counter.DistinctCount);
    }

    [TestMethod]
    public void Remove_FullCount_RemovesItemEntirely()
    {
        FrequencyCounter<string> counter = new();
        counter.Add("a", 3);

        Assert.IsTrue(counter.Remove("a", 3));
        Assert.AreEqual(0, counter.GetCount("a"));
        Assert.AreEqual(0, counter.DistinctCount);
    }

    [TestMethod]
    public void Remove_MoreThanCurrentCount_RemovesEntirely()
    {
        FrequencyCounter<string> counter = new();
        counter.Add("a", 2);

        Assert.IsTrue(counter.Remove("a", 100));
        Assert.AreEqual(0, counter.GetCount("a"));
    }

    [TestMethod]
    public void Remove_NeverAddedItem_ReturnsFalse()
    {
        FrequencyCounter<string> counter = new();
        Assert.IsFalse(counter.Remove("missing"));
    }

    #endregion

    #region Top

    [TestMethod]
    public void Top_ZeroOrNegativeN_Throws()
    {
        FrequencyCounter<string> counter = new();
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => counter.Top(0).ToList());
    }

    [TestMethod]
    public void Top_ReturnsMostFrequentFirst()
    {
        FrequencyCounter<string> counter = new();
        counter.Add("rare");
        counter.Add("common", 10);
        counter.Add("medium", 5);

        List<(string Item, int Count)> top = [.. counter.Top(2)];

        Assert.HasCount(2, top);
        Assert.AreEqual("common", top[0].Item);
        Assert.AreEqual("medium", top[1].Item);
    }

    [TestMethod]
    public void Top_NGreaterThanDistinctCount_ReturnsAll()
    {
        FrequencyCounter<string> counter = new();
        counter.Add("a");
        counter.Add("b");

        Assert.HasCount(2, counter.Top(100).ToList());
    }

    #endregion

    #region DistinctCount / TotalCount / Clear

    [TestMethod]
    public void TotalCount_SumsAllCounts()
    {
        FrequencyCounter<string> counter = new();
        counter.Add("a", 2);
        counter.Add("b", 3);
        Assert.AreEqual(5, counter.TotalCount);
    }

    [TestMethod]
    public void Clear_RemovesEverything()
    {
        FrequencyCounter<string> counter = new();
        counter.Add("a", 5);
        counter.Clear();
        Assert.AreEqual(0, counter.DistinctCount);
        Assert.AreEqual(0, counter.TotalCount);
    }

    #endregion
}
