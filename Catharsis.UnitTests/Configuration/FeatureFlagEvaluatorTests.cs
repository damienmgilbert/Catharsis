using Catharsis.Configuration;

namespace Catharsis.UnitTests.Configuration;

///<summary>
///Unit tests for the <see cref="FeatureFlagEvaluator"/> class.
///</summary>
[TestClass]
public class FeatureFlagEvaluatorTests
{
    #region Constructor

    [TestMethod]
    public void Constructor_NullFlags_Throws() { Assert.ThrowsExactly<ArgumentNullException>(static () => new FeatureFlagEvaluator(null!)); }

    #endregion

    #region IsEnabled

    [TestMethod]
    public void IsEnabled_NullFlagName_Throws()
    {
        FeatureFlagEvaluator evaluator = new([]);
        Assert.ThrowsExactly<ArgumentNullException>(() => evaluator.IsEnabled(null!));
    }

    [TestMethod]
    public void IsEnabled_UnknownFlag_ReturnsFalse()
    {
        FeatureFlagEvaluator evaluator = new([]);
        Assert.IsFalse(evaluator.IsEnabled("unknown"));
    }

    [TestMethod]
    public void IsEnabled_DisabledFlag_ReturnsFalse()
    {
        FeatureFlagEvaluator evaluator = new([new FeatureFlag("beta", Enabled: false)]);
        Assert.IsFalse(evaluator.IsEnabled("beta"));
    }

    [TestMethod]
    public void IsEnabled_FullRollout_ReturnsTrue()
    {
        FeatureFlagEvaluator evaluator = new([new FeatureFlag("beta", Enabled: true, RolloutPercentage: 100)]);
        Assert.IsTrue(evaluator.IsEnabled("beta"));
    }

    [TestMethod]
    public void IsEnabled_ZeroRollout_ReturnsFalse()
    {
        FeatureFlagEvaluator evaluator = new([new FeatureFlag("beta", Enabled: true, RolloutPercentage: 0)]);
        Assert.IsFalse(evaluator.IsEnabled("beta"));
    }

    [TestMethod]
    public void IsEnabled_FlagNameLookup_IsCaseInsensitive()
    {
        FeatureFlagEvaluator evaluator = new([new FeatureFlag("Beta", Enabled: true, RolloutPercentage: 100)]);
        Assert.IsTrue(evaluator.IsEnabled("beta"));
    }

    [TestMethod]
    public void IsEnabled_PartialRollout_SameStickyId_IsDeterministic()
    {
        FeatureFlagEvaluator evaluator = new([new FeatureFlag("beta", Enabled: true, RolloutPercentage: 50)]);

        bool first = evaluator.IsEnabled("beta", "user-123");
        bool second = evaluator.IsEnabled("beta", "user-123");

        Assert.AreEqual(first, second);
    }

    [TestMethod]
    public void IsEnabled_PartialRollout_DifferentStickyIds_ProduceAMix()
    {
        FeatureFlagEvaluator evaluator = new([new FeatureFlag("beta", Enabled: true, RolloutPercentage: 50)]);

        int enabledCount = 0;

        for (int i = 0; i < 200; i++)
        {
            if (evaluator.IsEnabled("beta", $"user-{i}"))
            {
                enabledCount++;
            }
        }

        Assert.IsTrue((enabledCount > 0) && (enabledCount < 200));
    }

    #endregion
}
