using Catharsis.Events;

namespace Catharsis.UnitTests.Events;

///<summary>
///Unit tests for the <see cref="DelegateChain{T}"/> class.
///</summary>
[TestClass]
public class DelegateChainTests
{
    [TestMethod]
    public void Invoke_EmptyChain_ReturnsInput() { Assert.AreEqual(5, new DelegateChain<int>().Invoke(5)); }

    [TestMethod]
    public void Invoke_RunsStepsInOrder()
    {
        DelegateChain<int> chain = new DelegateChain<int>().Then(static x => x + 1).Then(static x => x * 10);
        Assert.AreEqual(60, chain.Invoke(5));
    }

    [TestMethod]
    public void Add_ReturnsNewChainAndLeavesOriginalUnchanged()
    {
        DelegateChain<int> original = new();
        DelegateChain<int> extended = original + (static x => x + 1);

        Assert.AreEqual(0, original.Count);
        Assert.AreEqual(1, extended.Count);
        Assert.AreEqual(5, original.Invoke(5));
    }

    [TestMethod]
    public void PlusChain_ConcatenatesSteps()
    {
        DelegateChain<int> first = new DelegateChain<int>().Then(static x => x + 1);
        DelegateChain<int> second = new DelegateChain<int>().Then(static x => x * 2);

        Assert.AreEqual(12, (first + second).Invoke(5));
        Assert.AreEqual(11, (second + first).Invoke(5));
    }

    [TestMethod]
    public void Then_NullStep_Throws() { Assert.ThrowsExactly<ArgumentNullException>(static () => new DelegateChain<int>().Then(null!)); }

    [TestMethod]
    public void ToDelegate_InvokesChain()
    {
        Func<string, string> func = new DelegateChain<string>().Then(static s => s.Trim()).Then(static s => s.ToUpperInvariant()).ToDelegate();
        Assert.AreEqual("HI", func("  hi "));
    }
}
