using Catharsis.ComponentModel;

namespace Catharsis.UnitTests.ComponentModel;

///<summary>
///Tests for <see cref="ComponentServiceProvider"/>.
///</summary>
[TestClass]
public sealed class ComponentServiceProviderTests
{
    #region Public methods

    ///<summary>
    ///Tests that the constructor successfully creates an instance when no parent parameter is provided (default).
    ///</summary>
    [TestMethod]
    public void ComponentServiceProvider_WithDefaultParameter_CreatesInstance()
    {
        // Arrange & Act
        ComponentServiceProvider provider = new ComponentServiceProvider();

        // Assert
        Assert.IsNotNull(provider);
        Assert.IsInstanceOfType<ComponentServiceProvider>(provider);
    }

    ///<summary>
    ///Tests that the constructor successfully creates an instance when parent parameter is explicitly null.
    ///</summary>
    [TestMethod]
    public void ComponentServiceProvider_WithNullParent_CreatesInstance()
    {
        // Arrange
        IServiceProvider? parent = null;

        // Act
        ComponentServiceProvider provider = new ComponentServiceProvider(parent);

        // Assert
        Assert.IsNotNull(provider);
        Assert.IsInstanceOfType<ComponentServiceProvider>(provider);
    }

    ///<summary>
    ///Tests that the constructor successfully creates an instance when a valid parent IServiceProvider is provided.
    ///</summary>
    [TestMethod]
    public void ComponentServiceProvider_WithValidParent_CreatesInstance()
    {
        // Arrange
        StubServiceProvider parentStub = new StubServiceProvider();

        // Act
        ComponentServiceProvider provider = new ComponentServiceProvider(parentStub);

        // Assert
        Assert.IsNotNull(provider);
        Assert.IsInstanceOfType<ComponentServiceProvider>(provider);
    }

    ///<summary>
    ///Tests that GetService prefers instance registration over factory registration for the same type.
    ///</summary>
    [TestMethod]
    public void GetService_BothInstanceAndFactoryRegistered_ReturnsInstance()
    {
        // Arrange
        TestService instanceService = new();
        TestService factoryService = new();
        ComponentServiceProvider provider = new();
        provider.Register<ITestService>(() => factoryService);
        provider.Register<ITestService>(instanceService);

        // Act
        object? result = provider.GetService(typeof(ITestService));

        // Assert
        Assert.AreSame(instanceService, result);
    }

    ///<summary>
    ///Tests that GetService returns null when factory returns null.
    ///</summary>
    [TestMethod]
    public void GetService_FactoryReturnsNull_ReturnsNull()
    {
        // Arrange
        ComponentServiceProvider provider = new();
        provider.Register<ITestService>(() => null!);

        // Act
        object? result = provider.GetService(typeof(ITestService));

        // Assert
        Assert.IsNull(result);
    }

    ///<summary>
    ///Tests that GetService uses factory when instance was registered first but then replaced with factory.
    ///</summary>
    [TestMethod]
    public void GetService_InstanceReplacedWithFactory_ReturnsFactoryResult()
    {
        // Arrange
        TestService instanceService = new();
        TestService factoryService = new();
        ComponentServiceProvider provider = new();
        provider.Register<ITestService>(instanceService);
        provider.Register<ITestService>(() => factoryService);

        // Act
        object? result = provider.GetService(typeof(ITestService));

        // Assert
        Assert.AreSame(factoryService, result);
    }

    ///<summary>
    ///Tests that GetService prefers local factory registration over parent provider.
    ///</summary>
    [TestMethod]
    public void GetService_LocalFactoryAndParentBothRegistered_ReturnsLocalFactoryResult()
    {
        // Arrange
        TestService localService = new();
        TestService parentService = new();
        StubServiceProvider parentStub = new StubServiceProvider();
        parentStub.Services[typeof(ITestService)] = parentService;
        ComponentServiceProvider provider = new(parentStub);
        provider.Register<ITestService>(() => localService);

        // Act
        object? result = provider.GetService(typeof(ITestService));

        // Assert
        Assert.AreSame(localService, result);
        Assert.AreEqual(0, parentStub.GetServiceCallCount);
    }

