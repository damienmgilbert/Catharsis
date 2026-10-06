namespace Catharsis.Contracts;

///<summary>
///Marks a class as a decorator of another service, so <see cref="AttributeServiceScanner"/> can wrap the registered
///implementation with it. The decorator's constructor must accept the decorated service as a parameter.
///</summary>
///<param name="serviceType">The service being decorated.</param>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class DecoratorForAttribute(Type serviceType) : Attribute
{
    #region Public properties
    ///<summary>Gets the service being decorated.</summary>
    public Type ServiceType { get; } = serviceType ?? throw new ArgumentNullException(nameof(serviceType));

    ///<summary>
    ///Gets or sets the application order among decorators of the same service. Lower values are applied first, so
    ///they sit closest to the real implementation; higher values wrap the outside.
    ///</summary>
    public int Order { get; set; }
    #endregion
}
