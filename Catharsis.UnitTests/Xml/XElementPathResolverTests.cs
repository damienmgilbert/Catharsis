using Catharsis.Xml;
using System.Xml.Linq;

namespace Catharsis.UnitTests.Xml;

///<summary>
///Unit tests for the <see cref="XElementPathResolver"/> class.
///</summary>
[TestClass]
public class XElementPathResolverTests
{
    #region GetValue

    [TestMethod]
    public void GetValue_NullRoot_Throws() { Assert.ThrowsExactly<ArgumentNullException>(static () => XElementPathResolver.GetValue(null!, "Name")); }

    [TestMethod]
    public void GetValue_NullPath_Throws() { Assert.ThrowsExactly<ArgumentNullException>(static () => XElementPathResolver.GetValue(new XElement("Order"), null!)); }

    [TestMethod]
    public void GetValue_WhitespacePath_Throws() { Assert.ThrowsExactly<ArgumentException>(static () => XElementPathResolver.GetValue(new XElement("Order"), " ")); }

    [TestMethod]
    public void GetValue_SingleElementSegment_ReturnsElementText()
    {
        XElement order = XElement.Parse("<Order><Total>42</Total></Order>");
        Assert.AreEqual("42", XElementPathResolver.GetValue(order, "Total"));
    }

    [TestMethod]
    public void GetValue_NestedElementSegments_ReturnsNestedText()
    {
        XElement order = XElement.Parse("<Order><Customer><Name>Alice</Name></Customer></Order>");
        Assert.AreEqual("Alice", XElementPathResolver.GetValue(order, "Customer/Name"));
    }

    [TestMethod]
    public void GetValue_TrailingAttributeSegment_ReturnsAttributeValue()
    {
        XElement order = XElement.Parse("<Order><Customer id=\"123\" /></Order>");
        Assert.AreEqual("123", XElementPathResolver.GetValue(order, "Customer/@id"));
    }

    [TestMethod]
    public void GetValue_AttributeSegmentNotLast_Throws()
    {
        Assert.ThrowsExactly<ArgumentException>(() => XElementPathResolver.GetValue(new XElement("Order"), "@id/Name"));
    }

    [TestMethod]
    public void GetValue_MissingIntermediateElement_ReturnsNull()
    {
        XElement order = XElement.Parse("<Order></Order>");
        Assert.IsNull(XElementPathResolver.GetValue(order, "Customer/Name"));
    }

    [TestMethod]
    public void GetValue_MissingAttribute_ReturnsNull()
    {
        XElement order = XElement.Parse("<Order><Customer /></Order>");
        Assert.IsNull(XElementPathResolver.GetValue(order, "Customer/@id"));
    }

    #endregion

    #region SetValue

    [TestMethod]
    public void SetValue_NullRoot_Throws() { Assert.ThrowsExactly<ArgumentNullException>(static () => XElementPathResolver.SetValue(null!, "Name", "x")); }

    [TestMethod]
    public void SetValue_SingleElementSegment_SetsElementText()
    {
        XElement order = new("Order");
        XElementPathResolver.SetValue(order, "Total", "42");

        Assert.AreEqual("42", order.Element("Total")!.Value);
    }

    [TestMethod]
    public void SetValue_ExistingElement_OverwritesText()
    {
        XElement order = XElement.Parse("<Order><Total>10</Total></Order>");
        XElementPathResolver.SetValue(order, "Total", "42");

        Assert.AreEqual("42", order.Element("Total")!.Value);
    }

    [TestMethod]
    public void SetValue_MissingIntermediateElements_CreatesThem()
    {
        XElement order = new("Order");
        XElementPathResolver.SetValue(order, "Customer/Name", "Alice");

        Assert.AreEqual("Alice", order.Element("Customer")!.Element("Name")!.Value);
    }

    [TestMethod]
    public void SetValue_TrailingAttributeSegment_SetsAttribute()
    {
        XElement order = XElement.Parse("<Order><Customer /></Order>");
        XElementPathResolver.SetValue(order, "Customer/@id", "123");

        Assert.AreEqual("123", order.Element("Customer")!.Attribute("id")!.Value);
    }

    [TestMethod]
    public void SetValue_AttributeSegmentNotLast_Throws()
    {
        Assert.ThrowsExactly<ArgumentException>(() => XElementPathResolver.SetValue(new XElement("Order"), "@id/Name", "x"));
    }

    #endregion
}
