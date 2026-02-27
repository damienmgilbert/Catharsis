using Catharsis.Common;

namespace Catharsis.UnitTests.Common;

[TestClass]
public class BufferComparerTests
{
    #region Public methods
    [TestMethod]
    public void Compare_Equal_ReturnsZero()
    {
        int[] a = [ 1, 2, 3 ];
        int[] b = [ 1, 2, 3 ];
        Assert.AreEqual(0, BufferComparer<int>.Default.Compare(a, b));
    }

    [TestMethod]
    public void Compare_LessThan_ReturnsNegative()
    {
        int[] a = [ 1, 2 ];
        int[] b = [ 1, 3 ];
        Assert.IsTrue(BufferComparer<int>.Default.Compare(a, b) < 0);
    }

    [TestMethod]
    public void Compare_NullHandling()
    {
        Assert.AreEqual(0, BufferComparer<int>.Default.Compare(null, null));
        Assert.IsTrue(BufferComparer<int>.Default.Compare(null, [ 1 ]) < 0);
        Assert.IsTrue(BufferComparer<int>.Default.Compare([ 1 ], null) > 0);
    }

    [TestMethod]
    public void Compare_ShorterArray_ReturnsNegative()
    {
        int[] a = [ 1, 2 ];
        int[] b = [ 1, 2, 3 ];
        Assert.IsTrue(BufferComparer<int>.Default.Compare(a, b) < 0);
    }

    [TestMethod]
    public void Equals_DifferentArrays_ReturnsFalse()
    {
        int[] a = [ 1, 2, 3 ];
        int[] b = [ 1, 2, 4 ];
        Assert.IsFalse(BufferComparer<int>.Default.Equals(a, b));
    }

    [TestMethod]
    public void Equals_NullArrays_HandlesCorrectly()
    {
        Assert.IsTrue(BufferComparer<int>.Default.Equals(null, null));
        Assert.IsFalse(BufferComparer<int>.Default.Equals([ 1 ], null));
        Assert.IsFalse(BufferComparer<int>.Default.Equals(null, [ 1 ]));
    }

    [TestMethod]
    public void Equals_SameArrays_ReturnsTrue()
    {
        int[] a = [ 1, 2, 3 ];
        int[] b = [ 1, 2, 3 ];
        Assert.IsTrue(BufferComparer<int>.Default.Equals(a, b));
    }

    [TestMethod]
    public void Equals_SameReference_ReturnsTrue()
    {
        int[] a = [ 1, 2, 3 ];
        Assert.IsTrue(BufferComparer<int>.Default.Equals(a, a));
    }

    [TestMethod]
    public void GetHashCode_EqualArrays_SameHash()
    {
        int[] a = [ 1, 2, 3 ];
        int[] b = [ 1, 2, 3 ];
        Assert.AreEqual(BufferComparer<int>.Default.GetHashCode(a), BufferComparer<int>.Default.GetHashCode(b));
    }

    [TestMethod]
    public void Static_Equals_Span_ReturnsCorrectResult()
    {
        ReadOnlySpan<int> a = [ 1, 2, 3 ];
        ReadOnlySpan<int> b = [ 1, 2, 3 ];
        Assert.IsTrue(BufferComparer<int>.Equals(a, b));
    }
    #endregion
}
