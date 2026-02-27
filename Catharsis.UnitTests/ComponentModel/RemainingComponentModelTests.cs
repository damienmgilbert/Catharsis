using Catharsis.ComponentModel.DTO;
using Catharsis.ComponentModel.Lifecycle;
using Catharsis.ComponentModel.Observability;
using System.ComponentModel;

namespace Catharsis.ComponentModel.RemainingTests;

[TestClass]
public class ComponentContainerTests
{
    private sealed class TestComponent : Component { }

    [TestMethod]
    public void Add_Component_IncreasesCount()
    {
        using var container = new ComponentContainer();
        container.Add(new TestComponent());
        Assert.AreEqual(1, container.Count);
    }

    [TestMethod]
    public void Add_WithName_SitesComponent()
    {
        using var container = new ComponentContainer();
        var comp = new TestComponent();
        container.Add(comp, "test");
        Assert.IsNotNull(comp.Site);
        Assert.AreEqual("test", comp.Site.Name);
    }

    [TestMethod]
    public void Add_DuplicateName_Throws()
    {
        using var container = new ComponentContainer();
        container.Add(new TestComponent(), "name");
        Assert.ThrowsExactly<ArgumentException>(() => container.Add(new TestComponent(), "name"));
    }

    [TestMethod]
    public void Add_Null_IsIgnored()
    {
        using var container = new ComponentContainer();
        container.Add(null);
        Assert.AreEqual(0, container.Count);
    }

    [TestMethod]
    public void Remove_Component_DecreasesCount()
    {
        using var container = new ComponentContainer();
        var comp = new TestComponent();
        container.Add(comp);
        container.Remove(comp);
        Assert.AreEqual(0, container.Count);
        Assert.IsNull(comp.Site);
    }

    [TestMethod]
    public void GetComponent_ByName_ReturnsComponent()
    {
        using var container = new ComponentContainer();
        var comp = new TestComponent();
        container.Add(comp, "myComp");
        Assert.AreSame(comp, container.GetComponent("myComp"));
    }

    [TestMethod]
    public void Components_ReturnsAllComponents()
    {
        using var container = new ComponentContainer();
        container.Add(new TestComponent());
        container.Add(new TestComponent());
        Assert.AreEqual(2, container.Components.Count);
    }
}

[TestClass]
public class PropertyMetadataTests
{
    [TestMethod]
    public void Constructor_SetsProperties()
    {
        var meta = new PropertyMetadata("Name", typeof(string), typeof(object));
        Assert.AreEqual("Name", meta.Name);
        Assert.AreEqual(typeof(string), meta.PropertyType);
        Assert.AreEqual(typeof(object), meta.ComponentType);
        Assert.IsFalse(meta.IsReadOnly);
        Assert.IsNull(meta.DefaultValue);
    }

    [TestMethod]
    public void Constructor_WithDefaults()
    {
        var meta = new PropertyMetadata("Age", typeof(int), typeof(object), isReadOnly: true, defaultValue: 25);
        Assert.IsTrue(meta.IsReadOnly);
        Assert.AreEqual(25, meta.DefaultValue);
    }

    [TestMethod]
    public void Constructor_NullName_Throws()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => new PropertyMetadata(null!, typeof(string), typeof(object)));
    }

    [TestMethod]
    public void Constructor_NullType_Throws()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => new PropertyMetadata("X", null!, typeof(object)));
    }
}

[TestClass]
public class EventMetadataTests
{
    [TestMethod]
    public void Constructor_SetsProperties()
    {
        var meta = new EventMetadata("Click", typeof(EventHandler), typeof(object));
        Assert.AreEqual("Click", meta.Name);
        Assert.AreEqual(typeof(EventHandler), meta.EventType);
        Assert.AreEqual(typeof(object), meta.ComponentType);
        Assert.IsTrue(meta.IsMulticast);
    }

    [TestMethod]
    public void Constructor_NullName_Throws()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => new EventMetadata(null!, typeof(EventHandler), typeof(object)));
    }

    [TestMethod]
    public void Constructor_NullEventType_Throws()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => new EventMetadata("E", null!, typeof(object)));
    }

    [TestMethod]
    public void ToString_ReturnsNameAndType()
    {
        var meta = new EventMetadata("Click", typeof(EventHandler), typeof(object));
        Assert.AreEqual("Click (EventHandler)", meta.ToString());
    }
}

[TestClass]
public class AttributeCollectionBuilderTests
{
    [TestMethod]
    public void Add_IncreasesCount()
    {
        var builder = new AttributeCollectionBuilder();
        builder.Add(new System.ComponentModel.DescriptionAttribute("test"));
        Assert.AreEqual(1, builder.Count);
    }

    [TestMethod]
    public void Add_SameType_Replaces()
    {
        var builder = new AttributeCollectionBuilder();
        builder.Add(new System.ComponentModel.DescriptionAttribute("first"));
        builder.Add(new System.ComponentModel.DescriptionAttribute("second"));
        Assert.AreEqual(1, builder.Count);
    }

