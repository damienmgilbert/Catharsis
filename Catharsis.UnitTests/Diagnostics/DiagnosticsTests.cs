using Catharsis.Buffers;
using Catharsis.Services;
using System.Buffers;

namespace Catharsis.Diagnostics.UnitTests;

[TestClass]
public class ValidatedBufferWriterTests
{
    [TestMethod]
    public void Constructor_SetsMode()
    {
        var inner = new ArrayBufferWriter<byte>();
        var writer = new ValidatedBufferWriter<byte>(inner, ValidationMode.Full);
        Assert.AreEqual(ValidationMode.Full, writer.Mode);
        Assert.AreEqual(0L, writer.TotalAdvanced);
    }

    [TestMethod]
    public void GetSpan_And_Advance_TracksTotal()
    {
        var inner = new ArrayBufferWriter<byte>();
        var writer = new ValidatedBufferWriter<byte>(inner, ValidationMode.Full);

        Span<byte> span = writer.GetSpan(5);
        span[0] = 1;
        writer.Advance(1);

        Assert.AreEqual(1L, writer.TotalAdvanced);
    }

    [TestMethod]
    public void GetMemory_ReturnsNonEmptyMemory()
    {
        var inner = new ArrayBufferWriter<byte>();
        var writer = new ValidatedBufferWriter<byte>(inner, ValidationMode.Full);

        Memory<byte> mem = writer.GetMemory(10);
        Assert.IsTrue(mem.Length >= 10);
    }

    [TestMethod]
    public void Advance_NegativeCount_Throws()
    {
        var inner = new ArrayBufferWriter<byte>();
        var writer = new ValidatedBufferWriter<byte>(inner, ValidationMode.Full);
        writer.GetSpan(10);
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => writer.Advance(-1));
    }

    [TestMethod]
    public void Advance_PastSpanSize_Throws()
    {
        var inner = new ArrayBufferWriter<byte>();
        var writer = new ValidatedBufferWriter<byte>(inner, ValidationMode.Full);
        writer.GetSpan(5);
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => writer.Advance(99999));
    }

    [TestMethod]
    public void NoneMode_SkipsValidation()
    {
        var inner = new ArrayBufferWriter<byte>();
        var writer = new ValidatedBufferWriter<byte>(inner, ValidationMode.None);
        writer.GetSpan(5);
        writer.Advance(3);
        Assert.AreEqual(3L, writer.TotalAdvanced);
    }
}

[TestClass]
public class CheckedSpanTests
{
    [TestMethod]
    public void Indexer_ValidIndex_ReturnsElement()
    {
        byte[] data = [1, 2, 3];
        var span = new CheckedSpan<byte>(data, ValidationMode.Full);
        Assert.AreEqual(2, span[1]);
    }

    [TestMethod]
    public void Length_ReturnsSpanLength()
    {
        byte[] data = new byte[10];
        var span = new CheckedSpan<byte>(data);
        Assert.AreEqual(10, span.Length);
        Assert.IsFalse(span.IsEmpty);
    }

    [TestMethod]
    public void IsEmpty_EmptySpan_ReturnsTrue()
    {
        var span = new CheckedSpan<byte>(Span<byte>.Empty);
        Assert.IsTrue(span.IsEmpty);
    }

    [TestMethod]
    public void Slice_ReturnsCheckedSlice()
    {
        byte[] data = [10, 20, 30, 40, 50];
        var span = new CheckedSpan<byte>(data, ValidationMode.Full);
        var slice = span.Slice(1, 3);
        Assert.AreEqual(3, slice.Length);
        Assert.AreEqual(20, slice[0]);
    }

    [TestMethod]
    public void CopyTo_CopiesData()
    {
        byte[] src = [1, 2, 3];
        var span = new CheckedSpan<byte>(src, ValidationMode.Full);
        byte[] dest = new byte[5];
        span.CopyTo(dest);
        Assert.AreEqual(1, dest[0]);
        Assert.AreEqual(3, dest[2]);
    }

