using Catharsis.Buffers;
using Catharsis.Patterns.Composed;
using Catharsis.Services;
using Microsoft.Extensions.Logging;
using System.Buffers;

namespace Catharsis.UnitTests.Patterns.Composed;

///<summary>
///Unit tests for <see cref="CachingParserDecorator"/>.
///</summary>
[TestClass]
public class CachingParserDecoratorTests
{
    // Success once a newline is present (consuming through it); otherwise NeedMoreData, examining everything.
    sealed class LineParser : ISequenceParser
    {
        public int Calls { get; private set; }

        public int Resets { get; private set; }

        public SequenceParseStatus Status { get; set; } = SequenceParseStatus.Success;

        public void Reset() => Resets++;

        public SequenceParseStatus TryParse(in ReadOnlySequence<byte> sequence, out SequencePosition consumed, out SequencePosition examined)
        {
            Calls++;

            if(Status == SequenceParseStatus.Cancelled)
            {
                consumed = sequence.Start;
                examined = sequence.Start;
                return SequenceParseStatus.Cancelled;
            }

            SequencePosition? newline = sequence.PositionOf((byte)'\n');

            if(newline is null)
            {
                consumed = sequence.Start;
                examined = sequence.End;
                return SequenceParseStatus.NeedMoreData;
            }

            consumed = sequence.GetPosition(1, newline.Value);
            examined = consumed;
            return SequenceParseStatus.Success;
        }
    }

    sealed class Segment : ReadOnlySequenceSegment<byte>
    {
        public Segment(byte[] bytes) { Memory = bytes; }

        public Segment Append(byte[] bytes)
        {
            Segment next = new(bytes) { RunningIndex = RunningIndex + Memory.Length };
            Next = next;
            return next;
        }
    }

