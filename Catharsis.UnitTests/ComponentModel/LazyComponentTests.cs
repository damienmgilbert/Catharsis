using Catharsis.ComponentModel;
using System.ComponentModel;

namespace Catharsis.UnitTests.ComponentModel;

///<summary>
///Unit tests for the <see cref="LazyComponent{T}"/> class.
///</summary>
[TestClass]
public class LazyComponentTests
{
    #region Constructor

    [TestMethod]
    public void Constructor_NullFactory_Throws() { Assert.ThrowsExactly<ArgumentNullException>(static () => new LazyComponent<TestComponent>(null!)); }

    #endregion

    #region IsValueCreated / Value

    [TestMethod]
    public void IsValueCreated_BeforeAccess_IsFalse()
    {
        LazyComponent<TestComponent> lazy = new(static () => new TestComponent());
        Assert.IsFalse(lazy.IsValueCreated);
    }

    [TestMethod]
    public void Value_FirstAccess_InvokesFactory()
    {
        int calls = 0;
        LazyComponent<TestComponent> lazy = new(() => { calls++; return new TestComponent(); });

        TestComponent component = lazy.Value;

        Assert.IsNotNull(component);
        Assert.AreEqual(1, calls);
        Assert.IsTrue(lazy.IsValueCreated);
    }

    [TestMethod]
    public void Value_RepeatedAccess_ReturnsSameInstance()
    {
        LazyComponent<TestComponent> lazy = new(static () => new TestComponent());
        TestComponent first = lazy.Value;
        TestComponent second = lazy.Value;

        Assert.AreSame(first, second);
    }

    #endregion

    #region Dispose

    [TestMethod]
    public void Dispose_ValueNeverAccessed_DoesNotInvokeFactory()
    {
        int calls = 0;
        LazyComponent<TestComponent> lazy = new(() => { calls++; return new TestComponent(); });

        lazy.Dispose();

        Assert.AreEqual(0, calls);
    }

    [TestMethod]
    public void Dispose_ValueAccessed_DisposesComponent()
    {
        LazyComponent<TestComponent> lazy = new(static () => new TestComponent());
        TestComponent component = lazy.Value;

        lazy.Dispose();

        Assert.IsTrue(component.WasDisposed);
    }

    #endregion

    private sealed class TestComponent : IComponent
    {
        #region Events
        public event EventHandler? Disposed;
        #endregion

        #region Public methods
        public void Dispose()
        {
            WasDisposed = true;
            Disposed?.Invoke(this, EventArgs.Empty);
        }
        #endregion

        #region Public properties
        public ISite? Site { get; set; }
        public bool WasDisposed { get; private set; }
        #endregion
    }
}
