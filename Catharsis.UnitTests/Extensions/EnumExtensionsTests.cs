using Catharsis.Extensions;

namespace Catharsis.UnitTests.Extensions;

///<summary>
///Unit tests for the <see cref="EnumExtensions"/> class.
///</summary>
[TestClass]
public class EnumExtensionsTests
{
    #region GetFlags

    [TestMethod]
    public void GetFlags_CombinedValue_ReturnsIndividualFlags()
    {
        TestFlags value = TestFlags.Read | TestFlags.Write;
        List<TestFlags> flags = [.. value.GetFlags()];

        CollectionAssert.AreEquivalent(new[] { TestFlags.Read, TestFlags.Write }, flags);
    }

    [TestMethod]
    public void GetFlags_None_ReturnsEmpty()
    {
        Assert.IsEmpty(TestFlags.None.GetFlags().ToList());
    }

    [TestMethod]
    public void GetFlags_SingleFlag_ReturnsThatFlag()
    {
        List<TestFlags> flags = [.. TestFlags.Read.GetFlags()];
        CollectionAssert.AreEqual(new[] { TestFlags.Read }, flags);
    }

    #endregion

    #region GetDescription

    [TestMethod]
    public void GetDescription_DecoratedValue_ReturnsDescriptionText()
    {
        Assert.AreEqual("First value", TestEnum.First.GetDescription());
    }

    [TestMethod]
    public void GetDescription_UndecoratedValue_ReturnsName()
    {
        Assert.AreEqual("Second", TestEnum.Second.GetDescription());
    }

    [TestMethod]
    public void GetDescription_IsCachedAcrossCalls()
    {
        string first = TestEnum.First.GetDescription();
        string second = TestEnum.First.GetDescription();
        Assert.AreEqual(first, second);
    }

    #endregion

    #region IsDefined

    [TestMethod]
    public void IsDefined_NamedValue_ReturnsTrue()
    {
        Assert.IsTrue(TestEnum.First.IsDefined());
    }

    [TestMethod]
    public void IsDefined_UndefinedValue_ReturnsFalse()
    {
        Assert.IsFalse(((TestEnum)999).IsDefined());
    }

    #endregion

    enum TestEnum
    {
        [System.ComponentModel.Description("First value")]
        First,
        Second,
    }

    [Flags]
    enum TestFlags
    {
        None = 0,
        Read = 1,
        Write = 2,
        Execute = 4,
    }
}
