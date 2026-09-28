using Catharsis.ComponentModel.Observability;
using Catharsis.ComponentModel.Validation;
using Catharsis.Dynamic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Catharsis.UnitTests.Dynamic;

///<summary>
///Unit tests for the <see cref="FlexibleEntity"/> class.
///</summary>
[TestClass]
public class FlexibleEntityTests
{
    #region Helpers

    sealed class PassingRule : IValidationRule
    {
        public ValidationResult? Validate(object? value, ValidationContext context) { return ValidationResult.Success; }
        public ValidationScope Scope => ValidationScope.Property;
        public ValidationSeverity Severity => ValidationSeverity.Error;
    }

    sealed class RejectAboveThresholdRule(int threshold) : IValidationRule
    {
        public ValidationResult? Validate(object? value, ValidationContext context) { return ((value is int number) && (number > threshold)) ? new ValidationResult($"Value must not exceed {threshold}.") : ValidationResult.Success; }
        public ValidationScope Scope => ValidationScope.Property;
        public ValidationSeverity Severity => ValidationSeverity.Error;
    }

    #endregion

    #region Get / Set

    [TestMethod]
    public void Set_ThenGet_ReturnsStoredValue()
    {
        FlexibleEntity entity = new();

        entity.Set("Name", "Ada");

        Assert.AreEqual("Ada", entity.Get<string>("Name"));
    }

    [TestMethod]
    public void Get_MissingProperty_ReturnsDefault()
    {
        FlexibleEntity entity = new();
        Assert.IsNull(entity.Get<string>("Missing"));
    }

    [TestMethod]
    public void Set_NestedPath_AutoCreatesIntermediateEntity()
    {
        FlexibleEntity entity = new();

        entity.Set("Address.City", "Seattle");

        Assert.AreEqual("Seattle", entity.Get<string>("Address.City"));
        Assert.IsInstanceOfType<FlexibleEntity>(entity.Get<object?>("Address"));
    }

    [TestMethod]
    public void Set_NestedPathThroughNonEntityValue_Throws()
    {
        FlexibleEntity entity = new();
        entity.Set("Address", "not an entity");

        Assert.ThrowsExactly<InvalidOperationException>(() => entity.Set("Address.City", "Seattle"));
    }

    [TestMethod]
    public void Indexer_GetAndSet_RoundTrips()
    {
        FlexibleEntity entity = new()
        {
            ["Name"] = "Grace"
        };

        Assert.AreEqual("Grace", entity["Name"]);
    }

    [TestMethod]
    public void PropertyNames_ReflectsSetProperties()
    {
        FlexibleEntity entity = new();
        entity.Set("Name", "Ada");
        entity.Set("Age", 30);

        CollectionAssert.AreEquivalent(new[] { "Name", "Age" }, entity.PropertyNames.ToList());
    }

    #endregion

    #region Validation

    [TestMethod]
    public void Set_PassingValidationRule_CommitsValueAndReturnsTrue()
    {
        FlexibleEntity entity = new();
        entity.AddValidationRule("Age", new PassingRule());

        bool committed = entity.Set("Age", 30);

        Assert.IsTrue(committed);
        Assert.AreEqual(30, entity.Get<int>("Age"));
        Assert.IsFalse(entity.Errors.HasErrors);
    }

    [TestMethod]
    public void Set_FailingValidationRule_RejectsValueAndRecordsError()
    {
        FlexibleEntity entity = new();
        entity.AddValidationRule("Age", new RejectAboveThresholdRule(120));

        bool committed = entity.Set("Age", 200);

        Assert.IsFalse(committed);
        Assert.IsNull(entity.Get<object?>("Age"));
        Assert.IsTrue(entity.Errors.HasErrors);
        Assert.HasCount(1, entity.Errors.GetErrorInfos("Age"));
    }

    [TestMethod]
    public void Indexer_FailingValidationRule_ThrowsValidationException()
    {
        FlexibleEntity entity = new();
        entity.AddValidationRule("Age", new RejectAboveThresholdRule(120));

        Assert.ThrowsExactly<ValidationException>(() => entity["Age"] = 200);
    }

    [TestMethod]
    public void RemoveValidationRules_ExistingProperty_ReturnsTrueAndClearsRules()
    {
        FlexibleEntity entity = new();
        entity.AddValidationRule("Age", new RejectAboveThresholdRule(120));

        Assert.IsTrue(entity.RemoveValidationRules("Age"));
        Assert.IsTrue(entity.Set("Age", 200));
    }