    [TestMethod]
    public void Fill_FillsWithValue()
    {
        byte[] data = new byte[3];
        var span = new CheckedSpan<byte>(data);
        span.Fill(0xFF);
        Assert.AreEqual(0xFF, data[0]);
        Assert.AreEqual(0xFF, data[2]);
    }

    [TestMethod]
    public void Clear_ClearsToDefault()
    {
        byte[] data = [1, 2, 3];
        var span = new CheckedSpan<byte>(data);
        span.Clear();
        Assert.AreEqual(0, data[0]);
    }

    [TestMethod]
    public void AsSpan_ReturnsUnderlyingSpan()
    {
        byte[] data = [1, 2, 3];
        var span = new CheckedSpan<byte>(data);
        Span<byte> raw = span.AsSpan();
        Assert.AreEqual(3, raw.Length);
    }
}

[TestClass]
public class SafeSequenceParserTests
{
    private sealed class TestParser : ISequenceParser
    {
        public SequenceParseStatus ResultToReturn { get; set; } = SequenceParseStatus.Success;
        public bool ThrowOnParse { get; set; }

        public SequenceParseStatus TryParse(in ReadOnlySequence<byte> sequence, out SequencePosition consumed, out SequencePosition examined)
        {
            if (ThrowOnParse) throw new InvalidOperationException("Parse error");
            consumed = sequence.End;
            examined = sequence.End;
            return ResultToReturn;
        }

        public void Reset() { }
    }

    [TestMethod]
    public void TryParse_Success_IncrementsSuccessCount()
    {
        var inner = new TestParser { ResultToReturn = SequenceParseStatus.Success };
        var parser = new SafeSequenceParser(inner);
        var seq = new ReadOnlySequence<byte>(new byte[] { 1, 2, 3 });

        var status = parser.TryParse(in seq, out _, out _);

        Assert.AreEqual(SequenceParseStatus.Success, status);
        Assert.AreEqual(1, parser.ParseAttempts);
        Assert.AreEqual(1, parser.SuccessCount);
    }

    [TestMethod]
    public void TryParse_InvalidData_IncrementsFailureCount()
    {
        var inner = new TestParser { ResultToReturn = SequenceParseStatus.InvalidData };
        var parser = new SafeSequenceParser(inner);
        var seq = new ReadOnlySequence<byte>(new byte[] { 1 });

        parser.TryParse(in seq, out _, out _);

        Assert.AreEqual(1, parser.FailureCount);
    }

    [TestMethod]
    public void TryParse_EmptySequence_FullMode_ReturnsNeedMoreData()
    {
        var inner = new TestParser();
        var parser = new SafeSequenceParser(inner, ValidationMode.Full);
        var seq = ReadOnlySequence<byte>.Empty;

        var status = parser.TryParse(in seq, out _, out _);

        Assert.AreEqual(SequenceParseStatus.NeedMoreData, status);
        Assert.AreEqual(1, parser.FailureCount);
    }

    [TestMethod]
    public void TryParse_InnerThrows_ReturnsInvalidData()
    {
        var inner = new TestParser { ThrowOnParse = true };
        var parser = new SafeSequenceParser(inner);
        var seq = new ReadOnlySequence<byte>(new byte[] { 1 });

        var status = parser.TryParse(in seq, out _, out _);

        Assert.AreEqual(SequenceParseStatus.InvalidData, status);
        Assert.AreEqual(1, parser.FailureCount);
    }

    [TestMethod]
    public void Reset_ClearsStatistics()
    {
        var inner = new TestParser { ResultToReturn = SequenceParseStatus.Success };
        var parser = new SafeSequenceParser(inner);
        var seq = new ReadOnlySequence<byte>(new byte[] { 1 });
        parser.TryParse(in seq, out _, out _);

        parser.Reset();

        Assert.AreEqual(0, parser.ParseAttempts);
        Assert.AreEqual(0, parser.SuccessCount);
        Assert.AreEqual(0, parser.FailureCount);
    }
}
