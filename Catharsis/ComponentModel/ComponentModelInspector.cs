using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Catharsis.ComponentModel;

///<summary>
///Provides runtime introspection of component instances using <see cref="TypeDescriptor"/> and DataAnnotations
///metadata, surfacing properties, events, attributes, and validation state.
///</summary>
///<remarks>
///<para> Use <see cref="GetPropertyReport"/> for a full property-level report including type converter info, validation
///attributes, and current values. Use <see cref="GetEventReport"/> for event-level details.</para>
///</remarks>
public sealed class ComponentModelInspector
{
    #region Public methods
    ///<summary>
    ///Returns a summary of interfaces implemented by the component that are relevant to the ComponentModel
    ///infrastructure.
    ///</summary>
    ///<param name="component">The component to inspect.</param>
    ///<returns>A list of interface names.</returns>
    ///<exception cref="ArgumentNullException">
    ///<paramref name="component"/> is <c>null</c>.
    ///</exception>
    public IReadOnlyList<string> GetComponentModelInterfaces(object component)
    {
        ArgumentNullException.ThrowIfNull(component);

        Type componentType = component.GetType();
        Type[] relevant = new[]
                          {
                          typeof(IComponent),
                          typeof(INotifyPropertyChanged),
                          typeof(INotifyPropertyChanging),
                          typeof(INotifyDataErrorInfo),
                          typeof(IEditableObject),
                          typeof(IChangeTracking),
                          typeof(IRevertibleChangeTracking),
                          typeof(ICustomTypeDescriptor),
                          typeof(IDataErrorInfo),
                          typeof(ISupportInitialize),
                          typeof(IServiceProvider),
                          typeof(IDisposable)
                          };

        List<string> result = new List<string>();

        foreach(Type iface in relevant)
        {
            if(iface.IsAssignableFrom(componentType))
            {
                result.Add(iface.Name);
            }
        }

        return result;
    }

    ///<summary>
    ///Inspects the specified component and returns a report of all events.
    ///</summary>
    ///<param name="component">The component to inspect.</param>
    ///<returns>A list of <see cref="EventReport"/> entries.</returns>
    ///<exception cref="ArgumentNullException">
    ///<paramref name="component"/> is <c>null</c>.
    ///</exception>
    public IReadOnlyList<EventReport> GetEventReport(object component)
    {
        ArgumentNullException.ThrowIfNull(component);

        EventDescriptorCollection events = TypeDescriptor.GetEvents(component);
        List<EventReport> reports = new List<EventReport>(events.Count);

        foreach(EventDescriptor evt in events)
        {
            List<Attribute> attributes = new List<Attribute>();

            foreach(Attribute attr in evt.Attributes)
            {
                attributes.Add(attr);
            }

            reports.Add(new EventReport { Name = evt.Name, EventType = evt.EventType, ComponentType = evt.ComponentType, IsBrowsable = evt.IsBrowsable, Category = evt.Category, Description = evt.Description, DisplayName = evt.DisplayName, IsMulticast = evt.IsMulticast, AllAttributes = attributes });
        }

        return reports;
    }

    ///<summary>
    ///Inspects the specified component and returns a report of all properties with their metadata, current values, and
    ///associated attributes.
    ///</summary>
    ///<param name="component">The component to inspect.</param>
    ///<returns>A list of <see cref="PropertyReport"/> entries.</returns>
    ///<exception cref="ArgumentNullException">
    ///<paramref name="component"/> is <c>null</c>.
    ///</exception>
    public IReadOnlyList<PropertyReport> GetPropertyReport(object component)
    {
        ArgumentNullException.ThrowIfNull(component);

        PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(component);
        List<PropertyReport> reports = new List<PropertyReport>(properties.Count);

        foreach(PropertyDescriptor property in properties)
        {
            object? value = property.GetValue(component);
            System.ComponentModel.TypeConverter converter = property.Converter;
            List<Attribute> attributes = new List<Attribute>();

            foreach(Attribute attr in property.Attributes)
            {
                attributes.Add(attr);
            }

            List<ValidationAttribute> validationAttributes = attributes.OfType<ValidationAttribute>().ToList();

            reports.Add(
            new PropertyReport
            {
                Name = property.Name,
                PropertyType = property.PropertyType,
                ComponentType = property.ComponentType,
                IsReadOnly = property.IsReadOnly,
                IsBrowsable = property.IsBrowsable,
                Category = property.Category,
                Description = property.Description,
                DisplayName = property.DisplayName,
                CurrentValue = value,
                ConverterTypeName = converter.GetType().Name,
                CanConvertToString = converter.CanConvertTo(typeof(string)),
                HasDefaultValue = property.CanResetValue(component),
                ValidationAttributes = validationAttributes,
                AllAttributes = attributes
            });
        }

        return reports;
    }

