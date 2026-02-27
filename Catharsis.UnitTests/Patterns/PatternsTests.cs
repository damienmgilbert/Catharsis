using Catharsis.Buffers;
using Catharsis.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Buffers;

namespace Catharsis.Patterns.UnitTests;

[TestClass]
public class SpanSerializerPatternTests
{
    [TestMethod]
    public void SensorReading_RoundTrip()
    {
        var reading = new SpanSerializerPattern.SensorReading(1, 25.5f, 60.0f, 1000L);

        using var buffer = new PooledBuffer<byte>(SpanSerializerPattern.SensorReading.SerializedSizeValue);
        reading.Serialize(buffer);

        var deserialized = SpanSerializerPattern.SensorReading.Deserialize(buffer.WrittenSpan);

        Assert.AreEqual(1, deserialized.SensorId);
        Assert.AreEqual(25.5f, deserialized.Temperature);
        Assert.AreEqual(60.0f, deserialized.Humidity);
        Assert.AreEqual(1000L, deserialized.TimestampTicks);
    }

    [TestMethod]
    public void SerializeBatch_And_DeserializeBatch_RoundTrip()
    {
        SpanSerializerPattern.SensorReading[] readings =
        [
            new(1, 20.0f, 50.0f, 100L),
            new(2, 30.0f, 70.0f, 200L),
        ];

        byte[] data = SpanSerializerPattern.SerializeBatch(readings);

        var dest = new SpanSerializerPattern.SensorReading[2];
        int count = SpanSerializerPattern.DeserializeBatch(data, dest);

        Assert.AreEqual(2, count);
        Assert.AreEqual(1, dest[0].SensorId);
        Assert.AreEqual(2, dest[1].SensorId);
    }

    [TestMethod]
    public void GetSerializedSize_ReturnsExpected()
    {
        var reading = new SpanSerializerPattern.SensorReading(0, 0, 0, 0);
        Assert.AreEqual(SpanSerializerPattern.SensorReading.SerializedSizeValue, reading.GetSerializedSize());
    }
}

[TestClass]
public class ZeroAllocationPipelineTests
{
    [TestMethod]
    public void ParseRecords_ParsesCorrectly()
    {
        const int recordSize = sizeof(int) + sizeof(double) + sizeof(long);
        byte[] data = new byte[recordSize];
        BitConverter.TryWriteBytes(data.AsSpan(0), 42);
        BitConverter.TryWriteBytes(data.AsSpan(4), 3.14);
        BitConverter.TryWriteBytes(data.AsSpan(12), 999L);

        var dest = new ZeroAllocationPipeline.ParsedRecord[1];
        int count = ZeroAllocationPipeline.ParseRecords(data, dest);

        Assert.AreEqual(1, count);
        Assert.AreEqual(42, dest[0].Id);
        Assert.AreEqual(3.14, dest[0].Value, 0.001);
        Assert.AreEqual(999L, dest[0].Timestamp);
    }

    [TestMethod]
    public void SumCsvIntegers_SumsCorrectly()
    {
        long sum = ZeroAllocationPipeline.SumCsvIntegers("1,2,3,4,5".AsSpan());
        Assert.AreEqual(15, sum);
    }

    [TestMethod]
    public void SumCsvIntegers_EmptyString_ReturnsZero()
    {
        long sum = ZeroAllocationPipeline.SumCsvIntegers("".AsSpan());
        Assert.AreEqual(0, sum);
    }

    [TestMethod]
    public void SumCsvIntegers_SingleValue()
    {
        long sum = ZeroAllocationPipeline.SumCsvIntegers("42".AsSpan());
        Assert.AreEqual(42, sum);
    }

    [TestMethod]
    public void SumCsvIntegers_WithSpaces_TrimsCorrectly()
    {
        long sum = ZeroAllocationPipeline.SumCsvIntegers(" 10 , 20 , 30 ".AsSpan());
        Assert.AreEqual(60, sum);
    }
}

