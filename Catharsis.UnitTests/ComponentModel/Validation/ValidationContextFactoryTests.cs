using System.ComponentModel.DataAnnotations;
using Catharsis.ComponentModel.Validation;

namespace Catharsis.UnitTests.ComponentModel.Validation;

[TestClass]
public sealed class ValidationContextFactoryTests
{
    #region Public methods
    [TestMethod]
    public void Constructor_WithItems_AttachesItems()
    {
        Dictionary<object, object?> items = new Dictionary<object, object?> { ["key"] = "value" };
        ValidationContextFactory factory = new ValidationContextFactory(items: items);
        object instance = new object();

        ValidationContext context = factory.CreateContext(instance);

        Assert.AreEqual("value", context.Items["key"]);
    }

    [TestMethod]
    public void Constructor_WithServiceProvider_AttachesProvider()
    {
        StubServiceProvider provider = new StubServiceProvider();
        ValidationContextFactory factory = new ValidationContextFactory(serviceProvider: provider);
        object instance = new object();

        ValidationContext context = factory.CreateContext(instance);

        Assert.IsNotNull(context);
    }

    [TestMethod]
    public void CreateContext_NullInstance_ThrowsArgumentNullException()
    {
        ValidationContextFactory factory = new ValidationContextFactory();

        Assert.ThrowsExactly<ArgumentNullException>(() => factory.CreateContext(null!));
    }

    [TestMethod]
    public void CreateContext_ValidInstance_ReturnsContextForInstance()
    {
        ValidationContextFactory factory = new ValidationContextFactory();
        object instance = new object();

        ValidationContext context = factory.CreateContext(instance);

        Assert.AreSame(instance, context.ObjectInstance);
    }

    [TestMethod]
    public void CreatePropertyContext_NullInstance_ThrowsArgumentNullException()
    {
        ValidationContextFactory factory = new ValidationContextFactory();

        Assert.ThrowsExactly<ArgumentNullException>(() => factory.CreatePropertyContext(null!, "Name"));
    }

    [TestMethod]
    public void CreatePropertyContext_NullMemberName_ThrowsArgumentNullException()
    {
        ValidationContextFactory factory = new ValidationContextFactory();

        Assert.ThrowsExactly<ArgumentNullException>(() => factory.CreatePropertyContext(new object(), null!));
    }

    [TestMethod]
    public void CreatePropertyContext_SetsMemberName()
    {
        ValidationContextFactory factory = new ValidationContextFactory();
        object instance = new object();

        ValidationContext context = factory.CreatePropertyContext(instance, "Name");

        Assert.AreEqual("Name", context.MemberName);
    }

    [TestMethod]
    public void CreatePropertyContext_WithDisplayName_NullDisplayName_ThrowsArgumentNullException()
    {
        ValidationContextFactory factory = new ValidationContextFactory();

        Assert.ThrowsExactly<ArgumentNullException>(() => factory.CreatePropertyContext(new object(), "Name", null!));
    }

    [TestMethod]
    public void CreatePropertyContext_WithDisplayName_SetsDisplayName()
    {
        ValidationContextFactory factory = new ValidationContextFactory();
        object instance = new object();

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
