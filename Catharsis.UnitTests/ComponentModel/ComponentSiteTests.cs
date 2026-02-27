using System.ComponentModel;

namespace Catharsis.ComponentModel.UnitTests;


/// <summary>
/// Unit tests for the <see cref="ComponentSite"/> class.
/// </summary>
[TestClass]
public sealed class ComponentSiteTests
{
    #region GetService Tests

    /// <summary>
    /// Tests that GetService throws ArgumentNullException when serviceType is null.
    /// </summary>
    [TestMethod]
    public void GetService_NullServiceType_ThrowsArgumentNullException()
    {
        // Arrange
        var site = new ComponentSite(new StubContainer(), new StubComponent());

        // Act & Assert
        Assert.ThrowsExactly<ArgumentNullException>(() => site.GetService(null!));
    }

    /// <summary>
    /// Tests that GetService returns the site instance when requesting ISite.
    /// </summary>
    [TestMethod]
    public void GetService_RequestISite_ReturnsSiteInstance()
    {
        // Arrange
        var site = new ComponentSite(new StubContainer(), new StubComponent());

        // Act
        object? result = site.GetService(typeof(ISite));

        // Assert
        Assert.IsNotNull(result);
        Assert.AreSame(site, result);
    }

    /// <summary>
    /// Tests that GetService returns the container when requesting IContainer.
    /// </summary>
    [TestMethod]
    public void GetService_RequestIContainer_ReturnsContainer()
    {
        // Arrange
        var stubContainer = new StubContainer();
        var site = new ComponentSite(stubContainer, new StubComponent());

        // Act
        object? result = site.GetService(typeof(IContainer));

        // Assert
        Assert.IsNotNull(result);
        Assert.AreSame(stubContainer, result);
    }

    /// <summary>
    /// Tests that GetService returns the component when requesting IComponent.
    /// </summary>
    [TestMethod]
    public void GetService_RequestIComponent_ReturnsComponent()
    {
        // Arrange
        var stubComponent = new StubComponent();
        var site = new ComponentSite(new StubContainer(), stubComponent);

        // Act
        object? result = site.GetService(typeof(IComponent));

        // Assert
        Assert.IsNotNull(result);
        Assert.AreSame(stubComponent, result);
    }

    /// <summary>
    /// Tests that GetService returns service from serviceProvider when available.
    /// </summary>
    [TestMethod]
    public void GetService_ServiceAvailableFromServiceProvider_ReturnsService()
    {
        // Arrange
        var expectedService = new object();
        var stubServiceProvider = new StubServiceProvider();
        stubServiceProvider.Services[typeof(string)] = expectedService;
        var site = new ComponentSite(new StubContainer(), new StubComponent(), null, false, stubServiceProvider);

        // Act
        object? result = site.GetService(typeof(string));

        // Assert
        Assert.IsNotNull(result);
        Assert.AreSame(expectedService, result);
    }

    /// <summary>
    /// Tests that GetService returns null when serviceProvider returns null and container is not IServiceProvider.
    /// </summary>
    [TestMethod]
    public void GetService_ServiceProviderReturnsNullAndContainerNotServiceProvider_ReturnsNull()
    {
        // Arrange
        var stubServiceProvider = new StubServiceProvider();
        var site = new ComponentSite(new StubContainer(), new StubComponent(), null, false, stubServiceProvider);

        // Act
        object? result = site.GetService(typeof(string));

        // Assert
        Assert.IsNull(result);
    }

    /// <summary>
    /// Tests that GetService falls back to container as IServiceProvider when serviceProvider returns null.
    /// </summary>
    [TestMethod]
    public void GetService_ServiceProviderReturnsNullButContainerIsServiceProvider_ReturnsServiceFromContainer()
    {
        // Arrange
        var expectedService = new object();
        var stubContainerWithServices = new StubContainerWithServiceProvider();
        stubContainerWithServices.Services[typeof(string)] = expectedService;
        var stubServiceProvider = new StubServiceProvider();
        var site = new ComponentSite(stubContainerWithServices, new StubComponent(), null, false, stubServiceProvider);

        // Act
        object? result = site.GetService(typeof(string));

        // Assert
        Assert.IsNotNull(result);
        Assert.AreSame(expectedService, result);
    }

    /// <summary>
    /// Tests that GetService uses container as IServiceProvider when no serviceProvider is provided.
    /// </summary>
    [TestMethod]
    public void GetService_NoServiceProviderButContainerIsServiceProvider_ReturnsServiceFromContainer()
    {
        // Arrange
        var expectedService = new object();
        var stubContainerWithServices = new StubContainerWithServiceProvider();
        stubContainerWithServices.Services[typeof(string)] = expectedService;
        var site = new ComponentSite(stubContainerWithServices, new StubComponent());

        // Act
        object? result = site.GetService(typeof(string));

        // Assert
        Assert.IsNotNull(result);
        Assert.AreSame(expectedService, result);
    }

    /// <summary>
    /// Tests that GetService returns null when no serviceProvider and container is not IServiceProvider.
    /// </summary>
    [TestMethod]
    public void GetService_NoServiceProviderAndContainerNotServiceProvider_ReturnsNull()
    {
        // Arrange
        var site = new ComponentSite(new StubContainer(), new StubComponent());

        // Act
        object? result = site.GetService(typeof(string));

        // Assert
        Assert.IsNull(result);
    }

