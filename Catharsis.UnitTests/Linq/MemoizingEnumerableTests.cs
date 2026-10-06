using Catharsis.Linq;

namespace Catharsis.UnitTests.Linq;

///<summary>
///Unit tests for the <see cref="MemoizingEnumerable{T}"/> class.
///</summary>
[TestClass]
public class MemoizingEnumerableTests
{
    #region Construction

    [TestMethod]
    public void Constructor_NullSource_Throws()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => new MemoizingEnumerable<int>(null!));
    }

    #endregion

    #region Enumeration

    [TestMethod]
    public void GetEnumerator_YieldsSourceElements()
    {
        using MemoizingEnumerable<int> memoized = new([1, 2, 3]);
        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, memoized.ToList());
    }

    [TestMethod]
    public void GetEnumerator_MultipleEnumerations_OnlyEnumeratesSourceOnce()
    {
        int callCount = 0;

        IEnumerable<int> Source()
        {
            foreach (int value in new[] { 1, 2, 3 })
            {
                callCount++;
                yield return value;
            }
        }

        using MemoizingEnumerable<int> memoized = new(Source());

        List<int> first = [.. memoized];
        List<int> second = [.. memoized];

        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, first);
        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, second);
        Assert.AreEqual(3, callCount);
    }

    [TestMethod]
    public void GetEnumerator_PartialThenFullEnumeration_ResumesFromCache()
    {
        using MemoizingEnumerable<int> memoized = new([1, 2, 3, 4, 5]);

        List<int> partial = [.. memoized.Take(2)];
        List<int> full = [.. memoized];

        CollectionAssert.AreEqual(new[] { 1, 2 }, partial);
        CollectionAssert.AreEqual(new[] { 1, 2, 3, 4, 5 }, full);
    }

    [TestMethod]
    public void GetEnumerator_InterleavedEnumerators_BothCompleteCorrectly()
    {
        using MemoizingEnumerable<int> memoized = new([1, 2, 3]);

        using IEnumerator<int> enumeratorA = memoized.GetEnumerator();
        using IEnumerator<int> enumeratorB = memoized.GetEnumerator();

        Assert.IsTrue(enumeratorA.MoveNext());
        Assert.AreEqual(1, enumeratorA.Current);

        Assert.IsTrue(enumeratorB.MoveNext());
        Assert.AreEqual(1, enumeratorB.Current);

        Assert.IsTrue(enumeratorA.MoveNext());
        Assert.AreEqual(2, enumeratorA.Current);

        Assert.IsTrue(enumeratorB.MoveNext());
        Assert.AreEqual(2, enumeratorB.Current);
        Assert.IsTrue(enumeratorB.MoveNext());
        Assert.AreEqual(3, enumeratorB.Current);
        Assert.IsFalse(enumeratorB.MoveNext());

        Assert.IsTrue(enumeratorA.MoveNext());
        Assert.AreEqual(3, enumeratorA.Current);
        Assert.IsFalse(enumeratorA.MoveNext());
    }

    [TestMethod]
    public void GetEnumerator_EmptySource_YieldsNothing()
    {
        using MemoizingEnumerable<int> memoized = new([]);
        Assert.IsEmpty(memoized.ToList());
    }

    #endregion

    #region Dispose

    [TestMethod]
    public void Dispose_IsIdempotent()
    {
        MemoizingEnumerable<int> memoized = new([1, 2, 3]);
        memoized.Dispose();
        memoized.Dispose();
    }

    [TestMethod]
    public void Dispose_AfterPartialEnumeration_CachedItemsStillEnumerable()
    {
        MemoizingEnumerable<int> memoized = new([1, 2, 3]);
        List<int> partial = [.. memoized.Take(2)];

        memoized.Dispose();

        CollectionAssert.AreEqual(new[] { 1, 2 }, partial);
        CollectionAssert.AreEqual(new[] { 1, 2 }, memoized.ToList());
    }

    #endregion
}
