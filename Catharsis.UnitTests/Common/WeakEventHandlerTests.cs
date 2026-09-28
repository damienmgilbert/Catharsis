using Catharsis.Common;

namespace Catharsis.UnitTests.Common;

///<summary>
///Unit tests for the <see cref="WeakEventHandler{TEventArgs}"/> class.
///</summary>
[TestClass]
public class WeakEventHandlerTests
{
    #region Constructor

    [TestMethod]
    public void Constructor_NullHandler_Throws() { Assert.ThrowsExactly<ArgumentNullException>(static () => new WeakEventHandler<EventArgs>(null!)); }

    #endregion

    #region Invoke

    [TestMethod]
    public void Invoke_StaticHandler_Invokes()
    {
        int calls = 0;
        EventHandler<EventArgs> handler = static (_, _) => { };
        WeakEventHandler<EventArgs> weak = new((sender, args) => { calls++; handler(sender, args); });

        bool invoked = weak.Invoke(this, EventArgs.Empty);

        Assert.IsTrue(invoked);
        Assert.AreEqual(1, calls);
    }

    [TestMethod]
    public void Invoke_LiveTarget_InvokesAndReturnsTrue()
    {
        Subscriber subscriber = new();
        WeakEventHandler<EventArgs> weak = new(subscriber.Handle);

        bool invoked = weak.Invoke(this, EventArgs.Empty);

        Assert.IsTrue(invoked);
        Assert.AreEqual(1, subscriber.CallCount);
        GC.KeepAlive(subscriber);
    }

    [TestMethod]
    public void IsTargetAlive_StaticHandler_AlwaysTrue()
    {
        WeakEventHandler<EventArgs> weak = new(static (_, _) => { });
        Assert.IsTrue(weak.IsTargetAlive);
    }

    [TestMethod]
    public void IsTargetAlive_LiveTarget_ReturnsTrue()
    {
        Subscriber subscriber = new();
        WeakEventHandler<EventArgs> weak = new(subscriber.Handle);

        Assert.IsTrue(weak.IsTargetAlive);
        GC.KeepAlive(subscriber);
    }

    [TestMethod]
    public void Invoke_CollectedTarget_ReturnsFalse()
    {
        WeakEventHandler<EventArgs> weak = CreateWeakHandlerToCollectedTarget();

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        bool invoked = weak.Invoke(this, EventArgs.Empty);

        Assert.IsFalse(invoked);
    }

    #endregion

    #region Private methods
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    private static WeakEventHandler<EventArgs> CreateWeakHandlerToCollectedTarget()
    {
        Subscriber subscriber = new();
        return new WeakEventHandler<EventArgs>(subscriber.Handle);
    }
    #endregion

    private sealed class Subscriber
    {
        #region Public methods
        public void Handle(object? sender, EventArgs args) => CallCount++;
        #endregion

        #region Public properties
        public int CallCount { get; private set; }
        #endregion
    }
}
