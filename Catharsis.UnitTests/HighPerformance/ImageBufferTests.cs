using Catharsis.HighPerformance;

namespace Catharsis.UnitTests.HighPerformance;

[TestClass]
public class ImageBufferTests
{
    #region Public methods
    [TestMethod]
    public void Constructor_SetsWidthAndHeight()
    {
        using ImageBuffer<int> buf = new ImageBuffer<int>(10, 20);
        Assert.AreEqual(10, buf.Width);
        Assert.AreEqual(20, buf.Height);
        Assert.AreEqual(200, buf.PixelCount);
    }

    [TestMethod]
    public void Dispose_IsIdempotent()
    {
        ImageBuffer<int> buf = new ImageBuffer<int>(2, 2);
        buf.Dispose();
        buf.Dispose();
    }

    [TestMethod]
    public void GetPixelSpan_ReturnsAllPixels()
    {
        using ImageBuffer<int> buf = new ImageBuffer<int>(3, 2);
        Span<int> span = buf.GetPixelSpan();
        Assert.AreEqual(6, span.Length);
    }

    [TestMethod]
    public void GetRowSpan_ReturnsCorrectRow()
    {
        using ImageBuffer<int> buf = new ImageBuffer<int>(3, 3);
        buf[0, 1] = 42;
        Span<int> row = buf.GetRowSpan(1);
        Assert.AreEqual(3, row.Length);
        Assert.AreEqual(42, row[0]);
    }

    [TestMethod]
    public void Indexer_SetAndGetPixel()
    {
        using ImageBuffer<byte> buf = new ImageBuffer<byte>(5, 5);
        buf[2, 3] = 0xFF;
        Assert.AreEqual(0xFF, buf[2, 3]);
    }
    #endregion
}
