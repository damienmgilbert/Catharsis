using System.Collections.Concurrent;
using System.ComponentModel;

namespace Catharsis.ComponentModel;

///<summary>
///A thread-safe cache for reflected <see cref="PropertyDescriptorCollection"/> and <see
///cref="EventDescriptorCollection"/> instances, keyed by component type.
///</summary>
///<remarks>
///<para> Reflection and <see cref="TypeDescriptor"/> calls can be expensive when performed repeatedly. This cache
///stores the results of property and event descriptor lookups so they are computed at most once per type.</para> <para>
///Call <see cref="Invalidate(Type)"/> or <see cref="Clear"/> when the descriptor shape of a type changes at runtime
///(e.g., via<see cref="TypeDescriptor.AddProvider"/>).</para>
///</remarks>
public sealed class ComponentReflectionCache
{
    #region Fields
    readonly ConcurrentDictionary<Type, EventDescriptorCollection> _eventCache = new();
    readonly ConcurrentDictionary<Type, PropertyDescriptorCollection> _propertyCache = new();
    #endregion

    #region Public methods
    ///<summary>
    ///Clears the entire cache.
    ///</summary>
    public void Clear()
    {
        _propertyCache.Clear();
        _eventCache.Clear();
    }

    ///<summary>
    ///Looks up a single cached event descriptor by name.
    ///</summary>
    ///<param name="componentType">The owning type.</param>
    ///<param name="eventName">The event name to find.</param>
    ///<returns>
    ///The matching <see cref="EventDescriptor"/>, or <c>null</c> if not found.
    ///</returns>
    public EventDescriptor? FindEvent(Type componentType, string eventName)
    {
        ArgumentNullException.ThrowIfNull(componentType);
        ArgumentException.ThrowIfNullOrWhiteSpace(eventName);

        return GetEvents(componentType).Find(eventName, ignoreCase: false);
    }

    ///<summary>
    ///Looks up a single cached property descriptor by name.
    ///</summary>
    ///<param name="componentType">The owning type.</param>
    ///<param name="propertyName">The property name to find.</param>
    ///<returns>
    ///The matching <see cref="PropertyDescriptor"/>, or <c>null</c> if not found.
    ///</returns>
    public PropertyDescriptor? FindProperty(Type componentType, string propertyName)
    {
        ArgumentNullException.ThrowIfNull(componentType);
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);

        return GetProperties(componentType).Find(propertyName, ignoreCase: false);
    }

    ///<summary>
    ///Gets or creates the cached <see cref="EventDescriptorCollection"/> for the specified component type.
    ///</summary>
    ///<param name="componentType">The type to retrieve event descriptors for.</param>
    ///<returns>The cached event descriptor collection.</returns>
    ///<exception cref="ArgumentNullException">
    ///<paramref name="componentType"/> is <c>null</c>.
    ///</exception>
    public EventDescriptorCollection GetEvents(Type componentType)
    {
        ArgumentNullException.ThrowIfNull(componentType);

        return _eventCache.GetOrAdd(componentType, static type => TypeDescriptor.GetEvents(type));
    }

    ///<summary>
    ///Gets or creates the cached <see cref="EventDescriptorCollection"/> for the specified component type, filtered by
    ///the given attributes.
    ///</summary>
    ///<param name="componentType">The type to retrieve event descriptors for.</param>
    ///<param name="attributes">Attributes to filter by.</param>
    ///<returns>The filtered event descriptor collection (not cached).</returns>
    ///<exception cref="ArgumentNullException">
    ///<paramref name="componentType"/> is <c>null</c>.
    ///</exception>
    public EventDescriptorCollection GetEvents(Type componentType, Attribute[] attributes)
    {
        ArgumentNullException.ThrowIfNull(componentType);

        EventDescriptorCollection all = GetEvents(componentType);

        if((attributes is null) || (attributes.Length == 0))
        {
            return all;
        }

        List<EventDescriptor> filtered = new List<EventDescriptor>();

        foreach(EventDescriptor evt in all)
        {
            if(evt.Attributes.Matches(attributes))
            {
                filtered.Add(evt);
            }
        }

        return new EventDescriptorCollection([ .. filtered ]);
    }

    ///<summary>
    ///Gets or creates the cached <see cref="PropertyDescriptorCollection"/> for the specified component type.
    ///</summary>
    ///<param name="componentType">The type to retrieve property descriptors for.</param>
    ///<returns>The cached property descriptor collection.</returns>
    ///<exception cref="ArgumentNullException">
    ///<paramref name="componentType"/> is <c>null</c>.
    ///</exception>
    public PropertyDescriptorCollection GetProperties(Type componentType)
    {
        ArgumentNullException.ThrowIfNull(componentType);

        return _propertyCache.GetOrAdd(componentType, static type => TypeDescriptor.GetProperties(type));
    }

    ///<summary>
    ///Gets or creates the cached <see cref="PropertyDescriptorCollection"/> for the specified component type, filtered
    ///by the given attributes.
    ///</summary>
    ///<param name="componentType">The type to retrieve property descriptors for.</param>
    ///<param name="attributes">Attributes to filter by.</param>
    ///<returns>The filtered property descriptor collection (not cached).</returns>
    ///<exception cref="ArgumentNullException">
    ///<paramref name="componentType"/> is <c>null</c>.
    ///</exception>
    public PropertyDescriptorCollection GetProperties(Type componentType, Attribute[] attributes)
    {
        ArgumentNullException.ThrowIfNull(componentType);

        PropertyDescriptorCollection all = GetProperties(componentType);

        if((attributes is null) || (attributes.Length == 0))
        {
            return all;
        }

        List<PropertyDescriptor> filtered = new List<PropertyDescriptor>();

        foreach(PropertyDescriptor prop in all)
        {
            if(prop.Attributes.Matches(attributes))
            {
                filtered.Add(prop);
            }
        }

        return new PropertyDescriptorCollection([ .. filtered ]);
    }

    ///<summary>
    ///Invalidates all cached descriptors for the specified type.
    ///</summary>
    ///<param name="componentType">The type to invalidate.</param>
    public void Invalidate(Type componentType)
    {
        ArgumentNullException.ThrowIfNull(componentType);

        _propertyCache.TryRemove(componentType, out _);
        _eventCache.TryRemove(componentType, out _);
    }
    #endregion

    #region Public properties
    ///<summary>
    ///Gets the number of types currently cached for event descriptors.
    ///</summary>
    public int EventCacheCount => _eventCache.Count;

    ///<summary>
    ///Gets the number of types currently cached for property descriptors.
    ///</summary>
    public int PropertyCacheCount => _propertyCache.Count;
    #endregion
}
