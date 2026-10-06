using System.ComponentModel;
using Catharsis.ComponentModel;

namespace Catharsis.UnitTests.ComponentModel;

///<summary>
///Unit tests for the <see cref="ComponentBase"/> class.
///</summary>
[TestClass]
public class ComponentBaseTests
{
    #region Public methods

    ///<summary>
    ///Tests that Container returns the Site's Container when both Site and Site.Container are not null.
    ///</summary>
    [TestMethod]
    public void Container_WhenSiteAndContainerAreNotNull_ReturnsContainer()
    {
        // Arrange
        StubContainer stubContainer = new();
        StubSite stubSite = new() { ContainerValue = stubContainer };
        TestableComponentBase component = new() { Site = stubSite };

        // Act
        IContainer? container = component.ExposedContainer;

        // Assert
        Assert.IsNotNull(container);
        Assert.AreSame(stubContainer, container);
    }

    ///<summary>
    ///Tests that Container returns null when Site is changed from non-null to null.
    ///</summary>
    [TestMethod]
    public void Container_WhenSiteChangedFromNonNullToNull_ReturnsNull()
    {
        // Arrange
        StubContainer stubContainer = new();
        StubSite stubSite = new() { ContainerValue = stubContainer };
        TestableComponentBase component = new() { Site = stubSite };

        // Act
        component.Site = null;
        IContainer? container = component.ExposedContainer;

        // Assert
        Assert.IsNull(container);
    }

    ///<summary>
    ///Tests that Container updates when Site is changed from null to non-null.
    ///</summary>
    [TestMethod]
    public void Container_WhenSiteChangedFromNullToNonNull_ReturnsNewContainer()
    {
        // Arrange
        TestableComponentBase component = new();
        StubContainer stubContainer = new();
        StubSite stubSite = new() { ContainerValue = stubContainer };

        // Act
        component.Site = stubSite;
        IContainer? container = component.ExposedContainer;

        // Assert
        Assert.IsNotNull(container);
        Assert.AreSame(stubContainer, container);
    }

    ///<summary>
    ///Tests that Container returns null when Site is not null but Site.Container is null.
    ///</summary>
    [TestMethod]
    public void Container_WhenSiteContainerIsNull_ReturnsNull()
    {
        // Arrange
        StubSite stubSite = new() { ContainerValue = null };
        TestableComponentBase component = new() { Site = stubSite };

        // Act
        IContainer? container = component.ExposedContainer;

        // Assert
        Assert.IsNull(container);
    }

    ///<summary>
    ///Tests that Container returns null when Site is null.
    ///</summary>
    [TestMethod]
    public void Container_WhenSiteIsNull_ReturnsNull()
    {
        // Arrange
        TestableComponentBase component = new();

        // Act
        IContainer? container = component.ExposedContainer;

        // Assert
        Assert.IsNull(container);
    }

    ///<summary>
    ///Tests that Dispose raises the Disposed event with correct arguments.
    ///</summary>
    [TestMethod]
    public void Dispose_FirstCall_RaisesDisposedEvent()
    {
        // Arrange
        TestableComponentBase component = new();
        object? eventSender = null;
        EventArgs? eventArgs = null;
        component.Disposed += (sender, args) =>
        {
            eventSender = sender;
            eventArgs = args;
        };

        // Act
        component.Dispose();

        // Assert
        Assert.IsNotNull(eventSender);
        Assert.AreSame(component, eventSender);
        Assert.IsNotNull(eventArgs);
        Assert.AreSame(EventArgs.Empty, eventArgs);
    }

    ///<summary>
    ///Tests that Dispose raises the Disposed event only once when called multiple times.
    ///</summary>
    [TestMethod]
    public void Dispose_MultipleCalls_RaisesEventOnlyOnce()
    {
        // Arrange
        TestableComponentBase component = new();
        int eventRaisedCount = 0;
        component.Disposed += (sender, args) => eventRaisedCount++;

        // Act
        component.Dispose();
        component.Dispose();
        component.Dispose();

        // Assert
        Assert.AreEqual(1, eventRaisedCount);
    }

    ///<summary>
    ///Tests that Dispose clears the Site property.
    ///</summary>
    [TestMethod]
    public void Dispose_WithSite_ClearsSite()
    {
        // Arrange
        TestableComponentBase component = new();
        StubContainer stubContainer = new();
        StubSite stubSite = new() { ContainerValue = stubContainer };
        component.Site = stubSite;

        // Act
        component.Dispose();

        // Assert
        Assert.IsNull(component.Site);
    }

