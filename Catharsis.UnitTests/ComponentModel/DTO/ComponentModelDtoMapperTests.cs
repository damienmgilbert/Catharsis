using System.ComponentModel.DataAnnotations;
using Catharsis.ComponentModel.DTO;

namespace Catharsis.UnitTests.ComponentModel.DTO;

///<summary>
///Unit tests for the <see cref="ComponentModelDtoMapper"/> class.
///</summary>
[TestClass]
public sealed class ComponentModelDtoMapperTests
{
    #region Public methods
    [TestMethod]
    public void Constructor_NullOptions_UsesDefault()
    {
        ComponentModelDtoMapper mapper = new(null);
        SourceDto source = new() { Name = "Alice", Age = 30 };

        // Should work without error using defaults
        TargetDto result = mapper.Map<SourceDto, TargetDto>(source);

        Assert.AreEqual("Alice", result.Name);
    }

    [TestMethod]
    public void Map_CopiesMatchingProperties()
    {
        ComponentModelDtoMapper mapper = new();
        SourceDto source = new() { Name = "Alice", Age = 30 };
        TargetDto target = new();

        TargetDto result = mapper.Map<SourceDto, TargetDto>(source, target);

        Assert.AreSame(target, result);
        Assert.AreEqual("Alice", target.Name);
        Assert.AreEqual(30, target.Age);
    }

    [TestMethod]
    public void Map_CreateNewTarget_CopiesProperties()
    {
        ComponentModelDtoMapper mapper = new();
        SourceDto source = new() { Name = "Alice", Age = 30 };

        TargetDto result = mapper.Map<SourceDto, TargetDto>(source);

        Assert.AreEqual("Alice", result.Name);
        Assert.AreEqual(30, result.Age);
    }

    [TestMethod]
    public void Map_CreateNewTarget_NullSource_ThrowsArgumentNullException()
    {
        ComponentModelDtoMapper mapper = new();

        Assert.ThrowsExactly<ArgumentNullException>(() => mapper.Map<SourceDto, TargetDto>(null!));
    }

    [TestMethod]
    public void Map_MissingProperty_IgnoredByDefault()
    {
        ComponentModelDtoMapper mapper = new();
        SourceDto source = new() { Name = "Alice", Age = 30 };

        PartialTargetDto result = mapper.Map<SourceDto, PartialTargetDto>(source, new PartialTargetDto());

        Assert.AreEqual("Alice", result.Name);
    }

    [TestMethod]
    public void Map_MissingProperty_StrictMode_ThrowsInvalidOperationException()
    {
        ComponentModelDtoOptions options = new() { IgnoreMissingProperties = false };
        ComponentModelDtoMapper mapper = new(options);
        SourceDto source = new() { Name = "Alice", Age = 30 };

        Assert.ThrowsExactly<InvalidOperationException>(() => mapper.Map<SourceDto, PartialTargetDto>(source, new PartialTargetDto()));
    }

    [TestMethod]
    public void Map_NullSource_ThrowsArgumentNullException()
    {
        ComponentModelDtoMapper mapper = new();

        Assert.ThrowsExactly<ArgumentNullException>(() => mapper.Map<SourceDto, TargetDto>(null!, new TargetDto()));
    }

    [TestMethod]
    public void Map_NullTarget_ThrowsArgumentNullException()
    {
        ComponentModelDtoMapper mapper = new();

        Assert.ThrowsExactly<ArgumentNullException>(() => mapper.Map<SourceDto, TargetDto>(new SourceDto(), null!));
    }

    [TestMethod]
    public void Map_WithValidation_InvalidTarget_ThrowsValidationException()
    {
        ComponentModelDtoOptions options = new() { ValidateAfterMap = true };
        ComponentModelDtoMapper mapper = new(options);
        SourceDto source = new() { Name = null, Age = 200 };

        Assert.ThrowsExactly<ValidationException>(() => mapper.Map<SourceDto, ValidatedTargetDto>(source, new ValidatedTargetDto()));
    }

