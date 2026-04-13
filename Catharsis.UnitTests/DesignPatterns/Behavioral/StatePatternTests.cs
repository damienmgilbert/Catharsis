using Catharsis.DesignPatterns.Behavioral;

namespace Catharsis.UnitTests.DesignPatterns.Behavioral;

///<summary>
///Unit tests for the <see cref="StatePattern"/> class.
///</summary>
[TestClass]
public class StatePatternTests
{
    #region Public methods
    [TestMethod]
    public void State_AppliesBehaviorForState()
    {
        List<int> list = [];

        StatePattern.State(
        list,
        "add",
        static state => state switch
        {
            "add" => static l => l.Add(42),
            _ => static _ =>
            {
            }
        });

        Assert.HasCount(1, list);
        Assert.AreEqual(42, list[0]);
    }

    [TestMethod]
    public void State_NullBehaviorSelector_Throws()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => StatePattern.State(1, "s", (Func<string, Action<int>>)null!));
    }

    [TestMethod]
    public void State_WithResult_ReturnsBehaviorResult()
    {
        int result = StatePattern.State<int, string, int>(
                     10,
                     "double",
                     static state => state switch
        {
            "double" => static x => x * 2,
            _ => static x => x
        });

        Assert.AreEqual(20, result);
    }
    #endregion
}
