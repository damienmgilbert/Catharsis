using System.Buffers;
using Catharsis.Buffers;
using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Catharsis.Services;

///<summary>
///A DI-ready service that parses structured data from <see cref="ReadOnlySequence{T}"/> inputs using an <see
///cref="ISequenceParser"/> with logging support.
///</summary>
public sealed partial class SequenceParserService
{
    #region Fields
    readonly ILogger<SequenceParserService> _logger;
    readonly ISequenceParser _parser;
    #endregion

    #region Constructors
    ///<summary>
    ///Initializes a new <see cref="SequenceParserService"/> with the specified parser and logger.
    ///</summary>
    ///<param name="parser">The sequence parser implementation.</param>
    ///<param name="logger">The logger for diagnostic output.</param>
    public SequenceParserService(ISequenceParser parser, ILogger<SequenceParserService> logger)
    {
        Guard.IsNotNull(parser);
        Guard.IsNotNull(logger);

        _parser = parser;
        _logger = logger;
    }
    #endregion

    #region Public methods
    ///<summary>
    ///Parses a single message from the sequence.
    ///</summary>
    ///<param name="sequence">The input sequence.</param>
    ///<param name="consumed">The position up to which data was consumed.</param>
    ///<param name="examined">The position up to which data was examined.</param>
    ///<returns>The parse status.</returns>
    public SequenceParseStatus Parse(in ReadOnlySequence<byte> sequence, out SequencePosition consumed, out SequencePosition examined)
    {
        LogParsing(sequence.Length);

        SequenceParseStatus status = _parser.TryParse(sequence, out consumed, out examined);

        switch(status)
        {
            case SequenceParseStatus.Success:
                SuccessCount++;
                LogParseSucceeded();
                break;
            case SequenceParseStatus.InvalidData:
                FailureCount++;
                LogParseInvalidData();
                break;
            case SequenceParseStatus.NeedMoreData:
                LogParseNeedsMoreData();
                break;
        }

        return status;
    }

    ///<summary>
    ///Parses all available messages from the sequence.
    ///</summary>
    ///<param name="sequence">The input sequence.</param>
    ///<returns>The number of messages successfully parsed.</returns>
    public int ParseAll(ReadOnlySequence<byte> sequence)
    {
        int messageCount = 0;

        while(sequence.Length > 0)
        {
            SequenceParseStatus status = Parse(in sequence, out SequencePosition consumed, out SequencePosition examined);

            if(status == SequenceParseStatus.Success)
            {
                messageCount++;
                sequence = sequence.Slice(consumed);
            } else
            {
                break;
            }
        }

        LogParsedTotal(messageCount);
        return messageCount;
    }

    ///<summary>
    ///Resets the parser and clears statistics.
    ///</summary>
    public void Reset()
    {
        _parser.Reset();
        SuccessCount = 0;
        FailureCount = 0;
        LogReset();
    }
    #endregion

    #region Public properties
    ///<summary>
    ///Gets the total number of failed parse operations.
    ///</summary>
    public int FailureCount { get; private set; }

    ///<summary>
    ///Gets the total number of successful parse operations.
    ///</summary>
    public int SuccessCount { get; private set; }
    #endregion

    #region Log messages
    [LoggerMessage(EventId = 1, Level = LogLevel.Trace, Message = "Parsing sequence of {Length} bytes.")]
    partial void LogParsing(long length);

    [LoggerMessage(EventId = 2, Level = LogLevel.Trace, Message = "Parse succeeded.")]
    partial void LogParseSucceeded();

    [LoggerMessage(EventId = 3, Level = LogLevel.Warning, Message = "Parse failed: invalid data detected.")]
    partial void LogParseInvalidData();

    [LoggerMessage(EventId = 4, Level = LogLevel.Trace, Message = "Parse needs more data.")]
    partial void LogParseNeedsMoreData();

    [LoggerMessage(EventId = 5, Level = LogLevel.Debug, Message = "Parsed {Count} messages total.")]
    partial void LogParsedTotal(int count);

    [LoggerMessage(EventId = 6, Level = LogLevel.Debug, Message = "Parser service reset.")]
    partial void LogReset();
    #endregion
}
