using System.Buffers;
using Catharsis.Buffers;
using Catharsis.Diagnostics;
using Catharsis.Services;

namespace Catharsis.UnitTests.Diagnostics;

///<summary>
///Unit tests for the <see cref="SafeSequenceParser"/> class.
///</summary>
[TestClass]
public class SafeSequenceParserTests
{
    #region Public methods
    [TestMethod]
    public void Reset_ClearsStatistics()
    {
        TestParser inner = new() { ResultToReturn = SequenceParseStatus.Success };
        SafeSequenceParser parser = new(inner);
        ReadOnlySequence<byte> seq = new([1]);
        parser.TryParse(in seq, out _, out _);

        parser.Reset();

        Assert.AreEqual(0, parser.ParseAttempts);
        Assert.AreEqual(0, parser.SuccessCount);
        Assert.AreEqual(0, parser.FailureCount);
    }

    [TestMethod]
    public void TryParse_EmptySequence_FullMode_ReturnsNeedMoreData()
    {
        TestParser inner = new();
        SafeSequenceParser parser = new(inner, ValidationMode.Full);
        ReadOnlySequence<byte> seq = ReadOnlySequence<byte>.Empty;

        SequenceParseStatus status = parser.TryParse(in seq, out _, out _);

        Assert.AreEqual(SequenceParseStatus.NeedMoreData, status);
        Assert.AreEqual(1, parser.FailureCount);
    }

    [TestMethod]
    public void TryParse_InnerThrows_ReturnsInvalidData()
    {
        TestParser inner = new() { ThrowOnParse = true };
        SafeSequenceParser parser = new(inner);
        ReadOnlySequence<byte> seq = new([1]);

        SequenceParseStatus status = parser.TryParse(in seq, out _, out _);

        Assert.AreEqual(SequenceParseStatus.InvalidData, status);
        Assert.AreEqual(1, parser.FailureCount);
    }

    [TestMethod]
    public void TryParse_InvalidData_IncrementsFailureCount()
    {
        TestParser inner = new() { ResultToReturn = SequenceParseStatus.InvalidData };
        SafeSequenceParser parser = new(inner);
        ReadOnlySequence<byte> seq = new([1]);

        parser.TryParse(in seq, out _, out _);

        Assert.AreEqual(1, parser.FailureCount);
    }

    [TestMethod]
    public void TryParse_Success_IncrementsSuccessCount()
    {
        TestParser inner = new() { ResultToReturn = SequenceParseStatus.Success };
        SafeSequenceParser parser = new(inner);
        ReadOnlySequence<byte> seq = new([1, 2, 3]);

        SequenceParseStatus status = parser.TryParse(in seq, out _, out _);

        Assert.AreEqual(SequenceParseStatus.Success, status);
        Assert.AreEqual(1, parser.ParseAttempts);
        Assert.AreEqual(1, parser.SuccessCount);
    }
    #endregion

    sealed class TestParser : ISequenceParser
    {
        #region Public methods
        public void Reset()
        {
        }

        public SequenceParseStatus TryParse(in ReadOnlySequence<byte> sequence, out SequencePosition consumed, out SequencePosition examined)
        {
            if(ThrowOnParse)
            {
                throw new InvalidOperationException("Parse error");
            }

            consumed = sequence.End;
            examined = sequence.End;
            return ResultToReturn;
        }
        #endregion

        #region Public properties
        public SequenceParseStatus ResultToReturn { get; set; } = SequenceParseStatus.Success;

        public bool ThrowOnParse { get; set; }
        #endregion
    }
}