    [TestMethod]
    public void Remove_ByType_RemovesAttribute()
    {
        var builder = new AttributeCollectionBuilder();
        builder.Add(new System.ComponentModel.DescriptionAttribute("test"));
        builder.Remove<System.ComponentModel.DescriptionAttribute>();
        Assert.AreEqual(0, builder.Count);
    }

    [TestMethod]
    public void AddRange_AddsMultiple()
    {
        var builder = new AttributeCollectionBuilder();
        builder.AddRange([new System.ComponentModel.DescriptionAttribute("desc"), new CategoryAttribute("cat")]);
        Assert.AreEqual(2, builder.Count);
    }

    [TestMethod]
    public void Build_ReturnsAttributeCollection()
    {
        var builder = new AttributeCollectionBuilder();
        builder.Add(new System.ComponentModel.DescriptionAttribute("hello"));
        var collection = builder.Build();
        Assert.IsNotNull(collection);
        Assert.IsTrue(collection.Count > 0);
    }

    [TestMethod]
    public void FluentChaining_Works()
    {
        var builder = new AttributeCollectionBuilder()
            .Add(new System.ComponentModel.DescriptionAttribute("a"))
            .Add(new CategoryAttribute("b"));
        Assert.AreEqual(2, builder.Count);
    }
}

[TestClass]
public class ComponentReflectionCacheTests
{
    [TestMethod]
    public void GetProperties_ReturnsCachedCollection()
    {
        var cache = new ComponentReflectionCache();
        var props1 = cache.GetProperties(typeof(string));
        var props2 = cache.GetProperties(typeof(string));
        Assert.AreSame(props1, props2);
    }

    [TestMethod]
    public void GetProperties_NullType_Throws()
    {
        var cache = new ComponentReflectionCache();
        Assert.ThrowsExactly<ArgumentNullException>(() => cache.GetProperties(null!));
    }

    [TestMethod]
    public void GetEvents_ReturnsCachedCollection()
    {
        var cache = new ComponentReflectionCache();
        var events1 = cache.GetEvents(typeof(Component));
        var events2 = cache.GetEvents(typeof(Component));
        Assert.AreSame(events1, events2);
    }
}

[TestClass]
public class ComponentMetadataRegistryTests
{
    [TestMethod]
    public void RegisterProperty_And_GetProperties()
    {
        var registry = new ComponentMetadataRegistry();
        var meta = new PropertyMetadata("Name", typeof(string), typeof(object));
        registry.RegisterProperty(typeof(object), meta);

        var props = registry.GetProperties(typeof(object));
        Assert.AreEqual(1, props.Count);
        Assert.AreEqual("Name", props[0].Name);
    }

    [TestMethod]
    public void RegisterProperty_DuplicateName_Throws()
    {
        var registry = new ComponentMetadataRegistry();
        var meta = new PropertyMetadata("Name", typeof(string), typeof(object));
        registry.RegisterProperty(typeof(object), meta);
        Assert.ThrowsExactly<ArgumentException>(() =>
            registry.RegisterProperty(typeof(object), new PropertyMetadata("Name", typeof(int), typeof(object))));
    }

    [TestMethod]
    public void RegisterEvent_And_GetEvents()
    {
        var registry = new ComponentMetadataRegistry();
        var meta = new EventMetadata("Click", typeof(EventHandler), typeof(object));
        registry.RegisterEvent(typeof(object), meta);

        var events = registry.GetEvents(typeof(object));
        Assert.AreEqual(1, events.Count);
        Assert.AreEqual("Click", events[0].Name);
    }

    [TestMethod]
    public void GetProperties_UnregisteredType_ReturnsEmpty()
    {
        var registry = new ComponentMetadataRegistry();
        var props = registry.GetProperties(typeof(string));
        Assert.AreEqual(0, props.Count);
    }
}

[TestClass]
public class ChangeSetTests
{
    [TestMethod]
    public void Record_AddsEntry()
    {
        var set = new ChangeSet();
        set.Record("Prop", "old", "new");
        Assert.AreEqual(1, set.Count);
        Assert.IsTrue(set.HasChanges);
        Assert.IsTrue(set.CanUndo);
    }

    [TestMethod]
    public void Undo_MovesToRedoStack()
    {
        var set = new ChangeSet();
        set.Record("Prop", "a", "b");
        var entry = set.Undo();
        Assert.IsNotNull(entry);
        Assert.AreEqual("Prop", entry.PropertyName);
        Assert.AreEqual(0, set.Count);
        Assert.IsTrue(set.CanRedo);
    }

    [TestMethod]
    public void Undo_EmptyStack_ReturnsNull()
    {
        var set = new ChangeSet();
        Assert.IsNull(set.Undo());
    }

