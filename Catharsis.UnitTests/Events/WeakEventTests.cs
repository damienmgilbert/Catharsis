using Catharsis.Events;
using System.Runtime.CompilerServices;

namespace Catharsis.UnitTests.Events;

///<summary>
///Unit tests for the <see cref="WeakEvent{TArgs}"/> class.
///</summary>
[TestClass]
public class WeakEventTests
{
    sealed class Listener
    {
        public int Calls { get; private set; }

        public void OnEvent(object? sender, int args) => Calls += args;
    }

    static int StaticCalls;

    static void StaticHandler(object? sender, int args) => Interlocked.Add(ref StaticCalls, args);

    [TestMethod]
    public void Subscribe_NullHandler_Throws() { Assert.ThrowsExactly<ArgumentNullException>(static () => new WeakEvent<int>().Subscribe(null!)); }

    [TestMethod]
    public void Subscribe_MulticastDelegate_Throws()
    {
        Listener a = new();
        Listener b = new();
        EventHandler<int> multicast = a.OnEvent;
        multicast += b.OnEvent;

        Assert.ThrowsExactly<ArgumentException>(() => new WeakEvent<int>().Subscribe(multicast));
    }

    [TestMethod]
    public void Invoke_InstanceHandler_IsCalled()
    {
        WeakEvent<int> evt = new();
        Listener listener = new();

        evt.Subscribe(listener.OnEvent);
        evt.Invoke(null, 3);
        evt.Invoke(null, 4);

        Assert.AreEqual(7, listener.Calls);
        GC.KeepAlive(listener);
    }

    [TestMethod]
    public void Invoke_StaticHandler_IsCalled()
    {
        WeakEvent<int> evt = new();
        int before = Volatile.Read(ref StaticCalls);

        evt.Subscribe(StaticHandler);
        evt.Invoke(null, 5);

        Assert.AreEqual(before + 5, Volatile.Read(ref StaticCalls));
    }

    sealed class Thrower
    {
        public void OnEvent(object? sender, int args) => throw new InvalidOperationException("boom");
    }

    [TestMethod]
    public void Invoke_HandlerThrows_PropagatesOriginalExceptionUnwrapped()
    {
        WeakEvent<int> evt = new();
        Thrower thrower = new();
        evt.Subscribe(thrower.OnEvent);

        InvalidOperationException ex = Assert.ThrowsExactly<InvalidOperationException>(() => evt.Invoke(null, 1));

        Assert.AreEqual("boom", ex.Message);
        GC.KeepAlive(thrower);
    }

    [TestMethod]
    public void Unsubscribe_RemovesHandler()
    {
        WeakEvent<int> evt = new();
        Listener listener = new();

        evt.Subscribe(listener.OnEvent);
        Assert.IsTrue(evt.Unsubscribe(listener.OnEvent));
        Assert.IsFalse(evt.Unsubscribe(listener.OnEvent));
        evt.Invoke(null, 1);

        Assert.AreEqual(0, listener.Calls);
    }

    [TestMethod]
    public void Subscribe_DoesNotKeepSubscriberAlive()
    {
        WeakEvent<int> evt = new();
        WeakReference weak = SubscribeTransient(evt);

        for (int i = 0; i < 5 && weak.IsAlive; i++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        Assert.IsFalse(weak.IsAlive);
        Assert.AreEqual(0, evt.Count);
        evt.Invoke(null, 1);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    static WeakReference SubscribeTransient(WeakEvent<int> evt)
    {
        Listener listener = new();
        evt.Subscribe(listener.OnEvent);
        return new WeakReference(listener);
    }
}