    [TestMethod]
    public void Map_WithValidation_ValidTarget_Succeeds()
    {
        ComponentModelDtoOptions options = new() { ValidateAfterMap = true };
        ComponentModelDtoMapper mapper = new(options);
        SourceDto source = new() { Name = "Alice", Age = 30 };

        ValidatedTargetDto result = mapper.Map<SourceDto, ValidatedTargetDto>(source, new ValidatedTargetDto());

        Assert.AreEqual("Alice", result.Name);
    }

    [TestMethod]
    public void ToBindable_NullSource_ThrowsArgumentNullException()
    {
        ComponentModelDtoMapper mapper = new();

        Assert.ThrowsExactly<ArgumentNullException>(() => ComponentModelDtoMapper.ToBindable<SourceDto>(null!));
    }

    [TestMethod]
    public void ToBindable_WrapsSourceInBindableRecord()
    {
        ComponentModelDtoMapper mapper = new();
        SourceDto source = new() { Name = "Alice", Age = 30 };

        BindableRecord<SourceDto> result = ComponentModelDtoMapper.ToBindable(source);

        Assert.IsInstanceOfType<BindableRecord<SourceDto>>(result);
        Assert.AreSame(source, result.Value);
    }

    [TestMethod]
    public void ToEditable_NullSource_ThrowsArgumentNullException()
    {
        ComponentModelDtoMapper mapper = new();

        Assert.ThrowsExactly<ArgumentNullException>(() => ComponentModelDtoMapper.ToEditable<SourceDto>(null!));
    }

    [TestMethod]
    public void ToEditable_WrapsSourceInEditableRecord()
    {
        ComponentModelDtoMapper mapper = new();
        SourceDto source = new() { Name = "Alice", Age = 30 };

        EditableRecord<SourceDto> result = ComponentModelDtoMapper.ToEditable(source);

        Assert.IsInstanceOfType<EditableRecord<SourceDto>>(result);
        Assert.AreSame(source, result.Value);
    }

    [TestMethod]
    public void ToMetadataAnnotated_NullSource_ThrowsArgumentNullException()
    {
        ComponentModelDtoMapper mapper = new();

        Assert.ThrowsExactly<ArgumentNullException>(() => ComponentModelDtoMapper.ToMetadataAnnotated<SourceDto>(null!));
    }

    [TestMethod]
    public void ToMetadataAnnotated_WrapsSourceInMetadataAnnotatedRecord()
    {
        ComponentModelDtoMapper mapper = new();
        SourceDto source = new() { Name = "Alice", Age = 30 };

        MetadataAnnotatedRecord<SourceDto> result = ComponentModelDtoMapper.ToMetadataAnnotated(source);

        Assert.IsInstanceOfType<MetadataAnnotatedRecord<SourceDto>>(result);
        Assert.AreSame(source, result.Value);
    }

    [TestMethod]
    public void ToValidated_NullSource_ThrowsArgumentNullException()
    {
        ComponentModelDtoMapper mapper = new();

        Assert.ThrowsExactly<ArgumentNullException>(() => ComponentModelDtoMapper.ToValidated<SourceDto>(null!));
    }

    [TestMethod]
    public void ToValidated_WrapsSourceInValidatedRecord()
    {
        ComponentModelDtoMapper mapper = new();
        SourceDto source = new() { Name = "Alice", Age = 30 };

        ValidatedRecord<SourceDto> result = ComponentModelDtoMapper.ToValidated(source);

        Assert.IsInstanceOfType<ValidatedRecord<SourceDto>>(result);
        Assert.AreSame(source, result.Value);
    }
    #endregion

    sealed class SourceDto
    {
        #region Public properties
        public int Age { get; set; }

        public string? Name { get; set; }
        #endregion
    }

    sealed class TargetDto
    {
        #region Public properties
        public int Age { get; set; }

        public string? Name { get; set; }
        #endregion
    }

    sealed class PartialTargetDto
    {
        #region Public properties
        public string? Name { get; set; }
        #endregion
    }

    sealed class ValidatedTargetDto
    {
        #region Public properties
        [Range(0, 150)]
        public int Age { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        public string? Name { get; set; }
        #endregion
    }
}
