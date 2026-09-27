using System.ComponentModel;
using System.Reflection;

namespace Catharsis.ComponentModel;

///<summary>
///A higher-level <see cref="TypeDescriptionProvider"/> that combines a <see cref="ComponentMetadataRegistry"/>, <see
///cref="ComponentReflectionCache"/>, and <see cref="MetadataProvider"/> to deliver a unified type description
///experience with caching and metadata lookup support.
///</summary>
///<remarks>
///<para> This provider builds on <see cref="ComponentDescriptorProvider"/> by adding a <see
///cref="ComponentReflectionCache"/> for performance and exposing the full <see cref="MetadataProvider"/> for
///programmatic metadata queries.</para> <para> Typical usage:<code> var provider = new
///ComponentTypeDescriptionProvider(); provider.Registry.RegisterProperty(typeof(MyComponent), myPropertyMetadata);
///TypeDescriptor.AddProvider(provider, typeof(MyComponent));</code></para>
///</remarks>
public sealed class ComponentTypeDescriptionProvider : TypeDescriptionProvider
{
    #region Fields
    readonly ComponentReflectionCache _cache;
    readonly MetadataProvider _metadataProvider;
    readonly ComponentMetadataRegistry _registry;
    #endregion

    #region Constructors
    ///<summary>
    ///Initializes a new instance of <see cref="ComponentTypeDescriptionProvider"/> with new registry and
    ///cache instances.
    ///</summary>
    public ComponentTypeDescriptionProvider()
    {
        _registry = new ComponentMetadataRegistry();
        _cache = new ComponentReflectionCache();
        _metadataProvider = new MetadataProvider(_registry, _cache);
    }

    ///<summary>
    ///Initializes a new instance of <see cref="ComponentTypeDescriptionProvider"/> that chains to the specified
    ///parent provider.
    ///</summary>
    ///<param name="parent">The parent provider to chain to.</param>
    public ComponentTypeDescriptionProvider(TypeDescriptionProvider parent) : base(parent)
    {
        _registry = new ComponentMetadataRegistry();
        _cache = new ComponentReflectionCache();
        _metadataProvider = new MetadataProvider(_registry, _cache);
    }

    ///<summary>
    ///Initializes a new instance of <see cref="ComponentTypeDescriptionProvider"/> with the specified registry and
    ///cache.
    ///</summary>
    ///<param name="registry">The metadata registry.</param>
    ///<param name="cache">The reflection cache.</param>
    ///<exception cref="ArgumentNullException">
    ///<paramref name="registry"/> or <paramref name="cache"/> is <c>null</c>.
    ///</exception>
    public ComponentTypeDescriptionProvider(ComponentMetadataRegistry registry, ComponentReflectionCache cache)
    {
        ArgumentNullException.ThrowIfNull(registry);
        ArgumentNullException.ThrowIfNull(cache);

        _registry = registry;
        _cache = cache;
        _metadataProvider = new MetadataProvider(_registry, _cache);
    }

    ///<summary>
    ///Initializes a new instance of <see cref="ComponentTypeDescriptionProvider"/> with the specified registry,
    ///cache, and parent provider.
    ///</summary>
    ///<param name="registry">The metadata registry.</param>
    ///<param name="cache">The reflection cache.</param>
    ///<param name="parent">The parent provider to chain to.</param>
    ///<exception cref="ArgumentNullException">
    ///<paramref name="registry"/> or <paramref name="cache"/> is <c>null</c>.
    ///</exception>
    public ComponentTypeDescriptionProvider(ComponentMetadataRegistry registry, ComponentReflectionCache cache, TypeDescriptionProvider parent) : base(parent)
    {
        ArgumentNullException.ThrowIfNull(registry);
        ArgumentNullException.ThrowIfNull(cache);

        _registry = registry;
        _cache = cache;
        _metadataProvider = new MetadataProvider(_registry, _cache);
    }
    #endregion

    #region Public methods
    ///<inheritdoc/>
    public override ICustomTypeDescriptor? GetTypeDescriptor(Type objectType, object? instance)
    {
        ICustomTypeDescriptor? parent = base.GetTypeDescriptor(objectType, instance);

        if(!_registry.HasMetadata(objectType))
        {
            return new CachedTypeDescriptor(parent, _cache, objectType);
        }

        return new RegistryCachedTypeDescriptor(parent, _registry, _cache, objectType);
    }

    ///<summary>
    ///Invalidates cached data for the specified type, forcing the next type descriptor request to re-resolve from the
    ///registry and reflection.
    ///</summary>
    ///<param name="componentType">The type to invalidate.</param>
    public void Invalidate(Type componentType)
    {
        ArgumentNullException.ThrowIfNull(componentType);
        _cache.Invalidate(componentType);
    }
    #endregion

    #region Public properties
    ///<summary>
    ///Gets the reflection cache for cached descriptor lookups.
    ///</summary>
    public ComponentReflectionCache Cache => _cache;

    ///<summary>
    ///Gets the metadata provider that resolves metadata from the registry with cache fallback.
    ///</summary>
    public MetadataProvider Metadata => _metadataProvider;

    ///<summary>
    ///Gets the metadata registry for registering property and event metadata.
    ///</summary>
    public ComponentMetadataRegistry Registry => _registry;
    #endregion

