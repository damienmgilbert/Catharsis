using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace Catharsis.ComponentModel;

///<summary>
///A thread-safe registry that maps component types to their <see cref="PropertyMetadata"/> and <see
///cref="EventMetadata"/> collections.
///</summary>
///<remarks>
///<para> Register metadata for a type using <see cref="RegisterProperty"/> and<see cref="RegisterEvent"/>. Retrieve it
///with <see cref="GetProperties"/> and <see cref="GetEvents"/>.</para> <para> The registry is intended to be populated
///at application startup and queried by <see cref="MetadataProvider"/> and descriptor providers during type
///description.</para>
///</remarks>
public sealed class ComponentMetadataRegistry
{
    #region Fields
    readonly ConcurrentDictionary<Type, List<EventMetadata>> _events = new();
    readonly ConcurrentDictionary<Type, List<PropertyMetadata>> _properties = new();
    #endregion

    #region Public methods
    ///<summary>
    ///Removes all registered metadata for all types.
    ///</summary>
    public void Clear()
    {
        _properties.Clear();
        _events.Clear();
    }

    ///<summary>
    ///Gets all registered <see cref="EventMetadata"/> for the specified type.
    ///</summary>
    ///<param name="componentType">The component type to query.</param>
    ///<returns>
    ///A read-only list of event metadata, or an empty list if none are registered.
    ///</returns>
    public IReadOnlyList<EventMetadata> GetEvents(Type componentType)
    {
        ArgumentNullException.ThrowIfNull(componentType);

        if(_events.TryGetValue(componentType, out List<EventMetadata> list))
        {
            lock(list)
            {
                return list.ToArray();
            }
        }

        return [];
    }

    ///<summary>
    ///Gets all registered <see cref="PropertyMetadata"/> for the specified type.
    ///</summary>
    ///<param name="componentType">The component type to query.</param>
    ///<returns>
    ///A read-only list of property metadata, or an empty list if none are registered.
    ///</returns>
    public IReadOnlyList<PropertyMetadata> GetProperties(Type componentType)
    {
        ArgumentNullException.ThrowIfNull(componentType);

        if(_properties.TryGetValue(componentType, out List<PropertyMetadata> list))
        {
            lock(list)
            {
                return list.ToArray();
            }
        }

        return [];
    }

    ///<summary>
    ///Gets a value indicating whether any metadata is registered for the specified type.
    ///</summary>
    ///<param name="componentType">The component type to check.</param>
    ///<returns>
    ///<c>true</c> if property or event metadata exists; otherwise, <c>false</c>.
    ///</returns>
    public bool HasMetadata(Type componentType)
    {
        ArgumentNullException.ThrowIfNull(componentType);
        return _properties.ContainsKey(componentType) || _events.ContainsKey(componentType);
    }

    ///<summary>
    ///Registers an <see cref="EventMetadata"/> entry for the specified component type.
    ///</summary>
    ///<param name="componentType">The component type to register the event for.</param>
    ///<param name="metadata">The event metadata to register.</param>
    ///<returns>This instance, for fluent chaining.</returns>
    ///<exception cref="ArgumentNullException">
    ///<paramref name="componentType"/> or <paramref name="metadata"/> is <c>null</c>.
    ///</exception>
    ///<exception cref="ArgumentException">
    ///An event with the same name is already registered for the type.
    ///</exception>
    public ComponentMetadataRegistry RegisterEvent(Type componentType, EventMetadata metadata)
    {
        ArgumentNullException.ThrowIfNull(componentType);
        ArgumentNullException.ThrowIfNull(metadata);

        List<EventMetadata> list = _events.GetOrAdd(componentType, _ => []);

        lock(list)
        {
            if(list.Any(e => string.Equals(e.Name, metadata.Name, StringComparison.Ordinal)))
            {
                throw new ArgumentException($"An event named '{metadata.Name}' is already registered for type '{componentType.Name}'.", nameof(metadata));
            }

            list.Add(metadata);
        }

        return this;
    }

    ///<summary>
    ///Registers a <see cref="PropertyMetadata"/> entry for the specified component type.
    ///</summary>
    ///<param name="componentType">The component type to register the property for.</param>
    ///<param name="metadata">The property metadata to register.</param>
    ///<returns>This instance, for fluent chaining.</returns>
    ///<exception cref="ArgumentNullException">
    ///<paramref name="componentType"/> or <paramref name="metadata"/> is <c>null</c>.
    ///</exception>
    ///<exception cref="ArgumentException">
    ///A property with the same name is already registered for the type.
    ///</exception>
    public ComponentMetadataRegistry RegisterProperty(Type componentType, PropertyMetadata metadata)
    {
        ArgumentNullException.ThrowIfNull(componentType);
        ArgumentNullException.ThrowIfNull(metadata);

        List<PropertyMetadata> list = _properties.GetOrAdd(componentType, _ => []);

        lock(list)
        {
            if(list.Any(p => string.Equals(p.Name, metadata.Name, StringComparison.Ordinal)))
            {
                throw new ArgumentException($"A property named '{metadata.Name}' is already registered for type '{componentType.Name}'.", nameof(metadata));
            }

            list.Add(metadata);
        }

        return this;
    }

    ///<summary>
    ///Attempts to find an <see cref="EventMetadata"/> by name for the specified type.
    ///</summary>
    ///<param name="componentType">The component type to search.</param>
    ///<param name="eventName">The event name to find.</param>
    ///<param name="metadata">
    ///When this method returns, contains the matching metadata if found; otherwise, <c>null</c>.
    ///</param>
    ///<returns><c>true</c> if the event was found; otherwise, <c>false</c>.</returns>
    public bool TryGetEvent(Type componentType, string eventName, [NotNullWhen(true)] out EventMetadata? metadata)
    {
        ArgumentNullException.ThrowIfNull(componentType);

        metadata = null;

        if(!_events.TryGetValue(componentType, out List<EventMetadata> list))
        {
            return false;
        }

        lock(list)
        {
            metadata = list.FirstOrDefault(e => string.Equals(e.Name, eventName, StringComparison.Ordinal));
        }

        return metadata is not null;
    }

    ///<summary>
    ///Attempts to find a <see cref="PropertyMetadata"/> by name for the specified type.
    ///</summary>
    ///<param name="componentType">The component type to search.</param>
    ///<param name="propertyName">The property name to find.</param>
    ///<param name="metadata">
    ///When this method returns, contains the matching metadata if found; otherwise, <c>null</c>.
    ///</param>
    ///<returns><c>true</c> if the property was found; otherwise, <c>false</c>.</returns>
    public bool TryGetProperty(Type componentType, string propertyName, [NotNullWhen(true)] out PropertyMetadata? metadata)
    {
        ArgumentNullException.ThrowIfNull(componentType);

        metadata = null;

        if(!_properties.TryGetValue(componentType, out List<PropertyMetadata> list))
        {
            return false;
        }

        lock(list)
        {
            metadata = list.FirstOrDefault(p => string.Equals(p.Name, propertyName, StringComparison.Ordinal));
        }

        return metadata is not null;
    }

    ///<summary>
    ///Removes all registered metadata for the specified type.
    ///</summary>
    ///<param name="componentType">The component type to unregister.</param>
    ///<returns>
    ///<c>true</c> if any metadata was removed; otherwise, <c>false</c>.
    ///</returns>
    public bool Unregister(Type componentType)
    {
        ArgumentNullException.ThrowIfNull(componentType);

        bool removed = _properties.TryRemove(componentType, out _);
        removed |= _events.TryRemove(componentType, out _);
        return removed;
    }
    #endregion
}
