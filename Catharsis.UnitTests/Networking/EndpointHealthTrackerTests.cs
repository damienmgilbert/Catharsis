using Catharsis.Networking;

namespace Catharsis.UnitTests.Networking;

///<summary>
///Unit tests for the <see cref="EndpointHealthTracker"/> class.
///</summary>
[TestClass]
public class EndpointHealthTrackerTests
{
    #region GetSuccessRatio

    [TestMethod]
    public void GetSuccessRatio_NullEndpoint_Throws()
    {
        EndpointHealthTracker tracker = new();
        Assert.ThrowsExactly<ArgumentNullException>(() => tracker.GetSuccessRatio(null!));
    }

    [TestMethod]
    public void GetSuccessRatio_UnknownEndpoint_ReturnsFullyHealthy()
    {
        EndpointHealthTracker tracker = new();
        Assert.AreEqual(1.0, tracker.GetSuccessRatio(new Uri("https://a.test")));
    }

    [TestMethod]
    public void GetSuccessRatio_AllSuccesses_ReturnsOne()
    {
        EndpointHealthTracker tracker = new();
        Uri endpoint = new("https://a.test");

        tracker.RecordSuccess(endpoint);
        tracker.RecordSuccess(endpoint);

        Assert.AreEqual(1.0, tracker.GetSuccessRatio(endpoint));
    }

    [TestMethod]
    public void GetSuccessRatio_AllFailures_ReturnsZero()
    {
        EndpointHealthTracker tracker = new();
        Uri endpoint = new("https://a.test");

        tracker.RecordFailure(endpoint);
        tracker.RecordFailure(endpoint);

        Assert.AreEqual(0.0, tracker.GetSuccessRatio(endpoint));
    }

    [TestMethod]
    public void GetSuccessRatio_MixedOutcomes_ReturnsRatio()
    {
        EndpointHealthTracker tracker = new();
        Uri endpoint = new("https://a.test");

        tracker.RecordSuccess(endpoint);
        tracker.RecordSuccess(endpoint);
        tracker.RecordSuccess(endpoint);
        tracker.RecordFailure(endpoint);

        Assert.AreEqual(0.75, tracker.GetSuccessRatio(endpoint));
    }

    #endregion

    #region RecordSuccess / RecordFailure

    [TestMethod]
    public void RecordSuccess_NullEndpoint_Throws()
    {
        EndpointHealthTracker tracker = new();
        Assert.ThrowsExactly<ArgumentNullException>(() => tracker.RecordSuccess(null!));
    }

    [TestMethod]
    public void RecordFailure_NullEndpoint_Throws()
    {
        EndpointHealthTracker tracker = new();
        Assert.ThrowsExactly<ArgumentNullException>(() => tracker.RecordFailure(null!));
    }

    #endregion

    #region SelectHealthiest

    [TestMethod]
    public void SelectHealthiest_NullCandidates_Throws()
    {
        EndpointHealthTracker tracker = new();
        Assert.ThrowsExactly<ArgumentNullException>(() => tracker.SelectHealthiest(null!));
    }

    [TestMethod]
    public void SelectHealthiest_NoCandidates_ReturnsNull()
    {
        EndpointHealthTracker tracker = new();
        Assert.IsNull(tracker.SelectHealthiest([]));
    }

    [TestMethod]
    public void SelectHealthiest_PicksEndpointWithHighestSuccessRatio()
    {
        EndpointHealthTracker tracker = new();
        Uri healthy = new("https://healthy.test");
        Uri unhealthy = new("https://unhealthy.test");

        tracker.RecordSuccess(healthy);
        tracker.RecordSuccess(healthy);
        tracker.RecordFailure(unhealthy);
        tracker.RecordFailure(unhealthy);

        Uri? selected = tracker.SelectHealthiest([unhealthy, healthy]);

        Assert.AreEqual(healthy, selected);
    }

    [TestMethod]
    public void SelectHealthiest_AllUnknown_ReturnsFirstCandidate()
    {
        EndpointHealthTracker tracker = new();
        Uri first = new("https://a.test");
        Uri second = new("https://b.test");

        Uri? selected = tracker.SelectHealthiest([first, second]);

        Assert.AreEqual(first, selected);
    }

    #endregion
}
