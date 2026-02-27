using Catharsis.Buffers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Buffers;

namespace Catharsis.Services.UnitTests;

[TestClass]
public class PooledObjectFactoryTests
{
    private sealed class TestObj : IDisposable
    {
        public bool Disposed { get; private set; }
        public void Dispose() => Disposed = true;
    }

    [TestMethod]
    public void Rent_CreatesNewObject()
    {
        var logger = NullLoggerFactory.Instance.CreateLogger<PooledObjectFactory<TestObj>>();
        using var factory = new PooledObjectFactory<TestObj>(logger);

        var obj = factory.Rent();

        Assert.IsNotNull(obj);
        Assert.AreEqual(1, factory.TotalCreated);
    }

    [TestMethod]
    public void Return_And_Rent_ReusesObject()
    {
        var logger = NullLoggerFactory.Instance.CreateLogger<PooledObjectFactory<TestObj>>();
        using var factory = new PooledObjectFactory<TestObj>(logger);

        var obj = factory.Rent();
        factory.Return(obj);
        Assert.AreEqual(1, factory.AvailableCount);

        var obj2 = factory.Rent();
        Assert.AreSame(obj, obj2);
    }

    [TestMethod]
    public void Dispose_DisposesPooledObjects()
    {
        var logger = NullLoggerFactory.Instance.CreateLogger<PooledObjectFactory<TestObj>>();
        var factory = new PooledObjectFactory<TestObj>(logger);
        var obj = factory.Rent();
        factory.Return(obj);

        factory.Dispose();

        Assert.IsTrue(obj.Disposed);
    }

    [TestMethod]
    public void Rent_AfterDispose_Throws()
    {
        var logger = NullLoggerFactory.Instance.CreateLogger<PooledObjectFactory<TestObj>>();
        var factory = new PooledObjectFactory<TestObj>(logger);
        factory.Dispose();
        Assert.ThrowsExactly<ObjectDisposedException>(() => factory.Rent());
    }
}

[TestClass]
public class BufferLoggerTests
{
    [TestMethod]
    public void Log_BuffersEntry()
    {
        var inner = NullLoggerFactory.Instance.CreateLogger("test");
        using var logger = new BufferLogger(inner);

        logger.Log(LogLevel.Information, "test message");

        Assert.AreEqual(1, logger.PendingEntries);
    }

    [TestMethod]
    public void Flush_ClearsPendingEntries()
    {
        var inner = NullLoggerFactory.Instance.CreateLogger("test");
        using var logger = new BufferLogger(inner);

        logger.Log(LogLevel.Information, "msg1");
        logger.Log(LogLevel.Warning, "msg2");
        Assert.AreEqual(2, logger.PendingEntries);

        logger.Flush();
        Assert.AreEqual(0, logger.PendingEntries);
    }

    [TestMethod]
    public void Flush_NoPending_DoesNothing()
    {
        var inner = NullLoggerFactory.Instance.CreateLogger("test");
        using var logger = new BufferLogger(inner);
        logger.Flush();
        Assert.AreEqual(0, logger.PendingEntries);
    }
}

[TestClass]
public class SequenceParserServiceTests
{
    private sealed class SuccessParser : ISequenceParser
    {
        public SequenceParseStatus TryParse(in ReadOnlySequence<byte> sequence, out SequencePosition consumed, out SequencePosition examined)
        {
            consumed = sequence.End;
            examined = sequence.End;
            return SequenceParseStatus.Success;
        }
        public void Reset() { }
    }

    private sealed class FailParser : ISequenceParser
    {
        public SequenceParseStatus TryParse(in ReadOnlySequence<byte> sequence, out SequencePosition consumed, out SequencePosition examined)
        {
            consumed = sequence.Start;
            examined = sequence.End;
            return SequenceParseStatus.InvalidData;
        }
        public void Reset() { }
    }

    [TestMethod]
    public void Parse_Success_IncrementsSuccessCount()
    {
        var logger = NullLoggerFactory.Instance.CreateLogger<SequenceParserService>();
        var service = new SequenceParserService(new SuccessParser(), logger);
        var seq = new ReadOnlySequence<byte>(new byte[] { 1, 2, 3 });

        var status = service.Parse(in seq, out _, out _);

        Assert.AreEqual(SequenceParseStatus.Success, status);
        Assert.AreEqual(1, service.SuccessCount);
    }

    [TestMethod]
    public void Parse_InvalidData_IncrementsFailureCount()
    {
        var logger = NullLoggerFactory.Instance.CreateLogger<SequenceParserService>();
        var service = new SequenceParserService(new FailParser(), logger);
        var seq = new ReadOnlySequence<byte>(new byte[] { 1 });

        service.Parse(in seq, out _, out _);

        Assert.AreEqual(1, service.FailureCount);
    }
}

[TestClass]
public class BufferProcessingServiceTests
{
    private sealed class EchoProcessor : IBufferProcessor
    {
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
    }

    [TestMethod]
    public void Process_ReturnsProcessedData()
    {
        var logger = NullLoggerFactory.Instance.CreateLogger<BufferProcessingService>();
        using var service = new BufferProcessingService(new EchoProcessor(), logger);

        byte[] result = service.Process([1, 2, 3]);

        CollectionAssert.AreEqual(new byte[] { 1, 2, 3 }, result);
        Assert.AreEqual(3, service.TotalBytesProcessed);
    }

    [TestMethod]
    public async Task ProcessAsync_ReturnsProcessedData()
    {
        var logger = NullLoggerFactory.Instance.CreateLogger<BufferProcessingService>();
        using var service = new BufferProcessingService(new EchoProcessor(), logger);

        byte[] result = await service.ProcessAsync(new byte[] { 10, 20 });

        CollectionAssert.AreEqual(new byte[] { 10, 20 }, result);
    }
}
