using System.ComponentModel;

namespace Catharsis.ComponentModel;

/// <summary>
/// A <see cref="TypeDescriptionProvider"/> that builds
/// <see cref="ICustomTypeDescriptor"/> instances from a
/// <see cref="ComponentMetadataRegistry"/>, producing
/// <see cref="DynamicPropertyDescriptor"/> entries for each registered
/// <see cref="PropertyMetadata"/>.
/// </summary>
/// <remarks>
/// <para>
/// This provider chains to an optional parent <see cref="TypeDescriptionProvider"/>
/// so that standard reflection-based descriptors are preserved for types without
/// explicit registry entries.
/// </para>
/// <para>
/// Register this provider with <see cref="TypeDescriptor.AddProvider"/> to
/// override the default type description for specific component types.
/// </para>
/// </remarks>
public class ComponentDescriptorProvider : TypeDescriptionProvider
{
    private readonly ComponentMetadataRegistry _registry;

    /// <summary>
    /// Initializes a new instance of <see cref="ComponentDescriptorProvider"/>
    /// with the specified metadata registry.
    /// </summary>
    /// <param name="registry">The registry containing property and event metadata.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="registry"/> is <c>null</c>.
    /// </exception>
    public ComponentDescriptorProvider(ComponentMetadataRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(registry);
        _registry = registry;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="ComponentDescriptorProvider"/>
    /// with the specified metadata registry and parent provider.
    /// </summary>
    /// <param name="registry">The registry containing property and event metadata.</param>
    /// <param name="parent">
    /// The parent <see cref="TypeDescriptionProvider"/> to chain to for
    /// unregistered types.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="registry"/> is <c>null</c>.
    /// </exception>
    public ComponentDescriptorProvider(
        ComponentMetadataRegistry registry,
        TypeDescriptionProvider parent)
        : base(parent)
    {
        ArgumentNullException.ThrowIfNull(registry);
        _registry = registry;
    }

    /// <summary>
    /// Gets the metadata registry used by this provider.
    /// </summary>
    protected ComponentMetadataRegistry Registry => _registry;

    /// <inheritdoc />
    public override ICustomTypeDescriptor? GetTypeDescriptor(Type objectType, object? instance)
    {
        var parent = base.GetTypeDescriptor(objectType, instance);

        if (!_registry.HasMetadata(objectType))
            return parent;

        return new RegistryTypeDescriptor(parent, _registry, objectType);
    }

    private sealed class RegistryTypeDescriptor : CustomTypeDescriptor
    {
        private readonly ComponentMetadataRegistry _registry;
        private readonly Type _componentType;

        public RegistryTypeDescriptor(
            ICustomTypeDescriptor? parent,
            ComponentMetadataRegistry registry,
            Type componentType)
            : base(parent)
        {
            _registry = registry;
            _componentType = componentType;
        }

        public override PropertyDescriptorCollection GetProperties()
        {
            var baseProperties = base.GetProperties();
            var registeredMetadata = _registry.GetProperties(_componentType);

            if (registeredMetadata.Count == 0)
                return baseProperties;

            return MergeProperties(baseProperties, registeredMetadata);
        }

        public override PropertyDescriptorCollection GetProperties(Attribute[]? attributes)
        {
            var all = GetProperties();

            if (attributes is null || attributes.Length == 0)
                return all;

            var filtered = new List<PropertyDescriptor>();

            foreach (PropertyDescriptor prop in all)
            {
                if (MatchesAttributes(prop, attributes))
                    filtered.Add(prop);
            }

            return new PropertyDescriptorCollection([.. filtered]);
        }

        public override EventDescriptorCollection GetEvents()
        {
            var baseEvents = base.GetEvents();
            var registeredMetadata = _registry.GetEvents(_componentType);

            if (registeredMetadata.Count == 0)
                return baseEvents;

            return MergeEvents(baseEvents, registeredMetadata);
        }

        public override EventDescriptorCollection GetEvents(Attribute[]? attributes)
        {
            var all = GetEvents();

            if (attributes is null || attributes.Length == 0)
                return all;

            var filtered = new List<EventDescriptor>();

            foreach (EventDescriptor evt in all)
            {
                if (MatchesAttributes(evt, attributes))
                    filtered.Add(evt);
            }

            return new EventDescriptorCollection([.. filtered]);
        }

        private static PropertyDescriptorCollection MergeProperties(
            PropertyDescriptorCollection baseProperties,
            IReadOnlyList<PropertyMetadata> registeredMetadata)
        {
            var merged = new Dictionary<string, PropertyDescriptor>(StringComparer.Ordinal);

            foreach (PropertyDescriptor prop in baseProperties)
            {
                merged[prop.Name] = prop;
            }

            foreach (var metadata in registeredMetadata)
            {
                merged[metadata.Name] = CreatePropertyDescriptor(metadata);
            }

            return new PropertyDescriptorCollection([.. merged.Values]);
        }

        private static EventDescriptorCollection MergeEvents(
            EventDescriptorCollection baseEvents,
            IReadOnlyList<EventMetadata> registeredMetadata)
        {
            var merged = new Dictionary<string, EventDescriptor>(StringComparer.Ordinal);

            foreach (EventDescriptor evt in baseEvents)
            {
                merged[evt.Name] = evt;
            }

            foreach (var metadata in registeredMetadata)
            {
                merged[metadata.Name] = CreateEventDescriptor(metadata);
            }

            return new EventDescriptorCollection([.. merged.Values]);
        }

        private static DynamicPropertyDescriptor CreatePropertyDescriptor(PropertyMetadata metadata)
        {
            return new DynamicPropertyDescriptor(
                metadata,
                getter: component =>
                {
                    var prop = component.GetType().GetProperty(metadata.Name);
                    return prop?.GetValue(component);
                },
                setter: metadata.IsReadOnly
                    ? null
                    : (component, value) =>
                    {
                        var prop = component.GetType().GetProperty(metadata.Name);
                        prop?.SetValue(component, value);
                    });
        }

        private static DynamicEventDescriptor CreateEventDescriptor(EventMetadata metadata)
        {
            return new DynamicEventDescriptor(
                metadata,
                addHandler: (component, handler) =>
                {
                    var evt = component.GetType().GetEvent(metadata.Name);
                    evt?.AddEventHandler(component, handler);
                },
                removeHandler: (component, handler) =>
                {
                    var evt = component.GetType().GetEvent(metadata.Name);
                    evt?.RemoveEventHandler(component, handler);
                });
        }

        private static bool MatchesAttributes(MemberDescriptor descriptor, Attribute[] attributes)
        {
            foreach (var attribute in attributes)
            {
                if (!descriptor.Attributes.Contains(attribute))
                    return false;
            }

            return true;
        }
    }
}