    ///<summary>
    ///Tests that GetService prefers local instance registration over parent provider.
    ///</summary>
    [TestMethod]
    public void GetService_LocalInstanceAndParentBothRegistered_ReturnsLocalInstance()
    {
        // Arrange
        TestService localService = new();
        TestService parentService = new();
        StubServiceProvider parentStub = new StubServiceProvider();
        parentStub.Services[typeof(ITestService)] = parentService;
        ComponentServiceProvider provider = new(parentStub);
        provider.Register<ITestService>(localService);

        // Act
        object? result = provider.GetService(typeof(ITestService));

        // Assert
        Assert.AreSame(localService, result);
        Assert.AreEqual(0, parentStub.GetServiceCallCount);
    }

    ///<summary>
    ///Tests that GetService returns null when service is not registered locally and no parent provider exists.
    ///</summary>
    [TestMethod]
    public void GetService_NotRegisteredAndNoParent_ReturnsNull()
    {
        // Arrange
        ComponentServiceProvider provider = new();

        // Act
        object? result = provider.GetService(typeof(ITestService));

        // Assert
        Assert.IsNull(result);
    }

    ///<summary>
    ///Tests that GetService returns null when service is not registered locally and parent returns null.
    ///</summary>
    [TestMethod]
    public void GetService_NotRegisteredAndParentReturnsNull_ReturnsNull()
    {
        // Arrange
        StubServiceProvider parentStub = new StubServiceProvider();
        ComponentServiceProvider provider = new(parentStub);

        // Act
        object? result = provider.GetService(typeof(ITestService));

        // Assert
        Assert.IsNull(result);
    }

    ///<summary>
    ///Tests that GetService delegates to parent provider when service is not registered locally.
    ///</summary>
    [TestMethod]
    public void GetService_NotRegisteredLocallyButInParent_ReturnsFromParent()
    {
        // Arrange
        TestService expectedService = new();
        StubServiceProvider parentStub = new StubServiceProvider();
        parentStub.Services[typeof(ITestService)] = expectedService;
        ComponentServiceProvider provider = new(parentStub);

        // Act
        object? result = provider.GetService(typeof(ITestService));

        // Assert
        Assert.IsNotNull(result);
        Assert.AreSame(expectedService, result);
        Assert.AreEqual(1, parentStub.GetServiceCallCount);
    }

    ///<summary>
    ///Tests that GetService invokes the factory and returns the result when a factory is registered.
    ///</summary>
    [TestMethod]
    public void GetService_RegisteredFactory_InvokesFactoryAndReturnsResult()
    {
        // Arrange
        ComponentServiceProvider provider = new();
        TestService expectedService = new();
        bool factoryInvoked = false;
        provider.Register<ITestService>(
        () =>
        {
            factoryInvoked = true;
            return expectedService;
        });

        // Act
        object? result = provider.GetService(typeof(ITestService));

        // Assert
        Assert.IsTrue(factoryInvoked);
        Assert.IsNotNull(result);
        Assert.AreSame(expectedService, result);
    }

    ///<summary>
    ///Tests that GetService invokes the factory each time it is called.
    ///</summary>
    [TestMethod]
    public void GetService_RegisteredFactory_InvokesFactoryEachTime()
    {
        // Arrange
        ComponentServiceProvider provider = new();
        int invocationCount = 0;
        provider.Register<ITestService>(
        () =>
        {
            invocationCount++;
            return new TestService();
        });

        // Act
        provider.GetService(typeof(ITestService));
        provider.GetService(typeof(ITestService));

        // Assert
        Assert.AreEqual(2, invocationCount);
    }