    ///<summary>
    ///Checks whether the specified component has any validation errors (if it implements <see
    ///cref="INotifyDataErrorInfo"/>).
    ///</summary>
    ///<param name="component">The component to check.</param>
    ///<param name="errors">When this method returns <c>true</c>, contains the error messages.</param>
    ///<returns><c>true</c> if the component has errors; otherwise, <c>false</c>.</returns>
    public bool TryGetValidationErrors(object component, [NotNullWhen(true)] out IReadOnlyDictionary<string, IReadOnlyList<string>>? errors)
    {
        ArgumentNullException.ThrowIfNull(component);

        if((component is INotifyDataErrorInfo errorInfo) && errorInfo.HasErrors)
        {
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(component);
            Dictionary<string, IReadOnlyList<string>> dict = new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal);

            foreach(PropertyDescriptor prop in properties)
            {
                List<string> propErrors = errorInfo.GetErrors(prop.Name).Cast<object>().Select(e => e.ToString() ?? string.Empty).Where(e => !string.IsNullOrWhiteSpace(e)).ToList();

                if(propErrors.Count > 0)
                {
                    dict[prop.Name] = propErrors;
                }
            }

            if(dict.Count > 0)
            {
                errors = dict;
                return true;
            }
        }

        errors = null;
        return false;
    }
    #endregion

    ///<summary>
    ///Describes a single property of an inspected component.
    ///</summary>
    public sealed class PropertyReport
    {
        #region Public methods
        ///<inheritdoc/>
        public override string ToString() { return $"{Name} ({PropertyType.Name}){(IsReadOnly ? " [ReadOnly]" : string.Empty)} = {CurrentValue}"; }
        #endregion

        #region Public properties
        ///<summary>
        ///Gets all attributes on the property.
        ///</summary>
        public required IReadOnlyList<Attribute> AllAttributes { get; init; }

        ///<summary>
        ///Gets whether the converter can convert to string.
        ///</summary>
        public required bool CanConvertToString { get; init; }

        ///<summary>
        ///Gets the property category.
        ///</summary>
        public required string Category { get; init; }

        ///<summary>
        ///Gets the component type that owns this property.
        ///</summary>
        public required Type ComponentType { get; init; }

        ///<summary>
        ///Gets the type converter name.
        ///</summary>
        public required string ConverterTypeName { get; init; }

        ///<summary>
        ///Gets the current value.
        ///</summary>
        public object? CurrentValue { get; init; }

        ///<summary>
        ///Gets the property description.
        ///</summary>
        public required string Description { get; init; }

        ///<summary>
        ///Gets the display name.
        ///</summary>
        public required string DisplayName { get; init; }

        ///<summary>
        ///Gets whether the property has a non-default value.
        ///</summary>
        public required bool HasDefaultValue { get; init; }

        ///<summary>
        ///Gets whether the property is browsable.
        ///</summary>
        public required bool IsBrowsable { get; init; }

        ///<summary>
        ///Gets whether the property is read-only.
        ///</summary>
        public required bool IsReadOnly { get; init; }

        ///<summary>
        ///Gets the property name.
        ///</summary>
        public required string Name { get; init; }

        ///<summary>
        ///Gets the CLR type of the property.
        ///</summary>
        public required Type PropertyType { get; init; }

        ///<summary>
        ///Gets the DataAnnotations validation attributes.
        ///</summary>
        public required IReadOnlyList<ValidationAttribute> ValidationAttributes { get; init; }
        #endregion
    }

    ///<summary>
    ///Describes a single event of an inspected component.
    ///</summary>
    public sealed class EventReport
    {
        #region Public methods
        ///<inheritdoc/>
        public override string ToString() { return $"{Name} ({EventType.Name}){(IsMulticast ? " [Multicast]" : string.Empty)}"; }
        #endregion

        #region Public properties
        ///<summary>
        ///Gets all attributes on the event.
        ///</summary>
        public required IReadOnlyList<Attribute> AllAttributes { get; init; }

        ///<summary>
        ///Gets the event category.
        ///</summary>
        public required string Category { get; init; }

        ///<summary>
        ///Gets the component type that owns this event.
        ///</summary>
        public required Type ComponentType { get; init; }

        ///<summary>
        ///Gets the event description.
        ///</summary>
        public required string Description { get; init; }

        ///<summary>
        ///Gets the display name.
        ///</summary>
        public required string DisplayName { get; init; }

        ///<summary>
        ///Gets the event delegate type.
        ///</summary>
        public required Type EventType { get; init; }

        ///<summary>
        ///Gets whether the event is browsable.
        ///</summary>
        public required bool IsBrowsable { get; init; }

        ///<summary>
        ///Gets whether the event is multicast.
        ///</summary>
        public required bool IsMulticast { get; init; }

        ///<summary>
        ///Gets the event name.
        ///</summary>
        public required string Name { get; init; }
        #endregion
    }
}
