using System.ComponentModel;

namespace Catharsis.ComponentModel.UnitTests;


/// <summary>
/// Unit tests for the <see cref="ComponentBase"/> class.
/// </summary>
[TestClass]
public class ComponentBaseTests
{
    /// <summary>
    /// Tests that adding a handler to the Disposed event works correctly and the handler is invoked during disposal.
    /// </summary>
    [TestMethod]
    public void Disposed_AddHandler_HandlerInvokedOnDispose()
    {
        // Arrange
        var component = new TestComponent();
        var eventRaised = false;
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

    /// <summary>
    /// Tests that removing a handler from the Disposed event works correctly and the handler is not invoked after removal.
    /// </summary>
    [TestMethod]
    public void Disposed_RemoveHandler_HandlerNotInvokedOnDispose()
    {
        // Arrange
        var component = new TestComponent();
        var eventRaised = false;

        EventHandler handler = (sender, e) => eventRaised = true;
        component.Disposed += handler;

        // Act
        component.Disposed -= handler;
        component.Dispose();

        // Assert
        Assert.IsFalse(eventRaised, "Disposed event should not be raised after handler removal");
    }

    /// <summary>
    /// Tests that multiple handlers can be added to the Disposed event and all are invoked during disposal.
    /// </summary>
    [TestMethod]
    public void Disposed_MultipleHandlers_AllHandlersInvoked()
    {
        // Arrange
        var component = new TestComponent();
        var handler1Invoked = false;
        var handler2Invoked = false;
        var handler3Invoked = false;

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

    /// <summary>
    /// Tests that the Disposed event is only raised once even when Dispose is called multiple times.
    /// </summary>
    [TestMethod]
    public void Disposed_MultipleDisposes_EventRaisedOnlyOnce()
    {
        // Arrange
        var component = new TestComponent();
        var invocationCount = 0;

        EventHandler handler = (sender, e) => invocationCount++;

        // Act
        component.Disposed += handler;
        component.Dispose();
        component.Dispose();
        component.Dispose();

        // Assert
        Assert.AreEqual(1, invocationCount, "Disposed event should only be raised once");
    }

    /// <summary>
    /// Tests that when no handlers are subscribed, disposing does not throw an exception.
    /// </summary>
    [TestMethod]
    public void Disposed_NoHandlers_DisposalDoesNotThrow()
    {
        // Arrange
        var component = new TestComponent();

        // Act & Assert
        component.Dispose(); // Should not throw
    }

    /// <summary>
    /// Tests that adding and removing the same handler multiple times works correctly.
    /// </summary>
    [TestMethod]
    public void Disposed_AddRemoveSameHandlerMultipleTimes_BehavesCorrectly()
    {
        // Arrange
        var component = new TestComponent();
        var invocationCount = 0;

        EventHandler handler = (sender, e) => invocationCount++;

        // Act
        component.Disposed += handler;
        component.Disposed += handler;
        component.Disposed -= handler;
        component.Dispose();

        // Assert
        Assert.AreEqual(1, invocationCount, "Handler should be invoked once (second add creates duplicate, first remove removes one)");
    }

    /// <summary>
    /// Tests that when multiple handlers are subscribed and one is removed, the remaining handlers are still invoked.
    /// </summary>
    [TestMethod]
    public void Disposed_RemoveOneOfMultipleHandlers_RemainingHandlersInvoked()
    {
        // Arrange
        var component = new TestComponent();
        var handler1Invoked = false;
        var handler2Invoked = false;
        var handler3Invoked = false;

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

    /// <summary>
    /// Tests that removing a handler that was never added does not cause any issues.
    /// </summary>
    [TestMethod]
    public void Disposed_RemoveNonExistentHandler_DoesNotThrow()
    {
        // Arrange
        var component = new TestComponent();
        EventHandler handler = (sender, e) => { };

        // Act & Assert
        component.Disposed -= handler; // Should not throw
        component.Dispose(); // Should not throw
    }

    /// <summary>
    /// Minimal concrete implementation of ComponentBase for testing purposes.
    /// </summary>
    private class TestComponent : ComponentBase
    {
        // No additional implementation needed for event testing
    }

    /// <summary>
    /// Tests that Container returns null when Site is null.
    /// </summary>
    [TestMethod]
    public void Container_WhenSiteIsNull_ReturnsNull()
    {
        // Arrange
        var component = new TestableComponentBase();

        // Act
        var container = component.ExposedContainer;

        // Assert
        Assert.IsNull(container);
    }

    /// <summary>
    /// Tests that Container returns null when Site is not null but Site.Container is null.
    /// </summary>
    [TestMethod]
    public void Container_WhenSiteContainerIsNull_ReturnsNull()
    {
        // Arrange
        var stubSite = new StubSite { ContainerValue = null };
        var component = new TestableComponentBase
        {
            Site = stubSite
        };

        // Act
        var container = component.ExposedContainer;

        // Assert
        Assert.IsNull(container);
    }

    /// <summary>
    /// Tests that Container returns the Site's Container when both Site and Site.Container are not null.
    /// </summary>
    [TestMethod]
    public void Container_WhenSiteAndContainerAreNotNull_ReturnsContainer()
    {
        // Arrange
        var stubContainer = new StubContainer();
        var stubSite = new StubSite { ContainerValue = stubContainer };
        var component = new TestableComponentBase
        {
            Site = stubSite
        };

        // Act
        var container = component.ExposedContainer;

        // Assert
        Assert.IsNotNull(container);
        Assert.AreSame(stubContainer, container);
    }

    /// <summary>
    /// Tests that Container updates when Site is changed from null to non-null.
    /// </summary>
    [TestMethod]
    public void Container_WhenSiteChangedFromNullToNonNull_ReturnsNewContainer()
    {
        // Arrange
        var component = new TestableComponentBase();
        var stubContainer = new StubContainer();
        var stubSite = new StubSite { ContainerValue = stubContainer };

        // Act
        component.Site = stubSite;
        var container = component.ExposedContainer;

        // Assert
        Assert.IsNotNull(container);
        Assert.AreSame(stubContainer, container);
    }

    /// <summary>
    /// Tests that Container returns null when Site is changed from non-null to null.
    /// </summary>
    [TestMethod]
    public void Container_WhenSiteChangedFromNonNullToNull_ReturnsNull()
    {
        // Arrange
        var stubContainer = new StubContainer();
        var stubSite = new StubSite { ContainerValue = stubContainer };
        var component = new TestableComponentBase
        {
            Site = stubSite
        };

        // Act
        component.Site = null;
        var container = component.ExposedContainer;

        // Assert
        Assert.IsNull(container);
    }

    /// <summary>
    /// Helper class that exposes protected members of ComponentBase for testing.
    /// </summary>
    private class TestableComponentBase : ComponentBase
    {
        /// <summary>
        /// Exposes the protected Container property for testing.
        /// </summary>
        public IContainer? ExposedContainer => Container;
    }

    /// <summary>
    /// Tests that GetService returns the component itself when requesting IComponent type.
    /// </summary>
    [TestMethod]
    public void GetService_RequestIComponent_ReturnsThis()
    {
        // Arrange
        var component = new TestComponent();

        // Act
        var result = component.GetService(typeof(IComponent));

        // Assert
        Assert.AreSame(component, result);
    }

    /// <summary>
    /// Tests that GetService returns null when requesting ISite and Site is not set.
    /// </summary>
    [TestMethod]
    public void GetService_RequestISiteWithNullSite_ReturnsNull()
    {
        // Arrange
        var component = new TestComponent
        {
            Site = null
        };

        // Act
        var result = component.GetService(typeof(ISite));

        // Assert
        Assert.IsNull(result);
    }

    /// <summary>
    /// Tests that GetService returns the site when requesting ISite and Site is set.
    /// </summary>
    [TestMethod]
    public void GetService_RequestISiteWithSiteSet_ReturnsSite()
    {
        // Arrange
        var stubSite = new StubSite();
        var component = new TestComponent
        {
            Site = stubSite
        };

        // Act
        var result = component.GetService(typeof(ISite));

        // Assert
        Assert.AreSame(stubSite, result);
    }

    /// <summary>
    /// Tests that GetService returns null when requesting IContainer and Site is not set.
    /// </summary>
    [TestMethod]
    public void GetService_RequestIContainerWithNullSite_ReturnsNull()
    {
        // Arrange
        var component = new TestComponent
        {
            Site = null
        };

        // Act
        var result = component.GetService(typeof(IContainer));

        // Assert
        Assert.IsNull(result);
    }

    /// <summary>
    /// Tests that GetService returns null when requesting IContainer and Site.Container is null.
    /// </summary>
    [TestMethod]
    public void GetService_RequestIContainerWithNullContainer_ReturnsNull()
    {
        // Arrange
        var stubSite = new StubSite { ContainerValue = null };
        var component = new TestComponent
        {
            Site = stubSite
        };

        // Act
        var result = component.GetService(typeof(IContainer));

        // Assert
        Assert.IsNull(result);
    }

    /// <summary>
    /// Tests that GetService returns the container when requesting IContainer and Site.Container is set.
    /// </summary>
    [TestMethod]
    public void GetService_RequestIContainerWithContainerSet_ReturnsContainer()
    {
        // Arrange
        var stubContainer = new StubContainer();
        var stubSite = new StubSite { ContainerValue = stubContainer };
        var component = new TestComponent
        {
            Site = stubSite
        };

        // Act
        var result = component.GetService(typeof(IContainer));

        // Assert
        Assert.AreSame(stubContainer, result);
    }

    /// <summary>
    /// Tests that GetService returns null when requesting other service and Site is not set.
    /// </summary>
    [TestMethod]
    public void GetService_RequestOtherServiceWithNullSite_ReturnsNull()
    {
        // Arrange
        var component = new TestComponent
        {
            Site = null
        };

        // Act
        var result = component.GetService(typeof(IServiceProvider));

        // Assert
        Assert.IsNull(result);
    }

    /// <summary>
    /// Tests that GetService delegates to Site.GetService when requesting other service and Site is set.
    /// </summary>
    [TestMethod]
    public void GetService_RequestOtherServiceWithSiteSet_DelegatesToSiteGetService()
    {
        // Arrange
        var expectedService = new object();
        var stubSite = new StubSite();
        stubSite.Services[typeof(IServiceProvider)] = expectedService;
        var component = new TestComponent
        {
            Site = stubSite
        };

        // Act
        var result = component.GetService(typeof(IServiceProvider));

        // Assert
        Assert.AreSame(expectedService, result);
        Assert.AreEqual(1, stubSite.GetServiceCallCount);
    }

    /// <summary>
    /// Tests that GetService returns null when requesting other service and Site.GetService returns null.
    /// </summary>
    [TestMethod]
    public void GetService_RequestOtherServiceWithSiteReturningNull_ReturnsNull()
    {
        // Arrange
        var stubSite = new StubSite();
        var component = new TestComponent
        {
            Site = stubSite
        };

        // Act
        var result = component.GetService(typeof(IServiceProvider));

        // Assert
        Assert.IsNull(result);
        Assert.AreEqual(1, stubSite.GetServiceCallCount);
    }

    /// <summary>
    /// Tests that Dispose raises the Disposed event with correct arguments.
    /// </summary>
    [TestMethod]
    public void Dispose_FirstCall_RaisesDisposedEvent()
    {
        // Arrange
        var component = new TestableComponentBase();
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

    /// <summary>
    /// Tests that Dispose removes the component from its container when sited.
    /// </summary>
    [TestMethod]
    public void Dispose_WithSite_RemovesFromContainer()
    {
        // Arrange
        var component = new TestableComponentBase();
        var stubContainer = new StubContainer();
        var stubSite = new StubSite { ContainerValue = stubContainer };
        component.Site = stubSite;

        // Act
        component.Dispose();

        // Assert
        Assert.AreEqual(1, stubContainer.RemoveCallCount);
        Assert.AreSame(component, stubContainer.LastRemovedComponent);
    }

    /// <summary>
    /// Tests that Dispose clears the Site property.
    /// </summary>
    [TestMethod]
    public void Dispose_WithSite_ClearsSite()
    {
        // Arrange
        var component = new TestableComponentBase();
        var stubContainer = new StubContainer();
        var stubSite = new StubSite { ContainerValue = stubContainer };
        component.Site = stubSite;

        // Act
        component.Dispose();

        // Assert
        Assert.IsNull(component.Site);
    }

    /// <summary>
    /// Tests that Dispose raises the Disposed event only once when called multiple times.
    /// </summary>
    [TestMethod]
    public void Dispose_MultipleCalls_RaisesEventOnlyOnce()
    {
        // Arrange
        var component = new TestableComponentBase();
        int eventRaisedCount = 0;
        component.Disposed += (sender, args) => eventRaisedCount++;

        // Act
        component.Dispose();
        component.Dispose();
        component.Dispose();

        // Assert
        Assert.AreEqual(1, eventRaisedCount);
    }

    /// <summary>
    /// Tests that ThrowIfDisposed does not throw when the component has not been disposed.
    /// </summary>
    [TestMethod]
    public void ThrowIfDisposed_WhenNotDisposed_DoesNotThrow()
    {
        // Arrange
        using var component = new TestableComponent();

        // Act & Assert
        component.ExposeThrowIfDisposed();
    }

    /// <summary>
    /// Tests that ThrowIfDisposed can be called multiple times when not disposed without throwing.
    /// </summary>
    [TestMethod]
    public void ThrowIfDisposed_CalledMultipleTimesWhenNotDisposed_DoesNotThrow()
    {
        // Arrange
        using var component = new TestableComponent();

        // Act & Assert
        component.ExposeThrowIfDisposed();
        component.ExposeThrowIfDisposed();
        component.ExposeThrowIfDisposed();
    }

    /// <summary>
    /// Testable concrete implementation of ComponentBase for testing protected members.
    /// </summary>
    private sealed class TestableComponent : ComponentBase
    {
        /// <summary>
        /// Exposes the protected ThrowIfDisposed method for testing.
        /// </summary>
        public void ExposeThrowIfDisposed() => ThrowIfDisposed();
    }

    /// <summary>
    /// Tests that ToString returns only the type name when Site is null.
    /// </summary>
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

    /// <summary>
    /// Tests that ToString returns only the type name when Site is not null but Site.Name is null.
    /// </summary>
    [TestMethod]
    public void ToString_SiteNameIsNull_ReturnsTypeName()
    {
        // Arrange
        TestComponent component = new();
        var stubSite = new StubSite { NameValue = null };
        component.Site = stubSite;
        string expectedTypeName = "TestComponent";

        // Act
        string result = component.ToString();

        // Assert
        Assert.AreEqual(expectedTypeName, result);
    }

    /// <summary>
    /// Tests that ToString returns formatted string with type name and site name when both are available.
    /// </summary>
    [TestMethod]
    [DataRow("MyComponent")]
    [DataRow("Component1")]
    [DataRow("Test")]
    public void ToString_SiteNameIsNotNull_ReturnsFormattedString(string siteName)
    {
        // Arrange
        TestComponent component = new();
        var stubSite = new StubSite { NameValue = siteName };
        component.Site = stubSite;
        string expected = $"TestComponent [{siteName}]";

        // Act
        string result = component.ToString();

        // Assert
        Assert.AreEqual(expected, result);
    }

    /// <summary>
    /// Tests that ToString returns formatted string with empty brackets when Site.Name is empty string.
    /// </summary>
    [TestMethod]
    public void ToString_SiteNameIsEmpty_ReturnsFormattedStringWithEmptyBrackets()
    {
        // Arrange
        TestComponent component = new();
        var stubSite = new StubSite { NameValue = string.Empty };
        component.Site = stubSite;
        string expected = "TestComponent []";

        // Act
        string result = component.ToString();

        // Assert
        Assert.AreEqual(expected, result);
    }

    /// <summary>
    /// Tests that ToString returns formatted string with whitespace in brackets when Site.Name is whitespace.
    /// </summary>
    [TestMethod]
    public void ToString_SiteNameIsWhitespace_ReturnsFormattedStringWithWhitespaceBrackets()
    {
        // Arrange
        TestComponent component = new();
        string whitespace = "   ";
        var stubSite = new StubSite { NameValue = whitespace };
        component.Site = stubSite;
        string expected = $"TestComponent [{whitespace}]";

        // Act
        string result = component.ToString();

        // Assert
        Assert.AreEqual(expected, result);
    }

    #region Dispose(bool) Tests

    #endregion

    #region Helper Classes

    #endregion

    /// <summary>
    /// Verifies that the Site property returns null when first accessed without being set.
    /// Tests the getter with the initial default value.
    /// Expected: Returns null.
    /// </summary>
    [TestMethod]
    public void Site_InitialValue_ReturnsNull()
    {
        // Arrange
        var component = new TestableComponentBase();

        // Act
        var result = component.Site;

        // Assert
        Assert.IsNull(result);
    }

    /// <summary>
    /// Verifies that the Site property returns the value that was set.
    /// Tests both setter and getter with a valid ISite instance.
    /// Expected: Returns the same ISite instance that was set.
    /// </summary>
    [TestMethod]
    public void Site_SetValue_ReturnsSetValue()
    {
        // Arrange
        var component = new TestableComponentBase();
        var stubSite = new StubSite();

        // Act
        component.Site = stubSite;
        var result = component.Site;

        // Assert
        Assert.AreSame(stubSite, result);
    }

    /// <summary>
    /// Verifies that the Site property can be set to null and returns null.
    /// Tests setting null after having a previous non-null value.
    /// Expected: Returns null.
    /// </summary>
    [TestMethod]
    public void Site_SetNull_ReturnsNull()
    {
        // Arrange
        var component = new TestableComponentBase();
        var stubSite = new StubSite();
        component.Site = stubSite;

        // Act
        component.Site = null;
        var result = component.Site;

        // Assert
        Assert.IsNull(result);
    }

    /// <summary>
    /// Verifies that setting the Site property multiple times returns the last set value.
    /// Tests that the setter properly overwrites the previous value.
    /// Expected: Returns the second ISite instance that was set.
    /// </summary>
    [TestMethod]
    public void Site_SetMultipleTimes_ReturnsLastValue()
    {
        // Arrange
        var component = new TestableComponentBase();
        var stubSite1 = new StubSite();
        var stubSite2 = new StubSite();

        // Act
        component.Site = stubSite1;
        component.Site = stubSite2;
        var result = component.Site;

        // Assert
        Assert.AreSame(stubSite2, result);
    }

    private sealed class StubSite : ISite
    {
        public IContainer? ContainerValue { get; set; }
        public string? NameValue { get; set; }
        public Dictionary<Type, object?> Services { get; } = [];
        public int GetServiceCallCount { get; private set; }

        public IComponent Component { get; set; } = null!;
        public IContainer Container => ContainerValue!;
        public bool DesignMode { get; set; }
        public string? Name { get => NameValue; set => NameValue = value; }

        public object? GetService(Type serviceType)
        {
            GetServiceCallCount++;
            return Services.TryGetValue(serviceType, out var service) ? service : null;
        }
    }

    private sealed class StubContainer : IContainer
    {
        private readonly List<IComponent> _components = [];
        public int RemoveCallCount { get; private set; }
        public IComponent? LastRemovedComponent { get; private set; }

        public ComponentCollection Components => new([.. _components]);

        public void Add(IComponent? component) => Add(component, null);

        public void Add(IComponent? component, string? name)
        {
            if (component is not null)
                _components.Add(component);
        }

        public void Remove(IComponent? component)
        {
            RemoveCallCount++;
            LastRemovedComponent = component;
            if (component is not null)
                _components.Remove(component);
        }

        public void Dispose() { }
    }
}