    [TestMethod]
    public void Redo_RestoresEntry()
    {
        var set = new ChangeSet();
        set.Record("Prop", "a", "b");
        set.Undo();
        var entry = set.Redo();
        Assert.IsNotNull(entry);
        Assert.AreEqual(1, set.Count);
        Assert.IsFalse(set.CanRedo);
    }

    [TestMethod]
    public void Redo_EmptyStack_ReturnsNull()
    {
        var set = new ChangeSet();
        Assert.IsNull(set.Redo());
    }

    [TestMethod]
    public void Record_ClearsRedoStack()
    {
        var set = new ChangeSet();
        set.Record("P", "a", "b");
        set.Undo();
        Assert.IsTrue(set.CanRedo);
        set.Record("P", "c", "d");
        Assert.IsFalse(set.CanRedo);
    }

    [TestMethod]
    public void GetAll_ReturnsChronologicalOrder()
    {
        var set = new ChangeSet();
        set.Record("A", null, 1);
        set.Record("B", null, 2);
        var all = set.GetAll();
        Assert.AreEqual(2, all.Count);
        Assert.AreEqual("A", all[0].PropertyName);
        Assert.AreEqual("B", all[1].PropertyName);
    }

    [TestMethod]
    public void GetByProperty_FiltersCorrectly()
    {
        var set = new ChangeSet();
        set.Record("A", null, 1);
        set.Record("B", null, 2);
        set.Record("A", 1, 3);
        var aChanges = set.GetByProperty("A");
        Assert.AreEqual(2, aChanges.Count);
    }

    [TestMethod]
    public void AcceptAll_ClearsBothStacks()
    {
        var set = new ChangeSet();
        set.Record("P", null, 1);
        set.Undo();
        set.AcceptAll();
        Assert.IsFalse(set.HasChanges);
        Assert.IsFalse(set.CanRedo);
    }

    [TestMethod]
    public void Clear_ResetsBothStacks()
    {
        var set = new ChangeSet();
        set.Record("P", null, 1);
        set.Clear();
        Assert.AreEqual(0, set.Count);
    }
}

[TestClass]
public class ComponentModelDtoOptionsTests
{
    [TestMethod]
    public void Default_HasExpectedValues()
    {
        var options = ComponentModelDtoOptions.Default;
        Assert.IsTrue(options.RaisePropertyChangedOnMap);
        Assert.IsFalse(options.ValidateAfterMap);
        Assert.IsTrue(options.PreserveMetadata);
        Assert.IsTrue(options.IgnoreMissingProperties);
        Assert.AreEqual(StringComparison.Ordinal, options.PropertyNameComparison);
    }

    [TestMethod]
    public void Properties_CanBeModified()
    {
        var options = new ComponentModelDtoOptions
        {
            RaisePropertyChangedOnMap = false,
            ValidateAfterMap = true,
            PreserveMetadata = false,
            IgnoreMissingProperties = false,
            PropertyNameComparison = StringComparison.OrdinalIgnoreCase
        };

        Assert.IsFalse(options.RaisePropertyChangedOnMap);
        Assert.IsTrue(options.ValidateAfterMap);
        Assert.IsFalse(options.PreserveMetadata);
        Assert.IsFalse(options.IgnoreMissingProperties);
        Assert.AreEqual(StringComparison.OrdinalIgnoreCase, options.PropertyNameComparison);
    }
}

[TestClass]
public class ComponentTransitionTests
{
    [TestMethod]
    public void Constructor_SetsProperties()
    {
        var t = new ComponentTransition(ComponentState.Created, ComponentState.Initializing);
        Assert.AreEqual(ComponentState.Created, t.From);
        Assert.AreEqual(ComponentState.Initializing, t.To);
        Assert.AreEqual("Created -> Initializing", t.Name);
    }

    [TestMethod]
    public void Constructor_CustomName()
    {
        var t = new ComponentTransition(ComponentState.Active, ComponentState.Deactivating, "Pause");
        Assert.AreEqual("Pause", t.Name);
    }

    [TestMethod]
    public void CanExecute_NoGuard_ReturnsTrue()
    {
        var t = new ComponentTransition(ComponentState.Created, ComponentState.Initialized);
        Assert.IsTrue(t.CanExecute());
    }

    [TestMethod]
    public void CanExecute_GuardReturnsFalse_ReturnsFalse()
    {
        var t = new ComponentTransition(ComponentState.Created, ComponentState.Initialized)
        {
            Guard = () => false
        };
        Assert.IsFalse(t.CanExecute());
    }

    [TestMethod]
    public void OnTransition_Invoked()
    {
        bool invoked = false;
        var t = new ComponentTransition(ComponentState.Created, ComponentState.Initialized)
        {
            OnTransition = () => invoked = true
        };
        t.OnTransition?.Invoke();
        Assert.IsTrue(invoked);
    }

    [TestMethod]
    public void ToString_ReturnsName()
    {
        var t = new ComponentTransition(ComponentState.Active, ComponentState.Disposed);
        Assert.AreEqual("Active -> Disposed", t.ToString());
    }
}
