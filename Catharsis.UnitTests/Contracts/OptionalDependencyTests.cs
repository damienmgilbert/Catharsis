using Catharsis.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace Catharsis.UnitTests.Contracts;

///<summary>
///Unit tests for <see cref="OptionalDependencyAttribute"/> and <see cref="OptionalDependencyResolver"/>.
///</summary>
[TestClass]
public class OptionalDependencyTests
{
    interface IClock { string Now { get; } }

    sealed class FixedClock : IClock { public string Now => "noon"; }

    sealed class Audit { }

    [Service]
    sealed class Reporter(IClock clock, [OptionalDependency] Audit? audit)
    {
        public string Clock => clock.Now;
        public bool HasAudit => audit is not null;
    }

    sealed class NeedsAudit(Audit audit) { public Audit Audit { get; } = audit; }

    sealed class WithDefault(int retries = 3) { public int Retries { get; } = retries; }

    sealed class ValueOptional([OptionalDependency] int limit) { public int Limit { get; } = limit; }

    sealed class Plain(IClock clock) { public IClock Clock { get; } = clock; }

    static MiniServiceProvider Provider(Action<IServiceCollection> configure)
    {
        ServiceCollection services = [];
        configure(services);
        return new MiniServiceProvider(services);
    }

    [TestMethod]
    public void Create_OptionalMissing_PassesNull()
    {
        Reporter reporter = (Reporter)OptionalDependencyResolver.Create(Provider(static s => s.AddSingleton<IClock, FixedClock>()), typeof(Reporter));

        Assert.AreEqual("noon", reporter.Clock);
        Assert.IsFalse(reporter.HasAudit);
    }

    [TestMethod]
    public void Create_OptionalPresent_IsInjected()
    {
        Reporter reporter = (Reporter)OptionalDependencyResolver.Create(Provider(static s => s.AddSingleton<IClock, FixedClock>().AddSingleton<Audit>()), typeof(Reporter));

        Assert.IsTrue(reporter.HasAudit);
    }

    [TestMethod]
    public void Create_RequiredMissing_Throws() { Assert.ThrowsExactly<InvalidOperationException>(static () => OptionalDependencyResolver.Create(Provider(static _ => { }), typeof(NeedsAudit))); }

    [TestMethod]
    public void Create_CSharpDefaultParameter_UsesDefault() { Assert.AreEqual(3, ((WithDefault)OptionalDependencyResolver.Create(Provider(static _ => { }), typeof(WithDefault))).Retries); }

    [TestMethod]
    public void Create_OptionalValueType_UsesDefaultValue() { Assert.AreEqual(0, ((ValueOptional)OptionalDependencyResolver.Create(Provider(static _ => { }), typeof(ValueOptional))).Limit); }

    [TestMethod]
    public void Create_NullArguments_Throw()
    {
        Assert.ThrowsExactly<ArgumentNullException>(static () => OptionalDependencyResolver.Create(null!, typeof(Plain)));
        Assert.ThrowsExactly<ArgumentNullException>(static () => OptionalDependencyResolver.Create(Provider(static _ => { }), null!));
    }

    [TestMethod]
    public void HasOptionalDependencies_DetectsAttribute()
    {
        Assert.IsTrue(OptionalDependencyResolver.HasOptionalDependencies(typeof(Reporter)));
        Assert.IsFalse(OptionalDependencyResolver.HasOptionalDependencies(typeof(Plain)));
    }

    [TestMethod]
    public void AddAttributedServices_RoutesOptionalServicesThroughResolver()
    {
        IServiceCollection services = new ServiceCollection();
        services.AddSingleton<IClock, FixedClock>();
        services.AddAttributedServices([typeof(Reporter)]);

        Reporter reporter = (Reporter)new MiniServiceProvider(services).GetService(typeof(Reporter))!;

        Assert.IsFalse(reporter.HasAudit);
    }
}
