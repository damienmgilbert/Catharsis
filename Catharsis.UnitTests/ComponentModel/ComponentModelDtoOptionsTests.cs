using Catharsis.ComponentModel.DTO;

namespace Catharsis.UnitTests.ComponentModel;

///<summary>
///Unit tests for the <see cref="ComponentModelDtoOptions"/> class.
///</summary>
[TestClass]
public class ComponentModelDtoOptionsTests
{
    #region Public methods
    [TestMethod]
    public void Default_HasExpectedValues()
    {
        ComponentModelDtoOptions options = ComponentModelDtoOptions.Default;
        Assert.IsTrue(options.RaisePropertyChangedOnMap);
        Assert.IsFalse(options.ValidateAfterMap);
        Assert.IsTrue(options.PreserveMetadata);
        Assert.IsTrue(options.IgnoreMissingProperties);
        Assert.AreEqual(StringComparison.Ordinal, options.PropertyNameComparison);
    }

    [TestMethod]
    public void Properties_CanBeModified()
    {
        ComponentModelDtoOptions options = new() { RaisePropertyChangedOnMap = false, ValidateAfterMap = true, PreserveMetadata = false, IgnoreMissingProperties = false, PropertyNameComparison = StringComparison.OrdinalIgnoreCase };

        Assert.IsFalse(options.RaisePropertyChangedOnMap);
        Assert.IsTrue(options.ValidateAfterMap);
        Assert.IsFalse(options.PreserveMetadata);
        Assert.IsFalse(options.IgnoreMissingProperties);
        Assert.AreEqual(StringComparison.OrdinalIgnoreCase, options.PropertyNameComparison);
    }
    #endregion
}