    sealed class ListLogger : ILogger
    {
        public List<string> Messages { get; } = [];

        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) => Messages.Add(formatter(state, exception));
    }

    static ReadOnlySequence<byte> Bytes(string text) => new(System.Text.Encoding.ASCII.GetBytes(text));

    [TestMethod]
    public void Constructor_InvalidArguments_Throw()
    {
        Assert.ThrowsExactly<ArgumentNullException>(static () => new CachingParserDecorator(null!));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(static () => new CachingParserDecorator(new LineParser(), 0));
    }

    [TestMethod]
    public void TryParse_SameContentTwice_RunsInnerOnce()
    {
        LineParser inner = new();
        CachingParserDecorator parser = new(inner);
        ReadOnlySequence<byte> input = Bytes("abc\ndef");

        SequenceParseStatus first = parser.TryParse(in input, out SequencePosition firstConsumed, out _);
        SequenceParseStatus second = parser.TryParse(in input, out SequencePosition secondConsumed, out _);

        Assert.AreEqual(SequenceParseStatus.Success, first);
        Assert.AreEqual(first, second);
        Assert.AreEqual(1, inner.Calls);
        Assert.AreEqual(1, parser.Hits);
        Assert.AreEqual(1, parser.Misses);
        Assert.AreEqual(4, input.Slice(input.Start, firstConsumed).Length);
        Assert.AreEqual(4, input.Slice(input.Start, secondConsumed).Length);
    }

    [TestMethod]
    public void TryParse_DifferentContent_MissesEachTime()
    {
        LineParser inner = new();
        CachingParserDecorator parser = new(inner);
        ReadOnlySequence<byte> a = Bytes("a\n");
        ReadOnlySequence<byte> b = Bytes("b\n");

        parser.TryParse(in a, out _, out _);
        parser.TryParse(in b, out _, out _);

        Assert.AreEqual(2, inner.Calls);
        Assert.AreEqual(0, parser.Hits);
    }

    [TestMethod]
    public void TryParse_ExaminedPositionIsRestoredOnHit()
    {
        CachingParserDecorator parser = new(new LineParser());
        ReadOnlySequence<byte> input = Bytes("no newline");

        parser.TryParse(in input, out _, out _);
        SequenceParseStatus status = parser.TryParse(in input, out SequencePosition consumed, out SequencePosition examined);

        Assert.AreEqual(SequenceParseStatus.NeedMoreData, status);
        Assert.AreEqual(0, input.Slice(input.Start, consumed).Length);
        Assert.AreEqual(input.Length, input.Slice(input.Start, examined).Length);
    }

    [TestMethod]
    public void TryParse_MultiSegmentAndContiguousWithSameBytes_ShareAnEntry()
    {
        LineParser inner = new();
        CachingParserDecorator parser = new(inner);
        Segment first = new([(byte)'a', (byte)'b']);
        Segment last = first.Append([(byte)'\n']);
        ReadOnlySequence<byte> split = new(first, 0, last, last.Memory.Length);
        ReadOnlySequence<byte> contiguous = Bytes("ab\n");

        parser.TryParse(in split, out _, out _);
        parser.TryParse(in contiguous, out SequencePosition consumed, out _);

        Assert.AreEqual(1, inner.Calls);
        Assert.AreEqual(3, contiguous.Slice(contiguous.Start, consumed).Length);
    }

    [TestMethod]
    public void TryParse_CancelledResult_IsNotCached()
    {
        LineParser inner = new() { Status = SequenceParseStatus.Cancelled };
        CachingParserDecorator parser = new(inner);
        ReadOnlySequence<byte> input = Bytes("x\n");

        parser.TryParse(in input, out _, out _);
        inner.Status = SequenceParseStatus.Success;
        SequenceParseStatus status = parser.TryParse(in input, out _, out _);

        Assert.AreEqual(SequenceParseStatus.Success, status);
        Assert.AreEqual(2, inner.Calls);
    }

    [TestMethod]
    public void TryParse_OversizedInput_BypassesCache()
    {
        LineParser inner = new();
        CachingParserDecorator parser = new(inner);
        ReadOnlySequence<byte> big = new(new byte[CachingParserDecorator.MaxCacheableLength + 1]);

        parser.TryParse(in big, out _, out _);
        parser.TryParse(in big, out _, out _);

        Assert.AreEqual(2, inner.Calls);
        Assert.AreEqual(0, parser.Misses);
    }

    [TestMethod]
    public void TryParse_CapacityExceeded_EvictsLeastRecentlyUsed()
    {
        LineParser inner = new();
        CachingParserDecorator parser = new(inner, capacity: 2);
        ReadOnlySequence<byte> a = Bytes("a\n");
        ReadOnlySequence<byte> b = Bytes("b\n");
        ReadOnlySequence<byte> c = Bytes("c\n");

        parser.TryParse(in a, out _, out _);
        parser.TryParse(in b, out _, out _);
        parser.TryParse(in c, out _, out _);
        parser.TryParse(in a, out _, out _);

        Assert.AreEqual(4, inner.Calls);
    }

    [TestMethod]
    public void Reset_DelegatesToInner_AndKeepsCache()
    {
        LineParser inner = new();
        CachingParserDecorator parser = new(inner);
        ReadOnlySequence<byte> input = Bytes("a\n");

        parser.TryParse(in input, out _, out _);
        parser.Reset();
        parser.TryParse(in input, out _, out _);

        Assert.AreEqual(1, inner.Resets);
        Assert.AreEqual(1, inner.Calls);
    }

    [TestMethod]
    public void Logger_ReceivesHitAndMissEntries()
    {
        ListLogger logger = new();
        CachingParserDecorator parser = new(new LineParser(), logger: logger);
        ReadOnlySequence<byte> input = Bytes("ab\n");

        parser.TryParse(in input, out _, out _);
        parser.TryParse(in input, out _, out _);

        Assert.AreEqual(2, logger.Messages.Count);
        StringAssert.Contains(logger.Messages[0], "miss");
        StringAssert.Contains(logger.Messages[1], "hit");
    }
}
