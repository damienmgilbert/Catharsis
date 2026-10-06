namespace Catharsis.Configuration;

///<summary>
///Describes a single feature flag: whether it is enabled at all, and if so, what percentage of evaluations should
///see it as enabled.
///</summary>
///<param name="Name">The flag's unique name.</param>
///<param name="Enabled">Whether the flag is enabled at all. When <c>false</c>, <paramref name="RolloutPercentage"/> is ignored.</param>
///<param name="RolloutPercentage">The percentage (0-100) of evaluations that should see the flag as enabled. Defaults to 100 (everyone).</param>
public sealed record FeatureFlag(string Name, bool Enabled, int RolloutPercentage = 100);
