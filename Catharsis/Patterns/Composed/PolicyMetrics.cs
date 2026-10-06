namespace Catharsis.Patterns.Composed;

///<summary>
///A point-in-time snapshot of the counters kept by <see cref="MetricsPolicyDecorator"/>.
///</summary>
///<param name="Executions">How many executions have finished, successfully or not.</param>
///<param name="Failures">How many of those failed with an exception other than cancellation.</param>
///<param name="Cancellations">How many of those were cancelled.</param>
///<param name="TotalDuration">The combined running time of every finished execution.</param>
public sealed record PolicyMetrics(long Executions, long Failures, long Cancellations, TimeSpan TotalDuration)
{
    #region Public properties
    ///<summary>Gets the executions that completed normally.</summary>
    public long Successes => Executions - Failures - Cancellations;

    ///<summary>Gets the mean running time, or <see cref="TimeSpan.Zero"/> when nothing has run.</summary>
    public TimeSpan AverageDuration => Executions == 0 ? TimeSpan.Zero : TotalDuration / Executions;
    #endregion
}
