using Catharsis.ComponentModel.Validation;

namespace Catharsis.UnitTests.ComponentModel.Validation;

[TestClass]
public sealed class ValidationContextFactoryTests
{
    [TestMethod]
    public void CreateContext_ValidInstance_ReturnsContextForInstance()
    {
        var factory = new ValidationContextFactory();
        var instance = new object();

        var context = factory.CreateContext(instance);

        Assert.AreSame(instance, context.ObjectInstance);
    }

    [TestMethod]
    public void CreateContext_NullInstance_ThrowsArgumentNullException()
    {
        var factory = new ValidationContextFactory();

        Assert.ThrowsExactly<ArgumentNullException>(() => factory.CreateContext(null!));
    }

    [TestMethod]
    public void CreatePropertyContext_SetsMemberName()
    {
        var factory = new ValidationContextFactory();
        var instance = new object();

        var context = factory.CreatePropertyContext(instance, "Name");

        Assert.AreEqual("Name", context.MemberName);
    }

    [TestMethod]
    public void CreatePropertyContext_NullInstance_ThrowsArgumentNullException()
    {
        var factory = new ValidationContextFactory();

        Assert.ThrowsExactly<ArgumentNullException>(
            () => factory.CreatePropertyContext(null!, "Name"));
    }

    [TestMethod]
    public void CreatePropertyContext_NullMemberName_ThrowsArgumentNullException()
    {
        var factory = new ValidationContextFactory();

        Assert.ThrowsExactly<ArgumentNullException>(
            () => factory.CreatePropertyContext(new object(), null!));
    }

    [TestMethod]
    public void CreatePropertyContext_WithDisplayName_SetsDisplayName()
    {
        var factory = new ValidationContextFactory();
        var instance = new object();

        var context = factory.CreatePropertyContext(instance, "Name", "Full Name");

        Assert.AreEqual("Name", context.MemberName);
        Assert.AreEqual("Full Name", context.DisplayName);
    }

    [TestMethod]
    public void CreatePropertyContext_WithDisplayName_NullDisplayName_ThrowsArgumentNullException()
    {
        var factory = new ValidationContextFactory();

        Assert.ThrowsExactly<ArgumentNullException>(
            () => factory.CreatePropertyContext(new object(), "Name", null!));
    }

    [TestMethod]
    public void Constructor_WithItems_AttachesItems()
    {
        var items = new Dictionary<object, object?> { ["key"] = "value" };
        var factory = new ValidationContextFactory(items: items);
        var instance = new object();

        var context = factory.CreateContext(instance);

        Assert.AreEqual("value", context.Items["key"]);
    }

    [TestMethod]
    public void Constructor_WithServiceProvider_AttachesProvider()
    {
        var provider = new StubServiceProvider();
        var factory = new ValidationContextFactory(serviceProvider: provider);
        var instance = new object();

        var context = factory.CreateContext(instance);

        Assert.IsNotNull(context);
    }

    private sealed class StubServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }
}
