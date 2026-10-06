using Microsoft.Extensions.DependencyInjection;

namespace Catharsis.Contracts;

///<summary>
///Marks a class for automatic registration in a dependency injection container by
///<see cref="AttributeServiceScanner"/>. When <see cref="ServiceType"/> is not set the class is registered as itself.
///</summary>
///<param name="lifetime">How long each resolved instance lives.</param>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class ServiceAttribute(ServiceLifetime lifetime = ServiceLifetime.Transient) : Attribute
{
    #region Public properties
    ///<summary>Gets the lifetime the service is registered with.</summary>
    public ServiceLifetime Lifetime { get; } = lifetime;

    ///<summary>
    ///Gets or sets the service type to register the class under, typically an interface it implements. When
    ///<c>null</c> the class itself is the service type.
    ///</summary>
    public Type? ServiceType { get; set; }
    #endregion
}
