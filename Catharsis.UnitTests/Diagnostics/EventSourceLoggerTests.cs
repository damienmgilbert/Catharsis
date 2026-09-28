using Catharsis.Diagnostics;
using System.Diagnostics.Tracing;

namespace Catharsis.UnitTests.Diagnostics;

///<summary>
///Unit tests for the <see cref="EventSourceLogger"/> class.
///</summary>
[TestClass]
public class EventSourceLoggerTests
{
    #region Public methods

    [TestMethod]
    public void OperationStart_NoListener_DoesNotThrow() { EventSourceLogger.Log.OperationStart($"op-{Guid.NewGuid():N}"); }

    [TestMethod]
    public void OperationStart_WritesEventWithOperationName()
    {
        string operationName = $"op-{Guid.NewGuid():N}";
        using CapturingEventListener listener = new();

        EventSourceLogger.Log.OperationStart(operationName);

        EventWrittenEventArgs? written = listener.Events.FirstOrDefault(e => (e.EventId == 1) && Equals(e.Payload?[0], operationName));
        Assert.IsNotNull(written);
    }

    [TestMethod]
    public void OperationStop_WritesEventWithOperationNameAndElapsedMilliseconds()
    {
        string operationName = $"op-{Guid.NewGuid():N}";
        using CapturingEventListener listener = new();

        EventSourceLogger.Log.OperationStop(operationName, 42.5);

        EventWrittenEventArgs? written = listener.Events.FirstOrDefault(e => (e.EventId == 2) && Equals(e.Payload?[0], operationName));
        Assert.IsNotNull(written);
        Assert.AreEqual(42.5, written!.Payload![1]);
    }

    [TestMethod]
    public void OperationFault_WritesEventWithOperationNameAndErrorMessage()
    {
        string operationName = $"op-{Guid.NewGuid():N}";
        using CapturingEventListener listener = new();

        EventSourceLogger.Log.OperationFault(operationName, "boom");

        EventWrittenEventArgs? written = listener.Events.FirstOrDefault(e => (e.EventId == 3) && Equals(e.Payload?[0], operationName));
        Assert.IsNotNull(written);
        Assert.AreEqual("boom", written!.Payload![1]);
    }

    #endregion

    private sealed class CapturingEventListener : EventListener
    {
        #region Public methods
        protected override void OnEventSourceCreated(EventSource eventSource)
        {
            if(eventSource.Name == "Catharsis-Diagnostics")
            {
                EnableEvents(eventSource, EventLevel.Verbose);
            }
        }

        protected override void OnEventWritten(EventWrittenEventArgs eventData) => Events.Add(eventData);
        #endregion

        #region Public properties
        public List<EventWrittenEventArgs> Events { get; } = [];
        #endregion
    }
}
