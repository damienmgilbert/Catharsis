using Catharsis.DataStructures;

namespace Catharsis.UnitTests.DataStructures;

///<summary>
///Unit tests for the <see cref="SkipList{T}"/> class.
///</summary>
[TestClass]
public class SkipListTests
{
    #region Construction

    [TestMethod]
    public void Constructor_Default_IsEmpty()
    {
        SkipList<int> list = new();
        Assert.AreEqual(0, list.Count);
    }

    [TestMethod]
    public void Constructor_CustomComparer_OrdersAccordingly()
    {
        SkipList<int> list = new(Comparer<int>.Create(static (a, b) => b.CompareTo(a)));
        list.Add(1);
        list.Add(3);
        list.Add(2);

        CollectionAssert.AreEqual(new[] { 3, 2, 1 }, list.ToList());
    }

    [TestMethod]
    public void Constructor_DeterministicRandom_IsReproducible()
    {
        SkipList<int> list1 = new(null, new Random(42));
        SkipList<int> list2 = new(null, new Random(42));

        for(int i = 0; i < 50; i++)
        {
            list1.Add(i);
            list2.Add(i);
        }

        CollectionAssert.AreEqual(list1.ToList(), list2.ToList());
    }

    #endregion

    #region Add / enumeration order

    [TestMethod]
    public void Add_IncrementsCount()
    {
        SkipList<int> list = new();
        list.Add(1);
        list.Add(2);
        Assert.AreEqual(2, list.Count);
    }

    [TestMethod]
    public void GetEnumerator_YieldsValuesInAscendingOrder()
    {
        SkipList<int> list = new();

        foreach(int value in new[] { 5, 3, 8, 1, 9, 2 })
        {
            list.Add(value);
        }

        CollectionAssert.AreEqual(new[] { 1, 2, 3, 5, 8, 9 }, list.ToList());
    }

    [TestMethod]
    public void Add_DuplicateValues_AreAllPreserved()
    {
        SkipList<int> list = new();
        list.Add(1);
        list.Add(1);
        list.Add(1);

        Assert.AreEqual(3, list.Count);
        CollectionAssert.AreEqual(new[] { 1, 1, 1 }, list.ToList());
    }

    #endregion

    #region Contains

    [TestMethod]
    public void Contains_ExistingValue_ReturnsTrue()
    {
        SkipList<int> list = new();
        list.Add(5);
        Assert.IsTrue(list.Contains(5));
    }

    [TestMethod]
    public void Contains_MissingValue_ReturnsFalse()
    {
        SkipList<int> list = new();
        list.Add(5);
        Assert.IsFalse(list.Contains(99));
    }

    [TestMethod]
    public void Contains_EmptyList_ReturnsFalse()
    {
        SkipList<int> list = new();
        Assert.IsFalse(list.Contains(1));
    }

    #endregion

    #region Remove

    [TestMethod]
    public void Remove_ExistingValue_ReturnsTrueAndDecrementsCount()
    {
        SkipList<int> list = new();
        list.Add(5);
        list.Add(3);

        Assert.IsTrue(list.Remove(5));
        Assert.AreEqual(1, list.Count);
        Assert.IsFalse(list.Contains(5));
    }

    [TestMethod]
    public void Remove_MissingValue_ReturnsFalse()
    {
        SkipList<int> list = new();
        list.Add(5);
        Assert.IsFalse(list.Remove(99));
        Assert.AreEqual(1, list.Count);
    }

    [TestMethod]
    public void Remove_OneOfSeveralDuplicates_RemovesOnlyOne()
    {
        SkipList<int> list = new();
        list.Add(1);
        list.Add(1);

        Assert.IsTrue(list.Remove(1));
        Assert.AreEqual(1, list.Count);
        Assert.IsTrue(list.Contains(1));
    }

    [TestMethod]
    public void Remove_AllValues_LeavesListEmpty()
    {
        SkipList<int> list = new();

        foreach(int value in new[] { 5, 3, 8, 1, 9 })
        {
            list.Add(value);
        }

        foreach(int value in new[] { 5, 3, 8, 1, 9 })
        {
            Assert.IsTrue(list.Remove(value));
        }

        Assert.AreEqual(0, list.Count);
        Assert.IsEmpty(list.ToList());
    }

    #endregion

    #region Clear

    [TestMethod]
    public void Clear_RemovesAllValues()
    {
        SkipList<int> list = new();
        list.Add(1);
        list.Add(2);
        list.Clear();

        Assert.AreEqual(0, list.Count);
        Assert.IsFalse(list.Contains(1));
    }

    [TestMethod]
    public void Clear_ThenAdd_WorksCorrectly()
    {
        SkipList<int> list = new();
        list.Add(1);
        list.Clear();
        list.Add(2);

        Assert.AreEqual(1, list.Count);
        Assert.IsTrue(list.Contains(2));
    }

    #endregion

    #region Randomized stress test

    [TestMethod]
    public void RandomizedInsertions_EnumerationMatchesSortedReference()
    {
        Random random = new(2024);
        List<int> reference = [];
        SkipList<int> list = new();

        for(int i = 0; i < 1000; i++)
        {
            int value = random.Next(-5000, 5000);
            reference.Add(value);
            list.Add(value);
        }

        CollectionAssert.AreEqual(reference.OrderBy(static v => v).ToList(), list.ToList());
        Assert.AreEqual(reference.Count, list.Count);
    }

    [TestMethod]
    public void RandomizedMixedAddAndRemove_StaysConsistentWithReference()
    {
        Random random = new(4242);
        List<int> reference = [];
        SkipList<int> list = new();

        for(int i = 0; i < 1000; i++)
        {
            if(reference.Count == 0 || random.Next(2) == 0)
            {
                int value = random.Next(0, 200);
                reference.Add(value);
                list.Add(value);
            } else
            {
                int index = random.Next(reference.Count);
                int value = reference[index];
                reference.RemoveAt(index);
                Assert.IsTrue(list.Remove(value));
            }

            Assert.AreEqual(reference.Count, list.Count);
        }

        CollectionAssert.AreEqual(reference.OrderBy(static v => v).ToList(), list.ToList());
    }

    #endregion
}