    /// <summary>
    /// Tests that GetService returns null when both serviceProvider and container return null.
    /// </summary>
    [TestMethod]
    public void GetService_BothServiceProviderAndContainerReturnNull_ReturnsNull()
    {
        // Arrange
        var stubContainerWithServices = new StubContainerWithServiceProvider();
        var stubServiceProvider = new StubServiceProvider();
        var site = new ComponentSite(stubContainerWithServices, new StubComponent(), null, false, stubServiceProvider);

        // Act
        object? result = site.GetService(typeof(string));

        // Assert
        Assert.IsNull(result);
    }

    #endregion

    /// <summary>
    /// Tests that the constructor initializes all properties correctly with valid parameters.
    /// </summary>
    /// <param name="name">The name value to test.</param>
    /// <param name="designMode">The design mode value to test.</param>
    /// <param name="hasServiceProvider">Whether to provide a service provider.</param>
    [TestMethod]
    [DataRow(null, false, false, DisplayName = "Defaults: null name, false designMode, no serviceProvider")]
    [DataRow("TestComponent", false, false, DisplayName = "Normal name, false designMode, no serviceProvider")]
    [DataRow("TestComponent", true, false, DisplayName = "Normal name, true designMode, no serviceProvider")]
    [DataRow("TestComponent", false, true, DisplayName = "Normal name, false designMode, with serviceProvider")]
    [DataRow("TestComponent", true, true, DisplayName = "Normal name, true designMode, with serviceProvider")]
    [DataRow("", false, false, DisplayName = "Empty name")]
    [DataRow("   ", false, false, DisplayName = "Whitespace name")]
    [DataRow("Component-With_Special.Chars@123!", false, false, DisplayName = "Special characters in name")]
    public void ComponentSite_ValidParameters_InitializesPropertiesCorrectly(
        string? name,
        bool designMode,
        bool hasServiceProvider)
    {
        // Arrange
        var container = new StubContainer();
        var component = new StubComponent();
        var serviceProvider = hasServiceProvider ? new StubServiceProvider() : null;

        // Act
        var site = new ComponentSite(container, component, name, designMode, serviceProvider);

        // Assert
        Assert.AreSame(container, site.Container, "Container property should return the provided container.");
        Assert.AreSame(component, site.Component, "Component property should return the provided component.");
        Assert.AreEqual(name, site.Name, "Name property should return the provided name.");
        Assert.AreEqual(designMode, site.DesignMode, "DesignMode property should return the provided designMode.");
    }

    /// <summary>
    /// Tests that the constructor correctly handles a very long name string.
    /// </summary>
    [TestMethod]
    public void ComponentSite_VeryLongName_InitializesNameCorrectly()
    {
        // Arrange
        var container = new StubContainer();
        var component = new StubComponent();
        string veryLongName = new string('A', 10000);

        // Act
        var site = new ComponentSite(container, component, veryLongName);

        // Assert
        Assert.AreEqual(veryLongName, site.Name, "Name property should handle very long strings.");
    }

    /// <summary>
    /// Tests that the Name property can be set after construction.
    /// </summary>
    [TestMethod]
    public void ComponentSite_NameProperty_CanBeSet()
    {
        // Arrange
        var site = new ComponentSite(new StubContainer(), new StubComponent(), "InitialName");
        string newName = "UpdatedName";

        // Act
        site.Name = newName;

        // Assert
        Assert.AreEqual(newName, site.Name, "Name property should be settable.");
    }

    /// <summary>
    /// Tests that the Name property can be set to null after construction.
    /// </summary>
    [TestMethod]
    public void ComponentSite_NameProperty_CanBeSetToNull()
    {
        // Arrange
        var site = new ComponentSite(new StubContainer(), new StubComponent(), "InitialName");

        // Act
        site.Name = null;

        // Assert
        Assert.IsNull(site.Name, "Name property should be settable to null.");
    }

    /// <summary>
    /// Tests that the constructor correctly initializes with minimal parameters (using defaults).
    /// </summary>
    [TestMethod]
    public void ComponentSite_MinimalParameters_UsesDefaults()
    {
        // Arrange
        var container = new StubContainer();
        var component = new StubComponent();

        // Act
        var site = new ComponentSite(container, component);

        // Assert
        Assert.AreSame(container, site.Container);
        Assert.AreSame(component, site.Component);
        Assert.IsNull(site.Name, "Name should default to null.");
        Assert.IsFalse(site.DesignMode, "DesignMode should default to false.");
    }

    private sealed class StubContainer : IContainer
    {
        public ComponentCollection Components => new([]);
        public void Add(IComponent? component) { }
        public void Add(IComponent? component, string? name) { }
        public void Remove(IComponent? component) { }
        public void Dispose() { }
    }

    private sealed class StubComponent : IComponent
    {
        public ISite? Site { get; set; }
        public event EventHandler? Disposed;
        public void Dispose() => Disposed?.Invoke(this, EventArgs.Empty);
    }

    private sealed class StubServiceProvider : IServiceProvider
    {
        public Dictionary<Type, object?> Services { get; } = [];

        public object? GetService(Type serviceType) =>
            Services.TryGetValue(serviceType, out var service) ? service : null;
    }

    private sealed class StubContainerWithServiceProvider : IContainer, IServiceProvider
    {
        public Dictionary<Type, object?> Services { get; } = [];
        public ComponentCollection Components => new([]);
        public void Add(IComponent? component) { }
        public void Add(IComponent? component, string? name) { }
        public void Remove(IComponent? component) { }
        public void Dispose() { }

        public object? GetService(Type serviceType) =>
            Services.TryGetValue(serviceType, out var service) ? service : null;
    }
}