using System.Buffers;
using Catharsis.Buffers;
using Catharsis.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Catharsis.UnitTests.Services;

[TestClass]
public class SequenceParserServiceTests
{
    #region Public methods
    [TestMethod]
    public void Parse_InvalidData_IncrementsFailureCount()
    {
        ILogger<SequenceParserService> logger = NullLoggerFactory.Instance.CreateLogger<SequenceParserService>();
        SequenceParserService service = new SequenceParserService(new FailParser(), logger);
        ReadOnlySequence<byte> seq = new ReadOnlySequence<byte>(new byte[] { 1 });

        service.Parse(in seq, out _, out _);

        Assert.AreEqual(1, service.FailureCount);
    }

    [TestMethod]
    public void Parse_Success_IncrementsSuccessCount()
    {
        ILogger<SequenceParserService> logger = NullLoggerFactory.Instance.CreateLogger<SequenceParserService>();
        SequenceParserService service = new SequenceParserService(new SuccessParser(), logger);
        ReadOnlySequence<byte> seq = new ReadOnlySequence<byte>(new byte[] { 1, 2, 3 });

        SequenceParseStatus status = service.Parse(in seq, out _, out _);

        Assert.AreEqual(SequenceParseStatus.Success, status);
        Assert.AreEqual(1, service.SuccessCount);
    }
    #endregion

    sealed class SuccessParser : ISequenceParser
    {
        #region Public methods
        public void Reset()
        {
        }

        public SequenceParseStatus TryParse(in ReadOnlySequence<byte> sequence, out SequencePosition consumed, out SequencePosition examined)
        {
            consumed = sequence.End;
            examined = sequence.End;
            return SequenceParseStatus.Success;
        }
        #endregion
    }

    sealed class FailParser : ISequenceParser
    {
        #region Public methods
        public void Reset()
        {
        }

        public SequenceParseStatus TryParse(in ReadOnlySequence<byte> sequence, out SequencePosition consumed, out SequencePosition examined)
        {
            consumed = sequence.Start;
            examined = sequence.End;
            return SequenceParseStatus.InvalidData;
        }
        #endregion
    }
}
