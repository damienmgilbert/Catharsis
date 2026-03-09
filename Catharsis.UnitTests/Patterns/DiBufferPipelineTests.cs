using System.Buffers;
using Catharsis.Patterns;
using Catharsis.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Catharsis.UnitTests.Patterns;

///<summary>
///Unit tests for the <see cref="DiBufferPipeline"/> class.
///</summary>
[TestClass]
public class DiBufferPipelineTests
{
    #region Public methods
    [TestMethod]
    public async Task PipelineRunner_RunAsync_ProcessesData()
    {
        EchoProcessor processor = new EchoProcessor();
        ILogger<BufferProcessingService> processingLogger = NullLoggerFactory.Instance.CreateLogger<BufferProcessingService>();
        using BufferProcessingService processingService = new BufferProcessingService(processor, processingLogger);
        ILogger<DiBufferPipeline.PipelineRunner> runnerLogger = NullLoggerFactory.Instance.CreateLogger<DiBufferPipeline.PipelineRunner>();
        DiBufferPipeline.PipelineRunner runner = new DiBufferPipeline.PipelineRunner(processingService, runnerLogger);

        byte[] result = await runner.RunAsync([ 1, 2, 3 ]);

        CollectionAssert.AreEqual(new byte[] { 1, 2, 3 }, result);
    }
    #endregion

    sealed class EchoProcessor : IBufferProcessor
    {
        #region Public methods
        public void Process(ReadOnlySpan<byte> input, IBufferWriter<byte> output)
        {
            Span<byte> dest = output.GetSpan(input.Length);
            input.CopyTo(dest);
            output.Advance(input.Length);
        }

        public Task ProcessAsync(ReadOnlyMemory<byte> input, IBufferWriter<byte> output, CancellationToken ct = default)
        {
            Process(input.Span, output);
            return Task.CompletedTask;
        }

        public ValueTask ProcessValueAsync(ReadOnlyMemory<byte> input, IBufferWriter<byte> output, CancellationToken ct = default)
        {
            Process(input.Span, output);
            return ValueTask.CompletedTask;
        }
        #endregion
    }
}
