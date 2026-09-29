using Catharsis.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace Catharsis.UnitTests.Contracts;

///<summary>
///Unit tests for <see cref="PluginLoader"/>.
///</summary>
[TestClass]
public class PluginLoaderTests
{
    interface IPluginContract { string Run(); }

    sealed class Dependency { public string Value => "dep"; }

    [Plugin("beta", Version = "2.0")]
    sealed class BetaPlugin : IPluginContract { public string Run() => "beta"; }

    [Plugin("alpha")]
    sealed class AlphaPlugin : IPluginContract { public string Run() => "alpha"; }

    [Plugin("needs-dep")]
    sealed class DependentPlugin(Dependency dependency) : IPluginContract { public string Run() => dependency.Value; }

    [Plugin("other")]
    sealed class NotAContractPlugin { }

    sealed class UnmarkedPlugin : IPluginContract { public string Run() => "x"; }

    [Plugin("ALPHA")]
    sealed class DuplicateNamePlugin : IPluginContract { public string Run() => "dup"; }

    [TestMethod]
    public void PluginAttribute_EmptyName_Throws() { Assert.ThrowsExactly<ArgumentException>(static () => new PluginAttribute(" ")); }

    [TestMethod]
    public void Discover_FiltersByContractAndAttribute_OrderedByName()
    {
        IReadOnlyList<PluginDescriptor> found = PluginLoader.Discover<IPluginContract>([typeof(BetaPlugin), typeof(AlphaPlugin), typeof(NotAContractPlugin), typeof(UnmarkedPlugin)]);

        CollectionAssert.AreEqual(new[] { "alpha", "beta" }, found.Select(static d => d.Name).ToArray());
        Assert.AreEqual("2.0", found[1].Version);
        Assert.AreEqual("1.0", found[0].Version);
    }

    [TestMethod]
    public void Discover_DuplicateNamesIgnoringCase_Throws() { Assert.ThrowsExactly<InvalidOperationException>(static () => PluginLoader.Discover<IPluginContract>([typeof(AlphaPlugin), typeof(DuplicateNamePlugin)])); }

    [TestMethod]
    public void Create_ParameterlessPlugin_WithoutProvider_Works()
    {
        PluginDescriptor descriptor = PluginLoader.Discover<IPluginContract>([typeof(AlphaPlugin)])[0];

        Assert.AreEqual("alpha", PluginLoader.Create<IPluginContract>(descriptor).Run());
    }

    [TestMethod]
    public void Create_WithProvider_InjectsDependencies()
    {
        ServiceCollection services = [];
        services.AddSingleton<Dependency>();
        PluginDescriptor descriptor = PluginLoader.Discover<IPluginContract>([typeof(DependentPlugin)])[0];

        Assert.AreEqual("dep", PluginLoader.Create<IPluginContract>(descriptor, new MiniServiceProvider(services)).Run());
    }

    [TestMethod]
    public void Create_NullDescriptor_Throws() { Assert.ThrowsExactly<ArgumentNullException>(static () => PluginLoader.Create<IPluginContract>(null!)); }
}
