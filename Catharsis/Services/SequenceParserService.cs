using System.Buffers;
using Catharsis.Buffers;
using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Catharsis.Services;

/// <summary>
/// A DI-ready service that parses structured data from <see cref="ReadOnlySequence{T}"/>
/// inputs using an <see cref="ISequenceParser"/> with logging support.
/// </summary>
public sealed class SequenceParserService
{
    private readonly ISequenceParser _parser;
    private readonly ILogger<SequenceParserService> _logger;

    /// <summary>
    /// Initializes a new <see cref="SequenceParserService"/> with the specified parser and logger.
    /// </summary>
    /// <param name="parser">The sequence parser implementation.</param>
    /// <param name="logger">The logger for diagnostic output.</param>
    public SequenceParserService(ISequenceParser parser, ILogger<SequenceParserService> logger)
    {
        Guard.IsNotNull(parser);
        Guard.IsNotNull(logger);

        _parser = parser;
        _logger = logger;
    }

    /// <summary>Gets the total number of successful parse operations.</summary>
    public int SuccessCount { get; private set; }

    /// <summary>Gets the total number of failed parse operations.</summary>
    public int FailureCount { get; private set; }

    /// <summary>
    /// Parses a single message from the sequence.
    /// </summary>
    /// <param name="sequence">The input sequence.</param>
    /// <param name="consumed">The position up to which data was consumed.</param>
    /// <param name="examined">The position up to which data was examined.</param>
    /// <returns>The parse status.</returns>
    public SequenceParseStatus Parse(
        in ReadOnlySequence<byte> sequence,
        out SequencePosition consumed,
        out SequencePosition examined)
    {
        _logger.LogTrace("Parsing sequence of {Length} bytes.", sequence.Length);

        SequenceParseStatus status = _parser.TryParse(in sequence, out consumed, out examined);

        switch (status)
        {
            case SequenceParseStatus.Success:
                SuccessCount++;
                _logger.LogTrace("Parse succeeded.");
                break;
            case SequenceParseStatus.InvalidData:
                FailureCount++;
                _logger.LogWarning("Parse failed: invalid data detected.");
                break;
            case SequenceParseStatus.NeedMoreData:
                _logger.LogTrace("Parse needs more data.");
                break;
        }

        return status;
    }

    /// <summary>
    /// Parses all available messages from the sequence.
    /// </summary>
    /// <param name="sequence">The input sequence.</param>
    /// <returns>The number of messages successfully parsed.</returns>
    public int ParseAll(ReadOnlySequence<byte> sequence)
    {
        int messageCount = 0;

        while (sequence.Length > 0)
        {
            SequenceParseStatus status = Parse(in sequence, out SequencePosition consumed, out SequencePosition examined);

            if (status == SequenceParseStatus.Success)
            {
                messageCount++;
                sequence = sequence.Slice(consumed);
            }
            else
            {
                break;
            }
        }

        _logger.LogDebug("Parsed {Count} messages total.", messageCount);
        return messageCount;
    }

    /// <summary>
    /// Resets the parser and clears statistics.
    /// </summary>
    public void Reset()
    {
        _parser.Reset();
        SuccessCount = 0;
        FailureCount = 0;
        _logger.LogDebug("Parser service reset.");
    }
}
