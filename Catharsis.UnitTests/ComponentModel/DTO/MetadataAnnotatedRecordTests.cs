using System.ComponentModel.DataAnnotations;
using Catharsis.ComponentModel.DTO;

namespace Catharsis.UnitTests.ComponentModel.DTO;

///<summary>
///Unit tests for the <see cref="MetadataAnnotatedRecord"/> class.
///</summary>
[TestClass]
public sealed class MetadataAnnotatedRecordTests
{
    #region Public methods
    [TestMethod]
    public void AllPropertyMetadata_ReturnsEntriesForAllProperties()
    {
        MetadataAnnotatedRecord<AnnotatedDto> record = new MetadataAnnotatedRecord<AnnotatedDto>(new AnnotatedDto { Name = "Alice", Age = 30 });

        Assert.IsGreaterThanOrEqualTo(3, record.AllPropertyMetadata.Count);
    }

    [TestMethod]
    public void Constructor_NullValue_ThrowsArgumentNullException() { Assert.ThrowsExactly<ArgumentNullException>(static () => new MetadataAnnotatedRecord<AnnotatedDto>(null!)); }
    [TestMethod]
    public void GetPropertyMetadata_Attributes_ContainsAppliedAttributes()
    {
        MetadataAnnotatedRecord<AnnotatedDto> record = new MetadataAnnotatedRecord<AnnotatedDto>(new AnnotatedDto { Name = "Alice", Age = 30 });

        PropertyMetadataEntry? nameMeta = record.GetPropertyMetadata("Name");

        Assert.IsNotNull(nameMeta);
        Assert.IsTrue(nameMeta.Attributes.Any(static a => a is RequiredAttribute));
    }

    [TestMethod]
    public void GetPropertyMetadata_ExistingProperty_ReturnsEntry()
    {
        MetadataAnnotatedRecord<AnnotatedDto> record = new MetadataAnnotatedRecord<AnnotatedDto>(new AnnotatedDto { Name = "Alice", Age = 30 });

        PropertyMetadataEntry? meta = record.GetPropertyMetadata("Name");

        Assert.IsNotNull(meta);
        Assert.AreEqual("Name", meta.PropertyName);
        Assert.AreEqual("Full Name", meta.DisplayName);
        Assert.AreEqual("The person's full name", meta.Description);
        Assert.IsTrue(meta.IsRequired);
    }

    [TestMethod]
    public void GetPropertyMetadata_NonExistentProperty_ReturnsNull()
    {
        MetadataAnnotatedRecord<AnnotatedDto> record = new MetadataAnnotatedRecord<AnnotatedDto>(new AnnotatedDto { Name = "Alice", Age = 30 });

        PropertyMetadataEntry? meta = record.GetPropertyMetadata("NonExistent");

        Assert.IsNull(meta);
    }

    [TestMethod]
    public void GetPropertyMetadata_NullPropertyName_ThrowsArgumentNullException()
    {
        MetadataAnnotatedRecord<AnnotatedDto> record = new MetadataAnnotatedRecord<AnnotatedDto>(new AnnotatedDto { Name = "Alice", Age = 30 });

        Assert.ThrowsExactly<ArgumentNullException>(() => record.GetPropertyMetadata(null!));
    }

    [TestMethod]
    public void GetPropertyMetadata_PropertyType_IsCorrect()
    {
        MetadataAnnotatedRecord<AnnotatedDto> record = new MetadataAnnotatedRecord<AnnotatedDto>(new AnnotatedDto { Name = "Alice", Age = 30 });

        PropertyMetadataEntry? ageMeta = record.GetPropertyMetadata("Age");

        Assert.IsNotNull(ageMeta);
        Assert.AreEqual(typeof(int), ageMeta.PropertyType);
    }

    [TestMethod]
    public void GetPropertyMetadata_PropertyWithoutDisplayAttr_UsesPropertyName()
    {
        MetadataAnnotatedRecord<AnnotatedDto> record = new MetadataAnnotatedRecord<AnnotatedDto>(new AnnotatedDto { Name = "Alice", Age = 30, Nickname = "Al" });

        PropertyMetadataEntry? meta = record.GetPropertyMetadata("Nickname");

        Assert.IsNotNull(meta);
        Assert.AreEqual("Nickname", meta.DisplayName);
        Assert.IsFalse(meta.IsRequired);
    }

    [TestMethod]
    public void Value_InheritsBindableRecordBehavior()
    {
        AnnotatedDto initial = new AnnotatedDto { Name = "Alice", Age = 30 };
        MetadataAnnotatedRecord<AnnotatedDto> record = new MetadataAnnotatedRecord<AnnotatedDto>(initial);
        string? changedProp = null;
        record.PropertyChanged += (s, e) => changedProp = e.PropertyName;

        record.Value = new AnnotatedDto { Name = "Bob", Age = 25 };

        Assert.AreEqual("Value", changedProp);
    }
    #endregion

    sealed class AnnotatedDto
    {
        #region Public properties
        [Display(Name = "Age")]
        [Range(0, 150)]
        public int Age { get; set; }

        [Display(Name = "Full Name", Description = "The person's full name")]
        [Required]
        public string? Name { get; set; }

        public string? Nickname { get; set; }
        #endregion
    }
}