    ///<summary>
    ///Tests that GetService returns a registered instance when the service type is registered.
    ///</summary>
    [TestMethod]
    public void GetService_RegisteredInstance_ReturnsInstance()
    {
        // Arrange
        ComponentServiceProvider provider = new();
        TestService expectedService = new();
        provider.Register<ITestService>(expectedService);

        // Act
        object? result = provider.GetService(typeof(ITestService));

        // Assert
        Assert.IsNotNull(result);
        Assert.AreSame(expectedService, result);
    }

    ///<summary>
    ///Tests that GetService returns the provider itself when requesting IServiceProvider type.
    ///</summary>
    [TestMethod]
    public void GetService_RequestingIServiceProvider_ReturnsThis()
    {
        // Arrange
        ComponentServiceProvider provider = new();

        // Act
        object? result = provider.GetService(typeof(IServiceProvider));

        // Assert
        Assert.IsNotNull(result);
        Assert.AreSame(provider, result);
    }

    ///<summary>
    ///Tests that IsRegistered correctly handles different generic type arguments as distinct types.
    ///</summary>
    [TestMethod]
    public void IsRegistered_DifferentGenericTypeArguments_TreatedAsDistinctTypes()
    {
        // Arrange
        ComponentServiceProvider provider = new();
        System.Collections.Generic.IList<string> stringListService = new List<string>();
        provider.Register(stringListService);

        // Act
        bool stringListRegistered = provider.IsRegistered<System.Collections.Generic.IList<string>>();
        bool intListRegistered = provider.IsRegistered<System.Collections.Generic.IList<int>>();

        // Assert
        Assert.IsTrue(stringListRegistered);
        Assert.IsFalse(intListRegistered);
    }

    ///<summary>
    ///Tests that IsRegistered correctly distinguishes between different service types.
    ///</summary>
    [TestMethod]
    public void IsRegistered_MultipleServiceTypes_CorrectlyIdentifiesEachType()
    {
        // Arrange
        ComponentServiceProvider provider = new();
        IDisposable disposableService = new StubDisposable();
        provider.Register(disposableService);

        // Act
        bool disposableRegistered = provider.IsRegistered<IDisposable>();
        bool comparableRegistered = provider.IsRegistered<IComparable>();

        // Assert
        Assert.IsTrue(disposableRegistered);
        Assert.IsFalse(comparableRegistered);
    }

    [TestMethod]
    public void IsRegistered_NoServiceRegistered_ReturnsFalse()
    {
        // Arrange
        ComponentServiceProvider provider = new();

        // Act
        bool result = provider.IsRegistered<IDisposable>();

        // Assert
        Assert.IsFalse(result);
    }

    ///<summary>
    ///Tests that IsRegistered returns true when a service is registered locally, even if the parent provider also has a
    ///registration for the same type.
    ///</summary>
    [TestMethod]
    public void IsRegistered_ServiceInBothLocalAndParent_ReturnsTrue()
    {
        // Arrange
        StubServiceProvider parentStub = new StubServiceProvider();
        parentStub.Services[typeof(IDisposable)] = new StubDisposable();
        ComponentServiceProvider provider = new(parentStub);
        IDisposable localService = new StubDisposable();
        provider.Register(localService);

        // Act
        bool result = provider.IsRegistered<IDisposable>();

        // Assert
        Assert.IsTrue(result);
    }

    ///<summary>
    ///Tests that IsRegistered returns false when a service is only registered in the parent provider. This verifies
    ///that IsRegistered only checks local registrations.
    ///</summary>
    [TestMethod]
    public void IsRegistered_ServiceOnlyInParentProvider_ReturnsFalse()
    {
        // Arrange
        StubServiceProvider parentStub = new StubServiceProvider();
        parentStub.Services[typeof(IDisposable)] = new StubDisposable();
        ComponentServiceProvider provider = new(parentStub);

        // Act
        bool result = provider.IsRegistered<IDisposable>();

        // Assert
        Assert.IsFalse(result);
    }

