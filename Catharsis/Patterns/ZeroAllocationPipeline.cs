using System.Buffers;
using Catharsis.Buffers;
using Catharsis.HighPerformance;
using CommunityToolkit.Diagnostics;

namespace Catharsis.Patterns;

/// <summary>
/// Demonstrates a zero-allocation parsing pipeline using <see cref="SpanReader"/>,
/// <see cref="SpanTokenizer"/>, and pooled buffers. No heap allocations occur
/// during the parsing of structured data from a byte span.
/// </summary>
public static class ZeroAllocationPipeline
{
    /// <summary>
    /// Represents a parsed record extracted from a binary span.
    /// </summary>
    /// <param name="Id">The record identifier.</param>
    /// <param name="Value">The record value.</param>
    /// <param name="Timestamp">The record timestamp in ticks.</param>
    public readonly record struct ParsedRecord(int Id, double Value, long Timestamp);

    /// <summary>
    /// Parses a sequence of fixed-format records from a byte span with zero heap allocation.
    /// Each record is: [int32 Id][double Value][int64 Timestamp] = 20 bytes.
    /// </summary>
    /// <param name="data">The raw byte data.</param>
    /// <param name="destination">The destination span for parsed records.</param>
    /// <returns>The number of records parsed.</returns>
    public static int ParseRecords(ReadOnlySpan<byte> data, Span<ParsedRecord> destination)
    {
        const int recordSize = sizeof(int) + sizeof(double) + sizeof(long);

        Guard.IsGreaterThanOrEqualTo(destination.Length, data.Length / recordSize);

        var reader = new SpanReader(data);
        int count = 0;

        while (reader.Remaining >= recordSize && count < destination.Length)
        {
            int id = reader.ReadInt32LittleEndian();
            double value = reader.ReadDoubleLittleEndian();
            long timestamp = reader.ReadInt64LittleEndian();

            destination[count++] = new ParsedRecord(id, value, timestamp);
        }

        return count;
    }

    /// <summary>
    /// Tokenizes a CSV line from a character span and sums all integer tokens, demonstrating
    /// zero-allocation text parsing using <see cref="SpanTokenizer"/>.
    /// </summary>
    /// <param name="csvLine">A comma-separated line of integers.</param>
    /// <returns>The sum of all integer values.</returns>
    public static long SumCsvIntegers(ReadOnlySpan<char> csvLine)
    {
        var tokenizer = new SpanTokenizer(csvLine, ',');
        long sum = 0;

        while (tokenizer.TryGetNext(out ReadOnlySpan<char> token))
        {
            ReadOnlySpan<char> trimmed = token.Trim();
            if (!trimmed.IsEmpty && int.TryParse(trimmed, out int value))
                sum += value;
        }

        return sum;
    }
}
