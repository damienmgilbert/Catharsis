using Catharsis.ComponentModel;
using System.ComponentModel;

namespace Catharsis.UnitTests.ComponentModel;

[TestClass]
public class AttributeCollectionBuilderTests
{
    #region Public methods
    [TestMethod]
    public void Add_IncreasesCount()
    {
        AttributeCollectionBuilder builder = new AttributeCollectionBuilder();
        builder.Add(new System.ComponentModel.DescriptionAttribute("test"));
        Assert.AreEqual(1, builder.Count);
    }

    [TestMethod]
    public void Add_SameType_Replaces()
    {
        AttributeCollectionBuilder builder = new AttributeCollectionBuilder();
        builder.Add(new System.ComponentModel.DescriptionAttribute("first"));
        builder.Add(new System.ComponentModel.DescriptionAttribute("second"));
        Assert.AreEqual(1, builder.Count);
    }

    [TestMethod]
    public void AddRange_AddsMultiple()
    {
        AttributeCollectionBuilder builder = new AttributeCollectionBuilder();
        builder.AddRange([new System.ComponentModel.DescriptionAttribute("desc"), new CategoryAttribute("cat")]);
        Assert.AreEqual(2, builder.Count);
    }

    [TestMethod]
    public void Build_ReturnsAttributeCollection()
    {
        AttributeCollectionBuilder builder = new AttributeCollectionBuilder();
        builder.Add(new System.ComponentModel.DescriptionAttribute("hello"));
        AttributeCollection collection = builder.Build();
        Assert.IsNotNull(collection);
        Assert.IsNotEmpty(collection);
    }

    [TestMethod]
    public void FluentChaining_Works()
    {
        AttributeCollectionBuilder builder = new AttributeCollectionBuilder()
            .Add(new System.ComponentModel.DescriptionAttribute("a"))
            .Add(new CategoryAttribute("b"));
        Assert.AreEqual(2, builder.Count);
    }

    [TestMethod]
    public void Remove_ByType_RemovesAttribute()
    {
        AttributeCollectionBuilder builder = new AttributeCollectionBuilder();
        builder.Add(new System.ComponentModel.DescriptionAttribute("test"));
        builder.Remove<System.ComponentModel.DescriptionAttribute>();
        Assert.AreEqual(0, builder.Count);
    }
    #endregion
}
