using Catharsis.Diagnostics;

namespace Catharsis.UnitTests.Diagnostics;

[TestClass]
public class CheckedSpanTests
{
    #region Public methods
    [TestMethod]
    public void AsSpan_ReturnsUnderlyingSpan()
    {
        byte[] data = [ 1, 2, 3 ];
        CheckedSpan<byte> span = new CheckedSpan<byte>(data);
        Span<byte> raw = span.AsSpan();
        Assert.AreEqual(3, raw.Length);
    }

    [TestMethod]
    public void Clear_ClearsToDefault()
    {
        byte[] data = [ 1, 2, 3 ];
        CheckedSpan<byte> span = new CheckedSpan<byte>(data);
        span.Clear();
        Assert.AreEqual(0, data[0]);
    }

    [TestMethod]
    public void CopyTo_CopiesData()
    {
        byte[] src = [ 1, 2, 3 ];
        CheckedSpan<byte> span = new CheckedSpan<byte>(src, ValidationMode.Full);
        byte[] dest = new byte[5];
        span.CopyTo(dest);
        Assert.AreEqual(1, dest[0]);
        Assert.AreEqual(3, dest[2]);
    }

    [TestMethod]
    public void Fill_FillsWithValue()
    {
        byte[] data = new byte[3];
        CheckedSpan<byte> span = new CheckedSpan<byte>(data);
        span.Fill(0xFF);
        Assert.AreEqual(0xFF, data[0]);
        Assert.AreEqual(0xFF, data[2]);
    }

    [TestMethod]
    public void Indexer_ValidIndex_ReturnsElement()
    {
        byte[] data = [ 1, 2, 3 ];
        CheckedSpan<byte> span = new CheckedSpan<byte>(data, ValidationMode.Full);
        Assert.AreEqual(2, span[1]);
    }

    [TestMethod]
    public void IsEmpty_EmptySpan_ReturnsTrue()
    {
        CheckedSpan<byte> span = new CheckedSpan<byte>(Span<byte>.Empty);
        Assert.IsTrue(span.IsEmpty);
    }

    [TestMethod]
    public void Length_ReturnsSpanLength()
    {
        byte[] data = new byte[10];
        CheckedSpan<byte> span = new CheckedSpan<byte>(data);
        Assert.AreEqual(10, span.Length);
        Assert.IsFalse(span.IsEmpty);
    }

    [TestMethod]
    public void Slice_ReturnsCheckedSlice()
    {
        byte[] data = [ 10, 20, 30, 40, 50 ];
        CheckedSpan<byte> span = new CheckedSpan<byte>(data, ValidationMode.Full);
        CheckedSpan<byte> slice = span.Slice(1, 3);
        Assert.AreEqual(3, slice.Length);
        Assert.AreEqual(20, slice[0]);
    }
    #endregion
}
