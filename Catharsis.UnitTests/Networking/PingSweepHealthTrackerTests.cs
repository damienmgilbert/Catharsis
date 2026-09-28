using Catharsis.Networking;

namespace Catharsis.UnitTests.Networking;

///<summary>
///Unit tests for the <see cref="PingSweepHealthTracker"/> class.
///</summary>
[TestClass]
public class PingSweepHealthTrackerTests
{
    #region Constructor

    [TestMethod]
    public void Constructor_NullHealthTracker_Throws() { Assert.ThrowsExactly<ArgumentNullException>(static () => new PingSweepHealthTracker(null!)); }

    [TestMethod]
    public void Constructor_ZeroTimeout_Throws() { Assert.ThrowsExactly<ArgumentOutOfRangeException>(static () => new PingSweepHealthTracker(new EndpointHealthTracker(), TimeSpan.Zero)); }

    [TestMethod]
    public void Constructor_NegativeTimeout_Throws() { Assert.ThrowsExactly<ArgumentOutOfRangeException>(static () => new PingSweepHealthTracker(new EndpointHealthTracker(), TimeSpan.FromSeconds(-1))); }

    #endregion

    #region PingAsync

    [TestMethod]
    public async Task PingAsync_NullEndpoint_Throws()
    {
        PingSweepHealthTracker tracker = new(new EndpointHealthTracker());
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => tracker.PingAsync(null!));
    }

    [TestMethod]
    public async Task PingAsync_Loopback_ReturnsTrue()
    {
        PingSweepHealthTracker tracker = new(new EndpointHealthTracker());
        bool result = await tracker.PingAsync(new Uri("https://127.0.0.1"));

        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task PingAsync_Loopback_RecordsSuccessInHealthTracker()
    {
        EndpointHealthTracker healthTracker = new();
        PingSweepHealthTracker tracker = new(healthTracker);
        Uri endpoint = new("https://127.0.0.1");

        await tracker.PingAsync(endpoint);

        Assert.AreEqual(1.0, healthTracker.GetSuccessRatio(endpoint));
    }

    [TestMethod]
    public async Task PingAsync_UnresolvableHost_ReturnsFalse()
    {
        PingSweepHealthTracker tracker = new(new EndpointHealthTracker(), TimeSpan.FromSeconds(1));
        bool result = await tracker.PingAsync(new Uri("https://this-host-does-not-exist.invalid"));

        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task PingAsync_UnresolvableHost_RecordsFailureInHealthTracker()
    {
        EndpointHealthTracker healthTracker = new();
        PingSweepHealthTracker tracker = new(healthTracker, TimeSpan.FromSeconds(1));
        Uri endpoint = new("https://this-host-does-not-exist.invalid");

        await tracker.PingAsync(endpoint);

        Assert.AreEqual(0.0, healthTracker.GetSuccessRatio(endpoint));
    }

    #endregion

    #region SweepAsync

    [TestMethod]
    public async Task SweepAsync_NullEndpoints_Throws()
    {
        PingSweepHealthTracker tracker = new(new EndpointHealthTracker());
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => tracker.SweepAsync(null!));
    }

    [TestMethod]
    public async Task SweepAsync_MultipleEndpoints_ReturnsResultForEach()
    {
        PingSweepHealthTracker tracker = new(new EndpointHealthTracker(), TimeSpan.FromSeconds(1));
        Uri loopback = new("https://127.0.0.1");
        Uri unresolvable = new("https://this-host-does-not-exist.invalid");

        IReadOnlyDictionary<Uri, bool> results = await tracker.SweepAsync([loopback, unresolvable]);

        Assert.HasCount(2, results);
        Assert.IsTrue(results[loopback]);
        Assert.IsFalse(results[unresolvable]);
    }

    #endregion
}
