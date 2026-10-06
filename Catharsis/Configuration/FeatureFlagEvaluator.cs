using System.Text;

namespace Catharsis.Configuration;

///<summary>
///Evaluates whether a feature flag is enabled for a given caller, supporting both simple on/off flags and
///percentage-based gradual rollouts.
///</summary>
///<param name="flags">The flags this evaluator knows about.</param>
///<exception cref="ArgumentNullException"><paramref name="flags"/> is <c>null</c>.</exception>
public sealed class FeatureFlagEvaluator(IEnumerable<FeatureFlag> flags)
{
    #region Fields
    readonly Dictionary<string, FeatureFlag> _flags = (flags ?? throw new ArgumentNullException(nameof(flags))).ToDictionary(static flag => flag.Name, StringComparer.OrdinalIgnoreCase);
    #endregion

    #region Private methods
    ///<remarks>
    ///<see cref="string.GetHashCode()"/> is randomized per process for DoS-hardening, so it cannot be used here:
    ///the same caller must land in the same bucket every time, including across app restarts and different
    ///servers in a fleet. FNV-1a gives a small, simple, and fully deterministic alternative.
    ///</remarks>
    static uint StableHash(string value)
    {
        ReadOnlySpan<byte> bytes = Encoding.UTF8.GetBytes(value);
        uint hash = 2166136261;

        foreach(byte b in bytes)
        {
            hash ^= b;
            hash *= 16777619;
        }

        return hash;
    }
    #endregion

    #region Public methods
    ///<summary>
    ///Determines whether the specified flag is enabled for the given caller.
    ///</summary>
    ///<param name="flagName">The name of the flag to evaluate.</param>
    ///<param name="stickyId">
    ///An identifier (e.g. a user or session ID) used to deterministically bucket percentage rollouts, so the same
    ///caller consistently gets the same result. When <c>null</c>, the flag name itself is used, meaning every
    ///caller gets the same result for that flag.
    ///</param>
    ///<returns><c>true</c> if the flag is enabled for this caller; otherwise <c>false</c>.</returns>
    ///<exception cref="ArgumentException"><paramref name="flagName"/> is <c>null</c>, empty, or whitespace.</exception>
    public bool IsEnabled(string flagName, string? stickyId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(flagName);

        if(!_flags.TryGetValue(flagName, out FeatureFlag? flag) || !flag.Enabled)
        {
            return false;
        }

        if(flag.RolloutPercentage >= 100)
        {
            return true;
        }

        if(flag.RolloutPercentage <= 0)
        {
            return false;
        }

        int bucket = (int)(StableHash(stickyId ?? flagName) % 100);
        return bucket < flag.RolloutPercentage;
    }
    #endregion
}
