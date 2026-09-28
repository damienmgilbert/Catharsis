using Catharsis.Extensions;

namespace Catharsis.UnitTests.Extensions;

///<summary>
///Unit tests for the <see cref="NullableExtensions"/> class.
///</summary>
[TestClass]
public class NullableExtensionsTests
{
    #region Map

    [TestMethod]
    public void Map_NullMapFunction_Throws()
    {
        int? value = 5;
        Assert.ThrowsExactly<ArgumentNullException>(() => value.Map<int, int>(null!));
    }

    [TestMethod]
    public void Map_HasValue_ProjectsValue()
    {
        int? value = 5;
        int? result = value.Map(static v => v * 2);
        Assert.AreEqual(10, result);
    }

    [TestMethod]
    public void Map_NoValue_ReturnsNull()
    {
        int? value = null;
        int? result = value.Map(static v => v * 2);
        Assert.IsNull(result);
    }

    #endregion

    #region Match

    [TestMethod]
    public void Match_NullSomeFunction_Throws()
    {
        int? value = 5;
        Assert.ThrowsExactly<ArgumentNullException>(() => value.Match<int, int>(null!, static () => 0));
    }

    [TestMethod]
    public void Match_NullNoneFunction_Throws()
    {
        int? value = 5;
        Assert.ThrowsExactly<ArgumentNullException>(() => value.Match<int, int>(static v => v, null!));
    }

    [TestMethod]
    public void Match_HasValue_InvokesSome()
    {
        int? value = 5;
        string result = value.Match(static v => $"has {v}", static () => "none");
        Assert.AreEqual("has 5", result);
    }

    [TestMethod]
    public void Match_NoValue_InvokesNone()
    {
        int? value = null;
        string result = value.Match(static v => $"has {v}", static () => "none");
        Assert.AreEqual("none", result);
    }

    #endregion
}
