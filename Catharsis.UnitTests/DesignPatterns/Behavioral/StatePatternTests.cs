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
        StatePattern sp = new StatePattern();
        List<int> list = new List<int>();

        sp.State(
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
        StatePattern sp = new StatePattern();
        Assert.ThrowsExactly<ArgumentNullException>(() => sp.State(1, "s", null!));
    }

    [TestMethod]
    public void State_WithResult_ReturnsBehaviorResult()
    {
        StatePattern sp = new StatePattern();

        int result = sp.State(
                     10,
                     "double",
                     static state => state switch
        {
            "double" => (Func<int, int>)(static x => x * 2),
            _ => static x => x
        });

        Assert.AreEqual(20, result);
    }
    #endregion
}
