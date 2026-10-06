using System.ComponentModel.DataAnnotations;

namespace Catharsis.ComponentModel.Validation;

///<summary>
///Factory for creating <see cref="ValidationContext"/> instances with consistent service provider and member name
///configuration.
///</summary>
///<remarks>
///Initializes a new instance of <see cref="ValidationContextFactory"/>.
///</remarks>
///<param name="serviceProvider">
///An optional service provider to attach to created contexts.
///</param>
///<param name="items">
///Optional items dictionary to attach to created contexts.
///</param>
public sealed class ValidationContextFactory(IServiceProvider? serviceProvider = null, IDictionary<object, object?>? items = null)
{
    #region Fields
    private readonly IDictionary<object, object?>? _items = items;
    private readonly IServiceProvider? _serviceProvider = serviceProvider;
    #endregion

    #region Private methods
    private ValidationContext CreateCore(object instance, string? memberName)
    {
        ValidationContext context = (_serviceProvider is not null) ? (new ValidationContext(instance, _serviceProvider, _items)) : ((_items is not null) ? (new ValidationContext(instance, _items)) : (new ValidationContext(instance)));

        if(memberName is not null)
        {
            context.MemberName = memberName;
        }

        return context;
    }
    #endregion

    #region Public methods
    ///<summary>
    ///Creates a <see cref="ValidationContext"/> for the specified object.
    ///</summary>
    ///<param name="instance">The object to validate.</param>
    ///<returns>A configured <see cref="ValidationContext"/>.</returns>
    ///<exception cref="ArgumentNullException">
    public ValidationContext CreateContext(object instance)
    {
        ArgumentNullException.ThrowIfNull(instance);
        return CreateCore(instance, memberName: null);
    }

    ///<summary>
    ///Creates a <see cref="ValidationContext"/> for a specific property on the specified object.
    ///</summary>
    ///<param name="instance">The object that owns the property.</param>
    ///<param name="memberName">The property name.</param>
    ///<returns>A configured <see cref="ValidationContext"/>.</returns>
    ///<exception cref="ArgumentNullException">
    public ValidationContext CreatePropertyContext(object instance, string memberName)
    {
        ArgumentNullException.ThrowIfNull(instance);
        ArgumentNullException.ThrowIfNull(memberName);
        return CreateCore(instance, memberName);
    }

    ///<summary>
    ///Creates a <see cref="ValidationContext"/> for a specific property on the specified object with a custom display
    ///name.
    ///</summary>
    ///<param name="instance">The object that owns the property.</param>
    ///<param name="memberName">The property name.</param>
    ///<param name="displayName">The display name used in error messages.</param>
    ///<returns>A configured <see cref="ValidationContext"/>.</returns>
    ///<exception cref="ArgumentNullException">
    public ValidationContext CreatePropertyContext(object instance, string memberName, string displayName)
    {
        ArgumentNullException.ThrowIfNull(instance);
        ArgumentNullException.ThrowIfNull(memberName);
        ArgumentNullException.ThrowIfNull(displayName);

        ValidationContext context = CreateCore(instance, memberName);
        context.DisplayName = displayName;
        return context;
    }
    #endregion
}
