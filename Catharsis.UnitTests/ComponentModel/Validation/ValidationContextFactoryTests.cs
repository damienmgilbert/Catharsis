using System.ComponentModel.DataAnnotations;
using Catharsis.ComponentModel.Validation;

namespace Catharsis.UnitTests.ComponentModel.Validation;

///<summary>
///Unit tests for the <see cref="ValidationContextFactory"/> class.
///</summary>
[TestClass]
public sealed class ValidationContextFactoryTests
{
    #region Public methods
    [TestMethod]
    public void Constructor_WithItems_AttachesItems()
    {
        Dictionary<object, object?> items = new() { ["key"] = "value" };
        ValidationContextFactory factory = new(items: items);
        object instance = new();

        ValidationContext context = factory.CreateContext(instance);

        Assert.AreEqual("value", context.Items["key"]);
    }

    [TestMethod]
    public void Constructor_WithServiceProvider_AttachesProvider()
    {
        StubServiceProvider provider = new();
        ValidationContextFactory factory = new(serviceProvider: provider);
        object instance = new();

        ValidationContext context = factory.CreateContext(instance);

        Assert.IsNotNull(context);
    }

    [TestMethod]
    public void CreateContext_NullInstance_ThrowsArgumentNullException()
    {
        ValidationContextFactory factory = new();

        Assert.ThrowsExactly<ArgumentNullException>(() => factory.CreateContext(null!));
    }

    [TestMethod]
    public void CreateContext_ValidInstance_ReturnsContextForInstance()
    {
        ValidationContextFactory factory = new();
        object instance = new();

        ValidationContext context = factory.CreateContext(instance);

        Assert.AreSame(instance, context.ObjectInstance);
    }

    [TestMethod]
    public void CreatePropertyContext_NullInstance_ThrowsArgumentNullException()
    {
        ValidationContextFactory factory = new();

        Assert.ThrowsExactly<ArgumentNullException>(() => factory.CreatePropertyContext(null!, "Name"));
    }

    [TestMethod]
    public void CreatePropertyContext_NullMemberName_ThrowsArgumentNullException()
    {
        ValidationContextFactory factory = new();

        Assert.ThrowsExactly<ArgumentNullException>(() => factory.CreatePropertyContext(new object(), null!));
    }

    [TestMethod]
    public void CreatePropertyContext_SetsMemberName()
    {
        ValidationContextFactory factory = new();
        object instance = new();

        ValidationContext context = factory.CreatePropertyContext(instance, "Name");

        Assert.AreEqual("Name", context.MemberName);
    }

    [TestMethod]
    public void CreatePropertyContext_WithDisplayName_NullDisplayName_ThrowsArgumentNullException()
    {
        ValidationContextFactory factory = new();

        Assert.ThrowsExactly<ArgumentNullException>(() => factory.CreatePropertyContext(new object(), "Name", null!));
    }

    [TestMethod]
    public void CreatePropertyContext_WithDisplayName_SetsDisplayName()
    {
        ValidationContextFactory factory = new();
        object instance = new();

        ValidationContext context = factory.CreatePropertyContext(instance, "Name", "Full Name");

        Assert.AreEqual("Name", context.MemberName);
        Assert.AreEqual("Full Name", context.DisplayName);
    }
    #endregion

    sealed class StubServiceProvider : IServiceProvider
    {
        #region Public methods
        public object? GetService(Type serviceType) { return null; }
        #endregion
    }
}