[TestClass]
public class DiBufferPipelineTests
{
    [TestMethod]
    public async Task PipelineRunner_RunAsync_ProcessesData()
    {
        var processor = new EchoProcessor();
        var processingLogger = NullLoggerFactory.Instance.CreateLogger<BufferProcessingService>();
        using var processingService = new BufferProcessingService(processor, processingLogger);
        var runnerLogger = NullLoggerFactory.Instance.CreateLogger<DiBufferPipeline.PipelineRunner>();
        var runner = new DiBufferPipeline.PipelineRunner(processingService, runnerLogger);

        byte[] result = await runner.RunAsync([1, 2, 3]);

        CollectionAssert.AreEqual(new byte[] { 1, 2, 3 }, result);
    }

    private sealed class EchoProcessor : IBufferProcessor
    {
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
    }
}

[TestClass]
public class BinaryProcessingPipelineTests
{
    [TestMethod]
    public void AddStage_IncreasesStageCount()
    {
        using var pipeline = new BinaryProcessingPipeline();
        pipeline.AddStage(BinaryProcessingPipeline.CreateXorStage(0xFF));
        Assert.AreEqual(1, pipeline.StageCount);
    }

    [TestMethod]
    public void Execute_XorStage_TransformsData()
    {
        using var pipeline = new BinaryProcessingPipeline();
        pipeline.AddStage(BinaryProcessingPipeline.CreateXorStage(0xFF));

        byte[] result = pipeline.Execute([0x00, 0x01, 0x02], out double elapsed);

        Assert.AreEqual(0xFF, result[0]);
        Assert.AreEqual(0xFE, result[1]);
        Assert.AreEqual(0xFD, result[2]);
        Assert.IsTrue(elapsed >= 0);
    }

    [TestMethod]
    public void Execute_DoubleXor_ReturnsOriginal()
    {
        using var pipeline = new BinaryProcessingPipeline();
        pipeline.AddStage(BinaryProcessingPipeline.CreateXorStage(0xAB));
        pipeline.AddStage(BinaryProcessingPipeline.CreateXorStage(0xAB));

        byte[] result = pipeline.Execute([10, 20, 30], out _);

        CollectionAssert.AreEqual(new byte[] { 10, 20, 30 }, result);
    }

    [TestMethod]
    public void Execute_ReverseStage_ReversesData()
    {
        using var pipeline = new BinaryProcessingPipeline();
        pipeline.AddStage(BinaryProcessingPipeline.CreateReverseStage());

        byte[] result = pipeline.Execute([1, 2, 3], out _);

        CollectionAssert.AreEqual(new byte[] { 3, 2, 1 }, result);
    }

    [TestMethod]
    public void Execute_NoStages_ReturnsOriginal()
    {
        using var pipeline = new BinaryProcessingPipeline();
        byte[] result = pipeline.Execute([1, 2], out _);
        CollectionAssert.AreEqual(new byte[] { 1, 2 }, result);
    }

    [TestMethod]
    public void Dispose_IsIdempotent()
    {
        var pipeline = new BinaryProcessingPipeline();
        pipeline.Dispose();
        pipeline.Dispose();
    }
}

[TestClass]
public class ImmutablePooledHybridTests
{
    [TestMethod]
    public void Write_IncreasesCount()
    {
        using var hybrid = new ImmutablePooledHybrid<byte>();
        hybrid.Write([1, 2, 3]);
        Assert.AreEqual(3, hybrid.Count);
        Assert.IsFalse(hybrid.IsFrozen);
    }

    [TestMethod]
    public void Freeze_CreatesImmutableSnapshot()
    {
        using var hybrid = new ImmutablePooledHybrid<int>();
        hybrid.Write([10, 20]);
        var frozen = hybrid.Freeze();
        Assert.IsTrue(hybrid.IsFrozen);
        Assert.AreEqual(2, frozen.Count);
        Assert.AreEqual(10, frozen[0]);
        Assert.AreEqual(20, frozen[1]);
    }

