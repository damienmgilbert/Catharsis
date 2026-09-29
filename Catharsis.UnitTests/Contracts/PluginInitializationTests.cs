using Catharsis.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace Catharsis.UnitTests.Contracts;

///<summary>
///Unit tests for <see cref="IPlugin"/> startup through <see cref="PluginLoader.InitializeAllAsync"/>.
///</summary>
[TestClass]
public class PluginInitializationTests
{
    sealed class Log { public List<string> Lines { get; } = []; }

    [Plugin("b")]
    sealed class SecondPlugin(Log log) : IPlugin
    {
        public Task InitializeAsync(IServiceProvider services, CancellationToken cancellationToken = default)
        {
            log.Lines.Add("b");
            return Task.CompletedTask;
        }
    }

    [Plugin("a")]
    sealed class FirstPlugin(Log log) : IPlugin
    {
        public Task InitializeAsync(IServiceProvider services, CancellationToken cancellationToken = default)
        {
            log.Lines.Add("a");
            return Task.CompletedTask;
        }
    }

    static MiniServiceProvider Services(Log log)
    {
        ServiceCollection services = [];
        services.AddSingleton(log);
        return new MiniServiceProvider(services);
    }

    [TestMethod]
    public async Task InitializeAllAsync_CreatesAndInitializesInDescriptorOrder()
    {
        Log log = new();
        IReadOnlyList<PluginDescriptor> found = PluginLoader.Discover<IPlugin>([typeof(SecondPlugin), typeof(FirstPlugin)]);

        IReadOnlyList<IPlugin> started = await PluginLoader.InitializeAllAsync(found, Services(log));

        Assert.AreEqual(2, started.Count);
        CollectionAssert.AreEqual(new[] { "a", "b" }, log.Lines);
    }

    [TestMethod]
    public async Task InitializeAllAsync_Cancelled_StopsBeforeStarting()
    {
        Log log = new();
        using CancellationTokenSource cts = new();
        cts.Cancel();

        await Assert.ThrowsExactlyAsync<OperationCanceledException>(async () => await PluginLoader.InitializeAllAsync(PluginLoader.Discover<IPlugin>([typeof(FirstPlugin)]), Services(log), cts.Token));

        Assert.AreEqual(0, log.Lines.Count);
    }

    [TestMethod]
    public async Task InitializeAllAsync_NullArguments_Throw()
    {
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(async () => await PluginLoader.InitializeAllAsync(null!, Services(new Log())));
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(async () => await PluginLoader.InitializeAllAsync([], null!));
    }
}
