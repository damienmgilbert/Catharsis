using Catharsis.ComponentModel;

namespace Catharsis.UnitTests.ComponentModel;

///<summary>
///Unit tests for the <see cref="DynamicComponentProxy"/> class.
///</summary>
[TestClass]
public class DynamicComponentProxyTests
{
    #region Constructor

    [TestMethod]
    public void Constructor_NullComponent_Throws() { Assert.ThrowsExactly<ArgumentNullException>(static () => new DynamicComponentProxy(null!)); }

    #endregion

    #region Dynamic get

    [TestMethod]
    public void DynamicGet_ExistingReadableProperty_ReturnsValue()
    {
        dynamic proxy = new DynamicComponentProxy(new Widget { Name = "alpha" });
        Assert.AreEqual("alpha", proxy.Name);
    }

    [TestMethod]
    public void DynamicGet_UnknownProperty_ThrowsRuntimeBinderException()
    {
        dynamic proxy = new DynamicComponentProxy(new Widget());
        Assert.ThrowsExactly<Microsoft.CSharp.RuntimeBinder.RuntimeBinderException>(() => _ = proxy.DoesNotExist);
    }

    #endregion

    #region Dynamic set

    [TestMethod]
    public void DynamicSet_ExistingWritableProperty_UpdatesUnderlyingComponent()
    {
        Widget widget = new();
        dynamic proxy = new DynamicComponentProxy(widget);

        proxy.Name = "beta";

        Assert.AreEqual("beta", widget.Name);
    }

    [TestMethod]
    public void DynamicSet_ReadOnlyProperty_ThrowsRuntimeBinderException()
    {
        dynamic proxy = new DynamicComponentProxy(new Widget());
        Assert.ThrowsExactly<Microsoft.CSharp.RuntimeBinder.RuntimeBinderException>(() => proxy.ReadOnlyValue = 1);
    }

    #endregion

    #region GetDynamicMemberNames

    [TestMethod]
    public void GetDynamicMemberNames_ReturnsPublicPropertyNames()
    {
        DynamicComponentProxy proxy = new(new Widget());
        List<string> names = [.. proxy.GetDynamicMemberNames()];

        CollectionAssert.Contains(names, "Name");
        CollectionAssert.Contains(names, "ReadOnlyValue");
    }

    #endregion

    #region Component

    [TestMethod]
    public void Component_ReturnsWrappedInstance()
    {
        Widget widget = new();
        DynamicComponentProxy proxy = new(widget);

        Assert.AreSame(widget, proxy.Component);
    }

    #endregion

    #region Test types
    sealed class Widget
    {
        public string? Name { get; set; }
        public int ReadOnlyValue => 42;
    }
    #endregion
}
