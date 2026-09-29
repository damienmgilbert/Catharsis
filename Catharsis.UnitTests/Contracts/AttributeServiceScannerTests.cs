using Catharsis.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace Catharsis.UnitTests.Contracts;

///<summary>
///Unit tests for <see cref="AttributeServiceScanner"/>.
///</summary>
[TestClass]
public class AttributeServiceScannerTests
{
    interface IGreeter { string Greet(); }

    [Service(ServiceLifetime.Singleton, ServiceType = typeof(IGreeter))]
    sealed class PlainGreeter : IGreeter { public string Greet() => "hello"; }

    [Service]
    sealed class SelfRegistered { }

    sealed class NotMarked { }

    [Service(ServiceType = typeof(IGreeter))]
    sealed class WrongService { }

    [Service]
    abstract class AbstractMarked { }

    [DecoratorFor(typeof(IGreeter), Order = 2)]
    sealed class OuterDecorator(IGreeter inner) : IGreeter { public string Greet() => $"[{inner.Greet()}]"; }

    [DecoratorFor(typeof(IGreeter), Order = 1)]
    sealed class InnerDecorator(IGreeter inner) : IGreeter { public string Greet() => $"<{inner.Greet()}>"; }

    [DecoratorFor(typeof(IGreeter))]
    sealed class BrokenDecorator { }

    #region ScanServices

    [TestMethod]
    public void ScanServices_NullTypes_Throws() { Assert.ThrowsExactly<ArgumentNullException>(static () => AttributeServiceScanner.ScanServices((IEnumerable<Type>)null!)); }

    [TestMethod]
    public void ScanServices_FindsMarkedConcreteClassesOnly()
    {
        IReadOnlyList<ServiceRegistration> found = AttributeServiceScanner.ScanServices([typeof(PlainGreeter), typeof(SelfRegistered), typeof(NotMarked), typeof(AbstractMarked), typeof(IGreeter)]);

        Assert.AreEqual(2, found.Count);
        Assert.IsTrue(found.Contains(new ServiceRegistration(typeof(IGreeter), typeof(PlainGreeter), ServiceLifetime.Singleton)));
        Assert.IsTrue(found.Contains(new ServiceRegistration(typeof(SelfRegistered), typeof(SelfRegistered), ServiceLifetime.Transient)));
    }

    [TestMethod]
    public void ScanServices_ServiceTypeNotImplemented_Throws() { Assert.ThrowsExactly<InvalidOperationException>(static () => AttributeServiceScanner.ScanServices([typeof(WrongService)])); }

    [TestMethod]
    public void ScanServices_Assembly_ScansNestedTypesAndReportsInvalidFixture()
    {
        // The test assembly deliberately contains an invalid nested fixture (WrongService), so scanning it whole must report it.
        Assert.ThrowsExactly<InvalidOperationException>(static () => AttributeServiceScanner.ScanServices(typeof(AttributeServiceScannerTests).Assembly));
    }

    #endregion

    #region ScanDecorators

    [TestMethod]
    public void ScanDecorators_OrdersByOrder()
    {
        IReadOnlyList<DecoratorRegistration> found = AttributeServiceScanner.ScanDecorators([typeof(OuterDecorator), typeof(InnerDecorator)]);

        Assert.AreEqual(typeof(InnerDecorator), found[0].DecoratorType);
        Assert.AreEqual(typeof(OuterDecorator), found[1].DecoratorType);
    }

    [TestMethod]
    public void ScanDecorators_DecoratorDoesNotImplementService_Throws() { Assert.ThrowsExactly<InvalidOperationException>(static () => AttributeServiceScanner.ScanDecorators([typeof(BrokenDecorator)])); }

    #endregion

    #region Registration

    [TestMethod]
    public void AddDecorator_WrapsRegistration_AndKeepsLifetime()
    {
        ServiceCollection services = [];
        services.AddSingleton<IGreeter, PlainGreeter>();
        services.AddDecorator<IGreeter, InnerDecorator>();

        Assert.AreEqual(1, services.Count);
        Assert.AreEqual(ServiceLifetime.Singleton, services[0].Lifetime);
        Assert.AreEqual("<hello>", ((IGreeter)new MiniServiceProvider(services).GetService(typeof(IGreeter))!).Greet());
    }

    [TestMethod]
    public void AddDecorator_StackedDecorators_WrapOutward()
    {
        ServiceCollection services = [];
        services.AddTransient<IGreeter, PlainGreeter>();
        services.AddDecorator<IGreeter, InnerDecorator>();
        services.AddDecorator<IGreeter, OuterDecorator>();

        Assert.AreEqual("[<hello>]", ((IGreeter)new MiniServiceProvider(services).GetService(typeof(IGreeter))!).Greet());
    }

    [TestMethod]
    public void AddDecorator_NoRegistration_Throws() { Assert.ThrowsExactly<InvalidOperationException>(static () => new ServiceCollection().AddDecorator<IGreeter, InnerDecorator>()); }

    [TestMethod]
    public void AddDecorator_AbstractDecorator_Throws() { Assert.ThrowsExactly<ArgumentException>(static () => new ServiceCollection().AddDecorator(typeof(AbstractMarked), typeof(AbstractMarked))); }

    [TestMethod]
    public void AddDecorator_InstanceRegistration_WrapsInstance()
    {
        ServiceCollection services = [];
        services.AddSingleton<IGreeter>(new PlainGreeter());
        services.AddDecorator<IGreeter, OuterDecorator>();

        Assert.AreEqual("[hello]", ((IGreeter)new MiniServiceProvider(services).GetService(typeof(IGreeter))!).Greet());
    }

    [TestMethod]
    public void AddFactory_ResolvesFreshInstancePerCall()
    {
        ServiceCollection services = [];
        services.AddTransient<SelfRegistered>();
        services.AddFactory<SelfRegistered>();

        Func<SelfRegistered> factory = (Func<SelfRegistered>)new MiniServiceProvider(services).GetService(typeof(Func<SelfRegistered>))!;

        Assert.AreNotSame(factory(), factory());
    }

    [TestMethod]
    public void AddAttributedServices_NullArguments_Throw()
    {
        Assert.ThrowsExactly<ArgumentNullException>(static () => ((IServiceCollection)null!).AddAttributedServices());
        Assert.ThrowsExactly<ArgumentNullException>(static () => new ServiceCollection().AddAttributedServices((System.Reflection.Assembly[])null!));
        Assert.ThrowsExactly<ArgumentNullException>(static () => new ServiceCollection().AddAttributedServices((IEnumerable<Type>)null!));
    }

    [TestMethod]
    public void AddAttributedServices_NoAssemblies_LeavesCollectionUnchanged()
    {
        ServiceCollection services = [];
        services.AddAttributedServices();

        Assert.AreEqual(0, services.Count);
    }

    #endregion
}
