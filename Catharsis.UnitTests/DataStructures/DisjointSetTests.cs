using Catharsis.DataStructures;

namespace Catharsis.UnitTests.DataStructures;

///<summary>
///Unit tests for the <see cref="DisjointSet"/> class.
///</summary>
[TestClass]
public class DisjointSetTests
{
    #region Construction

    [TestMethod]
    public void Constructor_ZeroOrNegativeCount_Throws()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new DisjointSet(0));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new DisjointSet(-1));
    }

    [TestMethod]
    public void Constructor_EachElementStartsInItsOwnSet()
    {
        DisjointSet set = new(5);
        Assert.AreEqual(5, set.SetCount);
        Assert.IsFalse(set.AreConnected(0, 1));
    }

    #endregion

    #region Find / Union / AreConnected

    [TestMethod]
    public void Find_OutOfRange_Throws()
    {
        DisjointSet set = new(3);
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => set.Find(-1));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => set.Find(3));
    }

    [TestMethod]
    public void Union_TwoDifferentSets_MergesThemAndDecrementsSetCount()
    {
        DisjointSet set = new(5);
        Assert.IsTrue(set.Union(0, 1));
        Assert.AreEqual(4, set.SetCount);
        Assert.IsTrue(set.AreConnected(0, 1));
    }

    [TestMethod]
    public void Union_AlreadyConnected_ReturnsFalseAndDoesNotChangeSetCount()
    {
        DisjointSet set = new(5);
        set.Union(0, 1);
        Assert.IsFalse(set.Union(0, 1));
        Assert.AreEqual(4, set.SetCount);
    }

    [TestMethod]
    public void Union_IsTransitive()
    {
        DisjointSet set = new(5);
        set.Union(0, 1);
        set.Union(1, 2);

        Assert.IsTrue(set.AreConnected(0, 2));
        Assert.AreEqual(3, set.SetCount);
    }

    [TestMethod]
    public void AreConnected_UnrelatedElements_ReturnsFalse()
    {
        DisjointSet set = new(5);
        set.Union(0, 1);
        Assert.IsFalse(set.AreConnected(0, 4));
    }

    [TestMethod]
    public void Union_AllElements_ResultsInSingleSet()
    {
        DisjointSet set = new(10);

        for (int i = 1; i < 10; i++)
        {
            set.Union(0, i);
        }

        Assert.AreEqual(1, set.SetCount);

        for (int i = 0; i < 10; i++)
        {
            Assert.IsTrue(set.AreConnected(0, i));
        }
    }

    #endregion

    #region Count

    [TestMethod]
    public void Count_ReflectsConstructorArgument()
    {
        DisjointSet set = new(7);
        Assert.AreEqual(7, set.Count);
    }

    #endregion
}