    ///<summary>
    ///Tests that IsRegistered returns true when a service is registered as a factory.
    ///</summary>
    [TestMethod]
    public void IsRegistered_ServiceRegisteredAsFactory_ReturnsTrue()
    {
        // Arrange
        ComponentServiceProvider provider = new();
        provider.Register<IDisposable>(() => new StubDisposable());

        // Act
        bool result = provider.IsRegistered<IDisposable>();

        // Assert
        Assert.IsTrue(result);
    }

    ///<summary>
    ///Tests that IsRegistered returns true when a service is registered as an instance.
    ///</summary>
    [TestMethod]
    public void IsRegistered_ServiceRegisteredAsInstance_ReturnsTrue()
    {
        // Arrange
        ComponentServiceProvider provider = new();
        IDisposable service = new StubDisposable();
        provider.Register(service);

        // Act
        bool result = provider.IsRegistered<IDisposable>();

        // Assert
        Assert.IsTrue(result);
    }

    ///<summary>
    ///Tests that IsRegistered returns true after re-registering a service from factory to instance.
    ///</summary>
    [TestMethod]
    public void IsRegistered_ServiceReregisteredFromFactoryToInstance_ReturnsTrue()
    {
        // Arrange
        ComponentServiceProvider provider = new();
        provider.Register<IDisposable>(() => new StubDisposable());
        IDisposable service = new StubDisposable();
        provider.Register(service);

        // Act
        bool result = provider.IsRegistered<IDisposable>();

        // Assert
        Assert.IsTrue(result);
    }

    ///<summary>
    ///Tests that IsRegistered returns true after re-registering a service from instance to factory.
    ///</summary>
    [TestMethod]
    public void IsRegistered_ServiceReregisteredFromInstanceToFactory_ReturnsTrue()
    {
        // Arrange
        ComponentServiceProvider provider = new();
        IDisposable service = new StubDisposable();
        provider.Register(service);
        provider.Register<IDisposable>(() => new StubDisposable());

        // Act
        bool result = provider.IsRegistered<IDisposable>();

        // Assert
        Assert.IsTrue(result);
    }

    ///<summary>
    ///Tests that IsRegistered returns false after a service registered as a factory is unregistered.
    ///</summary>
    [TestMethod]
    public void IsRegistered_ServiceUnregisteredAfterFactoryRegistration_ReturnsFalse()
    {
        // Arrange
        ComponentServiceProvider provider = new();
        provider.Register<IDisposable>(() => new StubDisposable());
        provider.Unregister<IDisposable>();

        // Act
        bool result = provider.IsRegistered<IDisposable>();

        // Assert
        Assert.IsFalse(result);
    }

    ///<summary>
    ///Tests that IsRegistered returns false after a service registered as an instance is unregistered.
    ///</summary>
    [TestMethod]
    public void IsRegistered_ServiceUnregisteredAfterInstanceRegistration_ReturnsFalse()
    {
        // Arrange
        ComponentServiceProvider provider = new();
        IDisposable service = new StubDisposable();
        provider.Register(service);
        provider.Unregister<IDisposable>();

        // Act
        bool result = provider.IsRegistered<IDisposable>();

        // Assert
        Assert.IsFalse(result);
    }

    ///<summary>
    ///Tests that Register enables fluent chaining of multiple registrations.
    ///</summary>
    [TestMethod]
    public void Register_ChainedCalls_EnablesFluentRegistration()
    {
        // Arrange
        ComponentServiceProvider provider = new ComponentServiceProvider();
        TestService service1 = new TestService();
        AnotherTestService service2 = new AnotherTestService();

        // Act
        ComponentServiceProvider result = provider
            .Register<ITestService>(service1)
            .Register<IAnotherTestService>(service2);

        // Assert
        Assert.AreSame(provider, result);
        Assert.AreSame(service1, provider.GetService(typeof(ITestService)));
        Assert.AreSame(service2, provider.GetService(typeof(IAnotherTestService)));
    }