    ///<summary>
    ///Tests that Dispose removes the component from its container when sited.
    ///</summary>
    [TestMethod]
    public void Dispose_WithSite_RemovesFromContainer()
    {
        // Arrange
        TestableComponentBase component = new();
        StubContainer stubContainer = new();
        StubSite stubSite = new() { ContainerValue = stubContainer };
        component.Site = stubSite;

        // Act
        component.Dispose();

        // Assert
        Assert.AreEqual(1, stubContainer.RemoveCallCount);
        Assert.AreSame(component, stubContainer.LastRemovedComponent);
    }

    ///<summary>
    ///Tests that adding a handler to the Disposed event works correctly and the handler is invoked during disposal.
    ///</summary>
    [TestMethod]
    public void Disposed_AddHandler_HandlerInvokedOnDispose()
    {
        // Arrange
        TestComponent component = new();
        bool eventRaised = false;
        object? capturedSender = null;
        EventArgs? capturedArgs = null;

        EventHandler handler = (sender, e) =>
        {
            eventRaised = true;
            capturedSender = sender;
            capturedArgs = e;
        };

        // Act
        component.Disposed += handler;
        component.Dispose();

        // Assert
        Assert.IsTrue(eventRaised, "Disposed event should be raised");
        Assert.AreSame(component, capturedSender, "Sender should be the component instance");
        Assert.AreSame(EventArgs.Empty, capturedArgs, "EventArgs should be EventArgs.Empty");
    }

    ///<summary>
    ///Tests that adding and removing the same handler multiple times works correctly.
    ///</summary>
    [TestMethod]
    public void Disposed_AddRemoveSameHandlerMultipleTimes_BehavesCorrectly()
    {
        // Arrange
        TestComponent component = new();
        int invocationCount = 0;

        EventHandler handler = (sender, e) => invocationCount++;

        // Act
        component.Disposed += handler;
        component.Disposed += handler;
        component.Disposed -= handler;
        component.Dispose();

        // Assert
        Assert.AreEqual(1, invocationCount, "Handler should be invoked once (second add creates duplicate, first remove removes one)");
    }

    ///<summary>
    ///Tests that the Disposed event is only raised once even when Dispose is called multiple times.
    ///</summary>
    [TestMethod]
    public void Disposed_MultipleDisposes_EventRaisedOnlyOnce()
    {
        // Arrange
        TestComponent component = new();
        int invocationCount = 0;

        EventHandler handler = (sender, e) => invocationCount++;

        // Act
        component.Disposed += handler;
        component.Dispose();
        component.Dispose();
        component.Dispose();

        // Assert
        Assert.AreEqual(1, invocationCount, "Disposed event should only be raised once");
    }

    ///<summary>
    ///Tests that multiple handlers can be added to the Disposed event and all are invoked during disposal.
    ///</summary>
    [TestMethod]
    public void Disposed_MultipleHandlers_AllHandlersInvoked()
    {
        // Arrange
        TestComponent component = new();
        bool handler1Invoked = false;
        bool handler2Invoked = false;
        bool handler3Invoked = false;

        EventHandler handler1 = (sender, e) => handler1Invoked = true;
        EventHandler handler2 = (sender, e) => handler2Invoked = true;
        EventHandler handler3 = (sender, e) => handler3Invoked = true;

        // Act
        component.Disposed += handler1;
        component.Disposed += handler2;
        component.Disposed += handler3;
        component.Dispose();

        // Assert
        Assert.IsTrue(handler1Invoked, "First handler should be invoked");
        Assert.IsTrue(handler2Invoked, "Second handler should be invoked");
        Assert.IsTrue(handler3Invoked, "Third handler should be invoked");
    }

    ///<summary>
    ///Tests that when no handlers are subscribed, disposing does not throw an exception.
    ///</summary>
    [TestMethod]
    public void Disposed_NoHandlers_DisposalDoesNotThrow()
    {
        // Arrange
        TestComponent component = new();

        // Act & Assert
        component.Dispose(); // Should not throw
    }

    ///<summary>
    ///Tests that removing a handler from the Disposed event works correctly and the handler is not invoked after
    ///removal.
    ///</summary>
    [TestMethod]
    public void Disposed_RemoveHandler_HandlerNotInvokedOnDispose()
    {
        // Arrange
        TestComponent component = new();
        bool eventRaised = false;

        EventHandler handler = (sender, e) => eventRaised = true;
        component.Disposed += handler;

        // Act
        component.Disposed -= handler;
        component.Dispose();

        // Assert
        Assert.IsFalse(eventRaised, "Disposed event should not be raised after handler removal");
    }

