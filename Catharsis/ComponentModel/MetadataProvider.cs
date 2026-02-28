using System.ComponentModel;

namespace Catharsis.ComponentModel;

///<summary>
///A service that resolves property and event metadata for component types, first consulting a <see
///cref="ComponentMetadataRegistry"/> and then falling back to a <see cref="ComponentReflectionCache"/> for types
///without explicit registrations.
///</summary>
///<remarks>
public sealed class MetadataProvider
{
    #region Fields
    private readonly ComponentReflectionCache _cache;
    private readonly ComponentMetadataRegistry _registry;
    #endregion

    #region Constructors
    ///<summary>
    ///Initializes a FileName instance of <see cref="MetadataProvider"/> with the specified registry and reflection
    ///cache.
    ///</summary>
    ///<param name="registry">The metadata registry to consult first.</param>
    ///<param name="cache">The reflection cache for fallback lookups.</param>
    ///<exception cref="ArgumentNullException">
    public MetadataProvider(ComponentMetadataRegistry registry, ComponentReflectionCache cache)
    {
        ArgumentNullException.ThrowIfNull(registry);
        ArgumentNullException.ThrowIfNull(cache);

        _registry = registry;
        _cache = cache;
    }
    #endregion

    #region Private methods
    private IReadOnlyList<EventMetadata> ConvertToEventMetadata(Type componentType)
    {
        EventDescriptorCollection descriptors = _cache.GetEvents(componentType);
        EventMetadata[] result = new EventMetadata[descriptors.Count];

        for (int i = 0; i < descriptors.Count; i++)
        {
            result[i] = DescriptorToEventMetadata(componentType, descriptors[i]!);
        }

        return result;
    }

    private IReadOnlyList<PropertyMetadata> ConvertToPropertyMetadata(Type componentType)
    {
        PropertyDescriptorCollection descriptors = _cache.GetProperties(componentType);
        PropertyMetadata[] result = new PropertyMetadata[descriptors.Count];

        for (int i = 0; i < descriptors.Count; i++)
        {
            result[i] = DescriptorToPropertyMetadata(componentType, descriptors[i]);
        }

        return result;
    }

    private static EventMetadata DescriptorToEventMetadata(Type componentType, EventDescriptor descriptor)
    {
        Attribute[] attrs = new Attribute[descriptor.Attributes.Count];

        for (int i = 0; i < descriptor.Attributes.Count; i++)
        {
            attrs[i] = descriptor.Attributes[i];
        }

        return new EventMetadata(descriptor.Name, descriptor.EventType, componentType, descriptor.IsMulticast, attrs);
    }

    private static PropertyMetadata DescriptorToPropertyMetadata(Type componentType, PropertyDescriptor descriptor)
    {
        Attribute[] attrs = new Attribute[descriptor.Attributes.Count];

        for (int i = 0; i < descriptor.Attributes.Count; i++)
        {
            attrs[i] = descriptor.Attributes[i];
        }

        return new PropertyMetadata(descriptor.Name, descriptor.PropertyType, componentType, descriptor.IsReadOnly, attributes: attrs);
    }
    #endregion

    #region Public methods
    ///<summary>
    ///Attempts to find event metadata by name, checking the registry first and then the reflection cache.
    ///</summary>
    ///<param name="componentType">The component type to search.</param>
    ///<param name="eventName">The event name.</param>
    ///<returns>
    ///The matching <see cref="EventMetadata"/>, or <c>null</c> if not found.
    ///</returns>
    public EventMetadata? FindEvent(Type componentType, string eventName)
    {
        ArgumentNullException.ThrowIfNull(componentType);
        ArgumentException.ThrowIfNullOrWhiteSpace(eventName);

        if (_registry.TryGetEvent(componentType, eventName, out EventMetadata? metadata))
        {
            return metadata;
        }

        EventDescriptor? descriptor = _cache.FindEvent(componentType, eventName);

        if (descriptor is null)
        {
            return null;
        }

        return DescriptorToEventMetadata(componentType, descriptor);
    }

    ///<summary>
    ///Attempts to find property metadata by name, checking the registry first and then the reflection cache.
    ///</summary>
    ///<param name="componentType">The component type to search.</param>
    ///<param name="propertyName">The property name.</param>
    ///<returns>
    ///The matching <see cref="PropertyMetadata"/>, or <c>null</c> if not found.
    ///</returns>
    public PropertyMetadata? FindProperty(Type componentType, string propertyName)
    {
        ArgumentNullException.ThrowIfNull(componentType);
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);

        if (_registry.TryGetProperty(componentType, propertyName, out PropertyMetadata? metadata))
        {
            return metadata;
        }

        PropertyDescriptor? descriptor = _cache.FindProperty(componentType, propertyName);

        if (descriptor is null)
        {
            return null;
        }

        return DescriptorToPropertyMetadata(componentType, descriptor);
    }

    ///<summary>
    ///Gets the event metadata for the specified type. Returns explicitly registered metadata if available; otherwise
    ///returns reflected event descriptors wrapped as metadata.
    ///</summary>
    ///<param name="componentType">The component type to query.</param>
    ///<returns>A read-only list of event metadata.</returns>
    public IReadOnlyList<EventMetadata> GetEventMetadata(Type componentType)
    {
        ArgumentNullException.ThrowIfNull(componentType);

        IReadOnlyList<EventMetadata> registered = _registry.GetEvents(componentType);

        if (registered.Count > 0)
        {
            return registered;
        }

        return ConvertToEventMetadata(componentType);
    }

    ///<summary>
    ///Gets the property metadata for the specified type. Returns explicitly registered metadata if available; otherwise
    ///returns reflected property descriptors wrapped as metadata.
    ///</summary>
    ///<param name="componentType">The component type to query.</param>
    ///<returns>A read-only list of property metadata.</returns>
    public IReadOnlyList<PropertyMetadata> GetPropertyMetadata(Type componentType)
    {
        ArgumentNullException.ThrowIfNull(componentType);

        IReadOnlyList<PropertyMetadata> registered = _registry.GetProperties(componentType);

        if (registered.Count > 0)
        {
            return registered;
        }

        return ConvertToPropertyMetadata(componentType);
    }

    ///<summary>
    ///Gets a value indicating whether the specified type has any explicitly registered metadata in the registry.
    ///</summary>
    ///<param name="componentType">The component type to check.</param>
    ///<returns>
    public bool HasRegisteredMetadata(Type componentType)
    {
        ArgumentNullException.ThrowIfNull(componentType);
        return _registry.HasMetadata(componentType);
    }

    ///<summary>
    ///Invalidates any cached reflection data for the specified type, forcing the next lookup to re-reflect.
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
    ///Gets the reflection cache used by this provider.
    ///</summary>
    public ComponentReflectionCache Cache => _cache;

    ///<summary>
    ///Gets the metadata registry used by this provider.
    ///</summary>
    public ComponentMetadataRegistry Registry => _registry;
    #endregion
}
