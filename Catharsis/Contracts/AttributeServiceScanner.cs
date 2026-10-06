using System.Reflection;

namespace Catharsis.Contracts;

///<summary>
///Finds classes marked with <see cref="ServiceAttribute"/> and <see cref="DecoratorForAttribute"/> so they can be
///registered in a dependency injection container. Scanning only reads metadata; use
///<see cref="ContractServiceCollectionExtensions.AddAttributedServices(Microsoft.Extensions.DependencyInjection.IServiceCollection, Assembly[])"/>
///to register what it finds.
///</summary>
public static class AttributeServiceScanner
{
    #region Public methods
    ///<summary>
    ///Finds every service declared in <paramref name="assembly"/>.
    ///</summary>
    ///<param name="assembly">The assembly to scan.</param>
    ///<returns>The services, ordered by implementation name so the result is deterministic.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="assembly"/> is <c>null</c>.</exception>
    ///<exception cref="InvalidOperationException">A class declares a service type it cannot be assigned to.</exception>
    public static IReadOnlyList<ServiceRegistration> ScanServices(Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly);

        return ScanServices(GetLoadableTypes(assembly));
    }

    ///<summary>
    ///Finds every service among <paramref name="types"/>.
    ///</summary>
    ///<param name="types">The candidate types.</param>
    ///<returns>The services, ordered by implementation name so the result is deterministic.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="types"/> is <c>null</c>.</exception>
    ///<exception cref="InvalidOperationException">A class declares a service type it cannot be assigned to.</exception>
    public static IReadOnlyList<ServiceRegistration> ScanServices(IEnumerable<Type> types)
    {
        ArgumentNullException.ThrowIfNull(types);

        List<ServiceRegistration> found = [];

        foreach(Type type in types.Where(IsConcreteClass))
        {
            ServiceAttribute? attribute = type.GetCustomAttribute<ServiceAttribute>(inherit: false);

            if(attribute is null)
            {
                continue;
            }

            Type serviceType = attribute.ServiceType ?? type;

            if(!serviceType.IsAssignableFrom(type))
            {
                throw new InvalidOperationException($"{type.FullName} is declared as a {serviceType.FullName} service but does not implement it.");
            }

            found.Add(new ServiceRegistration(serviceType, type, attribute.Lifetime));
        }

        return [.. found.OrderBy(static r => r.ImplementationType.FullName, StringComparer.Ordinal)];
    }

    ///<summary>
    ///Finds every decorator declared in <paramref name="assembly"/>.
    ///</summary>
    ///<param name="assembly">The assembly to scan.</param>
    ///<returns>The decorators, ordered by <see cref="DecoratorRegistration.Order"/> and then by name.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="assembly"/> is <c>null</c>.</exception>
    ///<exception cref="InvalidOperationException">A decorator does not implement the service it decorates.</exception>
    public static IReadOnlyList<DecoratorRegistration> ScanDecorators(Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly);

        return ScanDecorators(GetLoadableTypes(assembly));
    }

    ///<summary>
    ///Finds every decorator among <paramref name="types"/>.
    ///</summary>
    ///<param name="types">The candidate types.</param>
    ///<returns>The decorators, ordered by <see cref="DecoratorRegistration.Order"/> and then by name.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="types"/> is <c>null</c>.</exception>
    ///<exception cref="InvalidOperationException">A decorator does not implement the service it decorates.</exception>
    public static IReadOnlyList<DecoratorRegistration> ScanDecorators(IEnumerable<Type> types)
    {
        ArgumentNullException.ThrowIfNull(types);

        List<DecoratorRegistration> found = [];

        foreach(Type type in types.Where(IsConcreteClass))
        {
            DecoratorForAttribute? attribute = type.GetCustomAttribute<DecoratorForAttribute>(inherit: false);

            if(attribute is null)
            {
                continue;
            }

            if(!attribute.ServiceType.IsAssignableFrom(type))
            {
                throw new InvalidOperationException($"{type.FullName} is declared as a decorator of {attribute.ServiceType.FullName} but does not implement it.");
            }

            found.Add(new DecoratorRegistration(attribute.ServiceType, type, attribute.Order));
        }

        return [.. found.OrderBy(static d => d.Order).ThenBy(static d => d.DecoratorType.FullName, StringComparer.Ordinal)];
    }
    #endregion

    #region Private methods
    static bool IsConcreteClass(Type type) => type is { IsClass: true, IsAbstract: false, IsGenericTypeDefinition: false };

    static Type[] GetLoadableTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch(ReflectionTypeLoadException ex)
        {
            return [.. ex.Types.OfType<Type>()];
        }
    }
    #endregion
}
