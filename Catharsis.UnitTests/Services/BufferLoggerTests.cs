using Catharsis.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Catharsis.UnitTests.Services;

///<summary>
///Unit tests for the <see cref="BufferLogger"/> class.
///</summary>
[TestClass]
public class BufferLoggerTests
{
    #region Public methods
    [TestMethod]
    public void Flush_ClearsPendingEntries()
    {
        ILogger inner = NullLoggerFactory.Instance.CreateLogger("test");
        using BufferLogger logger = new(inner);

        logger.Log(LogLevel.Information, "msg1");
        logger.Log(LogLevel.Warning, "msg2");
        Assert.AreEqual(2, logger.PendingEntries);

        logger.Flush();
        Assert.AreEqual(0, logger.PendingEntries);
    }

    [TestMethod]
    public void Flush_NoPending_DoesNothing()
    {
        ILogger inner = NullLoggerFactory.Instance.CreateLogger("test");
        using BufferLogger logger = new(inner);
        logger.Flush();
        Assert.AreEqual(0, logger.PendingEntries);
    }

    [TestMethod]
    public void Log_BuffersEntry()
    {
        ILogger inner = NullLoggerFactory.Instance.CreateLogger("test");
        using BufferLogger logger = new(inner);

        logger.Log(LogLevel.Information, "test message");

        Assert.AreEqual(1, logger.PendingEntries);
    }
    #endregion
}
