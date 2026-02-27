using Catharsis.Immutable;
using System.Collections.Immutable;

namespace Catharsis.UnitTests.Immutable;

[TestClass]
public class ImmutableBufferTests
{
    #region Public methods
    [TestMethod]
    public void Constructor_FromImmutableArray()
    {
        ImmutableArray<int> arr = ImmutableArray.Create(10, 20);
        ImmutableBuffer<int> buf = new ImmutableBuffer<int>(arr);
        Assert.AreEqual(2, buf.Count);
    }

    [TestMethod]
    public void Constructor_FromSpan_CopiesData()
    {
        ReadOnlySpan<int> data = [1, 2, 3];
        ImmutableBuffer<int> buf = new ImmutableBuffer<int>(data);
        Assert.AreEqual(3, buf.Count);
        Assert.AreEqual(1, buf[0]);
        Assert.AreEqual(3, buf[2]);
    }

    [TestMethod]
    public void Empty_ReturnsEmptyBuffer()
    {
        ImmutableBuffer<int> buf = ImmutableBuffer<int>.Empty;
        Assert.AreEqual(0, buf.Count);
        Assert.IsTrue(buf.IsEmpty);
    }

    [TestMethod]
    public void Enumeration_ReturnsAllElements()
    {
        ImmutableBuffer<int> buf = ImmutableBuffer<int>.Create([1, 2, 3]);
        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, buf.ToList());
    }

    [TestMethod]
    public void Equals_DifferentData_ReturnsFalse()
    {
        ImmutableBuffer<int> a = ImmutableBuffer<int>.Create([1, 2]);
        ImmutableBuffer<int> b = ImmutableBuffer<int>.Create([1, 3]);
        Assert.IsFalse(a.Equals(b));
        Assert.IsTrue(a != b);
    }

    [TestMethod]
    public void Equals_SameData_ReturnsTrue()
    {
        ImmutableBuffer<int> a = ImmutableBuffer<int>.Create([1, 2, 3]);
        ImmutableBuffer<int> b = ImmutableBuffer<int>.Create([1, 2, 3]);
        Assert.IsTrue(a.Equals(b));
        Assert.IsTrue(a == b);
    }

    [TestMethod]
    public void GetHashCode_EqualBuffers_SameHash()
    {
        ImmutableBuffer<int> a = ImmutableBuffer<int>.Create([1, 2]);
        ImmutableBuffer<int> b = ImmutableBuffer<int>.Create([1, 2]);
        Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
    }

    [TestMethod]
    public void Slice_ReturnsSubset()
    {
        ImmutableBuffer<int> buf = ImmutableBuffer<int>.Create([10, 20, 30, 40, 50]);
        ImmutableBuffer<int> sliced = buf.Slice(1, 3);
        Assert.AreEqual(3, sliced.Count);
        Assert.AreEqual(20, sliced[0]);
        Assert.AreEqual(40, sliced[2]);
    }

    [TestMethod]
    public void Span_ReturnsReadOnlySpan()
    {
        ImmutableBuffer<int> buf = ImmutableBuffer<int>.Create([5, 10]);
        ReadOnlySpan<int> span = buf.Span;
        Assert.AreEqual(2, span.Length);
        Assert.AreEqual(5, span[0]);
    }
    #endregion
}
