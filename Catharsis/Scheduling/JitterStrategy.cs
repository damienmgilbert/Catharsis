namespace Catharsis.Scheduling;

///<summary>
///The jitter strategy applied by <see cref="JitteredDelayCalculator"/> when computing a backoff delay, following the
///strategies described in the AWS Architecture Blog post "Exponential Backoff and Jitter".
///</summary>
public enum JitterStrategy
{
    ///<summary>
    ///No jitter: the delay is exactly the capped exponential value.
    ///</summary>
    None,

    ///<summary>
    ///Full jitter: the delay is a uniformly random value between zero and the capped exponential value.
    ///</summary>
    Full,

    ///<summary>
    ///Equal jitter: the delay is half the capped exponential value, plus a uniformly random amount up to the other
    ///half.
    ///</summary>
    Equal
}
