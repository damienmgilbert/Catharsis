using Catharsis.Diagnostics;
using System.Buffers;

namespace Catharsis.UnitTests.Diagnostics;

[TestClass]
public class ValidatedBufferWriterTests
{
    #region Public methods
    [TestMethod]
    public void Advance_NegativeCount_Throws()
    {
        ArrayBufferWriter<byte> inner = new ArrayBufferWriter<byte>();
        ValidatedBufferWriter<byte> writer = new ValidatedBufferWriter<byte>(inner, ValidationMode.Full);
        writer.GetSpan(10);
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => writer.Advance(-1));
    }

    [TestMethod]
    public void Advance_PastSpanSize_Throws()
    {
        ArrayBufferWriter<byte> inner = new ArrayBufferWriter<byte>();
        ValidatedBufferWriter<byte> writer = new ValidatedBufferWriter<byte>(inner, ValidationMode.Full);
        writer.GetSpan(5);
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => writer.Advance(99999));
    }

    [TestMethod]
    public void Constructor_SetsMode()
    {
        ArrayBufferWriter<byte> inner = new ArrayBufferWriter<byte>();
        ValidatedBufferWriter<byte> writer = new ValidatedBufferWriter<byte>(inner, ValidationMode.Full);
        Assert.AreEqual(ValidationMode.Full, writer.Mode);
        Assert.AreEqual(0L, writer.TotalAdvanced);
    }

    [TestMethod]
    public void GetMemory_ReturnsNonEmptyMemory()
    {
        ArrayBufferWriter<byte> inner = new ArrayBufferWriter<byte>();
        ValidatedBufferWriter<byte> writer = new ValidatedBufferWriter<byte>(inner, ValidationMode.Full);

        Memory<byte> mem = writer.GetMemory(10);
        Assert.IsTrue(mem.Length >= 10);
    }

    [TestMethod]
    public void GetSpan_And_Advance_TracksTotal()
    {
        ArrayBufferWriter<byte> inner = new ArrayBufferWriter<byte>();
        ValidatedBufferWriter<byte> writer = new ValidatedBufferWriter<byte>(inner, ValidationMode.Full);

        Span<byte> span = writer.GetSpan(5);
        span[0] = 1;
        writer.Advance(1);

        Assert.AreEqual(1L, writer.TotalAdvanced);
    }

    [TestMethod]
    public void NoneMode_SkipsValidation()
    {
        ArrayBufferWriter<byte> inner = new ArrayBufferWriter<byte>();
        ValidatedBufferWriter<byte> writer = new ValidatedBufferWriter<byte>(inner, ValidationMode.None);
        writer.GetSpan(5);
        writer.Advance(3);
        Assert.AreEqual(3L, writer.TotalAdvanced);
    }
    #endregion
}
