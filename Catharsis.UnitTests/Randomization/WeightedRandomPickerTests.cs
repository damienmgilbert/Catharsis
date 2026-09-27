using Catharsis.Randomization;

namespace Catharsis.UnitTests.Randomization;

///<summary>
///Unit tests for the <see cref="WeightedRandomPicker{T}"/> class.
///</summary>
[TestClass]
public class WeightedRandomPickerTests
{
    #region Add

    [TestMethod]
    public void Add_ZeroOrNegativeWeight_Throws()
    {
        WeightedRandomPicker<string> picker = new();
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => picker.Add("a", 0));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => picker.Add("a", -1));
    }

    [TestMethod]
    public void Add_ReturnsSameInstanceForChaining()
    {
        WeightedRandomPicker<string> picker = new();
        WeightedRandomPicker<string> result = picker.Add("a", 1);
        Assert.AreSame(picker, result);
    }

    [TestMethod]
    public void Add_IncrementsCount()
    {
        WeightedRandomPicker<string> picker = new();
        picker.Add("a", 1).Add("b", 1);
        Assert.AreEqual(2, picker.Count);
    }

    #endregion

    #region Pick

    [TestMethod]
    public void Pick_NoItems_Throws()
    {
        WeightedRandomPicker<string> picker = new();
        Assert.ThrowsExactly<InvalidOperationException>(() => picker.Pick());
    }

    [TestMethod]
    public void Pick_SingleItem_AlwaysReturnsThatItem()
    {
        WeightedRandomPicker<string> picker = new();
        picker.Add("only", 5);
        Assert.AreEqual("only", picker.Pick());
    }

    [TestMethod]
    public void Pick_ZeroWeightRoll_ReturnsFirstItem()
    {
        WeightedRandomPicker<string> picker = new();
        picker.Add("first", 1).Add("second", 1);

        Assert.AreEqual("first", picker.Pick(new ZeroRandom()));
    }

    [TestMethod]
    public void Pick_OnlyReturnsAddedItems()
    {
        WeightedRandomPicker<string> picker = new();
        picker.Add("a", 1).Add("b", 2).Add("c", 3);
        Random random = new(123);

        for(int i = 0; i < 100; i++)
        {
            string picked = picker.Pick(random);
            Assert.IsTrue(picked is "a" or "b" or "c");
        }
    }

    [TestMethod]
    public void Pick_HeavilyWeightedItem_IsPickedMuchMoreOften()
    {
        WeightedRandomPicker<string> picker = new();
        picker.Add("common", 99).Add("rare", 1);
        Random random = new(7);

        int commonCount = 0;

        for(int i = 0; i < 1000; i++)
        {
            if(picker.Pick(random) == "common")
            {
                commonCount++;
            }
        }

        Assert.IsGreaterThan(900, commonCount);
    }

    #endregion

    sealed class ZeroRandom : Random
    {
        public override double NextDouble() => 0.0;
    }
}
