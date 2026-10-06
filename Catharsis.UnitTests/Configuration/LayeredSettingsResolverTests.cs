using Catharsis.Configuration;

namespace Catharsis.UnitTests.Configuration;

///<summary>
///Unit tests for the <see cref="LayeredSettingsResolver"/> class.
///</summary>
[TestClass]
public class LayeredSettingsResolverTests
{
    #region Constructor

    [TestMethod]
    public void Constructor_NullLayers_Throws() { Assert.ThrowsExactly<ArgumentNullException>(static () => new LayeredSettingsResolver((IReadOnlyDictionary<string, string?>[])null!)); }

    #endregion

    #region TryGetValue / GetValue

    [TestMethod]
    public void TryGetValue_NullKey_Throws()
    {
        LayeredSettingsResolver resolver = new();
        Assert.ThrowsExactly<ArgumentNullException>(() => resolver.TryGetValue(null!, out _));
    }

    [TestMethod]
    public void TryGetValue_NoLayers_ReturnsFalse()
    {
        LayeredSettingsResolver resolver = new();
        Assert.IsFalse(resolver.TryGetValue("Key", out _));
    }

    [TestMethod]
    public void TryGetValue_FirstLayerHasKey_ReturnsFirstLayerValue()
    {
        Dictionary<string, string?> highest = new() { ["Key"] = "from-highest" };
        Dictionary<string, string?> lowest = new() { ["Key"] = "from-lowest" };
        LayeredSettingsResolver resolver = new(highest, lowest);

        Assert.IsTrue(resolver.TryGetValue("Key", out string? value));
        Assert.AreEqual("from-highest", value);
    }

    [TestMethod]
    public void TryGetValue_OnlyLowerLayerHasKey_FallsThrough()
    {
        Dictionary<string, string?> highest = [];
        Dictionary<string, string?> lowest = new() { ["Key"] = "from-lowest" };
        LayeredSettingsResolver resolver = new(highest, lowest);

        Assert.IsTrue(resolver.TryGetValue("Key", out string? value));
        Assert.AreEqual("from-lowest", value);
    }

    [TestMethod]
    public void TryGetValue_NoLayerHasKey_ReturnsFalse()
    {
        Dictionary<string, string?> layer = new() { ["Other"] = "value" };
        LayeredSettingsResolver resolver = new(layer);

        Assert.IsFalse(resolver.TryGetValue("Key", out _));
    }

    [TestMethod]
    public void GetValue_KeyNotFound_ReturnsDefault()
    {
        LayeredSettingsResolver resolver = new();
        Assert.AreEqual("fallback", resolver.GetValue("Key", "fallback"));
    }

    [TestMethod]
    public void GetValue_KeyFound_ReturnsResolvedValue()
    {
        Dictionary<string, string?> layer = new() { ["Key"] = "resolved" };
        LayeredSettingsResolver resolver = new(layer);

        Assert.AreEqual("resolved", resolver.GetValue("Key", "fallback"));
    }

    #endregion

    #region FromEnvironmentAndDefaults

    [TestMethod]
    public void FromEnvironmentAndDefaults_NullDefaults_Throws() { Assert.ThrowsExactly<ArgumentNullException>(static () => LayeredSettingsResolver.FromEnvironmentAndDefaults(null!)); }

    [TestMethod]
    public void FromEnvironmentAndDefaults_KeyOnlyInDefaults_ReturnsDefaultValue()
    {
        Dictionary<string, string?> defaults = new() { ["Catharsis_Test_Unlikely_Key"] = "default-value" };
        LayeredSettingsResolver resolver = LayeredSettingsResolver.FromEnvironmentAndDefaults(defaults);

        Assert.AreEqual("default-value", resolver.GetValue("Catharsis_Test_Unlikely_Key"));
    }

    [TestMethod]
    public void FromEnvironmentAndDefaults_EnvironmentVariableTakesPrecedence()
    {
        const string key = "CATHARSIS_TEST_LAYERED_SETTINGS_KEY";
        Environment.SetEnvironmentVariable(key, "from-env");

        try
        {
            Dictionary<string, string?> defaults = new() { [key] = "from-defaults" };
            LayeredSettingsResolver resolver = LayeredSettingsResolver.FromEnvironmentAndDefaults(defaults);

            Assert.AreEqual("from-env", resolver.GetValue(key));
        }
        finally
        {
            Environment.SetEnvironmentVariable(key, null);
        }
    }

    #endregion
}