    ///<summary>
    ///Tests that Register overwrites a previously registered instance of the same type.
    ///</summary>
    [TestMethod]
    public void Register_ExistingInstance_OverwritesPreviousRegistration()
    {
        // Arrange
        ComponentServiceProvider provider = new ComponentServiceProvider();
        TestService firstInstance = new TestService();
        TestService secondInstance = new TestService();

        // Act
        provider.Register<ITestService>(firstInstance);
        provider.Register<ITestService>(secondInstance);
        object? retrieved = provider.GetService(typeof(ITestService));

        // Assert
        Assert.AreSame(secondInstance, retrieved);
    }

    ///<summary>
    ///Tests that Register with factory removes any previously registered instance for the same service type.
    ///</summary>
    [TestMethod]
    public void Register_FactoryAfterInstance_RemovesExistingInstance()
    {
        // Arrange
        ComponentServiceProvider provider = new ComponentServiceProvider();
        TestService instance = new TestService();
        TestService factoryService = new TestService();
        Func<ITestService> factory = () => factoryService;

        // Act
        provider.Register<ITestService>(instance);
        provider.Register(factory);
        object? result = provider.GetService(typeof(ITestService));

        // Assert
        Assert.AreSame(factoryService, result);
    }

    ///<summary>
    ///Tests that Register with factory replaces a previously registered factory for the same service type.
    ///</summary>
    [TestMethod]
    public void Register_FactoryMultipleTimes_ReplacesExistingFactory()
    {
        // Arrange
        ComponentServiceProvider provider = new ComponentServiceProvider();
        TestService service1 = new TestService();
        TestService service2 = new TestService();
        Func<ITestService> factory1 = () => service1;
        Func<ITestService> factory2 = () => service2;

        // Act
        provider.Register(factory1);
        provider.Register(factory2);
        object? result = provider.GetService(typeof(ITestService));

        // Assert
        Assert.AreSame(service2, result);
    }

    ///<summary>
    ///Tests that Register with factory that returns null does not throw and GetService returns null.
    ///</summary>
    [TestMethod]
    public void Register_FactoryReturnsNull_GetServiceReturnsNull()
    {
        // Arrange
        ComponentServiceProvider provider = new ComponentServiceProvider();
        Func<ITestService?> factory = () => null;
        provider.Register(factory);

        // Act
        object? result = provider.GetService(typeof(ITestService));

        // Assert
        Assert.IsNull(result);
    }

    ///<summary>
    ///Tests that Register can handle multiple different service types independently.
    ///</summary>
    [TestMethod]
    public void Register_MultipleDifferentTypes_RegistersEachIndependently()
    {
        // Arrange
        ComponentServiceProvider provider = new ComponentServiceProvider();
        TestService service1 = new TestService();
        AnotherTestService service2 = new AnotherTestService();

        // Act
        provider.Register<ITestService>(service1);
        provider.Register<IAnotherTestService>(service2);

        // Assert
        Assert.AreSame(service1, provider.GetService(typeof(ITestService)));
        Assert.AreSame(service2, provider.GetService(typeof(IAnotherTestService)));
    }

    ///<summary>
    ///Tests that Register with factory allows service unregistration.
    ///</summary>
    [TestMethod]
    public void Register_ValidFactory_CanBeUnregistered()
    {
        // Arrange
        ComponentServiceProvider provider = new ComponentServiceProvider();
        TestService service = new TestService();
        Func<ITestService> factory = () => service;
        provider.Register(factory);

        // Act
        bool unregistered = provider.Unregister<ITestService>();
        object? result = provider.GetService(typeof(ITestService));

        // Assert
        Assert.IsTrue(unregistered);
        Assert.IsNull(result);
    }

    ///<summary>
    ///Tests that Register with factory stores the factory and GetService invokes it to retrieve the service.
    ///</summary>
    [TestMethod]
    public void Register_ValidFactory_GetServiceInvokesFactory()
    {
        // Arrange
        ComponentServiceProvider provider = new ComponentServiceProvider();
        TestService service = new TestService();
        Func<ITestService> factory = () => service;
        provider.Register(factory);

        // Act
        object? result = provider.GetService(typeof(ITestService));

        // Assert
        Assert.AreSame(service, result);
    }

