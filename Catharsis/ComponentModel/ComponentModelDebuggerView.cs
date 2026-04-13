using System.ComponentModel;
using System.Diagnostics;

namespace Catharsis.ComponentModel;

///<summary>
///A debugger proxy view for component instances that surfaces <see cref="TypeDescriptor"/> metadata, property values,
///validation state, and component lifecycle information in debugger tool windows.
///</summary>
///<remarks>
///<para> Apply <c>[DebuggerTypeProxy(typeof(ComponentModelDebuggerView))]</c> to a component class to make this view
///the default representation in Visual Studio debugger windows.</para>
///</remarks>
public sealed class ComponentModelDebuggerView
{
    #region Fields
    readonly object _component;
    #endregion

    #region Constructors
    ///<summary>
    ///Initializes a new instance of <see cref="ComponentModelDebuggerView"/> for the specified component instance.
    ///</summary>
    ///<param name="component">The component to create a debug view for.</param>
    ///<exception cref="ArgumentNullException">
    ///<paramref name="component"/> is <c>null</c>.
    ///</exception>
    public ComponentModelDebuggerView(object component)
    {
        ArgumentNullException.ThrowIfNull(component);
        _component = component;
    }
    #endregion

    #region Private methods
    string[] GetComponentModelInterfaces()
    {
        Type type = _component.GetType();
        Type[] relevant =
                          [
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
                          ];

        List<string> result = [];

        foreach(Type iface in relevant)
        {
            if(iface.IsAssignableFrom(type))
            {
                result.Add(iface.Name);
            }
        }

        return[ .. result ];
    }

    PropertyEntry[] GetPropertyEntries()
    {
        PropertyDescriptorCollection descriptors = TypeDescriptor.GetProperties(_component);
        PropertyEntry[] entries = new PropertyEntry[descriptors.Count];

        for(int i = 0; i < descriptors.Count; i++)
        {
            PropertyDescriptor prop = descriptors[i];
            object? value;

            try
            {
                value = prop.GetValue(_component);
            } catch(Exception ex)
            {
                value = $"<error: {ex.Message}>";
            }

            entries[i] = new PropertyEntry(prop.Name, prop.PropertyType.Name, value, prop.IsReadOnly, prop.Category);
        }

        return entries;
    }

    ValidationErrorEntry[] GetValidationErrors()
    {
        if((_component is not INotifyDataErrorInfo errorInfo) || !errorInfo.HasErrors)
        {
            return [];
        }

        List<ValidationErrorEntry> errors = [];
        PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(_component);

        foreach(PropertyDescriptor prop in properties)
        {
            foreach(object error in errorInfo.GetErrors(prop.Name))
            {
                string? message = error?.ToString();

                if(!string.IsNullOrWhiteSpace(message))
                {
                    errors.Add(new ValidationErrorEntry(prop.Name, message));
                }
            }
        }

        return[ .. errors ];
    }
    #endregion

    #region Public properties
    ///<summary>
    ///Gets whether the component has validation errors.
    ///</summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public bool HasErrors => (_component as INotifyDataErrorInfo)?.HasErrors ?? false;

    ///<summary>
    ///Gets the component model interfaces implemented by this component.
    ///</summary>
    public string[] Interfaces => GetComponentModelInterfaces();

    ///<summary>
    ///Gets whether the component supports change tracking and has uncommitted changes.
    ///</summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public bool IsChanged => (_component as IChangeTracking)?.IsChanged ?? false;

    ///<summary>
    ///Gets a value indicating whether the component is in design mode.
    ///</summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public bool IsDesignMode => (_component as IComponent)?.Site?.DesignMode ?? false;

    ///<summary>
    ///Gets the property values as a dictionary for display in the debugger.
    ///</summary>
    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    public PropertyEntry[] Properties => GetPropertyEntries();

    ///<summary>
    ///Gets the site name if the component is sited, or <c>null</c>.
    ///</summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public string? SiteName => (_component as IComponent)?.Site?.Name;

    ///<summary>
    ///Gets the runtime type name of the component.
    ///</summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public string TypeName => _component.GetType().FullName ?? _component.GetType().Name;

    ///<summary>
    ///Gets validation errors if the component implements <see cref="INotifyDataErrorInfo"/>.
    ///</summary>
    public ValidationErrorEntry[] ValidationErrors => GetValidationErrors();
    #endregion

    ///<summary>
    ///Represents a single property entry for debugger display.
    ///</summary>
    ///<param name="Name">The property name.</param>
    ///<param name="TypeName">The property type name.</param>
    ///<param name="Value">The current value.</param>
    ///<param name="IsReadOnly">Whether the property is read-only.</param>
    ///<param name="Category">The property category.</param>
    [DebuggerDisplay("{Name} ({TypeName}) = {Value}")]
    public sealed record PropertyEntry(string Name, string TypeName, object? Value, bool IsReadOnly, string Category);
    ///<summary>
    ///Represents a single validation error for debugger display.
    ///</summary>
    ///<param name="PropertyName">The property name with the error.</param>
    ///<param name="Message">The error message.</param>
    [DebuggerDisplay("{PropertyName}: {Message}")]
    public sealed record ValidationErrorEntry(string PropertyName, string Message);
}
