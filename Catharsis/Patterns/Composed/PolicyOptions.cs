namespace Catharsis.Patterns.Composed;

///<summary>
///Describes the resilience behaviour <see cref="PolicyFactory"/> should assemble.
///</summary>
public sealed record PolicyOptions
{
    #region Public properties
    ///<summary>
    ///Gets the maximum number of attempts, including the first. A value of 1 (the default) means no retry.
    ///</summary>
    public int MaxAttempts { get; init; } = 1;

    ///<summary>Gets the delay before the first retry; later retries back off exponentially.</summary>
    public TimeSpan InitialRetryDelay { get; init; } = TimeSpan.FromMilliseconds(200);

    ///<summary>Gets a value indicating whether retry delays are randomly reduced to avoid synchronised retries.</summary>
    public bool UseJitter { get; init; }

    ///<summary>Gets the time limit for each attempt, or <c>null</c> for none.</summary>
    public TimeSpan? Timeout { get; init; }
    #endregion
}
