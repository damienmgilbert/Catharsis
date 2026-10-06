using System.ComponentModel;

namespace Catharsis.ComponentModel;

///<summary>
///An <see cref="EventDescriptor"/> implementation that uses delegates for add and remove handler operations, supporting
///dynamic event definitions without requiring compile-time event accessors.
///</summary>
///<remarks>
public sealed class DynamicEventDescriptor : EventDescriptor
{
    #region Fields
    private readonly Action<object, Delegate> _addHandler;
    private readonly Type _componentType;
    private readonly Type _eventType;
    private readonly Action<object, Delegate> _removeHandler;
    #endregion

    #region Constructors
    ///<summary>
    ///Initializes a new instance of <see cref="DynamicEventDescriptor"/> from <see cref="EventMetadata"/> and delegate
    ///handlers.
    ///</summary>
    ///<param name="metadata">The event metadata.</param>
    ///<param name="addHandler">A delegate that subscribes a handler.</param>
    ///<param name="removeHandler">A delegate that unsubscribes a handler.</param>
    ///<exception cref="ArgumentNullException">
    public DynamicEventDescriptor(EventMetadata metadata, Action<object, Delegate> addHandler, Action<object, Delegate> removeHandler) : base(metadata?.Name ?? throw new ArgumentNullException(nameof(metadata)), ToAttributeArray(metadata.Attributes))
    {
        ArgumentNullException.ThrowIfNull(addHandler);
        ArgumentNullException.ThrowIfNull(removeHandler);

        _eventType = metadata.EventType;
        _componentType = metadata.ComponentType;
        _addHandler = addHandler;
        _removeHandler = removeHandler;
    }

    ///<summary>
    ///Initializes a new instance of <see cref="DynamicEventDescriptor"/>.
    ///</summary>
    ///<param name="name">The event name.</param>
    ///<param name="eventType">The delegate type of the event handler.</param>
    ///<param name="componentType">The type that owns this event.</param>
    ///<param name="addHandler">A delegate that subscribes a handler to the event.</param>
    ///<param name="removeHandler">A delegate that unsubscribes a handler from the event.</param>
    ///<param name="attributes">Optional attributes for the event.</param>
    ///<exception cref="ArgumentNullException">
    public DynamicEventDescriptor(string name, Type eventType, Type componentType, Action<object, Delegate> addHandler, Action<object, Delegate> removeHandler, params Attribute[] attributes) : base(name, attributes)
    {
        ArgumentNullException.ThrowIfNull(eventType);
        ArgumentNullException.ThrowIfNull(componentType);
        ArgumentNullException.ThrowIfNull(addHandler);
        ArgumentNullException.ThrowIfNull(removeHandler);

        _eventType = eventType;
        _componentType = componentType;
        _addHandler = addHandler;
        _removeHandler = removeHandler;
    }
    #endregion

    #region Private methods
    private static Attribute[] ToAttributeArray(AttributeCollection collection)
    {
        Attribute[] result = new Attribute[collection.Count];

        for(int i = 0; i < collection.Count; i++)
        {
            result[i] = collection[i];
        }

        return result;
    }
    #endregion

    #region Public methods
    ///<inheritdoc/>
    public override void AddEventHandler(object component, Delegate value)
    {
        ArgumentNullException.ThrowIfNull(component);
        ArgumentNullException.ThrowIfNull(value);

        _addHandler(component, value);
    }

    ///<inheritdoc/>
    public override void RemoveEventHandler(object component, Delegate value)
    {
        ArgumentNullException.ThrowIfNull(component);
        ArgumentNullException.ThrowIfNull(value);

        _removeHandler(component, value);
    }

    ///<summary>
    ///Creates a new <see cref="DynamicEventDescriptor"/> with the specified attributes merged onto the existing
    ///attribute set.
    ///</summary>
    ///<param name="additionalAttributes">The attributes to merge.</param>
    ///<returns>A new descriptor with the merged attributes.</returns>
    public DynamicEventDescriptor WithMergedAttributes(params Attribute[] additionalAttributes)
    {
        AttributeCollectionBuilder builder = new AttributeCollectionBuilder(Attributes)
            .Merge(additionalAttributes);

        return new DynamicEventDescriptor(Name, _eventType, _componentType, _addHandler, _removeHandler, ToAttributeArray(builder.Build()));
    }
    #endregion

    #region Public properties
    ///<inheritdoc/>
    public override Type ComponentType => _componentType;

    ///<inheritdoc/>
    public override Type EventType => _eventType;

    ///<inheritdoc/>
    public override bool IsMulticast => _eventType.IsSubclassOf(typeof(MulticastDelegate));
    #endregion
}
