using Catharsis.HighPerformance;

namespace Catharsis.UnitTests.HighPerformance;

[TestClass]
public class BitSpanTests
{
    #region Public methods
    [TestMethod]
    public void Clear_ClearsAllBits()
    {
        byte[] storage = new byte[2];
        BitSpan bits = new BitSpan(storage, 16);
        bits.Fill(true);
        bits.Clear();
        Assert.IsFalse(bits[0]);
    }

    [TestMethod]
    public void Fill_True_SetsAllBits()
    {
        byte[] storage = new byte[2];
        BitSpan bits = new BitSpan(storage, 16);
        bits.Fill(true);
        Assert.IsTrue(bits[0]);
        Assert.IsTrue(bits[15]);
    }

    [TestMethod]
    public void GetByteCount_ReturnsCorrectByteCount()
    {
        Assert.AreEqual(1, BitSpan.GetByteCount(1));
        Assert.AreEqual(1, BitSpan.GetByteCount(8));
        Assert.AreEqual(2, BitSpan.GetByteCount(9));
    }

    [TestMethod]
    public void Indexer_SetAndGetBit()
    {
        byte[] storage = new byte[2];
        BitSpan bits = new BitSpan(storage, 16);

        bits[0] = true;
        bits[7] = true;
        bits[8] = true;

        Assert.IsTrue(bits[0]);
        Assert.IsTrue(bits[7]);
        Assert.IsTrue(bits[8]);
        Assert.IsFalse(bits[1]);
    }

    [TestMethod]
    public void Length_ReturnsCorrectBitCount()
    {
        byte[] storage = new byte[3];
        BitSpan bits = new BitSpan(storage, 20);
        Assert.AreEqual(20, bits.Length);
        Assert.AreEqual(3, bits.ByteLength);
    }

    [TestMethod]
    public void Not_InvertsAllBits()
    {
        byte[] storage = new byte[1];
        BitSpan bits = new BitSpan(storage, 8);
        bits[0] = true;
        bits.Not();
        Assert.IsFalse(bits[0]);
        Assert.IsTrue(bits[1]);
    }

    [TestMethod]
    public void PopCount_CountsSetBits()
    {
        byte[] storage = new byte[1];
        BitSpan bits = new BitSpan(storage, 8);
        bits[0] = true;
        bits[2] = true;
        bits[4] = true;
        Assert.AreEqual(3, bits.PopCount());
    }
    #endregion
}
