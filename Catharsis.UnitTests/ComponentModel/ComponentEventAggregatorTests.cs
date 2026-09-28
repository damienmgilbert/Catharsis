using Catharsis.ComponentModel;

namespace Catharsis.UnitTests.ComponentModel;

///<summary>
///Unit tests for the <see cref="ComponentEventAggregator"/> class.
///</summary>
[TestClass]
public class ComponentEventAggregatorTests
{
    #region Subscribe

    [TestMethod]
    public void Subscribe_NullHandler_Throws()
    {
        ComponentEventAggregator aggregator = new();
        Assert.ThrowsExactly<ArgumentNullException>(() => aggregator.Subscribe<string>(null!));
    }

    #endregion

    #region Publish

    [TestMethod]
    public void Publish_NullEvent_Throws()
    {
        ComponentEventAggregator aggregator = new();
        Assert.ThrowsExactly<ArgumentNullException>(() => aggregator.Publish<string>(null!));
    }

    [TestMethod]
    public void Publish_NoSubscribers_DoesNotThrow()
    {
        ComponentEventAggregator aggregator = new();
        aggregator.Publish("hello");
    }

    [TestMethod]
    public void Publish_SubscribedHandler_ReceivesEvent()
    {
        ComponentEventAggregator aggregator = new();
        string? received = null;
        aggregator.Subscribe<string>(value => received = value);

        aggregator.Publish("hello");

        Assert.AreEqual("hello", received);
    }

    [TestMethod]
    public void Publish_MultipleSubscribers_AllReceiveEvent()
    {
        ComponentEventAggregator aggregator = new();
        List<string> received = [];

        aggregator.Subscribe<string>(received.Add);
        aggregator.Subscribe<string>(received.Add);

        aggregator.Publish("hello");

        CollectionAssert.AreEqual(new[] { "hello", "hello" }, received);
    }

    [TestMethod]
    public void Publish_DifferentEventType_DoesNotInvokeHandler()
    {
        ComponentEventAggregator aggregator = new();
        bool invoked = false;
        aggregator.Subscribe<string>(_ => invoked = true);

        aggregator.Publish(42);

        Assert.IsFalse(invoked);
    }

    #endregion

    #region Unsubscribe (via disposal)

    [TestMethod]
    public void Dispose_UnsubscribesHandler()
    {
        ComponentEventAggregator aggregator = new();
        bool invoked = false;
        IDisposable subscription = aggregator.Subscribe<string>(_ => invoked = true);

        subscription.Dispose();
        aggregator.Publish("hello");

        Assert.IsFalse(invoked);
    }

    [TestMethod]
    public void Dispose_OnlyUnsubscribesThatHandler()
    {
        ComponentEventAggregator aggregator = new();
        List<string> received = [];
        IDisposable first = aggregator.Subscribe<string>(received.Add);
        aggregator.Subscribe<string>(received.Add);

        first.Dispose();
        aggregator.Publish("hello");

        CollectionAssert.AreEqual(new[] { "hello" }, received);
    }

    #endregion
}
