using System.Buffers;
using Catharsis.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Catharsis.UnitTests.Services;

[TestClass]
public class BufferProcessingServiceTests
{
    #region Public methods
    [TestMethod]
    public void Process_ReturnsProcessedData()
    {
        ILogger<BufferProcessingService> logger = NullLoggerFactory.Instance.CreateLogger<BufferProcessingService>();
        using BufferProcessingService service = new BufferProcessingService(new EchoProcessor(), logger);

        byte[] result = service.Process([ 1, 2, 3 ]);

        CollectionAssert.AreEqual(new byte[] { 1, 2, 3 }, result);
        Assert.AreEqual(3, service.TotalBytesProcessed);
    }

    [TestMethod]
    public async Task ProcessAsync_ReturnsProcessedData()
    {
        ILogger<BufferProcessingService> logger = NullLoggerFactory.Instance.CreateLogger<BufferProcessingService>();
        using BufferProcessingService service = new BufferProcessingService(new EchoProcessor(), logger);

        byte[] result = await service.ProcessAsync(new byte[] { 10, 20 });

        CollectionAssert.AreEqual(new byte[] { 10, 20 }, result);
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

        public async Task ProcessAsync(ReadOnlyMemory<byte> input, IBufferWriter<byte> output, CancellationToken cancellationToken = default)
        {
            Process(input.Span, output);
            await Task.CompletedTask;
        }

        public async ValueTask ProcessValueAsync(ReadOnlyMemory<byte> input, IBufferWriter<byte> output, CancellationToken cancellationToken = default)
        {
            Process(input.Span, output);
            await Task.CompletedTask;
        }
        #endregion
    }
}
