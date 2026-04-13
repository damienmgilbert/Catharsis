using System.Globalization;
using Catharsis.ComponentModel.TypeConverter;

namespace Catharsis.UnitTests.ComponentModel.TypeConverter;

///<summary>
///Unit tests for the <see cref="ConverterContext"/> class.
///</summary>
[TestClass]
public sealed class ConverterContextTests
{
    #region Public methods
    [TestMethod]
    public void Constructor_Defaults_AppliedCorrectly()
    {
        // Arrange & Act
        ConverterContext context = new(CultureInfo.InvariantCulture);

        // Assert
        Assert.IsNull(context.Format);
        Assert.IsTrue(context.IgnoreCase);
        Assert.IsTrue(context.AllowLeadingWhiteSpace);
        Assert.IsTrue(context.AllowTrailingWhiteSpace);
    }

    [TestMethod]
    public void Constructor_NullCulture_ThrowsArgumentNullException() { Assert.ThrowsExactly<ArgumentNullException>(static () => new ConverterContext(null!)); }
    [TestMethod]
    public void Constructor_ValidCulture_SetsProperties()
    {
        // Arrange
        CultureInfo culture = CultureInfo.GetCultureInfo("fr-FR");

        // Act
        ConverterContext context = new(culture, "N2", ignoreCase: false, allowLeadingWhiteSpace: false, allowTrailingWhiteSpace: false);

        // Assert
        Assert.AreSame(culture, context.Culture);
        Assert.AreEqual("N2", context.Format);
        Assert.IsFalse(context.IgnoreCase);
        Assert.IsFalse(context.AllowLeadingWhiteSpace);
        Assert.IsFalse(context.AllowTrailingWhiteSpace);
    }

    [TestMethod]
    public void Default_UsesCurrentCulture()
    {
        ConverterContext context = ConverterContext.Default;

        Assert.AreSame(CultureInfo.CurrentCulture, context.Culture);
    }

    [TestMethod]
    public void Invariant_UsesInvariantCulture()
    {
        ConverterContext context = ConverterContext.Invariant;

        Assert.AreSame(CultureInfo.InvariantCulture, context.Culture);
    }

    [TestMethod]
    public void NumberStyles_BothWhiteSpaceAllowed_ReturnsCorrectStyles()
    {
        ConverterContext context = new(CultureInfo.InvariantCulture, allowLeadingWhiteSpace: true, allowTrailingWhiteSpace: true);

        Assert.AreEqual(NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite, context.NumberStyles);
    }

    [TestMethod]
    public void NumberStyles_NoWhiteSpaceAllowed_ReturnsNone()
    {
        ConverterContext context = new(CultureInfo.InvariantCulture, allowLeadingWhiteSpace: false, allowTrailingWhiteSpace: false);

        Assert.AreEqual(NumberStyles.None, context.NumberStyles);
    }

    [TestMethod]
    public void NumberStyles_OnlyLeadingAllowed_ReturnsLeadingOnly()
    {
        ConverterContext context = new(CultureInfo.InvariantCulture, allowLeadingWhiteSpace: true, allowTrailingWhiteSpace: false);

        Assert.AreEqual(NumberStyles.AllowLeadingWhite, context.NumberStyles);
    }

    [TestMethod]
    public void NumberStyles_OnlyTrailingAllowed_ReturnsTrailingOnly()
    {
        ConverterContext context = new(CultureInfo.InvariantCulture, allowLeadingWhiteSpace: false, allowTrailingWhiteSpace: true);

        Assert.AreEqual(NumberStyles.AllowTrailingWhite, context.NumberStyles);
    }

    [TestMethod]
    public void WithCulture_NullCulture_ThrowsArgumentNullException()
    {
        ConverterContext context = ConverterContext.Default;

        Assert.ThrowsExactly<ArgumentNullException>(() => context.WithCulture(null!));
    }

    [TestMethod]
    public void WithCulture_ValidCulture_ReturnsNewContextWithUpdatedCulture()
    {
        // Arrange
        ConverterContext original = new(CultureInfo.InvariantCulture, "N2", ignoreCase: false, allowLeadingWhiteSpace: false, allowTrailingWhiteSpace: false);
        CultureInfo newCulture = CultureInfo.GetCultureInfo("de-DE");

        // Act
        ConverterContext result = original.WithCulture(newCulture);

        // Assert
        Assert.AreSame(newCulture, result.Culture);
        Assert.AreEqual("N2", result.Format);
        Assert.IsFalse(result.IgnoreCase);
        Assert.IsFalse(result.AllowLeadingWhiteSpace);
        Assert.IsFalse(result.AllowTrailingWhiteSpace);
    }

    [TestMethod]
    public void WithFormat_Null_SetsFormatToNull()
    {
        ConverterContext original = new(CultureInfo.InvariantCulture, "N2");

        ConverterContext result = original.WithFormat(null);

        Assert.IsNull(result.Format);
    }

    [TestMethod]
    public void WithFormat_ReturnsNewContextWithUpdatedFormat()
    {
        // Arrange
        ConverterContext original = new(CultureInfo.InvariantCulture, "N2");

        // Act
        ConverterContext result = original.WithFormat("G");

        // Assert
        Assert.AreEqual("G", result.Format);
        Assert.AreSame(original.Culture, result.Culture);
    }
    #endregion
}