    ///<summary>
    ///Tests that removing a handler that was never added does not cause any issues.
    ///</summary>
    [TestMethod]
    public void Disposed_RemoveNonExistentHandler_DoesNotThrow()
    {
        // Arrange
        TestComponent component = new();
        EventHandler handler = static (sender, e) =>
        {
        };

        // Act & Assert
        component.Disposed -= handler; // Should not throw
        component.Dispose(); // Should not throw
    }

    ///<summary>
    ///Tests that when multiple handlers are subscribed and one is removed, the remaining handlers are still invoked.
    ///</summary>
    [TestMethod]
    public void Disposed_RemoveOneOfMultipleHandlers_RemainingHandlersInvoked()
    {
        // Arrange
        TestComponent component = new();
        bool handler1Invoked = false;
        bool handler2Invoked = false;
        bool handler3Invoked = false;

        EventHandler handler1 = (sender, e) => handler1Invoked = true;
        EventHandler handler2 = (sender, e) => handler2Invoked = true;
        EventHandler handler3 = (sender, e) => handler3Invoked = true;

        component.Disposed += handler1;
        component.Disposed += handler2;
        component.Disposed += handler3;

        // Act
        component.Disposed -= handler2;
        component.Dispose();

        // Assert
        Assert.IsTrue(handler1Invoked, "First handler should be invoked");
        Assert.IsFalse(handler2Invoked, "Second handler should not be invoked after removal");
        Assert.IsTrue(handler3Invoked, "Third handler should be invoked");
    }

    ///<summary>
    ///Tests that GetService returns the component itself when requesting IComponent type.
    ///</summary>
    [TestMethod]
    public void GetService_RequestIComponent_ReturnsThis()
    {
        // Arrange
        TestComponent component = new();

        // Act
        object? result = component.GetService(typeof(IComponent));

        // Assert
        Assert.AreSame(component, result);
    }

    ///<summary>
    ///Tests that GetService returns the container when requesting IContainer and Site.Container is set.
    ///</summary>
    [TestMethod]
    public void GetService_RequestIContainerWithContainerSet_ReturnsContainer()
    {
        // Arrange
        StubContainer stubContainer = new();
        StubSite stubSite = new() { ContainerValue = stubContainer };
        TestComponent component = new() { Site = stubSite };

        // Act
        object? result = component.GetService(typeof(IContainer));

        // Assert
        Assert.AreSame(stubContainer, result);
    }

    ///<summary>
    ///Tests that GetService returns null when requesting IContainer and Site.Container is null.
    ///</summary>
    [TestMethod]
    public void GetService_RequestIContainerWithNullContainer_ReturnsNull()
    {
        // Arrange
        StubSite stubSite = new() { ContainerValue = null };
        TestComponent component = new() { Site = stubSite };

        // Act
        object? result = component.GetService(typeof(IContainer));

        // Assert
        Assert.IsNull(result);
    }

    ///<summary>
    ///Tests that GetService returns null when requesting IContainer and Site is not set.
    ///</summary>
    [TestMethod]
    public void GetService_RequestIContainerWithNullSite_ReturnsNull()
    {
        // Arrange
        TestComponent component = new() { Site = null };

        // Act
        object? result = component.GetService(typeof(IContainer));

        // Assert
        Assert.IsNull(result);
    }

    ///<summary>
    ///Tests that GetService returns null when requesting ISite and Site is not set.
    ///</summary>
    [TestMethod]
    public void GetService_RequestISiteWithNullSite_ReturnsNull()
    {
        // Arrange
        TestComponent component = new() { Site = null };

        // Act
        object? result = component.GetService(typeof(ISite));

        // Assert
        Assert.IsNull(result);
    }

    ///<summary>
    ///Tests that GetService returns the site when requesting ISite and Site is set.
    ///</summary>
    [TestMethod]
    public void GetService_RequestISiteWithSiteSet_ReturnsSite()
    {
        // Arrange
        StubSite stubSite = new();
        TestComponent component = new() { Site = stubSite };

        // Act
        object? result = component.GetService(typeof(ISite));

        // Assert
        Assert.AreSame(stubSite, result);
    }

    ///<summary>
    ///Tests that GetService returns null when requesting other service and Site is not set.
    ///</summary>
    [TestMethod]
    public void GetService_RequestOtherServiceWithNullSite_ReturnsNull()
    {
        // Arrange
        TestComponent component = new() { Site = null };

        // Act
        object? result = component.GetService(typeof(IServiceProvider));

        // Assert
        Assert.IsNull(result);
    }

