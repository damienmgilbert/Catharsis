using Microsoft.Extensions.DependencyInjection;

namespace Catharsis.Contracts;

///<summary>
///A service found by <see cref="AttributeServiceScanner"/>.
///</summary>
///<param name="ServiceType">The type consumers resolve.</param>
///<param name="ImplementationType">The concrete class that is created.</param>
///<param name="Lifetime">The lifetime declared on the class.</param>
public sealed record ServiceRegistration(Type ServiceType, Type ImplementationType, ServiceLifetime Lifetime);
