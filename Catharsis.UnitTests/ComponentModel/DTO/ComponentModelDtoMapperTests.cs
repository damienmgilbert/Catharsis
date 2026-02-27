using Catharsis.ComponentModel.DTO;
using System.ComponentModel.DataAnnotations;

namespace Catharsis.UnitTests.ComponentModel.DTO;

[TestClass]
public sealed class ComponentModelDtoMapperTests
{
    private sealed class SourceDto
    {
        public string? Name { get; set; }
        public int Age { get; set; }
    }

    private sealed class TargetDto
    {
        public string? Name { get; set; }
        public int Age { get; set; }
    }

    private sealed class PartialTargetDto
    {
        public string? Name { get; set; }
    }

    private sealed class ValidatedTargetDto
    {
        [Required(ErrorMessage = "Name is required.")]
        public string? Name { get; set; }

        [Range(0, 150)]
        public int Age { get; set; }
    }

    [TestMethod]
    public void Map_CopiesMatchingProperties()
    {
        var mapper = new ComponentModelDtoMapper();
        var source = new SourceDto { Name = "Alice", Age = 30 };
        var target = new TargetDto();

        var result = mapper.Map<SourceDto, TargetDto>(source, target);

        Assert.AreSame(target, result);
        Assert.AreEqual("Alice", target.Name);
        Assert.AreEqual(30, target.Age);
    }

    [TestMethod]
    public void Map_NullSource_ThrowsArgumentNullException()
    {
        var mapper = new ComponentModelDtoMapper();

        Assert.ThrowsExactly<ArgumentNullException>(
            () => mapper.Map<SourceDto, TargetDto>(null!, new TargetDto()));
    }

    [TestMethod]
    public void Map_NullTarget_ThrowsArgumentNullException()
    {
        var mapper = new ComponentModelDtoMapper();

        Assert.ThrowsExactly<ArgumentNullException>(
            () => mapper.Map<SourceDto, TargetDto>(new SourceDto(), null!));
    }

    [TestMethod]
    public void Map_MissingProperty_IgnoredByDefault()
    {
        var mapper = new ComponentModelDtoMapper();
        var source = new SourceDto { Name = "Alice", Age = 30 };

        var result = mapper.Map<SourceDto, PartialTargetDto>(source, new PartialTargetDto());

        Assert.AreEqual("Alice", result.Name);
    }

    [TestMethod]
    public void Map_MissingProperty_StrictMode_ThrowsInvalidOperationException()
    {
        var options = new ComponentModelDtoOptions { IgnoreMissingProperties = false };
        var mapper = new ComponentModelDtoMapper(options);
        var source = new SourceDto { Name = "Alice", Age = 30 };

        Assert.ThrowsExactly<InvalidOperationException>(
            () => mapper.Map<SourceDto, PartialTargetDto>(source, new PartialTargetDto()));
    }

    [TestMethod]
    public void Map_WithValidation_ValidTarget_Succeeds()
    {
        var options = new ComponentModelDtoOptions { ValidateAfterMap = true };
        var mapper = new ComponentModelDtoMapper(options);
        var source = new SourceDto { Name = "Alice", Age = 30 };

        var result = mapper.Map<SourceDto, ValidatedTargetDto>(source, new ValidatedTargetDto());

        Assert.AreEqual("Alice", result.Name);
    }

    [TestMethod]
    public void Map_WithValidation_InvalidTarget_ThrowsValidationException()
    {
        var options = new ComponentModelDtoOptions { ValidateAfterMap = true };
        var mapper = new ComponentModelDtoMapper(options);
        var source = new SourceDto { Name = null, Age = 200 };

        Assert.ThrowsExactly<ValidationException>(
            () => mapper.Map<SourceDto, ValidatedTargetDto>(source, new ValidatedTargetDto()));
    }

    [TestMethod]
    public void Map_CreateNewTarget_CopiesProperties()
    {
        var mapper = new ComponentModelDtoMapper();
        var source = new SourceDto { Name = "Alice", Age = 30 };

        var result = mapper.Map<SourceDto, TargetDto>(source);

        Assert.AreEqual("Alice", result.Name);
        Assert.AreEqual(30, result.Age);
    }

    [TestMethod]
    public void Map_CreateNewTarget_NullSource_ThrowsArgumentNullException()
    {
        var mapper = new ComponentModelDtoMapper();

        Assert.ThrowsExactly<ArgumentNullException>(
            () => mapper.Map<SourceDto, TargetDto>(null!));
    }

    [TestMethod]
    public void ToBindable_NullSource_ThrowsArgumentNullException()
    {
        var mapper = new ComponentModelDtoMapper();

        Assert.ThrowsExactly<ArgumentNullException>(
            () => mapper.ToBindable<SourceDto>(null!));
    }

    [TestMethod]
    public void ToBindable_WrapsSourceInBindableRecord()
    {
        var mapper = new ComponentModelDtoMapper();
        var source = new SourceDto { Name = "Alice", Age = 30 };

        var result = mapper.ToBindable(source);

        Assert.IsInstanceOfType<BindableRecord<SourceDto>>(result);
        Assert.AreSame(source, result.Value);
    }

    [TestMethod]
    public void ToValidated_NullSource_ThrowsArgumentNullException()
    {
        var mapper = new ComponentModelDtoMapper();

        Assert.ThrowsExactly<ArgumentNullException>(
            () => mapper.ToValidated<SourceDto>(null!));
    }

    [TestMethod]
    public void ToValidated_WrapsSourceInValidatedRecord()
    {
        var mapper = new ComponentModelDtoMapper();
        var source = new SourceDto { Name = "Alice", Age = 30 };

        var result = mapper.ToValidated(source);

        Assert.IsInstanceOfType<ValidatedRecord<SourceDto>>(result);
        Assert.AreSame(source, result.Value);
    }

    [TestMethod]
    public void ToEditable_NullSource_ThrowsArgumentNullException()
    {
        var mapper = new ComponentModelDtoMapper();

        Assert.ThrowsExactly<ArgumentNullException>(
            () => mapper.ToEditable<SourceDto>(null!));
    }

    [TestMethod]
    public void ToEditable_WrapsSourceInEditableRecord()
    {
        var mapper = new ComponentModelDtoMapper();
        var source = new SourceDto { Name = "Alice", Age = 30 };

        var result = mapper.ToEditable(source);

        Assert.IsInstanceOfType<EditableRecord<SourceDto>>(result);
        Assert.AreSame(source, result.Value);
    }

    [TestMethod]
    public void ToMetadataAnnotated_NullSource_ThrowsArgumentNullException()
    {
        var mapper = new ComponentModelDtoMapper();

        Assert.ThrowsExactly<ArgumentNullException>(
            () => mapper.ToMetadataAnnotated<SourceDto>(null!));
    }

    [TestMethod]
    public void ToMetadataAnnotated_WrapsSourceInMetadataAnnotatedRecord()
    {
        var mapper = new ComponentModelDtoMapper();
        var source = new SourceDto { Name = "Alice", Age = 30 };

        var result = mapper.ToMetadataAnnotated(source);

        Assert.IsInstanceOfType<MetadataAnnotatedRecord<SourceDto>>(result);
        Assert.AreSame(source, result.Value);
    }

    [TestMethod]
    public void Constructor_NullOptions_UsesDefault()
    {
        var mapper = new ComponentModelDtoMapper(null);
        var source = new SourceDto { Name = "Alice", Age = 30 };

        // Should work without error using defaults
        var result = mapper.Map<SourceDto, TargetDto>(source);

        Assert.AreEqual("Alice", result.Name);
    }
}