    ///<summary>
    ///Tests that GetService returns null when requesting other service and Site.GetService returns null.
    ///</summary>
    [TestMethod]
    public void GetService_RequestOtherServiceWithSiteReturningNull_ReturnsNull()
    {
        // Arrange
        StubSite stubSite = new();
        TestComponent component = new() { Site = stubSite };

        // Act
        object? result = component.GetService(typeof(IServiceProvider));

        // Assert
        Assert.IsNull(result);
        Assert.AreEqual(1, stubSite.GetServiceCallCount);
    }

    ///<summary>
    ///Tests that GetService delegates to Site.GetService when requesting other service and Site is set.
    ///</summary>
    [TestMethod]
    public void GetService_RequestOtherServiceWithSiteSet_DelegatesToSiteGetService()
    {
        // Arrange
        object expectedService = new();
        StubSite stubSite = new();
        stubSite.Services[typeof(IServiceProvider)] = expectedService;
        TestComponent component = new() { Site = stubSite };

        // Act
        object? result = component.GetService(typeof(IServiceProvider));

        // Assert
        Assert.AreSame(expectedService, result);
        Assert.AreEqual(1, stubSite.GetServiceCallCount);
    }

    [TestMethod]
    public void Site_InitialValue_ReturnsNull()
    {
        // Arrange
        TestableComponentBase component = new();

        // Act
        ISite? result = component.Site;

        // Assert
        Assert.IsNull(result);
    }

    ///<summary>
    ///Verifies that setting the Site property multiple times returns the last set value. Tests that the setter properly
    ///overwrites the previous value. Expected: Returns the second ISite instance that was set.
    ///</summary>
    [TestMethod]
    public void Site_SetMultipleTimes_ReturnsLastValue()
    {
        // Arrange
        TestableComponentBase component = new();
        StubSite stubSite1 = new();
        StubSite stubSite2 = new();

        // Act
        component.Site = stubSite1;
        component.Site = stubSite2;
        ISite result = component.Site;

        // Assert
        Assert.AreSame(stubSite2, result);
    }

    ///<summary>
    ///Verifies that the Site property can be set to null and returns null. Tests setting null after having a previous
    ///non-null value. Expected: Returns null.
    ///</summary>
    [TestMethod]
    public void Site_SetNull_ReturnsNull()
    {
        // Arrange
        TestableComponentBase component = new();
        StubSite stubSite = new();
        component.Site = stubSite;

        // Act
        component.Site = null;
        ISite? result = component.Site;

        // Assert
        Assert.IsNull(result);
    }

    ///<summary>
    ///Verifies that the Site property returns the value that was set. Tests both setter and getter with a valid ISite
    ///instance. Expected: Returns the same ISite instance that was set.
    ///</summary>
    [TestMethod]
    public void Site_SetValue_ReturnsSetValue()
    {
        // Arrange
        TestableComponentBase component = new();
        StubSite stubSite = new();

        // Act
        component.Site = stubSite;
        ISite result = component.Site;

        // Assert
        Assert.AreSame(stubSite, result);
    }

    ///<summary>
    ///Tests that ThrowIfDisposed can be called multiple times when not disposed without throwing.
    ///</summary>
    [TestMethod]
    public void ThrowIfDisposed_CalledMultipleTimesWhenNotDisposed_DoesNotThrow()
    {
        // Arrange
        using TestableComponent component = new();

        // Act & Assert
        component.ExposeThrowIfDisposed();
        component.ExposeThrowIfDisposed();
        component.ExposeThrowIfDisposed();
    }

    ///<summary>
    ///Tests that ThrowIfDisposed does not throw when the component has not been disposed.
    ///</summary>
    [TestMethod]
    public void ThrowIfDisposed_WhenNotDisposed_DoesNotThrow()
    {
        // Arrange
        using TestableComponent component = new();

        // Act & Assert
        component.ExposeThrowIfDisposed();
    }

    ///<summary>
    ///Tests that ToString returns only the type name when Site is null.
    ///</summary>
    [TestMethod]
    public void ToString_SiteIsNull_ReturnsTypeName()
    {
        // Arrange
        TestComponent component = new();
        string expectedTypeName = "TestComponent";

        // Act
        string result = component.ToString();

        // Assert
        Assert.AreEqual(expectedTypeName, result);
    }

