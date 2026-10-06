using System.Diagnostics.Tracing;

namespace Catharsis.Diagnostics;

///<summary>
///A minimal <see cref="EventSource"/> exposing structured start/stop/fault events over EventPipe — a cross-platform
///alternative to ETW that <c>dotnet-trace</c> and other diagnostic tools can capture without any exporter
///configuration. This is distinct from the <c>Microsoft.Extensions.Logging.Abstractions</c> usage found elsewhere
///in the library: those calls go through whatever <c>ILogger</c> the caller wires up, while these events are
///captured by the EventPipe/ETW diagnostic pipeline regardless of any logging configuration.
///</summary>
[EventSource(Name = "Catharsis-Diagnostics")]
public sealed class EventSourceLogger : EventSource
{
    #region Fields
    ///<summary>
    ///The singleton instance of this event source, following the standard <see cref="EventSource"/> convention.
    ///</summary>
    public static readonly EventSourceLogger Log = new();
    #endregion

    #region Constructors
    EventSourceLogger() { }
    #endregion

    #region Public methods
    ///<summary>
    ///Writes an event recording that an operation faulted.
    ///</summary>
    ///<param name="operationName">The name of the operation that faulted.</param>
    ///<param name="errorMessage">The fault's error message.</param>
    [Event(3, Level = EventLevel.Error, Message = "{0} faulted: {1}")]
    public void OperationFault(string operationName, string errorMessage)
    {
        if (IsEnabled())
        {
            WriteEvent(3, operationName, errorMessage);
        }
    }

    ///<summary>
    ///Writes an event recording that an operation started.
    ///</summary>
    ///<param name="operationName">The name of the operation that started.</param>
    [Event(1, Level = EventLevel.Informational, Message = "{0} started.")]
    public void OperationStart(string operationName)
    {
        if (IsEnabled())
        {
            WriteEvent(1, operationName);
        }
    }

    ///<summary>
    ///Writes an event recording that an operation stopped, and how long it took.
    ///</summary>
    ///<param name="operationName">The name of the operation that stopped.</param>
    ///<param name="elapsedMilliseconds">The operation's duration, in milliseconds.</param>
    [Event(2, Level = EventLevel.Informational, Message = "{0} stopped after {1} ms.")]
    public void OperationStop(string operationName, double elapsedMilliseconds)
    {
        if (IsEnabled())
        {
            WriteEvent(2, operationName, elapsedMilliseconds);
        }
    }
    #endregion
}
