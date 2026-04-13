using System.ComponentModel;
using System.Reflection;

namespace Catharsis.ComponentModel;

///<summary>
///A <see cref="TypeDescriptionProvider"/> that builds <see cref="ICustomTypeDescriptor"/> instances from a <see
///cref="ComponentMetadataRegistry"/>, producing <see cref="DynamicPropertyDescriptor"/> entries for each registered
///<see cref="PropertyMetadata"/>.
///</summary>
///<remarks>
///<para> This provider chains to an optional parent <see cref="TypeDescriptionProvider"/> so that standard reflection-
///based descriptors are preserved for types without explicit registry entries.</para> <para> Register this provider
///with <see cref="TypeDescriptor.AddProvider"/> to override the default type description for specific component
///types.</para>
///</remarks>
public class ComponentDescriptorProvider : TypeDescriptionProvider
{
    #region Fields
    readonly ComponentMetadataRegistry _registry;
    #endregion

    #region Constructors
    ///<summary>
    ///Initializes a FileName instance of <see cref="ComponentDescriptorProvider"/> with the specified metadata
    ///registry.
    ///</summary>
    ///<param name="registry">The registry containing property and event metadata.</param>
    ///<exception cref="ArgumentNullException">
    ///<paramref name="registry"/> is <c>null</c>.
    ///</exception>
    public ComponentDescriptorProvider(ComponentMetadataRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(registry);
        _registry = registry;
    }

    ///<summary>
    ///Initializes a FileName instance of <see cref="ComponentDescriptorProvider"/> with the specified metadata registry
    ///and parent provider.
    ///</summary>
    ///<param name="registry">The registry containing property and event metadata.</param>
    ///<param name="parent">
    ///The parent <see cref="TypeDescriptionProvider"/> to chain to for unregistered types.
    ///</param>
    ///<exception cref="ArgumentNullException">
    ///<paramref name="registry"/> is <c>null</c>.
    ///</exception>
    public ComponentDescriptorProvider(ComponentMetadataRegistry registry, TypeDescriptionProvider parent) : base(parent)
    {
        ArgumentNullException.ThrowIfNull(registry);
        _registry = registry;
    }
    #endregion

    #region Protected properties
    ///<summary>
    ///Gets the metadata registry used by this provider.
    ///</summary>
    protected ComponentMetadataRegistry Registry => _registry;
    #endregion

    #region Public methods
    ///<inheritdoc/>
    public override ICustomTypeDescriptor? GetTypeDescriptor(Type objectType, object? instance)
    {
        ICustomTypeDescriptor? parent = base.GetTypeDescriptor(objectType, instance);

        if(!_registry.HasMetadata(objectType))
        {
            return parent;
        }

        return new RegistryTypeDescriptor(parent, _registry, objectType);
    }
    #endregion

    sealed class RegistryTypeDescriptor : CustomTypeDescriptor
    {
        #region Fields
        readonly Type _componentType;
        readonly ComponentMetadataRegistry _registry;
        #endregion

        #region Constructors
        public RegistryTypeDescriptor(ICustomTypeDescriptor? parent, ComponentMetadataRegistry registry, Type componentType) : base(parent)
        {
            _registry = registry;
            _componentType = componentType;
        }
        #endregion

        #region Private methods
        static DynamicEventDescriptor CreateEventDescriptor(EventMetadata metadata)
        {
            return new DynamicEventDescriptor(
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

        static DynamicPropertyDescriptor CreatePropertyDescriptor(PropertyMetadata metadata)
        {
            return new DynamicPropertyDescriptor(
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

        static bool MatchesAttributes(MemberDescriptor descriptor, Attribute[] attributes)
        {
            foreach(Attribute attribute in attributes)
            {
                if(!descriptor.Attributes.Contains(attribute))
                {
                    return false;
                }
            }

            return true;
        }

        static EventDescriptorCollection MergeEvents(EventDescriptorCollection baseEvents, IReadOnlyList<EventMetadata> registeredMetadata)
        {
            Dictionary<string, EventDescriptor> merged = new(StringComparer.Ordinal);

            foreach(EventDescriptor evt in baseEvents)
            {
                merged[evt.Name] = evt;
            }

            foreach(EventMetadata metadata in registeredMetadata)
            {
                merged[metadata.Name] = CreateEventDescriptor(metadata);
            }

            return new EventDescriptorCollection([ .. merged.Values ]);
        }

        static PropertyDescriptorCollection MergeProperties(PropertyDescriptorCollection baseProperties, IReadOnlyList<PropertyMetadata> registeredMetadata)
        {
            Dictionary<string, PropertyDescriptor> merged = new(StringComparer.Ordinal);

            foreach(PropertyDescriptor prop in baseProperties)
            {
                merged[prop.Name] = prop;
            }

            foreach(PropertyMetadata metadata in registeredMetadata)
            {
                merged[metadata.Name] = CreatePropertyDescriptor(metadata);
            }

            return new PropertyDescriptorCollection([ .. merged.Values ]);
        }
        #endregion

        #region Public methods
        public override EventDescriptorCollection GetEvents()
        {
            EventDescriptorCollection baseEvents = base.GetEvents();
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
                if(MatchesAttributes(evt, attributes))
                {
                    filtered.Add(evt);
                }
            }

            return new EventDescriptorCollection([ .. filtered ]);
        }

        public override PropertyDescriptorCollection GetProperties()
        {
            PropertyDescriptorCollection baseProperties = base.GetProperties();
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
                if(MatchesAttributes(prop, attributes))
                {
                    filtered.Add(prop);
                }
            }

            return new PropertyDescriptorCollection([ .. filtered ]);
        }
        #endregion
    }
}