    ///<summary>
    ///Tests that ToString returns formatted string with empty brackets when Site.Name is empty string.
    ///</summary>
    [TestMethod]
    public void ToString_SiteNameIsEmpty_ReturnsFormattedStringWithEmptyBrackets()
    {
        // Arrange
        TestComponent component = new();
        StubSite stubSite = new() { NameValue = string.Empty };
        component.Site = stubSite;
        string expected = "TestComponent []";

        // Act
        string result = component.ToString();

        // Assert
        Assert.AreEqual(expected, result);
    }

    ///<summary>
    ///Tests that ToString returns formatted string with type name and site name when both are available.
    ///</summary>
    [TestMethod]
    [DataRow("MyComponent")]
    [DataRow("Component1")]
    [DataRow("Test")]
    public void ToString_SiteNameIsNotNull_ReturnsFormattedString(string siteName)
    {
        // Arrange
        TestComponent component = new();
        StubSite stubSite = new() { NameValue = siteName };
        component.Site = stubSite;
        string expected = $"TestComponent [{siteName}]";

        // Act
        string result = component.ToString();

        // Assert
        Assert.AreEqual(expected, result);
    }

    ///<summary>
    ///Tests that ToString returns only the type name when Site is not null but Site.Name is null.
    ///</summary>
    [TestMethod]
    public void ToString_SiteNameIsNull_ReturnsTypeName()
    {
        // Arrange
        TestComponent component = new();
        StubSite stubSite = new() { NameValue = null };
        component.Site = stubSite;
        string expectedTypeName = "TestComponent";

        // Act
        string result = component.ToString();

        // Assert
        Assert.AreEqual(expectedTypeName, result);
    }

    ///<summary>
    ///Tests that ToString returns formatted string with whitespace in brackets when Site.Name is whitespace.
    ///</summary>
    [TestMethod]
    public void ToString_SiteNameIsWhitespace_ReturnsFormattedStringWithWhitespaceBrackets()
    {
        // Arrange
        TestComponent component = new();
        string whitespace = "   ";
        StubSite stubSite = new() { NameValue = whitespace };
        component.Site = stubSite;
        string expected = $"TestComponent [{whitespace}]";

        // Act
        string result = component.ToString();

        // Assert
        Assert.AreEqual(expected, result);
    }
    #endregion

    ///<summary>
    ///Minimal concrete implementation of ComponentBase for testing purposes.
    ///</summary>
    class TestComponent : ComponentBase
    {
        // No additional implementation needed for event testing
    }

    ///<summary>
    ///Helper class that exposes protected members of ComponentBase for testing.
    ///</summary>
    class TestableComponentBase : ComponentBase
    {
        #region Public properties

        ///<summary>
        ///Exposes the protected Container property for testing.
        ///</summary>
        public IContainer? ExposedContainer => Container;
        #endregion
    }

    ///<summary>
    ///Testable concrete implementation of ComponentBase for testing protected members.
    ///</summary>
    sealed class TestableComponent : ComponentBase
    {
        #region Public methods

        ///<summary>
        ///Exposes the protected ThrowIfDisposed method for testing.
        ///</summary>
        public void ExposeThrowIfDisposed() { ThrowIfDisposed(); }
        #endregion
    }

    sealed class StubSite : ISite
    {
        #region Public methods
        public object? GetService(Type serviceType)
        {
            GetServiceCallCount++;
            return Services.TryGetValue(serviceType, out object? service) ? service : null;
        }
        #endregion

        #region Public properties
        public IComponent Component { get; set; } = null!;

        public IContainer Container => ContainerValue!;

        public IContainer? ContainerValue { get; set; }

        public bool DesignMode { get; set; }

        public int GetServiceCallCount { get; private set; }

        public string? Name { get => NameValue; set => NameValue = value; }

        public string? NameValue { get; set; }

        public Dictionary<Type, object?> Services { get; } = [];
        #endregion
    }

    sealed class StubContainer : IContainer
    {
        #region Fields
        readonly List<IComponent> _components = [];
        #endregion

        #region Public methods
        public void Add(IComponent? component) { Add(component, null); }

        public void Add(IComponent? component, string? name)
        {
            if (component is not null)
            {
                _components.Add(component);
            }
        }

        public void Dispose()
        {
        }

        public void Remove(IComponent? component)
        {
            RemoveCallCount++;
            LastRemovedComponent = component;
            if (component is not null)
            {
                _components.Remove(component);
            }
        }
        #endregion

        #region Public properties
        public ComponentCollection Components => new([.. _components]);

        public IComponent? LastRemovedComponent { get; private set; }

        public int RemoveCallCount { get; private set; }
        #endregion
    }
}
