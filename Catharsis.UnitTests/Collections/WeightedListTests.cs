using Catharsis.Collections;

namespace Catharsis.UnitTests.Collections;

///<summary>
///Unit tests for the <see cref="WeightedList{T}"/> class.
///</summary>
[TestClass]
public class WeightedListTests
{
    #region Add

    [TestMethod]
    public void Add_ZeroOrNegativeWeight_Throws()
    {
        WeightedList<string> list = new();
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => list.Add("a", 0));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => list.Add("a", -1));
    }

    [TestMethod]
    public void Add_IncrementsCountAndTotalWeight()
    {
        WeightedList<string> list = new();
        list.Add("a", 1);
        list.Add("b", 2);
        Assert.AreEqual(2, list.Count);
        Assert.AreEqual(3, list.TotalWeight);
    }

    #endregion

    #region Enumeration

    [TestMethod]
    public void GetEnumerator_YieldsAddedItems()
    {
        WeightedList<string> list = new();
        list.Add("a", 1);
        list.Add("b", 2);
        CollectionAssert.AreEqual(new[] { "a", "b" }, list.ToList());
    }

    #endregion

    #region PickRandom

    [TestMethod]
    public void PickRandom_Empty_Throws()
    {
        WeightedList<string> list = new();
        Assert.ThrowsExactly<InvalidOperationException>(() => list.PickRandom());
    }

    [TestMethod]
    public void PickRandom_SingleItem_AlwaysReturnsThatItem()
    {
        WeightedList<string> list = new();
        list.Add("only", 5);
        Assert.AreEqual("only", list.PickRandom());
    }

    [TestMethod]
    public void PickRandom_OnlyReturnsAddedItems()
    {
        WeightedList<string> list = new();
        list.Add("a", 1);
        list.Add("b", 2);
        list.Add("c", 3);
        Random random = new(123);

        for(int i = 0; i < 100; i++)
        {
            Assert.IsTrue(list.PickRandom(random) is "a" or "b" or "c");
        }
    }

    [TestMethod]
    public void PickRandom_HeavilyWeightedItem_IsPickedMuchMoreOften()
    {
        WeightedList<string> list = new();
        list.Add("common", 99);
        list.Add("rare", 1);
        Random random = new(7);

        int commonCount = 0;

        for(int i = 0; i < 1000; i++)
        {
            if(list.PickRandom(random) == "common")
            {
                commonCount++;
            }
        }

        Assert.IsGreaterThan(900, commonCount);
    }

    #endregion

    #region Remove

    [TestMethod]
    public void Remove_ExistingItem_ReturnsTrueAndUpdatesTotalWeight()
    {
        WeightedList<string> list = new();
        list.Add("a", 1);
        list.Add("b", 2);

        Assert.IsTrue(list.Remove("a"));
        Assert.AreEqual(1, list.Count);
        Assert.AreEqual(2, list.TotalWeight);
    }

    [TestMethod]
    public void Remove_MissingItem_ReturnsFalse()
    {
        WeightedList<string> list = new();
        list.Add("a", 1);
        Assert.IsFalse(list.Remove("missing"));
    }

    [TestMethod]
    public void Remove_ThenPickRandom_StillWorksCorrectly()
    {
        WeightedList<string> list = new();
        list.Add("a", 1);
        list.Add("b", 100);
        list.Remove("b");

        Assert.AreEqual("a", list.PickRandom());
    }

    #endregion

    #region Clear

    [TestMethod]
    public void Clear_RemovesAllItemsAndResetsTotalWeight()
    {
        WeightedList<string> list = new();
        list.Add("a", 1);
        list.Add("b", 2);
        list.Clear();

        Assert.AreEqual(0, list.Count);
        Assert.AreEqual(0, list.TotalWeight);
    }

    #endregion
}
