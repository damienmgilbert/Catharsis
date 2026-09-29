namespace Catharsis.Contracts;

///<summary>
///A decorator found by <see cref="AttributeServiceScanner"/>.
///</summary>
///<param name="ServiceType">The service being decorated.</param>
///<param name="DecoratorType">The class that wraps the service.</param>
///<param name="Order">The application order; lower values are applied first.</param>
public sealed record DecoratorRegistration(Type ServiceType, Type DecoratorType, int Order);
