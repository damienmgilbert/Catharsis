using System.ComponentModel.DataAnnotations;

using Catharsis.ComponentModel.DTO;

namespace Catharsis.UnitTests.ComponentModel.DTO;

[TestClass]
public sealed class MetadataAnnotatedRecordTests
{
    private sealed class AnnotatedDto
    {
        [Display(Name = "Full Name", Description = "The person's full name")]
        [Required]
        public string? Name { get; set; }

        [Display(Name = "Age")]
        [Range(0, 150)]
        public int Age { get; set; }

        public string? Nickname { get; set; }
    }

    [TestMethod]
    public void Constructor_NullValue_ThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(
            () => new MetadataAnnotatedRecord<AnnotatedDto>(null!));
    }

    [TestMethod]
    public void AllPropertyMetadata_ReturnsEntriesForAllProperties()
    {
        var record = new MetadataAnnotatedRecord<AnnotatedDto>(
            new AnnotatedDto { Name = "Alice", Age = 30 });

        Assert.IsTrue(record.AllPropertyMetadata.Count >= 3);
    }

    [TestMethod]
    public void GetPropertyMetadata_ExistingProperty_ReturnsEntry()
    {
        var record = new MetadataAnnotatedRecord<AnnotatedDto>(
            new AnnotatedDto { Name = "Alice", Age = 30 });

        var meta = record.GetPropertyMetadata("Name");

        Assert.IsNotNull(meta);
        Assert.AreEqual("Name", meta.PropertyName);
        Assert.AreEqual("Full Name", meta.DisplayName);
        Assert.AreEqual("The person's full name", meta.Description);
        Assert.IsTrue(meta.IsRequired);
    }

    [TestMethod]
    public void GetPropertyMetadata_NonExistentProperty_ReturnsNull()
    {
        var record = new MetadataAnnotatedRecord<AnnotatedDto>(
            new AnnotatedDto { Name = "Alice", Age = 30 });

        var meta = record.GetPropertyMetadata("NonExistent");

        Assert.IsNull(meta);
    }

    [TestMethod]
    public void GetPropertyMetadata_NullPropertyName_ThrowsArgumentNullException()
    {
        var record = new MetadataAnnotatedRecord<AnnotatedDto>(
            new AnnotatedDto { Name = "Alice", Age = 30 });

        Assert.ThrowsExactly<ArgumentNullException>(
            () => record.GetPropertyMetadata(null!));
    }

    [TestMethod]
    public void GetPropertyMetadata_PropertyWithoutDisplayAttr_UsesPropertyName()
    {
        var record = new MetadataAnnotatedRecord<AnnotatedDto>(
            new AnnotatedDto { Name = "Alice", Age = 30, Nickname = "Al" });

        var meta = record.GetPropertyMetadata("Nickname");

        Assert.IsNotNull(meta);
        Assert.AreEqual("Nickname", meta.DisplayName);
        Assert.IsFalse(meta.IsRequired);
    }

    [TestMethod]
    public void GetPropertyMetadata_PropertyType_IsCorrect()
    {
        var record = new MetadataAnnotatedRecord<AnnotatedDto>(
            new AnnotatedDto { Name = "Alice", Age = 30 });

        var ageMeta = record.GetPropertyMetadata("Age");

        Assert.IsNotNull(ageMeta);
        Assert.AreEqual(typeof(int), ageMeta.PropertyType);
    }

    [TestMethod]
    public void GetPropertyMetadata_Attributes_ContainsAppliedAttributes()
    {
        var record = new MetadataAnnotatedRecord<AnnotatedDto>(
            new AnnotatedDto { Name = "Alice", Age = 30 });

        var nameMeta = record.GetPropertyMetadata("Name");

        Assert.IsNotNull(nameMeta);
        Assert.IsTrue(nameMeta.Attributes.Any(a => a is RequiredAttribute));
    }

    [TestMethod]
    public void Value_InheritsBindableRecordBehavior()
    {
        var initial = new AnnotatedDto { Name = "Alice", Age = 30 };
        var record = new MetadataAnnotatedRecord<AnnotatedDto>(initial);
        string? changedProp = null;
        record.PropertyChanged += (s, e) => changedProp = e.PropertyName;

        record.Value = new AnnotatedDto { Name = "Bob", Age = 25 };

        Assert.AreEqual("Value", changedProp);
    }
}
