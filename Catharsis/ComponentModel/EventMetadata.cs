using System.ComponentModel;

namespace Catharsis.ComponentModel;

///<summary>
///Immutable metadata describing an event for use with dynamic descriptor infrastructure. Captures the event name,
///handler type, owning component type, multicast behavior, and associated attributes.
///</summary>
///<remarks>
///Use <see cref="EventMetadata"/> to register event shapes in a <see cref="ComponentMetadataRegistry"/> without
///requiring reflection at descriptor-creation time.
///</remarks>
public sealed class EventMetadata
{
    #region Constructors
    ///<summary>
    ///Initializes a new instance of <see cref="EventMetadata"/>.
    ///</summary>
    ///<param name="name">The event name.</param>
    ///<param name="eventType">The delegate type of the event handler.</param>
    ///<param name="componentType">The type that owns this event.</param>
    ///<param name="isMulticast">Whether the event supports multiple handlers.</param>
    ///<param name="attributes">Optional attributes to associate with the event.</param>
    ///<exception cref="ArgumentException"><paramref name="name"/> is null or whitespace.</exception>
    ///<exception cref="ArgumentNullException">
    ///<paramref name="eventType"/> or <paramref name="componentType"/> is <c>null</c>.
    ///</exception>
    public EventMetadata(string name, Type eventType, Type componentType, bool isMulticast = true, params Attribute[] attributes)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(eventType);
        ArgumentNullException.ThrowIfNull(componentType);

        Name = name;
        EventType = eventType;
        ComponentType = componentType;
        IsMulticast = isMulticast;
        Attributes = (attributes.Length > 0) ? (new AttributeCollection(attributes)) : AttributeCollection.Empty;
    }
    #endregion

    #region Public methods
    ///<summary>
    ///Returns a string representation containing the event name and handler type.
    ///</summary>
    public override string ToString() { return $"{Name} ({EventType.Name})"; }
    #endregion

    #region Public properties
    ///<summary>
    ///Gets the attributes associated with this event.
    ///</summary>
    public AttributeCollection Attributes { get; }

    ///<summary>
    ///Gets the type that owns this event.
    ///</summary>
    public Type ComponentType { get; }

    ///<summary>
    ///Gets the delegate type of the event handler.
    ///</summary>
    public Type EventType { get; }

    ///<summary>
    ///Gets a value indicating whether the event supports multiple handlers.
    ///</summary>
    public bool IsMulticast { get; }

    ///<summary>
    ///Gets the event name.
    ///</summary>
    public string Name { get; }
    #endregion
}