    ///<summary>
    ///Tests that Register with factory invokes the factory on each GetService call (not cached).
    ///</summary>
    [TestMethod]
    public void Register_ValidFactory_GetServiceInvokesFactoryEachTime()
    {
        // Arrange
        ComponentServiceProvider provider = new ComponentServiceProvider();
        int invocationCount = 0;
        Func<ITestService> factory = () =>
        {
            invocationCount++;
            return new TestService();
        };
        provider.Register(factory);

        // Act
        object? result1 = provider.GetService(typeof(ITestService));
        object? result2 = provider.GetService(typeof(ITestService));

        // Assert
        Assert.AreEqual(2, invocationCount);
        Assert.IsNotNull(result1);
        Assert.IsNotNull(result2);
        Assert.AreNotSame(result1, result2);
    }

    ///<summary>
    ///Tests that Register with factory marks the service as registered locally.
    ///</summary>
    [TestMethod]
    public void Register_ValidFactory_IsRegisteredReturnsTrue()
    {
        // Arrange
        ComponentServiceProvider provider = new ComponentServiceProvider();
        TestService service = new TestService();
        Func<ITestService> factory = () => service;

        // Act
        provider.Register(factory);

        // Assert
        Assert.IsTrue(provider.IsRegistered<ITestService>());
    }

    ///<summary>
    ///Tests that Register with factory stores the factory and returns the provider instance for fluent chaining.
    ///</summary>
    [TestMethod]
    public void Register_ValidFactory_ReturnsProviderForFluentChaining()
    {
        // Arrange
        ComponentServiceProvider provider = new ComponentServiceProvider();
        TestService service = new TestService();
        Func<ITestService> factory = () => service;

        // Act
        ComponentServiceProvider result = provider.Register(factory);

        // Assert
        Assert.AreSame(provider, result);
    }

    ///<summary>
    ///Tests that Register with a valid instance succeeds and returns the provider for fluent chaining.
    ///</summary>
    [TestMethod]
    public void Register_ValidInstance_ReturnsProviderForChaining()
    {
        // Arrange
        ComponentServiceProvider provider = new ComponentServiceProvider();
        TestService instance = new TestService();

        // Act
        ComponentServiceProvider result = provider.Register<ITestService>(instance);

        // Assert
        Assert.AreSame(provider, result);
    }

    ///<summary>
    ///Tests that Register stores the instance and makes it retrievable via GetService.
    ///</summary>
    [TestMethod]
    public void Register_ValidInstance_StoresInstanceForRetrieval()
    {
        // Arrange
        ComponentServiceProvider provider = new ComponentServiceProvider();
        TestService instance = new TestService();

        // Act
        provider.Register<ITestService>(instance);
        object? retrieved = provider.GetService(typeof(ITestService));

        // Assert
        Assert.AreSame(instance, retrieved);
    }

    ///<summary>
    ///Tests that Unregister returns false when called twice on same service.
    ///</summary>
    [TestMethod]
    public void Unregister_CalledTwice_SecondCallReturnsFalse()
    {
        // Arrange
        ComponentServiceProvider provider = new ComponentServiceProvider();
        TestService service = new TestService();
        provider.Register<ITestService>(service);
        provider.Unregister<ITestService>();

        // Act
        bool result = provider.Unregister<ITestService>();

        // Assert
        Assert.IsFalse(result);
    }

    ///<summary>
    ///Tests that Unregister only removes specified service type and does not affect other registered services.
    ///</summary>
    [TestMethod]
    public void Unregister_MultipleServicesRegistered_OnlyRemovesSpecifiedService()
    {
        // Arrange
        ComponentServiceProvider provider = new ComponentServiceProvider();
        TestService service1 = new TestService();
        AnotherTestService service2 = new AnotherTestService();
        provider.Register<ITestService>(service1);
        provider.Register<IAnotherTestService>(service2);

        // Act
        bool result = provider.Unregister<ITestService>();

        // Assert
        Assert.IsTrue(result);
        Assert.IsFalse(provider.IsRegistered<ITestService>());
        Assert.IsTrue(provider.IsRegistered<IAnotherTestService>());
    }