    [TestMethod]
    public void Write_AfterFreeze_Throws()
    {
        using var hybrid = new ImmutablePooledHybrid<byte>();
        hybrid.Write([1]);
        hybrid.Freeze();
        Assert.ThrowsExactly<ArgumentException>(() => hybrid.Write([2]));
    }

    [TestMethod]
    public void Reset_AllowsRewrite()
    {
        using var hybrid = new ImmutablePooledHybrid<byte>();
        hybrid.Write([1]);
        hybrid.Freeze();

        hybrid.Reset();

        Assert.IsFalse(hybrid.IsFrozen);
        Assert.AreEqual(0, hybrid.Count);
        hybrid.Write([2, 3]);
        Assert.AreEqual(2, hybrid.Count);
    }

    [TestMethod]
    public void Span_ReturnsMutableDataBeforeFreeze()
    {
        using var hybrid = new ImmutablePooledHybrid<byte>();
        hybrid.Write([5, 10]);
        var span = hybrid.Span;
        Assert.AreEqual(2, span.Length);
        Assert.AreEqual(5, span[0]);
    }

    [TestMethod]
    public void Dispose_IsIdempotent()
    {
        var hybrid = new ImmutablePooledHybrid<byte>();
        hybrid.Dispose();
        hybrid.Dispose();
    }
}

[TestClass]
public class MvvmPooledViewModelTests
{
    [TestMethod]
    public async Task LoadAsync_LoadsDataIntoBuffer()
    {
        using var vm = new MvvmPooledViewModel();
        await vm.LoadAsync();
        Assert.IsTrue(vm.HasData);
        Assert.AreEqual(1024, vm.DataSize);
        Assert.IsTrue(vm.DisplayText.Contains("1024"));
    }

    [TestMethod]
    public void ClearAllDataCommand_ClearsState()
    {
        using var vm = new MvvmPooledViewModel();
        vm.ClearAllDataCommand.Execute(null);
        Assert.IsFalse(vm.HasData);
        Assert.AreEqual(string.Empty, vm.DisplayText);
    }

    [TestMethod]
    public void DataBuffer_IsNotNull()
    {
        using var vm = new MvvmPooledViewModel();
        Assert.IsNotNull(vm.DataBuffer);
    }

    [TestMethod]
    public void Commands_AreNotNull()
    {
        using var vm = new MvvmPooledViewModel();
        Assert.IsNotNull(vm.LoadSampleDataCommand);
        Assert.IsNotNull(vm.ClearAllDataCommand);
    }
}

[TestClass]
public class HighThroughputLoggingPipelineTests
{
    [TestMethod]
    public void Log_IncrementsTotalEntries()
    {
        var logger = NullLoggerFactory.Instance.CreateLogger("test");
        var pipeline = new HighThroughputLoggingPipeline(logger, flushThreshold: 10);

        pipeline.Log(LogLevel.Information, "test message");

        Assert.AreEqual(1, pipeline.TotalEntries);
    }

    [TestMethod]
    public void Log_AutoFlushes_AtThreshold()
    {
        var logger = NullLoggerFactory.Instance.CreateLogger("test");
        var pipeline = new HighThroughputLoggingPipeline(logger, flushThreshold: 3);

        for (int i = 0; i < 3; i++)
            pipeline.Log(LogLevel.Information, $"msg {i}");

        Assert.AreEqual(3, pipeline.TotalEntries);
    }

    [TestMethod]
    public void LogTimed_MeasuresExecution()
    {
        var logger = NullLoggerFactory.Instance.CreateLogger("test");
        var pipeline = new HighThroughputLoggingPipeline(logger, flushThreshold: 100);
        bool executed = false;

        pipeline.LogTimed("TestOp", () => { executed = true; Thread.Sleep(5); });

        Assert.IsTrue(executed);
        Assert.AreEqual(1, pipeline.TotalEntries);
    }

    [TestMethod]
    public void Flush_DoesNotThrow()
    {
        var logger = NullLoggerFactory.Instance.CreateLogger("test");
        var pipeline = new HighThroughputLoggingPipeline(logger);
        pipeline.Log(LogLevel.Debug, "msg");
        pipeline.Flush();
    }
}