    sealed class CachedTypeDescriptor(ICustomTypeDescriptor? parent, ComponentReflectionCache cache, Type componentType) : CustomTypeDescriptor(parent)
    {
        #region Fields
        readonly ComponentReflectionCache _cache = cache;
        readonly Type _componentType = componentType;

        #endregion
        #region Constructors
        #endregion

        #region Public methods
        public override EventDescriptorCollection GetEvents() { return _cache.GetEvents(_componentType); }
        public override EventDescriptorCollection GetEvents(Attribute[]? attributes) { return _cache.GetEvents(_componentType, attributes ?? []); }
        public override PropertyDescriptorCollection GetProperties() { return _cache.GetProperties(_componentType); }
        public override PropertyDescriptorCollection GetProperties(Attribute[]? attributes) { return _cache.GetProperties(_componentType, attributes ?? []); }
        #endregion
    }

    sealed class RegistryCachedTypeDescriptor(ICustomTypeDescriptor? parent, ComponentMetadataRegistry registry, ComponentReflectionCache cache, Type componentType) : CustomTypeDescriptor(parent)
    {
        #region Fields
        readonly ComponentReflectionCache _cache = cache;
        readonly Type _componentType = componentType;
        readonly ComponentMetadataRegistry _registry = registry;

        #endregion
        #region Constructors
        #endregion

        #region Private methods
        static EventDescriptorCollection MergeEvents(EventDescriptorCollection baseEvents, IReadOnlyList<EventMetadata> registeredMetadata)
        {
            Dictionary<string, EventDescriptor> merged = [with(StringComparer.Ordinal)];

            foreach(EventDescriptor evt in baseEvents)
            {
                merged[evt.Name] = evt;
            }

            foreach(EventMetadata metadata in registeredMetadata)
            {
                merged[metadata.Name] = new DynamicEventDescriptor(
                                        metadata,
                                        addHandler: (component, handler) =>
                {
                    EventInfo? evt = component.GetType().GetEvent(metadata.Name);
                    evt?.AddEventHandler(component, handler);
                },
                                        removeHandler: (component, handler) =>
                {
                    EventInfo? evt = component.GetType().GetEvent(metadata.Name);
                    evt?.RemoveEventHandler(component, handler);
                });
            }

            return new EventDescriptorCollection([ .. merged.Values ]);
        }

        static PropertyDescriptorCollection MergeProperties(PropertyDescriptorCollection baseProperties, IReadOnlyList<PropertyMetadata> registeredMetadata)
        {
            Dictionary<string, PropertyDescriptor> merged = [with(StringComparer.Ordinal)];

            foreach(PropertyDescriptor prop in baseProperties)
            {
                merged[prop.Name] = prop;
            }

            foreach(PropertyMetadata metadata in registeredMetadata)
            {
                merged[metadata.Name] = new DynamicPropertyDescriptor(
                                        metadata,
                                        getter: component =>
                {
                    PropertyInfo? prop = component.GetType().GetProperty(metadata.Name);
                    return prop?.GetValue(component);
                },
                                        setter: metadata.IsReadOnly
                                                ? null
                                                : (component, value) =>
                {
                    System.Reflection.PropertyInfo? prop = component.GetType().GetProperty(metadata.Name);
                    prop?.SetValue(component, value);
                });
            }

            return new PropertyDescriptorCollection([ .. merged.Values ]);
        }
        #endregion

        #region Public methods
        public override EventDescriptorCollection GetEvents()
        {
            EventDescriptorCollection baseEvents = _cache.GetEvents(_componentType);
            IReadOnlyList<EventMetadata> registeredMetadata = _registry.GetEvents(_componentType);

            if(registeredMetadata.Count == 0)
            {
                return baseEvents;
            }

            return MergeEvents(baseEvents, registeredMetadata);
        }

        public override EventDescriptorCollection GetEvents(Attribute[]? attributes)
        {
            EventDescriptorCollection all = GetEvents();

            if((attributes is null) || (attributes.Length == 0))
            {
                return all;
            }

            List<EventDescriptor> filtered = [];

            foreach(EventDescriptor evt in all)
            {
                if(evt.Attributes.Matches(attributes))
                {
                    filtered.Add(evt);
                }
            }

            return new EventDescriptorCollection([ .. filtered ]);
        }

        public override PropertyDescriptorCollection GetProperties()
        {
            PropertyDescriptorCollection baseProperties = _cache.GetProperties(_componentType);
            IReadOnlyList<PropertyMetadata> registeredMetadata = _registry.GetProperties(_componentType);

            if(registeredMetadata.Count == 0)
            {
                return baseProperties;
            }

            return MergeProperties(baseProperties, registeredMetadata);
        }

        public override PropertyDescriptorCollection GetProperties(Attribute[]? attributes)
        {
            PropertyDescriptorCollection all = GetProperties();

            if((attributes is null) || (attributes.Length == 0))
            {
                return all;
            }

            List<PropertyDescriptor> filtered = [];

            foreach(PropertyDescriptor prop in all)
            {
                if(prop.Attributes.Matches(attributes))
                {
                    filtered.Add(prop);
                }
            }

            return new PropertyDescriptorCollection([ .. filtered ]);
        }
        #endregion
    }
}