    ///<summary>
    ///Tests that Unregister returns false when service is not registered.
    ///</summary>
    [TestMethod]
    public void Unregister_ServiceNotRegistered_ReturnsFalse()
    {
        // Arrange
        ComponentServiceProvider provider = new ComponentServiceProvider();

        // Act
        bool result = provider.Unregister<ITestService>();

        // Assert
        Assert.IsFalse(result);
    }

    ///<summary>
    ///Tests that Unregister returns true and removes service when service is registered as factory.
    ///</summary>
    [TestMethod]
    public void Unregister_ServiceRegisteredAsFactory_ReturnsTrueAndRemovesService()
    {
        // Arrange
        ComponentServiceProvider provider = new ComponentServiceProvider();
        provider.Register<ITestService>(() => new TestService());

        // Act
        bool result = provider.Unregister<ITestService>();

        // Assert
        Assert.IsTrue(result);
        Assert.IsFalse(provider.IsRegistered<ITestService>());
    }

    ///<summary>
    ///Tests that Unregister returns true and removes service when service is registered as instance.
    ///</summary>
    [TestMethod]
    public void Unregister_ServiceRegisteredAsInstance_ReturnsTrueAndRemovesService()
    {
        // Arrange
        ComponentServiceProvider provider = new ComponentServiceProvider();
        TestService service = new TestService();
        provider.Register<ITestService>(service);

        // Act
        bool result = provider.Unregister<ITestService>();

        // Assert
        Assert.IsTrue(result);
        Assert.IsFalse(provider.IsRegistered<ITestService>());
    }

    ///<summary>
    ///Tests that Unregister returns true when service was previously registered as instance then re-registered as
    ///factory.
    ///</summary>
    [TestMethod]
    public void Unregister_ServiceReregisteredAsFactory_ReturnsTrueAndRemovesService()
    {
        // Arrange
        ComponentServiceProvider provider = new ComponentServiceProvider();
        TestService service = new TestService();
        provider.Register<ITestService>(service);
        provider.Register<ITestService>(() => new TestService());

        // Act
        bool result = provider.Unregister<ITestService>();

        // Assert
        Assert.IsTrue(result);
        Assert.IsFalse(provider.IsRegistered<ITestService>());
    }

    ///<summary>
    ///Tests that Unregister returns true when service was previously registered as factory then re-registered as
    ///instance.
    ///</summary>
    [TestMethod]
    public void Unregister_ServiceReregisteredAsInstance_ReturnsTrueAndRemovesService()
    {
        // Arrange
        ComponentServiceProvider provider = new ComponentServiceProvider();
        provider.Register<ITestService>(() => new TestService());
        TestService service = new TestService();
        provider.Register<ITestService>(service);

        // Act
        bool result = provider.Unregister<ITestService>();

        // Assert
        Assert.IsTrue(result);
        Assert.IsFalse(provider.IsRegistered<ITestService>());
    }
    #endregion

    interface ITestService
    {
    }

    sealed class TestService : ITestService
    {
    }

    interface IAnotherTestService
    {
    }

    sealed class AnotherTestService : IAnotherTestService
    {
    }

    sealed class StubServiceProvider : IServiceProvider
    {
        #region Public methods
        public object? GetService(Type serviceType)
        {
            GetServiceCallCount++;
            return Services.TryGetValue(serviceType, out object? service) ? service : null;
        }
        #endregion

        #region Public properties
        public int GetServiceCallCount { get; private set; }

        public Dictionary<Type, object?> Services { get; } = [];
        #endregion
    }

    sealed class StubDisposable : IDisposable
    {
        #region Public methods
        public void Dispose()
        {
        }
        #endregion
    }
}