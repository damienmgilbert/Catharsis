using Catharsis.Services;
using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Catharsis.Patterns;

/// <summary>
/// Demonstrates a DI-driven buffer processing pipeline that uses
/// <see cref="IServiceProvider"/> to resolve processors, parsers, and loggers,
/// combining multiple package capabilities into a cohesive architecture.
/// </summary>
public static class DiBufferPipeline
{
    /// <summary>
    /// Registers the buffer processing pipeline services with the DI container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddBufferPipeline(this IServiceCollection services)
    {
        Guard.IsNotNull(services);

        services.AddSingleton<BufferProcessingService>();
        services.AddSingleton<SequenceParserService>();
        services.AddSingleton(typeof(PooledObjectFactory<>));

        return services;
    }

    /// <summary>
    /// A sample end-to-end pipeline runner that demonstrates resolving and using
    /// the buffer processing service from a DI container.
    /// </summary>
    public sealed class PipelineRunner
    {
        private readonly BufferProcessingService _processingService;
        private readonly ILogger<PipelineRunner> _logger;

        /// <summary>
        /// Initializes a new <see cref="PipelineRunner"/>.
        /// </summary>
        /// <param name="processingService">The buffer processing service.</param>
        /// <param name="logger">The logger.</param>
        public PipelineRunner(BufferProcessingService processingService, ILogger<PipelineRunner> logger)
        {
            Guard.IsNotNull(processingService);
            Guard.IsNotNull(logger);

            _processingService = processingService;
            _logger = logger;
        }

        /// <summary>
        /// Runs the pipeline on the input data.
        /// </summary>
        /// <param name="input">The raw input data.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The processed output.</returns>
        public async Task<byte[]> RunAsync(byte[] input, CancellationToken cancellationToken = default)
        {
            Guard.IsNotNull(input);

            _logger.LogInformation("Starting DI pipeline with {ByteCount} bytes of input.", input.Length);

            byte[] result = await _processingService.ProcessAsync(input, cancellationToken);

            _logger.LogInformation("Pipeline complete. Output: {OutputBytes} bytes. Total processed: {Total} bytes.",
                result.Length, _processingService.TotalBytesProcessed);

            return result;
        }

        /// <summary>
        /// Runs the pipeline on the input data.
        /// </summary>
        /// <param name="input">The raw input data.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The processed output.</returns>
        public async ValueTask<byte[]> RunValueAsync(byte[] input, CancellationToken cancellationToken = default)
        {
            return await RunAsync(input, cancellationToken);
        }
    }
}
