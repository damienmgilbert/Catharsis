using Catharsis.Patterns.Composed;

namespace Catharsis.UnitTests.Patterns.Composed;

///<summary>
///Unit tests for <see cref="RoundRobinIterator"/>.
///</summary>
[TestClass]
public class RoundRobinIteratorTests
{
    sealed class TrackedEnumerable(IEnumerable<int> items) : IEnumerable<int>
    {
        public int Disposed { get; private set; }

        // A wrapper rather than an iterator method: disposing an iterator that never started skips its finally block.
        public IEnumerator<int> GetEnumerator() => new Tracker(items.GetEnumerator(), () => Disposed++);

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();

        sealed class Tracker(IEnumerator<int> inner, Action onDispose) : IEnumerator<int>
        {
            public int Current => inner.Current;

            object System.Collections.IEnumerator.Current => Current;

            public bool MoveNext() => inner.MoveNext();

            public void Reset() => inner.Reset();

            public void Dispose()
            {
                inner.Dispose();
                onDispose();
            }
        }
    }

    [TestMethod]
    public void Interleave_EqualLengths_AlternatesSources() { CollectionAssert.AreEqual(new[] { 1, 10, 2, 20, 3, 30 }, RoundRobinIterator.Interleave<int>([1, 2, 3], [10, 20, 30]).ToArray()); }

    [TestMethod]
    public void Interleave_UnequalLengths_LongerSourceFinishesAlone() { CollectionAssert.AreEqual(new[] { 1, 10, 100, 2, 3 }, RoundRobinIterator.Interleave<int>([1, 2, 3], [10], [100]).ToArray()); }

    [TestMethod]
    public void Interleave_NoSources_IsEmpty() { Assert.AreEqual(0, RoundRobinIterator.Interleave<int>().Count()); }

    [TestMethod]
    public void Interleave_NullSource_ThrowsEagerly()
    {
        Assert.ThrowsExactly<ArgumentNullException>(static () => RoundRobinIterator.Interleave<int>([1], null!));
        Assert.ThrowsExactly<ArgumentNullException>(static () => RoundRobinIterator.Interleave<int>(null!));
    }

    [TestMethod]
    public void Interleave_IsLazy()
    {
        TrackedEnumerable tracked = new([1, 2, 3]);

        using IEnumerator<int> enumerator = RoundRobinIterator.Interleave<int>(tracked).GetEnumerator();
        Assert.IsTrue(enumerator.MoveNext());
        Assert.AreEqual(1, enumerator.Current);
        Assert.AreEqual(0, tracked.Disposed);
    }

    [TestMethod]
    public void Interleave_DisposesEverySource_WhenFullyConsumed()
    {
        TrackedEnumerable a = new([1]);
        TrackedEnumerable b = new([2, 3]);

        _ = RoundRobinIterator.Interleave<int>(a, b).ToArray();

        Assert.AreEqual(1, a.Disposed);
        Assert.AreEqual(1, b.Disposed);
    }

    [TestMethod]
    public void Interleave_DisposesEverySource_WhenAbandonedEarly()
    {
        TrackedEnumerable a = new([1, 2]);
        TrackedEnumerable b = new([3, 4]);

        _ = RoundRobinIterator.Interleave<int>(a, b).First();

        Assert.AreEqual(1, a.Disposed);
        Assert.AreEqual(1, b.Disposed);
    }

    [TestMethod]
    public void Cycle_RepeatsInOrder() { CollectionAssert.AreEqual(new[] { 1, 2, 3, 1, 2, 3, 1 }, RoundRobinIterator.Cycle<int>([1, 2, 3]).Take(7).ToArray()); }

    [TestMethod]
    public void Cycle_Empty_TerminatesEmpty() { Assert.AreEqual(0, RoundRobinIterator.Cycle<int>([]).Count()); }

    [TestMethod]
    public void Cycle_Null_Throws() { Assert.ThrowsExactly<ArgumentNullException>(static () => RoundRobinIterator.Cycle<int>(null!)); }
}
