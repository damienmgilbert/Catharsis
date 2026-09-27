using Catharsis.DesignPatterns.Enterprise;

namespace Catharsis.UnitTests.DesignPatterns.Enterprise;

///<summary>
///Unit tests for the <see cref="EventSourcingPattern"/> class.
///</summary>
[TestClass]
public class EventSourcingPatternTests
{
    #region Public methods

    [TestMethod]
    public void Replay_NullEvents_Throws() { Assert.ThrowsExactly<ArgumentNullException>(static () => EventSourcingPattern.Replay(0, (IEnumerable<int>)null!, static (state, _) => state)); }

    [TestMethod]
    public void Replay_NullApply_Throws() { Assert.ThrowsExactly<ArgumentNullException>(static () => EventSourcingPattern.Replay(0, new[] { 1, 2 }, (Func<int, int, int>)null!)); }

    [TestMethod]
    public void Replay_NoEvents_ReturnsInitialState()
    {
        int result = EventSourcingPattern.Replay(10, Array.Empty<int>(), static (state, delta) => state + delta);
        Assert.AreEqual(10, result);
    }

    [TestMethod]
    public void Replay_MultipleEvents_FoldsInOrder()
    {
        int result = EventSourcingPattern.Replay(0, new[] { 1, 2, 3 }, static (state, delta) => state + delta);
        Assert.AreEqual(6, result);
    }

    [TestMethod]
    public void Replay_OrderMatters_AppliesSequentially()
    {
        List<string> result = EventSourcingPattern.Replay(new List<string>(), new[] { "a", "b", "c" }, static (List<string> state, string @event) => [.. state, @event]);
        CollectionAssert.AreEqual(new[] { "a", "b", "c" }, result);
    }

    #endregion
}
