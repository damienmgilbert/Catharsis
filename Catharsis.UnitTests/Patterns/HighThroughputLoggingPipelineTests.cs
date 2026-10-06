using Catharsis.Patterns;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Catharsis.UnitTests.Patterns;

///<summary>
///Unit tests for the <see cref="HighThroughputLoggingPipeline"/> class.
///</summary>
[TestClass]
public class HighThroughputLoggingPipelineTests
{
    #region Public methods
    [TestMethod]
    public void Flush_DoesNotThrow()
    {
        ILogger logger = NullLoggerFactory.Instance.CreateLogger("test");
        HighThroughputLoggingPipeline pipeline = new(logger);
        pipeline.Log(LogLevel.Debug, "msg");
        pipeline.Flush();
    }

    [TestMethod]
    public void Log_AutoFlushes_AtThreshold()
    {
        ILogger logger = NullLoggerFactory.Instance.CreateLogger("test");
        HighThroughputLoggingPipeline pipeline = new(logger, flushThreshold: 3);

        for (int i = 0; i < 3; i++)
        {
            pipeline.Log(LogLevel.Information, $"msg {i}");
        }

        Assert.AreEqual(3, pipeline.TotalEntries);
    }

    [TestMethod]
    public void Log_IncrementsTotalEntries()
    {
        ILogger logger = NullLoggerFactory.Instance.CreateLogger("test");
        HighThroughputLoggingPipeline pipeline = new(logger, flushThreshold: 10);

        pipeline.Log(LogLevel.Information, "test message");

        Assert.AreEqual(1, pipeline.TotalEntries);
    }

    [TestMethod]
    public void LogTimed_MeasuresExecution()
    {
        ILogger logger = NullLoggerFactory.Instance.CreateLogger("test");
        HighThroughputLoggingPipeline pipeline = new(logger, flushThreshold: 100);
        bool executed = false;

        pipeline.LogTimed(
        "TestOp",
        () =>
        {
            executed = true;
            Thread.Sleep(5);
        });

        Assert.IsTrue(executed);
        Assert.AreEqual(1, pipeline.TotalEntries);
    }
    #endregion
}