    [TestMethod]
    public void AddValidationRule_NullRule_Throws()
    {
        FlexibleEntity entity = new();
        Assert.ThrowsExactly<ArgumentNullException>(() => entity.AddValidationRule("Age", null!));
    }

    #endregion

    #region Change tracking

    [TestMethod]
    public void Set_RecordsChange()
    {
        FlexibleEntity entity = new();
        entity.Set("Name", "Ada");

        Assert.IsTrue(entity.IsChanged);
        Assert.IsTrue(entity.CanUndo);
    }

    [TestMethod]
    public void Undo_RevertsLastChange()
    {
        FlexibleEntity entity = new();
        entity.Set("Name", "Ada");
        entity.Set("Name", "Grace");

        ChangeEntry? undone = entity.Undo();

        Assert.IsNotNull(undone);
        Assert.AreEqual("Ada", entity.Get<string>("Name"));
        Assert.IsTrue(entity.CanRedo);
    }

    [TestMethod]
    public void Redo_ReappliesUndoneChange()
    {
        FlexibleEntity entity = new();
        entity.Set("Name", "Ada");
        entity.Set("Name", "Grace");
        entity.Undo();

        ChangeEntry? redone = entity.Redo();

        Assert.IsNotNull(redone);
        Assert.AreEqual("Grace", entity.Get<string>("Name"));
    }

    [TestMethod]
    public void AcceptChanges_ClearsUndoHistory()
    {
        FlexibleEntity entity = new();
        entity.Set("Name", "Ada");

        entity.AcceptChanges();

        Assert.IsFalse(entity.CanUndo);
        Assert.IsFalse(entity.IsChanged);
    }

    [TestMethod]
    public void RejectChanges_RevertsAllPendingChanges()
    {
        FlexibleEntity entity = new();
        entity.Set("Name", "Ada");
        entity.Set("Age", 30);

        entity.RejectChanges();

        Assert.IsNull(entity.Get<string>("Name"));
        Assert.IsFalse(entity.CanUndo);
    }

    #endregion

    #region Snapshot / restore

    [TestMethod]
    public void CreateSnapshot_ThenRestoreAfterFurtherChanges_RestoresOriginalValues()
    {
        FlexibleEntity entity = new();
        entity.Set("Name", "Ada");
        IReadOnlyDictionary<string, object?> snapshot = entity.CreateSnapshot();

        entity.Set("Name", "Grace");
        Assert.AreEqual("Grace", entity.Get<string>("Name"));

        entity.RestoreSnapshot(snapshot);

        Assert.AreEqual("Ada", entity.Get<string>("Name"));
    }

    [TestMethod]
    public void RestoreSnapshot_NullSnapshot_Throws()
    {
        FlexibleEntity entity = new();
        Assert.ThrowsExactly<ArgumentNullException>(() => entity.RestoreSnapshot(null!));
    }

    #endregion

    #region Type descriptor

    [TestMethod]
    public void AsTypeDescriptor_ReturnsOnePropertyPerCurrentKey()
    {
        FlexibleEntity entity = new();
        entity.Set("Name", "Ada");
        entity.Set("Age", 30);

        ICustomTypeDescriptor descriptor = entity.AsTypeDescriptor();

        Assert.AreEqual(2, descriptor.GetProperties().Count);
    }

    [TestMethod]
    public void AsTypeDescriptor_PropertyDescriptorGetValue_ReadsCurrentValue()
    {
        FlexibleEntity entity = new();
        entity.Set("Name", "Ada");

        ICustomTypeDescriptor descriptor = entity.AsTypeDescriptor();
        PropertyDescriptor property = descriptor.GetProperties()["Name"]!;

        Assert.AreEqual("Ada", property.GetValue(entity));
    }

    [TestMethod]
    public void AsTypeDescriptor_PropertyDescriptorSetValue_WritesThroughToEntity()
    {
        FlexibleEntity entity = new();
        entity.Set("Name", "Ada");

        ICustomTypeDescriptor descriptor = entity.AsTypeDescriptor();
        PropertyDescriptor property = descriptor.GetProperties()["Name"]!;
        property.SetValue(entity, "Grace");

        Assert.AreEqual("Grace", entity.Get<string>("Name"));
    }

    #endregion

    #region AsDynamic

    [TestMethod]
    public void AsDynamic_ReflectsSetValues()
    {
        FlexibleEntity entity = new();
        entity.Set("Name", "Ada");

        dynamic dynamicView = entity.AsDynamic;

        Assert.AreEqual("Ada", dynamicView.Name);
    }

    #endregion
}
