using System.Diagnostics;

namespace Catharsis.Diagnostics;

///<summary>
///An <see cref="IDisposable"/> wrapper around an <see cref="Activity"/> started from an <see cref="ActivitySource"/>,
///for distributed tracing. This is a BCL primitive only: it does not configure or require any exporter — the
///started <see cref="Activity"/> simply does nothing unless something else in the process (an
///<see cref="ActivityListener"/> or an OpenTelemetry SDK) is listening to the source.
///</summary>
///<remarks>
///Call <see cref="RecordException"/> from a <c>catch</c> block before the scope is disposed; there is no reliable,
///portable way for <see cref="Dispose"/> itself to detect that it is unwinding due to an exception, so recording
///must happen explicitly rather than automatically.
///</remarks>
public sealed class ActivityScope : IDisposable
{
    #region Constructors
    ActivityScope(Activity? activity) => Activity = activity;
    #endregion

    #region Public methods
    ///<summary>
    ///Records <paramref name="exception"/> as an exception event on the wrapped activity and marks the activity's
    ///status as an error. Does nothing if no activity was actually started (e.g. because nothing is listening to
    ///the source).
    ///</summary>
    ///<param name="exception">The exception to record.</param>
    ///<exception cref="ArgumentNullException"><paramref name="exception"/> is <c>null</c>.</exception>
    public void RecordException(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        if(Activity is null)
        {
            return;
        }

        ActivityTagsCollection tags = new()
        {
            ["exception.type"] = exception.GetType().FullName,
            ["exception.message"] = exception.Message,
            ["exception.stacktrace"] = exception.StackTrace
        };

        Activity.AddEvent(new ActivityEvent("exception", tags: tags));
        Activity.SetStatus(ActivityStatusCode.Error, exception.Message);
    }

    ///<summary>
    ///Starts a new activity from <paramref name="source"/> and wraps it in a scope that stops it on <see cref="Dispose"/>.
    ///</summary>
    ///<param name="source">The activity source to start the activity from.</param>
    ///<param name="name">The operation name.</param>
    ///<param name="kind">The kind of activity to start.</param>
    ///<returns>A new <see cref="ActivityScope"/>. Its <see cref="Activity"/> is <c>null</c> if nothing is listening to <paramref name="source"/>.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
    ///<exception cref="ArgumentException"><paramref name="name"/> is <c>null</c>, empty, or whitespace.</exception>
    public static ActivityScope Start(ActivitySource source, string name, ActivityKind kind = ActivityKind.Internal)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new ActivityScope(source.StartActivity(name, kind));
    }

    ///<summary>
    ///Stops the wrapped activity, if one was started.
    ///</summary>
    public void Dispose() => Activity?.Dispose();
    #endregion

    #region Public properties
    ///<summary>
    ///Gets the wrapped activity, or <c>null</c> if nothing was listening to the source when the scope was started.
    ///</summary>
    public Activity? Activity { get; }
    #endregion
}
