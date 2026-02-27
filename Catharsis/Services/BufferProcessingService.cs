using System.Buffers;
using Catharsis.Buffers;
using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Catharsis.Services;

/// <summary>
/// A DI-ready service that orchestrates buffer processing using an <see cref="IBufferProcessor"/>
/// pipeline with integrated logging and pooled memory.
/// </summary>
public sealed class BufferProcessingService : IDisposable
{
    private readonly IBufferProcessor _processor;
    private readonly ILogger<BufferProcessingService> _logger;
    private readonly ArrayPool<byte> _pool;
    private bool _disposed;

    /// <summary>
    /// Initializes a new <see cref="BufferProcessingService"/> with the specified processor and logger.
    /// </summary>
    /// <param name="processor">The buffer processor to delegate work to.</param>
    /// <param name="logger">The logger for diagnostic output.</param>
    public BufferProcessingService(IBufferProcessor processor, ILogger<BufferProcessingService> logger)
        : this(processor, logger, ArrayPool<byte>.Shared)
    {
    }

    /// <summary>
    /// Initializes a new <see cref="BufferProcessingService"/> with a custom pool.
    /// </summary>
    /// <param name="processor">The buffer processor to delegate work to.</param>
    /// <param name="logger">The logger for diagnostic output.</param>
    /// <param name="pool">The array pool to use.</param>
    public BufferProcessingService(IBufferProcessor processor, ILogger<BufferProcessingService> logger, ArrayPool<byte> pool)
    {
        Guard.IsNotNull(processor);
        Guard.IsNotNull(logger);
        Guard.IsNotNull(pool);

        _processor = processor;
        _logger = logger;
        _pool = pool;
    }

    /// <summary>Gets the total number of bytes processed.</summary>
    public long TotalBytesProcessed { get; private set; }

    /// <summary>
    /// Processes the input data synchronously using the configured processor.
    /// </summary>
    /// <param name="input">The input data to process.</param>
    /// <returns>The processed output data.</returns>
    public byte[] Process(ReadOnlySpan<byte> input)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        _logger.LogDebug("Processing {ByteCount} bytes synchronously.", input.Length);

        using var output = new PooledBuffer<byte>(input.Length * 2);
        _processor.Process(input, output);
        TotalBytesProcessed += input.Length;

        _logger.LogDebug("Processing complete. Output: {OutputBytes} bytes.", output.WrittenCount);
        return output.WrittenSpan.ToArray();
    }

    /// <summary>
    /// Asynchronously processes the input data using the configured processor.
    /// </summary>
    /// <param name="input">The input data to process.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task containing the processed output data.</returns>
    public async Task<byte[]> ProcessAsync(ReadOnlyMemory<byte> input, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        _logger.LogDebug("Processing {ByteCount} bytes asynchronously.", input.Length);

        using var output = new PooledBuffer<byte>(input.Length * 2);
        await _processor.ProcessAsync(input, output, cancellationToken);
        TotalBytesProcessed += input.Length;

        _logger.LogDebug("Async processing complete. Output: {OutputBytes} bytes.", output.WrittenCount);
        return output.WrittenSpan.ToArray();
    }

    /// <summary>
    /// Asynchronously processes the input data using the configured processor.
    /// </summary>
    /// <param name="input">The input data to process.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A value task containing the processed output data.</returns>
    public async ValueTask<byte[]> ProcessValueAsync(ReadOnlyMemory<byte> input, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        _logger.LogDebug("Processing {ByteCount} bytes (ValueTask).", input.Length);

        using var output = new PooledBuffer<byte>(input.Length * 2);
        await _processor.ProcessValueAsync(input, output, cancellationToken);
        TotalBytesProcessed += input.Length;

        return output.WrittenSpan.ToArray();
    }

    /// <summary>
    /// Processes data from a stream and writes results to an output stream.
    /// </summary>
    /// <param name="inputStream">The input stream.</param>
    /// <param name="outputStream">The output stream.</param>
    /// <param name="bufferSize">The read buffer size.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task representing the operation.</returns>
    public async Task ProcessStreamAsync(
        Stream inputStream,
        Stream outputStream,
        int bufferSize = 4096,
        CancellationToken cancellationToken = default)
    {
        Guard.IsNotNull(inputStream);
        Guard.IsNotNull(outputStream);
        ObjectDisposedException.ThrowIf(_disposed, this);

        byte[] readBuffer = _pool.Rent(bufferSize);
        try
        {
            int bytesRead;
            while ((bytesRead = await inputStream.ReadAsync(readBuffer.AsMemory(0, bufferSize), cancellationToken)) > 0)
            {
                byte[] result = await ProcessAsync(readBuffer.AsMemory(0, bytesRead), cancellationToken);
                await outputStream.WriteAsync(result, cancellationToken);
            }
        }
        finally
        {
            _pool.Return(readBuffer);
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _logger.LogDebug("BufferProcessingService disposed. Total bytes processed: {Total}.", TotalBytesProcessed);
    }
}
