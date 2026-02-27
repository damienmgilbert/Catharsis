using System.Buffers;

namespace Catharsis.Services;

/// <summary>
/// Defines a processor that transforms or analyzes buffer data.
/// </summary>
public interface IBufferProcessor
{
    /// <summary>
    /// Processes data from the input span and writes results to the output writer.
    /// </summary>
    /// <param name="input">The input data to process.</param>
    /// <param name="output">The output writer for processed results.</param>
    void Process(ReadOnlySpan<byte> input, IBufferWriter<byte> output);

    /// <summary>
    /// Asynchronously processes data from input memory and writes results to the output writer.
    /// </summary>
    /// <param name="input">The input data to process.</param>
    /// <param name="output">The output writer for processed results.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task ProcessAsync(ReadOnlyMemory<byte> input, IBufferWriter<byte> output, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously processes data from input memory and writes results to the output writer.
    /// </summary>
    /// <param name="input">The input data to process.</param>
    /// <param name="output">The output writer for processed results.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A value task representing the asynchronous operation.</returns>
    ValueTask ProcessValueAsync(ReadOnlyMemory<byte> input, IBufferWriter<byte> output, CancellationToken cancellationToken = default);
}
